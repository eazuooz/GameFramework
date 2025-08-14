//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

using System.Collections.Generic;
using System.IO;

namespace GameFramework
{
    public abstract class GameFrameworkSerializer<T>
    {
        private readonly Dictionary<byte, SerializeCallback> mSerializeCallbacks;
        private readonly Dictionary<byte, DeserializeCallback> mDeserializeCallbacks;
        private readonly Dictionary<byte, TryGetValueCallback> mTryGetValueCallbacks;
        private byte mLatestSerializeCallbackVersion;

        public GameFrameworkSerializer()
        {
            mSerializeCallbacks = new Dictionary<byte, SerializeCallback>();
            mDeserializeCallbacks = new Dictionary<byte, DeserializeCallback>();
            mTryGetValueCallbacks = new Dictionary<byte, TryGetValueCallback>();
            mLatestSerializeCallbackVersion = 0;
        }

        public delegate bool SerializeCallback(Stream stream, T data);

        public delegate T DeserializeCallback(Stream stream);

        public delegate bool TryGetValueCallback(Stream stream, string key, out object value);

        public void RegisterSerializeCallback(byte version, SerializeCallback callback)
        {
            if (callback == null)
            {
                throw new GameFrameworkException("Serialize callback is invalid.");
            }

            mSerializeCallbacks[version] = callback;
            if (version > mLatestSerializeCallbackVersion)
            {
                mLatestSerializeCallbackVersion = version;
            }
        }

        public void RegisterDeserializeCallback(byte version, DeserializeCallback callback)
        {
            if (callback == null)
            {
                throw new GameFrameworkException("Deserialize callback is invalid.");
            }

            mDeserializeCallbacks[version] = callback;
        }

        public void RegisterTryGetValueCallback(byte version, TryGetValueCallback callback)
        {
            if (callback == null)
            {
                throw new GameFrameworkException("Try get value callback is invalid.");
            }

            mTryGetValueCallbacks[version] = callback;
        }

        public bool Serialize(Stream stream, T data)
        {
            if (mSerializeCallbacks.Count <= 0)
            {
                throw new GameFrameworkException("No serialize callback registered.");
            }

            return Serialize(stream, data, mLatestSerializeCallbackVersion);
        }

        public bool Serialize(Stream stream, T data, byte version)
        {
            byte[] header = GetHeader();
            stream.WriteByte(header[0]);
            stream.WriteByte(header[1]);
            stream.WriteByte(header[2]);
            stream.WriteByte(version);
            SerializeCallback callback = null;
            if (!mSerializeCallbacks.TryGetValue(version, out callback))
            {
                throw new GameFrameworkException(Utility.Text.Format("Serialize callback '{0}' is not exist.", version));
            }

            return callback(stream, data);
        }

        public T Deserialize(Stream stream)
        {
            byte[] header = GetHeader();
            byte header0 = (byte)stream.ReadByte();
            byte header1 = (byte)stream.ReadByte();
            byte header2 = (byte)stream.ReadByte();
            if (header0 != header[0] || header1 != header[1] || header2 != header[2])
            {
                throw new GameFrameworkException(Utility.Text.Format("Header is invalid, need '{0}{1}{2}', current '{3}{4}{5}'.", (char)header[0], (char)header[1], (char)header[2], (char)header0, (char)header1, (char)header2));
            }

            byte version = (byte)stream.ReadByte();
            DeserializeCallback callback = null;
            if (!mDeserializeCallbacks.TryGetValue(version, out callback))
            {
                throw new GameFrameworkException(Utility.Text.Format("Deserialize callback '{0}' is not exist.", version));
            }

            return callback(stream);
        }

        public bool TryGetValue(Stream stream, string key, out object value)
        {
            value = null;
            byte[] header = GetHeader();
            byte header0 = (byte)stream.ReadByte();
            byte header1 = (byte)stream.ReadByte();
            byte header2 = (byte)stream.ReadByte();
            if (header0 != header[0] || header1 != header[1] || header2 != header[2])
            {
                return false;
            }

            byte version = (byte)stream.ReadByte();
            TryGetValueCallback callback = null;
            if (!mTryGetValueCallbacks.TryGetValue(version, out callback))
            {
                return false;
            }

            return callback(stream, key, out value);
        }

        protected abstract byte[] GetHeader();
    }
}
