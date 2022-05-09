using MaterialDesignThemes.Wpf;
using SocketIOClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace GamaManager.Dialogs
{
    /// <summary>
    /// Логика взаимодействия для TalkDialog.xaml
    /// </summary>
    public partial class TalkDialog : Window
    {

        public string currentUserId = "";
        public string talkId = "";
        public SocketIO client = null;
        public int activeChatIndex = -1;
        public DateTime lastInputTimeStamp; 
        public Brush msgsSeparatorBrush = null;
        public bool isStartBlink;

        private const UInt32 FLASHW_STOP = 0; //Stop flashing. The system restores the window to its original state.        private const UInt32 FLASHW_CAPTION = 1; //Flash the window caption.        
        private const UInt32 FLASHW_TRAY = 2; //Flash the taskbar button.        
        private const UInt32 FLASHW_ALL = 3; //Flash both the window caption and taskbar button.        
        private const UInt32 FLASHW_TIMER = 4; //Flash continuously, until the FLASHW_STOP flag is set.        
        private const UInt32 FLASHW_TIMERNOFG = 12; //Flash continuously until the window comes to the foreground.  

        [StructLayout(LayoutKind.Sequential)]
        private struct FLASHWINFO
        {
            public UInt32 cbSize; //The size of the structure in bytes.            
            public IntPtr hwnd; //A Handle to the Window to be Flashed. The window can be either opened or minimized.


            public UInt32 dwFlags; //The Flash Status.            
            public UInt32 uCount; // number of times to flash the window            
            public UInt32 dwTimeout; //The rate at which the Window is to be flashed, in milliseconds. If Zero, the function uses the default cursor blink rate.        
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool FlashWindowEx(ref FLASHWINFO pwfi);

        public TalkDialog(string currentUserId, string talkId, SocketIO client, bool isStartBlink)
        {
            InitializeComponent();

            this.currentUserId = currentUserId;
            this.talkId = talkId;
            this.client = client;
            this.isStartBlink = isStartBlink;
            SetTalkNameLabel();
            ToggleOwnerMenu();
            SetUsersCountLabel();
            GetUsers();
            GetTextChannels();
        }

        public void SetUsersCountLabel ()
        {
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/talks/relations/all");
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (var reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        string objText = reader.ReadToEnd();
                        TalkRelationsResponseInfo myObj = (TalkRelationsResponseInfo)js.Deserialize(objText, typeof(TalkRelationsResponseInfo));
                        string status = myObj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
                            List<TalkRelation> relations = myObj.relations;
                            List<TalkRelation> currentTalkUsers = relations.Where<TalkRelation>((TalkRelation relation) =>
                            {
                                string localTalkId = relation.talk;
                                bool isCurrentTalk = talkId == localTalkId;
                                return isCurrentTalk;
                            }).ToList<TalkRelation>();
                            int usersCount = currentTalkUsers.Count;
                            int countOnlineCurrentTalkUsers = currentTalkUsers.Count((TalkRelation currentTalkUser) =>
                            {
                                bool isOnline = false;
                                string userId = currentTalkUser.user;
                                HttpWebRequest innerWebRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/users/get/?id=" + userId);
                                innerWebRequest.Method = "GET";
                                innerWebRequest.UserAgent = ".NET Framework Test Client";
                                using (HttpWebResponse innerWebResponse = (HttpWebResponse)innerWebRequest.GetResponse())
                                {
                                    using (StreamReader innerReader = new StreamReader(innerWebResponse.GetResponseStream()))
                                    {
                                        js = new JavaScriptSerializer();
                                        objText = innerReader.ReadToEnd();
                                        UserResponseInfo myInnerObj = (UserResponseInfo)js.Deserialize(objText, typeof(UserResponseInfo));
                                        status = myInnerObj.status;
                                        isOkStatus = status == "OK";
                                        if (isOkStatus)
                                        {
                                            User user = myInnerObj.user;
                                            string userStatus = user.status;
                                            isOnline = userStatus == "online";
                                        }
                                    }
                                }
                                return isOnline;
                            });
                            string onlineUsersCountLabelContent = countOnlineCurrentTalkUsers.ToString();
                            onlineUsersCountLabel.Text = onlineUsersCountLabelContent;
                            string rawUsersCount = usersCount.ToString();
                            string usersCountLabelContent = "Участников: " + rawUsersCount;
                            usersCountLabel.Text = usersCountLabelContent;
                            /*foreach (TalkRelation currentTalkUser in currentTalkUsers)
                            {
                                string userId = currentTalkUser.user;
                                HttpWebRequest innerWebRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/users/get/?id=" + userId);
                                innerWebRequest.Method = "GET";
                                innerWebRequest.UserAgent = ".NET Framework Test Client";
                                using (HttpWebResponse innerWebResponse = (HttpWebResponse)innerWebRequest.GetResponse())
                                {
                                    using (StreamReader innerReader = new StreamReader(innerWebResponse.GetResponseStream()))
                                    {
                                        js = new JavaScriptSerializer();
                                        objText = innerReader.ReadToEnd();
                                        UserResponseInfo myInnerObj = (UserResponseInfo)js.Deserialize(objText, typeof(UserResponseInfo));
                                        status = myInnerObj.status;
                                        isOkStatus = status == "OK";
                                        if (isOkStatus)
                                        {
                                            User user = myInnerObj.user;
                                            string userName = user.name;
                                            string userStatus = user.status;
                                            StackPanel talkUser = new StackPanel();
                                            talkUser.Orientation = Orientation.Horizontal;
                                            talkUser.Height = 50;
                                            Image talkUserAvatar = new Image();
                                            talkUserAvatar.Width = 35;
                                            talkUserAvatar.Height = 35;
                                            talkUserAvatar.Margin = new Thickness(10);
                                            talkUserAvatar.BeginInit();
                                            talkUserAvatar.Source = new BitmapImage(new Uri(@"http://localhost:4000/api/user/avatar/?id=" + userId));
                                            talkUserAvatar.EndInit();
                                            talkUserAvatar.ImageFailed += SetDefaultAvatarHandler;
                                            talkUser.Children.Add(talkUserAvatar);
                                            StackPanel talkUserAside = new StackPanel();
                                            TextBlock talkUserNameLabel = new TextBlock();
                                            talkUserNameLabel.FontSize = 14;
                                            talkUserNameLabel.Margin = new Thickness(0, 5, 0, 5);
                                            talkUserNameLabel.Text = userName;
                                            talkUserAside.Children.Add(talkUserNameLabel);
                                            TextBlock talkUserStatusLabel = new TextBlock();
                                            talkUserStatusLabel.Margin = new Thickness(0, 5, 0, 5);
                                            talkUserStatusLabel.Text = userStatus;
                                            talkUserAside.Children.Add(talkUserStatusLabel);
                                            talkUser.Children.Add(talkUserAside);
                                            users.Children.Add(talkUser);
                                        }
                                    }
                                }
                            }*/
                        }
                    }
                }
            }
            catch (System.Net.WebException)
            {
                MessageBox.Show("Не удается подключиться к серверу", "Ошибка");
                this.Close();
            }
        }

        public void ToggleOwnerMenu ()
        {
            Talk talk = GetTalkInfo();
            string owner = talk.owner;
            /*bool isOwner = owner == currentUserId;
            if (isOwner)
            {
                ownerMenu.Visibility = Visibility.Visible;
            }
            else
            {
                ownerMenu.Visibility = Visibility.Collapsed;
            }*/
        }

        public void SetTalkNameLabel()
        {
            Talk talk = GetTalkInfo();
            string talkTitle = talk.title;
            talkTitleLabel.Text = talkTitle;
        }

        public void InitializeHandler(object sender, RoutedEventArgs e)
        {
            Initialize();
        }

        async public void ReceiveMessages()
        {
            try
            {
                client = new SocketIO("http://localhost:4000/");
                // client = new SocketIO("https://digitaldistributtionservice.herokuapp.com/");
                await client.ConnectAsync();
                client.On("friend_send_msg", async response =>
                {
                    var rawResult = response.GetValue<string>();
                    string[] result = rawResult.Split(new char[] { '|' });
                    string userId = result[0];
                    string msg = result[1];
                    string cachedFriendId = result[2];
                    string msgType = result[3];
                    string cachedId = result[4];
                    string msgChannelId = result[5];
                    Debugger.Log(0, "debug", Environment.NewLine + "user " + userId + " send msg: " + msg + Environment.NewLine);
                    string currentFriendId = this.talkId;
                    bool isCurrentChat = currentFriendId == cachedFriendId && userId != currentUserId;
                    Debugger.Log(0, "debug", Environment.NewLine + "isCurrentChat: " + isCurrentChat.ToString() + Environment.NewLine);
                    if (isCurrentChat)
                    {
                        this.Dispatcher.Invoke(() =>
                        {
                            try
                            {
                                HttpWebRequest innerWebRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/users/get/?id=" + userId);
                                innerWebRequest.Method = "GET";
                                innerWebRequest.UserAgent = ".NET Framework Test Client";
                                using (HttpWebResponse innerWebResponse = (HttpWebResponse)innerWebRequest.GetResponse())
                                {
                                    using (StreamReader innerReader = new StreamReader(innerWebResponse.GetResponseStream()))
                                    {
                                        JavaScriptSerializer js = new JavaScriptSerializer();
                                        var objText = innerReader.ReadToEnd();
                                        UserResponseInfo myInnerObj = (UserResponseInfo)js.Deserialize(objText, typeof(UserResponseInfo));
                                        string status = myInnerObj.status;
                                        bool isOkStatus = status == "OK";
                                        if (isOkStatus)
                                        {
                                            User friend = myInnerObj.user;
                                            string friendName = friend.name;
                                            ItemCollection chatControlItems = chatControl.Items;
                                            object rawActiveChat = chatControlItems[activeChatIndex];
                                            TabItem activeChat = ((TabItem)(rawActiveChat));

                                            /*object rawActiveChatScrollContent = activeChat.Content;
                                            ScrollViewer activeChatScrollContent = ((ScrollViewer)(rawActiveChatScrollContent));
                                            object rawActiveChatContent = activeChatScrollContent.Content;
                                            StackPanel activeChatContent = ((StackPanel)(rawActiveChatContent));*/

                                            object rawActiveChatControlContent = activeChat.Content;
                                            TabControl activeChatControlContent = ((TabControl)(rawActiveChatControlContent));
                                            ItemCollection activeChatControlContentItems = activeChatControlContent.Items;

                                            // int channelIndex = activeChatControlContent.SelectedIndex;
                                            int channelIndex = activeChatControlContent.SelectedIndex;
                                            foreach (TabItem activeChatControlContentItem in activeChatControlContentItems)
                                            {
                                                object rawChannelId = activeChatControlContentItem.DataContext;
                                                string channelId = ((string)(rawChannelId));
                                                bool isChannelFound = channelId == msgChannelId;
                                                if (isChannelFound)
                                                {
                                                    channelIndex = activeChatControlContentItems.IndexOf(activeChatControlContentItem);
                                                    break;
                                                }
                                            }

                                            object rawActiveChannel = activeChatControlContentItems[channelIndex];
                                            TabItem activeChannel = ((TabItem)(rawActiveChannel));
                                            object rawActiveChatScrollContent = activeChannel.Content;
                                            ScrollViewer activeChatScrollContent = ((ScrollViewer)(rawActiveChatScrollContent));
                                            object rawActiveChatContent = activeChatScrollContent.Content;
                                            StackPanel activeChatContent = ((StackPanel)(rawActiveChatContent));

                                            StackPanel newMsg = new StackPanel();
                                            StackPanel newMsgHeader = new StackPanel();
                                            newMsgHeader.Orientation = Orientation.Horizontal;
                                            Image newMsgHeaderAvatar = new Image();
                                            newMsgHeaderAvatar.Margin = new Thickness(5, 0, 5, 0);
                                            newMsgHeaderAvatar.Width = 25;
                                            newMsgHeaderAvatar.Height = 25;
                                            newMsgHeaderAvatar.BeginInit();
                                            Uri newMsgHeaderAvatarUri = new Uri("http://localhost:4000/api/user/avatar/?id=" + userId);
                                            newMsgHeaderAvatar.Source = new BitmapImage(newMsgHeaderAvatarUri);
                                            newMsgHeaderAvatar.EndInit();
                                            newMsgHeader.Children.Add(newMsgHeaderAvatar);
                                            TextBlock newMsgFriendNameLabel = new TextBlock();
                                            newMsgFriendNameLabel.Margin = new Thickness(5, 0, 5, 0);
                                            newMsgFriendNameLabel.Text = friendName;
                                            newMsgHeader.Children.Add(newMsgFriendNameLabel);
                                            TextBlock newMsgDateLabel = new TextBlock();
                                            newMsgDateLabel.Margin = new Thickness(5, 0, 5, 0);
                                            DateTime currentDate = DateTime.Now;
                                            string rawCurrentDate = currentDate.ToLongTimeString();
                                            newMsgDateLabel.Text = rawCurrentDate;
                                            newMsgHeader.Children.Add(newMsgDateLabel);
                                            newMsg.Children.Add(newMsgHeader);
                                            if (msgType == "text")
                                            {
                                                TextBlock newMsgLabel = new TextBlock();
                                                string newMsgContent = msg;
                                                newMsgLabel.Text = newMsgContent;
                                                newMsgLabel.Margin = new Thickness(40, 10, 10, 10);
                                                inputChatMsgBox.Text = "";
                                                newMsg.Children.Add(newMsgLabel);
                                                activeChatContent.Children.Add(newMsg);
                                            }
                                            else if (msgType == "emoji")
                                            {
                                                Image newMsgLabel = new Image();
                                                newMsgLabel.Margin = new Thickness(40, 10, 10, 10);
                                                newMsgLabel.Width = 35;
                                                newMsgLabel.Height = 35;
                                                newMsgLabel.HorizontalAlignment = HorizontalAlignment.Left;
                                                newMsgLabel.BeginInit();
                                                newMsgLabel.Source = new BitmapImage(new Uri(msg));
                                                newMsgLabel.EndInit();
                                                inputChatMsgBox.Text = "";
                                                newMsg.Children.Add(newMsgLabel);
                                                activeChatContent.Children.Add(newMsg);
                                            }
                                            else if (msgType == "file")
                                            {
                                                Image newMsgLabel = new Image();
                                                newMsgLabel.Margin = new Thickness(40, 10, 10, 10);
                                                newMsgLabel.Width = 35;
                                                newMsgLabel.Height = 35;
                                                newMsgLabel.HorizontalAlignment = HorizontalAlignment.Left;
                                                newMsgLabel.BeginInit();
                                                Uri newMsgLabelUri = new Uri("http://localhost:4000/api/msgs/thumbnail/?id=" + cachedId + @"&content=" + msg);
                                                newMsgLabel.Source = new BitmapImage(newMsgLabelUri);
                                                newMsgLabel.EndInit();
                                                inputChatMsgBox.Text = "";
                                                newMsg.Children.Add(newMsgLabel);
                                                activeChatContent.Children.Add(newMsg);
                                            }
                                        }
                                    }
                                }
                                FlashWindow(this);
                            }
                            catch (System.Net.WebException)
                            {
                                MessageBox.Show("Не удается подключиться к серверу", "Ошибка");
                                this.Close();
                            }
                        });
                    }
                });
                client.On("friend_write_msg", async response =>
                {
                    var rawResult = response.GetValue<string>();
                    string[] result = rawResult.Split(new char[] { '|' });
                    string userId = result[0];
                    string msg = result[1];
                    Debugger.Log(0, "debug", Environment.NewLine + "user " + userId + " send msg: " + msg + Environment.NewLine);
                    try
                    {
                        HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/friends/get");
                        webRequest.Method = "GET";
                        webRequest.UserAgent = ".NET Framework Test Client";
                        using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                        {
                            using (var reader = new StreamReader(webResponse.GetResponseStream()))
                            {
                                JavaScriptSerializer js = new JavaScriptSerializer();
                                var objText = reader.ReadToEnd();
                                FriendsResponseInfo myobj = (FriendsResponseInfo)js.Deserialize(objText, typeof(FriendsResponseInfo));
                                string status = myobj.status;
                                bool isOkStatus = status == "OK";
                                if (isOkStatus)
                                {
                                    List<Friend> friends = myobj.friends;
                                    List<Friend> myFriends = friends.Where<Friend>((Friend joint) =>
                                    {
                                        string localUserId = joint.user;
                                        bool isMyFriend = localUserId == currentUserId;
                                        return isMyFriend;
                                    }).ToList<Friend>();
                                    List<string> friendsIds = new List<string>();
                                    foreach (Friend myFriend in myFriends)
                                    {
                                        string friendId = myFriend.friend;
                                        friendsIds.Add(friendId);
                                    }
                                    bool isMyFriendOnline = friendsIds.Contains(userId);
                                    Debugger.Log(0, "debug", "myFriends: " + myFriends.Count.ToString());
                                    Debugger.Log(0, "debug", "friendsIds: " + String.Join("|", friendsIds));
                                    Debugger.Log(0, "debug", "isMyFriendOnline: " + isMyFriendOnline);
                                    if (isMyFriendOnline)
                                    {
                                        string currentFriendId = this.talkId;
                                        bool isCurrentChat = currentFriendId == userId;
                                        if (isCurrentChat)
                                        {
                                            this.Dispatcher.Invoke(async () =>
                                            {
                                                userIsWritingLabel.Visibility = Visibility.Visible;
                                                await Task.Delay(TimeSpan.FromSeconds(2)).ConfigureAwait(true);
                                                userIsWritingLabel.Visibility = Visibility.Hidden;
                                            });
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch (System.Net.WebException)
                    {
                        MessageBox.Show("Не удается подключиться к серверу", "Ошибка");
                        this.Close();
                    }
                });
            }
            catch (System.Net.WebSockets.WebSocketException)
            {
                Debugger.Log(0, "debug", "Ошибка сокетов");
                await client.ConnectAsync();
            }
        }

        public void AddChat ()
        {
            TabItem newChat = new TabItem();
            newChat.Header = talkId;
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/talks/get/?id=" + talkId);
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (StreamReader reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        string objText = reader.ReadToEnd();
                        var myobj = (TalkResponseInfo)js.Deserialize(objText, typeof(TalkResponseInfo));
                        string status = myobj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
                            Talk talk = myobj.talk;
                            string talkTitle = talk.title;
                            newChat.Header = talkTitle;
                            string userIsWritingLabelContent = talkTitle + " печатает...";
                            userIsWritingLabel.Text = userIsWritingLabelContent;

                            HttpWebRequest innerWebRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/talks/channels/all");
                            innerWebRequest.Method = "GET";
                            innerWebRequest.UserAgent = ".NET Framework Test Client";
                            using (HttpWebResponse innerWebResponse = (HttpWebResponse)innerWebRequest.GetResponse())
                            {
                                using (var innerReader = new StreamReader(innerWebResponse.GetResponseStream()))
                                {
                                    js = new JavaScriptSerializer();
                                    objText = innerReader.ReadToEnd();
                                    TalkChannelsResponseInfo myInnerObj = (TalkChannelsResponseInfo)js.Deserialize(objText, typeof(TalkChannelsResponseInfo));
                                    status = myInnerObj.status;
                                    isOkStatus = status == "OK";
                                    if (isOkStatus)
                                    {
                                        chatControl.Items.Add(newChat);
                                        TabControl newChatControlContent = new TabControl();
                                        List<TalkChannel> channels = myInnerObj.channels;
                                        foreach (TalkChannel channel in channels)
                                        {
                                            string channelId = channel._id;
                                            TabItem newChatControlContentItem = new TabItem();
                                            newChatControlContentItem.Visibility = Visibility.Collapsed;
                                            ScrollViewer newChatScrollContent = new ScrollViewer();
                                            StackPanel newChatContent = new StackPanel();
                                            newChatScrollContent.Content = newChatContent;
                                            newChatControlContentItem.Content = newChatScrollContent;
                                            newChatControlContent.Items.Add(newChatControlContentItem);
                                            newChatControlContentItem.DataContext = channelId;
                                        }
                                        newChatControlContent.SelectedIndex = 0;
                                        newChat.Content = newChatControlContent;
                                        activeChatIndex++;
                                        chatControl.SelectedIndex = activeChatIndex;
                                    }
                                }
                            }

                        }
                    }
                }
            }
            catch (System.Net.WebException)
            {
                MessageBox.Show("Не удается подключиться к серверу", "Ошибка");
                this.Close();
            }

        }

        public void Initialize ()
        {
            InitConstants(currentUserId, talkId, client);
            AddChat();
            ReceiveMessages();
            GetMsgs();
            InitFlash();
            this.DataContext = talkId;
        }

        public void InitConstants(string currentUserId, string talkId, SocketIO client)
        {
            msgsSeparatorBrush = System.Windows.Media.Brushes.LightGray;
            lastInputTimeStamp = DateTime.Now;
        }

        public void GetMsgs ()
        {
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/users/get/?id=" + currentUserId);
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (StreamReader reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        string objText = reader.ReadToEnd();
                        UserResponseInfo myobj = (UserResponseInfo)js.Deserialize(objText, typeof(UserResponseInfo));
                        string status = myobj.status;
                        bool isOkStatus = status == "OK";
                        {
                            if (isOkStatus)
                            {
                                try
                                {
                                    HttpWebRequest innerWebRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/msgs/get");
                                    innerWebRequest.Method = "GET";
                                    innerWebRequest.UserAgent = ".NET Framework Test Client";
                                    using (HttpWebResponse innerWebResponse = (HttpWebResponse)innerWebRequest.GetResponse())
                                    {
                                        using (StreamReader innerReader = new StreamReader(innerWebResponse.GetResponseStream()))
                                        {
                                            js = new JavaScriptSerializer();
                                            objText = innerReader.ReadToEnd();
                                            MsgsResponseInfo myInnerObj = (MsgsResponseInfo)js.Deserialize(objText, typeof(MsgsResponseInfo));
                                            status = myInnerObj.status;
                                            isOkStatus = status == "OK";
                                            if (isOkStatus)
                                            {
                                                List<Msg> msgs = myInnerObj.msgs;
                                                int msgsCursor = -1;
                                                foreach (Msg msg in msgs)
                                                {
                                                    string newMsgUserId = msg.user;
                                                    string newMsgFriendId = msg.friend;
                                                    bool isCurrentChatMsg = newMsgFriendId == talkId;
                                                    if (isCurrentChatMsg)
                                                    {
                                                        
                                                        string newMsgChannelId = msg.channel;
                                                        
                                                        string senderName = "";
                                                        HttpWebRequest nestedWebRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/users/get/?id=" + newMsgUserId);
                                                        nestedWebRequest.Method = "GET";
                                                        nestedWebRequest.UserAgent = ".NET Framework Test Client";
                                                        using (HttpWebResponse nestedWebResponse = (HttpWebResponse)nestedWebRequest.GetResponse())
                                                        {
                                                            using (StreamReader nestedReader = new StreamReader(nestedWebResponse.GetResponseStream()))
                                                            {
                                                                js = new JavaScriptSerializer();
                                                                objText = nestedReader.ReadToEnd();
                                                                UserResponseInfo myNestedObj = (UserResponseInfo)js.Deserialize(objText, typeof(UserResponseInfo));
                                                                status = myNestedObj.status;
                                                                isOkStatus = status == "OK";
                                                                {
                                                                    if (isOkStatus)
                                                                    {
                                                                        User sender = myNestedObj.user;
                                                                        senderName = sender.name;
                                                                    }
                                                                }
                                                            }
                                                        }
                                                        /*User friend = myobj.user;
                                                        string friendName = friend.name;*/
                                                        string friendName = senderName;
                                                        ItemCollection chatControlItems = chatControl.Items;
                                                        object rawActiveChat = chatControlItems[activeChatIndex];
                                                        TabItem activeChat = ((TabItem)(rawActiveChat));

                                                        // object rawActiveChatScrollContent = activeChat.Content;
                                                        // ScrollViewer activeChatScrollContent = ((ScrollViewer)(rawActiveChatScrollContent));
                                                        
                                                        object rawActiveChatControlContent = activeChat.Content;
                                                        TabControl activeChatControlContent = ((TabControl)(rawActiveChatControlContent));
                                                        ItemCollection activeChatControlContentItems = activeChatControlContent.Items;
                                                        // int channelIndex = activeChatControlContent.SelectedIndex;

                                                        int channelIndex = activeChatControlContent.SelectedIndex;
                                                        foreach (TabItem activeChatControlContentItem in activeChatControlContentItems)
                                                        {
                                                            object rawChannelId = activeChatControlContentItem.DataContext;
                                                            string channelId = ((string)(rawChannelId));
                                                            bool isChannelFound = channelId == newMsgChannelId;
                                                            if (isChannelFound)
                                                            {
                                                                channelIndex = activeChatControlContentItems.IndexOf(activeChatControlContentItem);
                                                                break;
                                                            }
                                                        }
                                                        // int channelIndex = activeChatControlContentItems.IndexOf();
                                                        
                                                        object rawActiveChannel = activeChatControlContentItems[channelIndex];
                                                        TabItem activeChannel = ((TabItem)(rawActiveChannel));
                                                        object rawActiveChannelScrollContent = activeChannel.Content;
                                                        ScrollViewer activeChannelScrollContent = ((ScrollViewer)(rawActiveChannelScrollContent));
                                                        
                                                        // object rawActiveChatContent = activeChatScrollContent.Content;
                                                        object rawActiveChatContent = activeChannelScrollContent.Content;

                                                        StackPanel activeChatContent = ((StackPanel)(rawActiveChatContent));
                                                        DateTime msgDate = msg.date;
                                                        string rawMsgDate = msgDate.ToLongDateString();
                                                        msgsCursor++;
                                                        int countMsgs = msgs.Count;
                                                        bool isManyMsgs = countMsgs >= 2;
                                                        if (isManyMsgs)
                                                        {
                                                            bool isNotFirstMsg = msgsCursor >= 1;
                                                            if (isNotFirstMsg)
                                                            {
                                                                int previousMsgIndex = msgsCursor - 1;
                                                                Msg previousMsg = msgs[previousMsgIndex];
                                                                DateTime previousMsgDate = previousMsg.date;
                                                                int previousMsgDateDay = previousMsgDate.DayOfYear;
                                                                int msgDateDay = msgDate.DayOfYear;
                                                                bool isDaysNotCompared = msgDateDay != previousMsgDateDay;
                                                                bool isAddMsgSeparator = isDaysNotCompared;
                                                                if (isAddMsgSeparator)
                                                                {
                                                                    StackPanel msgsSeparator = new StackPanel();
                                                                    msgsSeparator.Orientation = Orientation.Horizontal;
                                                                    msgsSeparator.Margin = new Thickness(10, 25, 10, 25);
                                                                    msgsSeparator.HorizontalAlignment = HorizontalAlignment.Center;
                                                                    Separator leftSeparator = new Separator();
                                                                    leftSeparator.Width = 350;
                                                                    leftSeparator.Margin = new Thickness(25, 0, 25, 0);
                                                                    leftSeparator.BorderBrush = msgsSeparatorBrush;
                                                                    leftSeparator.BorderThickness = new Thickness(2);
                                                                    msgsSeparator.Children.Add(leftSeparator);
                                                                    TextBlock msgsSeparatorDateLabel = new TextBlock();
                                                                    msgsSeparatorDateLabel.Text = rawMsgDate;
                                                                    msgsSeparator.Children.Add(msgsSeparatorDateLabel);
                                                                    Separator rightSeparator = new Separator();
                                                                    rightSeparator.Width = 350;
                                                                    rightSeparator.Margin = new Thickness(25, 0, 25, 0);
                                                                    rightSeparator.BorderBrush = msgsSeparatorBrush;
                                                                    rightSeparator.BorderThickness = new Thickness(2);
                                                                    msgsSeparator.Children.Add(rightSeparator);
                                                                    activeChatContent.Children.Add(msgsSeparator);
                                                                }
                                                            }
                                                        }
                                                        string newMsgType = msg.type;
                                                        string newMsgContent = msg.content;
                                                        string newMsgId = msg._id;
                                                        bool isTextMsg = newMsgType == "text";
                                                        bool isEmojiMsg = newMsgType == "emoji";
                                                        bool isFileMsg = newMsgType == "file";
                                                        if (isTextMsg)
                                                        {
                                                            StackPanel newMsg = new StackPanel();
                                                            StackPanel newMsgHeader = new StackPanel();
                                                            newMsgHeader.Orientation = Orientation.Horizontal;
                                                            Image newMsgHeaderAvatar = new Image();
                                                            newMsgHeaderAvatar.Margin = new Thickness(5, 0, 5, 0);
                                                            newMsgHeaderAvatar.Width = 25;
                                                            newMsgHeaderAvatar.Height = 25;
                                                            newMsgHeaderAvatar.BeginInit();
                                                            Uri newMsgHeaderAvatarUri = new Uri("http://localhost:4000/api/user/avatar/?id=" + newMsgUserId);
                                                            newMsgHeaderAvatar.Source = new BitmapImage(newMsgHeaderAvatarUri);
                                                            newMsgHeaderAvatar.EndInit();
                                                            newMsgHeader.Children.Add(newMsgHeaderAvatar);
                                                            TextBlock newMsgFriendNameLabel = new TextBlock();
                                                            newMsgFriendNameLabel.Margin = new Thickness(5, 0, 5, 0);
                                                            newMsgFriendNameLabel.Text = senderName;
                                                            newMsgHeader.Children.Add(newMsgFriendNameLabel);
                                                            TextBlock newMsgDateLabel = new TextBlock();
                                                            newMsgDateLabel.Margin = new Thickness(5, 0, 5, 0);
                                                            newMsgDateLabel.Text = rawMsgDate;
                                                            newMsgHeader.Children.Add(newMsgDateLabel);
                                                            newMsg.Children.Add(newMsgHeader);
                                                            TextBlock newMsgLabel = new TextBlock();
                                                            newMsgLabel.Margin = new Thickness(40, 10, 10, 10);
                                                            newMsgLabel.Text = newMsgContent;
                                                            inputChatMsgBox.Text = "";
                                                            newMsg.Children.Add(newMsgLabel);
                                                            activeChatContent.Children.Add(newMsg);
                                                        }
                                                        else if (isEmojiMsg)
                                                        {
                                                            StackPanel newMsg = new StackPanel();
                                                            StackPanel newMsgHeader = new StackPanel();
                                                            newMsgHeader.Orientation = Orientation.Horizontal;
                                                            Image newMsgHeaderAvatar = new Image();
                                                            newMsgHeaderAvatar.Margin = new Thickness(5, 0, 5, 0);
                                                            newMsgHeaderAvatar.Width = 25;
                                                            newMsgHeaderAvatar.Height = 25;
                                                            newMsgHeaderAvatar.BeginInit();
                                                            Uri newMsgHeaderAvatarUri = new Uri("http://localhost:4000/api/user/avatar/?id=" + newMsgUserId);
                                                            newMsgHeaderAvatar.Source = new BitmapImage(newMsgHeaderAvatarUri);
                                                            newMsgHeaderAvatar.EndInit();
                                                            newMsgHeader.Children.Add(newMsgHeaderAvatar);
                                                            TextBlock newMsgFriendNameLabel = new TextBlock();
                                                            newMsgFriendNameLabel.Margin = new Thickness(5, 0, 5, 0);
                                                            newMsgFriendNameLabel.Text = friendName;
                                                            newMsgHeader.Children.Add(newMsgFriendNameLabel);
                                                            TextBlock newMsgDateLabel = new TextBlock();
                                                            newMsgDateLabel.Margin = new Thickness(5, 0, 5, 0);
                                                            newMsgDateLabel.Text = rawMsgDate;
                                                            newMsgHeader.Children.Add(newMsgDateLabel);
                                                            newMsg.Children.Add(newMsgHeader);
                                                            Image newMsgLabel = new Image();
                                                            newMsgLabel.Margin = new Thickness(40, 10, 10, 10);
                                                            newMsgLabel.Width = 35;
                                                            newMsgLabel.Height = 35;
                                                            newMsgLabel.HorizontalAlignment = HorizontalAlignment.Left;
                                                            newMsgLabel.BeginInit();
                                                            newMsgLabel.Source = new BitmapImage(new Uri(newMsgContent));
                                                            newMsgLabel.EndInit();
                                                            inputChatMsgBox.Text = "";
                                                            newMsg.Children.Add(newMsgLabel);
                                                            activeChatContent.Children.Add(newMsg);
                                                        }
                                                        else if (isFileMsg)
                                                        {
                                                            StackPanel newMsg = new StackPanel();
                                                            StackPanel newMsgHeader = new StackPanel();
                                                            newMsgHeader.Orientation = Orientation.Horizontal;
                                                            Image newMsgHeaderAvatar = new Image();
                                                            newMsgHeaderAvatar.Margin = new Thickness(5, 0, 5, 0);
                                                            newMsgHeaderAvatar.Width = 25;
                                                            newMsgHeaderAvatar.Height = 25;
                                                            newMsgHeaderAvatar.BeginInit();
                                                            Uri newMsgHeaderAvatarUri = new Uri("http://localhost:4000/api/user/avatar/?id=" + newMsgUserId);
                                                            newMsgHeaderAvatar.Source = new BitmapImage(newMsgHeaderAvatarUri);
                                                            newMsgHeaderAvatar.EndInit();
                                                            newMsgHeader.Children.Add(newMsgHeaderAvatar);
                                                            TextBlock newMsgFriendNameLabel = new TextBlock();
                                                            newMsgFriendNameLabel.Margin = new Thickness(5, 0, 5, 0);
                                                            newMsgFriendNameLabel.Text = friendName;
                                                            newMsgHeader.Children.Add(newMsgFriendNameLabel);
                                                            TextBlock newMsgDateLabel = new TextBlock();
                                                            newMsgDateLabel.Margin = new Thickness(5, 0, 5, 0);
                                                            newMsgDateLabel.Text = rawMsgDate;
                                                            newMsgHeader.Children.Add(newMsgDateLabel);
                                                            newMsg.Children.Add(newMsgHeader);
                                                            Image newMsgLabel = new Image();
                                                            newMsgLabel.Margin = new Thickness(40, 10, 10, 10);
                                                            newMsgLabel.Width = 35;
                                                            newMsgLabel.Height = 35;
                                                            newMsgLabel.HorizontalAlignment = HorizontalAlignment.Left;
                                                            newMsgLabel.BeginInit();
                                                            Uri newMsgLabelUri = new Uri("http://localhost:4000/api/msgs/thumbnail/?id=" + newMsgId + @"&content=" + newMsgContent);
                                                            newMsgLabel.Source = new BitmapImage(newMsgLabelUri);
                                                            newMsgLabel.EndInit();
                                                            inputChatMsgBox.Text = "";
                                                            newMsg.Children.Add(newMsgLabel);
                                                            activeChatContent.Children.Add(newMsg);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                catch (System.Net.WebException)
                                {
                                    MessageBox.Show("Не удается подключиться к серверу", "Ошибка");
                                    this.Close();
                                }
                            }
                        }
                    }
                }
            }
            catch (System.Net.WebException)
            {
                MessageBox.Show("Не удается подключиться к серверу", "Ошибка");
                this.Close();
            }
        }

        public void InitFlash()
        {
            if (isStartBlink)
            {
                FlashWindow(this);
            }
        }

        public void FlashWindow(Window win, UInt32 count = UInt32.MaxValue)
        {
            //Don't flash if the window is active            
            if (win.IsActive) return;
            WindowInteropHelper h = new WindowInteropHelper(win);
            FLASHWINFO info = new FLASHWINFO
            {
                hwnd = h.Handle,
                dwFlags = FLASHW_ALL | FLASHW_TIMER,
                uCount = count,
                dwTimeout = 0
            };

            info.cbSize = Convert.ToUInt32(Marshal.SizeOf(info));
            FlashWindowEx(ref info);
        }


        private void SendMsgHandler(object sender, RoutedEventArgs e)
        {
            string newMsgContent = inputChatMsgBox.Text;
            SendMsg(newMsgContent);
        }

        private void StopBlinkWindowHandler(object sender, RoutedEventArgs e)
        {
            StopBlinkWindow();
        }

        public void StopBlinkWindow()
        {
            StopFlashingWindow(this);
        }

        public void StopFlashingWindow(Window win)
        {
            WindowInteropHelper h = new WindowInteropHelper(win);
            FLASHWINFO info = new FLASHWINFO();
            info.hwnd = h.Handle;
            info.cbSize = Convert.ToUInt32(Marshal.SizeOf(info));
            info.dwFlags = FLASHW_STOP;
            info.uCount = UInt32.MaxValue;
            info.dwTimeout = 0;
            FlashWindowEx(ref info);
        }

        async public void SendMsg(string newMsgContent)
        {

            try
            {
                HttpWebRequest rolesRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/talks/roles/all");
                rolesRequest.Method = "GET";
                rolesRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse rolesResponse = (HttpWebResponse)rolesRequest.GetResponse())
                {
                    using (StreamReader rolesReader = new StreamReader(rolesResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        string objText = rolesReader.ReadToEnd();
                        TalkRolesResponseInfo myRolesObj = (TalkRolesResponseInfo)js.Deserialize(objText, typeof(TalkRolesResponseInfo));
                        string status = myRolesObj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
                            List<Role> talkRoles = myRolesObj.roles;
                            HttpWebRequest roleRelationsWebRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/talks/roles/relations/all");
                            roleRelationsWebRequest.Method = "GET";
                            roleRelationsWebRequest.UserAgent = ".NET Framework Test Client";
                            using (HttpWebResponse roleRelationsWebResponse = (HttpWebResponse)roleRelationsWebRequest.GetResponse())
                            {
                                using (StreamReader roleRelationsReader = new StreamReader(roleRelationsWebResponse.GetResponseStream()))
                                {
                                    js = new JavaScriptSerializer();
                                    objText = roleRelationsReader.ReadToEnd();
                                    TalkRoleRelationsResponseInfo myRoleRelationsObj = (TalkRoleRelationsResponseInfo)js.Deserialize(objText, typeof(TalkRoleRelationsResponseInfo));
                                    status = myRoleRelationsObj.status;
                                    isOkStatus = status == "OK";
                                    if (isOkStatus)
                                    {
                                        List<TalkRoleRelation> talkRoleRelations = myRoleRelationsObj.relations;
                                        List<TalkRoleRelation> currentTalkRoleRelations = talkRoleRelations.Where<TalkRoleRelation>((TalkRoleRelation talkRoleRelation) =>
                                        {
                                            string talkRoleRelationRoleId = talkRoleRelation.role;
                                            string talkRoleRelationTalkId = talkRoleRelation.talk;
                                            string talkRoleRelationUserId = talkRoleRelation.user;
                                            bool isCurrentUser = talkRoleRelationUserId == currentUserId;
                                            bool isCurrentTalk = talkRoleRelationTalkId == talkId;
                                            bool isCurrentTalkRoleRelation = isCurrentUser && isCurrentTalk;
                                            return isCurrentTalkRoleRelation;
                                        }).ToList<TalkRoleRelation>();
                                        List<string> currentTalkRoleRelationsRoles = new List<string>();
                                        foreach (TalkRoleRelation currentTalkRoleRelation in currentTalkRoleRelations)
                                        {
                                            string currentTalkRoleRelationRoleId = currentTalkRoleRelation.role;
                                            currentTalkRoleRelationsRoles.Add(currentTalkRoleRelationRoleId);
                                        }
                                        List<Role> myRoles = talkRoles.Where<Role>((Role talkRole) =>
                                        {
                                            string talkRoleId = talkRole._id;
                                            bool isRoleFound = currentTalkRoleRelationsRoles.Contains(talkRoleId);
                                            bool isCanSendMsgs = talkRole.sendMsgs;
                                            bool isRoleMatch = isRoleFound && isCanSendMsgs;
                                            return isRoleMatch;
                                        }).ToList<Role>();
                                        int myRolesCount = myRoles.Count;
                                        bool isHaveRoles = myRolesCount >= 1;
                                        Talk talk = GetTalkInfo();
                                        string owner = talk.owner;
                                        bool isOwner = owner == currentUserId;
                                        bool isCanSendMsg = isHaveRoles || isOwner;
                                        if (isCanSendMsg)
                                        {

                                            try
                                            {
                                                try
                                                {
                                                    HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/users/get/?id=" + currentUserId);
                                                    webRequest.Method = "GET";
                                                    webRequest.UserAgent = ".NET Framework Test Client";
                                                    using (HttpWebResponse innerWebResponse = (HttpWebResponse)webRequest.GetResponse())
                                                    {
                                                        using (StreamReader innerReader = new StreamReader(innerWebResponse.GetResponseStream()))
                                                        {
                                                            js = new JavaScriptSerializer();
                                                            objText = innerReader.ReadToEnd();
                                                            UserResponseInfo myobj = (UserResponseInfo)js.Deserialize(objText, typeof(UserResponseInfo));
                                                            status = myobj.status;
                                                            isOkStatus = status == "OK";
                                                            if (isOkStatus)
                                                            {
                                                                User friend = myobj.user;
                                                                string friendName = friend.name;
                                                                ItemCollection chatControlItems = chatControl.Items;
                                                                object rawActiveChat = chatControlItems[activeChatIndex];
                                                                TabItem activeChat = ((TabItem)(rawActiveChat));
                                                                object rawActiveChatControlContent = activeChat.Content;
                                                                TabControl activeChatControlContent = ((TabControl)(rawActiveChatControlContent));
                                                                int channelIndex = activeChatControlContent.SelectedIndex;
                                                                ItemCollection activeChatControlContentItems = activeChatControlContent.Items;
                                                                object rawActiveChannel = activeChatControlContentItems[channelIndex];
                                                                TabItem activeChannel = ((TabItem)(rawActiveChannel));
                                                                object rawActiveChatScrollContent = activeChannel.Content;
                                                                ScrollViewer activeChatScrollContent = ((ScrollViewer)(rawActiveChatScrollContent));
                                                                object rawActiveChatContent = activeChatScrollContent.Content;
                                                                StackPanel activeChatContent = ((StackPanel)(rawActiveChatContent));

                                                                StackPanel newMsg = new StackPanel();
                                                                StackPanel newMsgHeader = new StackPanel();
                                                                newMsgHeader.Orientation = Orientation.Horizontal;
                                                                Image newMsgHeaderAvatar = new Image();
                                                                newMsgHeaderAvatar.Margin = new Thickness(5, 0, 5, 0);
                                                                newMsgHeaderAvatar.Width = 25;
                                                                newMsgHeaderAvatar.Height = 25;
                                                                newMsgHeaderAvatar.BeginInit();
                                                                Uri newMsgHeaderAvatarUri = new Uri("http://localhost:4000/api/user/avatar/?id=" + currentUserId);
                                                                newMsgHeaderAvatar.Source = new BitmapImage(newMsgHeaderAvatarUri);
                                                                newMsgHeaderAvatar.EndInit();
                                                                newMsgHeader.Children.Add(newMsgHeaderAvatar);
                                                                TextBlock newMsgFriendNameLabel = new TextBlock();
                                                                newMsgFriendNameLabel.Margin = new Thickness(5, 0, 5, 0);
                                                                newMsgFriendNameLabel.Text = friendName;
                                                                newMsgHeader.Children.Add(newMsgFriendNameLabel);
                                                                TextBlock newMsgDateLabel = new TextBlock();
                                                                newMsgDateLabel.Margin = new Thickness(5, 0, 5, 0);
                                                                DateTime currentDate = DateTime.Now;
                                                                string rawCurrentDate = currentDate.ToLongTimeString();
                                                                newMsgDateLabel.Text = rawCurrentDate;
                                                                newMsgHeader.Children.Add(newMsgDateLabel);
                                                                newMsg.Children.Add(newMsgHeader);
                                                                TextBlock newMsgLabel = new TextBlock();
                                                                newMsgLabel.Margin = new Thickness(40, 10, 10, 10);
                                                                newMsgLabel.Text = newMsgContent;
                                                                inputChatMsgBox.Text = "";
                                                                newMsg.Children.Add(newMsgLabel);
                                                                activeChatContent.Children.Add(newMsg);
                                                            }
                                                        }
                                                    }
                                                }
                                                catch (System.Net.WebException)
                                                {
                                                    MessageBox.Show("Не удается подключиться к серверу", "Ошибка");
                                                    this.Close();
                                                }
                                            }
                                            catch (Exception)
                                            {
                                                Debugger.Log(0, "debug", "поток занят");
                                            }
                                            try
                                            {
                                                string newMsgType = "text";
                                                string newMsgId = "mock";

                                                ItemCollection chatControlItems = chatControl.Items;
                                                object rawActiveChat = chatControlItems[activeChatIndex];
                                                TabItem activeChat = ((TabItem)(rawActiveChat));
                                                object rawActiveChatControlContent = activeChat.Content;
                                                TabControl activeChatControlContent = ((TabControl)(rawActiveChatControlContent));
                                                int channelIndex = activeChatControlContent.SelectedIndex;
                                                ItemCollection activeChatControlContentItems = activeChatControlContent.Items;
                                                object rawActiveChannel = activeChatControlContentItems[channelIndex];
                                                TabItem activeChannel = ((TabItem)(rawActiveChannel));
                                                object rawChannelId = activeChannel.DataContext;
                                                string channelId = ((string)(rawChannelId));

                                                string newMsgChannel = channelId;

                                                await client.EmitAsync("user_send_msg", currentUserId + "|" + newMsgContent + "|" + this.talkId + "|" + newMsgType + "|" + newMsgId + "|" + newMsgChannel);

                                            }
                                            catch (System.Net.WebSockets.WebSocketException)
                                            {
                                                Debugger.Log(0, "debug", "Ошибка сокетов");
                                            }
                                            catch (InvalidOperationException)
                                            {
                                                Debugger.Log(0, "debug", "Нельзя отправить повторно");
                                            }
                                            try
                                            {

                                                ItemCollection chatControlItems = chatControl.Items;
                                                object rawActiveChat = chatControlItems[activeChatIndex];
                                                TabItem activeChat = ((TabItem)(rawActiveChat));

                                                object rawActiveChatControlContent = activeChat.Content;
                                                TabControl activeChatControlContent = ((TabControl)(rawActiveChatControlContent));
                                                int channelIndex = activeChatControlContent.SelectedIndex;
                                                ItemCollection activeChatControlContentItems = activeChatControlContent.Items;
                                                object rawActiveChannel = activeChatControlContentItems[channelIndex];
                                                TabItem activeChannel = ((TabItem)(rawActiveChannel));
                                                object rawChannelId = activeChannel.DataContext;
                                                string channelId = ((string)(rawChannelId));

                                                string newMsgType = "text";
                                                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/msgs/add/?user=" + currentUserId + "&friend=" + talkId + "&content=" + newMsgContent + "&type=" + newMsgType + @"&channel=" + channelId);
                                                webRequest.Method = "GET";
                                                webRequest.UserAgent = ".NET Framework Test Client";
                                                using (HttpWebResponse innerWebResponse = (HttpWebResponse)webRequest.GetResponse())
                                                {
                                                    using (StreamReader innerReader = new StreamReader(innerWebResponse.GetResponseStream()))
                                                    {
                                                        js = new JavaScriptSerializer();
                                                        objText = innerReader.ReadToEnd();
                                                        UserResponseInfo myobj = (UserResponseInfo)js.Deserialize(objText, typeof(UserResponseInfo));
                                                        status = myobj.status;
                                                        isOkStatus = status == "OK";
                                                        if (isOkStatus)
                                                        {

                                                        }
                                                    }
                                                }
                                            }
                                            catch (System.Net.WebException)
                                            {
                                                MessageBox.Show("Не удается подключиться к серверу", "Ошибка");
                                                this.Close();
                                            }

                                        }
                                        else
                                        {
                                            MessageBox.Show("У вас нет разрешения отправлять сообщения. Обратитесь к владельцу беседы.", "Внимание");
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (System.Net.WebException)
            {
                MessageBox.Show("Не удается подключиться к серверу", "Ошибка");
                this.Close();
            }
        }

        private void InputToChatFieldHandler(object sender, TextChangedEventArgs e)
        {
            InputToChatField();
        }

        public void InputToChatField()
        {
            DateTime currentDateTime = DateTime.Now;
            TimeSpan diff = currentDateTime.Subtract(lastInputTimeStamp);
            double diffInSeconds = diff.TotalSeconds;
            bool isNeedEmit = diffInSeconds > 10;
            if (isNeedEmit)
            {
                string eventData = currentUserId + "|" + this.talkId;
                client.EmitAsync("user_write_msg", eventData);
            }
            lastInputTimeStamp = currentDateTime;
        }

        private void AttachFileHandler(object sender, RoutedEventArgs e)
        {
            AttachFile();
        }

        public void AttachFile()
        {
            try
            {
                HttpWebRequest rolesRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/talks/roles/all");
                rolesRequest.Method = "GET";
                rolesRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse rolesResponse = (HttpWebResponse)rolesRequest.GetResponse())
                {
                    using (StreamReader rolesReader = new StreamReader(rolesResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        string objText = rolesReader.ReadToEnd();
                        TalkRolesResponseInfo myRolesObj = (TalkRolesResponseInfo)js.Deserialize(objText, typeof(TalkRolesResponseInfo));
                        string status = myRolesObj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
                            List<Role> talkRoles = myRolesObj.roles;
                            HttpWebRequest roleRelationsWebRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/talks/roles/relations/all");
                            roleRelationsWebRequest.Method = "GET";
                            roleRelationsWebRequest.UserAgent = ".NET Framework Test Client";
                            using (HttpWebResponse roleRelationsWebResponse = (HttpWebResponse)roleRelationsWebRequest.GetResponse())
                            {
                                using (StreamReader roleRelationsReader = new StreamReader(roleRelationsWebResponse.GetResponseStream()))
                                {
                                    js = new JavaScriptSerializer();
                                    objText = roleRelationsReader.ReadToEnd();
                                    TalkRoleRelationsResponseInfo myRoleRelationsObj = (TalkRoleRelationsResponseInfo)js.Deserialize(objText, typeof(TalkRoleRelationsResponseInfo));
                                    status = myRoleRelationsObj.status;
                                    isOkStatus = status == "OK";
                                    if (isOkStatus)
                                    {
                                        List<TalkRoleRelation> talkRoleRelations = myRoleRelationsObj.relations;
                                        List<TalkRoleRelation> currentTalkRoleRelations = talkRoleRelations.Where<TalkRoleRelation>((TalkRoleRelation talkRoleRelation) =>
                                        {
                                            string talkRoleRelationRoleId = talkRoleRelation.role;
                                            string talkRoleRelationTalkId = talkRoleRelation.talk;
                                            string talkRoleRelationUserId = talkRoleRelation.user;
                                            bool isCurrentUser = talkRoleRelationUserId == currentUserId;
                                            bool isCurrentTalk = talkRoleRelationTalkId == talkId;
                                            bool isCurrentTalkRoleRelation = isCurrentUser && isCurrentTalk;
                                            return isCurrentTalkRoleRelation;
                                        }).ToList<TalkRoleRelation>();
                                        List<string> currentTalkRoleRelationsRoles = new List<string>();
                                        foreach (TalkRoleRelation currentTalkRoleRelation in currentTalkRoleRelations)
                                        {
                                            string currentTalkRoleRelationRoleId = currentTalkRoleRelation.role;
                                            currentTalkRoleRelationsRoles.Add(currentTalkRoleRelationRoleId);
                                        }
                                        List<Role> myRoles = talkRoles.Where<Role>((Role talkRole) =>
                                        {
                                            string talkRoleId = talkRole._id;
                                            bool isRoleFound = currentTalkRoleRelationsRoles.Contains(talkRoleId);
                                            bool isCanSendMsgs = talkRole.sendMsgs;
                                            bool isRoleMatch = isRoleFound && isCanSendMsgs;
                                            return isRoleMatch;
                                        }).ToList<Role>();
                                        int myRolesCount = myRoles.Count;
                                        bool isHaveRoles = myRolesCount >= 1;
                                        Talk talk = GetTalkInfo();
                                        string owner = talk.owner;
                                        bool isOwner = owner == currentUserId;
                                        bool isCanSendMsg = isHaveRoles || isOwner;
                                        if (isCanSendMsg)
                                        {
                                            Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
                                            ofd.Title = "Выберите изображение";
                                            ofd.Filter = "BMP|*.bmp|GIF|*.gif|JPG|*.jpg;*.jpeg|PNG|*.png|TIFF|*.tif;*.tiff";
                                            bool? res = ofd.ShowDialog();
                                            bool isOpened = res != false;
                                            if (isOpened)
                                            {
                                                string filePath = ofd.FileName;
                                                SendFileMsg(filePath);
                                            }
                                        }
                                        else
                                        {
                                            MessageBox.Show("У вас нет разрешения отправлять сообщения. Обратитесь к владельцу беседы.", "Внимание");
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (System.Net.WebException)
            {
                MessageBox.Show("Не удается подключиться к серверу", "Ошибка");
                this.Close();
            }
        }

        async public void SendFileMsg(string filePath)
        {
            byte[] rawImage = ImageFileToByteArray(filePath);
            string newMsgContent = "";
            try
            {
                try
                {
                    HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/users/get/?id=" + currentUserId);
                    webRequest.Method = "GET";
                    webRequest.UserAgent = ".NET Framework Test Client";
                    using (HttpWebResponse innerWebResponse = (HttpWebResponse)webRequest.GetResponse())
                    {
                        using (StreamReader innerReader = new StreamReader(innerWebResponse.GetResponseStream()))
                        {
                            JavaScriptSerializer js = new JavaScriptSerializer();
                            string objText = innerReader.ReadToEnd();

                            UserResponseInfo myobj = (UserResponseInfo)js.Deserialize(objText, typeof(UserResponseInfo));

                            string status = myobj.status;
                            bool isOkStatus = status == "OK";
                            if (isOkStatus)
                            {
                                User friend = myobj.user;
                                string friendName = friend.name;
                                ItemCollection chatControlItems = chatControl.Items;
                                object rawActiveChat = chatControlItems[activeChatIndex];
                                TabItem activeChat = ((TabItem)(rawActiveChat));

                                object rawActiveChatControlContent = activeChat.Content;
                                TabControl activeChatControlContent = ((TabControl)(rawActiveChatControlContent));
                                int channelIndex = activeChatControlContent.SelectedIndex;
                                ItemCollection activeChatControlContentItems = activeChatControlContent.Items;
                                object rawActiveChannel = activeChatControlContentItems[channelIndex];
                                TabItem activeChannel = ((TabItem)(rawActiveChannel));
                                object rawActiveChatScrollContent = activeChannel.Content;
                                ScrollViewer activeChatScrollContent = ((ScrollViewer)(rawActiveChatScrollContent));
                                object rawActiveChatContent = activeChatScrollContent.Content;
                                StackPanel activeChatContent = ((StackPanel)(rawActiveChatContent));

                                StackPanel newMsg = new StackPanel();
                                StackPanel newMsgHeader = new StackPanel();
                                newMsgHeader.Orientation = Orientation.Horizontal;
                                Image newMsgHeaderAvatar = new Image();
                                newMsgHeaderAvatar.Margin = new Thickness(5, 0, 5, 0);
                                newMsgHeaderAvatar.Width = 25;
                                newMsgHeaderAvatar.Height = 25;
                                newMsgHeaderAvatar.BeginInit();

                                // Uri newMsgHeaderAvatarUri = new Uri("https://cdn2.iconfinder.com/data/icons/ios-7-icons/50/user_male-128.png");
                                Uri newMsgHeaderAvatarUri = new Uri("http://localhost:4000/api/user/avatar/?id=" + currentUserId);

                                newMsgHeaderAvatar.Source = new BitmapImage(newMsgHeaderAvatarUri);
                                newMsgHeaderAvatar.EndInit();
                                newMsgHeader.Children.Add(newMsgHeaderAvatar);
                                TextBlock newMsgFriendNameLabel = new TextBlock();
                                newMsgFriendNameLabel.Margin = new Thickness(5, 0, 5, 0);
                                newMsgFriendNameLabel.Text = friendName;
                                newMsgHeader.Children.Add(newMsgFriendNameLabel);
                                TextBlock newMsgDateLabel = new TextBlock();
                                newMsgDateLabel.Margin = new Thickness(5, 0, 5, 0);
                                DateTime currentDate = DateTime.Now;
                                string rawCurrentDate = currentDate.ToLongTimeString();
                                newMsgDateLabel.Text = rawCurrentDate;
                                newMsgHeader.Children.Add(newMsgDateLabel);
                                newMsg.Children.Add(newMsgHeader);
                                Image newMsgLabel = new Image();
                                newMsgLabel.Width = 25;
                                newMsgLabel.Height = 25;
                                newMsgLabel.HorizontalAlignment = HorizontalAlignment.Left;
                                newMsgLabel.Margin = new Thickness(40, 10, 10, 10);
                                newMsgLabel.BeginInit();
                                newMsgLabel.Source = new BitmapImage(new Uri(filePath));
                                newMsgLabel.EndInit();
                                newMsg.Children.Add(newMsgLabel);
                                inputChatMsgBox.Text = "";
                                activeChatContent.Children.Add(newMsg);
                            }
                        }
                    }
                }
                catch (System.Net.WebException)
                {
                    MessageBox.Show("Не удается подключиться к серверу", "Ошибка");
                    this.Close();
                }
            }
            catch (Exception)
            {
                Debugger.Log(0, "debug", "поток занят");
            }
            try
            {

                ItemCollection chatControlItems = chatControl.Items;
                object rawActiveChat = chatControlItems[activeChatIndex];
                TabItem activeChat = ((TabItem)(rawActiveChat));

                object rawActiveChatControlContent = activeChat.Content;
                TabControl activeChatControlContent = ((TabControl)(rawActiveChatControlContent));
                int channelIndex = activeChatControlContent.SelectedIndex;
                ItemCollection activeChatControlContentItems = activeChatControlContent.Items;
                object rawActiveChannel = activeChatControlContentItems[channelIndex];
                TabItem activeChannel = ((TabItem)(rawActiveChannel));
                object rawChannelId = activeChannel.DataContext;
                string channelId = ((string)(rawChannelId));

                string newMsgType = "file";
                HttpClient httpClient = new HttpClient();
                MultipartFormDataContent form = new MultipartFormDataContent();
                byte[] imagebytearraystring = ImageFileToByteArray(filePath);
                form.Add(new ByteArrayContent(imagebytearraystring, 0, imagebytearraystring.Count()), "profile_pic", "mock" + System.IO.Path.GetExtension(filePath));
                string url = @"http://localhost:4000/api/msgs/add/?user=" + currentUserId + "&friend=" + talkId + "&content=" + "newMsgContent" + "&type=" + newMsgType + "&id=" + "hash" + "&ext=" + System.IO.Path.GetExtension(filePath) + "&channel=" + channelId;
                HttpResponseMessage response = httpClient.PostAsync(url, form).Result;
                httpClient.Dispose();
                string sd = response.Content.ReadAsStringAsync().Result;
                try
                {
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    MsgResponseInfo myobj = (MsgResponseInfo)js.Deserialize(sd, typeof(MsgResponseInfo));
                    string newMsgId = myobj.id;
                    string ext = myobj.content;
                    Debugger.Log(0, "debug", Environment.NewLine + "id: " + newMsgId + ", ext: " + ext + ", sd: " + sd + Environment.NewLine);

                    string newMsgChannel = channelId;

                    await client.EmitAsync("user_send_msg", currentUserId + "|" + ext + "|" + this.talkId + "|" + newMsgType + "|" + newMsgId + "|" + newMsgChannel);
                }
                catch (System.Net.WebSockets.WebSocketException)
                {
                    Debugger.Log(0, "debug", "Ошибка сокетов");
                }
                catch (InvalidOperationException)
                {
                    Debugger.Log(0, "debug", "Нельзя отправить повторно");
                }

            }
            catch (System.Net.WebException)
            {
                MessageBox.Show("Не удается подключиться к серверу", "Ошибка");
                this.Close();
            }
        }

        private void OpenEmojiPopupHandler(object sender, RoutedEventArgs e)
        {
            OpenEmojiPopup();
        }

        public void OpenEmojiPopup()
        {
            emojiPopup.IsOpen = true;
        }

        private void AddEmojiMsgHandler(object sender, MouseButtonEventArgs e)
        {
            Image emoji = ((Image)(sender));
            object rawEmojiData = emoji.DataContext;
            string emojiData = rawEmojiData.ToString();
            AddEmojiMsg(emojiData);
        }

        async public void AddEmojiMsg (string emojiData)
        {
            try
            {
                HttpWebRequest rolesRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/talks/roles/all");
                rolesRequest.Method = "GET";
                rolesRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse rolesResponse = (HttpWebResponse)rolesRequest.GetResponse())
                {
                    using (StreamReader rolesReader = new StreamReader(rolesResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        string objText = rolesReader.ReadToEnd();
                        TalkRolesResponseInfo myRolesObj = (TalkRolesResponseInfo)js.Deserialize(objText, typeof(TalkRolesResponseInfo));
                        string status = myRolesObj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
                            List<Role> talkRoles = myRolesObj.roles;
                            HttpWebRequest roleRelationsWebRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/talks/roles/relations/all");
                            roleRelationsWebRequest.Method = "GET";
                            roleRelationsWebRequest.UserAgent = ".NET Framework Test Client";
                            using (HttpWebResponse roleRelationsWebResponse = (HttpWebResponse)roleRelationsWebRequest.GetResponse())
                            {
                                using (StreamReader roleRelationsReader = new StreamReader(roleRelationsWebResponse.GetResponseStream()))
                                {
                                    js = new JavaScriptSerializer();
                                    objText = roleRelationsReader.ReadToEnd();
                                    TalkRoleRelationsResponseInfo myRoleRelationsObj = (TalkRoleRelationsResponseInfo)js.Deserialize(objText, typeof(TalkRoleRelationsResponseInfo));
                                    status = myRoleRelationsObj.status;
                                    isOkStatus = status == "OK";
                                    if (isOkStatus)
                                    {
                                        List<TalkRoleRelation> talkRoleRelations = myRoleRelationsObj.relations;
                                        List<TalkRoleRelation> currentTalkRoleRelations = talkRoleRelations.Where<TalkRoleRelation>((TalkRoleRelation talkRoleRelation) =>
                                        {
                                            string talkRoleRelationRoleId = talkRoleRelation.role;
                                            string talkRoleRelationTalkId = talkRoleRelation.talk;
                                            string talkRoleRelationUserId = talkRoleRelation.user;
                                            bool isCurrentUser = talkRoleRelationUserId == currentUserId;
                                            bool isCurrentTalk = talkRoleRelationTalkId == talkId;
                                            bool isCurrentTalkRoleRelation = isCurrentUser && isCurrentTalk;
                                            return isCurrentTalkRoleRelation;
                                        }).ToList<TalkRoleRelation>();
                                        List<string> currentTalkRoleRelationsRoles = new List<string>();
                                        foreach (TalkRoleRelation currentTalkRoleRelation in currentTalkRoleRelations)
                                        {
                                            string currentTalkRoleRelationRoleId = currentTalkRoleRelation.role;
                                            currentTalkRoleRelationsRoles.Add(currentTalkRoleRelationRoleId);
                                        }
                                        List<Role> myRoles = talkRoles.Where<Role>((Role talkRole) =>
                                        {
                                            string talkRoleId = talkRole._id;
                                            bool isRoleFound = currentTalkRoleRelationsRoles.Contains(talkRoleId);
                                            bool isCanSendMsgs = talkRole.sendMsgs;
                                            bool isRoleMatch = isRoleFound && isCanSendMsgs;
                                            return isRoleMatch;
                                        }).ToList<Role>();
                                        int myRolesCount = myRoles.Count;
                                        bool isHaveRoles = myRolesCount >= 1;
                                        Talk talk = GetTalkInfo();
                                        string owner = talk.owner;
                                        bool isOwner = owner == currentUserId;
                                        bool isCanSendMsg = isHaveRoles || isOwner;
                                        if (isCanSendMsg)
                                        {
                                            try
                                            {
                                                try
                                                {
                                                    HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/users/get/?id=" + currentUserId);
                                                    webRequest.Method = "GET";
                                                    webRequest.UserAgent = ".NET Framework Test Client";
                                                    using (HttpWebResponse innerWebResponse = (HttpWebResponse)webRequest.GetResponse())
                                                    {
                                                        using (StreamReader innerReader = new StreamReader(innerWebResponse.GetResponseStream()))
                                                        {
                                                            js = new JavaScriptSerializer();
                                                            objText = innerReader.ReadToEnd();
                                                            UserResponseInfo myobj = (UserResponseInfo)js.Deserialize(objText, typeof(UserResponseInfo));
                                                            status = myobj.status;
                                                            isOkStatus = status == "OK";
                                                            if (isOkStatus)
                                                            {
                                                                User friend = myobj.user;
                                                                string friendName = friend.name;
                                                                ItemCollection chatControlItems = chatControl.Items;
                                                                object rawActiveChat = chatControlItems[activeChatIndex];
                                                                TabItem activeChat = ((TabItem)(rawActiveChat));


                                                                object rawActiveChatControlContent = activeChat.Content;
                                                                TabControl activeChatControlContent = ((TabControl)(rawActiveChatControlContent));
                                                                int channelIndex = activeChatControlContent.SelectedIndex;
                                                                ItemCollection activeChatControlContentItems = activeChatControlContent.Items;
                                                                object rawActiveChannel = activeChatControlContentItems[channelIndex];
                                                                TabItem activeChannel = ((TabItem)(rawActiveChannel));
                                                                object rawActiveChatScrollContent = activeChannel.Content;
                                                                ScrollViewer activeChatScrollContent = ((ScrollViewer)(rawActiveChatScrollContent));
                                                                object rawActiveChatContent = activeChatScrollContent.Content;
                                                                StackPanel activeChatContent = ((StackPanel)(rawActiveChatContent));

                                                                StackPanel newMsg = new StackPanel();
                                                                StackPanel newMsgHeader = new StackPanel();
                                                                newMsgHeader.Orientation = Orientation.Horizontal;
                                                                Image newMsgHeaderAvatar = new Image();
                                                                newMsgHeaderAvatar.Margin = new Thickness(5, 0, 5, 0);
                                                                newMsgHeaderAvatar.Width = 25;
                                                                newMsgHeaderAvatar.Height = 25;
                                                                newMsgHeaderAvatar.BeginInit();
                                                                Uri newMsgHeaderAvatarUri = new Uri("http://localhost:4000/api/user/avatar/?id=" + currentUserId);
                                                                newMsgHeaderAvatar.Source = new BitmapImage(newMsgHeaderAvatarUri);
                                                                newMsgHeaderAvatar.EndInit();
                                                                newMsgHeader.Children.Add(newMsgHeaderAvatar);
                                                                TextBlock newMsgFriendNameLabel = new TextBlock();
                                                                newMsgFriendNameLabel.Margin = new Thickness(5, 0, 5, 0);
                                                                newMsgFriendNameLabel.Text = friendName;
                                                                newMsgHeader.Children.Add(newMsgFriendNameLabel);
                                                                TextBlock newMsgDateLabel = new TextBlock();
                                                                newMsgDateLabel.Margin = new Thickness(5, 0, 5, 0);
                                                                DateTime currentDate = DateTime.Now;
                                                                string rawCurrentDate = currentDate.ToLongTimeString();
                                                                newMsgDateLabel.Text = rawCurrentDate;
                                                                newMsgHeader.Children.Add(newMsgDateLabel);
                                                                newMsg.Children.Add(newMsgHeader);
                                                                Image newMsgLabel = new Image();
                                                                newMsgLabel.Margin = new Thickness(40, 10, 10, 10);
                                                                newMsgLabel.Width = 35;
                                                                newMsgLabel.Height = 35;
                                                                newMsgLabel.HorizontalAlignment = HorizontalAlignment.Left;
                                                                newMsgLabel.BeginInit();
                                                                newMsgLabel.Source = new BitmapImage(new Uri(emojiData));
                                                                newMsgLabel.EndInit();
                                                                inputChatMsgBox.Text = "";
                                                                newMsg.Children.Add(newMsgLabel);
                                                                activeChatContent.Children.Add(newMsg);
                                                                emojiPopup.IsOpen = false;
                                                            }
                                                        }
                                                    }
                                                }
                                                catch (System.Net.WebException)
                                                {
                                                    MessageBox.Show("Не удается подключиться к серверу", "Ошибка");
                                                    this.Close();
                                                }
                                            }
                                            catch (Exception)
                                            {
                                                Debugger.Log(0, "debug", "поток занят");
                                            }
                                            try
                                            {
                                                string newMsgType = "emoji";
                                                string newMsgId = "mock";

                                                ItemCollection chatControlItems = chatControl.Items;
                                                object rawActiveChat = chatControlItems[activeChatIndex];
                                                TabItem activeChat = ((TabItem)(rawActiveChat));
                                                object rawActiveChatControlContent = activeChat.Content;
                                                TabControl activeChatControlContent = ((TabControl)(rawActiveChatControlContent));
                                                int channelIndex = activeChatControlContent.SelectedIndex;
                                                ItemCollection activeChatControlContentItems = activeChatControlContent.Items;
                                                object rawActiveChannel = activeChatControlContentItems[channelIndex];
                                                TabItem activeChannel = ((TabItem)(rawActiveChannel));
                                                object rawChannelId = activeChannel.DataContext;
                                                string channelId = ((string)(rawChannelId));

                                                string newMsgChannel = channelId;

                                                await client.EmitAsync("user_send_msg", currentUserId + "|" + emojiData + "|" + this.talkId + "|" + newMsgType + "|" + newMsgId + "|" + newMsgChannel);
                                            }
                                            catch (System.Net.WebSockets.WebSocketException)
                                            {
                                                Debugger.Log(0, "debug", "Ошибка сокетов");
                                            }
                                            catch (InvalidOperationException)
                                            {
                                                Debugger.Log(0, "debug", "Нельзя отправить повторно");
                                            }
                                            try
                                            {
                                                string newMsgType = "emoji";

                                                ItemCollection chatControlItems = chatControl.Items;
                                                object rawActiveChat = chatControlItems[activeChatIndex];
                                                TabItem activeChat = ((TabItem)(rawActiveChat));

                                                object rawActiveChatControlContent = activeChat.Content;
                                                TabControl activeChatControlContent = ((TabControl)(rawActiveChatControlContent));
                                                int channelIndex = activeChatControlContent.SelectedIndex;
                                                ItemCollection activeChatControlContentItems = activeChatControlContent.Items;
                                                object rawActiveChannel = activeChatControlContentItems[channelIndex];
                                                TabItem activeChannel = ((TabItem)(rawActiveChannel));
                                                object rawChannelId = activeChannel.DataContext;
                                                string channelId = ((string)(rawChannelId));

                                                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/msgs/add/?user=" + currentUserId + "&friend=" + talkId + "&content=" + emojiData + "&type=" + newMsgType + "&channel=" + channelId);
                                                webRequest.Method = "GET";
                                                webRequest.UserAgent = ".NET Framework Test Client";
                                                using (HttpWebResponse innerWebResponse = (HttpWebResponse)webRequest.GetResponse())
                                                {
                                                    using (StreamReader innerReader = new StreamReader(innerWebResponse.GetResponseStream()))
                                                    {
                                                        js = new JavaScriptSerializer();
                                                        objText = innerReader.ReadToEnd();
                                                        UserResponseInfo myobj = (UserResponseInfo)js.Deserialize(objText, typeof(UserResponseInfo));
                                                        status = myobj.status;
                                                        isOkStatus = status == "OK";
                                                        {
                                                            if (isOkStatus)
                                                            {

                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            catch (System.Net.WebException)
                                            {
                                                MessageBox.Show("Не удается подключиться к серверу", "Ошибка");
                                                this.Close();
                                            }
                                        }
                                        else
                                        {
                                            MessageBox.Show("У вас нет разрешения отправлять сообщения. Обратитесь к владельцу беседы.", "Внимание");
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (System.Net.WebException)
            {
                MessageBox.Show("Не удается подключиться к серверу", "Ошибка");
                this.Close();
            }
        }

        public byte[] ImageFileToByteArray(string fullFilePath)
        {
            FileStream fs = File.OpenRead(fullFilePath);
            byte[] bytes = new byte[fs.Length];
            fs.Read(bytes, 0, Convert.ToInt32(fs.Length));
            fs.Close();
            return bytes;
        }

        private void InviteFriendsToTalkHandler (object sender, MouseButtonEventArgs e)
        {
            InviteFriendsToTalk();
        }

        public void InviteFriendsToTalk ()
        {
            Dialogs.InviteTalkDialog dialog = new Dialogs.InviteTalkDialog(currentUserId, talkId);
            dialog.Show();
        }

        private Talk GetTalkInfo()
        {
            Talk talk = null;
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/talks/get/?id=" + talkId);
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (var reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        var objText = reader.ReadToEnd();
                        TalkResponseInfo myobj = (TalkResponseInfo)js.Deserialize(objText, typeof(TalkResponseInfo));
                        string status = myobj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
                            talk = myobj.talk;
                        }
                    }
                }
            }
            catch (System.Net.WebException)
            {
                MessageBox.Show("Не удается подключиться к серверу", "Ошибка");
                this.Close();
            }
            return talk;
        }

        private void OpenTalkSettingsHandler(object sender, MouseButtonEventArgs e)
        {
            OpenTalkSettings();
        }

        private void OpenTalkNotificationsHandler(object sender, RoutedEventArgs e)
        {
            OpenTalkNotifications();
        }


        private void OpenTalkNotifications()
        {
            Dialogs.TalkNotificationsDialog dialog = new Dialogs.TalkNotificationsDialog(currentUserId, talkId);
            dialog.Show();
        }

        private void OpenTalkSettingsHandler(object sender, RoutedEventArgs e)
        {
            OpenTalkSettings();
        }

        private void OpenTalkSettings ()
        {
            Dialogs.TalkSettingsDialog dialog = new Dialogs.TalkSettingsDialog(currentUserId, talkId);
            dialog.Closed += GetTextChannelsHandler;
            dialog.Show();
        }

        private void SetDefaultAvatarHandler(object sender, ExceptionRoutedEventArgs e)
        {
            Image avatar = ((Image)(sender));
            SetDefaultAvatar(avatar);
        }


        public void SetDefaultAvatar(Image avatar)
        {
            avatar.BeginInit();
            avatar.Source = new BitmapImage(new Uri(@"https://cdn2.iconfinder.com/data/icons/ios-7-icons/50/user_male-128.png"));
            avatar.EndInit();
        }

        private void GetUsersHandler(object sender, TextChangedEventArgs e)
        {
            GetUsers();
        }

        public void GetUsers ()
        {
            users.Children.Clear();
            string keywords = usersBox.Text;
            string insensitiveCaseKeywords = keywords.ToLower();
            int insensitiveCaseKeywordsLength = insensitiveCaseKeywords.Length;
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/talks/relations/all");
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (var reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        string objText = reader.ReadToEnd();
                        TalkRelationsResponseInfo myObj = (TalkRelationsResponseInfo)js.Deserialize(objText, typeof(TalkRelationsResponseInfo));
                        string status = myObj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
                            List<TalkRelation> relations = myObj.relations;
                            List<TalkRelation> currentTalkUsers = relations.Where<TalkRelation>((TalkRelation relation) =>
                            {
                                string localTalkId = relation.talk;
                                bool isCurrentTalk = talkId == localTalkId;
                                return isCurrentTalk;
                            }).ToList<TalkRelation>();
                            foreach (TalkRelation currentTalkUser in currentTalkUsers)
                            {
                                string userId = currentTalkUser.user;
                                HttpWebRequest innerWebRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/users/get/?id=" + userId);
                                innerWebRequest.Method = "GET";
                                innerWebRequest.UserAgent = ".NET Framework Test Client";
                                using (HttpWebResponse innerWebResponse = (HttpWebResponse)innerWebRequest.GetResponse())
                                {
                                    using (StreamReader innerReader = new StreamReader(innerWebResponse.GetResponseStream()))
                                    {
                                        js = new JavaScriptSerializer();
                                        objText = innerReader.ReadToEnd();
                                        UserResponseInfo myInnerObj = (UserResponseInfo)js.Deserialize(objText, typeof(UserResponseInfo));
                                        status = myInnerObj.status;
                                        isOkStatus = status == "OK";
                                        if (isOkStatus)
                                        {
                                            User user = myInnerObj.user;
                                            string userName = user.name;
                                            bool isKeywordsMatch = userName.Contains(insensitiveCaseKeywords);
                                            bool isFilterDisabled = insensitiveCaseKeywordsLength <= 0;
                                            bool isAddFriend = isFilterDisabled || isKeywordsMatch;
                                            if (isAddFriend)
                                            {
                                                string userStatus = user.status;
                                                StackPanel talkUser = new StackPanel();
                                                talkUser.Orientation = Orientation.Horizontal;
                                                talkUser.Height = 50;
                                                Image talkUserAvatar = new Image();
                                                talkUserAvatar.Width = 35;
                                                talkUserAvatar.Height = 35;
                                                talkUserAvatar.Margin = new Thickness(10);
                                                talkUserAvatar.BeginInit();
                                                talkUserAvatar.Source = new BitmapImage(new Uri(@"http://localhost:4000/api/user/avatar/?id=" + userId));
                                                talkUserAvatar.EndInit();
                                                talkUserAvatar.ImageFailed += SetDefaultAvatarHandler;
                                                talkUser.Children.Add(talkUserAvatar);
                                                StackPanel talkUserAside = new StackPanel();
                                                TextBlock talkUserNameLabel = new TextBlock();
                                                talkUserNameLabel.FontSize = 14;
                                                talkUserNameLabel.Margin = new Thickness(0, 5, 0, 5);
                                                talkUserNameLabel.Text = userName;
                                                talkUserAside.Children.Add(talkUserNameLabel);
                                                TextBlock talkUserStatusLabel = new TextBlock();
                                                talkUserStatusLabel.Margin = new Thickness(0, 5, 0, 5);
                                                talkUserStatusLabel.Text = userStatus;
                                                talkUserAside.Children.Add(talkUserStatusLabel);
                                                talkUser.Children.Add(talkUserAside);
                                                users.Children.Add(talkUser);
                                            }
                                        }
                                    }
                                }
                            }
                            UIElementCollection filteredUsers = users.Children;
                            int countUsers = filteredUsers.Count;
                            string rawCountUsers = countUsers.ToString();
                            filteredUsersCountLabel.Text = rawCountUsers;
                        }
                    }
                }
            }
            catch (System.Net.WebException)
            {
                MessageBox.Show("Не удается подключиться к серверу", "Ошибка");
                this.Close();
            }

        }

        private void CreateTextChannelHandler (object sender, MouseButtonEventArgs e)
        {
            CreateTextChannel();
        }

        public void CreateTextChannel ()
        {
            Dialogs.CreateTextChannelDialog dialog = new Dialogs.CreateTextChannelDialog(talkId);
            dialog.Closed += GetTextChannelsHandler;
            dialog.Show();
        }

        public void GetTextChannelsHandler (object sender, EventArgs e)
        {
            GetTextChannels();
        }

        public void GetTextChannels()
        {
            textChannels.Children.Clear();
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/talks/channels/all");
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (var reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        var objText = reader.ReadToEnd();
                        TalkChannelsResponseInfo myobj = (TalkChannelsResponseInfo)js.Deserialize(objText, typeof(TalkChannelsResponseInfo));
                        string status = myobj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
                            List<TalkChannel> totalChannels = myobj.channels;
                            int channelIndex = -1;
                            foreach (TalkChannel channel in totalChannels)
                            {
                                string channelTalkId = channel.talk;
                                string channelId = channel._id;
                                string channelTitle = channel.title;
                                bool isCurrentTalkChannel = channelTalkId == talkId;
                                if (isCurrentTalkChannel)
                                {
                                    channelIndex++;
                                    StackPanel channelsItem = new StackPanel();
                                    channelsItem.Orientation = Orientation.Horizontal;
                                    channelsItem.Margin = new Thickness(20, 5, 20, 5);
                                    PackIcon channelsItemIcon = new PackIcon();
                                    channelsItemIcon.Margin = new Thickness(15, 0, 15, 0);
                                    channelsItemIcon.Kind = PackIconKind.Menu;
                                    channelsItemIcon.HorizontalAlignment = HorizontalAlignment.Right;
                                    channelsItemIcon.VerticalAlignment = VerticalAlignment.Center;
                                    channelsItem.Children.Add(channelsItemIcon);
                                    TextBlock channelsItemTitleLabel = new TextBlock();
                                    channelsItemTitleLabel.Margin = new Thickness(15, 0, 15, 0);
                                    channelsItemTitleLabel.Text = channelTitle;
                                    channelsItem.Children.Add(channelsItemTitleLabel);
                                    textChannels.Children.Add(channelsItem);
                                    channelsItem.DataContext = channelIndex;
                                    channelsItem.MouseLeftButtonUp += SelectTextChannelHandler;
                                }
                            }
                        }
                    }
                }
            }
            catch (System.Net.WebException)
            {
                MessageBox.Show("Не удается подключиться к серверу", "Ошибка");
                this.Close();
            }
        }

        public void SelectTextChannelHandler (object sender, RoutedEventArgs e)
        {
            StackPanel channel = ((StackPanel)(sender));
            object channelData = channel.DataContext;
            int channelIndex = ((int)(channelData));
            SelectTextChannel(channelIndex);
        }

        public void SelectTextChannel (int channelIndex)
        {
            ItemCollection chatControlItems = chatControl.Items;
            object rawActiveChat = chatControlItems[activeChatIndex];
            TabItem activeChat = ((TabItem)(rawActiveChat));
            object rawActiveChatControlContent = activeChat.Content;
            TabControl activeChatControlContent = ((TabControl)(rawActiveChatControlContent));
            activeChatControlContent.SelectedIndex = channelIndex;
        }


    }
}
