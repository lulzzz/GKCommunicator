﻿using System;
using System.Linq;
using System.Net;
using GKNet.TCP;
using NUnit.Framework;

namespace GKNet
{
    [TestFixture]
    public class CommunicatorCoreTests
    {
        private class ChatForm : IChatForm
        {
            public ICommunicatorCore Core { get { return null; } }

            public void OnInitialized()
            {
            }

            public void OnJoin(Peer member)
            {
            }

            public void OnLeave(Peer member)
            {
            }

            public void OnMessageReceived(Peer sender, string message)
            {
            }

            public void OnPeersListChanged()
            {
            }
        }

        [Test]
        public void Test_ctor()
        {
            Assert.Throws(typeof(ArgumentNullException), () => { new CommunicatorCore(null); });

            var chatForm = new ChatForm();
            using (var core = new CommunicatorCore(chatForm)) {
                Assert.IsNotNull(core);
                Assert.AreEqual(false, core.IsConnected);
                Assert.IsNotNull(core.DHTClient);
                Assert.IsNotNull(core.Profile);
                Assert.IsNotNull(core.Peers);
                //Assert.IsNull(core.STUNInfo);
                Assert.IsNotNull(core.GetPeersList());
                Assert.IsNotNull(core.Database);

                var peer = core.AddPeer(IPAddress.Any, 1111);
                Assert.IsNotNull(peer);

                peer = core.FindPeer(IPAddress.Any);
                Assert.IsNotNull(peer);

                bool res = core.CheckPeer(null);
                Assert.IsFalse(res);

                res = core.CheckPeer(new IPEndPoint(IPAddress.Any, 1111));
                Assert.IsTrue(res);

                res = core.UpdatePeer(new IPEndPoint(IPAddress.Any, 1111));
                Assert.IsFalse(res);
            }
        }

        [Test]
        public void Test_AddPeer_FindPeer()
        {
        }

        [Test]
        public void Test_DataReceiveEventArgs()
        {
            IPEndPoint peerEndPoint = new IPEndPoint(IPAddress.Any, 1111);
            byte[] data = new byte[] {};
            var evt = new DataReceiveEventArgs(data, peerEndPoint);
            Assert.IsNotNull(evt);
            Assert.AreEqual(peerEndPoint, evt.Peer);
            Assert.AreEqual(data, evt.Data);
        }
    }
}
