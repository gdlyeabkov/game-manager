using AVSPEED;
using NAudio.Wave;
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
    /// Логика взаимодействия для ChatDialog.xaml
    /// </summary>
    public partial class ChatDialog : Window
    {

        public int activeChatIndex = -1;
        public string currentUserId;
        public SocketIO client;
        public string friendId;
        public bool isStartBlink;
        public DateTime lastInputTimeStamp;
        public bool isStartRing = false;
        public WaveIn waveSource;
        public WaveFileWriter waveFile;
        // public RTCControl rtcControl = null;

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
        public ChatDialog(string currentUserId, SocketIO client, string friendId, bool isStartBlink)
        {
            InitializeComponent();
            this.currentUserId = currentUserId;
            this.friendId = friendId;
            this.isStartBlink = isStartBlink; 
        }

        async public void ReceiveMessages ()
        {
            try
            {
                // client = new SocketIO("https://loud-reminiscent-jackrabbit.glitch.me/");
                client = new SocketIO("http://localhost:4000/");
                await client.ConnectAsync();
                client.On("friend_send_msg", async response =>
                {
                    var rawResult = response.GetValue<string>();
                    string[] result = rawResult.Split(new char[] { '|' });
                    string userId = result[0];
                    string msg = result[1];
                    Debugger.Log(0, "debug", Environment.NewLine + "user " + userId + " send msg: " + msg + Environment.NewLine);
                    try
                    {
                        HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("https://loud-reminiscent-jackrabbit.glitch.me/api/friends/get");
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
                                        string currentFriendId = this.friendId;
                                        bool isCurrentChat = currentFriendId == userId;
                                        if (isCurrentChat)
                                        {
                                            this.Dispatcher.Invoke(() =>
                                            {
                                                try
                                                {
                                                    HttpWebRequest innerWebRequest = (HttpWebRequest)HttpWebRequest.Create("https://loud-reminiscent-jackrabbit.glitch.me/api/users/get/?id=" + friendId);
                                                    innerWebRequest.Method = "GET";
                                                    innerWebRequest.UserAgent = ".NET Framework Test Client";
                                                    using (HttpWebResponse innerWebResponse = (HttpWebResponse)innerWebRequest.GetResponse())
                                                    {
                                                        using (StreamReader innerReader = new StreamReader(innerWebResponse.GetResponseStream()))
                                                        {
                                                            js = new JavaScriptSerializer();
                                                            objText = innerReader.ReadToEnd();

                                                            UserResponseInfo myInnerObj = (UserResponseInfo)js.Deserialize(objText, typeof(UserResponseInfo));

                                                            status = myobj.status;
                                                            isOkStatus = status == "OK";
                                                            if (isOkStatus)
                                                            {
                                                                User friend = myInnerObj.user;
                                                                string friendName = friend.name;

                                                                ItemCollection chatControlItems = chatControl.Items;
                                                                object rawActiveChat = chatControlItems[activeChatIndex];
                                                                TabItem activeChat = ((TabItem)(rawActiveChat));
                                                                object rawActiveChatScrollContent = activeChat.Content;
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
                                                                Uri newMsgHeaderAvatarUri = new Uri("https://cdn2.iconfinder.com/data/icons/ios-7-icons/50/user_male-128.png");
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
                                                                string newMsgContent = msg;
                                                                newMsgLabel.Text = newMsgContent;
                                                                newMsgLabel.Margin = new Thickness(40, 10, 10, 10);
                                                                inputChatMsgBox.Text = "";
                                                                newMsg.Children.Add(newMsgLabel);

                                                                activeChatContent.Children.Add(newMsg);

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
                client.On("friend_write_msg", async response =>
                {
                    var rawResult = response.GetValue<string>();
                    string[] result = rawResult.Split(new char[] { '|' });
                    string userId = result[0];
                    string msg = result[1];
                    Debugger.Log(0, "debug", Environment.NewLine + "user " + userId + " send msg: " + msg + Environment.NewLine);
                    try
                    {
                        HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("https://loud-reminiscent-jackrabbit.glitch.me/api/friends/get");
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
                                        string currentFriendId = this.friendId;
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

        public void InitializeHandler (object sender, RoutedEventArgs e)
        {
            Initialize();
        }

        public void Initialize()
        {
            lastInputTimeStamp = DateTime.Now;
            AddChat();
            ReceiveMessages();
            GetMsgs();
            if (isStartBlink)
            {
                FlashWindow(this);
            }
        }

        public void AddChat()
        {
            TabItem newChat = new TabItem();

            newChat.Header = friendId;
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("https://loud-reminiscent-jackrabbit.glitch.me/api/users/get/?id=" + friendId);
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse innerWebResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (StreamReader innerReader = new StreamReader(innerWebResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        string objText = innerReader.ReadToEnd();

                        var myobj = (UserResponseInfo)js.Deserialize(objText, typeof(UserResponseInfo));

                        string status = myobj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
                            User friend = myobj.user;
                            string friendName = friend.name;
                            newChat.Header = friendName;

                            string userIsWritingLabelContent = friendName + " печатает...";
                            userIsWritingLabel.Text = userIsWritingLabelContent;

                        }
                    }
                }
            }
            catch (System.Net.WebException)
            {
                MessageBox.Show("Не удается подключиться к серверу", "Ошибка");
                this.Close();
            }

            chatControl.Items.Add(newChat);
            ScrollViewer newChatScrollContent = new ScrollViewer();
            StackPanel newChatContent = new StackPanel();
            newChatScrollContent.Content = newChatContent;
            newChat.Content = newChatScrollContent;
            activeChatIndex++;
            chatControl.SelectedIndex = activeChatIndex;
        }

        private void SendMsgHandler (object sender, RoutedEventArgs e)
        {
            string newMsgContent = inputChatMsgBox.Text;
            SendMsg(newMsgContent);
        }

        public void GetMsgs ()
        {
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("https://loud-reminiscent-jackrabbit.glitch.me/api/users/get/?id=" + currentUserId);
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
                                    HttpWebRequest innerWebRequest = (HttpWebRequest)HttpWebRequest.Create("https://loud-reminiscent-jackrabbit.glitch.me/api/msgs/get");
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
                                                foreach (Msg msg in msgs)
                                                {
                                                    string newMsgUserId = msg.user;
                                                    string newMsgFriendId = msg.friend;
                                                    bool isCurrentChatMsg = (newMsgUserId == currentUserId && newMsgFriendId == friendId) || (newMsgUserId == friendId && newMsgFriendId == currentUserId);
                                                    if (isCurrentChatMsg)
                                                    {
                                                        string newMsgType = msg.type;
                                                        string newMsgContent = msg.content;
                                                        string newMsgId = msg._id;
                                                        bool isTextMsg = newMsgType == "text";
                                                        bool isEmojiMsg = newMsgType == "emoji";
                                                        bool isFileMsg = newMsgType == "file";
                                                        if (isTextMsg)
                                                        {
                                                            User friend = myobj.user;
                                                            string friendName = friend.name;
                                                            ItemCollection chatControlItems = chatControl.Items;
                                                            object rawActiveChat = chatControlItems[activeChatIndex];
                                                            TabItem activeChat = ((TabItem)(rawActiveChat));
                                                            object rawActiveChatScrollContent = activeChat.Content;
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
                                                            Uri newMsgHeaderAvatarUri = new Uri("https://cdn2.iconfinder.com/data/icons/ios-7-icons/50/user_male-128.png");
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
                                                        else if (isEmojiMsg)
                                                        {
                                                            User friend = myobj.user;
                                                            string friendName = friend.name;
                                                            ItemCollection chatControlItems = chatControl.Items;
                                                            object rawActiveChat = chatControlItems[activeChatIndex];
                                                            TabItem activeChat = ((TabItem)(rawActiveChat));
                                                            object rawActiveChatScrollContent = activeChat.Content;
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
                                                            Uri newMsgHeaderAvatarUri = new Uri("https://cdn2.iconfinder.com/data/icons/ios-7-icons/50/user_male-128.png");
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
                                                            newMsgLabel.Source = new BitmapImage(new Uri(newMsgContent));
                                                            newMsgLabel.EndInit();
                                                            inputChatMsgBox.Text = "";
                                                            newMsg.Children.Add(newMsgLabel);
                                                            activeChatContent.Children.Add(newMsg);
                                                        }
                                                        else if (isFileMsg)
                                                        {
                                                            User friend = myobj.user;
                                                            string friendName = friend.name;
                                                            ItemCollection chatControlItems = chatControl.Items;
                                                            object rawActiveChat = chatControlItems[activeChatIndex];
                                                            TabItem activeChat = ((TabItem)(rawActiveChat));
                                                            object rawActiveChatScrollContent = activeChat.Content;
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
                                                            Uri newMsgHeaderAvatarUri = new Uri("https://cdn2.iconfinder.com/data/icons/ios-7-icons/50/user_male-128.png");
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
                                                            /*List<Byte> newMsgContentItems = new List<byte>();
                                                            foreach (string rawNewMsgContentItem in newMsgContent.Split(new char[] { '|' }))
                                                            {
                                                                byte newMsgContentItem = Byte.Parse(rawNewMsgContentItem);
                                                                newMsgContentItems.Add(newMsgContentItem);
                                                            }
                                                            newMsgLabel.Source = LoadImage(newMsgContentItems.ToArray());*/
                                                            // Uri newMsgLabelUri = new Uri("https://cdn2.iconfinder.com/data/icons/ios-7-icons/50/user_male-128.png");
                                                            // Uri newMsgLabelUri = new Uri("http://localhost:4000/api/msgs/thumbnail/?id=" + newMsgContent);
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

        private BitmapImage LoadImage(byte[] imageData)
        {
            if (imageData == null || imageData.Length == 0) return null;
            var image = new BitmapImage();
            using (var mem = new MemoryStream(imageData))
            {
                mem.Position = 0;
                image.BeginInit();
                image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = null;
                image.StreamSource = mem;
                image.EndInit();
            }
            image.Freeze();
            return image;
        }

        async public void SendMsg (string newMsgContent)
        {
            try
            {
                try
                {
                    HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("https://loud-reminiscent-jackrabbit.glitch.me/api/users/get/?id=" + currentUserId);
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
                                object rawActiveChatScrollContent = activeChat.Content;
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
                                Uri newMsgHeaderAvatarUri = new Uri("https://cdn2.iconfinder.com/data/icons/ios-7-icons/50/user_male-128.png");
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
                await client.EmitAsync("user_send_msg", currentUserId + "|" + newMsgContent + "|" + this.friendId);

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
                string newMsgType = "text";
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("https://loud-reminiscent-jackrabbit.glitch.me/api/msgs/add/?user=" + currentUserId + "&friend=" + friendId + "&content=" + newMsgContent + "&type=" + newMsgType);
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

        private void StopBlinkWindowHandler (object sender, RoutedEventArgs e)
        {
            StopBlinkWindow();
        }

        public void StopBlinkWindow()
        {
            StopFlashingWindow(this);
        }

        private void InputToChatFieldHandler (object sender, TextChangedEventArgs e)
        {
            InputToChatField();
        }

        public void InputToChatField ()
        {
            DateTime currentDateTime = DateTime.Now;
            TimeSpan diff = currentDateTime.Subtract(lastInputTimeStamp);
            double diffInSeconds = diff.TotalSeconds;
            bool isNeedEmit = diffInSeconds > 10;
            if (isNeedEmit)
            {
                string eventData = currentUserId + "|" + this.friendId;
                client.EmitAsync("user_write_msg", eventData);
            }
            lastInputTimeStamp = currentDateTime;
        }

        private void ToggleRingHandler (object sender, RoutedEventArgs e)
        {
            Button btn = ((Button)(sender));
            ToggleRing(btn);
        }

        async public void ToggleRing (Button btn)
        {
            isStartRing = !isStartRing;
            if (isStartRing)
            {
                waveSource = new WaveIn();
                waveSource.WaveFormat = new WaveFormat(44100, 1);
                waveSource.DataAvailable += new EventHandler<WaveInEventArgs>(MicroDataAvailableHandler);
                waveSource.RecordingStopped += new EventHandler<StoppedEventArgs>(MicroRecordingStoppedHandler);

                Environment.SpecialFolder localApplicationDataFolder = Environment.SpecialFolder.LocalApplicationData;
                string localApplicationDataFolderPath = Environment.GetFolderPath(localApplicationDataFolder);
                string tempRecordFilePath = localApplicationDataFolderPath + @"\OfficeWare\GameManager\" + currentUserId + @"\record.wav";

                waveFile = new WaveFileWriter(tempRecordFilePath, waveSource.WaveFormat);
                waveSource.StartRecording();

                btn.Content = "Остановить голосовой чат";

            }
            else
            {

                waveSource.StopRecording();
            
                btn.Content = "Начать голосовой чат";
            
            }
        }

        public void MicroDataAvailableHandler (object sender, WaveInEventArgs e)
        {
            byte[] buffer = e.Buffer;
            int recordedBytes = e.BytesRecorded;
            MicroDataAvailable(buffer, recordedBytes);
        }

        public void MicroDataAvailable (byte[] buffer, int recordedBytes)
        {
            if (waveFile != null)
            {
                waveFile.Write(buffer, 0, recordedBytes);
                waveFile.Flush();

                /*string rawBuffer = "";
                foreach (byte someByte in buffer) {
                    string rawByte = someByte.ToString();
                    rawBuffer += rawByte;
                }
                string rawRecordedBytes = recordedBytes.ToString();
                string eventData = currentUserId + "|" + this.friendId + "|" + rawBuffer + "|" + rawRecordedBytes;
                client.EmitAsync("user_is_speak", eventData);*/

            }
        }

        public void MicroRecordingStoppedHandler (object sender, StoppedEventArgs e)
        {
            MicroRecordingStopped();
        }

        public void MicroRecordingStopped ()
        {
            if (waveSource != null)
            {
                waveSource.Dispose();
                waveSource = null;
            }

            if (waveFile != null)
            {
                waveFile.Dispose();
                waveFile = null;
            }

            Environment.SpecialFolder localApplicationDataFolder = Environment.SpecialFolder.LocalApplicationData;
            string localApplicationDataFolderPath = Environment.GetFolderPath(localApplicationDataFolder);
            string tempRecordFilePath = localApplicationDataFolderPath + @"\OfficeWare\GameManager\" + currentUserId + @"\record.wav";
            WaveFileReader waveFileReader = new WaveFileReader(tempRecordFilePath);
            IWavePlayer player = new WaveOut(WaveCallbackInfo.FunctionCallback());
            player.Volume = 1.0f;
            player.Init(waveFileReader);
            player.Play();
            while (true)
            {
                if (player.PlaybackState == NAudio.Wave.PlaybackState.Stopped)
                {
                    player.Dispose();
                    waveFileReader.Close();
                    waveFileReader.Dispose();
                    break;
                }
            };

        }

        private void AttachFileHandler (object sender, RoutedEventArgs e)
        {
            AttachFile();
        }

        public void AttachFile ()
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

        async public void SendFileMsg (string filePath)
        {
            byte[] rawImage = ImageFileToByteArray(filePath);
            string newMsgContent = "";
            /*int rawImageItemIndex = -1;
            foreach (byte rawImageItem in rawImage)
            {
                rawImageItemIndex++;
                newMsgContent += rawImageItem;
                if (rawImageItemIndex < rawImage.Length - 2)
                {
                    newMsgContent += "^";
                }
            }*/
            try
            {
                try
                {
                    HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("https://loud-reminiscent-jackrabbit.glitch.me/api/users/get/?id=" + currentUserId);
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
                                object rawActiveChatScrollContent = activeChat.Content;
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
                                Uri newMsgHeaderAvatarUri = new Uri("https://cdn2.iconfinder.com/data/icons/ios-7-icons/50/user_male-128.png");
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
                await client.EmitAsync("user_send_msg", currentUserId + "|" + newMsgContent + "|" + this.friendId);
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
                string newMsgType = "file";
                HttpClient httpClient = new HttpClient();
                MultipartFormDataContent form = new MultipartFormDataContent();
                byte[] imagebytearraystring = ImageFileToByteArray(filePath);
                form.Add(new ByteArrayContent(imagebytearraystring, 0, imagebytearraystring.Count()), "profile_pic", "mock" + System.IO.Path.GetExtension(filePath));
                string url = @"http://localhost:4000/api/msgs/add/?user=" + currentUserId + "&friend=" + friendId + "&content=" + "newMsgContent" + "&type=" + newMsgType + "&id=" + "hash" + "&ext=" + System.IO.Path.GetExtension(filePath);
                HttpResponseMessage response = httpClient.PostAsync(url, form).Result;
                httpClient.Dispose();
                string sd = response.Content.ReadAsStringAsync().Result;
            }
            catch (System.Net.WebException)
            {
                MessageBox.Show("Не удается подключиться к серверу", "Ошибка");
                this.Close();
            }
        }

        private void OpenEmojiPopupHandler (object sender, RoutedEventArgs e)
        {
            OpenEmojiPopup();
        }

        public void OpenEmojiPopup () {
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
                try
                {
                    HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("https://loud-reminiscent-jackrabbit.glitch.me/api/users/get/?id=" + currentUserId);
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
                                object rawActiveChatScrollContent = activeChat.Content;
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
                                Uri newMsgHeaderAvatarUri = new Uri("https://cdn2.iconfinder.com/data/icons/ios-7-icons/50/user_male-128.png");
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
                await client.EmitAsync("user_send_msg", currentUserId + "|" + emojiData + "|" + this.friendId);
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
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("https://loud-reminiscent-jackrabbit.glitch.me/api/msgs/add/?user=" + currentUserId + "&friend=" + friendId + "&content=" + emojiData + "&type=" + newMsgType);
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

        public byte[] ImageFileToByteArray(string fullFilePath)
        {
            FileStream fs = File.OpenRead(fullFilePath);
            byte[] bytes = new byte[fs.Length];
            fs.Read(bytes, 0, Convert.ToInt32(fs.Length));
            fs.Close();
            return bytes;
        }

    }

    class MsgsResponseInfo
    {
        public List<Msg> msgs;
        public string status;
    }

    class Msg
    {
        public string _id;
        public string user;
        public string friend;
        public string content;
        public string type;
    }

}
