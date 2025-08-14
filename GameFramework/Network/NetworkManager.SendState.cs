//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

using System;
using System.IO;

namespace GameFramework.Network
{
    internal sealed partial class NetworkManager : GameFrameworkModule, INetworkManager
    {
        private sealed class SendState : IDisposable
        {
            private const int DefaultBufferLength = 1024 * 64;
            private MemoryStream mStream;
            private bool mDisposed;

            public SendState()
            {
                mStream = new MemoryStream(DefaultBufferLength);
                mDisposed = false;
            }

            public MemoryStream Stream
            {
                get
                {
                    return mStream;
                }
            }

            public void Reset()
            {
                mStream.Position = 0L;
                mStream.SetLength(0L);
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            private void Dispose(bool disposing)
            {
                if (mDisposed)
                {
                    return;
                }

                if (disposing)
                {
                    if (mStream != null)
                    {
                        mStream.Dispose();
                        mStream = null;
                    }
                }

                mDisposed = true;
            }
        }
    }
}
