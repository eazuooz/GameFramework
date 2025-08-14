//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

using System;
using System.Net;
using System.Net.Sockets;

namespace GameFramework.Network
{
    public interface INetworkChannel
    {
        string Name
        {
            get;
        }

        Socket Socket
        {
            get;
        }

        bool Connected
        {
            get;
        }

        ServiceType ServiceType
        {
            get;
        }

        AddressFamily AddressFamily
        {
            get;
        }

        int SendPacketCount
        {
            get;
        }

        int SentPacketCount
        {
            get;
        }

        int ReceivePacketCount
        {
            get;
        }

        int ReceivedPacketCount
        {
            get;
        }

        bool ResetHeartBeatElapseSecondsWhenReceivePacket
        {
            get;
            set;
        }

        int MissHeartBeatCount
        {
            get;
        }

        float HeartBeatInterval
        {
            get;
            set;
        }

        float HeartBeatElapseSeconds
        {
            get;
        }

        void RegisterHandler(IPacketHandler handler);

        void SetDefaultHandler(EventHandler<Packet> handler);

        void Connect(IPAddress ipAddress, int port);

        void Connect(IPAddress ipAddress, int port, object userData);

        void Close();

        void Send<T>(T packet) where T : Packet;
    }
}
