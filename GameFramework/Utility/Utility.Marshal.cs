//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

using System;

namespace GameFramework
{
    public static partial class Utility
    {
        public static class Marshal
        {
            private const int BlockSize = 1024 * 4;
            private static IntPtr s_CachedHGlobalPtr = IntPtr.Zero;
            private static int s_CachedHGlobalSize = 0;

            public static int CachedHGlobalSize
            {
                get
                {
                    return s_CachedHGlobalSize;
                }
            }

            public static void EnsureCachedHGlobalSize(int ensureSize)
            {
                if (ensureSize < 0)
                {
                    throw new GameFrameworkException("Ensure size is invalid.");
                }

                if (s_CachedHGlobalPtr == IntPtr.Zero || s_CachedHGlobalSize < ensureSize)
                {
                    FreeCachedHGlobal();
                    int size = (ensureSize - 1 + BlockSize) / BlockSize * BlockSize;
                    s_CachedHGlobalPtr = System.Runtime.InteropServices.Marshal.AllocHGlobal(size);
                    s_CachedHGlobalSize = size;
                }
            }

            public static void FreeCachedHGlobal()
            {
                if (s_CachedHGlobalPtr != IntPtr.Zero)
                {
                    System.Runtime.InteropServices.Marshal.FreeHGlobal(s_CachedHGlobalPtr);
                    s_CachedHGlobalPtr = IntPtr.Zero;
                    s_CachedHGlobalSize = 0;
                }
            }

            public static byte[] StructureToBytes<T>(T structure)
            {
                return StructureToBytes(structure, System.Runtime.InteropServices.Marshal.SizeOf(typeof(T)));
            }

            internal static byte[] StructureToBytes<T>(T structure, int structureSize)
            {
                if (structureSize < 0)
                {
                    throw new GameFrameworkException("Structure size is invalid.");
                }

                EnsureCachedHGlobalSize(structureSize);
                System.Runtime.InteropServices.Marshal.StructureToPtr(structure, s_CachedHGlobalPtr, true);
                byte[] result = new byte[structureSize];
                System.Runtime.InteropServices.Marshal.Copy(s_CachedHGlobalPtr, result, 0, structureSize);
                return result;
            }

            public static void StructureToBytes<T>(T structure, byte[] result)
            {
                StructureToBytes(structure, System.Runtime.InteropServices.Marshal.SizeOf(typeof(T)), result, 0);
            }

            internal static void StructureToBytes<T>(T structure, int structureSize, byte[] result)
            {
                StructureToBytes(structure, structureSize, result, 0);
            }

            public static void StructureToBytes<T>(T structure, byte[] result, int startIndex)
            {
                StructureToBytes(structure, System.Runtime.InteropServices.Marshal.SizeOf(typeof(T)), result, startIndex);
            }

            internal static void StructureToBytes<T>(T structure, int structureSize, byte[] result, int startIndex)
            {
                if (structureSize < 0)
                {
                    throw new GameFrameworkException("Structure size is invalid.");
                }

                if (result == null)
                {
                    throw new GameFrameworkException("Result is invalid.");
                }

                if (startIndex < 0)
                {
                    throw new GameFrameworkException("Start index is invalid.");
                }

                if (startIndex + structureSize > result.Length)
                {
                    throw new GameFrameworkException("Result length is not enough.");
                }

                EnsureCachedHGlobalSize(structureSize);
                System.Runtime.InteropServices.Marshal.StructureToPtr(structure, s_CachedHGlobalPtr, true);
                System.Runtime.InteropServices.Marshal.Copy(s_CachedHGlobalPtr, result, startIndex, structureSize);
            }

            public static T BytesToStructure<T>(byte[] buffer)
            {
                return BytesToStructure<T>(System.Runtime.InteropServices.Marshal.SizeOf(typeof(T)), buffer, 0);
            }

            public static T BytesToStructure<T>(byte[] buffer, int startIndex)
            {
                return BytesToStructure<T>(System.Runtime.InteropServices.Marshal.SizeOf(typeof(T)), buffer, startIndex);
            }

            internal static T BytesToStructure<T>(int structureSize, byte[] buffer)
            {
                return BytesToStructure<T>(structureSize, buffer, 0);
            }

            internal static T BytesToStructure<T>(int structureSize, byte[] buffer, int startIndex)
            {
                if (structureSize < 0)
                {
                    throw new GameFrameworkException("Structure size is invalid.");
                }

                if (buffer == null)
                {
                    throw new GameFrameworkException("Buffer is invalid.");
                }

                if (startIndex < 0)
                {
                    throw new GameFrameworkException("Start index is invalid.");
                }

                if (startIndex + structureSize > buffer.Length)
                {
                    throw new GameFrameworkException("Buffer length is not enough.");
                }

                EnsureCachedHGlobalSize(structureSize);
                System.Runtime.InteropServices.Marshal.Copy(buffer, startIndex, s_CachedHGlobalPtr, structureSize);
                return (T)System.Runtime.InteropServices.Marshal.PtrToStructure(s_CachedHGlobalPtr, typeof(T));
            }
        }
    }
}
