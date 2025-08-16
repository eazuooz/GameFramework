//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace GameFramework.FileSystem
{
    internal sealed partial class FileSystem : IFileSystem
    {
        private const int ClusterSize = 1024 * 4;
        private const int CachedBytesLength = 0x1000;

        private static readonly string[] EmptyStringArray = new string[] { };
        private static readonly byte[] CachedBytes = new byte[CachedBytesLength];

        private static readonly int HeaderDataSize = Marshal.SizeOf(typeof(HeaderData));
        private static readonly int BlockDataSize = Marshal.SizeOf(typeof(BlockData));
        private static readonly int StringDataSize = Marshal.SizeOf(typeof(StringData));

        private readonly string mFullPath;
        private readonly FileSystemAccess mAccess;
        private readonly FileSystemStream mStream;
        private readonly Dictionary<string, int> mFileDatas;
        private readonly List<BlockData> mBlockDatas;
        private readonly GameFrameworkMultiDictionary<int, int> mFreeBlockIndexes;
        private readonly SortedDictionary<int, StringData> mStringDatas;
        private readonly Queue<int> mFreeStringIndexes;
        private readonly Queue<StringData> mFreeStringDatas;

        private HeaderData mHeaderData;
        private int mBlockDataOffset;
        private int mStringDataOffset;
        private int mFileDataOffset;

        private FileSystem(string fullPath, FileSystemAccess access, FileSystemStream stream)
        {
            if (string.IsNullOrEmpty(fullPath))
            {
                throw new GameFrameworkException("Full path is invalid.");
            }

            if (access == FileSystemAccess.Unspecified)
            {
                throw new GameFrameworkException("Access is invalid.");
            }

            if (stream == null)
            {
                throw new GameFrameworkException("Stream is invalid.");
            }

            mFullPath = fullPath;
            mAccess = access;
            mStream = stream;
            mFileDatas = new Dictionary<string, int>(StringComparer.Ordinal);
            mBlockDatas = new List<BlockData>();
            mFreeBlockIndexes = new GameFrameworkMultiDictionary<int, int>();
            mStringDatas = new SortedDictionary<int, StringData>();
            mFreeStringIndexes = new Queue<int>();
            mFreeStringDatas = new Queue<StringData>();

            mHeaderData = default(HeaderData);
            mBlockDataOffset = 0;
            mStringDataOffset = 0;
            mFileDataOffset = 0;

            Utility.Marshal.EnsureCachedHGlobalSize(CachedBytesLength);
        }

        public string FullPath
        {
            get
            {
                return mFullPath;
            }
        }

        public FileSystemAccess Access
        {
            get
            {
                return mAccess;
            }
        }

        public int FileCount
        {
            get
            {
                return mFileDatas.Count;
            }
        }

        public int MaxFileCount
        {
            get
            {
                return mHeaderData.MaxFileCount;
            }
        }

        public static FileSystem Create(string fullPath, FileSystemAccess access, FileSystemStream stream, int maxFileCount, int maxBlockCount)
        {
            if (maxFileCount <= 0)
            {
                throw new GameFrameworkException("Max file count is invalid.");
            }

            if (maxBlockCount <= 0)
            {
                throw new GameFrameworkException("Max block count is invalid.");
            }

            if (maxFileCount > maxBlockCount)
            {
                throw new GameFrameworkException("Max file count can not larger than max block count.");
            }

            FileSystem fileSystem = new FileSystem(fullPath, access, stream);
            fileSystem.mHeaderData = new HeaderData(maxFileCount, maxBlockCount);
            CalcOffsets(fileSystem);
            Utility.Marshal.StructureToBytes(fileSystem.mHeaderData, HeaderDataSize, CachedBytes);

            try
            {
                stream.Write(CachedBytes, 0, HeaderDataSize);
                stream.SetLength(fileSystem.mFileDataOffset);
                return fileSystem;
            }
            catch
            {
                fileSystem.Shutdown();
                return null;
            }
        }

        public static FileSystem Load(string fullPath, FileSystemAccess access, FileSystemStream stream)
        {
            FileSystem fileSystem = new FileSystem(fullPath, access, stream);

            stream.Read(CachedBytes, 0, HeaderDataSize);
            fileSystem.mHeaderData = Utility.Marshal.BytesToStructure<HeaderData>(HeaderDataSize, CachedBytes);
            if (!fileSystem.mHeaderData.IsValid)
            {
                return null;
            }

            CalcOffsets(fileSystem);

            if (fileSystem.mBlockDatas.Capacity < fileSystem.mHeaderData.BlockCount)
            {
                fileSystem.mBlockDatas.Capacity = fileSystem.mHeaderData.BlockCount;
            }

            for (int i = 0; i < fileSystem.mHeaderData.BlockCount; i++)
            {
                stream.Read(CachedBytes, 0, BlockDataSize);
                BlockData blockData = Utility.Marshal.BytesToStructure<BlockData>(BlockDataSize, CachedBytes);
                fileSystem.mBlockDatas.Add(blockData);
            }

            for (int i = 0; i < fileSystem.mBlockDatas.Count; i++)
            {
                BlockData blockData = fileSystem.mBlockDatas[i];
                if (blockData.Using)
                {
                    StringData stringData = fileSystem.ReadStringData(blockData.StringIndex);
                    fileSystem.mStringDatas.Add(blockData.StringIndex, stringData);
                    fileSystem.mFileDatas.Add(stringData.GetString(fileSystem.mHeaderData.GetEncryptBytes()), i);
                }
                else
                {
                    fileSystem.mFreeBlockIndexes.Add(blockData.Length, i);
                }
            }

            int index = 0;
            foreach (KeyValuePair<int, StringData> i in fileSystem.mStringDatas)
            {
                while (index < i.Key)
                {
                    fileSystem.mFreeStringIndexes.Enqueue(index++);
                }

                index++;
            }

            return fileSystem;
        }

        public void Shutdown()
        {
            mStream.Close();

            mFileDatas.Clear();
            mBlockDatas.Clear();
            mFreeBlockIndexes.Clear();
            mStringDatas.Clear();
            mFreeStringIndexes.Clear();
            mFreeStringDatas.Clear();

            mBlockDataOffset = 0;
            mStringDataOffset = 0;
            mFileDataOffset = 0;
        }

        public FileInfo GetFileInfo(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new GameFrameworkException("Name is invalid.");
            }

            int blockIndex = 0;
            if (!mFileDatas.TryGetValue(name, out blockIndex))
            {
                return default(FileInfo);
            }

            BlockData blockData = mBlockDatas[blockIndex];
            return new FileInfo(name, GetClusterOffset(blockData.ClusterIndex), blockData.Length);
        }

        public FileInfo[] GetAllFileInfos()
        {
            int index = 0;
            FileInfo[] results = new FileInfo[mFileDatas.Count];
            foreach (KeyValuePair<string, int> fileData in mFileDatas)
            {
                BlockData blockData = mBlockDatas[fileData.Value];
                results[index++] = new FileInfo(fileData.Key, GetClusterOffset(blockData.ClusterIndex), blockData.Length);
            }

            return results;
        }

        public void GetAllFileInfos(List<FileInfo> results)
        {
            if (results == null)
            {
                throw new GameFrameworkException("Results is invalid.");
            }

            results.Clear();
            foreach (KeyValuePair<string, int> fileData in mFileDatas)
            {
                BlockData blockData = mBlockDatas[fileData.Value];
                results.Add(new FileInfo(fileData.Key, GetClusterOffset(blockData.ClusterIndex), blockData.Length));
            }
        }

        public bool HasFile(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new GameFrameworkException("Name is invalid.");
            }

            return mFileDatas.ContainsKey(name);
        }

        public byte[] ReadFile(string name)
        {
            if (mAccess != FileSystemAccess.Read && mAccess != FileSystemAccess.ReadWrite)
            {
                throw new GameFrameworkException("File system is not readable.");
            }

            if (string.IsNullOrEmpty(name))
            {
                throw new GameFrameworkException("Name is invalid.");
            }

            FileInfo fileInfo = GetFileInfo(name);
            if (!fileInfo.IsValid)
            {
                return null;
            }

            int length = fileInfo.Length;
            byte[] buffer = new byte[length];
            if (length > 0)
            {
                mStream.Position = fileInfo.Offset;
                mStream.Read(buffer, 0, length);
            }

            return buffer;
        }

        public int ReadFile(string name, byte[] buffer)
        {
            if (buffer == null)
            {
                throw new GameFrameworkException("Buffer is invalid.");
            }

            return ReadFile(name, buffer, 0, buffer.Length);
        }

        public int ReadFile(string name, byte[] buffer, int startIndex)
        {
            if (buffer == null)
            {
                throw new GameFrameworkException("Buffer is invalid.");
            }

            return ReadFile(name, buffer, startIndex, buffer.Length - startIndex);
        }

        public int ReadFile(string name, byte[] buffer, int startIndex, int length)
        {
            if (mAccess != FileSystemAccess.Read && mAccess != FileSystemAccess.ReadWrite)
            {
                throw new GameFrameworkException("File system is not readable.");
            }

            if (string.IsNullOrEmpty(name))
            {
                throw new GameFrameworkException("Name is invalid.");
            }

            if (buffer == null)
            {
                throw new GameFrameworkException("Buffer is invalid.");
            }

            if (startIndex < 0 || length < 0 || startIndex + length > buffer.Length)
            {
                throw new GameFrameworkException("Start index or length is invalid.");
            }

            FileInfo fileInfo = GetFileInfo(name);
            if (!fileInfo.IsValid)
            {
                return 0;
            }

            mStream.Position = fileInfo.Offset;
            if (length > fileInfo.Length)
            {
                length = fileInfo.Length;
            }

            if (length > 0)
            {
                return mStream.Read(buffer, startIndex, length);
            }

            return 0;
        }

        public int ReadFile(string name, Stream stream)
        {
            if (mAccess != FileSystemAccess.Read && mAccess != FileSystemAccess.ReadWrite)
            {
                throw new GameFrameworkException("File system is not readable.");
            }

            if (string.IsNullOrEmpty(name))
            {
                throw new GameFrameworkException("Name is invalid.");
            }

            if (stream == null)
            {
                throw new GameFrameworkException("Stream is invalid.");
            }

            if (!stream.CanWrite)
            {
                throw new GameFrameworkException("Stream is not writable.");
            }

            FileInfo fileInfo = GetFileInfo(name);
            if (!fileInfo.IsValid)
            {
                return 0;
            }

            int length = fileInfo.Length;
            if (length > 0)
            {
                mStream.Position = fileInfo.Offset;
                return mStream.Read(stream, length);
            }

            return 0;
        }

        public byte[] ReadFileSegment(string name, int length)
        {
            return ReadFileSegment(name, 0, length);
        }

        public byte[] ReadFileSegment(string name, int offset, int length)
        {
            if (mAccess != FileSystemAccess.Read && mAccess != FileSystemAccess.ReadWrite)
            {
                throw new GameFrameworkException("File system is not readable.");
            }

            if (string.IsNullOrEmpty(name))
            {
                throw new GameFrameworkException("Name is invalid.");
            }

            if (offset < 0)
            {
                throw new GameFrameworkException("Index is invalid.");
            }

            if (length < 0)
            {
                throw new GameFrameworkException("Length is invalid.");
            }

            FileInfo fileInfo = GetFileInfo(name);
            if (!fileInfo.IsValid)
            {
                return null;
            }

            if (offset > fileInfo.Length)
            {
                offset = fileInfo.Length;
            }

            int leftLength = fileInfo.Length - offset;
            if (length > leftLength)
            {
                length = leftLength;
            }

            byte[] buffer = new byte[length];
            if (length > 0)
            {
                mStream.Position = fileInfo.Offset + offset;
                mStream.Read(buffer, 0, length);
            }

            return buffer;
        }

        public int ReadFileSegment(string name, byte[] buffer)
        {
            if (buffer == null)
            {
                throw new GameFrameworkException("Buffer is invalid.");
            }

            return ReadFileSegment(name, 0, buffer, 0, buffer.Length);
        }

        public int ReadFileSegment(string name, byte[] buffer, int length)
        {
            return ReadFileSegment(name, 0, buffer, 0, length);
        }

        public int ReadFileSegment(string name, byte[] buffer, int startIndex, int length)
        {
            return ReadFileSegment(name, 0, buffer, startIndex, length);
        }

        public int ReadFileSegment(string name, int offset, byte[] buffer)
        {
            if (buffer == null)
            {
                throw new GameFrameworkException("Buffer is invalid.");
            }

            return ReadFileSegment(name, offset, buffer, 0, buffer.Length);
        }

        public int ReadFileSegment(string name, int offset, byte[] buffer, int length)
        {
            return ReadFileSegment(name, offset, buffer, 0, length);
        }

        public int ReadFileSegment(string name, int offset, byte[] buffer, int startIndex, int length)
        {
            if (mAccess != FileSystemAccess.Read && mAccess != FileSystemAccess.ReadWrite)
            {
                throw new GameFrameworkException("File system is not readable.");
            }

            if (string.IsNullOrEmpty(name))
            {
                throw new GameFrameworkException("Name is invalid.");
            }

            if (offset < 0)
            {
                throw new GameFrameworkException("Index is invalid.");
            }

            if (buffer == null)
            {
                throw new GameFrameworkException("Buffer is invalid.");
            }

            if (startIndex < 0 || length < 0 || startIndex + length > buffer.Length)
            {
                throw new GameFrameworkException("Start index or length is invalid.");
            }

            FileInfo fileInfo = GetFileInfo(name);
            if (!fileInfo.IsValid)
            {
                return 0;
            }

            if (offset > fileInfo.Length)
            {
                offset = fileInfo.Length;
            }

            int leftLength = fileInfo.Length - offset;
            if (length > leftLength)
            {
                length = leftLength;
            }

            if (length > 0)
            {
                mStream.Position = fileInfo.Offset + offset;
                return mStream.Read(buffer, startIndex, length);
            }

            return 0;
        }

        public int ReadFileSegment(string name, Stream stream, int length)
        {
            return ReadFileSegment(name, 0, stream, length);
        }

        public int ReadFileSegment(string name, int offset, Stream stream, int length)
        {
            if (mAccess != FileSystemAccess.Read && mAccess != FileSystemAccess.ReadWrite)
            {
                throw new GameFrameworkException("File system is not readable.");
            }

            if (string.IsNullOrEmpty(name))
            {
                throw new GameFrameworkException("Name is invalid.");
            }

            if (offset < 0)
            {
                throw new GameFrameworkException("Index is invalid.");
            }

            if (stream == null)
            {
                throw new GameFrameworkException("Stream is invalid.");
            }

            if (!stream.CanWrite)
            {
                throw new GameFrameworkException("Stream is not writable.");
            }

            if (length < 0)
            {
                throw new GameFrameworkException("Length is invalid.");
            }

            FileInfo fileInfo = GetFileInfo(name);
            if (!fileInfo.IsValid)
            {
                return 0;
            }

            if (offset > fileInfo.Length)
            {
                offset = fileInfo.Length;
            }

            int leftLength = fileInfo.Length - offset;
            if (length > leftLength)
            {
                length = leftLength;
            }

            if (length > 0)
            {
                mStream.Position = fileInfo.Offset + offset;
                return mStream.Read(stream, length);
            }

            return 0;
        }

        public bool WriteFile(string name, byte[] buffer)
        {
            if (buffer == null)
            {
                throw new GameFrameworkException("Buffer is invalid.");
            }

            return WriteFile(name, buffer, 0, buffer.Length);
        }

        public bool WriteFile(string name, byte[] buffer, int startIndex)
        {
            if (buffer == null)
            {
                throw new GameFrameworkException("Buffer is invalid.");
            }

            return WriteFile(name, buffer, startIndex, buffer.Length - startIndex);
        }

        public bool WriteFile(string name, byte[] buffer, int startIndex, int length)
        {
            if (mAccess != FileSystemAccess.Write && mAccess != FileSystemAccess.ReadWrite)
            {
                throw new GameFrameworkException("File system is not writable.");
            }

            if (string.IsNullOrEmpty(name))
            {
                throw new GameFrameworkException("Name is invalid.");
            }

            if (name.Length > byte.MaxValue)
            {
                throw new GameFrameworkException(Utility.Text.Format("Name '{0}' is too long.", name));
            }

            if (buffer == null)
            {
                throw new GameFrameworkException("Buffer is invalid.");
            }

            if (startIndex < 0 || length < 0 || startIndex + length > buffer.Length)
            {
                throw new GameFrameworkException("Start index or length is invalid.");
            }

            bool hasFile = false;
            int oldBlockIndex = -1;
            if (mFileDatas.TryGetValue(name, out oldBlockIndex))
            {
                hasFile = true;
            }

            if (!hasFile && mFileDatas.Count >= mHeaderData.MaxFileCount)
            {
                return false;
            }

            int blockIndex = AllocBlock(length);
            if (blockIndex < 0)
            {
                return false;
            }

            if (length > 0)
            {
                mStream.Position = GetClusterOffset(mBlockDatas[blockIndex].ClusterIndex);
                mStream.Write(buffer, startIndex, length);
            }

            ProcessWriteFile(name, hasFile, oldBlockIndex, blockIndex, length);
            mStream.Flush();
            return true;
        }

        public bool WriteFile(string name, Stream stream)
        {
            if (mAccess != FileSystemAccess.Write && mAccess != FileSystemAccess.ReadWrite)
            {
                throw new GameFrameworkException("File system is not writable.");
            }

            if (string.IsNullOrEmpty(name))
            {
                throw new GameFrameworkException("Name is invalid.");
            }

            if (name.Length > byte.MaxValue)
            {
                throw new GameFrameworkException(Utility.Text.Format("Name '{0}' is too long.", name));
            }

            if (stream == null)
            {
                throw new GameFrameworkException("Stream is invalid.");
            }

            if (!stream.CanRead)
            {
                throw new GameFrameworkException("Stream is not readable.");
            }

            bool hasFile = false;
            int oldBlockIndex = -1;
            if (mFileDatas.TryGetValue(name, out oldBlockIndex))
            {
                hasFile = true;
            }

            if (!hasFile && mFileDatas.Count >= mHeaderData.MaxFileCount)
            {
                return false;
            }

            int length = (int)(stream.Length - stream.Position);
            int blockIndex = AllocBlock(length);
            if (blockIndex < 0)
            {
                return false;
            }

            if (length > 0)
            {
                mStream.Position = GetClusterOffset(mBlockDatas[blockIndex].ClusterIndex);
                mStream.Write(stream, length);
            }

            ProcessWriteFile(name, hasFile, oldBlockIndex, blockIndex, length);
            mStream.Flush();
            return true;
        }

        public bool WriteFile(string name, string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                throw new GameFrameworkException("File path is invalid");
            }

            if (!File.Exists(filePath))
            {
                return false;
            }

            using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return WriteFile(name, fileStream);
            }
        }

        public bool SaveAsFile(string name, string filePath)
        {
            if (mAccess != FileSystemAccess.Read && mAccess != FileSystemAccess.ReadWrite)
            {
                throw new GameFrameworkException("File system is not readable.");
            }

            if (string.IsNullOrEmpty(name))
            {
                throw new GameFrameworkException("Name is invalid.");
            }

            if (string.IsNullOrEmpty(filePath))
            {
                throw new GameFrameworkException("File path is invalid");
            }

            FileInfo fileInfo = GetFileInfo(name);
            if (!fileInfo.IsValid)
            {
                return false;
            }

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            string directory = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using (FileStream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                int length = fileInfo.Length;
                if (length > 0)
                {
                    mStream.Position = fileInfo.Offset;
                    return mStream.Read(fileStream, length) == length;
                }

                return true;
            }
        }

        public bool RenameFile(string oldName, string newName)
        {
            if (mAccess != FileSystemAccess.Write && mAccess != FileSystemAccess.ReadWrite)
            {
                throw new GameFrameworkException("File system is not writable.");
            }

            if (string.IsNullOrEmpty(oldName))
            {
                throw new GameFrameworkException("Old name is invalid.");
            }

            if (string.IsNullOrEmpty(newName))
            {
                throw new GameFrameworkException("New name is invalid.");
            }

            if (newName.Length > byte.MaxValue)
            {
                throw new GameFrameworkException(Utility.Text.Format("New name '{0}' is too long.", newName));
            }

            if (oldName == newName)
            {
                return true;
            }

            if (mFileDatas.ContainsKey(newName))
            {
                return false;
            }

            int blockIndex = 0;
            if (!mFileDatas.TryGetValue(oldName, out blockIndex))
            {
                return false;
            }

            int stringIndex = mBlockDatas[blockIndex].StringIndex;
            StringData stringData = mStringDatas[stringIndex].SetString(newName, mHeaderData.GetEncryptBytes());
            mStringDatas[stringIndex] = stringData;
            WriteStringData(stringIndex, stringData);
            mFileDatas.Add(newName, blockIndex);
            mFileDatas.Remove(oldName);
            mStream.Flush();
            return true;
        }

        public bool DeleteFile(string name)
        {
            if (mAccess != FileSystemAccess.Write && mAccess != FileSystemAccess.ReadWrite)
            {
                throw new GameFrameworkException("File system is not writable.");
            }

            if (string.IsNullOrEmpty(name))
            {
                throw new GameFrameworkException("Name is invalid.");
            }

            int blockIndex = 0;
            if (!mFileDatas.TryGetValue(name, out blockIndex))
            {
                return false;
            }

            mFileDatas.Remove(name);

            BlockData blockData = mBlockDatas[blockIndex];
            int stringIndex = blockData.StringIndex;
            StringData stringData = mStringDatas[stringIndex].Clear();
            mFreeStringIndexes.Enqueue(stringIndex);
            mFreeStringDatas.Enqueue(stringData);
            mStringDatas.Remove(stringIndex);
            WriteStringData(stringIndex, stringData);

            blockData = blockData.Free();
            mBlockDatas[blockIndex] = blockData;
            if (!TryCombineFreeBlocks(blockIndex))
            {
                mFreeBlockIndexes.Add(blockData.Length, blockIndex);
                WriteBlockData(blockIndex);
            }

            mStream.Flush();
            return true;
        }

        private void ProcessWriteFile(string name, bool hasFile, int oldBlockIndex, int blockIndex, int length)
        {
            BlockData blockData = mBlockDatas[blockIndex];
            if (hasFile)
            {
                BlockData oldBlockData = mBlockDatas[oldBlockIndex];
                blockData = new BlockData(oldBlockData.StringIndex, blockData.ClusterIndex, length);
                mBlockDatas[blockIndex] = blockData;
                WriteBlockData(blockIndex);

                oldBlockData = oldBlockData.Free();
                mBlockDatas[oldBlockIndex] = oldBlockData;
                if (!TryCombineFreeBlocks(oldBlockIndex))
                {
                    mFreeBlockIndexes.Add(oldBlockData.Length, oldBlockIndex);
                    WriteBlockData(oldBlockIndex);
                }
            }
            else
            {
                int stringIndex = AllocString(name);
                blockData = new BlockData(stringIndex, blockData.ClusterIndex, length);
                mBlockDatas[blockIndex] = blockData;
                WriteBlockData(blockIndex);
            }

            if (hasFile)
            {
                mFileDatas[name] = blockIndex;
            }
            else
            {
                mFileDatas.Add(name, blockIndex);
            }
        }

        private bool TryCombineFreeBlocks(int freeBlockIndex)
        {
            BlockData freeBlockData = mBlockDatas[freeBlockIndex];
            if (freeBlockData.Length <= 0)
            {
                return false;
            }

            int previousFreeBlockIndex = -1;
            int nextFreeBlockIndex = -1;
            int nextBlockDataClusterIndex = freeBlockData.ClusterIndex + GetUpBoundClusterCount(freeBlockData.Length);
            foreach (KeyValuePair<int, GameFrameworkLinkedListRange<int>> blockIndexes in mFreeBlockIndexes)
            {
                if (blockIndexes.Key <= 0)
                {
                    continue;
                }

                int blockDataClusterCount = GetUpBoundClusterCount(blockIndexes.Key);
                foreach (int blockIndex in blockIndexes.Value)
                {
                    BlockData blockData = mBlockDatas[blockIndex];
                    if (blockData.ClusterIndex + blockDataClusterCount == freeBlockData.ClusterIndex)
                    {
                        previousFreeBlockIndex = blockIndex;
                    }
                    else if (blockData.ClusterIndex == nextBlockDataClusterIndex)
                    {
                        nextFreeBlockIndex = blockIndex;
                    }
                }
            }

            if (previousFreeBlockIndex < 0 && nextFreeBlockIndex < 0)
            {
                return false;
            }

            mFreeBlockIndexes.Remove(freeBlockData.Length, freeBlockIndex);
            if (previousFreeBlockIndex >= 0)
            {
                BlockData previousFreeBlockData = mBlockDatas[previousFreeBlockIndex];
                mFreeBlockIndexes.Remove(previousFreeBlockData.Length, previousFreeBlockIndex);
                freeBlockData = new BlockData(previousFreeBlockData.ClusterIndex, previousFreeBlockData.Length + freeBlockData.Length);
                mBlockDatas[previousFreeBlockIndex] = BlockData.Empty;
                mFreeBlockIndexes.Add(0, previousFreeBlockIndex);
                WriteBlockData(previousFreeBlockIndex);
            }

            if (nextFreeBlockIndex >= 0)
            {
                BlockData nextFreeBlockData = mBlockDatas[nextFreeBlockIndex];
                mFreeBlockIndexes.Remove(nextFreeBlockData.Length, nextFreeBlockIndex);
                freeBlockData = new BlockData(freeBlockData.ClusterIndex, freeBlockData.Length + nextFreeBlockData.Length);
                mBlockDatas[nextFreeBlockIndex] = BlockData.Empty;
                mFreeBlockIndexes.Add(0, nextFreeBlockIndex);
                WriteBlockData(nextFreeBlockIndex);
            }

            mBlockDatas[freeBlockIndex] = freeBlockData;
            mFreeBlockIndexes.Add(freeBlockData.Length, freeBlockIndex);
            WriteBlockData(freeBlockIndex);
            return true;
        }

        private int GetEmptyBlockIndex()
        {
            GameFrameworkLinkedListRange<int> lengthRange = default(GameFrameworkLinkedListRange<int>);
            if (mFreeBlockIndexes.TryGetValue(0, out lengthRange))
            {
                int blockIndex = lengthRange.First.Value;
                mFreeBlockIndexes.Remove(0, blockIndex);
                return blockIndex;
            }

            if (mBlockDatas.Count < mHeaderData.MaxBlockCount)
            {
                int blockIndex = mBlockDatas.Count;
                mBlockDatas.Add(BlockData.Empty);
                WriteHeaderData();
                return blockIndex;
            }

            return -1;
        }

        private int AllocBlock(int length)
        {
            if (length <= 0)
            {
                return GetEmptyBlockIndex();
            }

            length = (int)GetUpBoundClusterOffset(length);

            int lengthFound = -1;
            GameFrameworkLinkedListRange<int> lengthRange = default(GameFrameworkLinkedListRange<int>);
            foreach (KeyValuePair<int, GameFrameworkLinkedListRange<int>> i in mFreeBlockIndexes)
            {
                if (i.Key < length)
                {
                    continue;
                }

                if (lengthFound >= 0 && lengthFound < i.Key)
                {
                    continue;
                }

                lengthFound = i.Key;
                lengthRange = i.Value;
            }

            if (lengthFound >= 0)
            {
                if (lengthFound > length && mBlockDatas.Count >= mHeaderData.MaxBlockCount)
                {
                    return -1;
                }

                int blockIndex = lengthRange.First.Value;
                mFreeBlockIndexes.Remove(lengthFound, blockIndex);
                if (lengthFound > length)
                {
                    BlockData blockData = mBlockDatas[blockIndex];
                    mBlockDatas[blockIndex] = new BlockData(blockData.ClusterIndex, length);
                    WriteBlockData(blockIndex);

                    int deltaLength = lengthFound - length;
                    int anotherBlockIndex = GetEmptyBlockIndex();
                    mBlockDatas[anotherBlockIndex] = new BlockData(blockData.ClusterIndex + GetUpBoundClusterCount(length), deltaLength);
                    mFreeBlockIndexes.Add(deltaLength, anotherBlockIndex);
                    WriteBlockData(anotherBlockIndex);
                }

                return blockIndex;
            }
            else
            {
                int blockIndex = GetEmptyBlockIndex();
                if (blockIndex < 0)
                {
                    return -1;
                }

                long fileLength = mStream.Length;
                try
                {
                    mStream.SetLength(fileLength + length);
                }
                catch
                {
                    return -1;
                }

                mBlockDatas[blockIndex] = new BlockData(GetUpBoundClusterCount(fileLength), length);
                WriteBlockData(blockIndex);
                return blockIndex;
            }
        }

        private int AllocString(string value)
        {
            int stringIndex = -1;
            StringData stringData = default(StringData);

            if (mFreeStringIndexes.Count > 0)
            {
                stringIndex = mFreeStringIndexes.Dequeue();
            }
            else
            {
                stringIndex = mStringDatas.Count;
            }

            if (mFreeStringDatas.Count > 0)
            {
                stringData = mFreeStringDatas.Dequeue();
            }
            else
            {
                byte[] bytes = new byte[byte.MaxValue];
                Utility.Random.GetRandomBytes(bytes);
                stringData = new StringData(0, bytes);
            }

            stringData = stringData.SetString(value, mHeaderData.GetEncryptBytes());
            mStringDatas.Add(stringIndex, stringData);
            WriteStringData(stringIndex, stringData);
            return stringIndex;
        }

        private void WriteHeaderData()
        {
            mHeaderData = mHeaderData.SetBlockCount(mBlockDatas.Count);
            Utility.Marshal.StructureToBytes(mHeaderData, HeaderDataSize, CachedBytes);
            mStream.Position = 0L;
            mStream.Write(CachedBytes, 0, HeaderDataSize);
        }

        private void WriteBlockData(int blockIndex)
        {
            Utility.Marshal.StructureToBytes(mBlockDatas[blockIndex], BlockDataSize, CachedBytes);
            mStream.Position = mBlockDataOffset + BlockDataSize * blockIndex;
            mStream.Write(CachedBytes, 0, BlockDataSize);
        }

        private StringData ReadStringData(int stringIndex)
        {
            mStream.Position = mStringDataOffset + StringDataSize * stringIndex;
            mStream.Read(CachedBytes, 0, StringDataSize);
            return Utility.Marshal.BytesToStructure<StringData>(StringDataSize, CachedBytes);
        }

        private void WriteStringData(int stringIndex, StringData stringData)
        {
            Utility.Marshal.StructureToBytes(stringData, StringDataSize, CachedBytes);
            mStream.Position = mStringDataOffset + StringDataSize * stringIndex;
            mStream.Write(CachedBytes, 0, StringDataSize);
        }

        private static void CalcOffsets(FileSystem fileSystem)
        {
            fileSystem.mBlockDataOffset = HeaderDataSize;
            fileSystem.mStringDataOffset = fileSystem.mBlockDataOffset + BlockDataSize * fileSystem.mHeaderData.MaxBlockCount;
            fileSystem.mFileDataOffset = (int)GetUpBoundClusterOffset(fileSystem.mStringDataOffset + StringDataSize * fileSystem.mHeaderData.MaxFileCount);
        }

        private static long GetUpBoundClusterOffset(long offset)
        {
            return (offset - 1L + ClusterSize) / ClusterSize * ClusterSize;
        }

        private static int GetUpBoundClusterCount(long length)
        {
            return (int)((length - 1L + ClusterSize) / ClusterSize);
        }

        private static long GetClusterOffset(int clusterIndex)
        {
            return (long)ClusterSize * clusterIndex;
        }
    }
}
