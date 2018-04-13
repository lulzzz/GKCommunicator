﻿using System;
using System.ComponentModel;
using System.ServiceModel;
using System.ServiceModel.PeerResolvers;
using System.Windows.Forms;
using GKNetCore;

namespace GKSimpleChat
{
    public partial class ChatForm : Form, IChat
    {
        // a generic delegate to execute a thread against that accepts no args
        private delegate void NoArgDelegate();

        // the chat member name
        private string fMemberName;
        // the channel instance where we execute our service methods against
        private IChatChannel fChatCore;
        // the instance context which in this case is our window since it is the service host
        private InstanceContext fContext;
        // our binding transport for the p2p mesh
        private NetPeerTcpBinding fBinding;
        // the factory to create our chat channel
        private ChannelFactory<IChatChannel> fChannelFactory;
        // an interface provided by the channel exposing events to indicate
        // when we have connected or disconnected from the mesh
        private IOnlineStatus fStatusHandler;

        private IChatCore fCore;

        public ChatForm()
        {
            InitializeComponent();
            Closing += new CancelEventHandler(WindowMain_Closing);
            txtMemberName.Focus();
        }

        void WindowMain_Closing(object sender, CancelEventArgs e)
        {
            if (fChatCore != null) {
                fChatCore.Leave(fMemberName);
                fChatCore.Close();
            }
        }

        // this method gets called from a background thread to 
        // connect the service client to the p2p mesh specified
        // by the binding info in the app.config
        private void ConnectToMesh()
        {
            //since this window is the service behavior use it as the instance context
            fContext = new InstanceContext(this);

            //use the binding from the app.config with default settings
            //m_binding = new NetPeerTcpBinding("WPFChatBinding");
            fBinding = new NetPeerTcpBinding();
            fBinding.Port = 0;
            fBinding.Resolver.Mode = PeerResolverMode.Auto;
            fBinding.Security.Mode = SecurityMode.None;

            //create a new channel based off of our composite interface "IChatChannel" and the 
            //endpoint specified in the app.config
            //m_channelFactory = new DuplexChannelFactory<IChatChannel>(m_site, "WPFChatEndpoint");
            fChannelFactory = new DuplexChannelFactory<IChatChannel>(fContext, fBinding, "net.p2p://gedkeeper.network");
            fChatCore = fChannelFactory.CreateChannel();

            //the next 3 lines setup the event handlers for handling online/offline events
            //in the MS P2P world, online/offline is defined as follows:
            //Online: the client is connected to one or more peers in the mesh
            //Offline: the client is all alone in the mesh
            fStatusHandler = fChatCore.GetProperty<IOnlineStatus>();
            fStatusHandler.Online += new EventHandler(ostat_Online);
            fStatusHandler.Offline += new EventHandler(ostat_Offline);

            //this is an empty unhandled method on the service interface.
            //why? because for some reason p2p clients don't try to connect to the mesh
            //until the first service method call.  so to facilitate connecting i call this method
            //to get the ball rolling.
            fChatCore.InitializeMesh();
        }

        #region IChat Members

        public void Join(string Member)
        {
            // again we need to sync the worker thread with the UI thread via Dispatcher
            Invoke(
                (MethodInvoker)delegate {
                    //add the joined member to the chatroom
                    lstChatMsgs.Items.Add(Member + " joined the chatroom.");

                    //this will retrieve any new members that have joined before the current user
                    fChatCore.SynchronizeMemberList(fMemberName);
                });
        }

        public void Chat(string Member, string Message)
        {
            // again we need to sync the worker thread with the UI thread via Dispatcher
            Invoke(
                (MethodInvoker)delegate {
                    //we simply add the chat message to the listbox
                    lstChatMsgs.Items.Add(Member + " says: " + Message);
                });
        }

