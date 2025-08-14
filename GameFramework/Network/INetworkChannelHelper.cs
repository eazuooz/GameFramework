//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

using System.IO;

namespace GameFramework.Network
{
    public interface INetworkChannelHelper
    {
        int PacketHeaderLength
        {
            get;
        }

        void Initialize(INetworkChannel networkChannel);

        void Shutdown();

        void PrepareForConnecting();

        bool SendHeartBeat();

        bool Serialize<T>(T packet, Stream destination) where T : Packet;

        IPacketHeader DeserializePacketHeader(Stream source, out object customErrorData);

        Packet DeserializePacket(IPacketHeader packetHeader, Stream source, out object customErrorData);
    }
}
