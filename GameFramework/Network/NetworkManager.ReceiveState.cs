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
        private sealed class ReceiveState : IDisposable
        {
            private const int DefaultBufferLength = 1024 * 64;
            private MemoryStream mStream;
            private IPacketHeader mPacketHeader;
            private bool mDisposed;

            public ReceiveState()
            {
                mStream = new MemoryStream(DefaultBufferLength);
                mPacketHeader = null;
                mDisposed = false;
            }

            public MemoryStream Stream
            {
                get
                {
                    return mStream;
                }
            }

            public IPacketHeader PacketHeader
            {
                get
                {
                    return mPacketHeader;
                }
            }

            public void PrepareForPacketHeader(int packetHeaderLength)
            {
                Reset(packetHeaderLength, null);
            }

            public void PrepareForPacket(IPacketHeader packetHeader)
            {
                if (packetHeader == null)
                {
                    throw new GameFrameworkException("Packet header is invalid.");
                }

                Reset(packetHeader.PacketLength, packetHeader);
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

            private void Reset(int targetLength, IPacketHeader packetHeader)
            {
                if (targetLength < 0)
                {
                    throw new GameFrameworkException("Target length is invalid.");
                }

                mStream.Position = 0L;
                mStream.SetLength(targetLength);
                mPacketHeader = packetHeader;
            }
        }
    }
}