        // again we need to sync the worker thread with the UI thread via Dispatcher
        public void Whisper(string Member, string MemberTo, string Message)
        {
            Invoke(
                (MethodInvoker)delegate {
                    //this is a rudimentary form of whisper and is flawed so should NOT be used in production.
                    //this method simply checks the sender and to address and only displays the message
                    //if it belongs to this member, however! - if there are N members with the same name
                    //they will all be whispered to from the sender since the message is broadcast to everybody.
                    //the correct way to implement this would
                    //be to instead retrieve the peer name from the mesh for the member you want to whisper to
                    //and send the message directly to that peer node via the mesh.  i may update the code to do 
                    //that in the future but for now i'm too busy with other things to mess with it hence it's
                    //left as an exercise for the reader.  good luck! ;-)
                    if (fMemberName.Equals(Member) || fMemberName.Equals(MemberTo)) {
                        //we simply add the whisper message to the listbox
                        lstChatMsgs.Items.Add(Member + " whispers: " + Message);
                    }
                });
        }

        public void InitializeMesh()
        {
            // do nothing
        }

        public void Leave(string Member)
        {
            // again we need to sync the worker thread with the UI thread via Dispatcher
            Invoke(
                (MethodInvoker)delegate {
                    //notify that the user has left
                    lstChatMsgs.Items.Add(Member + " left the chatroom.");
                });
        }

        public void SynchronizeMemberList(string Member)
        {
            // again we need to sync the worker thread with the UI thread via Dispatcher
            Invoke(
                (MethodInvoker)delegate {
                    //as member names come in we simply disregard duplicates and 
                    //add them to the member list, this way we can retrieve a list
                    //of members already in the chatroom when we enter at any time.

                    //again, since this is just an example this is the simplified
                    //way to do things.  the correct way would be to retrieve a list
                    //of peernames and retrieve the metadata from each one which would
                    //tell us what the member name is and add it.  we would want to check
                    //this list when we join the mesh to make sure our member name doesn't 
                    //conflict with someone else
                    if (!lstMembers.Items.Contains(Member)) {
                        lstMembers.Items.Add(Member);
                    }
                });
        }

        #endregion

        private void btnConnect_Click(object sender, EventArgs e)
        {
            lblConnectionStatus.Visible = true;
            // join the P2P mesh from a worker thread
            NoArgDelegate executor = new NoArgDelegate(ConnectToMesh);
            executor.BeginInvoke(null, null);
        }

        private void btnChat_Click(object sender, EventArgs e)
        {
            // broadcast the chat message to the peer mesh and clear the box
            if (!String.IsNullOrEmpty(txtChatMsg.Text)) {
                fChatCore.Chat(fMemberName, txtChatMsg.Text);
                txtChatMsg.Clear();
                txtChatMsg.Focus();
            }
        }

        private void btnWhisper_Click(object sender, EventArgs e)
        {
            // broadcast the chat message to the peer mesh with the member name it is intended for
            if ((!String.IsNullOrEmpty(txtChatMsg.Text)) && (lstMembers.SelectedIndex >= 0)) {
                fChatCore.Whisper(fMemberName, lstMembers.SelectedItem.ToString(), txtChatMsg.Text);
                txtChatMsg.Clear();
                txtChatMsg.Focus();
            }
        }

        void ostat_Offline(object sender, EventArgs e)
        {
            // we could update a status bar or animate an icon to 
            //indicate to the user they have disconnected from the mesh

            //currently i don't have a "disconnect" button but adding it
            //should be trivial if you understand the rest of this code
        }

        void ostat_Online(object sender, EventArgs e)
        {
            //because this event handler is called from a worker thread
            //we need to use the dispatcher to sync it with the UI thread.
            //below illustrates how and is used throughout the code.
            //note that a generic handler could be used to prevent having to recode the delegate
            //each time but i didn't bother since the app is so small at this time.
            Invoke((MethodInvoker)delegate {
                //here we retrieve the chat member name
                fMemberName = txtMemberName.Text;

                //updating the UI to show the chat window
                //grdLogin.Visibility = Visibility.Collapsed;
                //grdChat.Visibility = Visibility.Visible;
                //((Storyboard)Resources["OnJoinMesh"]).Begin(this);
                lblConnectionStatus.Text = "Welcome to the chat room!";
                //((Storyboard)Resources["HideConnectStatus"]).Begin(this);

                //broadcasting a join method call to the mesh members
                fChatCore.Join(fMemberName);
            });
        }

        [STAThread]
        static void Main()
        {
            Application.Run(new ChatForm());
        }
    }
}