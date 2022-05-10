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
        public Brush msgsSeparatorBrush = null;
        public List<string> chats;

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
        
        public ChatDialog(string currentUserId, SocketIO client, string friendId, bool isStartBlink, List<string> chats)
        {
            InitializeComponent();
            this.currentUserId = currentUserId;
            this.friendId = friendId;
            this.isStartBlink = isStartBlink;
            this.chats = chats;
        }

        async public void ReceiveMessages()
        {
            Environment.SpecialFolder localApplicationDataFolder = Environment.SpecialFolder.LocalApplicationData;
            string localApplicationDataFolderPath = Environment.GetFolderPath(localApplicationDataFolder);
            string saveDataFilePath = localApplicationDataFolderPath + @"\OfficeWare\GameManager\" + currentUserId + @"\save-data.txt";
            JavaScriptSerializer js = new JavaScriptSerializer();
            string saveDataFileContent = File.ReadAllText(saveDataFilePath);
            SavedContent loadedContent = js.Deserialize<SavedContent>(saveDataFileContent);
            Settings currentSettings = loadedContent.settings;
            try
            {
                // client = new SocketIO("http://localhost:4000/");
                client = new SocketIO("https://digitaldistributtionservice.herokuapp.com/");
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
                    Debugger.Log(0, "debug", Environment.NewLine + "user " + userId + " send msg: " + msg + Environment.NewLine);
                    Debugger.Log(0, "debug", Environment.NewLine + "chats " + String.Join("|", this.chats) + Environment.NewLine);
                    try
                    {
                        HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/friends/get");
                        webRequest.Method = "GET";
                        webRequest.UserAgent = ".NET Framework Test Client";
                        using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                        {
                            using (var reader = new StreamReader(webResponse.GetResponseStream()))
                            {
                                js = new JavaScriptSerializer();
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
                                        this.Dispatcher.Invoke(() =>
                                        {
                                            string currentFriendId = this.chats[chatControl.SelectedIndex];
                                            // bool isCurrentChat = currentFriendId == userId;
                                            // bool isCurrentChat = currentFriendId == userId && currentUserId == cachedFriendId;
                                            bool isCurrentChat = currentUserId == cachedFriendId;
                                            if (isCurrentChat)
                                            {
                                                this.Dispatcher.Invoke(() =>
                                                {
                                                    try
                                                    {
                                                        HttpWebRequest innerWebRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/users/get/?id=" + this.chats[chatControl.SelectedIndex]);
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

                                                                    // object rawActiveChat = chatControlItems[chatControl.SelectedIndex];
                                                                    int chatIndex = chatControl.SelectedIndex;
                                                                    chatIndex = chats.FindIndex((string chatId) =>
                                                                    {
                                                                        bool isChatForUser = userId == chatId;
                                                                        return isChatForUser;
                                                                    });
                                                                    bool isChatFound = chatIndex >= 0;
                                                                    if (isChatFound)
                                                                    {
                                                                        object rawActiveChat = chatControlItems[chatIndex];

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

                                                                        bool isShowTimeIn24 = currentSettings.isShowTimeIn24;
                                                                        bool isShowTimeIn12 = !isShowTimeIn24;                                                                        
                                                                        
                                                                        // string rawCurrentDate = currentDate.ToLongTimeString(); 
                                                                        string rawCurrentDate = currentDate.ToLongDateString();
                                                                        string rawCurrentTime = currentDate.ToLongTimeString();
                                                                        if (isShowTimeIn12)
                                                                        {
                                                                            string rawDate = currentDate.ToString("h:mm");
                                                                            DateTime dt = DateTime.Parse(rawDate);
                                                                            rawCurrentDate = dt.ToLongTimeString();
                                                                            rawCurrentTime = dt.ToLongTimeString();
                                                                        }
                                                                        string newMsgDateLabelContent = rawCurrentDate + " " + rawCurrentTime; ;
                                                                        // newMsgDateLabel.Text = rawCurrentDate;
                                                                        newMsgDateLabel.Text = newMsgDateLabelContent;
                                                                        
                                                                        newMsgHeader.Children.Add(newMsgDateLabel);
                                                                        newMsg.Children.Add(newMsgHeader);
                                                                        bool isTextMsg = msgType == "text";
                                                                        bool isEmojiMsg = msgType == "emoji";
                                                                        bool isFileMsg = msgType == "file";
                                                                        if (isTextMsg)
                                                                        {
                                                                            TextBlock newMsgLabel = new TextBlock();
                                                                            string newMsgContent = msg;
                                                                            newMsgLabel.Text = newMsgContent;
                                                                            newMsgLabel.Margin = new Thickness(40, 10, 10, 10);

                                                                            int fontSize = 12;
                                                                            string chatFontSize = currentSettings.chatFontSize;
                                                                            bool isSmallChatFontSize = chatFontSize == "small";
                                                                            bool isStandardChatFontSize = chatFontSize == "standard";
                                                                            bool isBigChatFontSize = chatFontSize == "big";
                                                                            if (isSmallChatFontSize)
                                                                            {
                                                                                fontSize = 12;
                                                                            }
                                                                            else if (isStandardChatFontSize)
                                                                            {
                                                                                fontSize = 14;
                                                                            }
                                                                            else if (isBigChatFontSize)
                                                                            {
                                                                                fontSize = 16;
                                                                            }
                                                                            newMsgLabel.FontSize = fontSize;

                                                                            inputChatMsgBox.Text = "";
                                                                            newMsg.Children.Add(newMsgLabel);
                                                                            activeChatContent.Children.Add(newMsg);
                                                                        }
                                                                        else if (isEmojiMsg)
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
                                                                        else if (isFileMsg)
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
                                                                        activeChatScrollContent.ScrollToBottom();
                                                                    }
                                                                }
                                                            }
                                                        }

                                                        string blinkType = currentSettings.sendMsgBlinkWindowType;
                                                        bool isAlwaysBlinkType = blinkType == "always";
                                                        bool isMinimizeBlinkType = blinkType == "minimize";
                                                        if (isAlwaysBlinkType)
                                                        {
                                                            FlashWindow(this);
                                                        }
                                                        else if (isMinimizeBlinkType)
                                                        {
                                                            WindowState windowState = this.WindowState;
                                                            WindowState minimizedWindow = WindowState.Minimized;
                                                            bool isWindowMinimized = minimizedWindow == windowState;
                                                            if (isWindowMinimized)
                                                            {
                                                                FlashWindow(this);
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
                                        });
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
                    string recepientId = result[1];
                    Debugger.Log(0, "debug", Environment.NewLine + "user " + userId + " send msg: " + recepientId + Environment.NewLine);
                    Debugger.Log(0, "debug", Environment.NewLine + "chats " + String.Join("|", this.chats) + Environment.NewLine);
                    try
                    {
                        HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/friends/get");
                        webRequest.Method = "GET";
                        webRequest.UserAgent = ".NET Framework Test Client";
                        using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                        {
                            using (var reader = new StreamReader(webResponse.GetResponseStream()))
                            {
                                js = new JavaScriptSerializer();
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
                                        this.Dispatcher.Invoke(async () =>
                                        {
                                            string currentFriendId = this.chats[chatControl.SelectedIndex];
                                            bool isCurrentChat = currentFriendId == userId && recepientId == currentUserId;
                                            if (isCurrentChat)
                                            {
                                                this.Dispatcher.Invoke(async () =>
                                                {
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
                                                                string friendName = user.name;
                                                                string userIsWritingLabelContent = friendName + " печатает...";
                                                                userIsWritingLabel.Text = userIsWritingLabelContent;
                                                                userIsWritingLabel.Visibility = Visibility.Visible;
                                                                await Task.Delay(TimeSpan.FromSeconds(2)).ConfigureAwait(true);
                                                                userIsWritingLabel.Visibility = Visibility.Hidden;
                                                            }
                                                        }
                                                    }
                                                });
                                            }
                                        });
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

        public void InitializeHandler(object sender, RoutedEventArgs e)
        {
            Initialize();

            /*int waveindevice = (Int32)System.Windows.Forms.Application.UserAppDataRegistry.GetValue("WaveIn", 0);
            int waveoutdevice = (Int32)System.Windows.Forms.Application.UserAppDataRegistry.GetValue("Waveout", 0);*/

        }

        public void InitConstants()
        {
            msgsSeparatorBrush = System.Windows.Media.Brushes.LightGray;
            lastInputTimeStamp = DateTime.Now;
        }

        public void Initialize()
        {
            InitConstants();
            AddChat();
            ReceiveMessages();
            // GetMsgs();
            InitFlash();
            GetChatSettings();
        }

        public void GetChatSettings ()
        {
            Environment.SpecialFolder localApplicationDataFolder = Environment.SpecialFolder.LocalApplicationData;
            string localApplicationDataFolderPath = Environment.GetFolderPath(localApplicationDataFolder);
            string saveDataFilePath = localApplicationDataFolderPath + @"\OfficeWare\GameManager\" + currentUserId + @"\save-data.txt";
            JavaScriptSerializer js = new JavaScriptSerializer();
            string saveDataFileContent = File.ReadAllText(saveDataFilePath);
            SavedContent loadedContent = js.Deserialize<SavedContent>(saveDataFileContent);
            Settings currentSettings = loadedContent.settings;
            bool isDisableSpellCheck = currentSettings.isDisableSpellCheck;
            bool isEnableSpellCheck = !isDisableSpellCheck;
            inputChatMsgBox.SpellCheck.IsEnabled = isEnableSpellCheck;
        }

        public void InitFlash()
        {
            if (isStartBlink)
            {
                FlashWindow(this);
            }
        }

        // public void AddChat()
        public void AddChat()
        {

            TabItem newChat = new TabItem();

            int countChats = this.chats.Count;
            int lastChatIndex = countChats - 1;
            string lastChatId = this.chats[lastChatIndex];
            newChat.Header = lastChatId;

            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/users/get/?id=" + this.chats[this.chats.Count - 1]);
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

                            /*string userIsWritingLabelContent = friendName + " печатает...";
                            userIsWritingLabel.Text = userIsWritingLabelContent;*/

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

            GetMsgs();

            ContextMenu newChatContextMenu = new ContextMenu();
            MenuItem newChatContextMenuItem = new MenuItem();
            newChatContextMenuItem.Header = "Закрыть вкладку";
            newChatContextMenuItem.DataContext = lastChatId;
            newChatContextMenuItem.Click += CloseChatHandler;
            newChatContextMenu.Items.Add(newChatContextMenuItem);
            newChat.ContextMenu = newChatContextMenu;

        }

        private void SendsgHandler(object sender, RoutedEventArgs e)
        {
            string newMsgContent = inputChatMsgBox.Text;
            SendMsg(newMsgContent);
        }

        public void GetMsgs ()
        {

            Environment.SpecialFolder localApplicationDataFolder = Environment.SpecialFolder.LocalApplicationData;
            string localApplicationDataFolderPath = Environment.GetFolderPath(localApplicationDataFolder);
            string saveDataFilePath = localApplicationDataFolderPath + @"\OfficeWare\GameManager\" + currentUserId + @"\save-data.txt";
            JavaScriptSerializer js = new JavaScriptSerializer();
            string saveDataFileContent = File.ReadAllText(saveDataFilePath);
            SavedContent loadedContent = js.Deserialize<SavedContent>(saveDataFileContent);
            Settings currentSettings = loadedContent.settings;

            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/users/get/?id=" + currentUserId);
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (StreamReader reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        js = new JavaScriptSerializer();
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
                                                    // bool isCurrentChatMsg = (newMsgUserId == currentUserId && newMsgFriendId == friendId) || (newMsgUserId == friendId && newMsgFriendId == currentUserId);
                                                    bool isCurrentChatMsg = (newMsgUserId == currentUserId && newMsgFriendId == this.chats[chatControl.SelectedIndex]) || (newMsgUserId == this.chats[chatControl.SelectedIndex] && newMsgFriendId == this.chats[chatControl.SelectedIndex]);
                                                    if (isCurrentChatMsg)
                                                    {
                                                        DateTime msgDate = msg.date;
                                                        string rawMsgDate = msgDate.ToLongDateString();
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
                                                        User friend = myobj.user;
                                                        string friendName = friend.name;
                                                        ItemCollection chatControlItems = chatControl.Items;
                                                        object rawActiveChat = chatControlItems[chatControl.SelectedIndex];
                                                        TabItem activeChat = ((TabItem)(rawActiveChat));
                                                        object rawActiveChatScrollContent = activeChat.Content;
                                                        ScrollViewer activeChatScrollContent = ((ScrollViewer)(rawActiveChatScrollContent));
                                                        object rawActiveChatContent = activeChatScrollContent.Content;
                                                        StackPanel activeChatContent = ((StackPanel)(rawActiveChatContent));
                                                        // string rawMsgDate = msgDate.ToLongDateString();
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

                                                                    // string rawMsgDate = msgDate.ToLongDateString();
                                                                    string rawMsgTime = msgDate.ToLongTimeString();
                                                                    bool isShowTimeIn24 = currentSettings.isShowTimeIn24;
                                                                    bool isShowTimeIn12 = !isShowTimeIn24;
                                                                    if (isShowTimeIn12)
                                                                    {
                                                                        string rawDate = msgDate.ToString("h:mm");
                                                                        DateTime dt = DateTime.Parse(rawDate);
                                                                        rawMsgDate = dt.ToLongTimeString();
                                                                        rawMsgTime = dt.ToLongTimeString();
                                                                    }
                                                                    string msgsSeparatorDateLabelContent = rawMsgDate + " " + rawMsgTime;
                                                                    msgsSeparatorDateLabel.Text = msgsSeparatorDateLabelContent;
                                                                    // msgsSeparatorDateLabel.Text = rawMsgDate;

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
                                                        bool isLinkMsg = newMsgType == "link";
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

                                                            string rawCurrentDate = msgDate.ToLongDateString();
                                                            string rawCurrentTime = msgDate.ToLongTimeString();
                                                            bool isShowTimeIn24 = currentSettings.isShowTimeIn24;
                                                            bool isShowTimeIn12 = !isShowTimeIn24;
                                                            if (isShowTimeIn12)
                                                            {
                                                                string rawDate = msgDate.ToString("h:mm:ss");
                                                                DateTime dt = DateTime.Parse(rawDate);
                                                                rawCurrentTime = dt.ToLongTimeString();
                                                            }
                                                            string newMsgDateLabelContent = rawCurrentDate + " " + rawCurrentTime;
                                                            // newMsgDateLabel.Text = rawMsgDate;
                                                            newMsgDateLabel.Text = newMsgDateLabelContent;
                                                            
                                                            newMsgHeader.Children.Add(newMsgDateLabel);
                                                            newMsg.Children.Add(newMsgHeader);
                                                            TextBlock newMsgLabel = new TextBlock();
                                                            newMsgLabel.Margin = new Thickness(40, 10, 10, 10);

                                                            int fontSize = 12;
                                                            string chatFontSize = currentSettings.chatFontSize;
                                                            bool isSmallChatFontSize = chatFontSize == "small";
                                                            bool isStandardChatFontSize = chatFontSize == "standard";
                                                            bool isBigChatFontSize = chatFontSize == "big";
                                                            if (isSmallChatFontSize)
                                                            {
                                                                fontSize = 12;
                                                            }
                                                            else if (isStandardChatFontSize)
                                                            {
                                                                fontSize = 14;
                                                            }
                                                            else if (isBigChatFontSize)
                                                            {
                                                                fontSize = 16;
                                                            }
                                                            newMsgLabel.FontSize = fontSize;

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

                                                            string rawCurrentDate = msgDate.ToLongDateString();
                                                            string rawCurrentTime = msgDate.ToLongTimeString();
                                                            bool isShowTimeIn24 = currentSettings.isShowTimeIn24;
                                                            bool isShowTimeIn12 = !isShowTimeIn24;
                                                            if (isShowTimeIn12)
                                                            {
                                                                string rawDate = msgDate.ToString("h:mm:ss");
                                                                DateTime dt = DateTime.Parse(rawDate);
                                                                rawCurrentTime = dt.ToLongTimeString();
                                                            }
                                                            string newMsgDateLabelContent = rawCurrentDate + " " + rawCurrentTime;
                                                            // newMsgDateLabel.Text = rawMsgDate;
                                                            newMsgDateLabel.Text = newMsgDateLabelContent;
                                                            
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

                                                            string rawCurrentDate = msgDate.ToLongDateString();
                                                            string rawCurrentTime = msgDate.ToLongTimeString();
                                                            bool isShowTimeIn24 = currentSettings.isShowTimeIn24;
                                                            bool isShowTimeIn12 = !isShowTimeIn24;
                                                            if (isShowTimeIn12)
                                                            {
                                                                string rawDate = msgDate.ToString("h:mm:ss");
                                                                DateTime dt = DateTime.Parse(rawDate);
                                                                rawCurrentTime = dt.ToLongTimeString();
                                                            }
                                                            string newMsgDateLabelContent = rawCurrentDate + " " + rawCurrentTime;
                                                            // newMsgDateLabel.Text = rawMsgDate;
                                                            newMsgDateLabel.Text = newMsgDateLabelContent;
                                                            
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
                                                        else if (isLinkMsg)
                                                        {
                                                            nestedWebRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/talks/relations/all");
                                                            nestedWebRequest.Method = "GET";
                                                            nestedWebRequest.UserAgent = ".NET Framework Test Client";
                                                            using (HttpWebResponse nestedWebResponse = (HttpWebResponse)nestedWebRequest.GetResponse())
                                                            {
                                                                using (var nestedReader = new StreamReader(nestedWebResponse.GetResponseStream()))
                                                                {
                                                                    js = new JavaScriptSerializer();
                                                                    objText = nestedReader.ReadToEnd();
                                                                    TalkRelationsResponseInfo myNestedObj = (TalkRelationsResponseInfo)js.Deserialize(objText, typeof(TalkRelationsResponseInfo));
                                                                    status = myNestedObj.status;
                                                                    isOkStatus = status == "OK";
                                                                    if (isOkStatus)
                                                                    {
                                                                        List<TalkRelation> relations = myNestedObj.relations;
                                                                        Talk talk = GetTalkInfo(newMsgContent);
                                                                        string talkId = talk._id;
                                                                        List<string> talkRelationIds = new List<string>();
                                                                        foreach (TalkRelation relation in relations)
                                                                        {
                                                                            string relationUserId = relation.user;
                                                                            string relationTalkId = relation.talk;
                                                                            bool isCurrentTalk = relationTalkId == talkId;
                                                                            if (isCurrentTalk)
                                                                            {
                                                                                talkRelationIds.Add(relationUserId);
                                                                            }
                                                                        }
                                                                        bool isFriendInTalk = talkRelationIds.Contains(newMsgFriendId);
                                                                        bool isFriendNotInTalk = !isFriendInTalk;
                                                                        if (isFriendNotInTalk)
                                                                        {
                                                                            // приглашение не принято можно вывести ссылку на добавление в беседу
                                                                            string talkTitle = talk.title;
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

                                                                            string rawCurrentDate = msgDate.ToLongDateString();
                                                                            string rawCurrentTime = msgDate.ToLongTimeString();
                                                                            bool isShowTimeIn24 = currentSettings.isShowTimeIn24;
                                                                            bool isShowTimeIn12 = !isShowTimeIn24;
                                                                            if (isShowTimeIn12)
                                                                            {
                                                                                string rawDate = msgDate.ToString("h:mm:ss");
                                                                                DateTime dt = DateTime.Parse(rawDate);
                                                                                rawCurrentTime = dt.ToLongTimeString();
                                                                            }
                                                                            string newMsgDateLabelContent = rawCurrentDate + " " + rawCurrentTime;
                                                                            // newMsgDateLabel.Text = rawMsgDate;
                                                                            newMsgDateLabel.Text = newMsgDateLabelContent;
                                                                            
                                                                            newMsgHeader.Children.Add(newMsgDateLabel);
                                                                            newMsg.Children.Add(newMsgHeader);
                                                                            TextBlock newMsgLabel = new TextBlock();
                                                                            newMsgLabel.Margin = new Thickness(40, 10, 10, 10);
                                                                            string newMsgLabelContent = "Принять приглашение в беседу " + talkTitle;
                                                                            newMsgLabel.Text = newMsgLabelContent;

                                                                            int fontSize = 12;
                                                                            string chatFontSize = currentSettings.chatFontSize;
                                                                            bool isSmallChatFontSize = chatFontSize == "small";
                                                                            bool isStandardChatFontSize = chatFontSize == "standard";
                                                                            bool isBigChatFontSize = chatFontSize == "big";
                                                                            if (isSmallChatFontSize)
                                                                            {
                                                                                fontSize = 12;
                                                                            }
                                                                            else if (isStandardChatFontSize)
                                                                            {
                                                                                fontSize = 14;
                                                                            }
                                                                            else if (isBigChatFontSize)
                                                                            {
                                                                                fontSize = 16;
                                                                            }
                                                                            newMsgLabel.FontSize = fontSize;

                                                                            newMsg.Children.Add(newMsgLabel);
                                                                            activeChatContent.Children.Add(newMsg);
                                                                            bool isLinkForMe = newMsgFriendId == currentUserId;
                                                                            if (isLinkForMe)
                                                                            {
                                                                                newMsgLabel.TextDecorations = TextDecorations.Underline;
                                                                                Dictionary<String, Object> newMsgData = new Dictionary<String, Object>();
                                                                                newMsgData.Add("msg", newMsgId);
                                                                                newMsgData.Add("talk", newMsgContent);
                                                                                newMsgData.Add("label", newMsgLabel);
                                                                                newMsg.DataContext = newMsgData;
                                                                                newMsg.MouseLeftButtonUp += OpenLinkHandler;
                                                                            }
                                                                            else
                                                                            {
                                                                                newMsgLabel.Foreground = System.Windows.Media.Brushes.LightGray;
                                                                            }
                                                                        }
                                                                        else
                                                                        {
                                                                            // приглашение уже принято нужно блокировать ссылки на добавление в беседу, чтобы не создавать лишних связей
                                                                            string talkTitle = talk.title;
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

                                                                            string rawCurrentDate = msgDate.ToLongDateString();
                                                                            string rawCurrentTime = msgDate.ToLongTimeString();
                                                                            bool isShowTimeIn24 = currentSettings.isShowTimeIn24;
                                                                            bool isShowTimeIn12 = !isShowTimeIn24;
                                                                            if (isShowTimeIn12)
                                                                            {
                                                                                string rawDate = msgDate.ToString("h:mm:ss");
                                                                                DateTime dt = DateTime.Parse(rawDate);
                                                                                rawCurrentTime = dt.ToLongTimeString();
                                                                            }
                                                                            string newMsgDateLabelContent = rawCurrentDate + " " + rawCurrentTime;
                                                                            // newMsgDateLabel.Text = rawMsgDate;
                                                                            newMsgDateLabel.Text = newMsgDateLabelContent;
                                                                            
                                                                            newMsgHeader.Children.Add(newMsgDateLabel);
                                                                            newMsg.Children.Add(newMsgHeader);
                                                                            TextBlock newMsgLabel = new TextBlock();
                                                                            newMsgLabel.Margin = new Thickness(40, 10, 10, 10);
                                                                            string newMsgLabelContent = "Приглашение в беседу " + talkTitle + " было принято.";
                                                                            newMsgLabel.Text = newMsgLabelContent;

                                                                            int fontSize = 12;
                                                                            string chatFontSize = currentSettings.chatFontSize;
                                                                            bool isSmallChatFontSize = chatFontSize == "small";
                                                                            bool isStandardChatFontSize = chatFontSize == "standard";
                                                                            bool isBigChatFontSize = chatFontSize == "big";
                                                                            if (isSmallChatFontSize)
                                                                            {
                                                                                fontSize = 12;
                                                                            }
                                                                            else if (isStandardChatFontSize)
                                                                            {
                                                                                fontSize = 14;
                                                                            }
                                                                            else if (isBigChatFontSize)
                                                                            {
                                                                                fontSize = 16;
                                                                            }
                                                                            newMsgLabel.FontSize = fontSize;

                                                                            newMsg.Children.Add(newMsgLabel);
                                                                            activeChatContent.Children.Add(newMsg);
                                                                            newMsgLabel.Foreground = System.Windows.Media.Brushes.LightGray;
                                                                            // вы уже приняли это приглашение
                                                                        }
                                                                    }
                                                                }
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

        public void OpenLinkHandler(object sender, RoutedEventArgs e)
        {
            StackPanel msg = ((StackPanel)(sender));
            object rawMsgData = msg.DataContext;
            Dictionary<String, Object> msgData = ((Dictionary<String, Object>)(rawMsgData));
            string msgId = ((string)(msgData["msg"]));
            string talkId = ((string)(msgData["talk"]));
            TextBlock label = ((TextBlock)(msgData["label"]));
            OpenLink(talkId, msg, msgId, label);
        }

        public void OpenLink(string talkId, StackPanel msg, string msgId, TextBlock msgLabel)
        {
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/talks/relations/add/?id=" + talkId + "&user=" + currentUserId + "&msg=" + msgId);
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (var reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        msgLabel.Foreground = System.Windows.Media.Brushes.LightGray;
                        msgLabel.TextDecorations = null;
                        msg.MouseLeftButtonUp -= OpenLinkHandler;
                        // закрываем окно чтобы в случае если было несколько приглашений в 1 беседу обновить сообщения при следующем открытии окна и заблокировать такие ссылки
                        this.Close();
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

        private void SendMsgHandler(object sender, RoutedEventArgs e)
        {
            string newMsgContent = inputChatMsgBox.Text;
            SendMsg(newMsgContent);
        }

        async public void SendMsg (string newMsgContent)
        {

            Environment.SpecialFolder localApplicationDataFolder = Environment.SpecialFolder.LocalApplicationData;
            string localApplicationDataFolderPath = Environment.GetFolderPath(localApplicationDataFolder);
            string saveDataFilePath = localApplicationDataFolderPath + @"\OfficeWare\GameManager\" + currentUserId + @"\save-data.txt";
            JavaScriptSerializer js = new JavaScriptSerializer();
            string saveDataFileContent = File.ReadAllText(saveDataFilePath);
            SavedContent loadedContent = js.Deserialize<SavedContent>(saveDataFileContent);
            Settings currentSettings = loadedContent.settings;

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
                            string objText = innerReader.ReadToEnd();
                            UserResponseInfo myobj = (UserResponseInfo)js.Deserialize(objText, typeof(UserResponseInfo));
                            string status = myobj.status;
                            bool isOkStatus = status == "OK";
                            if (isOkStatus)
                            {
                                User friend = myobj.user;
                                string friendName = friend.name;
                                ItemCollection chatControlItems = chatControl.Items;
                                object rawActiveChat = chatControlItems[chatControl.SelectedIndex];
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

                                // string rawCurrentDate = currentDate.ToLongTimeString();
                                string rawCurrentDate = currentDate.ToLongDateString();
                                string rawCurrentTime = currentDate.ToLongTimeString();

                                bool isShowTimeIn24 = currentSettings.isShowTimeIn24;
                                bool isShowTimeIn12 = !isShowTimeIn24;
                                if (isShowTimeIn12)
                                {
                                    string rawDate = currentDate.ToString("h:mm:ss");
                                    DateTime dt = DateTime.Parse(rawDate);
                                    rawCurrentTime = dt.ToLongTimeString();
                                }

                                string newMsgDateLabelContent = rawCurrentDate + " " + rawCurrentTime; ;
                                newMsgDateLabel.Text = newMsgDateLabelContent;
                                newMsgHeader.Children.Add(newMsgDateLabel);
                                newMsg.Children.Add(newMsgHeader);
                                TextBlock newMsgLabel = new TextBlock();
                                newMsgLabel.Margin = new Thickness(40, 10, 10, 10);
                                newMsgLabel.Text = newMsgContent;
                                
                                int fontSize = 12;
                                string chatFontSize = currentSettings.chatFontSize;
                                bool isSmallChatFontSize = chatFontSize == "small";
                                bool isStandardChatFontSize = chatFontSize == "standard";
                                bool isBigChatFontSize = chatFontSize == "big";
                                if (isSmallChatFontSize)
                                {
                                    fontSize = 12;
                                }
                                else if (isStandardChatFontSize)
                                {
                                    fontSize = 14;
                                }
                                else if (isBigChatFontSize)
                                {
                                    fontSize = 16;
                                }
                                newMsgLabel.FontSize = fontSize;

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
                // await client.EmitAsync("user_send_msg", currentUserId + "|" + newMsgContent + "|" + this.friendId + "|" + newMsgType + "|" + newMsgId);
                await client.EmitAsync("user_send_msg", currentUserId + "|" + newMsgContent + "|" + this.chats[chatControl.SelectedIndex] + "|" + newMsgType + "|" + newMsgId);
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
                // HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/msgs/add/?user=" + currentUserId + "&friend=" + friendId + "&content=" + newMsgContent + "&type=" + newMsgType + "&channel=mockChannelId");
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/msgs/add/?user=" + currentUserId + "&friend=" + this.chats[chatControl.SelectedIndex] + "&content=" + newMsgContent + "&type=" + newMsgType + "&channel=mockChannelId");
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse innerWebResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (StreamReader innerReader = new StreamReader(innerWebResponse.GetResponseStream()))
                    {
                        js = new JavaScriptSerializer();
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

        private void StopBlinkWindowHandler(object sender, RoutedEventArgs e)
        {
            StopBlinkWindow();
        }

        public void StopBlinkWindow()
        {
            StopFlashingWindow(this);
            /*int chatIndex = chatControl.SelectedIndex;
            chatIndex = chats.FindIndex((string chatId) =>
            {
                bool isChatForUser = friendId == chatId;
                return isChatForUser;
            });
            bool isChatFound = chatIndex >= 0;
            if (isChatFound)
            {
                chatControl.SelectedIndex = chatIndex;
            }*/
        }

        public void SelectChat (string localFriendId)
        {
            int chatIndex = chatControl.SelectedIndex;
            chatIndex = chats.FindIndex((string chatId) =>
            {
                bool isChatForUser = localFriendId == chatId;
                return isChatForUser;
            });
            bool isChatFound = chatIndex >= 0;
            if (isChatFound)
            {
                chatControl.SelectedIndex = chatIndex;
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
                // string eventData = currentUserId + "|" + this.friendId;
                string eventData = currentUserId + "|" + this.chats[chatControl.SelectedIndex];
                client.EmitAsync("user_write_msg", eventData);
            }
            lastInputTimeStamp = currentDateTime;
        }

        private void ToggleRingHandler(object sender, RoutedEventArgs e)
        {
            Button btn = ((Button)(sender));
            ToggleRing(btn);
        }

        async public void ToggleRing(Button btn)
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

        public void MicroDataAvailableHandler(object sender, WaveInEventArgs e)
        {
            byte[] buffer = e.Buffer;
            int recordedBytes = e.BytesRecorded;
            MicroDataAvailable(buffer, recordedBytes);
        }

        public void MicroDataAvailable(byte[] buffer, int recordedBytes)
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

        public void MicroRecordingStoppedHandler(object sender, StoppedEventArgs e)
        {
            MicroRecordingStopped();
        }

        public void MicroRecordingStopped()
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

        private void AttachFileHandler(object sender, RoutedEventArgs e)
        {
            AttachFile();
        }

        public void AttachFile ()
        {
            Environment.SpecialFolder localApplicationDataFolder = Environment.SpecialFolder.LocalApplicationData;
            string localApplicationDataFolderPath = Environment.GetFolderPath(localApplicationDataFolder);
            string saveDataFilePath = localApplicationDataFolderPath + @"\OfficeWare\GameManager\" + currentUserId + @"\save-data.txt";
            JavaScriptSerializer js = new JavaScriptSerializer();
            string saveDataFileContent = File.ReadAllText(saveDataFilePath);
            SavedContent loadedContent = js.Deserialize<SavedContent>(saveDataFileContent);
            Settings currentSettings = loadedContent.settings;
            bool isNotIncludeImagesAndMediaFiles = currentSettings.isNotIncludeImagesAndMediaFiles;
            bool isIncludeImagesAndMediaFiles = !isNotIncludeImagesAndMediaFiles;
            if (isIncludeImagesAndMediaFiles)
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
        }

        async public void SendFileMsg (string filePath)
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
                                object rawActiveChat = chatControlItems[chatControl.SelectedIndex];
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

                                Environment.SpecialFolder localApplicationDataFolder = Environment.SpecialFolder.LocalApplicationData;
                                string localApplicationDataFolderPath = Environment.GetFolderPath(localApplicationDataFolder);
                                string saveDataFilePath = localApplicationDataFolderPath + @"\OfficeWare\GameManager\" + currentUserId + @"\save-data.txt";
                                js = new JavaScriptSerializer();
                                string saveDataFileContent = File.ReadAllText(saveDataFilePath);
                                SavedContent loadedContent = js.Deserialize<SavedContent>(saveDataFileContent);
                                Settings currentSettings = loadedContent.settings;
                                // string rawCurrentDate = currentDate.ToLongTimeString();
                                string rawCurrentDate = currentDate.ToLongDateString();
                                string rawCurrentTime = currentDate.ToLongTimeString();
                                bool isShowTimeIn24 = currentSettings.isShowTimeIn24;
                                bool isShowTimeIn12 = !isShowTimeIn24;
                                if (isShowTimeIn12)
                                {
                                    string rawDate = currentDate.ToString("h:mm:ss");
                                    DateTime dt = DateTime.Parse(rawDate);
                                    rawCurrentTime = dt.ToLongTimeString();
                                }
                                string newMsgDateLabelContent = rawCurrentDate + " " + rawCurrentTime; ;
                                // newMsgDateLabel.Text = rawCurrentDate;
                                newMsgDateLabel.Text = newMsgDateLabelContent;
                                
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
                string newMsgType = "file";
                HttpClient httpClient = new HttpClient();
                MultipartFormDataContent form = new MultipartFormDataContent();
                byte[] imagebytearraystring = ImageFileToByteArray(filePath);
                form.Add(new ByteArrayContent(imagebytearraystring, 0, imagebytearraystring.Count()), "profile_pic", "mock" + System.IO.Path.GetExtension(filePath));
                // string url = @"http://localhost:4000/api/msgs/add/?user=" + currentUserId + "&friend=" + friendId + "&content=" + "newMsgContent" + "&type=" + newMsgType + "&id=" + "hash" + "&ext=" + System.IO.Path.GetExtension(filePath) + "&channel=mockChannelId";
                string url = @"http://localhost:4000/api/msgs/add/?user=" + currentUserId + "&friend=" + this.chats[this.chatControl.SelectedIndex] + "&content=" + "newMsgContent" + "&type=" + newMsgType + "&id=" + "hash" + "&ext=" + System.IO.Path.GetExtension(filePath) + "&channel=mockChannelId";
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
                    // await client.EmitAsync("user_send_msg", currentUserId + "|" + ext + "|" + this.friendId + "|" + newMsgType + "|" + newMsgId);
                    await client.EmitAsync("user_send_msg", currentUserId + "|" + ext + "|" + this.chats[chatControl.SelectedIndex] + "|" + newMsgType + "|" + newMsgId);
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

        async public void AddEmojiMsg(string emojiData)
        {
            Environment.SpecialFolder localApplicationDataFolder = Environment.SpecialFolder.LocalApplicationData;
            string localApplicationDataFolderPath = Environment.GetFolderPath(localApplicationDataFolder);
            string saveDataFilePath = localApplicationDataFolderPath + @"\OfficeWare\GameManager\" + currentUserId + @"\save-data.txt";
            JavaScriptSerializer js = new JavaScriptSerializer();
            string saveDataFileContent = File.ReadAllText(saveDataFilePath);
            SavedContent loadedContent = js.Deserialize<SavedContent>(saveDataFileContent);
            Settings currentSettings = loadedContent.settings;
            bool isNotIncludeImagesAndMediaFiles = currentSettings.isNotIncludeImagesAndMediaFiles;
            bool isIncludeImagesAndMediaFiles = !isNotIncludeImagesAndMediaFiles;
            if (isIncludeImagesAndMediaFiles)
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
                                string objText = innerReader.ReadToEnd();
                                UserResponseInfo myobj = (UserResponseInfo)js.Deserialize(objText, typeof(UserResponseInfo));
                                string status = myobj.status;
                                bool isOkStatus = status == "OK";
                                if (isOkStatus)
                                {
                                    User friend = myobj.user;
                                    string friendName = friend.name;
                                    ItemCollection chatControlItems = chatControl.Items;
                                    object rawActiveChat = chatControlItems[chatControl.SelectedIndex];
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
                                    
                                    string rawCurrentDate = currentDate.ToLongDateString();
                                    string rawCurrentTime = currentDate.ToLongTimeString();
                                    bool isShowTimeIn24 = currentSettings.isShowTimeIn24;
                                    bool isShowTimeIn12 = !isShowTimeIn24;
                                    if (isShowTimeIn12)
                                    {
                                        string rawDate = currentDate.ToString("h:mm:ss");
                                        DateTime dt = DateTime.Parse(rawDate);
                                        rawCurrentTime = dt.ToLongTimeString();
                                    }
                                    string newMsgDateLabelContent = rawCurrentDate + " " + rawCurrentTime; ;
                                    // newMsgDateLabel.Text = rawCurrentDate;
                                    newMsgDateLabel.Text = newMsgDateLabelContent;
                                    
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
                    // await client.EmitAsync("user_send_msg", currentUserId + "|" + emojiData + "|" + this.friendId + "|" + newMsgType + "|" + newMsgId);
                    await client.EmitAsync("user_send_msg", currentUserId + "|" + emojiData + "|" + this.chats[chatControl.SelectedIndex] + "|" + newMsgType + "|" + newMsgId);
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
                    // HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/msgs/add/?user=" + currentUserId + "&friend=" + friendId + "&content=" + emojiData + "&type=" + newMsgType + "&channel=mockChannelId");
                    HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/msgs/add/?user=" + currentUserId + "&friend=" + this.chatControl.SelectedIndex + "&content=" + emojiData + "&type=" + newMsgType + "&channel=mockChannelId");
                    webRequest.Method = "GET";
                    webRequest.UserAgent = ".NET Framework Test Client";
                    using (HttpWebResponse innerWebResponse = (HttpWebResponse)webRequest.GetResponse())
                    {
                        using (StreamReader innerReader = new StreamReader(innerWebResponse.GetResponseStream()))
                        {
                            js = new JavaScriptSerializer();
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
            emojiPopup.IsOpen = false;
        }

        public byte[] ImageFileToByteArray(string fullFilePath)
        {
            FileStream fs = File.OpenRead(fullFilePath);
            byte[] bytes = new byte[fs.Length];
            fs.Read(bytes, 0, Convert.ToInt32(fs.Length));
            fs.Close();
            return bytes;
        }

        private Talk GetTalkInfo(string talkId)
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

        public void CloseChatHandler (object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = ((MenuItem)(sender));
            object menuItemData = menuItem.DataContext;
            string id = ((string)(menuItemData));
            CloseChat(id);
        }

        public void CloseChat (string id)
        {
            ItemCollection chatControlItems = chatControl.Items;
            int chatControlItemsCount = chatControlItems.Count;
            bool isManyTabs = chatControlItemsCount >= 2;
            if (isManyTabs)
            {
                int chatIndex = this.chats.IndexOf(id);
                chatControl.Items.RemoveAt(chatIndex);
                this.chats = this.chats.Where<string>((string chatId) =>
                {
                    bool isCurrentChat = id == chatId;
                    bool isNotCurrentChat = !isCurrentChat;
                    return isNotCurrentChat;
                }).ToList<string>();
            }
            chatControlItemsCount = chatControlItems.Count;
            isManyTabs = chatControlItemsCount >= 2;
            foreach (TabItem chatControlItem in chatControlItems)
            {
                ContextMenu chatControlItemContextMenu = chatControlItem.ContextMenu;
                foreach (MenuItem chatControlItemContextMenuItem in chatControlItemContextMenu.Items)
                {
                    chatControlItemContextMenuItem.IsEnabled = isManyTabs;
                }
            }
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
        public DateTime date;
        public string channel;
    }

    class MsgResponseInfo
    {
        public string id;
        public string content;
    }

}
