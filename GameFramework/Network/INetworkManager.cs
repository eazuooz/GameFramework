//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace GameFramework.Network
{
    public interface INetworkManager
    {
        int NetworkChannelCount
        {
            get;
        }

        event EventHandler<NetworkConnectedEventArgs> NetworkConnected;

        event EventHandler<NetworkClosedEventArgs> NetworkClosed;

        event EventHandler<NetworkMissHeartBeatEventArgs> NetworkMissHeartBeat;

        event EventHandler<NetworkErrorEventArgs> NetworkError;

        event EventHandler<NetworkCustomErrorEventArgs> NetworkCustomError;

        bool HasNetworkChannel(string name);

        INetworkChannel GetNetworkChannel(string name);

        INetworkChannel[] GetAllNetworkChannels();

        void GetAllNetworkChannels(List<INetworkChannel> results);

        INetworkChannel CreateNetworkChannel(string name, ServiceType serviceType, INetworkChannelHelper networkChannelHelper);

        bool DestroyNetworkChannel(string name);
    }
}
