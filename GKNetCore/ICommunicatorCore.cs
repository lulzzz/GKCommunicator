﻿/*
 *  "GKCommunicator", the chat and bulletin board of the genealogical network.
 *  Copyright (C) 2018 by Sergey V. Zhdanovskih.
 *
 *  This file is part of "GEDKeeper".
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System.Collections.Generic;
using System.Net;
using GKNet.Database;
using GKNet.DHT;
using LumiSoft.Net.STUN.Client;

namespace GKNet
{
    public interface ICommunicatorCore
    {
        IDatabase Database { get; }
        bool IsConnected { get; }
        IList<Peer> Peers { get; }
        UserProfile Profile { get; }
        int TCPListenerPort { get; set; }
        DHTClient DHTClient { get; }
        STUN_Result STUNInfo { get; }

        void Connect();
        void Disconnect();
        void Join(string member);
        void Leave(string member);
        void Send(Peer target, string message);
        void SendToAll(string message);

        Peer AddPeer(IPAddress peerAddress, int port);
        Peer FindPeer(IPAddress peerAddress);
    }
}
