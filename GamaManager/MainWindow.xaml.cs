using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Windows.Interop;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Net;
using System.ComponentModel;
using System.Management.Automation;
using System.Collections.ObjectModel;
using System.Web.Script.Serialization;
using System.IO;
using System.Windows.Controls.Primitives;
using MaterialDesignThemes.Wpf;
using System.Windows.Threading;
using GamaManager.Dialogs;
using SocketIOClient;
using Debugger = System.Diagnostics.Debugger;
using System.Windows.Media.Animation;
using System.Collections;
/*using OxyPlot;
using OxyPlot.Series;*/
using System.Net.Http;
using System.Net.Http.Headers;
using System.Collections.Specialized;
using Sparrow.Chart;
using ImapX;
using System.Net.Mail;

namespace GamaManager
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public string currentUserId = "";
        private User currentUser = null;
        public Visibility visible;
        public Visibility invisible;
        public Brush friendRequestBackground;
        public bool isAppInit = false;
        public DispatcherTimer timer;
        public int timerHours = 0;
        public SocketIO client;
        public List<int> history;
        public int historyCursor = -1;
        public Brush disabledColor;
        public Brush enabledColor;
        public bool isFullScreenMode = false;
        public string cachedUserProfileId = "";
        public string cachedGroupId = "";
        public byte[] manualAttachment;
        public string manualAttachmentExt;
        public Microsoft.Office.Interop.Word.Document doc = null;
        public Microsoft.Office.Interop.Word.Application wordApplication = null;
        public System.Windows.Xps.Packaging.XpsDocument document = null;
        public ImapClient mailClient = null;
        public bool isFamilyViewMode = false;

        public ObservableCollection<Model> Collection { get; set; }

        public MainWindow(string id)
        {

            PreInit(id);

            InitializeComponent();

            Initialize(id);

            SetStatsChart();

            /*using (LibVLCSharp.Shared.LibVLC libVlc = new LibVLCSharp.Shared.LibVLC())
            using (LibVLCSharp.Shared.MediaPlayer mediaPlayer = new LibVLCSharp.Shared.MediaPlayer(libVlc))
            {
                LibVLCSharp.Shared.Media media = new LibVLCSharp.Shared.Media(libVlc, "screen://", LibVLCSharp.Shared.FromType.FromLocation);
                media.AddOption(":screen-fps=24");
                media.AddOption(":sout=#transcode{vcodec=h264,vb=0,scale=0,acodec=mp4a,ab=128,channels=2,samplerate=44100}:file{dst=testvlc.mp4}");
                media.AddOption(":sout-keep");
                mediaPlayer.Play(media);
                Thread.Sleep(5 * 1000);
                mediaPlayer.Stop();
            }*/

        }

        public void GetTotalFriendsCount()
        {
            int countFriends = 0;
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/users/get/?id=" + currentUserId);
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (var reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        var objText = reader.ReadToEnd();
                        UserResponseInfo myobj = (UserResponseInfo)js.Deserialize(objText, typeof(UserResponseInfo));
                        string status = myobj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
                            webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/friends/get");
                            webRequest.Method = "GET";
                            webRequest.UserAgent = ".NET Framework Test Client";
                            using (HttpWebResponse nestedWebResponse = (HttpWebResponse)webRequest.GetResponse())
                            {
                                using (var nestedReader = new StreamReader(nestedWebResponse.GetResponseStream()))
                                {
                                    js = new JavaScriptSerializer();
                                    objText = nestedReader.ReadToEnd();
                                    FriendsResponseInfo myInnerObj = (FriendsResponseInfo)js.Deserialize(objText, typeof(FriendsResponseInfo));
                                    status = myobj.status;
                                    isOkStatus = status == "OK";
                                    if (isOkStatus)
                                    {
                                        List<Friend> receivedFriends = myInnerObj.friends;
                                        List<Friend> myFriends = receivedFriends.Where<Friend>((Friend friend) =>
                                        {
                                            return friend.user == currentUserId;
                                        }).ToList<Friend>();

                                        countFriends = myFriends.Count;

                                        string rawCountFriends = countFriends.ToString();
                                        yourCountFriendsLabel.Text = rawCountFriends;

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

        public void GetFriendsHandler(object sender, TextChangedEventArgs e)
        {
            GetFriends();
        }

        public void GetFriends()
        {
            GetConnectedFriends();
            GetOfflineFriends();
        }

        public void GetOnlineFriendsCount()
        {
            int countOnlineFriends = 0;
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/users/get/?id=" + currentUserId);
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (var reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        var objText = reader.ReadToEnd();

                        UserResponseInfo myobj = (UserResponseInfo)js.Deserialize(objText, typeof(UserResponseInfo));

                        string status = myobj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
                            webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/friends/get");
                            webRequest.Method = "GET";
                            webRequest.UserAgent = ".NET Framework Test Client";
                            using (HttpWebResponse nestedWebResponse = (HttpWebResponse)webRequest.GetResponse())
                            {
                                using (var nestedReader = new StreamReader(nestedWebResponse.GetResponseStream()))
                                {
                                    js = new JavaScriptSerializer();
                                    objText = nestedReader.ReadToEnd();
                                    FriendsResponseInfo myInnerObj = (FriendsResponseInfo)js.Deserialize(objText, typeof(FriendsResponseInfo));
                                    status = myobj.status;
                                    isOkStatus = status == "OK";
                                    if (isOkStatus)
                                    {
                                        List<Friend> receivedFriends = myInnerObj.friends;
                                        List<Friend> myFriends = receivedFriends.Where<Friend>((Friend friend) =>
                                        {
                                            return friend.user == currentUserId;
                                        }).ToList<Friend>();

                                        int countFriends = myFriends.Count;

                                        List<string> friendsIds = new List<string>();
                                        foreach (Friend friendInfo in myFriends)
                                        {
                                            string friendId = friendInfo.friend;
                                            friendsIds.Add(friendId);
                                        }
                                        foreach (Friend friendInfo in myFriends)
                                        {
                                            string friendId = friendInfo.friend;
                                            if (friendsIds.Contains(friendId))
                                            {
                                                webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/users/get/?id=" + friendId);
                                                webRequest.Method = "GET";
                                                webRequest.UserAgent = ".NET Framework Test Client";
                                                using (HttpWebResponse innerWebResponse = (HttpWebResponse)webRequest.GetResponse())
                                                {
                                                    using (StreamReader innerReader = new StreamReader(innerWebResponse.GetResponseStream()))
                                                    {
                                                        js = new JavaScriptSerializer();
                                                        objText = innerReader.ReadToEnd();
                                                        myobj = (UserResponseInfo)js.Deserialize(objText, typeof(UserResponseInfo));
                                                        status = myobj.status;
                                                        isOkStatus = status == "OK";
                                                        if (isOkStatus)
                                                        {
                                                            User myFriend = myobj.user;
                                                            string name = myFriend.name;
                                                            string userStatus = myFriend.status;
                                                            bool isOnline = userStatus == "online";
                                                            if (isOnline)
                                                            {
                                                                countOnlineFriends++;
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
                }
            }
            catch (System.Net.WebException)
            {
                MessageBox.Show("Не удается подключиться к серверу", "Ошибка");
                this.Close();
            }

            string rawCountOnlineFriends = countOnlineFriends.ToString();
            string yourCountFriendsLabelContent = "Ваши друзья" + rawCountOnlineFriends + "/";
            yourCountOnlineFriendsLabel.Text = yourCountFriendsLabelContent;

        }

        public void OpenChatHandler(object sender, RoutedEventArgs e)
        {
            OpenChat();
        }

        public void OpenChat()
        {
            object openChatBtnData = openChatBtn.DataContext;
            string id = ((string)(openChatBtnData));
            Application app = Application.Current;
            WindowCollection windows = app.Windows;
            IEnumerable<Window> myWindows = windows.OfType<Window>();
            List<Window> chatWindows = myWindows.Where<Window>(window =>
            {
                string windowTitle = window.Title;
                bool isChatWindow = windowTitle == "Чат";
                object windowData = window.DataContext;
                bool isWindowDataExists = windowData != null;
                bool isChatExists = true;
                if (isWindowDataExists && isChatWindow)
                {
                    string localFriend = ((string)(windowData));
                    isChatExists = id == localFriend;
                }
                return isWindowDataExists && isChatWindow && isChatExists;
            }).ToList<Window>();
            int countChatWindows = chatWindows.Count;
            bool isNotOpenedChatWindows = countChatWindows <= 0;
            if (isNotOpenedChatWindows)
            {
                Dialogs.ChatDialog dialog = new Dialogs.ChatDialog(currentUserId, client, id, false);
                dialog.Show();
            }
        }

        public void GetConnectedFriends()
        {
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/users/get/?id=" + currentUserId);
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (var reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        var objText = reader.ReadToEnd();

                        UserResponseInfo myobj = (UserResponseInfo)js.Deserialize(objText, typeof(UserResponseInfo));

                        string status = myobj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
                            webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/friends/get");
                            webRequest.Method = "GET";
                            webRequest.UserAgent = ".NET Framework Test Client";
                            using (HttpWebResponse nestedWebResponse = (HttpWebResponse)webRequest.GetResponse())
                            {
                                using (var nestedReader = new StreamReader(nestedWebResponse.GetResponseStream()))
                                {
                                    js = new JavaScriptSerializer();
                                    objText = nestedReader.ReadToEnd();
                                    FriendsResponseInfo myInnerObj = (FriendsResponseInfo)js.Deserialize(objText, typeof(FriendsResponseInfo));
                                    status = myobj.status;
                                    isOkStatus = status == "OK";
                                    onlineFriendsList.Children.Clear();
                                    if (isOkStatus)
                                    {
                                        List<Friend> receivedFriends = myInnerObj.friends;
                                        List<Friend> myFriends = receivedFriends.Where<Friend>((Friend friend) =>
                                        {
                                            return friend.user == currentUserId;
                                        }).ToList<Friend>();

                                        int countFriends = myFriends.Count;

                                        List<string> friendsIds = new List<string>();
                                        foreach (Friend friendInfo in myFriends)
                                        {
                                            string friendId = friendInfo.friend;
                                            friendsIds.Add(friendId);
                                        }
                                        foreach (Friend friendInfo in myFriends)
                                        {
                                            string friendId = friendInfo.friend;
                                            if (friendsIds.Contains(friendId))
                                            {
                                                webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/users/get/?id=" + friendId);
                                                webRequest.Method = "GET";
                                                webRequest.UserAgent = ".NET Framework Test Client";
                                                using (HttpWebResponse innerWebResponse = (HttpWebResponse)webRequest.GetResponse())
                                                {
                                                    using (StreamReader innerReader = new StreamReader(innerWebResponse.GetResponseStream()))
                                                    {
                                                        js = new JavaScriptSerializer();
                                                        objText = innerReader.ReadToEnd();
                                                        myobj = (UserResponseInfo)js.Deserialize(objText, typeof(UserResponseInfo));
                                                        status = myobj.status;
                                                        isOkStatus = status == "OK";
                                                        if (isOkStatus)
                                                        {
                                                            User myFriend = myobj.user;
                                                            string name = myFriend.name;
                                                            string userStatus = myFriend.status;
                                                            bool isOnline = userStatus == "online";
                                                            if (isOnline)
                                                            {
                                                                string senderId = myFriend._id;
                                                                string senderName = myFriend.name;
                                                                string insensitiveCaseSenderName = senderName.ToLower();
                                                                string friendBoxContent = friendBox.Text;
                                                                string insensitiveCaseKeywords = friendBoxContent.ToLower();
                                                                bool isFriendFound = insensitiveCaseSenderName.Contains(insensitiveCaseKeywords);
                                                                int insensitiveCaseKeywordsLength = insensitiveCaseKeywords.Length;
                                                                bool isFilterDisabled = insensitiveCaseKeywordsLength <= 0;
                                                                bool isRequestMatch = isFriendFound || isFilterDisabled;
                                                                if (isRequestMatch)
                                                                {
                                                                    StackPanel friend = new StackPanel();
                                                                    friend.Margin = new Thickness(15);
                                                                    friend.Width = 250;
                                                                    friend.Height = 50;
                                                                    friend.Orientation = Orientation.Horizontal;
                                                                    friend.Background = System.Windows.Media.Brushes.DarkCyan;
                                                                    Image friendIcon = new Image();
                                                                    friendIcon.Width = 50;
                                                                    friendIcon.Height = 50;
                                                                    friendIcon.BeginInit();
                                                                    friendIcon.Source = new BitmapImage(new Uri(@"https://cdn2.iconfinder.com/data/icons/ios-7-icons/50/user_male-128.png"));
                                                                    friendIcon.EndInit();
                                                                    friendIcon.ImageFailed += SetDefautAvatarHandler;
                                                                    friend.Children.Add(friendIcon);
                                                                    Separator friendStatus = new Separator();
                                                                    friendStatus.BorderBrush = System.Windows.Media.Brushes.SkyBlue;
                                                                    friendStatus.LayoutTransform = new RotateTransform(90);
                                                                    friend.Children.Add(friendStatus);
                                                                    TextBlock friendNameLabel = new TextBlock();
                                                                    friendNameLabel.Margin = new Thickness(10, 0, 10, 0);
                                                                    friendNameLabel.VerticalAlignment = VerticalAlignment.Center;
                                                                    friendNameLabel.Width = 75;
                                                                    friendNameLabel.Text = name;
                                                                    friend.Children.Add(friendNameLabel);
                                                                    CheckBox friendCheckBox = new CheckBox();
                                                                    Visibility friendsListManagementVisibility = friendsListManagement.Visibility;
                                                                    bool isVisible = friendsListManagementVisibility == visible;
                                                                    if (isVisible)
                                                                    {
                                                                        friendCheckBox.Visibility = visible;
                                                                    }
                                                                    else
                                                                    {
                                                                        friendCheckBox.Visibility = invisible;
                                                                    }
                                                                    friendCheckBox.Margin = new Thickness(5, 15, 5, 15);
                                                                    friend.Children.Add(friendCheckBox);
                                                                    onlineFriendsList.Children.Add(friend);
                                                                    friend.DataContext = senderId;
                                                                    /*
                                                                    friend.MouseEnter += ShowFriendInfoHandler;
                                                                    friend.MouseLeave += HideFriendInfoHandler;
                                                                    */
                                                                    friend.MouseMove += ShowFriendInfoHandler;
                                                                    mainControl.DataContext = senderId;
                                                                    friend.MouseLeftButtonUp += ReturnToProfileHandler;
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
                    }
                }
            }
            catch (System.Net.WebException)
            {
                MessageBox.Show("Не удается подключиться к серверу", "Ошибка");
                this.Close();
            }
        }

        public void ShowFriendInfoHandler (object sender, RoutedEventArgs e)
        {
            StackPanel friend = ((StackPanel)(sender));
            object friendData = friend.DataContext;
            string friendId = ((string)(friendData));
            ShowFriendInfo(friendId, friend);
        }

        public void ShowFriendInfo (string friendId, StackPanel friend)
        {
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/users/get/?id=" + friendId);
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (var reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        var objText = reader.ReadToEnd();
                        UserResponseInfo myobj = (UserResponseInfo)js.Deserialize(objText, typeof(UserResponseInfo));
                        string status = myobj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
                            User user = myobj.user;
                            string userName = user.name;
                            string userStatus = user.status;
                            string userLevel = "Уровень 0";
                            friendInfoPopupAvatar.BeginInit();
                            friendInfoPopupAvatar.Source = new BitmapImage(new Uri(@"http://localhost:4000/api/user/avatar/?id=" + friendId));
                            friendInfoPopupAvatar.EndInit();
                            friendInfoPopupNameLabel.Text = userName;
                            friendInfoPopupStatusLabel.Text = userStatus;
                            friendInfoPopupLevelLabel.Text = userLevel;
                            if (friend.IsMouseOver)
                            {
                                friendInfoPopup.IsOpen = true;
                            }
                            else
                            {
                                friendInfoPopup.IsOpen = false;
                            }
                            friendInfoPopup.PlacementTarget = friend;
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

        public void HideFriendInfoHandler(object sender, RoutedEventArgs e)
        {
            HideFriendInfo();
        }

        public void HideFriendInfo()
        {
            friendInfoPopup.IsOpen = false;
        }


        public void GetOfflineFriends()
        {
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/users/get/?id=" + currentUserId);
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (var reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        var objText = reader.ReadToEnd();
                        UserResponseInfo myobj = (UserResponseInfo)js.Deserialize(objText, typeof(UserResponseInfo));
                        string status = myobj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
                            webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/friends/get");
                            webRequest.Method = "GET";
                            webRequest.UserAgent = ".NET Framework Test Client";
                            using (HttpWebResponse nestedWebResponse = (HttpWebResponse)webRequest.GetResponse())
                            {
                                using (var nestedReader = new StreamReader(nestedWebResponse.GetResponseStream()))
                                {
                                    js = new JavaScriptSerializer();
                                    objText = nestedReader.ReadToEnd();
                                    FriendsResponseInfo myInnerObj = (FriendsResponseInfo)js.Deserialize(objText, typeof(FriendsResponseInfo));
                                    status = myobj.status;
                                    isOkStatus = status == "OK";
                                    offlineFriendsList.Children.Clear();
                                    if (isOkStatus)
                                    {
                                        List<Friend> receivedFriends = myInnerObj.friends;
                                        List<Friend> myFriends = receivedFriends.Where<Friend>((Friend friend) =>
                                        {
                                            return friend.user == currentUserId;
                                        }).ToList<Friend>();
                                        List<string> friendsIds = new List<string>();
                                        foreach (Friend friendInfo in myFriends)
                                        {
                                            string friendId = friendInfo.friend;
                                            friendsIds.Add(friendId);
                                        }
                                        foreach (Friend friendInfo in myFriends)
                                        {
                                            string friendId = friendInfo.friend;
                                            if (friendsIds.Contains(friendId))
                                            {
                                                webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/users/get/?id=" + friendId);
                                                webRequest.Method = "GET";
                                                webRequest.UserAgent = ".NET Framework Test Client";
                                                using (HttpWebResponse innerWebResponse = (HttpWebResponse)webRequest.GetResponse())
                                                {
                                                    using (StreamReader innerReader = new StreamReader(innerWebResponse.GetResponseStream()))
                                                    {
                                                        js = new JavaScriptSerializer();
                                                        objText = innerReader.ReadToEnd();
                                                        myobj = (UserResponseInfo)js.Deserialize(objText, typeof(UserResponseInfo));
                                                        status = myobj.status;
                                                        isOkStatus = status == "OK";
                                                        if (isOkStatus)
                                                        {
                                                            User myFriend = myobj.user;
                                                            string name = myFriend.name;
                                                            string userStatus = myFriend.status;
                                                            bool isOffline = userStatus == "offline";
                                                            if (isOffline)
                                                            {
                                                                string senderId = myFriend._id;
                                                                string senderName = myFriend.name;
                                                                string insensitiveCaseSenderName = senderName.ToLower();
                                                                string friendBoxContent = friendBox.Text;
                                                                string insensitiveCaseKeywords = friendBoxContent.ToLower();
                                                                bool isFriendFound = insensitiveCaseSenderName.Contains(insensitiveCaseKeywords);
                                                                int insensitiveCaseKeywordsLength = insensitiveCaseKeywords.Length;
                                                                bool isFilterDisabled = insensitiveCaseKeywordsLength <= 0;
                                                                bool isRequestMatch = isFriendFound || isFilterDisabled;
                                                                if (isRequestMatch)
                                                                {
                                                                    StackPanel friend = new StackPanel();
                                                                    friend.Margin = new Thickness(15);
                                                                    friend.Width = 250;
                                                                    friend.Height = 50;
                                                                    friend.Orientation = Orientation.Horizontal;
                                                                    friend.Background = System.Windows.Media.Brushes.DarkCyan;
                                                                    Image friendIcon = new Image();
                                                                    friendIcon.Width = 50;
                                                                    friendIcon.Height = 50;
                                                                    friendIcon.BeginInit();
                                                                    friendIcon.Source = new BitmapImage(new Uri(@"https://cdn2.iconfinder.com/data/icons/ios-7-icons/50/user_male-128.png"));
                                                                    friendIcon.EndInit();
                                                                    friendIcon.ImageFailed += SetDefautAvatarHandler;
                                                                    friend.Children.Add(friendIcon);
                                                                    Separator friendStatus = new Separator();
                                                                    friendStatus.BorderBrush = System.Windows.Media.Brushes.LightGray;
                                                                    friendStatus.LayoutTransform = new RotateTransform(90);
                                                                    friend.Children.Add(friendStatus);
                                                                    TextBlock friendNameLabel = new TextBlock();
                                                                    friendNameLabel.Margin = new Thickness(10, 0, 10, 0);
                                                                    friendNameLabel.VerticalAlignment = VerticalAlignment.Center;
                                                                    friendNameLabel.Width = 75;
                                                                    friendNameLabel.Text = name;
                                                                    friend.Children.Add(friendNameLabel);
                                                                    CheckBox friendCheckBox = new CheckBox();
                                                                    Visibility friendsListManagementVisibility = friendsListManagement.Visibility;
                                                                    bool isVisible = friendsListManagementVisibility == visible;
                                                                    if (isVisible)
                                                                    {
                                                                        friendCheckBox.Visibility = visible;
                                                                    }
                                                                    else
                                                                    {
                                                                        friendCheckBox.Visibility = invisible;
                                                                    }
                                                                    friendCheckBox.Margin = new Thickness(5, 15, 5, 15);
                                                                    friend.Children.Add(friendCheckBox);
                                                                    offlineFriendsList.Children.Add(friend);
                                                                    friend.DataContext = senderId;
                                                                    /*
                                                                    friend.MouseEnter += ShowFriendInfoHandler;
                                                                    friend.MouseLeave += HideFriendInfoHandler;
                                                                    */
                                                                    friend.MouseMove += ShowFriendInfoHandler;
                                                                    mainControl.DataContext = senderId;
                                                                    friend.MouseLeftButtonUp += ReturnToProfileHandler;
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
                    }
                }
            }
            catch (System.Net.WebException)
            {
                MessageBox.Show("Не удается подключиться к серверу", "Ошибка");
                this.Close();
            }
        }

        public void GetFriendsSettings()
        {

            GetFriends();

            GetTotalFriendsCount();

            GetOnlineFriendsCount();

            GetFriendRequestsForMe();

            GetFriendRequestsFromMe();

            GetUserName();

            GetGroups();

            GetGroupRequestsForMe();

            GetSearchedGroups();

        }

        public void GetSearchedGroups()
        {
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/groups/all");
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (var reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        var objText = reader.ReadToEnd();
                        GroupsResponseInfo myObj = (GroupsResponseInfo)js.Deserialize(objText, typeof(GroupsResponseInfo));
                        string status = myObj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
                            searchedGroups.Children.Clear();
                            List<Group> totalGroups = myObj.groups;
                            foreach (Group group in totalGroups)
                            {
                                string groupId = group._id;
                                string name = group.name;
                                string insensitiveCaseName = name.ToLower();
                                string searchedGroupsBoxContent = searchedGroupsBox.Text;
                                string insensitiveCaseSearchedGroupsBoxContent = searchedGroupsBoxContent.ToLower();
                                int insensitiveCaseNameLength = insensitiveCaseName.Length;
                                bool isMatch = insensitiveCaseName.Contains(insensitiveCaseSearchedGroupsBoxContent);
                                bool isBoxEmpty = insensitiveCaseNameLength <= 0;
                                bool isAddGroup = isMatch || isBoxEmpty;
                                if (isAddGroup)
                                {
                                    StackPanel localGroup = new StackPanel();
                                    localGroup.Margin = new Thickness(15);
                                    localGroup.Height = 50;
                                    localGroup.Background = System.Windows.Media.Brushes.LightGray;
                                    TextBlock localGroupNameLabel = new TextBlock();
                                    localGroupNameLabel.FontSize = 20;
                                    localGroupNameLabel.Margin = new Thickness(15);
                                    localGroupNameLabel.Text = name;
                                    localGroup.Children.Add(localGroupNameLabel);
                                    searchedGroups.Children.Add(localGroup);
                                    localGroup.DataContext = groupId;
                                    localGroup.MouseLeftButtonUp += SelectGroupHandler;
                                }
                            }
                        }
                    }
                }
            }
            catch (System.Net.WebException exception)
            {
                MessageBox.Show("Не удается подключиться к серверу", "Ошибка");
                this.Close();
            }
        }

        public void OpenAddIllustrationPageHandler(object sender, RoutedEventArgs e)
        {
            OpenAddIllustrationPage();
        }

        public void OpenAddIllustrationPage()
        {
            mainControl.SelectedIndex = 23;
        }

        public void OpenAddManualPageHandler(object sender, RoutedEventArgs e)
        {
            OpenAddManualPage();
        }

        public void OpenAddManualPage ()
        {
            mainControl.SelectedIndex = 21;
        }

        public void AddManualHandler (object sender, RoutedEventArgs e)
        {
            AddManual();
        }

        public void AddManual ()
        {
            try
            {
                string manualNameBoxContent = manualNameBox.Text;
                string manualDescBoxContent = manualDescBox.Text;
                string manualLang = "";
                object rawIsChecked = manualLangRuBtn.IsChecked;
                bool isChecked = ((bool)(rawIsChecked));
                if (isChecked)
                {
                    manualLang = "русский";
                }
                rawIsChecked = manualLangEngBtn.IsChecked;
                isChecked = ((bool)(rawIsChecked));
                if (isChecked)
                {
                    manualLang = "english";
                }
                string manualCategories = "";
                rawIsChecked = manualCategoriesAchievementsBox.IsChecked;
                isChecked = ((bool)(rawIsChecked));
                if (isChecked)
                {
                    int manualCategoriesLength = manualCategories.Length;
                    bool isManualCategoriesLengthExists = manualCategoriesLength >= 1;
                    if (isManualCategoriesLengthExists)
                    {
                        manualCategories += "|";
                    }
                    manualCategories += "Достижения";
                }
                rawIsChecked = manualCategoriesAchievementsBox.IsChecked;
                isChecked = ((bool)(rawIsChecked));
                if (isChecked)
                {
                    int manualCategoriesLength = manualCategories.Length;
                    bool isManualCategoriesLengthExists = manualCategoriesLength >= 1;
                    if (isManualCategoriesLengthExists)
                    {
                        manualCategories += "|";
                    }
                    manualCategories += "Достижения";
                }
                rawIsChecked = manualCategoriesCharactersBox.IsChecked;
                isChecked = ((bool)(rawIsChecked));
                if (isChecked)
                {
                    int manualCategoriesLength = manualCategories.Length;
                    bool isManualCategoriesLengthExists = manualCategoriesLength >= 1;
                    if (isManualCategoriesLengthExists)
                    {
                        manualCategories += "|";
                    }
                    manualCategories += "Персонажи";
                }
                rawIsChecked = manualCategoriesClassesBox.IsChecked;
                isChecked = ((bool)(rawIsChecked));
                if (isChecked)
                {
                    int manualCategoriesLength = manualCategories.Length;
                    bool isManualCategoriesLengthExists = manualCategoriesLength >= 1;
                    if (isManualCategoriesLengthExists)
                    {
                        manualCategories += "|";
                    }
                    manualCategories += "Классы";
                }
                rawIsChecked = manualCategoriesCooperativeBox.IsChecked;
                isChecked = ((bool)(rawIsChecked));
                if (isChecked)
                {
                    int manualCategoriesLength = manualCategories.Length;
                    bool isManualCategoriesLengthExists = manualCategoriesLength >= 1;
                    if (isManualCategoriesLengthExists)
                    {
                        manualCategories += "|";
                    }
                    manualCategories += "Кооператив";
                }
                rawIsChecked = manualCategoriesCraftBox.IsChecked;
                isChecked = ((bool)(rawIsChecked));
                if (isChecked)
                {
                    int manualCategoriesLength = manualCategories.Length;
                    bool isManualCategoriesLengthExists = manualCategoriesLength >= 1;
                    if (isManualCategoriesLengthExists)
                    {
                        manualCategories += "|";
                    }
                    manualCategories += "Крафтинг";
                }
                rawIsChecked = manualCategoriesModesBox.IsChecked;
                isChecked = ((bool)(rawIsChecked));
                if (isChecked)
                {
                    int manualCategoriesLength = manualCategories.Length;
                    bool isManualCategoriesLengthExists = manualCategoriesLength >= 1;
                    if (isManualCategoriesLengthExists)
                    {
                        manualCategories += "|";
                    }
                    manualCategories += "Режимы игры";
                }
                rawIsChecked = manualCategoriesTutorialsBox.IsChecked;
                isChecked = ((bool)(rawIsChecked));
                if (isChecked)
                {
                    int manualCategoriesLength = manualCategories.Length;
                    bool isManualCategoriesLengthExists = manualCategoriesLength >= 1;
                    if (isManualCategoriesLengthExists)
                    {
                        manualCategories += "|";
                    }
                    manualCategories += "Основы игры";
                }
                rawIsChecked = manualCategoriesRewardsBox.IsChecked;
                isChecked = ((bool)(rawIsChecked));
                if (isChecked)
                {
                    int manualCategoriesLength = manualCategories.Length;
                    bool isManualCategoriesLengthExists = manualCategoriesLength >= 1;
                    if (isManualCategoriesLengthExists)
                    {
                        manualCategories += "|";
                    }
                    manualCategories += "Награды";
                }
                rawIsChecked = manualCategoriesMapsBox.IsChecked;
                isChecked = ((bool)(rawIsChecked));
                if (isChecked)
                {
                    int manualCategoriesLength = manualCategories.Length;
                    bool isManualCategoriesLengthExists = manualCategoriesLength >= 1;
                    if (isManualCategoriesLengthExists)
                    {
                        manualCategories += "|";
                    }
                    manualCategories += "Карты или уровни";
                }
                rawIsChecked = manualCategoriesSettingsBox.IsChecked;
                isChecked = ((bool)(rawIsChecked));
                if (isChecked)
                {
                    int manualCategoriesLength = manualCategories.Length;
                    bool isManualCategoriesLengthExists = manualCategoriesLength >= 1;
                    if (isManualCategoriesLengthExists)
                    {
                        manualCategories += "|";
                    }
                    manualCategories += "Модификации или настройки";
                }
                rawIsChecked = manualCategoriesMultiplayerBox.IsChecked;
                isChecked = ((bool)(rawIsChecked));
                if (isChecked)
                {
                    int manualCategoriesLength = manualCategories.Length;
                    bool isManualCategoriesLengthExists = manualCategoriesLength >= 1;
                    if (isManualCategoriesLengthExists)
                    {
                        manualCategories += "|";
                    }
                    manualCategories += "Мультиплеер";
                }
                rawIsChecked = manualCategoriesSecretsBox.IsChecked;
                isChecked = ((bool)(rawIsChecked));
                if (isChecked)
                {
                    int manualCategoriesLength = manualCategories.Length;
                    bool isManualCategoriesLengthExists = manualCategoriesLength >= 1;
                    if (isManualCategoriesLengthExists)
                    {
                        manualCategories += "|";
                    }
                    manualCategories += "Секреты";
                }
                rawIsChecked = manualCategoriesStoryBox.IsChecked;
                isChecked = ((bool)(rawIsChecked));
                if (isChecked)
                {
                    int manualCategoriesLength = manualCategories.Length;
                    bool isManualCategoriesLengthExists = manualCategoriesLength >= 1;
                    if (isManualCategoriesLengthExists)
                    {
                        manualCategories += "|";
                    }
                    manualCategories += "Сюжет или история";
                }
                rawIsChecked = manualCategoriesTradeBox.IsChecked;
                isChecked = ((bool)(rawIsChecked));
                if (isChecked)
                {
                    int manualCategoriesLength = manualCategories.Length;
                    bool isManualCategoriesLengthExists = manualCategoriesLength >= 1;
                    if (isManualCategoriesLengthExists)
                    {
                        manualCategories += "|";
                    }
                    manualCategories += "Обмен";
                }
                rawIsChecked = manualCategoriesSpeedRunBox.IsChecked;
                isChecked = ((bool)(rawIsChecked));
                if (isChecked)
                {
                    int manualCategoriesLength = manualCategories.Length;
                    bool isManualCategoriesLengthExists = manualCategoriesLength >= 1;
                    if (isManualCategoriesLengthExists)
                    {
                        manualCategories += "|";
                    }
                    manualCategories += "Прохождения";
                }
                rawIsChecked = manualCategoriesWeaponBox.IsChecked;
                isChecked = ((bool)(rawIsChecked));
                if (isChecked)
                {
                    int manualCategoriesLength = manualCategories.Length;
                    bool isManualCategoriesLengthExists = manualCategoriesLength >= 1;
                    if (isManualCategoriesLengthExists)
                    {
                        manualCategories += "|";
                    }
                    manualCategories += "Оружие";
                }
                rawIsChecked = manualWorkShopBox.IsChecked;
                isChecked = ((bool)(rawIsChecked));
                if (isChecked)
                {
                    int manualCategoriesLength = manualCategories.Length;
                    bool isManualCategoriesLengthExists = manualCategoriesLength >= 1;
                    if (isManualCategoriesLengthExists)
                    {
                        manualCategories += "|";
                    }
                    manualCategories += "Мастерская";
                }
                rawIsChecked = drmBox.IsChecked;
                isChecked = ((bool)(rawIsChecked));
                bool isDrm = false;
                string rawIsDrm = "false";
                if (isChecked)
                {
                    isDrm = true;
                    rawIsDrm = "true";
                }
                /*HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/manuals/add/?id=" + currentUserId + @"&title=" + manualNameBoxContent + @"&desc=" + manualDescBoxContent + @"&lang=" + manualLang + @"&categories=" + manualCategories + @"&drm=" + rawIsDrm);
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (var reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        var objText = reader.ReadToEnd();
                        UserResponseInfo myobj = (UserResponseInfo)js.Deserialize(objText, typeof(UserResponseInfo));
                        string status = myobj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
                            mainControl.SelectedIndex = 20;
                            GetCommunityInfo();
                        }
                    }
                }*/

                string url = "http://localhost:4000/api/manuals/add/?id=" + currentUserId + @"&title=" + manualNameBoxContent + @"&desc=" + manualDescBoxContent + @"&lang=" + manualLang + @"&categories=" + manualCategories + @"&drm=" + rawIsDrm + @"&ext=" + manualAttachmentExt;
                HttpClient httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("User-Agent", "C# App");
                MultipartFormDataContent form = new MultipartFormDataContent();
                byte[] imagebytearraystring = manualAttachment;
                form.Add(new ByteArrayContent(imagebytearraystring, 0, imagebytearraystring.Count()), "profile_pic", "mock.png");
                HttpResponseMessage response = httpClient.PostAsync(url, form).Result;
                httpClient.Dispose();

                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/users/points/increase/?id=" + currentUserId);
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (var reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        var objText = reader.ReadToEnd();
                        UserResponseInfo myobj = (UserResponseInfo)js.Deserialize(objText, typeof(UserResponseInfo));
                        string status = myobj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
                            mainControl.SelectedIndex = 20;
                            GetCommunityInfo();
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

        public void OpenCommunityInfoHandler(object sender, RoutedEventArgs e)
        {
            OpenCommunityInfo();
        }

        public void OpenCommunityInfo()
        {
            mainControl.SelectedIndex = 20;
            GetCommunityInfo();
        }

        public void SelectGroupHandler (object sender, RoutedEventArgs e)
        {
            StackPanel group = ((StackPanel)(sender));
            object groupData = group.DataContext;
            string groupId = ((string)(groupData));
            SelectGroup(groupId);
        }

        public void SelectGroup (string groupId)
        {
            HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/groups/get/?id=" + groupId);
            webRequest.Method = "GET";
            webRequest.UserAgent = ".NET Framework Test Client";
            using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
            {
                using (var reader = new StreamReader(webResponse.GetResponseStream()))
                {
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    string objText = reader.ReadToEnd();
                    GroupResponseInfo myObj = (GroupResponseInfo)js.Deserialize(objText, typeof(GroupResponseInfo));
                    string status = myObj.status;
                    bool isOkStatus = status == "OK";
                    if (isOkStatus)
                    {
                        Group group = myObj.group;
                        string groupName = group.name;
                        DateTime groupDate = group.date;
                        string rawGroupDate = groupDate.ToLongDateString();
                        string groupLang = group.lang;
                        string groupCountry = group.country;
                        string groupFanPage = group.fanPage;
                        string groupTwitch = group.twitch;
                        string groupYotube = group.youtube;
                        groupNameLabel.Text = groupName;
                        groupDateLabel.Text = rawGroupDate;
                        groupLangLabel.Text = groupLang;
                        groupCountryLabel.Text = groupCountry;
                        groupFanPageLabel.DataContext = groupFanPage;
                        groupTwitchLabel.DataContext = groupTwitch;
                        groupYoutubeLabel.DataContext = groupYotube;
                        HttpWebRequest innerWebRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/groups/relations/all");
                        webRequest.Method = "GET";
                        webRequest.UserAgent = ".NET Framework Test Client";
                        using (HttpWebResponse innerWebResponse = (HttpWebResponse)innerWebRequest.GetResponse())
                        {
                            using (var innerReader = new StreamReader(innerWebResponse.GetResponseStream()))
                            {
                                js = new JavaScriptSerializer();
                                objText = innerReader.ReadToEnd();
                                GroupRelationsResponseInfo myInnerObj = (GroupRelationsResponseInfo)js.Deserialize(objText, typeof(GroupRelationsResponseInfo));

                                status = myInnerObj.status;
                                isOkStatus = status == "OK";
                                if (isOkStatus)
                                {
                                    List<GroupRelation> relations = myInnerObj.relations;
                                    int countGroupUsers = 0;
                                    List<string> relationGroupUserIds = new List<string>();
                                    foreach (GroupRelation relation in relations)
                                    {
                                        string relationUser = relation.user;
                                        string relationGroup = relation.group;
                                        bool isGroupUser = relationGroup == groupId;
                                        if (isGroupUser)
                                        {
                                            countGroupUsers++;
                                            relationGroupUserIds.Add(relationUser);
                                        }
                                    }
                                    string rawCountGroupUsers = countGroupUsers.ToString();
                                    string newLine = Environment.NewLine;
                                    string countGroupUsersLabelContent = rawCountGroupUsers + newLine + "участники";
                                    countGroupUsersLabel.Text = countGroupUsersLabelContent;
                                    bool isMyUserInGroup = relationGroupUserIds.Contains(currentUserId);
                                    bool isMyUserNotInGroup = !isMyUserInGroup;
                                    if (isMyUserNotInGroup)
                                    {
                                        groupJoinBtn.Content = "Присоединиться";
                                        groupJoinBtn.Click -= ExitFromGroupHandler;
                                        groupJoinBtn.Click += JoinToGroupHandler;
                                    }
                                    else
                                    {
                                        groupJoinBtn.Content = "Выйти";
                                        groupJoinBtn.Click -= JoinToGroupHandler;
                                        groupJoinBtn.Click += ExitFromGroupHandler;
                                    }
                                }
                            }
                        }
                        cachedGroupId = groupId;
                        mainControl.SelectedIndex = 19;
                    }
                }
            }
        }

        public void ExitFromGroupHandler (object sender, RoutedEventArgs e)
        {
            ExitFromGroup();
        }

        public void ExitFromGroup ()
        {
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/groups/relations/remove/?id=" + currentUserId + @"&group=" + cachedGroupId);
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (var reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        var objText = reader.ReadToEnd();
                        UserResponseInfo myobj = (UserResponseInfo)js.Deserialize(objText, typeof(UserResponseInfo));
                        string status = myobj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
                            string msgContent = "Вы были успешно удалены из группы";
                            GetGroupRequests();
                            GetFriendsSettings();
                            SelectGroup(cachedGroupId);
                            MessageBox.Show(msgContent, "Внимание");
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

        public void JoinToGroupHandler (object sender, RoutedEventArgs e)
        {
            JoinToGroup();
        }

        public void JoinToGroup ()
        {
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/groups/relations/add/?id=" + cachedGroupId + @"&user=" + currentUserId + "&request=" + "mockId");
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (var reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        var objText = reader.ReadToEnd();

                        UserResponseInfo myobj = (UserResponseInfo)js.Deserialize(objText, typeof(UserResponseInfo));

                        string status = myobj.status;
                        bool isOkStatus = status == "OK";
                        HttpWebRequest innerWebRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/groups/get/?id=" + cachedGroupId);
                        innerWebRequest.Method = "GET";
                        innerWebRequest.UserAgent = ".NET Framework Test Client";
                        using (HttpWebResponse innerWebResponse = (HttpWebResponse)innerWebRequest.GetResponse())
                        {
                            using (StreamReader innerReader = new StreamReader(innerWebResponse.GetResponseStream()))
                            {
                                js = new JavaScriptSerializer();
                                objText = innerReader.ReadToEnd();
                                GroupResponseInfo myInnerObj = (GroupResponseInfo)js.Deserialize(objText, typeof(GroupResponseInfo));
                                status = myInnerObj.status;
                                isOkStatus = status == "OK";
                                if (isOkStatus)
                                {
                                    Group group = myInnerObj.group;
                                    string groupName = group.name;
                                    string msgContent = "Вы были успешно добавлены в группу " + groupName;
                                    GetGroupRequests();
                                    GetFriendsSettings();
                                    SelectGroup(cachedGroupId);
                                    MessageBox.Show(msgContent, "Внимание");
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
            TextBlock label = ((TextBlock)(sender));
            object labelData = label.DataContext;
            string link = ((string)(labelData));
            OpenLink(link);
        }

        public void OpenLink(string link)
        {
            System.Diagnostics.Process.Start(new ProcessStartInfo
            {
                FileName = link,
                UseShellExecute = true
            });
        }

        public void GetGroups()
        {
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/groups/all");
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (var reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        var objText = reader.ReadToEnd();
                        GroupsResponseInfo myObj = (GroupsResponseInfo)js.Deserialize(objText, typeof(GroupsResponseInfo));
                        string status = myObj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
                            groups.Children.Clear();
                            List<Group> totalGroups = myObj.groups;
                            foreach (Group group in totalGroups)
                            {
                                string groupId = group._id;
                                string owner = group.owner;
                                string name = group.name;
                                string insensitiveCaseName = name.ToLower();
                                string groupsBoxContent = groupsBox.Text;
                                string insensitiveCaseGroupsBoxContent = groupsBoxContent.ToLower();
                                int insensitiveCaseNameLength = insensitiveCaseName.Length;
                                bool isMatch = insensitiveCaseName.Contains(insensitiveCaseGroupsBoxContent);
                                bool isBoxEmpty = insensitiveCaseNameLength <= 0;
                                bool isMyGroup = owner == currentUserId;
                                bool isAddGroup = (isMatch || isBoxEmpty) && isMyGroup;
                                if (isAddGroup)
                                {
                                    StackPanel localGroup = new StackPanel();
                                    localGroup.Margin = new Thickness(15);
                                    localGroup.Height = 50;
                                    localGroup.Background = System.Windows.Media.Brushes.LightGray;
                                    TextBlock localGroupNameLabel = new TextBlock();
                                    localGroupNameLabel.FontSize = 20;
                                    localGroupNameLabel.Margin = new Thickness(15);
                                    localGroupNameLabel.Text = name;
                                    localGroup.Children.Add(localGroupNameLabel);
                                    groups.Children.Add(localGroup);
                                    localGroup.DataContext = groupId;
                                    localGroup.MouseLeftButtonUp += SelectGroupHandler;
                                }
                            }
                        }
                    }
                }
            }
            catch (System.Net.WebException exception)
            {
                MessageBox.Show("Не удается подключиться к серверу", "Ошибка");
                this.Close();
            }
        }

        public void GetUserName()
        {
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/users/get?id=" + currentUserId);
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (var reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        var objText = reader.ReadToEnd();
                        UserResponseInfo myObj = (UserResponseInfo)js.Deserialize(objText, typeof(UserResponseInfo));
                        string status = myObj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
                            User user = myObj.user;
                            string userName = user.name;
                            friendsSettingsUserNameLabel.Text = userName;
                        }
                    }
                }
            }
            catch (System.Net.WebException exception)
            {
                Debugger.Log(0, "debug", "friend requests: " + exception.Message);
                MessageBox.Show("Не удается подключиться к серверу", "Ошибка");
                this.Close();
            }

        }

        public void GetFriendRequestsFromMe()
        {
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/friends/requests/get/?id=" + currentUserId);
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (var reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        var objText = reader.ReadToEnd();
                        FriendRequestsResponseInfo myobj = (FriendRequestsResponseInfo)js.Deserialize(objText, typeof(FriendRequestsResponseInfo));
                        string status = myobj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
                            List<FriendRequest> myRequests = new List<FriendRequest>();
                            List<FriendRequest> requests = myobj.requests;
                            foreach (FriendRequest request in requests)
                            {
                                string recepientId = request.friend;
                                bool isRequestForMe = currentUserId == recepientId;
                                bool isMyRequest = !isRequestForMe;
                                if (isMyRequest)
                                {
                                    myRequests.Add(request);
                                }
                            }
                            friendRequestsFromMe.Children.Clear();
                            int countRequestsFromMe = myRequests.Count;
                            bool isHaveRequests = countRequestsFromMe >= 1;
                            if (isHaveRequests)
                            {

                                foreach (FriendRequest myRequest in myRequests)
                                {
                                    string senderId = myRequest.user;
                                    HttpWebRequest innerWebRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/users/get/?id=" + senderId);
                                    innerWebRequest.Method = "GET";
                                    innerWebRequest.UserAgent = ".NET Framework Test Client";
                                    using (HttpWebResponse innerWebResponse = (HttpWebResponse)innerWebRequest.GetResponse())
                                    {
                                        using (var innerReader = new StreamReader(innerWebResponse.GetResponseStream()))
                                        {
                                            js = new JavaScriptSerializer();
                                            objText = innerReader.ReadToEnd();
                                            UserResponseInfo myInnerObj = (UserResponseInfo)js.Deserialize(objText, typeof(UserResponseInfo));
                                            status = myInnerObj.status;
                                            isOkStatus = status == "OK";
                                            if (isOkStatus)
                                            {
                                                User user = myInnerObj.user;
                                                string senderLogin = user.login;
                                                string senderName = user.name;
                                                string insensitiveCaseSenderName = senderName.ToLower();
                                                string friendRequestsFromMeBoxContent = friendRequestsFromMeBox.Text;
                                                string insensitiveCaseKeywords = friendRequestsFromMeBoxContent.ToLower();
                                                bool isFriendFound = insensitiveCaseSenderName.Contains(insensitiveCaseKeywords);
                                                int insensitiveCaseKeywordsLength = insensitiveCaseKeywords.Length;
                                                bool isFilterDisabled = insensitiveCaseKeywordsLength <= 0;
                                                bool isRequestMatch = isFriendFound || isFilterDisabled;
                                                if (isRequestMatch)
                                                {
                                                    StackPanel friend = new StackPanel();
                                                    friend.Margin = new Thickness(15);
                                                    friend.Width = 250;
                                                    friend.Height = 50;
                                                    friend.Orientation = Orientation.Horizontal;
                                                    friend.Background = System.Windows.Media.Brushes.DarkCyan;
                                                    Image friendIcon = new Image();
                                                    friendIcon.Width = 50;
                                                    friendIcon.Height = 50;
                                                    friendIcon.BeginInit();
                                                    friendIcon.Source = new BitmapImage(new Uri(@"https://cdn2.iconfinder.com/data/icons/ios-7-icons/50/user_male-128.png"));
                                                    friendIcon.EndInit();
                                                    friendIcon.ImageFailed += SetDefautAvatarHandler;
                                                    friend.Children.Add(friendIcon);
                                                    Separator friendStatus = new Separator();
                                                    friendStatus.BorderBrush = System.Windows.Media.Brushes.LightGray;
                                                    friendStatus.LayoutTransform = new RotateTransform(90);
                                                    friend.Children.Add(friendStatus);
                                                    TextBlock friendNameLabel = new TextBlock();
                                                    friendNameLabel.Margin = new Thickness(10, 0, 10, 0);
                                                    friendNameLabel.VerticalAlignment = VerticalAlignment.Center;
                                                    friendNameLabel.Text = senderLogin;
                                                    friend.Children.Add(friendNameLabel);
                                                    friendRequestsFromMe.Children.Add(friend);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                TextBlock requestsNotFoundLabel = new TextBlock();
                                requestsNotFoundLabel.Margin = new Thickness(15);
                                requestsNotFoundLabel.FontSize = 18;
                                requestsNotFoundLabel.Text = "Извините, здесь ничего нет.";
                                friendRequestsFromMe.Children.Add(requestsNotFoundLabel);
                            }
                        }
                        else
                        {
                            CloseManager();
                        }
                    }
                }
            }
            catch (System.Net.WebException exception)
            {
                Debugger.Log(0, "debug", "friend requests: " + exception.Message);
                MessageBox.Show("Не удается подключиться к серверу", "Ошибка");
                this.Close();
            }
        }

        public void PreInit(string id)
        {
            Environment.SpecialFolder localApplicationDataFolder = Environment.SpecialFolder.LocalApplicationData;
            string localApplicationDataFolderPath = Environment.GetFolderPath(localApplicationDataFolder);
            string saveDataFilePath = localApplicationDataFolderPath + @"\OfficeWare\GameManager\" + id + @"\save-data.txt";
            string cachePath = localApplicationDataFolderPath + @"\OfficeWare\GameManager\" + id;
            bool isCacheFolderExists = Directory.Exists(cachePath);
            if (isCacheFolderExists)
            {
                JavaScriptSerializer js = new JavaScriptSerializer();
                string saveDataFileContent = File.ReadAllText(saveDataFilePath);
                SavedContent loadedContent = js.Deserialize<SavedContent>(saveDataFileContent);
                Settings currentSettings = loadedContent.settings;
                string lang = currentSettings.language;
                System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(lang);
            }
        }

        public void Initialize(string id)
        {
            GetUser(id);
            InitConstants();
            ShowOffers();
            GetGamesList("");
            GetFriendRequests();
            GetGamesInfo();
            GetUserInfo(currentUserId, true);
            GetEditInfo();
            GetGamesStats();
            CheckFriendsCache();
            LoadStartWindow();
            GetOnlineFriends();
            GetDownloads();
            GetContent();
            GetForums("");
            GetGameCollections();
            GetFriendsSettings();
            GetGroupRequests();
            GetRequestsCount();
            GetComments(currentUserId);
            GetCommunityInfo();
            InitializeTray();
            GetExperiments();
            GetAccountSettings();
            InitMail();
            GetFamilyView();
            GetIcons();/**/
        }

        public void GetFamilyView ()
        {
            Environment.SpecialFolder localApplicationDataFolder = Environment.SpecialFolder.LocalApplicationData;
            string localApplicationDataFolderPath = Environment.GetFolderPath(localApplicationDataFolder);
            string saveDataFilePath = localApplicationDataFolderPath + @"\OfficeWare\GameManager\" + currentUserId + @"\save-data.txt";
            JavaScriptSerializer js = new JavaScriptSerializer();
            string saveDataFileContent = File.ReadAllText(saveDataFilePath);
            SavedContent loadedContent = js.Deserialize<SavedContent>(saveDataFileContent);
            Settings currentSettings = loadedContent.settings;
            bool isFamilyViewEnabled = currentSettings.familyView;
            if (isFamilyViewEnabled)
            {
                familyViewIcon.Visibility = visible;
                moreInfoFamilyViewBtn.Visibility = invisible;
                disableFamilyViewBtn.Visibility = visible;
            }
            else
            {
                familyViewIcon.Visibility = invisible;
                moreInfoFamilyViewBtn.Visibility = visible;
                disableFamilyViewBtn.Visibility = invisible;
            }
        }

        public void InitMail()
        {
            mailClient = new ImapClient("imap.gmail.com", true);
            bool isConnected = mailClient.Connect();
            bool isNotConnected = !isConnected;
            if (isNotConnected)
            {
                ClientClosed();
                this.Close();
            }
            else
            {
                bool isLogined = mailClient.Login(@"glebdyakov2000@gmail.com", @"ttolpqpdzbigrkhz");
                bool isNotLogined = !isLogined;
                if (isNotConnected)
                {
                    ClientClosed();
                    this.Close();
                }
            }
        }

    public void GetContent ()
        {
            GetScreenShots("", true);
            GetIllustrationsContent();
            GetVideoContent();
            GetStoreContent();
            GetCollectionsContent();
            GetManualsContent();
        }

        public void GetIllustrationsContent ()
        {
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/illustrations/all");
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (var reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        var objText = reader.ReadToEnd();
                        IllustrationsResponseInfo myobj = (IllustrationsResponseInfo)js.Deserialize(objText, typeof(IllustrationsResponseInfo));
                        string status = myobj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
                            List<Illustration> totalIllustrations = myobj.illustrations;
                            totalIllustrations = totalIllustrations.Where<Illustration>((Illustration content) =>
                            {
                                string userId = content.user;
                                bool isMyContent = userId == currentUserId;
                                return isMyContent;
                            }).ToList<Illustration>();
                            contentIllustrations.Children.Clear();
                            int totalIllustrationsCount = totalIllustrations.Count;
                            bool isHaveIllustrations = totalIllustrationsCount >= 1;
                            if (isHaveIllustrations)
                            {
                                contentIllustrations.HorizontalAlignment = HorizontalAlignment.Left;
                                foreach (Illustration totalIllustrationsItem in totalIllustrations)
                                {
                                    string id = totalIllustrationsItem._id;
                                    string userId = totalIllustrationsItem.user;
                                    bool isMyContent = userId == currentUserId;
                                    if (isMyContent)
                                    {
                                        string title = totalIllustrationsItem.title;
                                        string desc = totalIllustrationsItem.desc;
                                        StackPanel illustration = new StackPanel();
                                        illustration.Width = 500;
                                        illustration.Margin = new Thickness(15);
                                        illustration.Background = System.Windows.Media.Brushes.LightGray;
                                        TextBlock illustrationTitleLabel = new TextBlock();
                                        illustrationTitleLabel.FontSize = 16;
                                        illustrationTitleLabel.Margin = new Thickness(15);
                                        illustrationTitleLabel.Text = title;
                                        illustration.Children.Add(illustrationTitleLabel);
                                        Image illustrationPhoto = new Image();
                                        illustrationPhoto.Margin = new Thickness(15);
                                        illustrationPhoto.HorizontalAlignment = HorizontalAlignment.Left;
                                        illustrationPhoto.Width = 50;
                                        illustrationPhoto.Height = 50;
                                        illustrationPhoto.BeginInit();
                                        illustrationPhoto.Source = new BitmapImage(new Uri(@"http://localhost:4000/api/illustration/photo/?id=" + id));
                                        illustrationPhoto.EndInit();
                                        illustration.Children.Add(illustrationPhoto);
                                        TextBlock illustrationDescLabel = new TextBlock();
                                        illustrationDescLabel.Margin = new Thickness(15);
                                        illustrationDescLabel.Text = desc;
                                        illustration.Children.Add(illustrationDescLabel);
                                        contentIllustrations.Children.Add(illustration);
                                        illustration.DataContext = id;
                                        illustration.MouseLeftButtonUp += SelectIllustrationHandler;
                                    }
                                }
                            }
                            else
                            {
                                StackPanel notFound = new StackPanel();
                                notFound.Margin = new Thickness(0, 15, 0, 15);
                                TextBlock notFoundLabel = new TextBlock();
                                notFoundLabel.HorizontalAlignment = HorizontalAlignment.Center;
                                notFoundLabel.TextAlignment = TextAlignment.Center;
                                notFoundLabel.FontSize = 18;
                                string newLine = Environment.NewLine;
                                notFoundLabel.Text = "На данный момент у вас нет иллюстраций, хранящихся в Steam" + newLine + "Cloud. Чтобы загрузить иллюстрацию, перейдите во вкладку" + newLine + "«Иллюстрации» в центре сообщества игры и найдите кнопку" + newLine + "«Загрузить иллюстрацию» справа вверху.";
                                notFound.Children.Add(notFoundLabel);
                                TextBlock notFoundSubLabel = new TextBlock();
                                notFoundSubLabel.HorizontalAlignment = HorizontalAlignment.Center;
                                notFoundSubLabel.TextAlignment = TextAlignment.Center;
                                notFoundSubLabel.FontSize = 18;
                                notFoundSubLabel.Text = "Или нажмите кнопку «Загрузить иллюстрацию» ниже.";
                                notFound.Children.Add(notFoundSubLabel);
                                Button uploadBtn = new Button();
                                uploadBtn.Width = 175;
                                uploadBtn.Height = 25;
                                uploadBtn.Content = "Загрузить иллюстрацию";
                                notFound.Children.Add(uploadBtn);
                                contentIllustrations.HorizontalAlignment = HorizontalAlignment.Center;
                                contentIllustrations.Children.Add(notFound);
                            }
                        }
                    }
                }
            }
            catch (System.Net.WebException exception)
            {
                MessageBox.Show("Не удается подключиться к серверу", "Ошибка");
                this.Close();
            }
        }

        public void GetVideoContent()
        {

        }
        public void GetStoreContent()
        {

        }
        public void GetCollectionsContent()
        {

        }

        public void GetManualsContent()
        {
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/manuals/all");
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (var reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        var objText = reader.ReadToEnd();
                        ManualsResponseInfo myobj = (ManualsResponseInfo)js.Deserialize(objText, typeof(ManualsResponseInfo));
                        string status = myobj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
                            List<Manual> totalManuals = myobj.manuals;
                            totalManuals = totalManuals.Where<Manual>((Manual content) =>
                            {
                                string userId = content.user;
                                bool isMyContent = userId == currentUserId;
                                return isMyContent;
                            }).ToList<Manual>();
                            contentManuals.Children.Clear();
                            int totalManualsCount = totalManuals.Count;
                            bool isHaveManuals = totalManualsCount >= 1;
                            if (isHaveManuals)
                            {
                                contentManuals.HorizontalAlignment = HorizontalAlignment.Left;
                                foreach (Manual totalManualsItem in totalManuals)
                                {
                                    string id = totalManualsItem._id;
                                    string userId = totalManualsItem.user;
                                    bool isMyContent = userId == currentUserId;
                                    if (isMyContent)
                                    {
                                        string title = totalManualsItem.title;
                                        string desc = totalManualsItem.desc;
                                        StackPanel manual = new StackPanel();
                                        manual.Width = 500;
                                        manual.Margin = new Thickness(15);
                                        manual.Background = System.Windows.Media.Brushes.LightGray;
                                        TextBlock manualTitleLabel = new TextBlock();
                                        manualTitleLabel.FontSize = 16;
                                        manualTitleLabel.Margin = new Thickness(15);
                                        manualTitleLabel.Text = title;
                                        manual.Children.Add(manualTitleLabel);
                                        Image manualPhoto = new Image();
                                        manualPhoto.Margin = new Thickness(15);
                                        manualPhoto.HorizontalAlignment = HorizontalAlignment.Left;
                                        manualPhoto.Width = 50;
                                        manualPhoto.Height = 50;
                                        manualPhoto.BeginInit();
                                        manualPhoto.Source = new BitmapImage(new Uri(@"http://localhost:4000/api/manual/photo/?id=" + id));
                                        manualPhoto.EndInit();
                                        manual.Children.Add(manualPhoto);
                                        TextBlock manualDescLabel = new TextBlock();
                                        manualDescLabel.Margin = new Thickness(15);
                                        manualDescLabel.Text = desc;
                                        manual.Children.Add(manualDescLabel);
                                        contentManuals.Children.Add(manual);
                                        manual.DataContext = id;
                                        manual.MouseLeftButtonUp += SelectManualHandler;
                                    }
                                }
                            }
                            else
                            {
                                TextBlock notFoundLabel = new TextBlock();
                                notFoundLabel.HorizontalAlignment = HorizontalAlignment.Center;
                                notFoundLabel.TextAlignment = TextAlignment.Center;
                                notFoundLabel.FontSize = 18;
                                notFoundLabel.Text = "Не найдено файлов, удовлетворяющих критериям запроса пользователя.";
                                contentManuals.HorizontalAlignment = HorizontalAlignment.Center;
                                contentManuals.Children.Add(notFoundLabel);
                            }
                        }
                    }
                }
            }
            catch (System.Net.WebException exception)
            {
                MessageBox.Show("Не удается подключиться к серверу", "Ошибка");
                this.Close();
            }
        }

        public void GetExperiments ()
        {
            experiments.Children.Clear();
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/experiments/all");
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (var reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        var objText = reader.ReadToEnd();
                        ExperimentsResponseInfo myobj = (ExperimentsResponseInfo)js.Deserialize(objText, typeof(ExperimentsResponseInfo));
                        string status = myobj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
                            List<Experiment> totalExperiments = myobj.experiments;
                            foreach (Experiment totalExperimentsItem in totalExperiments)
                            {
                                string id = totalExperimentsItem._id;
                                string title = totalExperimentsItem.title;
                                string desc = totalExperimentsItem.desc;
                                StackPanel experiment = new StackPanel();
                                experiment.Background = System.Windows.Media.Brushes.LightGray;
                                experiment.Margin = new Thickness(15);
                                experiment.Width = 850;
                                experiment.Height = 350;
                                Image experimentPhoto = new Image();
                                experimentPhoto.Width = 500;
                                experimentPhoto.Height = 300;
                                experimentPhoto.BeginInit();
                                experimentPhoto.Source = new BitmapImage(new Uri(@"http://localhost:4000/api/experiment/photo/?id=" + id));
                                experimentPhoto.EndInit();
                                experiment.Children.Add(experimentPhoto);
                                TextBlock experimentTitleLabel = new TextBlock();
                                experimentTitleLabel.Margin = new Thickness(15);
                                experimentTitleLabel.FontSize = 20;
                                experimentTitleLabel.Text = title;
                                experiment.Children.Add(experimentTitleLabel);
                                TextBlock experimentDescLabel = new TextBlock();
                                experimentDescLabel.Margin = new Thickness(15);
                                experimentDescLabel.Text = desc;
                                experiment.Children.Add(experimentDescLabel);
                                experiments.Children.Add(experiment);
                                experiment.DataContext = id;
                                experiment.MouseLeftButtonUp += OpenExperimentHandler;
                            }
                        }
                    }
                }
            }
            catch (System.Net.WebException exception)
            {
                MessageBox.Show("Не удается подключиться к серверу", "Ошибка");
                this.Close();
            }
        }

        public void OpenExperimentHandler (object sender, RoutedEventArgs e)
        {
            StackPanel experiment = ((StackPanel)(sender));
            object experimentData = experiment.DataContext;
            string experimentId = ((string)(experimentData));
            OpenExperiment(experimentId);
        }

        public void OpenExperiment (string experimentId)
        {
            Environment.SpecialFolder localApplicationDataFolder = Environment.SpecialFolder.LocalApplicationData;
            string localApplicationDataFolderPath = Environment.GetFolderPath(localApplicationDataFolder);
            string appFolder = localApplicationDataFolderPath + @"\OfficeWare\GameManager\" + currentUserId + @"\";
            string wordPath = appFolder + "index.doc";
            WebClient wc = new WebClient();
            wc.Headers.Add("User-Agent: Other");   //that is the simple line!
            wc.DownloadFileAsync(new Uri(@"http://localhost:4000/api/experiment/document/?id=" + experimentId), wordPath);
            wc.DownloadFileCompleted += new AsyncCompletedEventHandler(wc_DownloadFileCompleted);
        }

        private void wc_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            Exception downloadError = e.Error;
            bool isErrorsNotFound = downloadError == null;
            if (isErrorsNotFound)
            {
                activeExperiment.Document = null;
                bool isWordOpened = doc != null;
                if (isWordOpened)
                {
                    doc.Close();
                    doc = null;
                    bool isDocsOpened = wordApplication.Documents.Count >= 1;
                    if (isDocsOpened)
                    {
                        wordApplication.Documents.Close();
                        wordApplication.Quit();
                    }
                }
                bool isDocOpened = document != null;
                if (isDocOpened)
                {
                    document.Close();
                    document = null;
                }
                Environment.SpecialFolder localApplicationDataFolder = Environment.SpecialFolder.LocalApplicationData;
                string localApplicationDataFolderPath = Environment.GetFolderPath(localApplicationDataFolder);
                string appFolder = localApplicationDataFolderPath + @"\OfficeWare\GameManager\" + currentUserId + @"\";
                string xpsPath = appFolder + "index.xps";
                string wordPath = appFolder + "index.doc";
                // Create a WordApplication and add Document to it
                wordApplication = new Microsoft.Office.Interop.Word.Application();
                wordApplication.Documents.Add(wordPath);
                doc = wordApplication.ActiveDocument;
                bool isXpsExists = File.Exists(xpsPath);
                if (isXpsExists)
                {
                    File.Delete(xpsPath);
                }
                doc.SaveAs(xpsPath, Microsoft.Office.Interop.Word.WdSaveFormat.wdFormatXPS);
                document = new System.Windows.Xps.Packaging.XpsDocument(xpsPath, FileAccess.Read);
                activeExperiment.Document = document.GetFixedDocumentSequence();
                mainControl.SelectedIndex = 30;
            }
        }

        public void InitializeTray ()
        {
            System.Windows.Forms.NotifyIcon nIcon = new System.Windows.Forms.NotifyIcon();
            nIcon.Icon = new System.Drawing.Icon(@"C:\wpf_projects\AntiVirus\AntiVirus\Assets\application_icon.ico");
            nIcon.Visible = true;
            string nIconTitle = "Office ware game manager";
            nIcon.Text = nIconTitle;
        }

        public void GetCommunityInfo ()
        {
            GetCommunityTotalContent();
            GetCommunityScreenShots();
            GetIllustrations();
            GetManuals();
            GetReviews();
        }

        public void GetReviews ()
        {
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/reviews/all");
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (var reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        var objText = reader.ReadToEnd();
                        ReviewsResponseInfo myobj = (ReviewsResponseInfo)js.Deserialize(objText, typeof(ReviewsResponseInfo));
                        string status = myobj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
                            List<Review> totalReviews = myobj.reviews;
                            reviews.Children.Clear();
                            int totalReviewsCount = totalReviews.Count;
                            bool isHaveReviews = totalReviewsCount >= 1;
                            if (isHaveReviews)
                            {
                                reviews.HorizontalAlignment = HorizontalAlignment.Left;
                                foreach (Review totalReviewsItem in totalReviews)
                                {
                                    string id = totalReviewsItem._id;
                                    string desc = totalReviewsItem.desc;
                                    StackPanel review = new StackPanel();
                                    review.Width = 500;
                                    review.Margin = new Thickness(15);
                                    review.Background = System.Windows.Media.Brushes.LightGray;
                                    PackIcon reviewIcon = new PackIcon();
                                    reviewIcon.Margin = new Thickness(15);
                                    reviewIcon.HorizontalAlignment = HorizontalAlignment.Left;
                                    reviewIcon.Width = 50;
                                    reviewIcon.Height = 50;
                                    reviewIcon.Kind = PackIconKind.ThumbsUp;
                                    review.Children.Add(reviewIcon);
                                    TextBlock reviewDescLabel = new TextBlock();
                                    reviewDescLabel.Margin = new Thickness(15);
                                    reviewDescLabel.Text = desc;
                                    review.Children.Add(reviewDescLabel);
                                    reviews.Children.Add(review);
                                    review.DataContext = id;
                                    review.MouseLeftButtonUp += SelectReviewHandler;
                                }
                            }
                            else
                            {
                                TextBlock notFoundLabel = new TextBlock();
                                notFoundLabel.HorizontalAlignment = HorizontalAlignment.Center;
                                notFoundLabel.TextAlignment = TextAlignment.Center;
                                notFoundLabel.FontSize = 18;
                                notFoundLabel.Text = "Обзоров не найдено";
                                reviews.HorizontalAlignment = HorizontalAlignment.Center;
                                reviews.Children.Add(notFoundLabel);
                            }
                        }
                    }
                }
            }
            catch (System.Net.WebException exception)
            {
                MessageBox.Show("Не удается подключиться к серверу", "Ошибка");
                this.Close();
            }
        }

        public void SelectReviewHandler (object sender, RoutedEventArgs e)
        {
            StackPanel review = ((StackPanel)(sender));
            object reviewData = review.DataContext;
            string reviewId = ((string)(reviewData));
            SelectReview(reviewId);
        }

        public void SelectReview (string id)
        {
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/reviews/get/?id=" + id);
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (var reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        var objText = reader.ReadToEnd();
                        ReviewResponseInfo myobj = (ReviewResponseInfo)js.Deserialize(objText, typeof(ReviewResponseInfo));
                        string status = myobj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
                            Review review = myobj.review;
                            string userId = review.user;
                            string currentGameId = review.game;
                            string desc = review.desc;
                            DateTime date = review.date;
                            string hours = review.hours;
                            string rawDate = date.ToLongDateString();
                            HttpWebRequest innerWebRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/games/get");
                            innerWebRequest.Method = "GET";
                            innerWebRequest.UserAgent = ".NET Framework Test Client";
                            using (HttpWebResponse innerWebResponse = (HttpWebResponse)innerWebRequest.GetResponse())
                            {
                                using (var innerReader = new StreamReader(innerWebResponse.GetResponseStream()))
                                {
                                    js = new JavaScriptSerializer();
                                    objText = innerReader.ReadToEnd();
                                    GamesListResponseInfo myInnerObj = (GamesListResponseInfo)js.Deserialize(objText, typeof(GamesListResponseInfo));
                                    status = myInnerObj.status;
                                    isOkStatus = status == "OK";
                                    if (isOkStatus)
                                    {
                                        List<GameResponseInfo> games = myInnerObj.games;
                                        List<GameResponseInfo> gameResults = games.Where<GameResponseInfo>((GameResponseInfo game) =>
                                        {
                                            string gameId = game._id;
                                            bool isIdMatches = gameId == currentGameId;
                                            return isIdMatches;
                                        }).ToList<GameResponseInfo>();
                                        int countResults = gameResults.Count;
                                        bool isResultsFound = countResults >= 1;
                                        if (isResultsFound)
                                        {
                                            HttpWebRequest nestedWebRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/users/get/?id=" + userId);
                                            nestedWebRequest.Method = "GET";
                                            nestedWebRequest.UserAgent = ".NET Framework Test Client";
                                            using (HttpWebResponse nestedWebResponse = (HttpWebResponse)nestedWebRequest.GetResponse())
                                            {
                                                using (var nestedReader = new StreamReader(nestedWebResponse.GetResponseStream()))
                                                {
                                                    js = new JavaScriptSerializer();
                                                    objText = nestedReader.ReadToEnd();
                                                    UserResponseInfo myNestedObj = (UserResponseInfo)js.Deserialize(objText, typeof(UserResponseInfo));
                                                    status = myNestedObj.status;
                                                    isOkStatus = status == "OK";
                                                    if (isOkStatus)
                                                    {
                                                        User user = myNestedObj.user;
                                                        string userName = user.name;
                                                        string hoursMeasure = "часов";
                                                        string mainReviewGameHoursLabelContent = hours + " " + hoursMeasure;
                                                        mainReviewGameHoursLabel.Text = mainReviewGameHoursLabelContent;
                                                        mainReviewUserNameLabel.Text = userName;
                                                        mainReviewUserAvatar.BeginInit();
                                                        mainReviewUserAvatar.Source = new BitmapImage(new Uri(@"http://localhost:4000/api/user/avatar/?id=" + userId));
                                                        mainReviewUserAvatar.EndInit();
                                                        GameResponseInfo foundedGame = gameResults[0];
                                                        string foundedGameName = foundedGame.name;
                                                        Debugger.Log(0, "debug", Environment.NewLine + "foundedGameName: " + foundedGameName + Environment.NewLine);
                                                        mainReviewGameThumbnail.BeginInit();
                                                        // mainReviewGameThumbnail.Source = new BitmapImage(new Uri(@"http://localhost:4000/api/game/thumbnail/?name=" + foundedGameName));
                                                        mainReviewGameThumbnail.Source = new BitmapImage(new Uri(@"https://cdn3.iconfinder.com/data/icons/solid-locations-icon-set/64/Games_2-256.png"));
                                                        mainReviewGameThumbnail.EndInit();
                                                        mainReviewGameLabel.Text = foundedGameName;
                                                        mainReviewDescLabel.Text = desc;
                                                        mainReviewDateLabel.Text = rawDate;
                                                        mainControl.SelectedIndex = 27;
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
            catch (System.Net.WebException exception)
            {
                MessageBox.Show("Не удается подключиться к серверу", "Ошибка");
                this.Close();
            }
        }

        public void GetCommunityScreenShots ()
        {
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/screenshots/all");
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (var reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        var objText = reader.ReadToEnd();
                        ScreenShotsResponseInfo myobj = (ScreenShotsResponseInfo)js.Deserialize(objText, typeof(ScreenShotsResponseInfo));
                        string status = myobj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
                            List<ScreenShot> totalCommunityScreenShots = myobj.screenShots;
                            communityScreenShots.Children.Clear();
                            int totalCommunityScreenShotsCount = totalCommunityScreenShots.Count;
                            bool isHaveCommunityScreenShots = totalCommunityScreenShotsCount >= 1;
                            if (isHaveCommunityScreenShots)
                            {
                                communityScreenShots.HorizontalAlignment = HorizontalAlignment.Left;
                                foreach (ScreenShot totalCommunityScreenShotsItem in totalCommunityScreenShots)
                                {
                                    string id = totalCommunityScreenShotsItem._id;
                                    StackPanel communityScreenShot = new StackPanel();
                                    communityScreenShot.Width = 500;
                                    communityScreenShot.Margin = new Thickness(15);
                                    communityScreenShot.Background = System.Windows.Media.Brushes.LightGray;
                                    Image communityScreenShotPhoto = new Image();
                                    communityScreenShotPhoto.Margin = new Thickness(15);
                                    communityScreenShotPhoto.HorizontalAlignment = HorizontalAlignment.Left;
                                    communityScreenShotPhoto.Width = 50;
                                    communityScreenShotPhoto.Height = 50;
                                    communityScreenShotPhoto.BeginInit();
                                    communityScreenShotPhoto.Source = new BitmapImage(new Uri(@"http://localhost:4000/api/screenshot/photo/?id=" + id));
                                    communityScreenShotPhoto.EndInit();
                                    communityScreenShot.Children.Add(communityScreenShotPhoto);
                                    communityScreenShots.Children.Add(communityScreenShot);
                                    communityScreenShot.DataContext = id;
                                    communityScreenShot.MouseLeftButtonUp += SelectCommunityScreenShotHandler;
                                }
                            }
                            else
                            {
                                TextBlock notFoundLabel = new TextBlock();
                                notFoundLabel.HorizontalAlignment = HorizontalAlignment.Center;
                                notFoundLabel.TextAlignment = TextAlignment.Center;
                                notFoundLabel.FontSize = 18;
                                notFoundLabel.Text = "Скриншотов не найдено";
                                communityScreenShots.HorizontalAlignment = HorizontalAlignment.Center;
                                communityScreenShots.Children.Add(notFoundLabel);
                            }
                        }
                    }
                }
            }
            catch (System.Net.WebException exception)
            {
                MessageBox.Show("Не удается подключиться к серверу", "Ошибка");
                this.Close();
            }
        }

        public void SelectCommunityScreenShotHandler (object sender, RoutedEventArgs e)
        {
            StackPanel screenShot = ((StackPanel)(sender));
            object screenShotData = screenShot.DataContext;
            string screenShotId = ((string)(screenShotData));
            SelectCommunityScreenShot(screenShotId);
        }

        public void SelectCommunityScreenShot (string id)
        {
            mainControl.SelectedIndex = 25;
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/screenshots/get/?id=" + id);
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (var reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        var objText = reader.ReadToEnd();
                        ScreenShotResponseInfo myobj = (ScreenShotResponseInfo)js.Deserialize(objText, typeof(ScreenShotResponseInfo));
                        string status = myobj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
                            ScreenShot communityScreenShot = myobj.screenShot;
                            mainCommunityScreenShotPhoto.BeginInit();
                            mainCommunityScreenShotPhoto.Source = new BitmapImage(new Uri(@"http://localhost:4000/api/screenshot/photo/?id=" + id));
                            mainCommunityScreenShotPhoto.EndInit();
                        }
                    }
                }
            }
            catch (System.Net.WebException exception)
            {
                MessageBox.Show("Не удается подключиться к серверу", "Ошибка");
                this.Close();
            }
        }


        public void GetCommunityTotalContent()
        {
            List<UIElement> communityElements = new List<UIElement>();
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/illustrations/all");
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (var reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        var objText = reader.ReadToEnd();
                        IllustrationsResponseInfo myobj = (IllustrationsResponseInfo)js.Deserialize(objText, typeof(IllustrationsResponseInfo));
                        string status = myobj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
                            List<Illustration> totalIllustrations = myobj.illustrations;
                            foreach (Illustration totalIllustrationsItem in totalIllustrations)
                            {
                                string id = totalIllustrationsItem._id;
                                string title = totalIllustrationsItem.title;
                                string desc = totalIllustrationsItem.desc;
                                StackPanel illustration = new StackPanel();
                                illustration.Width = 500;
                                illustration.Margin = new Thickness(15);
                                illustration.Background = System.Windows.Media.Brushes.LightGray;
                                TextBlock illustrationTitleLabel = new TextBlock();
                                illustrationTitleLabel.FontSize = 16;
                                illustrationTitleLabel.Margin = new Thickness(15);
                                illustrationTitleLabel.Text = title;
                                illustration.Children.Add(illustrationTitleLabel);
                                Image illustrationPhoto = new Image();
                                illustrationPhoto.Margin = new Thickness(15);
                                illustrationPhoto.HorizontalAlignment = HorizontalAlignment.Left;
                                illustrationPhoto.Width = 50;
                                illustrationPhoto.Height = 50;
                                illustrationPhoto.BeginInit();
                                illustrationPhoto.Source = new BitmapImage(new Uri(@"http://localhost:4000/api/illustration/photo/?id=" + id));
                                illustrationPhoto.EndInit();
                                illustration.Children.Add(illustrationPhoto);
                                TextBlock illustrationDescLabel = new TextBlock();
                                illustrationDescLabel.Margin = new Thickness(15);
                                illustrationDescLabel.Text = desc;
                                illustration.Children.Add(illustrationDescLabel);
                                communityElements.Add(illustration);
                                illustration.DataContext = id;
                                illustration.MouseLeftButtonUp += SelectIllustrationHandler;
                            }
                        }
                    }
                }
            }
            catch (System.Net.WebException exception)
            {
                MessageBox.Show("Не удается подключиться к серверу", "Ошибка");
                this.Close();
            }
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/manuals/all");
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (var reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        var objText = reader.ReadToEnd();
                        ManualsResponseInfo myobj = (ManualsResponseInfo)js.Deserialize(objText, typeof(ManualsResponseInfo));
                        string status = myobj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
                            List<Manual> totalManuals = myobj.manuals;
                            foreach (Manual totalManualsItem in totalManuals)
                            {
                                string id = totalManualsItem._id;
                                string title = totalManualsItem.title;
                                string desc = totalManualsItem.desc;
                                StackPanel manual = new StackPanel();
                                manual.Width = 500;
                                manual.Margin = new Thickness(15);
                                manual.Background = System.Windows.Media.Brushes.LightGray;
                                TextBlock manualTitleLabel = new TextBlock();
                                manualTitleLabel.FontSize = 16;
                                manualTitleLabel.Margin = new Thickness(15);
                                manualTitleLabel.Text = title;
                                manual.Children.Add(manualTitleLabel);
                                Image manualPhoto = new Image();
                                manualPhoto.Margin = new Thickness(15);
                                manualPhoto.HorizontalAlignment = HorizontalAlignment.Left;
                                manualPhoto.Width = 50;
                                manualPhoto.Height = 50;
                                manualPhoto.BeginInit();
                                manualPhoto.Source = new BitmapImage(new Uri(@"http://localhost:4000/api/manual/photo/?id=" + id));
                                manualPhoto.EndInit();
                                manual.Children.Add(manualPhoto);
                                TextBlock manualDescLabel = new TextBlock();
                                manualDescLabel.Margin = new Thickness(15);
                                manualDescLabel.Text = desc;
                                manual.Children.Add(manualDescLabel);
                                communityElements.Add(manual);
                                manual.DataContext = id;
                                manual.MouseLeftButtonUp += SelectManualHandler;
                            }
                        }
                    }
                }
            }
            catch (System.Net.WebException exception)
            {
                MessageBox.Show("Не удается подключиться к серверу", "Ошибка");
                this.Close();
            }

            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/screenshots/all");
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (var reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        var objText = reader.ReadToEnd();
                        ScreenShotsResponseInfo myobj = (ScreenShotsResponseInfo)js.Deserialize(objText, typeof(ScreenShotsResponseInfo));
                        string status = myobj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
                            List<ScreenShot> totalCommunityScreenShots = myobj.screenShots;
                            communityScreenShots.Children.Clear();
                            int totalCommunityScreenShotsCount = totalCommunityScreenShots.Count;
                            bool isHaveCommunityScreenShots = totalCommunityScreenShotsCount >= 1;
                            if (isHaveCommunityScreenShots)
                            {
                                foreach (ScreenShot totalCommunityScreenShotsItem in totalCommunityScreenShots)
                                {
                                    string id = totalCommunityScreenShotsItem._id;
                                    StackPanel communityScreenShot = new StackPanel();
                                    communityScreenShot.Width = 500;
                                    communityScreenShot.Margin = new Thickness(15);
                                    communityScreenShot.Background = System.Windows.Media.Brushes.LightGray;
                                    Image communityScreenShotPhoto = new Image();
                                    communityScreenShotPhoto.Margin = new Thickness(15);
                                    communityScreenShotPhoto.HorizontalAlignment = HorizontalAlignment.Left;
                                    communityScreenShotPhoto.Width = 50;
                                    communityScreenShotPhoto.Height = 50;
                                    communityScreenShotPhoto.BeginInit();
                                    communityScreenShotPhoto.Source = new BitmapImage(new Uri(@"http://localhost:4000/api/screenshot/photo/?id=" + id));
                                    communityScreenShotPhoto.EndInit();
                                    communityScreenShot.Children.Add(communityScreenShotPhoto);
                                    communityElements.Add(communityScreenShot);
                                    communityScreenShot.DataContext = id;
                                    communityScreenShot.MouseLeftButtonUp += SelectCommunityScreenShotHandler;
                                }
                            }
                            else
                            {
                                TextBlock notFoundLabel = new TextBlock();
                                notFoundLabel.HorizontalAlignment = HorizontalAlignment.Center;
                                notFoundLabel.TextAlignment = TextAlignment.Center;
                                notFoundLabel.FontSize = 18;
                                notFoundLabel.Text = "Не найдено";
                                communityScreenShots.Children.Add(notFoundLabel);
                            }
                        }
                    }
                }
            }
            catch (System.Net.WebException exception)
            {
                MessageBox.Show("Не удается подключиться к серверу", "Ошибка");
                this.Close();
            }

            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/reviews/all");
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (var reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        var objText = reader.ReadToEnd();
                        ReviewsResponseInfo myobj = (ReviewsResponseInfo)js.Deserialize(objText, typeof(ReviewsResponseInfo));
                        string status = myobj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
                            List<Review> totalReviews = myobj.reviews;
                            reviews.Children.Clear();
                            int totalReviewsCount = totalReviews.Count;
                            bool isHaveReviews = totalReviewsCount >= 1;
                            if (isHaveReviews)
                            {
                                foreach (Review totalReviewsItem in totalReviews)
                                {
                                    string id = totalReviewsItem._id;
                                    string desc = totalReviewsItem.desc;
                                    StackPanel review = new StackPanel();
                                    review.Width = 500;
                                    review.Margin = new Thickness(15);
                                    review.Background = System.Windows.Media.Brushes.LightGray;
                                    PackIcon reviewIcon = new PackIcon();
                                    reviewIcon.Margin = new Thickness(15);
                                    reviewIcon.HorizontalAlignment = HorizontalAlignment.Left;
                                    reviewIcon.Width = 50;
                                    reviewIcon.Height = 50;
                                    reviewIcon.Kind = PackIconKind.ThumbsUp;
                                    review.Children.Add(reviewIcon);
                                    TextBlock reviewDescLabel = new TextBlock();
                                    reviewDescLabel.Margin = new Thickness(15);
                                    reviewDescLabel.Text = desc;
                                    review.Children.Add(reviewDescLabel);
                                    communityElements.Add(review);
                                    review.DataContext = id;
                                    review.MouseLeftButtonUp += SelectReviewHandler;
                                }
                            }
                            else
                            {
                                TextBlock notFoundLabel = new TextBlock();
                                notFoundLabel.HorizontalAlignment = HorizontalAlignment.Center;
                                notFoundLabel.TextAlignment = TextAlignment.Center;
                                notFoundLabel.FontSize = 18;
                                notFoundLabel.Text = "Обзоров не найдено";
                                reviews.Children.Add(notFoundLabel);
                            }
                        }
                    }
                }
            }
            catch (System.Net.WebException exception)
            {
                MessageBox.Show("Не удается подключиться к серверу", "Ошибка");
                this.Close();
            }

            communityTotalContent.Children.Clear();
            var r = new Random();
            communityElements = communityElements.OrderBy(x => r.Next()).ToList<UIElement>();
            int communityElementsCount = communityElements.Count;
            bool isHaveElements = communityElementsCount >= 1;
            if (isHaveElements)
            {
                communityTotalContent.HorizontalAlignment = HorizontalAlignment.Left;
                foreach (UIElement communityElement in communityElements)
                {
                    communityTotalContent.Children.Add(communityElement);
                }
            }
            else
            {
                TextBlock notFoundLabel = new TextBlock();
                notFoundLabel.HorizontalAlignment = HorizontalAlignment.Center;
                notFoundLabel.TextAlignment = TextAlignment.Center;
                notFoundLabel.FontSize = 18;
                notFoundLabel.Text = "Иллюстраций не найдено";
                communityTotalContent.HorizontalAlignment = HorizontalAlignment.Center;
                communityTotalContent.Children.Add(notFoundLabel);
            }
        }

        public void GetIllustrations ()
        {
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/illustrations/all");
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (var reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        var objText = reader.ReadToEnd();
                        IllustrationsResponseInfo myobj = (IllustrationsResponseInfo)js.Deserialize(objText, typeof(IllustrationsResponseInfo));
                        string status = myobj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
                            List<Illustration> totalIllustrations = myobj.illustrations;
                            illustrations.Children.Clear();
                            int totalIllustrationsCount = totalIllustrations.Count;
                            bool isHaveIllustrations = totalIllustrationsCount >= 1;
                            if (isHaveIllustrations)
                            {
                                illustrations.HorizontalAlignment = HorizontalAlignment.Left;
                                foreach (Illustration totalIllustrationsItem in totalIllustrations)
                                {
                                    string id = totalIllustrationsItem._id;
                                    string title = totalIllustrationsItem.title;
                                    string desc = totalIllustrationsItem.desc;
                                    StackPanel illustration = new StackPanel();
                                    illustration.Width = 500;
                                    illustration.Margin = new Thickness(15);
                                    illustration.Background = System.Windows.Media.Brushes.LightGray;
                                    TextBlock illustrationTitleLabel = new TextBlock();
                                    illustrationTitleLabel.FontSize = 16;
                                    illustrationTitleLabel.Margin = new Thickness(15);
                                    illustrationTitleLabel.Text = title;
                                    illustration.Children.Add(illustrationTitleLabel);
                                    Image illustrationPhoto = new Image();
                                    illustrationPhoto.Margin = new Thickness(15);
                                    illustrationPhoto.HorizontalAlignment = HorizontalAlignment.Left;
                                    illustrationPhoto.Width = 50;
                                    illustrationPhoto.Height = 50;
                                    illustrationPhoto.BeginInit();
                                    illustrationPhoto.Source = new BitmapImage(new Uri(@"http://localhost:4000/api/illustration/photo/?id=" + id));
                                    illustrationPhoto.EndInit();
                                    illustration.Children.Add(illustrationPhoto);
                                    TextBlock illustrationDescLabel = new TextBlock();
                                    illustrationDescLabel.Margin = new Thickness(15);
                                    illustrationDescLabel.Text = desc;
                                    illustration.Children.Add(illustrationDescLabel);
                                    illustrations.Children.Add(illustration);
                                    illustration.DataContext = id;
                                    illustration.MouseLeftButtonUp += SelectIllustrationHandler;
                                }
                            }
                            else
                            {
                                TextBlock notFoundLabel = new TextBlock();
                                notFoundLabel.HorizontalAlignment = HorizontalAlignment.Center;
                                notFoundLabel.TextAlignment = TextAlignment.Center;
                                notFoundLabel.FontSize = 18;
                                notFoundLabel.Text = "Иллюстраций не найдено";
                                illustrations.HorizontalAlignment = HorizontalAlignment.Center;
                                illustrations.Children.Add(notFoundLabel);
                            }
                        }
                    }
                }
            }
            catch (System.Net.WebException exception)
            {
                MessageBox.Show("Не удается подключиться к серверу", "Ошибка");
                this.Close();
            }
        }

        public void GetManuals()
        {
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/manuals/all");
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (var reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        var objText = reader.ReadToEnd();
                        ManualsResponseInfo myobj = (ManualsResponseInfo)js.Deserialize(objText, typeof(ManualsResponseInfo));
                        string status = myobj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
                            List<Manual> totalManuals = myobj.manuals;
                            manuals.Children.Clear();
                            int totalManualsCount = totalManuals.Count;
                            bool isHaveManuals = totalManualsCount >= 1;
                            if (isHaveManuals)
                            {
                                manuals.HorizontalAlignment = HorizontalAlignment.Left;
                                foreach (Manual totalManualsItem in totalManuals)
                                {
                                    string id = totalManualsItem._id;
                                    string title = totalManualsItem.title;
                                    string desc = totalManualsItem.desc;
                                    StackPanel manual = new StackPanel();
                                    manual.Width = 500;
                                    manual.Margin = new Thickness(15);
                                    manual.Background = System.Windows.Media.Brushes.LightGray;
                                    TextBlock manualTitleLabel = new TextBlock();
                                    manualTitleLabel.FontSize = 16;
                                    manualTitleLabel.Margin = new Thickness(15);
                                    manualTitleLabel.Text = title;
                                    manual.Children.Add(manualTitleLabel);
                                    Image manualPhoto = new Image();
                                    manualPhoto.Margin = new Thickness(15);
                                    manualPhoto.HorizontalAlignment = HorizontalAlignment.Left;
                                    manualPhoto.Width = 50;
                                    manualPhoto.Height = 50;
                                    manualPhoto.BeginInit();
                                    // manualPhoto.Source = new BitmapImage(new Uri(@"https://cdn2.iconfinder.com/data/icons/ios-7-icons/50/user_male-128.png"));
                                    manualPhoto.Source = new BitmapImage(new Uri(@"http://localhost:4000/api/manual/photo/?id=" + id));
                                    manualPhoto.EndInit();
                                    manual.Children.Add(manualPhoto);
                                    TextBlock manualDescLabel = new TextBlock();
                                    manualDescLabel.Margin = new Thickness(15);
                                    manualDescLabel.Text = desc;
                                    manual.Children.Add(manualDescLabel);
                                    manuals.Children.Add(manual);
                                    manual.DataContext = id;
                                    manual.MouseLeftButtonUp += SelectManualHandler;
                                }
                            }
                            else
                            {
                                TextBlock notFoundLabel = new TextBlock();
                                notFoundLabel.HorizontalAlignment = HorizontalAlignment.Center;
                                notFoundLabel.TextAlignment = TextAlignment.Center;
                                notFoundLabel.FontSize = 18;
                                notFoundLabel.Text = "Руководств не найдено";
                                manuals.HorizontalAlignment = HorizontalAlignment.Center;
                                manuals.Children.Add(notFoundLabel);
                            }
                        }
                    }
                }
            }
            catch (System.Net.WebException exception)
            {
                MessageBox.Show("Не удается подключиться к серверу", "Ошибка");
                this.Close();
            }
        }

        public void SelectManualHandler (object sender, RoutedEventArgs e)
        {
            StackPanel manual = ((StackPanel)(sender));
            object manualData = manual.DataContext;
            string manualId = ((string)(manualData));
            SelectManual(manualId);
        }

        public void SelectManual (string manualId)
        {
            mainControl.SelectedIndex = 22;
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/manuals/get/?id=" + manualId);
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (var reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        var objText = reader.ReadToEnd();
                        ManualResponseInfo myobj = (ManualResponseInfo)js.Deserialize(objText, typeof(ManualResponseInfo));
                        string status = myobj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
                            Manual manual = myobj.manual;
                            string title = manual.title;
                            string desc = manual.desc;
                            mainManualPhoto.BeginInit();
                            mainManualPhoto.Source = new BitmapImage(new Uri(@"http://localhost:4000/api/manual/photo/?id=" + manualId));
                            mainManualPhoto.EndInit();
                            mainManualTitleLabel.Text = title;
                            mainManualDescLabel.Text = desc;
                        }
                    }
                }
            }
            catch (System.Net.WebException exception)
            {
                MessageBox.Show("Не удается подключиться к серверу", "Ошибка");
                this.Close();
            }
        }

        public void SelectIllustrationHandler(object sender, RoutedEventArgs e)
        {
            StackPanel illustration = ((StackPanel)(sender));
            object illustrationData = illustration.DataContext;
            string illustrationId = ((string)(illustrationData));
            SelectIllustration(illustrationId);
        }

        public void SelectIllustration(string illustrationId)
        {
            mainControl.SelectedIndex = 24;
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/illustrations/get/?id=" + illustrationId);
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (var reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        var objText = reader.ReadToEnd();
                        IllustrationResponseInfo myobj = (IllustrationResponseInfo)js.Deserialize(objText, typeof(IllustrationResponseInfo));
                        string status = myobj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
                            Illustration illustration = myobj.illustration;
                            string title = illustration.title;
                            string desc = illustration.desc;
                            mainIllustrationPhoto.BeginInit();
                            mainIllustrationPhoto.Source = new BitmapImage(new Uri(@"http://localhost:4000/api/illustration/photo/?id=" + illustrationId));
                            mainIllustrationPhoto.EndInit();
                            mainIllustrationTitleLabel.Text = title;
                            mainIllustrationDescLabel.Text = desc;
                        }
                    }
                }
            }
            catch (System.Net.WebException exception)
            {
                MessageBox.Show("Не удается подключиться к серверу", "Ошибка");
                this.Close();
            }
        }

        public void GetGroupRequests()
        {
            foreach (Popup groupRequest in groupRequests.Children)
            {
                groupRequest.IsOpen = false;
            }
            groupRequests.Children.Clear();
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/groups/requests/all");
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (var reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        var objText = reader.ReadToEnd();
                        GroupRequestsResponseInfo myobj = (GroupRequestsResponseInfo)js.Deserialize(objText, typeof(GroupRequestsResponseInfo));
                        string status = myobj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
                            List<GroupRequest> myRequests = new List<GroupRequest>();
                            List<GroupRequest> requests = myobj.requests;
                            foreach (GroupRequest request in requests)
                            {
                                string recepientId = request.user;
                                bool isRequestForMe = currentUserId == recepientId;
                                if (isRequestForMe)
                                {
                                    myRequests.Add(request);
                                }
                            }
                            foreach (GroupRequest myRequest in myRequests)
                            {
                                string groupId = myRequest.group;
                                HttpWebRequest innerWebRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/groups/get/?id=" + groupId);
                                innerWebRequest.Method = "GET";
                                innerWebRequest.UserAgent = ".NET Framework Test Client";
                                using (HttpWebResponse innerWebResponse = (HttpWebResponse)innerWebRequest.GetResponse())
                                {
                                    using (var innerReader = new StreamReader(innerWebResponse.GetResponseStream()))
                                    {
                                        js = new JavaScriptSerializer();
                                        objText = innerReader.ReadToEnd();
                                        GroupResponseInfo myInnerObj = (GroupResponseInfo)js.Deserialize(objText, typeof(GroupResponseInfo));
                                        status = myInnerObj.status;
                                        isOkStatus = status == "OK";
                                        if (isOkStatus)
                                        {
                                            Group group = myInnerObj.group;
                                            string groupName = group.name;
                                            Popup groupRequest = new Popup();
                                            groupRequest.Placement = PlacementMode.Custom;
                                            groupRequest.CustomPopupPlacementCallback = new CustomPopupPlacementCallback(FriendRequestPlacementHandler);
                                            groupRequest.PlacementTarget = this;
                                            groupRequest.Width = 225;
                                            groupRequest.Height = 275;
                                            StackPanel groupRequestBody = new StackPanel();
                                            groupRequestBody.Background = friendRequestBackground;
                                            PackIcon closeRequestBtn = new PackIcon();
                                            closeRequestBtn.Margin = new Thickness(10);
                                            closeRequestBtn.Kind = PackIconKind.Close;
                                            closeRequestBtn.DataContext = groupRequest;
                                            closeRequestBtn.MouseLeftButtonUp += CloseGroupRequestHandler;
                                            groupRequestBody.Children.Add(closeRequestBtn);
                                            Image groupRequestBodySenderAvatar = new Image();
                                            groupRequestBodySenderAvatar.Width = 100;
                                            groupRequestBodySenderAvatar.Height = 100;
                                            groupRequestBodySenderAvatar.BeginInit();
                                            Uri groupRequestBodySenderAvatarUri = new Uri("http://localhost:4000/api/user/avatar/?id=" + currentUserId);
                                            BitmapImage groupRequestBodySenderAvatarImg = new BitmapImage(groupRequestBodySenderAvatarUri);
                                            groupRequestBodySenderAvatar.Source = groupRequestBodySenderAvatarImg;
                                            groupRequestBodySenderAvatar.EndInit();
                                            groupRequestBodySenderAvatar.ImageFailed += SetDefautAvatarHandler;
                                            groupRequestBody.Children.Add(groupRequestBodySenderAvatar);
                                            TextBlock groupRequestBodySenderLoginLabel = new TextBlock();
                                            groupRequestBodySenderLoginLabel.Margin = new Thickness(10);
                                            groupRequestBodySenderLoginLabel.Text = groupName;
                                            groupRequestBody.Children.Add(groupRequestBodySenderLoginLabel);
                                            StackPanel groupRequestBodyActions = new StackPanel();
                                            groupRequestBodyActions.Orientation = Orientation.Horizontal;
                                            Button acceptActionBtn = new Button();
                                            acceptActionBtn.Margin = new Thickness(10, 5, 10, 5);
                                            acceptActionBtn.Height = 25;
                                            acceptActionBtn.Width = 65;
                                            acceptActionBtn.Content = "Принять";
                                            string myUserId = myRequest.user;
                                            string myRequestId = myRequest._id;
                                            Dictionary<String, Object> acceptActionBtnData = new Dictionary<String, Object>();
                                            acceptActionBtnData.Add("groupId", ((string)(groupId)));
                                            acceptActionBtnData.Add("userId", ((string)(myUserId)));
                                            acceptActionBtnData.Add("requestId", ((string)(myRequestId)));
                                            acceptActionBtnData.Add("request", ((Popup)(groupRequest)));
                                            acceptActionBtn.DataContext = acceptActionBtnData;
                                            acceptActionBtn.Click += AcceptGroupRequestHandler;
                                            groupRequestBodyActions.Children.Add(acceptActionBtn);
                                            Button rejectActionBtn = new Button();
                                            rejectActionBtn.Margin = new Thickness(10, 5, 10, 5);
                                            rejectActionBtn.Height = 25;
                                            rejectActionBtn.Width = 65;
                                            rejectActionBtn.Content = "Отклонить";
                                            Dictionary<String, Object> rejectActionBtnData = new Dictionary<String, Object>();
                                            rejectActionBtnData.Add("groupId", ((string)(groupId)));
                                            rejectActionBtnData.Add("userId", ((string)(myUserId)));
                                            rejectActionBtnData.Add("requestId", ((string)(myRequestId)));
                                            rejectActionBtnData.Add("request", ((Popup)(groupRequest)));
                                            rejectActionBtn.DataContext = rejectActionBtnData;
                                            rejectActionBtn.Click += RejectGroupRequestHandler;
                                            groupRequestBodyActions.Children.Add(rejectActionBtn);
                                            groupRequestBody.Children.Add(groupRequestBodyActions);
                                            groupRequest.Child = groupRequestBody;
                                            groupRequests.Children.Add(groupRequest);
                                            groupRequest.IsOpen = true;
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            CloseManager();
                        }
                    }
                }
            }
            catch (System.Net.WebException exception)
            {
                MessageBox.Show("Не удается подключиться к серверу", "Ошибка");
                this.Close();
            }
        }

        public void RejectGroupRequestHandler(object sender, RoutedEventArgs e)
        {
            Button btn = ((Button)(sender));
            object rawBtnData = btn.DataContext;
            Dictionary<String, Object> btnData = ((Dictionary<String, Object>)(rawBtnData));
            string groupId = ((string)(btnData["groupId"]));
            string userId = ((string)(btnData["userId"]));
            string requestId = ((string)(btnData["requestId"]));
            Popup request = ((Popup)(btnData["request"]));
            RejectGroupRequest(groupId, userId, requestId, request);
        }

        public void RejectGroupRequest(string groupId, string userId, string requestId, Popup request)
        {
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/groups/requests/reject/?id=" + requestId);
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (var reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        var objText = reader.ReadToEnd();
                        UserResponseInfo myobj = (UserResponseInfo)js.Deserialize(objText, typeof(UserResponseInfo));
                        string status = myobj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
                            webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/users/get/?id=" + userId);
                            webRequest.Method = "GET";
                            webRequest.UserAgent = ".NET Framework Test Client";
                            using (HttpWebResponse innerWebResponse = (HttpWebResponse)webRequest.GetResponse())
                            {
                                using (StreamReader innerReader = new StreamReader(innerWebResponse.GetResponseStream()))
                                {
                                    js = new JavaScriptSerializer();
                                    objText = innerReader.ReadToEnd();

                                    myobj = (UserResponseInfo)js.Deserialize(objText, typeof(UserResponseInfo));

                                    status = myobj.status;
                                    isOkStatus = status == "OK";
                                    if (isOkStatus)
                                    {
                                        CloseGroupRequest(request);
                                        User friend = myobj.user;
                                        string friendLogin = friend.login;
                                        string msgContent = "Вы отклонили приглашение в группу";
                                        MessageBox.Show(msgContent, "Внимание");
                                    }
                                }
                            }
                        }
                        else
                        {
                            MessageBox.Show("Не удалось отклонить приглашение", "Ошибка");
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

        public void RemoveFriendHandler(object sender, RoutedEventArgs e)
        {
            RemoveFriend();
        }

        public void RemoveFriend()
        {
            try
            {
                string friendId = cachedUserProfileId;
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/friends/remove/?id=" + currentUserId + "&friend=" + friendId);
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (var reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        var objText = reader.ReadToEnd();
                        UserResponseInfo myobj = (UserResponseInfo)js.Deserialize(objText, typeof(UserResponseInfo));
                        string status = myobj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
                            Environment.SpecialFolder localApplicationDataFolder = Environment.SpecialFolder.LocalApplicationData;
                            string localApplicationDataFolderPath = Environment.GetFolderPath(localApplicationDataFolder);
                            string saveDataFilePath = localApplicationDataFolderPath + @"\OfficeWare\GameManager\" + currentUserId + @"\save-data.txt";
                            js = new JavaScriptSerializer();
                            string saveDataFileContent = File.ReadAllText(saveDataFilePath);
                            SavedContent loadedContent = js.Deserialize<SavedContent>(saveDataFileContent);
                            List<Game> currentGames = loadedContent.games;
                            List<FriendSettings> updatedFriends = loadedContent.friends;
                            Settings currentSettings = loadedContent.settings;
                            List<string> currentCollections = loadedContent.collections;
                            Notifications currentNotifications = loadedContent.notifications;
                            List<FriendSettings> cachedFriends = updatedFriends.Where<FriendSettings>((FriendSettings friend) =>
                            {
                                return friend.id == friendId;
                            }).ToList();
                            int countCachedFriends = cachedFriends.Count;
                            bool isCachedFriendsExists = countCachedFriends >= 1;
                            if (isCachedFriendsExists)
                            {
                                FriendSettings cachedFriend = cachedFriends[0];
                                updatedFriends.Remove(cachedFriend);
                                string savedContent = js.Serialize(new SavedContent
                                {
                                    games = currentGames,
                                    friends = updatedFriends,
                                    settings = currentSettings,
                                    collections = currentCollections,
                                    notifications = currentNotifications
                                });
                                File.WriteAllText(saveDataFilePath, savedContent);
                                mainControl.DataContext = currentUserId;
                                mainControl.SelectedIndex = 0;
                                GetFriendsSettings();
                                GetOnlineFriends();
                            }
                        }
                        else
                        {
                            MessageBox.Show("Не удается удалить друга", "Ошибка");
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

        public void CloseGroupRequestHandler(object sender, RoutedEventArgs e)
        {
            PackIcon btn = ((PackIcon)(sender));
            object btnData = btn.DataContext;
            Popup request = ((Popup)(btnData));
            CloseGroupRequest(request);
        }

        public void CloseGroupRequest(Popup request)
        {
            groupRequests.Children.Remove(request);
        }


        public void GetFriendRequestsForMeHandler(object sender, TextChangedEventArgs e)
        {
            GetFriendRequestsForMe();
        }

        public void GetFriendRequestsFromMeHandler(object sender, TextChangedEventArgs e)
        {
            GetFriendRequestsFromMe();
        }


        public void ShowOffersHandler(object sender, RoutedEventArgs e)
        {
            ShowOffers();
        }

        public void GetGameCollections()
        {
            Environment.SpecialFolder localApplicationDataFolder = Environment.SpecialFolder.LocalApplicationData;
            string localApplicationDataFolderPath = Environment.GetFolderPath(localApplicationDataFolder);
            string saveDataFilePath = localApplicationDataFolderPath + @"\OfficeWare\GameManager\" + currentUserId + @"\save-data.txt";
            JavaScriptSerializer js = new JavaScriptSerializer();
            string saveDataFileContent = File.ReadAllText(saveDataFilePath);
            SavedContent loadedContent = js.Deserialize<SavedContent>(saveDataFileContent);
            List<Game> currentGames = loadedContent.games;
            List<string> currentCollections = loadedContent.collections;
            gameCollections.Children.RemoveRange(1, gameCollections.Children.Count - 1);
            foreach (string currentCollection in currentCollections)
            {
                Border gameCollection = new Border();
                gameCollection.Margin = new Thickness(25);
                gameCollection.Padding = new Thickness(1.5);
                gameCollection.BorderBrush = System.Windows.Media.Brushes.Black;
                gameCollection.BorderThickness = new Thickness(2);
                gameCollection.CornerRadius = new CornerRadius(5);
                StackPanel gameCollectionBody = new StackPanel();
                gameCollectionBody.Width = 175;
                gameCollectionBody.Height = 175;
                gameCollection.Child = gameCollectionBody;
                TextBlock gameCollectionBodyNameLabel = new TextBlock();
                gameCollectionBodyNameLabel.TextAlignment = TextAlignment.Center;
                gameCollectionBodyNameLabel.Margin = new Thickness(0, 125, 0, 0);
                gameCollectionBodyNameLabel.Text = currentCollection;
                gameCollectionBody.Children.Add(gameCollectionBodyNameLabel);
                TextBlock gameCollectionBodyCountGamesLabel = new TextBlock();
                gameCollectionBodyCountGamesLabel.TextAlignment = TextAlignment.Center;
                gameCollectionBodyCountGamesLabel.Margin = new Thickness(0, 15, 0, 0);
                List<Game> gamesForCollection = currentGames.Where<Game>((Game game) =>
                {
                    List<string> gameCollections = game.collections;
                    bool isGameForCollection = gameCollections.Contains(currentCollection);
                    return isGameForCollection;
                }).ToList<Game>();
                int countGamesForCollection = gamesForCollection.Count;
                string rawCountGamesForCollection = countGamesForCollection.ToString();
                gameCollectionBodyCountGamesLabel.Text = rawCountGamesForCollection;
                gameCollectionBody.Children.Add(gameCollectionBodyCountGamesLabel);
                gameCollections.Children.Add(gameCollection);
                gameCollection.DataContext = currentCollection;
                gameCollection.MouseLeftButtonUp += SelectGameCollectionHandler;
                ContextMenu gameCollectionContextMenu = new ContextMenu();
                MenuItem gameCollectionContextMenuItem = new MenuItem();
                gameCollectionContextMenuItem.Header = "Переименовать коллекцию";
                gameCollectionContextMenuItem.DataContext = currentCollection;
                gameCollectionContextMenuItem.Click += RenameGameCollectionHandler;
                gameCollectionContextMenu.Items.Add(gameCollectionContextMenuItem);
                gameCollectionContextMenuItem = new MenuItem();
                gameCollectionContextMenuItem.Header = "Удалить коллекцию";
                gameCollectionContextMenuItem.DataContext = currentCollection;
                gameCollectionContextMenuItem.Click += RemoveGameCollectionHandler;
                gameCollectionContextMenu.Items.Add(gameCollectionContextMenuItem);
                gameCollection.ContextMenu = gameCollectionContextMenu;

                int imageWidth = 35;
                int imageHeight = 35;
                DrawingVisual drawingVisual = new DrawingVisual();
                using (DrawingContext drawingContext = drawingVisual.RenderOpen())
                {
                    SkewTransform transform = new SkewTransform();
                    /*transform.AngleX = 75;
                    transform.AngleY = 225;
                    transform.CenterX = 25;
                    transform.CenterY = 75;*/
                    transform.AngleX = 15;
                    transform.AngleY = 15;
                    drawingContext.PushTransform(transform);
                    foreach (Game gameForCollection in gamesForCollection)
                    {
                        int frameIndex = gamesForCollection.IndexOf(gameForCollection);
                        string currentGameId = gameForCollection.id;
                        string currentGameName = gameForCollection.name;
                        string currentGameCover = gameForCollection.cover;
                        // Loads the images to tile (no need to specify PngBitmapDecoder, the correct decoder is automatically selected)
                        bool isCoverSet = currentGameCover != "";
                        bool isCoverFound = File.Exists(currentGameCover);
                        bool isCoverExists = isCoverSet && isCoverFound;
                        Uri coverUri = null;
                        bool isCustomGame = currentGameId == "mockId";
                        bool isNotCustomGame = !isCustomGame;
                        if (isCoverExists)
                        {
                            coverUri = new Uri(currentGameCover);
                        }
                        else if (isNotCustomGame)
                        {
                            coverUri = new Uri(@"http://localhost:4000/api/game/thumbnail/?name=" + currentGameName);
                        }
                        else
                        {
                            coverUri = new Uri(@"https://cdn3.iconfinder.com/data/icons/solid-locations-icon-set/64/Games_2-256.png");
                        }
                        BitmapFrame frame = BitmapDecoder.Create(coverUri, BitmapCreateOptions.None, BitmapCacheOption.OnLoad).Frames.First();
                        // Gets the size of the images (I assume each image has the same size)
                        // Draws the images into a DrawingVisual component
                        drawingContext.DrawImage(frame, new Rect(frameIndex * 35, 0, imageWidth, imageHeight));
                    }
                }
                VisualBrush visualBrush = new VisualBrush(drawingVisual);
                gameCollectionBody.Background = visualBrush;

            }
        }

        public void RenameGameCollectionHandler(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = ((MenuItem)(sender));
            object menuItemData = menuItem.DataContext;
            string name = ((string)(menuItemData));
            RenameGameCollection(name);
        }

        public void RenameGameCollection(string name)
        {
            SelectGameCollection(name);
            ToggleRenameBtn(renameIcon);
        }

        public void RemoveGameCollectionHandler(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = ((MenuItem)(sender));
            object menuItemData = menuItem.DataContext;
            string name = ((string)(menuItemData));
            RemoveGameCollection(name);
        }

        public void RemoveGameCollection(string name)
        {
            Environment.SpecialFolder localApplicationDataFolder = Environment.SpecialFolder.LocalApplicationData;
            string localApplicationDataFolderPath = Environment.GetFolderPath(localApplicationDataFolder);
            string saveDataFilePath = localApplicationDataFolderPath + @"\OfficeWare\GameManager\" + currentUserId + @"\save-data.txt";
            JavaScriptSerializer js = new JavaScriptSerializer();
            string saveDataFileContent = File.ReadAllText(saveDataFilePath);
            SavedContent loadedContent = js.Deserialize<SavedContent>(saveDataFileContent);
            List<Game> updatedGames = loadedContent.games;
            List<FriendSettings> currentFriends = loadedContent.friends;
            Settings currentSettings = loadedContent.settings;
            List<string> updatedCollections = loadedContent.collections;
            Notifications currentNotifications = loadedContent.notifications;
            string gameCollectionNameLabelContent = gameCollectionNameLabel.Text;
            object rawCurrentGameCollection = gameCollectionNameLabel.DataContext;
            string currentGameCollection = ((string)(rawCurrentGameCollection));
            updatedCollections = updatedCollections.Where<string>((string collection) =>
            {
                bool isRemoveCollection = name == collection;
                bool isSkipCollection = !isRemoveCollection;
                return isSkipCollection;
            }).ToList<string>();
            foreach (Game updatedGame in updatedGames)
            {
                List<string> updatedGameCollections = updatedGame.collections;
                int collectionIndex = -1;
                List<int> updatedGameCollectionsIndexes = new List<int>();
                foreach (string updatedGameCollection in updatedGameCollections)
                {
                    collectionIndex++;
                    bool isRemove = updatedGameCollection == name;
                    if (isRemove)
                    {
                        updatedGameCollectionsIndexes.Add(collectionIndex);
                    }
                }
                for (int i = 0; i < updatedGameCollectionsIndexes.Count; i++)
                {
                    updatedGameCollections.RemoveAt(updatedGameCollectionsIndexes[i]);
                }
            }
            string savedContent = js.Serialize(new SavedContent
            {
                games = updatedGames,
                friends = currentFriends,
                settings = currentSettings,
                collections = updatedCollections,
                notifications = currentNotifications
            });
            File.WriteAllText(saveDataFilePath, savedContent);
            GetGamesList("");
            GetGameCollections();
        }

        public void SelectGameCollectionHandler(object sender, RoutedEventArgs e)
        {
            Border collection = ((Border)(sender));
            object collectionData = collection.DataContext;
            string collectionName = ((string)(collectionData));
            SelectGameCollection(collectionName);
        }

        public void SelectGameCollection(string name)
        {

            /*AdornerLayer layer = AdornerLayer.GetAdornerLayer(adornerWrap);
            StackPanel toAdorn = new StackPanel();
            toAdorn.Width = 100;
            toAdorn.Height = 100;
            toAdorn.Background = System.Windows.Media.Brushes.Red;
            layer.Add(new Helpers.SimpleCircleAdorner(toAdorn));*/
            gameCollectionNameLabel.DataContext = name;
            mainControl.SelectedIndex = 10;
            GetGameCollectionItems(name);
            gameCollectionNameLabel.Text = name;
            Environment.SpecialFolder localApplicationDataFolder = Environment.SpecialFolder.LocalApplicationData;
            string localApplicationDataFolderPath = Environment.GetFolderPath(localApplicationDataFolder);
            string saveDataFilePath = localApplicationDataFolderPath + @"\OfficeWare\GameManager\" + currentUserId + @"\save-data.txt";
            JavaScriptSerializer js = new JavaScriptSerializer();
            string saveDataFileContent = File.ReadAllText(saveDataFilePath);
            SavedContent loadedContent = js.Deserialize<SavedContent>(saveDataFileContent);
            List<Game> currentGames = loadedContent.games;
            List<Game> gamesForCollection = currentGames.Where<Game>((Game game) =>
            {
                List<string> gameCollections = game.collections;
                bool isGameForCollection = gameCollections.Contains(name);
                return isGameForCollection;
            }).ToList<Game>();
            int countGamesForCollection = gamesForCollection.Count;
            string rawCountGamesForCollection = countGamesForCollection.ToString();
            countGameCollectionGamesLabel.Text = "(" + rawCountGamesForCollection + ")";

        }

        public void GetGameCollectionItems(string name)
        {
            Environment.SpecialFolder localApplicationDataFolder = Environment.SpecialFolder.LocalApplicationData;
            string localApplicationDataFolderPath = Environment.GetFolderPath(localApplicationDataFolder);
            string saveDataFilePath = localApplicationDataFolderPath + @"\OfficeWare\GameManager\" + currentUserId + @"\save-data.txt";
            JavaScriptSerializer js = new JavaScriptSerializer();
            string saveDataFileContent = File.ReadAllText(saveDataFilePath);
            SavedContent loadedContent = js.Deserialize<SavedContent>(saveDataFileContent);
            List<Game> currentGames = loadedContent.games;
            List<string> currentCollections = loadedContent.collections;
            List<Game> collectionGames = currentGames.Where<Game>((Game game) =>
            {
                List<string> gameCollections = game.collections;
                bool isGameForCollection = gameCollections.Contains(name);
                bool isHiddenGame = game.isHidden;
                bool isDisplayedGame = !isHiddenGame;
                bool isShowGame = isGameForCollection && isDisplayedGame;
                return isShowGame;
            }).ToList();
            int collectionGamesCount = collectionGames.Count;
            bool isHaveGames = collectionGamesCount >= 1;
            gameCollectionItems.HorizontalAlignment = HorizontalAlignment.Left;
            gameCollectionItems.Children.Clear();
            if (isHaveGames)
            {
                foreach (Game currentGame in currentGames)
                {
                    List<string> currentGameCollections = currentGame.collections;
                    bool isGameForCurrentCollection = currentGameCollections.Contains(name);
                    bool isHiddenGame = currentGame.isHidden;
                    bool isDisplayedGame = !isHiddenGame;
                    bool isShowGame = isGameForCurrentCollection && isDisplayedGame;
                    if (isShowGame)
                    {
                        string currentGameId = currentGame.id;
                        string currentGameName = currentGame.name;
                        string currentGameCover = currentGame.cover;
                        bool isCoverSet = currentGameCover != "";
                        bool isCoverFound = File.Exists(currentGameCover);
                        bool isCoverExists = isCoverSet && isCoverFound;
                        Image gameCollectionItem = new Image();
                        gameCollectionItem.Width = 100;
                        gameCollectionItem.Height = 100;
                        gameCollectionItem.Margin = new Thickness(25);
                        gameCollectionItem.BeginInit();
                        Uri coverUri = null;
                        if (isCoverExists)
                        {
                            coverUri = new Uri(currentGameCover);
                        }
                        else
                        {
                            coverUri = new Uri(@"http://localhost:4000/api/game/thumbnail/?name=" + currentGameName);
                        }
                        gameCollectionItem.Source = new BitmapImage(coverUri);
                        gameCollectionItem.ImageFailed += SetDefaultThumbnailHandler;
                        gameCollectionItem.EndInit();
                        gameCollectionItems.Children.Add(gameCollectionItem);
                        gameCollectionItem.DataContext = currentGameName;
                        gameCollectionItem.MouseLeftButtonUp += SelectGameCollectionItemHandler;
                        ContextMenu gameCollectionItemContextMenu = new ContextMenu();

                        MenuItem gameCollectionItemContextMenuItem = new MenuItem();
                        gameCollectionItemContextMenuItem.Header = "Играть";
                        gameCollectionItemContextMenuItem.DataContext = currentGameName;
                        gameCollectionItemContextMenuItem.Click += RunGameHandler;
                        gameCollectionItemContextMenu.Items.Add(gameCollectionItemContextMenuItem);

                        gameCollectionItemContextMenuItem = new MenuItem();
                        gameCollectionItemContextMenuItem.Header = "Добавить в";
                        MenuItem gameCollectionItemNestedContextMenuItem;
                        Dictionary<String, Object> gameCollectionItemNestedContextMenuItemData;
                        foreach (string currentCollection in currentCollections)
                        {
                            gameCollectionItemNestedContextMenuItem = new MenuItem();
                            gameCollectionItemNestedContextMenuItem.Header = currentCollection;
                            gameCollectionItemNestedContextMenuItemData = new Dictionary<String, Object>();
                            gameCollectionItemNestedContextMenuItemData.Add("name", currentGameName);
                            gameCollectionItemNestedContextMenuItemData.Add("collection", currentCollection);
                            gameCollectionItemNestedContextMenuItem.DataContext = gameCollectionItemNestedContextMenuItemData;
                            gameCollectionItemNestedContextMenuItem.Click += AddGameToCollectionHandler;
                            gameCollectionItemContextMenuItem.Items.Add(gameCollectionItemNestedContextMenuItem);
                            bool isDisabledCollection = currentGameCollections.Contains(currentCollection);
                            if (isDisabledCollection)
                            {
                                gameCollectionItemNestedContextMenuItem.IsEnabled = false;
                            }
                        }
                        gameCollectionItemNestedContextMenuItem = new MenuItem();
                        gameCollectionItemNestedContextMenuItem.Header = "Создать коллекцию";
                        gameCollectionItemNestedContextMenuItemData = new Dictionary<String, Object>();
                        gameCollectionItemNestedContextMenuItemData.Add("name", currentGameName);
                        gameCollectionItemNestedContextMenuItemData.Add("collection", name);
                        gameCollectionItemNestedContextMenuItem.DataContext = gameCollectionItemNestedContextMenuItemData;
                        gameCollectionItemNestedContextMenuItem.Click += CreateCollectionFromMenuHandler;
                        gameCollectionItemContextMenuItem.Items.Add(gameCollectionItemNestedContextMenuItem);
                        gameCollectionItemContextMenu.Items.Add(gameCollectionItemContextMenuItem);

                        gameCollectionItemContextMenuItem = new MenuItem();
                        string gameCollectionItemContextMenuItemHeaderContent = "Убрать из " + name;
                        gameCollectionItemContextMenuItem.Header = gameCollectionItemContextMenuItemHeaderContent;
                        Dictionary<String, Object> gameCollectionItemContextMenuItemData = new Dictionary<String, Object>();
                        gameCollectionItemContextMenuItemData.Add("game", currentGameName);
                        gameCollectionItemContextMenuItemData.Add("collection", name);
                        gameCollectionItemContextMenuItem.DataContext = gameCollectionItemContextMenuItemData;
                        gameCollectionItemContextMenuItem.Click += RemoveGameFromCollectionHandler;
                        gameCollectionItemContextMenu.Items.Add(gameCollectionItemContextMenuItem);

                        gameCollectionItemContextMenuItem = new MenuItem();
                        gameCollectionItemContextMenuItem.Header = "Управление";

                        gameCollectionItemNestedContextMenuItem = new MenuItem();
                        gameCollectionItemNestedContextMenuItem.Header = "Создать ярлык на рабочем столе";
                        gameCollectionItemNestedContextMenuItem.DataContext = currentGameName;
                        gameCollectionItemNestedContextMenuItem.Click += CreateShortcutHandler;
                        gameCollectionItemContextMenuItem.Items.Add(gameCollectionItemNestedContextMenuItem);

                        gameCollectionItemNestedContextMenuItem = new MenuItem();
                        bool IsCoverSet = currentGameCover != "";
                        if (IsCoverSet)
                        {
                            gameCollectionItemNestedContextMenuItem.Header = "Удалить свою обложку";
                        }
                        else
                        {
                            gameCollectionItemNestedContextMenuItem.Header = "Задать свою обложку";
                        }
                        gameCollectionItemNestedContextMenuItemData = new Dictionary<String, Object>();
                        gameCollectionItemNestedContextMenuItemData.Add("game", currentGameName);
                        gameCollectionItemNestedContextMenuItemData.Add("collection", name);
                        gameCollectionItemNestedContextMenuItemData.Add("cover", currentGameCover);
                        gameCollectionItemNestedContextMenuItem.DataContext = gameCollectionItemNestedContextMenuItemData;
                        gameCollectionItemNestedContextMenuItem.Click += ToggleGameCoverHandler;
                        gameCollectionItemContextMenuItem.Items.Add(gameCollectionItemNestedContextMenuItem);

                        gameCollectionItemNestedContextMenuItem = new MenuItem();
                        gameCollectionItemNestedContextMenuItem.Header = "Просмотреть локальные файлы";
                        gameCollectionItemNestedContextMenuItem.DataContext = currentGameName;
                        gameCollectionItemNestedContextMenuItem.Click += ShowGamesLocalFilesHandler;
                        gameCollectionItemContextMenuItem.Items.Add(gameCollectionItemNestedContextMenuItem);

                        gameCollectionItemNestedContextMenuItem = new MenuItem();
                        isHiddenGame = currentGame.isHidden;
                        if (isHiddenGame)
                        {
                            gameCollectionItemNestedContextMenuItem.Header = "Убрать из скрытого";
                        }
                        else
                        {
                            gameCollectionItemNestedContextMenuItem.Header = "Скрыть игру";
                        }
                        gameCollectionItemNestedContextMenuItemData = new Dictionary<String, Object>();
                        gameCollectionItemNestedContextMenuItemData.Add("game", currentGameName);
                        gameCollectionItemNestedContextMenuItemData.Add("collection", name);
                        gameCollectionItemNestedContextMenuItem.DataContext = gameCollectionItemNestedContextMenuItemData;
                        gameCollectionItemNestedContextMenuItem.Click += ToggleGameVisibilityHandler;
                        gameCollectionItemContextMenuItem.Items.Add(gameCollectionItemNestedContextMenuItem);

                        gameCollectionItemNestedContextMenuItem = new MenuItem();
                        gameCollectionItemNestedContextMenuItem.Header = "Удалить с утройства";
                        gameCollectionItemNestedContextMenuItemData = new Dictionary<String, Object>();
                        gameCollectionItemNestedContextMenuItemData.Add("game", currentGameName);
                        gameCollectionItemNestedContextMenuItemData.Add("collection", name);
                        gameCollectionItemNestedContextMenuItem.DataContext = gameCollectionItemNestedContextMenuItemData;
                        gameCollectionItemNestedContextMenuItem.Click += RemoveGameFromCollectionsMenuHandler;
                        gameCollectionItemContextMenuItem.Items.Add(gameCollectionItemNestedContextMenuItem);

                        gameCollectionItemContextMenu.Items.Add(gameCollectionItemContextMenuItem);

                        gameCollectionItemContextMenuItem = new MenuItem();
                        gameCollectionItemContextMenuItem.Header = "Свойства";
                        // gameCollectionItemContextMenuItem.DataContext = currentGameName;
                        gameCollectionItemContextMenuItemData = new Dictionary<String, Object>();
                        gameCollectionItemContextMenuItemData.Add("game", currentGameName);
                        bool isCustomGame = currentGameId == "mockId";
                        gameCollectionItemContextMenuItemData.Add("isCustomGame", isCustomGame);
                        gameCollectionItemContextMenuItem.DataContext = gameCollectionItemContextMenuItemData;
                        gameCollectionItemContextMenuItem.Click += OpenGameSettingsHandler;
                        gameCollectionItemContextMenu.Items.Add(gameCollectionItemContextMenuItem);

                        gameCollectionItem.ContextMenu = gameCollectionItemContextMenu;
                    }
                }
            }
            else
            {
                gameCollectionItems.HorizontalAlignment = HorizontalAlignment.Center;
                TextBlock gameCollectionsNotFoundLabel = new TextBlock();
                gameCollectionsNotFoundLabel.Text = "Перетащите сюда игры чтобы создать коллекцию.";
                gameCollectionItems.Children.Add(gameCollectionsNotFoundLabel);
            }
        }

        public void InsertGameToCollectionHandler(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = ((MenuItem)(sender));
            object menuItemData = menuItem.DataContext;
            Dictionary<String, Object> data = ((Dictionary<String, Object>)(menuItemData));
            InsertGameToCollection(data);
        }

        public void InsertGameToCollection(Dictionary<String, Object> data)
        {
            string game = ((string)(data["game"]));
            string collection = ((string)(data["collection"]));
        }


        public void OpenGameSettingsHandler(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = ((MenuItem)(sender));
            object menuItemData = menuItem.DataContext;
            Dictionary<String, Object> data = ((Dictionary<String, Object>)(menuItemData));
            OpenGameSettings(data);
        }

        public void OpenGameSettings(Dictionary<String, Object> data)
        {
            string name = ((string)(data["game"]));
            bool isCustomGame = ((bool)(data["isCustomGame"]));
            Dialogs.GameSettingsDialog dialog = new Dialogs.GameSettingsDialog(currentUserId, name, isCustomGame);
            dialog.Show();
        }

        public void ToggleGameCoverHandler(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = ((MenuItem)(sender));
            object rawMenuItemData = menuItem.DataContext;
            Dictionary<String, Object> menuItemData = ((Dictionary<String, Object>)(rawMenuItemData));
            ToggleGameCover(menuItemData);
        }

        public void ToggleGameCover(Dictionary<String, Object> data)
        {
            string name = ((string)(data["game"]));
            string collection = ((string)(data["collection"]));
            string cover = ((string)(data["cover"]));
            Environment.SpecialFolder localApplicationDataFolder = Environment.SpecialFolder.LocalApplicationData;
            string localApplicationDataFolderPath = Environment.GetFolderPath(localApplicationDataFolder);
            string saveDataFilePath = localApplicationDataFolderPath + @"\OfficeWare\GameManager\" + currentUserId + @"\save-data.txt";
            JavaScriptSerializer js = new JavaScriptSerializer();
            string saveDataFileContent = File.ReadAllText(saveDataFilePath);
            SavedContent loadedContent = js.Deserialize<SavedContent>(saveDataFileContent);
            List<Game> updatedGames = loadedContent.games;
            List<FriendSettings> currentFriends = loadedContent.friends;
            Settings currentSettings = loadedContent.settings;
            List<string> currentCollections = loadedContent.collections;
            Notifications currentNotifications = loadedContent.notifications;
            List<Game> results = updatedGames.Where<Game>((Game game) =>
            {
                return game.name == name;
            }).ToList<Game>();
            int resultsCount = results.Count;
            bool isFound = resultsCount >= 1;
            if (isFound)
            {
                Game result = results[0];
                bool isCoverSet = cover != "";
                if (isCoverSet)
                {
                    result.cover = "";
                }
                else
                {
                    Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
                    ofd.Title = "Выберите обложку";
                    ofd.Filter = "Png documents (.png)|*.png";
                    bool? res = ofd.ShowDialog();
                    bool isOpened = res != false;
                    if (isOpened)
                    {
                        string path = ofd.FileName;
                        result.cover = path;
                    }
                }
                string savedContent = js.Serialize(new SavedContent
                {
                    games = updatedGames,
                    friends = currentFriends,
                    settings = currentSettings,
                    collections = currentCollections,
                    notifications = currentNotifications
                });
                File.WriteAllText(saveDataFilePath, savedContent);
                GetGameCollections();
                GetGameCollectionItems(collection);
                GetHiddenGames();
            }
        }

        public void ShowGamesLocalFilesHandler(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = ((MenuItem)(sender));
            object menuItemData = menuItem.DataContext;
            string name = ((string)(menuItemData));
            ShowGamesLocalFiles(name);
        }

        public void ShowGamesLocalFiles(string name)
        {
            Environment.SpecialFolder localApplicationDataFolder = Environment.SpecialFolder.LocalApplicationData;
            string localApplicationDataFolderPath = Environment.GetFolderPath(localApplicationDataFolder);
            string gamePath = localApplicationDataFolderPath + @"\OfficeWare\GameManager\" + currentUserId + @"\games\" + name;
            System.Diagnostics.Process.Start(new ProcessStartInfo
            {
                FileName = "explorer",
                Arguments = gamePath,
                UseShellExecute = true
            });
        }

        public void RemoveGameFromCollectionHandler(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = ((MenuItem)(sender));
            object rawMenuItemData = menuItem.DataContext;
            Dictionary<String, Object> menuItemData = ((Dictionary<String, Object>)(rawMenuItemData));
            RemoveGameFromCollection(menuItemData);
        }

        public void RemoveGameFromCollection(Dictionary<String, Object> menuItemData)
        {
            string game = ((string)(menuItemData["game"]));
            string collection = ((string)(menuItemData["collection"]));
            Environment.SpecialFolder localApplicationDataFolder = Environment.SpecialFolder.LocalApplicationData;
            string localApplicationDataFolderPath = Environment.GetFolderPath(localApplicationDataFolder);
            string saveDataFilePath = localApplicationDataFolderPath + @"\OfficeWare\GameManager\" + currentUserId + @"\save-data.txt";
            JavaScriptSerializer js = new JavaScriptSerializer();
            string saveDataFileContent = File.ReadAllText(saveDataFilePath);
            SavedContent loadedContent = js.Deserialize<SavedContent>(saveDataFileContent);
            List<Game> updatedGames = loadedContent.games;
            List<FriendSettings> currentFriends = loadedContent.friends;
            Settings currentSettings = loadedContent.settings;
            List<string> currentCollections = loadedContent.collections;
            Notifications currentNotifications = loadedContent.notifications;
            foreach (Game updatedGame in updatedGames)
            {
                string updatedGameName = updatedGame.name;
                List<string> updatedGameCollections = updatedGame.collections;
                int collectionIndex = -1;
                List<int> updatedGameCollectionsIndexes = new List<int>();
                foreach (string updatedGameCollection in updatedGameCollections)
                {
                    collectionIndex++;
                    bool isRemove = updatedGameCollection == collection && updatedGameName == game;
                    if (isRemove)
                    {
                        updatedGameCollectionsIndexes.Add(collectionIndex);
                    }
                }
                for (int i = 0; i < updatedGameCollectionsIndexes.Count; i++)
                {
                    updatedGameCollections.RemoveAt(updatedGameCollectionsIndexes[i]);
                }
            }
            string savedContent = js.Serialize(new SavedContent
            {
                games = updatedGames,
                friends = currentFriends,
                settings = currentSettings,
                collections = currentCollections,
                notifications = currentNotifications
            });
            File.WriteAllText(saveDataFilePath, savedContent);
            GetGamesList("");
            GetGameCollections();
            GetGameCollectionItems(collection);
        }

        public void SelectGameCollectionItemHandler(object sender, RoutedEventArgs e)
        {
            Image thumbnail = ((Image)(sender));
            object thumbnailData = thumbnail.DataContext;
            string name = ((string)(thumbnailData));
            SelectGameCollectionItem(name);
        }

        public void SelectGameCollectionItem(string name)
        {
            Dictionary<String, Object> myGameData = null;
            foreach (StackPanel game in games.Children)
            {
                object rawGameData = game.DataContext;
                Dictionary<String, Object> gameData = ((Dictionary<String, Object>)(rawGameData));
                bool isGameDataFound = name == ((string)(gameData["name"]));
                if (isGameDataFound)
                {
                    myGameData = gameData;
                    break;
                }
            }
            bool isMyGameDataExists = myGameData != null;
            if (isMyGameDataExists)
            {
                OpenGamesLibrary();
                SelectGame(myGameData);
            }
        }

        public void GetLocalGames(string keywords)
        {

            Environment.SpecialFolder localApplicationDataFolder = Environment.SpecialFolder.LocalApplicationData;
            string localApplicationDataFolderPath = Environment.GetFolderPath(localApplicationDataFolder);
            string saveDataFilePath = localApplicationDataFolderPath + @"\OfficeWare\GameManager\" + currentUserId + @"\save-data.txt";
            JavaScriptSerializer js = new JavaScriptSerializer();
            string saveDataFileContent = File.ReadAllText(saveDataFilePath);
            SavedContent loadedContent = js.Deserialize<SavedContent>(saveDataFileContent);
            List<string> currentCollections = loadedContent.collections;
            List<Game> currentGames = loadedContent.games;
            List<Game> loadedGames = currentGames;
            loadedGames = loadedGames.Where<Game>((Game gameInfo) =>
            {
                int keywordsLength = keywords.Length;
                bool isKeywordsSetted = keywordsLength >= 1;
                bool isKeywordsNotSetted = !isKeywordsSetted;
                string gameId = gameInfo.id;
                string gameName = gameInfo.name;
                string ignoreCaseKeywords = keywords.ToLower();
                string ignoreCaseGameName = gameName.ToLower();
                bool isGameNameMatch = ignoreCaseGameName.Contains(ignoreCaseKeywords);
                bool isSearchEnabled = isKeywordsSetted && isGameNameMatch;
                bool isGameMatch = isSearchEnabled || isKeywordsNotSetted;
                bool isCustomGame = gameId == "mockId";
                bool isCustomGameMatch = isGameMatch && isCustomGame;
                return isCustomGameMatch;
            }).ToList<Game>();
            int countLoadedGames = loadedGames.Count;
            // bool isGamesExists = countLoadedGames >= 1;
            bool isGamesExists = countLoadedGames >= 1;
            if (isGamesExists)
            {
                activeGame.Visibility = visible;
                foreach (Game gamesListItem in loadedGames)
                {

                    string gamesListItemName = gamesListItem.name;

                    List<Game> results = currentGames.Where<Game>((Game game) =>
                    {
                        return game.name == gamesListItemName;
                    }).ToList<Game>();
                    int countResults = results.Count;
                    bool isHaveResults = countResults >= 1;
                    bool isShowGame = true;
                    Game result = null;
                    if (isHaveResults)
                    {
                        result = results[0];
                        bool isHidden = result.isHidden;
                        isShowGame = !isHidden;
                    }
                    if (isShowGame)
                    {
                        StackPanel newGame = new StackPanel();
                        newGame.MouseLeftButtonUp += SelectGameHandler;
                        newGame.Orientation = Orientation.Horizontal;
                        newGame.Height = 35;

                        // string gamesListItemId = gamesListItem._id;

                        Dictionary<String, Object> newGameData = new Dictionary<String, Object>();
                        newGameData.Add("id", "mockId");
                        newGameData.Add("name", gamesListItemName);
                        newGameData.Add("url", "");
                        newGameData.Add("image", "");
                        newGameData.Add("price", 0);
                        newGame.DataContext = newGameData;
                        Image newGamePhoto = new Image();
                        newGamePhoto.Margin = new Thickness(5);
                        newGamePhoto.Width = 25;
                        newGamePhoto.Height = 25;
                        newGamePhoto.BeginInit();
                        // Uri newGamePhotoUri = new Uri(gamesListItemImage);
                        // Uri newGamePhotoUri = new Uri("https://cdn2.iconfinder.com/data/icons/ios-7-icons/50/user_male-128.png");
                        Uri newGamePhotoUri = new Uri(@"https://cdn3.iconfinder.com/data/icons/solid-locations-icon-set/64/Games_2-256.png");
                        newGamePhoto.Source = new BitmapImage(newGamePhotoUri);
                        newGamePhoto.EndInit();
                        newGame.Children.Add(newGamePhoto);
                        newGamePhoto.ImageFailed += SetDefaultThumbnailHandler;
                        TextBlock newGameLabel = new TextBlock();
                        newGameLabel.Margin = new Thickness(5);
                        newGameLabel.Text = gamesListItem.name;
                        newGame.Children.Add(newGameLabel);
                        games.Children.Add(newGame);

                        List<Game> gameSearchResults = currentGames.Where<Game>((Game game) =>
                        {
                            string gameName = game.name;
                            bool isGameFound = gameName == gamesListItemName;
                            return isGameFound;
                        }).ToList<Game>();
                        int countGameSearchResults = gameSearchResults.Count;
                        bool isGameInstalled = countGameSearchResults >= 1;
                        bool isGameNotInstalled = !isGameInstalled;
                        if (isGameInstalled)
                        {

                            ContextMenu newGameContextMenu = new ContextMenu();

                            MenuItem newGameContextMenuItem = new MenuItem();
                            newGameContextMenuItem.Header = "Играть";
                            newGameContextMenuItem.DataContext = gamesListItemName;
                            newGameContextMenuItem.Click += RunGameHandler;
                            newGameContextMenu.Items.Add(newGameContextMenuItem);

                            newGameContextMenuItem = new MenuItem();
                            newGameContextMenuItem.Header = "Добавить в коллекцию";
                            newGameContextMenu.Items.Add(newGameContextMenuItem);

                            foreach (string collectionName in currentCollections)
                            {
                                MenuItem newGameInnerContextMenuItem = new MenuItem();
                                List<string> resultCollections = new List<string>();
                                if (isHaveResults)
                                {
                                    resultCollections = result.collections;
                                }
                                bool isAlreadyInCollection = resultCollections.Contains(collectionName);
                                if (isAlreadyInCollection)
                                {
                                    newGameInnerContextMenuItem.IsEnabled = false;
                                }

                                newGameInnerContextMenuItem.Header = collectionName;
                                Dictionary<String, Object> newGameInnerContextMenuItemData = new Dictionary<String, Object>();
                                newGameInnerContextMenuItemData.Add("collection", collectionName);
                                newGameInnerContextMenuItemData.Add("name", gamesListItemName);
                                newGameInnerContextMenuItem.DataContext = newGameInnerContextMenuItemData;
                                newGameInnerContextMenuItem.Click += AddGameToCollectionHandler;
                                newGameContextMenuItem.Items.Add(newGameInnerContextMenuItem);
                            }

                            newGameContextMenuItem = new MenuItem();
                            string gameCollectionItemContextMenuItemHeaderContent = "Убрать из ";
                            newGameContextMenuItem.Header = gameCollectionItemContextMenuItemHeaderContent;
                            MenuItem newGameNestedContextMenuItem;
                            Dictionary<String, Object> newGameNestedContextMenuItemData;
                            foreach (string hiddenGameCollection in result.collections)
                            {
                                newGameNestedContextMenuItem = new MenuItem();
                                newGameNestedContextMenuItem.Header = hiddenGameCollection;
                                newGameNestedContextMenuItemData = new Dictionary<String, Object>();
                                newGameNestedContextMenuItemData.Add("game", gamesListItemName);
                                newGameNestedContextMenuItemData.Add("collection", hiddenGameCollection);
                                newGameNestedContextMenuItem.DataContext = newGameNestedContextMenuItemData;
                                newGameNestedContextMenuItem.Click += RemoveGameFromCollectionHandler;
                                newGameContextMenuItem.Items.Add(newGameNestedContextMenuItem);
                            }
                            newGameContextMenu.Items.Add(newGameContextMenuItem);

                            newGameContextMenuItem = new MenuItem();
                            newGameContextMenuItem.Header = "Управление";

                            newGameNestedContextMenuItem = new MenuItem();
                            newGameNestedContextMenuItem.Header = "Создать ярлык на рабочем столе";
                            newGameNestedContextMenuItem.DataContext = gamesListItemName;
                            newGameNestedContextMenuItem.Click += CreateShortcutHandler;
                            newGameContextMenuItem.Items.Add(newGameNestedContextMenuItem);

                            newGameNestedContextMenuItem = new MenuItem();
                            bool IsCoverSet = result.cover != "";
                            if (IsCoverSet)
                            {
                                newGameNestedContextMenuItem.Header = "Удалить свою обложку";
                            }
                            else
                            {
                                newGameNestedContextMenuItem.Header = "Задать свою обложку";
                            }
                            newGameNestedContextMenuItemData = new Dictionary<String, Object>();
                            newGameNestedContextMenuItemData.Add("game", gamesListItemName);
                            newGameNestedContextMenuItemData.Add("collection", "");
                            newGameNestedContextMenuItemData.Add("cover", result.cover);
                            newGameNestedContextMenuItem.DataContext = newGameNestedContextMenuItemData;
                            newGameNestedContextMenuItem.Click += ToggleGameCoverHandler;
                            newGameContextMenuItem.Items.Add(newGameNestedContextMenuItem);

                            newGameNestedContextMenuItem = new MenuItem();
                            newGameNestedContextMenuItem.Header = "Просмотреть локальные файлы";
                            newGameNestedContextMenuItem.DataContext = gamesListItemName;
                            newGameNestedContextMenuItem.Click += ShowGamesLocalFilesHandler;
                            newGameContextMenuItem.Items.Add(newGameNestedContextMenuItem);

                            newGameNestedContextMenuItem = new MenuItem();
                            bool isHiddenGame = result.isHidden;
                            if (isHiddenGame)
                            {
                                newGameNestedContextMenuItem.Header = "Убрать из скрытого";
                            }
                            else
                            {
                                newGameNestedContextMenuItem.Header = "Скрыть игру";
                            }

                            newGameNestedContextMenuItemData = new Dictionary<String, Object>();
                            newGameNestedContextMenuItemData.Add("game", gamesListItemName);
                            newGameNestedContextMenuItemData.Add("collection", "");
                            newGameNestedContextMenuItem.DataContext = newGameNestedContextMenuItemData;
                            newGameNestedContextMenuItem.Click += ToggleGameVisibilityHandler;
                            newGameContextMenuItem.Items.Add(newGameNestedContextMenuItem);

                            newGameNestedContextMenuItem = new MenuItem();
                            newGameNestedContextMenuItem.Header = "Удалить с утройства";
                            newGameNestedContextMenuItemData = new Dictionary<String, Object>();
                            newGameNestedContextMenuItemData.Add("game", gamesListItemName);
                            newGameNestedContextMenuItemData.Add("collection", "");
                            newGameNestedContextMenuItem.DataContext = newGameNestedContextMenuItemData;
                            newGameNestedContextMenuItem.Click += RemoveGameFromCollectionsMenuHandler;
                            newGameContextMenuItem.Items.Add(newGameNestedContextMenuItem);
                            newGameContextMenu.Items.Add(newGameContextMenuItem);

                            string currentGameId = result.id;
                            newGameContextMenuItem = new MenuItem();
                            newGameContextMenuItem.Header = "Свойства";
                            Dictionary<String, Object> gameCollectionItemContextMenuItemData = new Dictionary<String, Object>();
                            gameCollectionItemContextMenuItemData.Add("game", gamesListItemName);
                            bool isCustomGame = currentGameId == "mockId";
                            gameCollectionItemContextMenuItemData.Add("isCustomGame", isCustomGame);
                            newGameContextMenuItem.DataContext = gameCollectionItemContextMenuItemData;
                            newGameContextMenuItem.Click += OpenGameSettingsHandler;
                            newGameContextMenu.Items.Add(newGameContextMenuItem);

                            newGame.ContextMenu = newGameContextMenu;
                        }
                    }
                }
                List<Game> displayedGames = currentGames.Where<Game>((Game game) =>
                {
                    bool isHidden = game.isHidden;
                    bool isDisplayed = !isHidden;
                    return isDisplayed;
                }).ToList<Game>();
                int countdisplayedGames = displayedGames.Count;
                bool isHaveDisplayedGames = countdisplayedGames >= 1;
                if (isHaveDisplayedGames)
                {

                    Game displayedGame = displayedGames[0];
                    string displayedGameName = displayedGame.name;
                    int index = loadedGames.FindIndex((Game game) =>
                    {
                        string gameName = game.name;
                        bool isGameFound = gameName == displayedGameName;
                        return isGameFound;
                    });
                    bool isFound = index >= 0;
                    if (isFound)
                    {
                        Game crossGame = loadedGames[index];
                        Game firstGame = crossGame;
                        Dictionary<String, Object> firstGameData = new Dictionary<String, Object>();
                        string firstGameId = firstGame.id;
                        string firstGameName = firstGame.name;
                        string firstGameUrl = @"";
                        string firstGameImage = @"";
                        int firstGamePrice = 0;
                        Debugger.Log(0, "debug", Environment.NewLine + "firstGameId: " + firstGameId + Environment.NewLine);
                        firstGameData.Add("id", firstGameId);
                        firstGameData.Add("name", firstGameName);
                        firstGameData.Add("url", firstGameUrl);
                        firstGameData.Add("image", firstGameImage);
                        firstGameData.Add("price", firstGamePrice);
                        SelectGame(firstGameData);
                    }
                    else
                    {
                        activeGame.Visibility = invisible;
                    }
                }
                else
                {
                    activeGame.Visibility = invisible;
                }
            }
            else
            {
                activeGame.Visibility = invisible;
            }
        }

        public void GetForums(string keywords)
        {
            string ignoreCaseKeywords = keywords.ToLower();
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/forums/all");
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (var reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        var objText = reader.ReadToEnd();
                        ForumsListResponseInfo myobj = (ForumsListResponseInfo)js.Deserialize(objText, typeof(ForumsListResponseInfo));
                        string status = myobj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
                            UIElementCollection forumsChildren = forums.Children;
                            int countForumsChildren = forumsChildren.Count;
                            for (int i = countForumsChildren - 1; i > 0; i--)
                            {
                                UIElement element = forums.Children[i];
                                int row = Grid.GetRow(element);
                                bool isForumItemElement = row >= 1;
                                if (isForumItemElement)
                                {
                                    forums.Children.Remove(element);
                                }
                            }
                            RowDefinitionCollection rows = forums.RowDefinitions;
                            int countRows = rows.Count;
                            bool isManyRows = countRows >= 2;
                            if (isManyRows)
                            {
                                rows.RemoveRange(1, countRows - 1);
                            }
                            List<Forum> forumsList = myobj.forums;
                            foreach (Forum forumsListItem in forumsList)
                            {
                                string forumId = forumsListItem._id;
                                string forumTitle = forumsListItem.title;
                                string forumIgnoreCaseTitle = forumTitle.ToLower();
                                bool isFilterMatch = forumIgnoreCaseTitle.Contains(ignoreCaseKeywords);
                                int keywordsLength = keywords.Length;
                                bool isEmptyKeywordsLength = keywordsLength <= 0;
                                bool isAddForum = isFilterMatch || isEmptyKeywordsLength;
                                if (isAddForum)
                                {
                                    RowDefinition row = new RowDefinition();
                                    row.Height = new GridLength(50);
                                    forums.RowDefinitions.Add(row);
                                    rows = forums.RowDefinitions;
                                    countRows = rows.Count;
                                    int lastRowIndex = countRows - 1;
                                    StackPanel forumName = new StackPanel();
                                    forumName.Margin = new Thickness(0, 2, 0, 2);
                                    forumName.Background = System.Windows.Media.Brushes.DarkCyan;
                                    TextBlock forumNameLabel = new TextBlock();
                                    forumNameLabel.Foreground = System.Windows.Media.Brushes.White;
                                    forumNameLabel.FontWeight = System.Windows.FontWeights.Bold;
                                    forumNameLabel.Margin = new Thickness(10, 15, 10, 15);
                                    forumNameLabel.Text = forumTitle;
                                    forumName.Children.Add(forumNameLabel);
                                    forums.Children.Add(forumName);
                                    Grid.SetRow(forumName, lastRowIndex);
                                    Grid.SetColumn(forumName, 0);
                                    forumNameLabel.DataContext = forumId;
                                    forumNameLabel.MouseLeftButtonUp += SelectForumHandler;
                                    StackPanel forumLastMsgDate = new StackPanel();
                                    forumLastMsgDate.Margin = new Thickness(0, 2, 0, 2);
                                    forumLastMsgDate.Background = System.Windows.Media.Brushes.DarkCyan;
                                    TextBlock forumLastMsgDateLabel = new TextBlock();
                                    forumLastMsgDateLabel.Foreground = System.Windows.Media.Brushes.White;
                                    forumLastMsgDateLabel.Margin = new Thickness(10, 15, 10, 15);
                                    forumLastMsgDateLabel.Text = "00/00/00";

                                    List<ForumTopicMsg> totalForumMsgs = new List<ForumTopicMsg>();
                                    HttpWebRequest innerWebRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/forum/topics/get/?id=" + forumId);
                                    innerWebRequest.Method = "GET";
                                    innerWebRequest.UserAgent = ".NET Framework Test Client";
                                    using (HttpWebResponse innerWebResponse = (HttpWebResponse)innerWebRequest.GetResponse())
                                    {
                                        using (var innerReader = new StreamReader(innerWebResponse.GetResponseStream()))
                                        {
                                            js = new JavaScriptSerializer();
                                            objText = innerReader.ReadToEnd();
                                            ForumTopicsResponseInfo myInnerObj = (ForumTopicsResponseInfo)js.Deserialize(objText, typeof(ForumTopicsResponseInfo));
                                            status = myInnerObj.status;
                                            isOkStatus = status == "OK";
                                            if (isOkStatus)
                                            {
                                                List<Topic> topics = myInnerObj.topics;
                                                foreach (Topic topic in topics)
                                                {
                                                    string topicId = topic._id;
                                                    HttpWebRequest nestedWebRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/forum/topic/msgs/get/?topic=" + topicId);
                                                    nestedWebRequest.Method = "GET";
                                                    nestedWebRequest.UserAgent = ".NET Framework Test Client";
                                                    using (HttpWebResponse nestedWebResponse = (HttpWebResponse)nestedWebRequest.GetResponse())
                                                    {
                                                        using (var nestedReader = new StreamReader(nestedWebResponse.GetResponseStream()))
                                                        {
                                                            js = new JavaScriptSerializer();
                                                            objText = nestedReader.ReadToEnd();
                                                            ForumTopicMsgsResponseInfo myNestedObj = (ForumTopicMsgsResponseInfo)js.Deserialize(objText, typeof(ForumTopicMsgsResponseInfo));
                                                            status = myNestedObj.status;
                                                            isOkStatus = status == "OK";
                                                            if (isOkStatus)
                                                            {
                                                                List<ForumTopicMsg> msgs = myNestedObj.msgs;
                                                                foreach (ForumTopicMsg msg in msgs)
                                                                {
                                                                    totalForumMsgs.Add(msg);
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    int countTotalForumMsgs = totalForumMsgs.Count;
                                    bool isMultipleMsgs = countTotalForumMsgs >= 2;
                                    bool isOnlyOneMsg = countTotalForumMsgs == 1;
                                    if (isMultipleMsgs)
                                    {
                                        IEnumerable<ForumTopicMsg> orderedMsgs = totalForumMsgs.OrderBy((ForumTopicMsg localMsg) => localMsg.date);
                                        List<ForumTopicMsg> orderedMsgsList = orderedMsgs.ToList<ForumTopicMsg>();
                                        int countMsgs = orderedMsgsList.Count;
                                        int lastMsgIndex = countMsgs - 1;
                                        ForumTopicMsg msg = orderedMsgsList[lastMsgIndex];
                                        DateTime msgDate = msg.date;
                                        string parsedMsgDate = msgDate.ToLongDateString();
                                        forumLastMsgDateLabel.Text = parsedMsgDate;
                                    }
                                    else if (isOnlyOneMsg)
                                    {
                                        IEnumerable<ForumTopicMsg> orderedMsgs = totalForumMsgs.OrderBy((ForumTopicMsg localMsg) => localMsg.date);
                                        List<ForumTopicMsg> orderedMsgsList = orderedMsgs.ToList<ForumTopicMsg>();
                                        ForumTopicMsg msg = orderedMsgsList[0];
                                        DateTime msgDate = msg.date;
                                        string parsedMsgDate = msgDate.ToLongDateString();
                                        forumLastMsgDateLabel.Text = parsedMsgDate;
                                    }
                                    else
                                    {
                                        forumLastMsgDateLabel.Text = "---";
                                    }
                                    forumLastMsgDate.Children.Add(forumLastMsgDateLabel);
                                    forums.Children.Add(forumLastMsgDate);
                                    Grid.SetRow(forumLastMsgDate, lastRowIndex);
                                    Grid.SetColumn(forumLastMsgDate, 1);
                                    StackPanel forumDiscussionsCount = new StackPanel();
                                    forumDiscussionsCount.Margin = new Thickness(0, 2, 0, 2);
                                    forumDiscussionsCount.Background = System.Windows.Media.Brushes.DarkCyan;
                                    TextBlock forumDiscussionsCountLabel = new TextBlock();
                                    forumDiscussionsCountLabel.Foreground = System.Windows.Media.Brushes.White;
                                    forumDiscussionsCountLabel.Margin = new Thickness(10, 15, 10, 15);
                                    innerWebRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/forum/topics/get/?id=" + forumId);
                                    innerWebRequest.Method = "GET";
                                    innerWebRequest.UserAgent = ".NET Framework Test Client";
                                    using (HttpWebResponse innerWebResponse = (HttpWebResponse)innerWebRequest.GetResponse())
                                    {
                                        using (var innerReader = new StreamReader(innerWebResponse.GetResponseStream()))
                                        {
                                            js = new JavaScriptSerializer();
                                            objText = innerReader.ReadToEnd();
                                            ForumTopicsResponseInfo myInnerObj = (ForumTopicsResponseInfo)js.Deserialize(objText, typeof(ForumTopicsResponseInfo));
                                            status = myInnerObj.status;
                                            isOkStatus = status == "OK";
                                            if (isOkStatus)
                                            {
                                                List<Topic> topics = myInnerObj.topics;
                                                int countTopics = topics.Count;
                                                string rawCountTopics = countTopics.ToString();
                                                forumDiscussionsCountLabel.Text = rawCountTopics;
                                            }
                                        }
                                    }
                                    forumDiscussionsCount.Children.Add(forumDiscussionsCountLabel);
                                    forums.Children.Add(forumDiscussionsCount);
                                    Grid.SetRow(forumDiscussionsCount, lastRowIndex);
                                    Grid.SetColumn(forumDiscussionsCount, 2);
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

        public void SelectTopicHandler(object sender, RoutedEventArgs e)
        {
            TextBlock topicNameLabel = ((TextBlock)(sender));
            object topicData = topicNameLabel.DataContext;
            string topicId = ((string)(topicData));
            SelectTopic(topicId);
        }

        public void SelectTopic(string id)
        {

            int countResultPerPage = 15;
            object rawCountMsgs = forumTopicCountMsgs.DataContext;
            bool isNotData = rawCountMsgs == null;
            bool isHaveData = !isNotData;
            if (isHaveData)
            {
                int countMsgs = ((int)(rawCountMsgs));
                countResultPerPage = countMsgs;
            }

            addDiscussionMsgBtn.DataContext = id;
            mainControl.SelectedIndex = 8;
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/forums/topics/get/?id=" + id);
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (var reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        var objText = reader.ReadToEnd();
                        ForumTopicResponseInfo myobj = (ForumTopicResponseInfo)js.Deserialize(objText, typeof(ForumTopicResponseInfo));
                        string status = myobj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
                            Topic topic = myobj.topic;
                            string userId = topic.user;
                            string title = topic.title;
                            activeTopicNameLabel.Text = title;
                            HttpWebRequest innerWebRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/forum/topic/msgs/get/?topic=" + id);
                            innerWebRequest.Method = "GET";
                            innerWebRequest.UserAgent = ".NET Framework Test Client";
                            using (HttpWebResponse innerWebResponse = (HttpWebResponse)innerWebRequest.GetResponse())
                            {
                                using (var innerReader = new StreamReader(innerWebResponse.GetResponseStream()))
                                {
                                    js = new JavaScriptSerializer();
                                    objText = innerReader.ReadToEnd();
                                    ForumTopicMsgsResponseInfo myInnerObj = (ForumTopicMsgsResponseInfo)js.Deserialize(objText, typeof(ForumTopicMsgsResponseInfo));
                                    status = myInnerObj.status;
                                    isOkStatus = status == "OK";
                                    if (isOkStatus)
                                    {
                                        forumTopicMsgs.Children.Clear();
                                        List<ForumTopicMsg> msgs = myInnerObj.msgs;
                                        int msgsCount = msgs.Count;
                                        string rawMsgsCount = msgsCount.ToString();

                                        // string forumTopicMsgsCountLabelContent = "Сообщения 0 - 0 из " + rawMsgsCount;

                                        int msgsCursor = -1;

                                        object currentPageNumber = forumTopicPages.DataContext;
                                        string rawCurrentPageNumber = currentPageNumber.ToString();
                                        int currentPage = Int32.Parse(rawCurrentPageNumber);
                                        int currentPageIndex = currentPage - 1;

                                        forumTopicPages.Children.Clear();
                                        bool isHaveMsgs = msgsCount >= 1;
                                        if (isHaveMsgs)
                                        {
                                            foreach (ForumTopicMsg msg in msgs)
                                            {
                                                msgsCursor++;

                                                int countPages = forumTopicPages.Children.Count;
                                                bool isAddPageLabel = msgsCursor == countResultPerPage * countPages;
                                                if (isAddPageLabel)
                                                {
                                                    TextBlock forumTopicPageLabel = new TextBlock();
                                                    int pageNumber = countPages + 1;
                                                    string rawPageNumber = pageNumber.ToString();
                                                    forumTopicPageLabel.Text = rawPageNumber;
                                                    forumTopicPageLabel.DataContext = pageNumber;
                                                    forumTopicPageLabel.MouseLeftButtonUp += SelectForumTopicPageHandler;
                                                    forumTopicPageLabel.Margin = new Thickness(10, 0, 10, 0);
                                                    forumTopicPages.Children.Add(forumTopicPageLabel);
                                                    bool isCurrentPageLabel = rawCurrentPageNumber == rawPageNumber;
                                                    if (isCurrentPageLabel)
                                                    {
                                                        forumTopicPageLabel.Foreground = System.Windows.Media.Brushes.DarkCyan;
                                                    }
                                                    else
                                                    {
                                                        forumTopicPageLabel.Foreground = System.Windows.Media.Brushes.White;
                                                    }
                                                }

                                                bool isMsgForCurrentPage = msgsCursor < countResultPerPage * currentPage && (msgsCursor >= countResultPerPage * currentPage - countResultPerPage);
                                                if (isMsgForCurrentPage)
                                                {
                                                    string msgUserId = msg.user;
                                                    DateTime msgDate = msg.date;
                                                    string rawMsgDate = msgDate.ToLongDateString();
                                                    StackPanel forumTopicMsg = new StackPanel();
                                                    forumTopicMsg.Background = System.Windows.Media.Brushes.LightGray;
                                                    forumTopicMsg.Margin = new Thickness(10);
                                                    StackPanel msgHeader = new StackPanel();
                                                    msgHeader.Margin = new Thickness(10);
                                                    msgHeader.Orientation = Orientation.Horizontal;
                                                    Image msgHeaderUserAvatar = new Image();
                                                    msgHeaderUserAvatar.Margin = new Thickness(10, 0, 10, 0);
                                                    msgHeaderUserAvatar.Width = 25;
                                                    msgHeaderUserAvatar.Height = 25;
                                                    msgHeaderUserAvatar.BeginInit();

                                                    msgHeaderUserAvatar.Source = new BitmapImage(new Uri(@"http://localhost:4000/api/user/avatar/?id=" + msgUserId));
                                                    // msgHeaderUserAvatar.Source = new BitmapImage(new Uri(@"https://cdn2.iconfinder.com/data/icons/ios-7-icons/50/user_male-128.png"));

                                                    msgHeaderUserAvatar.EndInit();
                                                    msgHeader.Children.Add(msgHeaderUserAvatar);
                                                    TextBlock msgHeaderUserNameLabel = new TextBlock();
                                                    msgHeaderUserNameLabel.Margin = new Thickness(10, 0, 10, 0);
                                                    msgHeaderUserNameLabel.Text = "Пользователь";
                                                    msgHeader.Children.Add(msgHeaderUserNameLabel);
                                                    TextBlock msgHeaderDateLabel = new TextBlock();
                                                    msgHeaderDateLabel.Margin = new Thickness(10, 0, 10, 0);
                                                    msgHeaderDateLabel.Text = rawMsgDate;
                                                    msgHeader.Children.Add(msgHeaderDateLabel);
                                                    forumTopicMsg.Children.Add(msgHeader);
                                                    string msgContent = msg.content;
                                                    TextBlock msgContentLabel = new TextBlock();
                                                    msgContentLabel.Margin = new Thickness(10);
                                                    msgContentLabel.Text = msgContent;
                                                    forumTopicMsg.Children.Add(msgContentLabel);
                                                    StackPanel msgFooter = new StackPanel();
                                                    msgFooter.Margin = new Thickness(10);
                                                    msgFooter.Orientation = Orientation.Horizontal;
                                                    TextBlock msgFooterEditLabel = new TextBlock();
                                                    msgFooterEditLabel.Margin = new Thickness(10, 0, 10, 0);
                                                    msgFooterEditLabel.Text = "Отредактировано пользователем: " + rawMsgDate;
                                                    msgFooter.Children.Add(msgFooterEditLabel);
                                                    TextBlock msgFooterNumberLabel = new TextBlock();
                                                    msgFooterNumberLabel.Margin = new Thickness(10, 0, 10, 0);
                                                    msgFooterNumberLabel.TextAlignment = TextAlignment.Right;
                                                    int msgNumber = msgsCursor + 1;
                                                    string rawMsgNumber = msgNumber.ToString();
                                                    string msgFooterNumberLabelContent = "#" + rawMsgNumber;
                                                    msgFooterNumberLabel.Text = msgFooterNumberLabelContent;
                                                    msgFooter.Children.Add(msgFooterNumberLabel);
                                                    forumTopicMsg.Children.Add(msgFooter);
                                                    forumTopicMsgs.Children.Add(forumTopicMsg);

                                                    HttpWebRequest nestedWebRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/users/get?id=" + msgUserId);
                                                    nestedWebRequest.Method = "GET";
                                                    nestedWebRequest.UserAgent = ".NET Framework Test Client";
                                                    using (HttpWebResponse nestedWebResponse = (HttpWebResponse)nestedWebRequest.GetResponse())
                                                    {
                                                        using (var nestedReader = new StreamReader(nestedWebResponse.GetResponseStream()))
                                                        {
                                                            js = new JavaScriptSerializer();
                                                            objText = nestedReader.ReadToEnd();
                                                            UserResponseInfo myNestedObj = (UserResponseInfo)js.Deserialize(objText, typeof(UserResponseInfo));
                                                            status = myNestedObj.status;
                                                            isOkStatus = status == "OK";
                                                            if (isOkStatus)
                                                            {
                                                                User user = myNestedObj.user;
                                                                string userName = user.name;
                                                                msgHeaderUserNameLabel.Text = userName;
                                                                msgFooterEditLabel.Text = "Отредактировано " + userName + ": " + rawMsgDate;
                                                            }
                                                        }
                                                    }

                                                }
                                            }

                                            int firstMsgIndex = countResultPerPage * currentPage - countResultPerPage;
                                            int firstMsgNumber = firstMsgIndex + 1;
                                            int rawFirstMsgNumber = firstMsgNumber;

                                            // int lastMsgIndex = countResultPerPage * currentPage;
                                            // int lastMsgNumber = lastMsgIndex + 1;

                                            int lastMsgNumber = countResultPerPage * currentPage;

                                            int rawLastMsgNumber = lastMsgNumber;
                                            string forumTopicMsgsCountLabelContent = "Сообщения " + rawFirstMsgNumber + " - " + rawLastMsgNumber + " из " + rawMsgsCount;
                                            forumTopicMsgsCountLabel.Text = forumTopicMsgsCountLabelContent;

                                            UpdatePaginationPointers(currentPage);

                                        }
                                        else
                                        {
                                            TextBlock forumTopicPageLabel = new TextBlock();
                                            int pageNumber = 1;
                                            string rawPageNumber = pageNumber.ToString();
                                            forumTopicPageLabel.Text = rawPageNumber;
                                            forumTopicPageLabel.DataContext = pageNumber;
                                            forumTopicPageLabel.MouseLeftButtonUp += SelectForumTopicPageHandler;
                                            forumTopicPageLabel.Margin = new Thickness(10, 0, 10, 0);
                                            forumTopicPages.Children.Add(forumTopicPageLabel);
                                            bool isCurrentPageLabel = rawCurrentPageNumber == rawPageNumber;
                                            if (isCurrentPageLabel)
                                            {
                                                forumTopicPageLabel.Foreground = System.Windows.Media.Brushes.DarkCyan;
                                            }
                                            else
                                            {
                                                forumTopicPageLabel.Foreground = System.Windows.Media.Brushes.White;
                                            }

                                            int rawFirstMsgNumber = 0;
                                            int rawLastMsgNumber = 0;
                                            string forumTopicMsgsCountLabelContent = "Сообщения " + rawFirstMsgNumber + " - " + rawLastMsgNumber + " из " + rawMsgsCount;
                                            forumTopicMsgsCountLabel.Text = forumTopicMsgsCountLabelContent;

                                            UpdatePaginationPointers(1);

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

            addDiscussionMsgUserAvatar.Source = new BitmapImage(new Uri(@"http://localhost:4000/api/user/avatar/?id=" + currentUserId));

        }

        public void SelectForumHandler(object sender, RoutedEventArgs e)
        {
            TextBlock forumNameLabel = ((TextBlock)(sender));
            object forumData = forumNameLabel.DataContext;
            string forumId = ((string)(forumData));
            SelectForum(forumId);
        }

        public void SelectForum(string id)
        {
            addDiscussionDialog.Visibility = invisible;
            addDiscussionBtn.DataContext = id;
            mainControl.SelectedIndex = 7;
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/forums/get/?id=" + id);
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (var reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        var objText = reader.ReadToEnd();
                        ForumResponseInfo myobj = (ForumResponseInfo)js.Deserialize(objText, typeof(ForumResponseInfo));
                        string status = myobj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
                            Forum currentForum = myobj.forum;
                            string title = currentForum.title;
                            activeForumNameLabel.Text = title;
                            HttpWebRequest innerWebRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/forum/topics/get/?id=" + id);
                            innerWebRequest.Method = "GET";
                            innerWebRequest.UserAgent = ".NET Framework Test Client";
                            using (HttpWebResponse innerWebResponse = (HttpWebResponse)innerWebRequest.GetResponse())
                            {
                                using (var innerReader = new StreamReader(innerWebResponse.GetResponseStream()))
                                {
                                    js = new JavaScriptSerializer();
                                    objText = innerReader.ReadToEnd();
                                    ForumTopicsResponseInfo myInnerObj = (ForumTopicsResponseInfo)js.Deserialize(objText, typeof(ForumTopicsResponseInfo));
                                    status = myInnerObj.status;
                                    isOkStatus = status == "OK";
                                    if (isOkStatus)
                                    {
                                        forumTopics.Children.Clear();
                                        RowDefinitionCollection rows = forumTopics.RowDefinitions;
                                        int countRows = rows.Count;
                                        bool isHaveRows = countRows >= 1;
                                        if (isHaveRows)
                                        {
                                            forumTopics.RowDefinitions.RemoveRange(0, forumTopics.RowDefinitions.Count);
                                        }
                                        List<Topic> topics = myInnerObj.topics;
                                        int topicsCursor = -1;
                                        foreach (Topic topic in topics)
                                        {
                                            topicsCursor++;
                                            string topicId = topic._id;
                                            string topicTitle = topic.title;
                                            string userId = topic.user;
                                            RowDefinition row = new RowDefinition();
                                            row.Height = new GridLength(65);
                                            forumTopics.RowDefinitions.Add(row);
                                            rows = forums.RowDefinitions;
                                            countRows = rows.Count;
                                            int lastRowIndex = countRows - 1;
                                            StackPanel topicHeader = new StackPanel();
                                            topicHeader.Margin = new Thickness(0, 2, 0, 2);
                                            topicHeader.Background = System.Windows.Media.Brushes.DarkCyan;
                                            topicHeader.Orientation = Orientation.Horizontal;
                                            PackIcon topicHeaderIcon = new PackIcon();
                                            topicHeaderIcon.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                                            topicHeaderIcon.Margin = new Thickness(10, 0, 10, 0);
                                            topicHeaderIcon.Kind = PackIconKind.Email;
                                            topicHeaderIcon.Foreground = System.Windows.Media.Brushes.SkyBlue;
                                            topicHeader.Children.Add(topicHeaderIcon);
                                            StackPanel topicHeaderAside = new StackPanel();
                                            TextBlock topicNameLabel = new TextBlock();
                                            topicNameLabel.Foreground = System.Windows.Media.Brushes.White;
                                            topicNameLabel.FontWeight = System.Windows.FontWeights.Bold;
                                            topicNameLabel.Margin = new Thickness(0, 5, 0, 5);
                                            topicNameLabel.Text = topicTitle;
                                            topicHeaderAside.Children.Add(topicNameLabel);
                                            TextBlock topicAuthorLabel = new TextBlock();
                                            topicAuthorLabel.Margin = new Thickness(0, 5, 0, 5);
                                            topicAuthorLabel.Foreground = System.Windows.Media.Brushes.SkyBlue;
                                            topicAuthorLabel.Text = "Пользователь";

                                            HttpWebRequest nestedWebRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/users/get?id=" + userId);
                                            nestedWebRequest.Method = "GET";
                                            nestedWebRequest.UserAgent = ".NET Framework Test Client";
                                            using (HttpWebResponse nestedWebResponse = (HttpWebResponse)nestedWebRequest.GetResponse())
                                            {
                                                using (var nestedReader = new StreamReader(nestedWebResponse.GetResponseStream()))
                                                {
                                                    js = new JavaScriptSerializer();
                                                    objText = nestedReader.ReadToEnd();
                                                    UserResponseInfo myNestedObj = (UserResponseInfo)js.Deserialize(objText, typeof(UserResponseInfo));
                                                    status = myNestedObj.status;
                                                    isOkStatus = status == "OK";
                                                    if (isOkStatus)
                                                    {
                                                        User user = myNestedObj.user;
                                                        string userName = user.name;
                                                        topicAuthorLabel.Text = userName;
                                                    }
                                                }
                                            }

                                            topicHeaderAside.Children.Add(topicAuthorLabel);
                                            topicHeader.Children.Add(topicHeaderAside);
                                            forumTopics.Children.Add(topicHeader);
                                            Grid.SetRow(topicHeader, topicsCursor);
                                            Grid.SetColumn(topicHeader, 0);
                                            topicNameLabel.DataContext = topicId;
                                            topicNameLabel.MouseLeftButtonUp += SelectTopicHandler;
                                            StackPanel topicLastMsgDate = new StackPanel();
                                            topicLastMsgDate.Margin = new Thickness(0, 2, 0, 2);
                                            topicLastMsgDate.Background = System.Windows.Media.Brushes.DarkCyan;
                                            topicLastMsgDate.Height = 65;
                                            TextBlock topicLastMsgDateLabel = new TextBlock();
                                            topicLastMsgDateLabel.Foreground = System.Windows.Media.Brushes.SkyBlue;
                                            topicLastMsgDateLabel.Height = 65;
                                            topicLastMsgDateLabel.Margin = new Thickness(15);
                                            topicLastMsgDateLabel.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                                            topicLastMsgDateLabel.Text = "00/00/00";
                                            topicLastMsgDate.Children.Add(topicLastMsgDateLabel);
                                            forumTopics.Children.Add(topicLastMsgDate);
                                            Grid.SetRow(topicLastMsgDate, topicsCursor);
                                            Grid.SetColumn(topicLastMsgDate, 1);
                                            DockPanel forumMsgsCount = new DockPanel();
                                            forumMsgsCount.Margin = new Thickness(0, 2, 0, 2);
                                            forumMsgsCount.Height = 65;
                                            forumMsgsCount.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                                            forumMsgsCount.Background = System.Windows.Media.Brushes.DarkCyan;
                                            PackIcon forumMsgsCountIcon = new PackIcon();
                                            forumMsgsCountIcon.Foreground = System.Windows.Media.Brushes.White;
                                            forumMsgsCountIcon.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                                            forumMsgsCountIcon.Kind = PackIconKind.ChatBubble;
                                            forumMsgsCountIcon.Margin = new Thickness(10, 0, 10, 0);
                                            forumMsgsCount.Children.Add(forumMsgsCountIcon);
                                            TextBlock forumMsgsCountLabel = new TextBlock();
                                            forumMsgsCountLabel.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                                            forumMsgsCountLabel.Margin = new Thickness(10, 0, 10, 0);
                                            forumMsgsCountLabel.Foreground = System.Windows.Media.Brushes.White;

                                            nestedWebRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/forum/topic/msgs/get/?topic=" + topicId);
                                            nestedWebRequest.Method = "GET";
                                            nestedWebRequest.UserAgent = ".NET Framework Test Client";
                                            using (HttpWebResponse nestedWebResponse = (HttpWebResponse)nestedWebRequest.GetResponse())
                                            {
                                                using (var nestedReader = new StreamReader(nestedWebResponse.GetResponseStream()))
                                                {
                                                    js = new JavaScriptSerializer();
                                                    objText = nestedReader.ReadToEnd();
                                                    ForumTopicMsgsResponseInfo myNestedObj = (ForumTopicMsgsResponseInfo)js.Deserialize(objText, typeof(ForumTopicMsgsResponseInfo));
                                                    status = myNestedObj.status;
                                                    isOkStatus = status == "OK";
                                                    if (isOkStatus)
                                                    {
                                                        List<ForumTopicMsg> msgs = myNestedObj.msgs;
                                                        int countMsgs = msgs.Count;
                                                        string rawCountMsgs = countMsgs.ToString();
                                                        forumMsgsCountLabel.Text = rawCountMsgs;
                                                        bool isMultipleMsgs = countMsgs >= 2;
                                                        bool isOnlyOneMsg = countMsgs == 1;
                                                        if (isMultipleMsgs)
                                                        {
                                                            IEnumerable<ForumTopicMsg> orderedMsgs = msgs.OrderBy((ForumTopicMsg localMsg) => localMsg.date);
                                                            List<ForumTopicMsg> orderedMsgsList = orderedMsgs.ToList<ForumTopicMsg>();
                                                            int lastMsgIndex = countMsgs - 1;
                                                            ForumTopicMsg msg = orderedMsgsList[lastMsgIndex];
                                                            DateTime msgDate = msg.date;
                                                            string parsedMsgDate = msgDate.ToLongDateString();
                                                            topicLastMsgDateLabel.Text = parsedMsgDate;
                                                        }
                                                        else if (isOnlyOneMsg)
                                                        {
                                                            IEnumerable<ForumTopicMsg> orderedMsgs = msgs.OrderBy((ForumTopicMsg localMsg) => localMsg.date);
                                                            List<ForumTopicMsg> orderedMsgsList = orderedMsgs.ToList<ForumTopicMsg>();
                                                            ForumTopicMsg msg = orderedMsgsList[0];
                                                            DateTime msgDate = msg.date;
                                                            string parsedMsgDate = msgDate.ToLongDateString();
                                                            topicLastMsgDateLabel.Text = parsedMsgDate;
                                                        }
                                                        else
                                                        {
                                                            topicLastMsgDateLabel.Text = "---";
                                                        }
                                                    }
                                                }
                                            }

                                            forumMsgsCount.Children.Add(forumMsgsCountLabel);
                                            forumTopics.Children.Add(forumMsgsCount);
                                            Grid.SetRow(forumMsgsCount, topicsCursor);
                                            Grid.SetColumn(forumMsgsCount, 2);
                                            Debugger.Log(0, "debug", Environment.NewLine + "forumTopics.RowDefinitions.Count: " + forumTopics.RowDefinitions.Count.ToString() + ", topicsCursor: " + topicsCursor.ToString() + ", lastRowIndex: " + lastRowIndex.ToString() + Environment.NewLine);
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

        public void SetStatsChart()
        {
            /*Sparrow.Chart.ChartPoint point = new Sparrow.Chart.ChartPoint();
            PointsCollection points = new PointsCollection();
            ChartPoint chartPoint = new ChartPoint();
            points.Add(chartPoint);
            points.Add(new ChartPoint());
            points.Add(new ChartPoint());
            points.Add(new ChartPoint());
            points.Add(new ChartPoint());
            var asss = new Sparrow.Chart.AreaSeries()
            {
                Points = points
            };
            chartUsersStats.Series.Add(asss);*/

            /*List<CPU> source = new List<CPU>();
            DateTime dt = DateTime.Now;
            System.Random rad = new Random(System.DateTime.Now.Millisecond);
            for (int n = 0; n < 100; n++)
            {
                dt = dt.AddSeconds(1);
                CPU cpu = new CPU(dt, rad.Next(100), 51);
                source.Add(cpu);
            }
            ((Sparrow.Chart.LineSeries)(chartUsersStats.Series[0])).PointsSource = source;*/

            GenerateDatas();

        }

        public void GetDownloads()
        {
            Environment.SpecialFolder localApplicationDataFolder = Environment.SpecialFolder.LocalApplicationData;
            string localApplicationDataFolderPath = Environment.GetFolderPath(localApplicationDataFolder);
            string saveDataFilePath = localApplicationDataFolderPath + @"\OfficeWare\GameManager\" + currentUserId + @"\save-data.txt";
            JavaScriptSerializer js = new JavaScriptSerializer();
            string saveDataFileContent = File.ReadAllText(saveDataFilePath);
            SavedContent loadedContent = js.Deserialize<SavedContent>(saveDataFileContent);
            List<Game> currentGames = loadedContent.games;
            int dowloadsCursor = 0;
            downloads.Children.Clear();
            downloads.RowDefinitions.Clear();
            foreach (Game currentGame in currentGames)
            {
                string currentGameId = currentGame.id;
                string currentGameName = currentGame.name;
                string currentGamePath = currentGame.path;
                string currentGameInstallDate = currentGame.installDate;
                try
                {
                    HttpWebRequest innerWebRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/games/get");
                    innerWebRequest.Method = "GET";
                    innerWebRequest.UserAgent = ".NET Framework Test Client";
                    using (HttpWebResponse webResponse = (HttpWebResponse)innerWebRequest.GetResponse())
                    {
                        using (var reader = new StreamReader(webResponse.GetResponseStream()))
                        {
                            js = new JavaScriptSerializer();
                            var objText = reader.ReadToEnd();

                            GamesListResponseInfo myobj = (GamesListResponseInfo)js.Deserialize(objText, typeof(GamesListResponseInfo));

                            string status = myobj.status;
                            bool isOkStatus = status == "OK";
                            if (isOkStatus)
                            {
                                List<GameResponseInfo> games = myobj.games;
                                List<GameResponseInfo> gameResults = games.Where<GameResponseInfo>((GameResponseInfo game) =>
                                {
                                    string gameName = game.name;
                                    bool isNamesMatches = game.name == currentGameName;
                                    return isNamesMatches;
                                }).ToList<GameResponseInfo>();
                                int countResults = gameResults.Count;
                                bool isResultsFound = countResults >= 1;
                                if (isResultsFound)
                                {
                                    GameResponseInfo foundedGame = gameResults[0];
                                    string currentGameImg = foundedGame.image;
                                    // string gameName = foundedGame.name;
                                    dowloadsCursor++;
                                    try
                                    {
                                        FileInfo currentGameInfo = new FileInfo(currentGamePath);
                                        long currentGameSize = currentGameInfo.Length;
                                        double currentGameSizeInGb = currentGameSize / 1024 / 1024 / 1024;
                                        string rawCurrentGameSize = currentGameSizeInGb + " Гб";
                                        RowDefinition download = new RowDefinition();
                                        download.Height = new GridLength(275);
                                        downloads.RowDefinitions.Add(download);
                                        RowDefinitionCollection rows = downloads.RowDefinitions;
                                        int rowsCount = rows.Count;
                                        int lastRowIndex = rowsCount - 1;
                                        Image downloadImg = new Image();
                                        downloadImg.BeginInit();
                                        // downloadImg.Source = new BitmapImage(new Uri(currentGameImg));
                                        Uri source = new Uri(@"http://localhost:4000/api/game/thumbnail/?name=" + currentGameName);
                                        downloadImg.Source = new BitmapImage();
                                        downloadImg.EndInit();
                                        downloadImg.Margin = new Thickness(15, 0, 15, 0);
                                        downloads.Children.Add(downloadImg);
                                        Grid.SetRow(downloadImg, lastRowIndex);
                                        Grid.SetColumn(downloadImg, 0);
                                        StackPanel downloadInfo = new StackPanel();
                                        TextBlock downloadNameLabel = new TextBlock();
                                        downloadNameLabel.Text = currentGameName;
                                        downloadNameLabel.FontSize = 24;
                                        downloadNameLabel.Margin = new Thickness(15, 0, 15, 0);
                                        downloadInfo.Children.Add(downloadNameLabel);
                                        TextBlock downloadSizeLabel = new TextBlock();
                                        downloadSizeLabel.Text = rawCurrentGameSize;
                                        downloadSizeLabel.Margin = new Thickness(15, 0, 15, 0);
                                        downloadInfo.Children.Add(downloadSizeLabel);
                                        downloads.Children.Add(downloadInfo);
                                        Grid.SetRow(downloadInfo, lastRowIndex);
                                        Grid.SetColumn(downloadInfo, 1);
                                        TextBlock downloadDateLabel = new TextBlock();
                                        downloadDateLabel.Text = currentGameInstallDate;
                                        downloadDateLabel.Margin = new Thickness(15, 0, 15, 0);
                                        downloads.Children.Add(downloadDateLabel);
                                        Grid.SetRow(downloadDateLabel, lastRowIndex);
                                        Grid.SetColumn(downloadDateLabel, 2);
                                    }
                                    catch (FileNotFoundException e)
                                    {

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
            countDownloadsLabel.Text = "Загрузки (" + dowloadsCursor + ")";
        }

        public void GetOnlineFriends()
        {

            string friendsListLabelHeaderContent = Properties.Resources.friendsListLabelContent;
            string onlineLabelContent = Properties.Resources.onlineLabelContent;
            friendsListLabel.Header = friendsListLabelHeaderContent;
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
                        string objText = reader.ReadToEnd();
                        FriendsResponseInfo myObj = (FriendsResponseInfo)js.Deserialize(objText, typeof(FriendsResponseInfo));
                        string status = myObj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
                            List<Friend> myFriends = myObj.friends.Where<Friend>((Friend joint) =>
                            {
                                string userId = joint.user;
                                bool isMyFriend = userId == currentUserId;
                                return isMyFriend;
                            }).ToList<Friend>();
                            List<Friend> myOnlineFriends = myFriends.Where<Friend>((Friend friend) =>
                            {
                                string friendId = friend.friend;
                                bool isUserOnline = false;
                                HttpWebRequest innerWebRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/users/get?id=" + friendId);
                                innerWebRequest.Method = "GET";
                                innerWebRequest.UserAgent = ".NET Framework Test Client";
                                using (HttpWebResponse innerWebResponse = (HttpWebResponse)innerWebRequest.GetResponse())
                                {
                                    using (var innerReader = new StreamReader(innerWebResponse.GetResponseStream()))
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
                                            isUserOnline = userStatus == "online";
                                        }
                                    }
                                }
                                return isUserOnline;
                            }).ToList<Friend>();
                            int countOnlineFriends = myOnlineFriends.Count;
                            string rawCountOnlineFriends = countOnlineFriends.ToString();
                            string friendsOnlineCountLabelContent = " (" + onlineLabelContent + " " + rawCountOnlineFriends + ")";
                            friendsListLabel.Header += friendsOnlineCountLabelContent;
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

        public void ResetEditInfoHandler(object sender, RoutedEventArgs e)
        {
            GetEditInfo();
        }

        public void CheckFriendsCache()
        {
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
                            List<Friend> friendRecords = myobj.friends.Where<Friend>((Friend joint) =>
                            {
                                string userId = joint.user;
                                bool isMyFriend = userId == currentUserId;
                                return isMyFriend;
                            }).ToList<Friend>();
                            List<string> friendsIds = new List<string>();
                            foreach (Friend friendRecord in friendRecords)
                            {
                                string localFriendId = friendRecord.friend;
                                friendsIds.Add(localFriendId);
                            }
                            Environment.SpecialFolder localApplicationDataFolder = Environment.SpecialFolder.LocalApplicationData;
                            string localApplicationDataFolderPath = Environment.GetFolderPath(localApplicationDataFolder);
                            string saveDataFilePath = localApplicationDataFolderPath + @"\OfficeWare\GameManager\" + currentUserId + @"\save-data.txt";
                            js = new JavaScriptSerializer();
                            string saveDataFileContent = File.ReadAllText(saveDataFilePath);
                            SavedContent loadedContent = js.Deserialize<SavedContent>(saveDataFileContent);
                            List<Game> currentGames = loadedContent.games;
                            Settings currentSettings = loadedContent.settings;
                            List<string> currentCollections = loadedContent.collections;
                            Notifications currentNotifications = loadedContent.notifications;
                            List<FriendSettings> updatedFriends = loadedContent.friends;
                            int updatedFriendsCount = updatedFriends.Count;
                            for (int i = 0; i < updatedFriendsCount; i++)
                            {
                                FriendSettings currentFriend = updatedFriends[i];
                                string currentFriendId = currentFriend.id;
                                bool isFriendExists = friendsIds.Contains(currentFriendId);
                                bool isFriendNotExists = !isFriendExists;
                                if (isFriendNotExists)
                                {
                                    updatedFriends.Remove(currentFriend);
                                }
                            }
                            string savedContent = js.Serialize(new SavedContent
                            {
                                games = currentGames,
                                friends = updatedFriends,
                                settings = currentSettings,
                                collections = currentCollections,
                                notifications = currentNotifications
                            });
                            File.WriteAllText(saveDataFilePath, savedContent);
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

        public void GetGamesStats()
        {
            DateTime currentDate = DateTime.Now;
            int hours = currentDate.Hour;
            int minutes = currentDate.Minute;
            string rawHours = hours.ToString();
            int measureLength = rawHours.Length;
            bool isAddPrefix = measureLength <= 1;
            if (isAddPrefix)
            {
                rawHours = "0" + rawHours;
            }
            string rawMinutes = minutes.ToString();
            measureLength = rawMinutes.Length;
            isAddPrefix = measureLength <= 1;
            if (isAddPrefix)
            {
                rawMinutes = "0" + rawMinutes;
            }
            string time = rawHours + ":" + rawMinutes;
            int day = currentDate.Day;
            string rawDay = day.ToString();
            List<string> monthLabels = new List<string>() {
                "января",
                "февраля",
                "марта",
                "апреля",
                "мая",
                "июня",
                "июля",
                "августа",
                "сентября",
                "октября",
                "ноября",
                "декабря"
            };
            int month = currentDate.Month;
            string rawMonthLabel = monthLabels[month];
            int year = currentDate.Year;
            string rawYear = year.ToString();
            string date = rawDay + " " + rawMonthLabel + " " + rawYear;
            statsHeaderLabel.Text = "СТАТИСТИКА Office Game Manager И ИГРОВАЯ СТАТИСТИКА: " + date + " В " + time;
            HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/users/stats/get");
            webRequest.Method = "GET";
            webRequest.UserAgent = ".NET Framework Test Client";
            using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
            {
                using (var reader = new StreamReader(webResponse.GetResponseStream()))
                {
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    var objText = reader.ReadToEnd();

                    GamesStatsResponseInfo myobj = (GamesStatsResponseInfo)js.Deserialize(objText, typeof(GamesStatsResponseInfo));

                    string status = myobj.status;
                    bool isOkStatus = status == "OK";
                    if (isOkStatus)
                    {
                        int countUsers = myobj.users;
                        int countMaxUsers = myobj.maxUsers;
                        string rawCountUsers = countUsers.ToString();
                        string rawCountMaxUsers = countMaxUsers.ToString();
                        countLifeUsersLabel.Text = rawCountUsers;
                        countMaxUsersLabel.Text = rawCountMaxUsers;
                    }
                }
            }

            try
            {
                webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/games/get");
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (var reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        var objText = reader.ReadToEnd();

                        GamesListResponseInfo myobj = (GamesListResponseInfo)js.Deserialize(objText, typeof(GamesListResponseInfo));

                        string status = myobj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
                            UIElementCollection items = popularGames.Children;
                            int countItems = items.Count;
                            bool isGamesExists = countItems >= 4;
                            if (isGamesExists)
                            {
                                items = popularGames.Children;
                                countItems = items.Count;
                                int countRemovedItems = countItems - 3;
                                popularGames.Children.RemoveRange(3, countRemovedItems);
                            }
                            RowDefinitionCollection rows = popularGames.RowDefinitions;
                            int countRows = rows.Count;
                            isGamesExists = countRows >= 2;
                            if (isGamesExists)
                            {
                                rows = popularGames.RowDefinitions;
                                countRows = rows.Count;
                                int countRemovedRows = countRows - 1;
                                popularGames.RowDefinitions.RemoveRange(1, countRemovedRows);
                            }
                            foreach (GameResponseInfo gamesItem in myobj.games)
                            {
                                int gameUsers = gamesItem.users;
                                string rawGameUsers = gameUsers.ToString();
                                int gameMaxUsers = gamesItem.maxUsers;
                                string rawGameMaxUsers = gameMaxUsers.ToString();
                                string gameName = gamesItem.name;
                                RowDefinition row = new RowDefinition();
                                row.Height = new GridLength(35);
                                popularGames.RowDefinitions.Add(row);
                                countRows = popularGames.RowDefinitions.Count;
                                int gameIndex = countRows - 1;
                                TextBlock popularGameUsersLabel = new TextBlock();
                                popularGameUsersLabel.Text = rawGameUsers;
                                popularGames.Children.Add(popularGameUsersLabel);
                                Grid.SetRow(popularGameUsersLabel, gameIndex);
                                Grid.SetColumn(popularGameUsersLabel, 0);
                                TextBlock popularGameMaxUsersLabel = new TextBlock();
                                popularGameMaxUsersLabel.Text = rawGameMaxUsers;
                                popularGames.Children.Add(popularGameMaxUsersLabel);
                                Grid.SetRow(popularGameMaxUsersLabel, gameIndex);
                                Grid.SetColumn(popularGameMaxUsersLabel, 1);
                                TextBlock popularGameNameLabel = new TextBlock();
                                popularGameNameLabel.Text = gameName;
                                popularGames.Children.Add(popularGameNameLabel);
                                Grid.SetRow(popularGameNameLabel, gameIndex);
                                Grid.SetColumn(popularGameNameLabel, 2);
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

        public void GetEditInfo()
        {

            editProfileAvatarImg.BeginInit();
            editProfileAvatarImg.Source = new BitmapImage(new Uri("http://localhost:4000/api/user/avatar/?id=" + currentUserId));
            editProfileAvatarImg.EndInit();

            JavaScriptSerializer js = new JavaScriptSerializer();

            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/friends/get");
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (var reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        // js = new JavaScriptSerializer();
                        var objText = reader.ReadToEnd();

                        FriendsResponseInfo myobj = (FriendsResponseInfo)js.Deserialize(objText, typeof(FriendsResponseInfo));

                        string status = myobj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
                            List<Friend> friendsIds = myobj.friends.Where<Friend>((Friend joint) =>
                            {
                                string userId = joint.user;
                                bool isMyFriend = userId == currentUserId;
                                return isMyFriend;
                            }).ToList<Friend>();
                            int countFriends = friendsIds.Count;
                            string rawCountFriends = countFriends.ToString();
                            countFriendsLabel.Text = rawCountFriends;
                            string currentUserName = currentUser.name;
                            string currentUserCountry = currentUser.country;
                            string currentUserAbout = currentUser.about;
                            userEditProfileNameLabel.Text = currentUserName;
                            userNameBox.Text = currentUserName;
                            ItemCollection userCountryBoxItems = userCountryBox.Items;
                            foreach (ComboBoxItem userCountryBoxItem in userCountryBoxItems)
                            {
                                object rawUserCountryBoxItemContent = userCountryBoxItem.Content;
                                string userCountryBoxItemContent = rawUserCountryBoxItemContent.ToString();
                                bool isUserCountry = userCountryBoxItemContent == currentUserCountry;
                                if (isUserCountry)
                                {
                                    userCountryBox.SelectedIndex = userCountryBox.Items.IndexOf(userCountryBoxItem);
                                }
                            }
                            userAboutBox.Text = currentUserAbout;

                            userProfileEditAvatar.Source = new BitmapImage(new Uri(@"http://localhost:4000/api/user/avatar/?id=" + currentUserId));

                        }
                    }
                }
            }
            catch (System.Net.WebException)
            {
                MessageBox.Show("Не удается подключиться к серверу", "Ошибка");
                this.Close();
            }

            Environment.SpecialFolder localApplicationDataFolder = Environment.SpecialFolder.LocalApplicationData;
            string localApplicationDataFolderPath = Environment.GetFolderPath(localApplicationDataFolder);
            string saveDataFilePath = localApplicationDataFolderPath + @"\OfficeWare\GameManager\" + currentUserId + @"\save-data.txt";
            js = new JavaScriptSerializer();
            string saveDataFileContent = File.ReadAllText(saveDataFilePath);
            SavedContent loadedContent = js.Deserialize<SavedContent>(saveDataFileContent);
            List<Game> currentGames = loadedContent.games;
            List<FriendSettings> currentFriends = loadedContent.friends;
            Settings currentSettings = loadedContent.settings;
            string themeName = currentSettings.profileTheme;
            foreach (StackPanel profileTheme in profileThemes.Children)
            {
                object rawProfileThemeName = profileTheme.DataContext;
                string profileThemeName = rawProfileThemeName.ToString();
                bool isThemeFound = profileThemeName == themeName;
                if (isThemeFound)
                {
                    SelectProfileTheme(themeName, profileTheme);
                    break;
                }
            }

            try
            {
                // HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/users/get/?id=" + currentUserId);
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/users/get/?id=" + currentUserId);
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (var reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        js = new JavaScriptSerializer();
                        var objText = reader.ReadToEnd();

                        UserResponseInfo myobj = (UserResponseInfo)js.Deserialize(objText, typeof(UserResponseInfo));

                        string status = myobj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
                            User user = myobj.user;
                            string friendsListSettings = user.friendsListSettings;
                            string gamesSettings = user.gamesSettings;
                            string equipmentSettings = user.equipmentSettings;
                            string commentsSettings = user.commentsSettings;
                            bool isPublic = friendsListSettings == "public";
                            bool isFriends = friendsListSettings == "friends";
                            bool isHidden = friendsListSettings == "hidden";
                            if (isPublic)
                            {
                                userFriendsSettingsSelector.SelectedIndex = 0;
                            }
                            else if (isFriends)
                            {
                                userFriendsSettingsSelector.SelectedIndex = 1;
                            }
                            else if (isHidden)
                            {
                                userFriendsSettingsSelector.SelectedIndex = 2;
                            }
                            isPublic = gamesSettings == "public";
                            isFriends = gamesSettings == "friends";
                            isHidden = gamesSettings == "hidden";
                            if (isPublic)
                            {
                                userGamesSettingsSelector.SelectedIndex = 0;
                            }
                            else if (isFriends)
                            {
                                userGamesSettingsSelector.SelectedIndex = 1;
                            }
                            else if (isHidden)
                            {
                                userGamesSettingsSelector.SelectedIndex = 2;
                            }
                            isPublic = equipmentSettings == "public";
                            isFriends = equipmentSettings == "friends";
                            isHidden = equipmentSettings == "hidden";
                            if (isPublic)
                            {
                                userEquipmentSettingsSelector.SelectedIndex = 0;
                            }
                            else if (isFriends)
                            {
                                userEquipmentSettingsSelector.SelectedIndex = 1;
                            }
                            else if (isHidden)
                            {
                                userEquipmentSettingsSelector.SelectedIndex = 2;
                            }
                            isPublic = commentsSettings == "public";
                            isFriends = commentsSettings == "friends";
                            isHidden = commentsSettings == "hidden";
                            if (isPublic)
                            {
                                userCommentsSettingsSelector.SelectedIndex = 0;
                            }
                            else if (isFriends)
                            {
                                userCommentsSettingsSelector.SelectedIndex = 1;
                            }
                            else if (isHidden)
                            {
                                userCommentsSettingsSelector.SelectedIndex = 2;
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

        public void GetGamesInfo()
        {

            Environment.SpecialFolder localApplicationDataFolder = Environment.SpecialFolder.LocalApplicationData;
            string localApplicationDataFolderPath = Environment.GetFolderPath(localApplicationDataFolder);
            string saveDataFilePath = localApplicationDataFolderPath + @"\OfficeWare\GameManager\" + currentUserId + @"\save-data.txt";
            string saveDataFileContent = File.ReadAllText(saveDataFilePath);
            JavaScriptSerializer js = new JavaScriptSerializer();
            SavedContent loadedContent = js.Deserialize<SavedContent>(saveDataFileContent);
            List<Game> myGames = loadedContent.games;
            gamesInfo.Children.Clear();
            foreach (Game myGame in myGames)
            {
                string myGameName = myGame.name;
                string myGameHours = myGame.hours;
                string myGameLastLaunchDate = myGame.date;
                DockPanel gameStats = new DockPanel();
                gameStats.Margin = new Thickness(0, 25, 0, 25);
                gameStats.Height = 150;
                gameStats.Background = System.Windows.Media.Brushes.DarkGray;
                Image gameStatsImg = new Image();
                gameStatsImg.Width = 125;
                gameStatsImg.Height = 125;
                gameStatsImg.Margin = new Thickness(10);

                gameStatsImg.Source = new BitmapImage(new Uri("https://cdn2.iconfinder.com/data/icons/ios-7-icons/50/user_male-128.png"));
                // gameStatsImg.Source = new BitmapImage(new Uri("http://localhost:4000/api/game/thumbnail/?name=" + myGameName));

                gameStats.Children.Add(gameStatsImg);
                TextBlock gameStatsNameLabel = new TextBlock();
                gameStatsNameLabel.Margin = new Thickness(10);
                gameStatsNameLabel.FontSize = 18;
                gameStatsNameLabel.Text = myGameName;
                gameStats.Children.Add(gameStatsNameLabel);
                StackPanel gameStatsInfo = new StackPanel();
                gameStatsInfo.Margin = new Thickness(10);
                gameStatsInfo.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;
                gameStatsInfo.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
                TextBlock gameStatsInfoHoursLabel = new TextBlock();
                gameStatsInfoHoursLabel.Margin = new Thickness(0, 5, 0, 5);
                string totalHoursLabelContent = Properties.Resources.totalHoursLabelContent;
                string gameStatsInfoHoursLabelContent = myGameHours + " " + totalHoursLabelContent;
                gameStatsInfoHoursLabel.Text = gameStatsInfoHoursLabelContent;
                gameStatsInfo.Children.Add(gameStatsInfoHoursLabel);
                TextBlock gameStatsInfoLastLaunchLabel = new TextBlock();
                gameStatsInfoLastLaunchLabel.Margin = new Thickness(0, 5, 0, 5);
                string lastLaunchLabelContent = Properties.Resources.lastLaunchLabelContent;
                string gameStatsInfoLastLaunchLabelContent = lastLaunchLabelContent + " " + myGameLastLaunchDate;
                gameStatsInfoLastLaunchLabel.Text = gameStatsInfoLastLaunchLabelContent;
                gameStatsInfo.Children.Add(gameStatsInfoLastLaunchLabel);
                gameStats.Children.Add(gameStatsInfo);
                gamesInfo.Children.Add(gameStats);
            }
        }

        public void GetUserInfo(string id, bool isLocalUser)
        {

            string gamesSettings = "public";
            string friendsSettings = "public";

            JavaScriptSerializer js = new JavaScriptSerializer();
            if (isLocalUser)
            {
                Environment.SpecialFolder localApplicationDataFolder = Environment.SpecialFolder.LocalApplicationData;
                string localApplicationDataFolderPath = Environment.GetFolderPath(localApplicationDataFolder);
                string saveDataFilePath = localApplicationDataFolderPath + @"\OfficeWare\GameManager\" + currentUserId + @"\save-data.txt";
                string saveDataFileContent = File.ReadAllText(saveDataFilePath);
                SavedContent loadedContent = js.Deserialize<SavedContent>(saveDataFileContent);
                List<Game> myGames = loadedContent.games;

                Settings mySettings = loadedContent.settings;
                string profileTheme = mySettings.profileTheme;
                bool isDefaultTheme = profileTheme == "Default";
                bool isSummerTheme = profileTheme == "Summer";
                bool isMidnightTheme = profileTheme == "Midnight";
                bool isSteelTheme = profileTheme == "Steel";
                bool isSpaceTheme = profileTheme == "Space";
                bool isDarkTheme = profileTheme == "Dark";
                if (isDefaultTheme)
                {
                    profileThemeAside.Color = System.Windows.Media.Brushes.Blue.Color;
                }
                else if (isSummerTheme)
                {
                    profileThemeAside.Color = System.Windows.Media.Brushes.SandyBrown.Color;
                }
                else if (isMidnightTheme)
                {
                    profileThemeAside.Color = System.Windows.Media.Brushes.LightGray.Color;
                }
                else if (isSteelTheme)
                {
                    profileThemeAside.Color = System.Windows.Media.Brushes.DarkGray.Color;
                }
                else if (isSpaceTheme)
                {
                    profileThemeAside.Color = System.Windows.Media.Brushes.Violet.Color;
                }
                else if (isDarkTheme)
                {
                    profileThemeAside.Color = System.Windows.Media.Brushes.Black.Color;
                }

                int countGames = myGames.Count;
                string rawCountGames = countGames.ToString();
                countGamesLabel.Text = rawCountGames;
            }
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/users/get/?id=" + id);
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (var reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        js = new JavaScriptSerializer();
                        var objText = reader.ReadToEnd();

                        UserResponseInfo myobj = (UserResponseInfo)js.Deserialize(objText, typeof(UserResponseInfo));

                        string status = myobj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
                            User user = myobj.user;
                            HttpWebRequest innerWebRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/friends/get");
                            innerWebRequest.Method = "GET";
                            innerWebRequest.UserAgent = ".NET Framework Test Client";
                            using (HttpWebResponse innerWebResponse = (HttpWebResponse)innerWebRequest.GetResponse())
                            {
                                using (var innerReader = new StreamReader(innerWebResponse.GetResponseStream()))
                                {
                                    js = new JavaScriptSerializer();
                                    objText = innerReader.ReadToEnd();

                                    FriendsResponseInfo myInnerObj = (FriendsResponseInfo)js.Deserialize(objText, typeof(FriendsResponseInfo));

                                    status = myInnerObj.status;
                                    isOkStatus = status == "OK";
                                    if (isOkStatus)
                                    {
                                        List<Friend> friendsIds = myInnerObj.friends.Where<Friend>((Friend joint) =>
                                        {
                                            string userId = joint.user;
                                            bool isMyFriend = userId == id;
                                            return isMyFriend;
                                        }).ToList<Friend>();
                                        int countFriends = friendsIds.Count;
                                        string rawCountFriends = countFriends.ToString();
                                        countFriendsLabel.Text = rawCountFriends;
                                        string currentUserName = user.name;
                                        userProfileNameLabel.Text = currentUserName;
                                        // string currentUserCountry = user.country;
                                        // userProfileAboutLabel.Text = currentUserCountry;
                                        string currentUserAbout = user.about;
                                        userProfileAboutLabel.Text = currentUserAbout;

                                        userProfileAvatar.BeginInit();
                                        Uri avatar = new Uri(@"http://localhost:4000/api/user/avatar/?id=" + id);
                                        userProfileAvatar.Source = new BitmapImage(avatar);
                                        userProfileAvatar.EndInit();


                                        gamesSettings = user.gamesSettings;
                                        friendsSettings = user.friendsListSettings;

                                        string onlineStatus = Properties.Resources.onlineLabelContent;
                                        string offlineStatus = Properties.Resources.offlineLabelContent;
                                        string userStatus = user.status;
                                        bool isOnline = userStatus == "online";
                                        bool isPlayed = userStatus == "played";
                                        bool isConnected = isOnline || isPlayed;
                                        if (isConnected)
                                        {
                                            userProfileStatusLabel.Text = onlineStatus;
                                            userProfileStatusLabel.Foreground = System.Windows.Media.Brushes.Cyan;
                                        }
                                        else
                                        {
                                            userProfileStatusLabel.Text = offlineStatus;
                                            userProfileStatusLabel.Foreground = System.Windows.Media.Brushes.LightGray;
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

            Visibility reverseVisibility = Visibility.Collapsed;
            bool isOtherUser = !isLocalUser;
            if (isOtherUser)
            {
                openChatBtn.DataContext = id;
                reverseVisibility = Visibility.Visible;
            }
            else
            {
                reverseVisibility = Visibility.Collapsed;
            }
            otherUserBtns.Visibility = reverseVisibility;

            Visibility visibility = Visibility.Collapsed;
            if (isLocalUser)
            {
                visibility = Visibility.Visible;
            }
            else
            {
                visibility = Visibility.Collapsed;
            }
            editProfileBtn.Visibility = visibility;

            Visibility gamesVisibility = Visibility.Collapsed;
            bool isHiddenAccess = gamesSettings == "hidden";
            bool isNotHiddenAccess = !isHiddenAccess;
            bool isShowGames = isLocalUser || isNotHiddenAccess;
            if (isShowGames)
            {
                gamesVisibility = Visibility.Visible;
            }
            else
            {
                gamesVisibility = Visibility.Collapsed;
            }
            userProfileDetailsGames.Visibility = gamesVisibility;
            Visibility friendsVisibility = Visibility.Collapsed;
            isHiddenAccess = friendsSettings == "hidden";
            isNotHiddenAccess = !isHiddenAccess;
            bool isShowFriends = isLocalUser || isNotHiddenAccess;
            if (isShowGames)
            {
                friendsVisibility = Visibility.Visible;
            }
            else
            {
                friendsVisibility = Visibility.Collapsed;
            }
            userProfileDetailsFriends.Visibility = friendsVisibility;

            object currentUserProfileId = mainControl.DataContext;
            cachedUserProfileId = ((string)(currentUserProfileId));

            mainControl.DataContext = currentUserId;

        }

        public void GetFriendRequests()
        {
            foreach (Popup friendRequest in friendRequests.Children)
            {
                friendRequest.IsOpen = false;
            }
            friendRequests.Children.Clear();
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/friends/requests/get/?id=" + currentUserId);
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (var reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        var objText = reader.ReadToEnd();
                        FriendRequestsResponseInfo myobj = (FriendRequestsResponseInfo)js.Deserialize(objText, typeof(FriendRequestsResponseInfo));
                        string status = myobj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
                            List<FriendRequest> myRequests = new List<FriendRequest>();
                            List<FriendRequest> requests = myobj.requests;
                            foreach (FriendRequest request in requests)
                            {
                                string recepientId = request.friend;
                                bool isRequestForMe = currentUserId == recepientId;
                                if (isRequestForMe)
                                {
                                    myRequests.Add(request);
                                }
                            }
                            foreach (FriendRequest myRequest in myRequests)
                            {
                                string senderId = myRequest.user;
                                HttpWebRequest innerWebRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/users/get/?id=" + senderId);
                                innerWebRequest.Method = "GET";
                                innerWebRequest.UserAgent = ".NET Framework Test Client";
                                using (HttpWebResponse innerWebResponse = (HttpWebResponse)innerWebRequest.GetResponse())
                                {
                                    using (var innerReader = new StreamReader(innerWebResponse.GetResponseStream()))
                                    {
                                        js = new JavaScriptSerializer();
                                        objText = innerReader.ReadToEnd();
                                        UserResponseInfo myInnerObj = (UserResponseInfo)js.Deserialize(objText, typeof(UserResponseInfo));
                                        status = myInnerObj.status;
                                        isOkStatus = status == "OK";
                                        if (isOkStatus)
                                        {
                                            User user = myInnerObj.user;
                                            string senderLogin = user.login;
                                            Popup friendRequest = new Popup();
                                            friendRequest.Placement = PlacementMode.Custom;
                                            friendRequest.CustomPopupPlacementCallback = new CustomPopupPlacementCallback(FriendRequestPlacementHandler);
                                            friendRequest.PlacementTarget = this;
                                            friendRequest.Width = 225;
                                            friendRequest.Height = 275;
                                            StackPanel friendRequestBody = new StackPanel();
                                            friendRequestBody.Background = friendRequestBackground;
                                            PackIcon closeRequestBtn = new PackIcon();
                                            closeRequestBtn.Margin = new Thickness(10);
                                            closeRequestBtn.Kind = PackIconKind.Close;
                                            closeRequestBtn.DataContext = friendRequest;
                                            closeRequestBtn.MouseLeftButtonUp += CloseFriendRequestHandler;
                                            friendRequestBody.Children.Add(closeRequestBtn);
                                            Image friendRequestBodySenderAvatar = new Image();
                                            friendRequestBodySenderAvatar.Width = 100;
                                            friendRequestBodySenderAvatar.Height = 100;
                                            friendRequestBodySenderAvatar.BeginInit();
                                            Uri friendRequestBodySenderAvatarUri = new Uri("http://localhost:4000/api/user/avatar/?id=" + senderId);
                                            BitmapImage friendRequestBodySenderAvatarImg = new BitmapImage(friendRequestBodySenderAvatarUri);
                                            friendRequestBodySenderAvatar.Source = friendRequestBodySenderAvatarImg;
                                            friendRequestBodySenderAvatar.EndInit();
                                            friendRequestBodySenderAvatar.ImageFailed += SetDefautAvatarHandler;
                                            friendRequestBody.Children.Add(friendRequestBodySenderAvatar);
                                            TextBlock friendRequestBodySenderLoginLabel = new TextBlock();
                                            friendRequestBodySenderLoginLabel.Margin = new Thickness(10);
                                            friendRequestBodySenderLoginLabel.Text = senderLogin;
                                            friendRequestBody.Children.Add(friendRequestBodySenderLoginLabel);
                                            StackPanel friendRequestBodyActions = new StackPanel();
                                            friendRequestBodyActions.Orientation = Orientation.Horizontal;
                                            Button acceptActionBtn = new Button();
                                            acceptActionBtn.Margin = new Thickness(10, 5, 10, 5);
                                            acceptActionBtn.Height = 25;
                                            acceptActionBtn.Width = 65;
                                            acceptActionBtn.Content = "Принять";
                                            string myNewFriendId = myRequest.user;
                                            string myRequestId = myRequest._id;
                                            Dictionary<String, Object> acceptActionBtnData = new Dictionary<String, Object>();
                                            acceptActionBtnData.Add("friendId", ((string)(myNewFriendId)));
                                            acceptActionBtnData.Add("requestId", ((string)(myRequestId)));
                                            acceptActionBtnData.Add("request", ((Popup)(friendRequest)));
                                            acceptActionBtn.DataContext = acceptActionBtnData;
                                            acceptActionBtn.Click += AcceptFriendRequestHandler;
                                            friendRequestBodyActions.Children.Add(acceptActionBtn);
                                            Button rejectActionBtn = new Button();
                                            rejectActionBtn.Margin = new Thickness(10, 5, 10, 5);
                                            rejectActionBtn.Height = 25;
                                            rejectActionBtn.Width = 65;
                                            rejectActionBtn.Content = "Отклонить";
                                            Dictionary<String, Object> rejectActionBtnData = new Dictionary<String, Object>();
                                            rejectActionBtnData.Add("friendId", ((string)(myNewFriendId)));
                                            rejectActionBtnData.Add("requestId", ((string)(myRequestId)));
                                            rejectActionBtnData.Add("request", ((Popup)(friendRequest)));
                                            rejectActionBtn.DataContext = rejectActionBtnData;
                                            rejectActionBtn.Click += RejectFriendRequestHandler;
                                            friendRequestBodyActions.Children.Add(rejectActionBtn);
                                            friendRequestBody.Children.Add(friendRequestBodyActions);
                                            friendRequest.Child = friendRequestBody;
                                            friendRequests.Children.Add(friendRequest);
                                            friendRequest.IsOpen = true;
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            CloseManager();
                        }

                    }
                }
            }
            catch (System.Net.WebException exception)
            {
                Debugger.Log(0, "debug", "friend requests: " + exception.Message);
                MessageBox.Show("Не удается подключиться к серверу", "Ошибка");
                this.Close();
            }
        }

        public void GetUser(string userId)
        {
            currentUserId = userId;
            System.Diagnostics.Debugger.Log(0, "debug", "userId: " + userId + Environment.NewLine);
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/users/get/?id=" + userId);
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (var reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        var objText = reader.ReadToEnd();

                        UserResponseInfo myobj = (UserResponseInfo)js.Deserialize(objText, typeof(UserResponseInfo));

                        string status = myobj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
                            currentUser = myobj.user;
                            bool isUserExists = currentUser != null;
                            if (isUserExists)
                            {
                                string userName = currentUser.name;
                                ItemCollection userMenuItems = userMenu.Items;
                                ComboBoxItem userLoginLabel = ((ComboBoxItem)(userMenuItems[0]));
                                userLoginLabel.Content = userName;
                                ItemCollection profileMenuItems = profileMenu.Items;
                                object rawMainProfileMenuItem = profileMenuItems[0];
                                ComboBoxItem mainProfileMenuItem = ((ComboBoxItem)(rawMainProfileMenuItem));
                                mainProfileMenuItem.Content = userName;
                                InitCache(userId);
                            }
                            else
                            {
                                CloseManager();
                            }
                        }
                        else
                        {
                            CloseManager();
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

        public void CloseManager()
        {
            MessageBox.Show("Не удалось подключиться", "Ошибка");
            this.Close();
        }

        public void InitConstants()
        {
            visible = Visibility.Visible;
            invisible = Visibility.Collapsed;
            friendRequestBackground = System.Windows.Media.Brushes.AliceBlue;
            disabledColor = System.Windows.Media.Brushes.LightGray;
            enabledColor = System.Windows.Media.Brushes.Black;
            history = new List<int>();
        }

        public void LoadStartWindow()
        {
            Environment.SpecialFolder localApplicationDataFolder = Environment.SpecialFolder.LocalApplicationData;
            string localApplicationDataFolderPath = Environment.GetFolderPath(localApplicationDataFolder);
            string saveDataFilePath = localApplicationDataFolderPath + @"\OfficeWare\GameManager\" + currentUserId + @"\save-data.txt";
            JavaScriptSerializer js = new JavaScriptSerializer();
            string saveDataFileContent = File.ReadAllText(saveDataFilePath);
            SavedContent loadedContent = js.Deserialize<SavedContent>(saveDataFileContent);
            Settings currentSettings = loadedContent.settings;
            int currentStartWindow = currentSettings.startWindow;

            mainControl.SelectedIndex = currentStartWindow;
            AddHistoryRecord();
            arrowBackBtn.Foreground = disabledColor;
            arrowForwardBtn.Foreground = disabledColor;

        }

        public void GetGamesList(string keywords)
        {
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/games/get");
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (var reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        var objText = reader.ReadToEnd();
                        GamesListResponseInfo myobj = (GamesListResponseInfo)js.Deserialize(objText, typeof(GamesListResponseInfo));
                        string responseStatus = myobj.status;
                        bool isOKStatus = responseStatus == "OK";
                        if (isOKStatus)
                        {

                            Environment.SpecialFolder localApplicationDataFolder = Environment.SpecialFolder.LocalApplicationData;
                            string localApplicationDataFolderPath = Environment.GetFolderPath(localApplicationDataFolder);
                            string saveDataFilePath = localApplicationDataFolderPath + @"\OfficeWare\GameManager\" + currentUserId + @"\save-data.txt";
                            js = new JavaScriptSerializer();
                            string saveDataFileContent = File.ReadAllText(saveDataFilePath);
                            SavedContent loadedContent = js.Deserialize<SavedContent>(saveDataFileContent);
                            List<string> currentCollections = loadedContent.collections;
                            List<Game> currentGames = loadedContent.games;

                            games.Children.Clear();
                            List<GameResponseInfo> loadedGames = myobj.games;
                            loadedGames = loadedGames.Where<GameResponseInfo>((GameResponseInfo gameInfo) =>
                            {
                                int keywordsLength = keywords.Length;
                                bool isKeywordsSetted = keywordsLength >= 1;
                                bool isKeywordsNotSetted = !isKeywordsSetted;
                                string gameName = gameInfo.name;
                                string ignoreCaseKeywords = keywords.ToLower();
                                string ignoreCaseGameName = gameName.ToLower();
                                bool isGameNameMatch = ignoreCaseGameName.Contains(ignoreCaseKeywords);
                                bool isSearchEnabled = isKeywordsSetted && isGameNameMatch;
                                bool isGameMatch = isSearchEnabled || isKeywordsNotSetted;
                                return isGameMatch;
                            }).ToList<GameResponseInfo>();
                            int countLoadedGames = loadedGames.Count;

                            // bool isGamesExists = countLoadedGames >= 1;
                            bool isGamesExists = myobj.games.Count >= 1;
                            if (isGamesExists)
                            {
                                activeGame.Visibility = visible;
                                // foreach (GameResponseInfo gamesListItem in loadedGames)
                                foreach (GameResponseInfo gamesListItem in myobj.games)
                                {

                                    string gamesListItemName = gamesListItem.name;

                                    List<Game> results = currentGames.Where<Game>((Game game) =>
                                    {
                                        return game.name == gamesListItemName;
                                    }).ToList<Game>();
                                    int countResults = results.Count;
                                    bool isHaveResults = countResults >= 1;
                                    bool isShowGame = true;
                                    Game result = null;
                                    if (isHaveResults)
                                    {
                                        result = results[0];
                                        bool isHidden = result.isHidden;
                                        isShowGame = !isHidden;
                                    }
                                    if (isShowGame)
                                    {
                                        StackPanel newGame = new StackPanel();
                                        newGame.MouseLeftButtonUp += SelectGameHandler;
                                        newGame.Orientation = Orientation.Horizontal;
                                        newGame.Height = 35;
                                        string gamesListItemId = gamesListItem._id;
                                        Debugger.Log(0, "debug", Environment.NewLine + "gamesListItemId: " + gamesListItemId + Environment.NewLine);
                                        // string gamesListItemUrl = gamesListItem.url;
                                        string gamesListItemUrl = @"http://localhost:4000/api/game/distributive/?name=" + gamesListItemName;
                                        // string gamesListItemImage = gamesListItem.image;
                                        string gamesListItemImage = @"http://localhost:4000/api/game/thumbnail/?name=" + gamesListItemName;
                                        int gamesListItemPrice = gamesListItem.price;
                                        Dictionary<String, Object> newGameData = new Dictionary<String, Object>();
                                        newGameData.Add("id", gamesListItemId);
                                        newGameData.Add("name", gamesListItemName);
                                        newGameData.Add("url", gamesListItemUrl);
                                        newGameData.Add("image", gamesListItemImage);
                                        newGameData.Add("price", gamesListItemPrice);
                                        newGame.DataContext = newGameData;
                                        Image newGamePhoto = new Image();
                                        newGamePhoto.Margin = new Thickness(5);
                                        newGamePhoto.Width = 25;
                                        newGamePhoto.Height = 25;
                                        newGamePhoto.BeginInit();
                                        // Uri newGamePhotoUri = new Uri(gamesListItemImage);
                                        // Uri newGamePhotoUri = new Uri("https://cdn2.iconfinder.com/data/icons/ios-7-icons/50/user_male-128.png");
                                        Uri newGamePhotoUri = new Uri(gamesListItemImage);
                                        newGamePhoto.Source = new BitmapImage(newGamePhotoUri);
                                        newGamePhoto.EndInit();
                                        newGame.Children.Add(newGamePhoto);
                                        newGamePhoto.ImageFailed += SetDefaultThumbnailHandler;
                                        TextBlock newGameLabel = new TextBlock();
                                        newGameLabel.Margin = new Thickness(5);
                                        newGameLabel.Text = gamesListItem.name;
                                        newGame.Children.Add(newGameLabel);
                                        games.Children.Add(newGame);

                                        List<Game> gameSearchResults = currentGames.Where<Game>((Game game) =>
                                        {
                                            string gameName = game.name;
                                            bool isGameFound = gameName == gamesListItemName;
                                            return isGameFound;
                                        }).ToList<Game>();
                                        int countGameSearchResults = gameSearchResults.Count;
                                        bool isGameInstalled = countGameSearchResults >= 1;
                                        bool isGameNotInstalled = !isGameInstalled;

                                        if (isGameInstalled)
                                        {
                                            ContextMenu newGameContextMenu = new ContextMenu();

                                            MenuItem newGameContextMenuItem = new MenuItem();
                                            newGameContextMenuItem.Header = "Играть";
                                            newGameContextMenuItem.DataContext = gamesListItemName;
                                            newGameContextMenuItem.Click += RunGameHandler;
                                            newGameContextMenu.Items.Add(newGameContextMenuItem);

                                            newGameContextMenuItem = new MenuItem();
                                            newGameContextMenuItem.Header = "Добавить в коллекцию";

                                            newGameContextMenu.Items.Add(newGameContextMenuItem);
                                            foreach (string collectionName in currentCollections)
                                            {
                                                MenuItem newGameInnerContextMenuItem = new MenuItem();
                                                List<string> resultCollections = new List<string>();
                                                if (isHaveResults)
                                                {
                                                    resultCollections = result.collections;
                                                }
                                                bool isAlreadyInCollection = resultCollections.Contains(collectionName);
                                                if (isAlreadyInCollection)
                                                {
                                                    newGameInnerContextMenuItem.IsEnabled = false;
                                                }

                                                newGameInnerContextMenuItem.Header = collectionName;
                                                Dictionary<String, Object> newGameInnerContextMenuItemData = new Dictionary<String, Object>();
                                                newGameInnerContextMenuItemData.Add("collection", collectionName);
                                                newGameInnerContextMenuItemData.Add("name", gamesListItemName);
                                                newGameInnerContextMenuItem.DataContext = newGameInnerContextMenuItemData;
                                                newGameInnerContextMenuItem.Click += AddGameToCollectionHandler;
                                                newGameContextMenuItem.Items.Add(newGameInnerContextMenuItem);
                                            }

                                            /*MenuItem gameCollectionItemNestedContextMenuItem = new MenuItem();
                                            gameCollectionItemNestedContextMenuItem.Header = "Удалить с утройства";
                                            Dictionary<String, Object> gameCollectionItemNestedContextMenuItemData = new Dictionary<String, Object>();
                                            gameCollectionItemNestedContextMenuItemData.Add("game", gamesListItemName);
                                            gameCollectionItemNestedContextMenuItemData.Add("collection", "");
                                            gameCollectionItemNestedContextMenuItem.DataContext = gameCollectionItemNestedContextMenuItemData;
                                            gameCollectionItemNestedContextMenuItem.Click += RemoveGameFromCollectionsMenuHandler;
                                            newGameContextMenuItem.Items.Add(gameCollectionItemNestedContextMenuItem);*/

                                            newGameContextMenuItem = new MenuItem();
                                            string gameCollectionItemContextMenuItemHeaderContent = "Убрать из ";
                                            newGameContextMenuItem.Header = gameCollectionItemContextMenuItemHeaderContent;
                                            MenuItem newGameNestedContextMenuItem;
                                            Dictionary<String, Object> newGameNestedContextMenuItemData;
                                            foreach (string hiddenGameCollection in result.collections)
                                            {
                                                newGameNestedContextMenuItem = new MenuItem();
                                                newGameNestedContextMenuItem.Header = hiddenGameCollection;
                                                newGameNestedContextMenuItemData = new Dictionary<String, Object>();
                                                newGameNestedContextMenuItemData.Add("game", gamesListItemName);
                                                newGameNestedContextMenuItemData.Add("collection", hiddenGameCollection);
                                                newGameNestedContextMenuItem.DataContext = newGameNestedContextMenuItemData;
                                                newGameNestedContextMenuItem.Click += RemoveGameFromCollectionHandler;
                                                newGameContextMenuItem.Items.Add(newGameNestedContextMenuItem);
                                            }
                                            newGameContextMenu.Items.Add(newGameContextMenuItem);

                                            newGameContextMenuItem = new MenuItem();
                                            newGameContextMenuItem.Header = "Управление";

                                            newGameNestedContextMenuItem = new MenuItem();
                                            newGameNestedContextMenuItem.Header = "Создать ярлык на рабочем столе";
                                            newGameNestedContextMenuItem.DataContext = gamesListItemName;
                                            newGameNestedContextMenuItem.Click += CreateShortcutHandler;
                                            newGameContextMenuItem.Items.Add(newGameNestedContextMenuItem);

                                            newGameNestedContextMenuItem = new MenuItem();
                                            bool IsCoverSet = result.cover != "";
                                            if (IsCoverSet)
                                            {
                                                newGameNestedContextMenuItem.Header = "Удалить свою обложку";
                                            }
                                            else
                                            {
                                                newGameNestedContextMenuItem.Header = "Задать свою обложку";
                                            }
                                            newGameNestedContextMenuItemData = new Dictionary<String, Object>();
                                            newGameNestedContextMenuItemData.Add("game", gamesListItemName);
                                            newGameNestedContextMenuItemData.Add("collection", "");
                                            newGameNestedContextMenuItemData.Add("cover", result.cover);
                                            newGameNestedContextMenuItem.DataContext = newGameNestedContextMenuItemData;
                                            newGameNestedContextMenuItem.Click += ToggleGameCoverHandler;
                                            newGameContextMenuItem.Items.Add(newGameNestedContextMenuItem);

                                            newGameNestedContextMenuItem = new MenuItem();
                                            newGameNestedContextMenuItem.Header = "Просмотреть локальные файлы";
                                            newGameNestedContextMenuItem.DataContext = gamesListItemName;
                                            newGameNestedContextMenuItem.Click += ShowGamesLocalFilesHandler;
                                            newGameContextMenuItem.Items.Add(newGameNestedContextMenuItem);

                                            newGameNestedContextMenuItem = new MenuItem();
                                            bool isHiddenGame = result.isHidden;
                                            if (isHiddenGame)
                                            {
                                                newGameNestedContextMenuItem.Header = "Убрать из скрытого";
                                            }
                                            else
                                            {
                                                newGameNestedContextMenuItem.Header = "Скрыть игру";
                                            }

                                            newGameNestedContextMenuItemData = new Dictionary<String, Object>();
                                            newGameNestedContextMenuItemData.Add("game", gamesListItemName);
                                            newGameNestedContextMenuItemData.Add("collection", "");
                                            newGameNestedContextMenuItem.DataContext = newGameNestedContextMenuItemData;
                                            newGameNestedContextMenuItem.Click += ToggleGameVisibilityHandler;
                                            newGameContextMenuItem.Items.Add(newGameNestedContextMenuItem);

                                            newGameNestedContextMenuItem = new MenuItem();
                                            newGameNestedContextMenuItem.Header = "Удалить с утройства";
                                            newGameNestedContextMenuItemData = new Dictionary<String, Object>();
                                            newGameNestedContextMenuItemData.Add("game", gamesListItemName);
                                            newGameNestedContextMenuItemData.Add("collection", "");
                                            newGameNestedContextMenuItem.DataContext = newGameNestedContextMenuItemData;
                                            newGameNestedContextMenuItem.Click += RemoveGameFromCollectionsMenuHandler;
                                            newGameContextMenuItem.Items.Add(newGameNestedContextMenuItem);
                                            newGameContextMenu.Items.Add(newGameContextMenuItem);

                                            string currentGameId = result.id;
                                            newGameContextMenuItem = new MenuItem();
                                            newGameContextMenuItem.Header = "Свойства";
                                            Dictionary<String, Object> gameCollectionItemContextMenuItemData = new Dictionary<String, Object>();
                                            gameCollectionItemContextMenuItemData.Add("game", gamesListItemName);
                                            bool isCustomGame = currentGameId == "mockId";
                                            gameCollectionItemContextMenuItemData.Add("isCustomGame", isCustomGame);
                                            newGameContextMenuItem.DataContext = gameCollectionItemContextMenuItemData;
                                            newGameContextMenuItem.Click += OpenGameSettingsHandler;
                                            newGameContextMenu.Items.Add(newGameContextMenuItem);

                                            newGame.ContextMenu = newGameContextMenu;
                                        }
                                    }
                                }
                                List<Game> displayedGames = currentGames.Where<Game>((Game game) =>
                                {
                                    bool isHidden = game.isHidden;
                                    bool isDisplayed = !isHidden;
                                    return isDisplayed;
                                }).ToList<Game>();
                                int countdisplayedGames = displayedGames.Count;
                                bool isHaveDisplayedGames = countdisplayedGames >= 1;
                                if (isHaveDisplayedGames)
                                {

                                    Game displayedGame = displayedGames[0];
                                    string displayedGameName = displayedGame.name;
                                    int index = loadedGames.FindIndex((GameResponseInfo game) =>
                                    {
                                        string gameName = game.name;
                                        bool isGameFound = gameName == displayedGameName;
                                        return isGameFound;
                                    });
                                    bool isFound = index >= 0;
                                    if (isFound)
                                    {
                                        GameResponseInfo crossGame = loadedGames[index];

                                        // GameResponseInfo firstGame = loadedGames[0];
                                        // GameResponseInfo firstGame = myobj.games[0];
                                        GameResponseInfo firstGame = crossGame;
                                        Dictionary<String, Object> firstGameData = new Dictionary<String, Object>();
                                        string firstGameId = firstGame._id;
                                        string firstGameName = firstGame.name;
                                        /*string firstGameUrl = firstGame.url;
                                        string firstGameImage = firstGame.image;*/
                                        string firstGameUrl = @"http://localhost:4000/api/game/distributive/?name=" + firstGameName;
                                        string firstGameImage = @"http://localhost:4000/api/game/thumbnail/?name=" + firstGameName;
                                        int firstGamePrice = firstGame.price;
                                        Debugger.Log(0, "debug", Environment.NewLine + "firstGameId: " + firstGameId + Environment.NewLine);
                                        firstGameData.Add("id", firstGameId);
                                        firstGameData.Add("name", firstGameName);
                                        firstGameData.Add("url", firstGameUrl);
                                        firstGameData.Add("image", firstGameImage);
                                        firstGameData.Add("price", firstGamePrice);
                                        SelectGame(firstGameData);
                                    }
                                    else
                                    {
                                        activeGame.Visibility = invisible;
                                    }
                                }
                                else
                                {
                                    activeGame.Visibility = invisible;
                                }
                            }
                            else
                            {
                                activeGame.Visibility = invisible;
                            }
                        }
                    }
                }
            }
            catch
            {

            }
            GetLocalGames(keywords);

        }

        public void DragGameToCollectionHandler(object sender, MouseEventArgs e)
        {
            StackPanel someGame = ((StackPanel)(sender));
            bool isLeftMouseBtnPressed = e.LeftButton == MouseButtonState.Pressed;
            if (isLeftMouseBtnPressed)
            {

            }
        }

        public void AddGameToCollectionHandler(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = ((MenuItem)(sender));
            object rawMenuItemData = menuItem.DataContext;
            Dictionary<String, Object> menuItemData = ((Dictionary<String, Object>)(rawMenuItemData));
            AddGameToCollection(menuItemData);
        }

        public void AddGameToCollection(Dictionary<String, Object> data)
        {
            string collection = ((string)(data["collection"]));
            string name = ((string)(data["name"]));
            Debugger.Log(0, "debug", Environment.NewLine + "collection: " + collection + ", name: " + name + Environment.NewLine);
            Environment.SpecialFolder localApplicationDataFolder = Environment.SpecialFolder.LocalApplicationData;
            string localApplicationDataFolderPath = Environment.GetFolderPath(localApplicationDataFolder);
            string saveDataFilePath = localApplicationDataFolderPath + @"\OfficeWare\GameManager\" + currentUserId + @"\save-data.txt";
            JavaScriptSerializer js = new JavaScriptSerializer();
            string saveDataFileContent = File.ReadAllText(saveDataFilePath);
            SavedContent loadedContent = js.Deserialize<SavedContent>(saveDataFileContent);
            List<Game> updatedGames = loadedContent.games;
            List<FriendSettings> currentFriends = loadedContent.friends;
            Settings currentSettings = loadedContent.settings;
            List<string> currentCollections = loadedContent.collections;
            Notifications currentNotifications = loadedContent.notifications;
            List<Game> results = updatedGames.Where<Game>((Game game) =>
            {
                return game.name == name;
            }).ToList<Game>();
            int resultsCount = results.Count;
            bool isFound = resultsCount >= 1;
            if (isFound)
            {
                Game result = results[0];
                result.collections.Add(collection);
                saveDataFileContent = js.Serialize(new SavedContent()
                {
                    games = updatedGames,
                    friends = currentFriends,
                    settings = currentSettings,
                    collections = currentCollections,
                    notifications = currentNotifications
                });
                File.WriteAllText(saveDataFilePath, saveDataFileContent);
                GetGameCollections();
                GetGamesList("");
                GetGameCollectionItems(collection);
                GetHiddenGames();
            }
        }


        public void InitCache(string id)
        {
            Environment.SpecialFolder localApplicationDataFolder = Environment.SpecialFolder.LocalApplicationData;
            string localApplicationDataFolderPath = Environment.GetFolderPath(localApplicationDataFolder);
            string userFolder = "";
            int idLength = id.Length;
            bool isIdExists = idLength >= 1;
            if (isIdExists)
            {
                userFolder = id + @"\";
            }
            string saveDataFilePath = localApplicationDataFolderPath + @"\OfficeWare\GameManager\" + userFolder + "save-data.txt";
            string cachePath = localApplicationDataFolderPath + @"\OfficeWare\GameManager\" + id;

            string cacheGamesPath = localApplicationDataFolderPath + @"\OfficeWare\GameManager\" + id + @"\games";
            string cacheScreenShotsPath = localApplicationDataFolderPath + @"\OfficeWare\GameManager\" + id + @"\screenshots";

            bool isCacheFolderExists = Directory.Exists(cachePath);
            bool isCacheFolderNotExists = !isCacheFolderExists;
            if (isCacheFolderNotExists)
            {
                Directory.CreateDirectory(cachePath);
                using (Stream s = File.Open(saveDataFilePath, FileMode.OpenOrCreate))
                {
                    using (StreamWriter sw = new StreamWriter(s))
                    {
                        sw.Write("");
                    }
                };
                JavaScriptSerializer js = new JavaScriptSerializer();
                string savedContent = js.Serialize(new SavedContent
                {
                    games = new List<Game>(),
                    friends = new List<FriendSettings>(),
                    settings = new Settings()
                    {
                        language = "ru-RU",
                        startWindow = 0,
                        overlayHotKey = "Shift + Tab",
                        music = new MusicSettings()
                        {
                            paths = new List<string>(),
                            volume = 100
                        },
                        profileTheme = "Default",
                        screenShotsHotKey = "Shift + S",
                        frames = "Disabled",
                        showScreenShotsNotification = true,
                        playScreenShotsNotification = true,
                        saveScreenShotsCopy = false,
                        showOverlay = true,
                        familyView = false,
                        familyViewCode = "",
                        familyViewGames = new List<string>()
                    },
                    collections = new List<string>() { },
                    notifications = new Notifications() {
                        isNotificationsEnabled = true,
                        notificationsProductFromWantListWithDiscount = true,
                        notificationsProductFromWantListUpdateAcccess = true,
                        notificationsProductFromSubsOrFavoritesUpdateAcccess = true,
                        notificationsProductFromDeveloperUpdateAcccess = true,
                        notificationsStartYearlyDiscount = true,
                        notificationsGroupUpdateGameReview = true,
                        notificationsUpdateIcon = true,
                        notificationsUpdateGames = true
                    }
                });
                File.WriteAllText(saveDataFilePath, savedContent);

                Directory.CreateDirectory(cacheGamesPath);
                Directory.CreateDirectory(cacheScreenShotsPath);

            }
        }

        public void ShowOffers()
        {
            Dialogs.OffersDialog dialog = new Dialogs.OffersDialog();
            dialog.Show();
        }

        public void OpenSettingsHandler(object sender, RoutedEventArgs e)
        {
            OpenSettings();
        }

        public void OpenSettings()
        {
            Dialogs.SettingsDialog dialog = new Dialogs.SettingsDialog(currentUserId);
            dialog.Closed += DetectSettingsEventHandler;
            dialog.Show();
        }

        public void DetectSettingsEventHandler (object sender, EventArgs e)
        {
            Dialogs.SettingsDialog dialog = ((Dialogs.SettingsDialog)(sender));
            object dialogData = dialog.DataContext;
            DetectSettingsEvent(dialogData);
        }

        public void DetectSettingsEvent (object data)
        {
            bool isDataExists = data != null;
            if (isDataExists)
            {
                string settingsEvent = ((string)(data));
                bool isUpdateEmail = settingsEvent == "email update";
                bool isUpdatePassword = settingsEvent == "password update";
                bool isUpdateFamilyView = settingsEvent == "family view update";
                if (isUpdateEmail)
                {
                    OpenUpdateEmail();
                }
                else if (isUpdatePassword)
                {
                    OpenUpdatePassword();
                }
                else if (isUpdateFamilyView)
                {
                    OpenFamilyViewManagement();
                }
            }
        }

        public void OpenFamilyViewManagement()
        {
            mainControl.SelectedIndex = 44;
        }

        async public void RunGame(string gameName, string joinedGameName = "")
        {

            StartDetectGameHours();
            GameWindow window = new GameWindow(currentUserId);

            // window.DataContext = gameActionLabel.DataContext;
            Environment.SpecialFolder localApplicationDataFolder = Environment.SpecialFolder.LocalApplicationData;
            string localApplicationDataFolderPath = Environment.GetFolderPath(localApplicationDataFolder);
            string userPath = localApplicationDataFolderPath + @"\OfficeWare\GameManager\" + currentUserId;
            string gamePath = userPath + @"\games\" + gameName + @"\game.exe";
            window.DataContext = gamePath;

            window.Closed += ComputeGameHoursHandler;
            window.Show();
            // string gameName = gameNameLabel.Text;
            try
            {
                // await client.EmitAsync("user_is_played", currentUserId + "|" + gameName);
            }
            catch (System.Net.WebSockets.WebSocketException)
            {
                Debugger.Log(0, "debug", "Ошибка сокетов");
                await client.ConnectAsync();
            }

            string gameId = "1";
            string saveDataFilePath = userPath + @"\save-data.txt";
            string currentGameName = joinedGameName;
            int joinedGameNameLength = joinedGameName.Length;
            bool isNotJoinedGame = joinedGameNameLength <= 0;
            if (isNotJoinedGame)
            {
                currentGameName = gameName;
            }
            string appPath = localApplicationDataFolderPath + @"\OfficeWare\GameManager\" + currentUserId + @"\";
            // string cachePath = appPath + currentGameName;
            string cachePath = appPath + @"games\" + currentGameName;
            string filename = cachePath + @"\game.exe";
            JavaScriptSerializer js = new JavaScriptSerializer();
            string saveDataFileContent = File.ReadAllText(saveDataFilePath);
            SavedContent loadedContent = js.Deserialize<SavedContent>(saveDataFileContent);
            List<Game> updatedGames = loadedContent.games;
            object gameNameLabelData = gameNameLabel.DataContext;
            string gameUploadedPath = ((string)(gameNameLabelData));
            DateTime currentDate = DateTime.Now;
            string gameLastLaunchDate = currentDate.ToLongDateString();
            string rawTimerHours = timerHours.ToString();
            int gameIndex = -1;
            foreach (Game someGame in updatedGames)
            {
                string someGameName = someGame.name;
                bool isNamesMatch = someGameName == currentGameName;
                if (isNamesMatch)
                {
                    gameIndex = updatedGames.IndexOf(someGame);
                    break;
                }
            }
            bool isGameFound = gameIndex >= 0;
            if (isGameFound)
            {
                Game currentGame = updatedGames[gameIndex];
                gameId = currentGame.id;
            }
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/games/stats/increase/?id=" + gameId);
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (var reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        js = new JavaScriptSerializer();
                        var objText = reader.ReadToEnd();

                        UserResponseInfo myobj = (UserResponseInfo)js.Deserialize(objText, typeof(UserResponseInfo));

                        string status = myobj.status;
                        bool isOkStatus = status == "OK";
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

            // SetUserStatus("played");
            UpdateUserStatus("played");

            client.EmitAsync("user_is_toggle_status", "played");
        }

        public void StartDetectGameHours()
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromHours(1);
            timer.Tick += GameHoursUpdateHandler;
            timer.Start();
            timerHours = 0;
        }

        public void GameHoursUpdateHandler(object sender, EventArgs e)
        {
            timerHours++;
        }

        public void ComputeGameHoursHandler(object sender, EventArgs e)
        {
            ComputeGameHours();
        }

        public void ComputeGameHours()
        {
            timer.Stop();
            Environment.SpecialFolder localApplicationDataFolder = Environment.SpecialFolder.LocalApplicationData;
            string localApplicationDataFolderPath = Environment.GetFolderPath(localApplicationDataFolder);
            string saveDataFilePath = localApplicationDataFolderPath + @"\OfficeWare\GameManager\" + currentUserId + @"\save-data.txt";
            string gameName = gameNameLabel.Text;
            string appPath = localApplicationDataFolderPath + @"\OfficeWare\GameManager\" + currentUserId + @"\";
            // string cachePath = appPath + gameName;
            string cachePath = appPath + @"games\" + gameName;
            string filename = cachePath + @"\game.exe";
            JavaScriptSerializer js = new JavaScriptSerializer();
            string saveDataFileContent = File.ReadAllText(saveDataFilePath);
            SavedContent loadedContent = js.Deserialize<SavedContent>(saveDataFileContent);
            List<Game> updatedGames = loadedContent.games;
            List<FriendSettings> currentFriends = loadedContent.friends;
            Settings currentSettings = loadedContent.settings;
            List<string> currentCollections = loadedContent.collections;
            Notifications currentNotifications = loadedContent.notifications;
            object gameNameLabelData = gameNameLabel.DataContext;
            string gameUploadedPath = ((string)(gameNameLabelData));
            DateTime currentDate = DateTime.Now;
            string gameLastLaunchDate = currentDate.ToLongDateString();
            string rawTimerHours = timerHours.ToString();
            int gameIndex = -1;
            foreach (Game someGame in updatedGames)
            {
                string someGameName = someGame.name;
                bool isNamesMatch = someGameName == gameName;
                if (isNamesMatch)
                {
                    gameIndex = updatedGames.IndexOf(someGame);
                    break;
                }
            }
            bool isGameFound = gameIndex >= 0;
            if (isGameFound)
            {
                Game currentGame = updatedGames[gameIndex];
                string currentGameId = currentGame.id;
                string currentGameName = currentGame.name;
                string currentGamePath = currentGame.path;
                string currentInstallDate = currentGame.installDate;
                updatedGames[gameIndex] = new Game()
                {
                    id = currentGameId,
                    name = currentGameName,
                    path = currentGamePath,
                    hours = rawTimerHours,
                    date = gameLastLaunchDate,
                    installDate = currentInstallDate,
                    collections = new List<string>(),
                    isHidden = false,
                    cover = "",
                    overlay = true
                };
                string savedContent = js.Serialize(new SavedContent
                {
                    games = updatedGames,
                    friends = currentFriends,
                    settings = currentSettings,
                    collections = currentCollections,
                    notifications = currentNotifications
                });
                File.WriteAllText(saveDataFilePath, savedContent);

                DecreaseUserToGameStats(currentGameId);

            }

            GetGamesInfo();

            // SetUserStatus("online");
            UpdateUserStatus("online");

            client.EmitAsync("user_is_toggle_status", "online");

            GetScreenShots("", false);

        }

        public void DecreaseUserToGameStats(string gameId)
        {
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/games/stats/decrease/?id=" + gameId);
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (var reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        var objText = reader.ReadToEnd();

                        UserResponseInfo myobj = (UserResponseInfo)js.Deserialize(objText, typeof(UserResponseInfo));

                        string status = myobj.status;
                        bool isOkStatus = status == "OK";
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

        private void InstallGameHandler(object sender, RoutedEventArgs e)
        {
            InstallGame();
        }

        public void InstallGame()
        {

            object rawGameActionLabelData = gameActionLabel.DataContext;
            Dictionary<String, Object> dataParts = ((Dictionary<String, Object>)(rawGameActionLabelData));
            string gameId = ((string)(dataParts["id"]));
            string gameName = ((string)(dataParts["name"]));
            /*string gameUrl = ((string)(dataParts["url"]));
            string gameImg = ((string)(dataParts["image"]));*/
            string gameUrl = @"http://localhost:4000/api/game/distributive/?name=" + gameName; ;
            string gameImg = @"http://localhost:4000/api/game/thumbnail/?name=" + gameName;

            Dialogs.DownloadGameDialog dialog = new Dialogs.DownloadGameDialog(currentUserId);
            dialog.DataContext = dataParts;
            dialog.Closed += GameDownloadedHandler;
            dialog.Show();
            Environment.SpecialFolder localApplicationDataFolder = Environment.SpecialFolder.LocalApplicationData;
            string localApplicationDataFolderPath = Environment.GetFolderPath(localApplicationDataFolder);
            string appFolder = localApplicationDataFolderPath + @"\OfficeWare\GameManager\" + currentUserId + @"\";
            string cachePath = appFolder + @"games\" + gameName;
            // string cachePath = appFolder + gameName;
            string filename = cachePath + @"\game.exe";
            gameNameLabel.DataContext = ((string)(filename));
            gameActionLabel.IsEnabled = false;
        }


        public void GameSuccessDownloaded (string id)
        {
            Environment.SpecialFolder localApplicationDataFolder = Environment.SpecialFolder.LocalApplicationData;
            string localApplicationDataFolderPath = Environment.GetFolderPath(localApplicationDataFolder);
            string saveDataFilePath = localApplicationDataFolderPath + @"\OfficeWare\GameManager\" + currentUserId + @"\save-data.txt";
            string gameId = id;
            string gameName = gameNameLabel.Text;
            string appPath = localApplicationDataFolderPath + @"\OfficeWare\GameManager\" + currentUserId + @"\";
            string cachePath = appPath + @"games\" + gameName;
            // string cachePath = appPath + gameName;
            Directory.CreateDirectory(cachePath);
            string filename = cachePath + @"\game.exe";
            JavaScriptSerializer js = new JavaScriptSerializer();
            string saveDataFileContent = File.ReadAllText(saveDataFilePath);
            SavedContent loadedContent = js.Deserialize<SavedContent>(saveDataFileContent);
            List<Game> updatedGames = loadedContent.games;
            List<FriendSettings> currentFriends = loadedContent.friends;
            Settings currentSettings = loadedContent.settings;
            List<string> currentCollections = loadedContent.collections;
            Notifications currentNotifications = loadedContent.notifications;
            object gameNameLabelData = gameNameLabel.DataContext;
            string gameUploadedPath = ((string)(gameNameLabelData));
            string gameHours = "0";
            DateTime currentDate = DateTime.Now;
            string gameLastLaunchDate = currentDate.ToLongDateString();
            updatedGames.Add(new Game()
            {
                id = gameId,
                name = gameName,
                path = gameUploadedPath,
                hours = gameHours,
                date = gameLastLaunchDate,
                installDate = gameLastLaunchDate,
                collections = new List<string>(),
                isHidden = false,
                cover = "",
                overlay = true
            });
            string savedContent = js.Serialize(new SavedContent
            {
                games = updatedGames,
                friends = currentFriends,
                settings = currentSettings,
                collections = currentCollections,
                notifications = currentNotifications
            });
            File.WriteAllText(saveDataFilePath, savedContent);
            gameActionLabel.Content = Properties.Resources.playBtnLabelContent;
            // gameActionLabel.IsEnabled = true;
            removeGameBtn.Visibility = visible;
            string gamePath = ((string)(gameNameLabel.DataContext));
            gameActionLabel.DataContext = filename;
            string gameUploadedLabelContent = Properties.Resources.gameUploadedLabelContent;
            string attentionLabelContent = Properties.Resources.attentionLabelContent;
            GetDownloads();

            /*ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = filename;
            startInfo.Arguments = "/D=" + cachePath + " /VERYSILENT";
            Process.Start(startInfo);*/

            GetScreenShots("", false);

            GetGamesList("");

            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/games/likes/increase/?id=" + gameId);
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (var reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        js = new JavaScriptSerializer();
                        var objText = reader.ReadToEnd();
                        UserResponseInfo myobj = (UserResponseInfo)js.Deserialize(objText, typeof(UserResponseInfo));
                        string status = myobj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
                            MessageBox.Show(gameUploadedLabelContent, attentionLabelContent);
                        }
                    }
                }
            }
            catch (System.Net.WebException)
            {
                MessageBox.Show("Не удается подключиться к серверу", "Ошибка");
                this.Close();
            }

            int countInstalledGames = updatedGames.Count;
            bool isAddIconForInstalledGames = countInstalledGames >= 2;
            if (isAddIconForInstalledGames)
            {
                bool isAlreadyExists = false;
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/icons/relations/all");
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (var reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        js = new JavaScriptSerializer();
                        var objText = reader.ReadToEnd();
                        IconRelationsResponseInfo myobj = (IconRelationsResponseInfo)js.Deserialize(objText, typeof(IconRelationsResponseInfo));
                        string status = myobj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
                            List<IconRelation> relations = myobj.relations;
                            List<IconRelation> sameIcons = relations.Where<IconRelation>((IconRelation relation) =>
                            {
                                string userId = relation.user;
                                string iconId = relation.icon;
                                HttpWebRequest innerWebRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/icons/get/?id=" + iconId);
                                innerWebRequest.Method = "GET";
                                innerWebRequest.UserAgent = ".NET Framework Test Client";
                                using (HttpWebResponse innerWebResponse = (HttpWebResponse)innerWebRequest.GetResponse())
                                {
                                    using (var innerReader = new StreamReader(innerWebResponse.GetResponseStream()))
                                    {
                                        js = new JavaScriptSerializer();
                                        objText = innerReader.ReadToEnd();
                                        IconResponseInfo myInnerObj = (IconResponseInfo)js.Deserialize(objText, typeof(IconResponseInfo));
                                        status = myInnerObj.status;
                                        isOkStatus = status == "OK";
                                        if (isOkStatus)
                                        {
                                            Icon icon = myInnerObj.icon;
                                            string title = icon.title;
                                            bool isMyRelation = userId == currentUserId;
                                            bool isRelationForInstalledGames = title == "Игроман";
                                            isAlreadyExists = isMyRelation && isRelationForInstalledGames;
                                        }
                                    }
                                }
                                return isAlreadyExists;
                            }).ToList<IconRelation>();
                            int sameIconsCount = sameIcons.Count;
                            bool isResultsNotFound = sameIconsCount <= 0;
                            if (isResultsNotFound)
                            {
                                HttpWebRequest innerWebRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/icons/all");
                                innerWebRequest.Method = "GET";
                                innerWebRequest.UserAgent = ".NET Framework Test Client";
                                using (HttpWebResponse innerWebResponse = (HttpWebResponse)innerWebRequest.GetResponse())
                                {
                                    using (var innerReader = new StreamReader(innerWebResponse.GetResponseStream()))
                                    {
                                        js = new JavaScriptSerializer();
                                        objText = innerReader.ReadToEnd();
                                        IconsResponseInfo myInnerObj = (IconsResponseInfo)js.Deserialize(objText, typeof(IconsResponseInfo));
                                        status = myInnerObj.status;
                                        isOkStatus = status == "OK";
                                        if (isOkStatus)
                                        {
                                            List<Icon> icons = myInnerObj.icons;
                                            List<Icon> results = icons.Where<Icon>((Icon icon) =>
                                            {
                                                string title = icon.title;
                                                bool isRelationForInstalledGames = title == "Игроман";
                                                return isRelationForInstalledGames;
                                            }).ToList<Icon>();
                                            int resultsCount = results.Count;
                                            bool isHaveResults = resultsCount >= 1;
                                            if (isHaveResults)
                                            {
                                                Icon result = results[0];
                                                string resultId = result._id;
                                                string desc = result.desc;
                                                HttpWebRequest nestedWebRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/icons/relations/add/?id=" + resultId + @"&user=" + currentUserId);
                                                nestedWebRequest.Method = "GET";
                                                nestedWebRequest.UserAgent = ".NET Framework Test Client";
                                                using (HttpWebResponse nestedWebResponse = (HttpWebResponse)nestedWebRequest.GetResponse())
                                                {
                                                    using (var nestedReader = new StreamReader(nestedWebResponse.GetResponseStream()))
                                                    {
                                                        js = new JavaScriptSerializer();
                                                        objText = nestedReader.ReadToEnd();
                                                        UserResponseInfo myNestedObj = (UserResponseInfo)js.Deserialize(objText, typeof(UserResponseInfo));
                                                        status = myNestedObj.status;
                                                        isOkStatus = status == "OK";
                                                        if (isOkStatus)
                                                        {
                                                            bool isNotificationsEnabled = currentNotifications.notificationsUpdateIcon;
                                                            if (isNotificationsEnabled)
                                                            {
                                                                HttpWebRequest innerNestedWebRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/users/get/?id=" + currentUserId);
                                                                innerNestedWebRequest.Method = "GET";
                                                                innerNestedWebRequest.UserAgent = ".NET Framework Test Client";
                                                                using (HttpWebResponse innerNestedWebResponse = (HttpWebResponse)innerNestedWebRequest.GetResponse())
                                                                {
                                                                    using (var innerNestedReader = new StreamReader(innerNestedWebResponse.GetResponseStream()))
                                                                    {
                                                                        js = new JavaScriptSerializer();
                                                                        objText = innerNestedReader.ReadToEnd();
                                                                        UserResponseInfo myInnerNestedObj = (UserResponseInfo)js.Deserialize(objText, typeof(UserResponseInfo));
                                                                        status = myInnerNestedObj.status;
                                                                        isOkStatus = status == "OK";
                                                                        if (isOkStatus)
                                                                        {
                                                                            User user = myInnerNestedObj.user;
                                                                            string email = user.login;
                                                                            try
                                                                            {
                                                                                MailMessage message = new MailMessage();
                                                                                SmtpClient smtp = new SmtpClient();
                                                                                message.From = new System.Net.Mail.MailAddress("glebdyakov2000@gmail.com");
                                                                                message.To.Add(new System.Net.Mail.MailAddress(email));
                                                                                string subjectBoxContent = @"Уведомления Office ware game manager";
                                                                                message.Subject = subjectBoxContent;
                                                                                message.IsBodyHtml = true; //to make message body as html  
                                                                                string messageBodyBoxContent = "<h3>Здравствуйте, " + email + "!</h3><p>Вы получили значок \"Игроман\"</p><p>Описание: " + desc + "</p>";
                                                                                message.Body = messageBodyBoxContent;
                                                                                smtp.Port = 587;
                                                                                smtp.Host = "smtp.gmail.com"; //for gmail host  
                                                                                smtp.EnableSsl = true;
                                                                                smtp.UseDefaultCredentials = false;
                                                                                smtp.Credentials = new NetworkCredential("glebdyakov2000@gmail.com", "ttolpqpdzbigrkhz");
                                                                                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                                                                                smtp.Send(message);
                                                                            }
                                                                            catch (Exception)
                                                                            {
                                                                                MessageBox.Show("Произошла ошибка при отправке письма", "Ошибка");
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                            GetIcons();
                                                            MessageBox.Show("Вы получили значок \"Игроман\"", "Внимание");
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
            }

        }

        private void SelectGameHandler(object sender, MouseButtonEventArgs e)
        {
            StackPanel game = ((StackPanel)(sender));
            object rawGameData = game.DataContext;
            Dictionary<String, Object> gameData = ((Dictionary<String, Object>)(rawGameData));
            SelectGame(gameData);
        }

        public void SelectGame(Dictionary<String, Object> gameData)
        {

            activeGame.Visibility = visible;

            Environment.SpecialFolder localApplicationDataFolder = Environment.SpecialFolder.LocalApplicationData;
            string localApplicationDataFolderPath = Environment.GetFolderPath(localApplicationDataFolder);
            string saveDataFilePath = localApplicationDataFolderPath + @"\OfficeWare\GameManager\" + currentUserId + @"\save-data.txt";
            JavaScriptSerializer js = new JavaScriptSerializer();
            SavedContent loadedContent = js.Deserialize<SavedContent>(File.ReadAllText(saveDataFilePath));
            List<Game> loadedGames = loadedContent.games;
            List<string> gameNames = new List<string>();
            foreach (Game loadedGame in loadedGames)
            {
                gameNames.Add(loadedGame.name);
            }
            Dictionary<String, Object> dataParts = ((Dictionary<String, Object>)(gameData));
            string gameId = ((string)(dataParts["id"]));
            string gameName = ((string)(dataParts["name"]));
            string gameUrl = ((string)(dataParts["url"]));
            string gameImg = ((string)(dataParts["image"]));
            int gamePrice = ((int)(dataParts["price"]));
            bool isCustomGame = gameId == "mockId";
            bool isNotCustomGame = !isCustomGame;
            Application.Current.Dispatcher.Invoke(() =>
            {
                // gamePhoto.BeginInit();
                Uri gameImageUri = null;
                if (isNotCustomGame)
                {
                    gameImageUri = new Uri(gameImg);
                }
                else
                {
                    gameImageUri = new Uri("https://cdn3.iconfinder.com/data/icons/solid-locations-icon-set/64/Games_2-256.png");
                }
                gamePhoto.Source = new BitmapImage(gameImageUri);
                // gamePhoto.EndInit();
            });
            bool isGameExists = gameNames.Contains(gameName);
            if (isGameExists)
            {
                gameActionLabel.Content = Properties.Resources.playBtnLabelContent;
                int gameIndex = gameNames.IndexOf(gameName);
                Game loadedGame = loadedGames[gameIndex];
                string gamePath = loadedGame.path;
                gameActionLabel.DataContext = gamePath;
                removeGameBtn.Visibility = visible;
                AddUserToGameStats(gameId);
            
                gameDetails.Visibility = visible;

            }
            else
            {
                bool isFreeGame = gamePrice <= 0;
                bool isGamePayed = false;

                HttpWebRequest innerWebRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/games/relations/all");
                innerWebRequest.Method = "GET";
                innerWebRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse innerWebResponse = (HttpWebResponse)innerWebRequest.GetResponse())
                {
                    using (var innerReader = new StreamReader(innerWebResponse.GetResponseStream()))
                    {
                        js = new JavaScriptSerializer();
                        string objText = innerReader.ReadToEnd();
                        GameRelationsResponseInfo myInnerObj = (GameRelationsResponseInfo)js.Deserialize(objText, typeof(GameRelationsResponseInfo));
                        string status = myInnerObj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
                            List<GameRelation> relations = myInnerObj.relations;
                            List<GameRelation> myPayedGames = relations.Where<GameRelation>((GameRelation relation) =>
                            {
                                string localGameId = relation.game;
                                string userId = relation.user;
                                bool isMyGame = userId == currentUserId;
                                bool isCurrentGame = localGameId == gameId;
                                bool isLocalGamePayed = isMyGame && isCurrentGame;
                                return isLocalGamePayed;
                            }).ToList<GameRelation>();
                            int myPayedGamesCount = myPayedGames.Count;
                            bool isHaveGames = myPayedGamesCount >= 1;
                            if (isHaveGames)
                            {
                                isGamePayed = true;
                            }
                        }
                    }
                }

                bool isCanInstall = isFreeGame || isGamePayed;
                if (isCanInstall)
                {
                    gameActionLabel.Content = Properties.Resources.installBtnLabelContent;
                }
                else
                {
                    string rawPrice = gamePrice.ToString();
                    string measure = "Р";
                    gameActionLabel.Content = "Купить " + rawPrice + " " + measure;
                }
                gameActionLabel.DataContext = gameData;
                removeGameBtn.Visibility = invisible;
                gameDetails.Visibility = invisible;
            }
            gameNameLabel.Text = gameName;

            removeGameBtn.DataContext = gameName;

        }

        private void GameActionHandler(object sender, RoutedEventArgs e)
        {
            GameAction();
        }

        public void AddUserToGameStats(string gameId)
        {
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/games/stats/increase/?id=" + gameId);
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (var reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        var objText = reader.ReadToEnd();
                        UserResponseInfo myobj = (UserResponseInfo)js.Deserialize(objText, typeof(UserResponseInfo));
                        string status = myobj.status;
                        bool isOkStatus = status == "OK";
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

        public void GameAction ()
        {
            object rawGameActionLabelContent = gameActionLabel.Content;
            string gameActionLabelContent = rawGameActionLabelContent.ToString();
            bool isPlayAction = gameActionLabelContent == Properties.Resources.playBtnLabelContent;
            bool isInstallAction = gameActionLabelContent == Properties.Resources.installBtnLabelContent;
            bool isBuyAction = gameActionLabelContent.StartsWith("Купить");
            if (isPlayAction)
            {
                RunGame(gameNameLabel.Text);
            }
            else if (isInstallAction)
            {
                InstallGame();
            }
            else if (isBuyAction)
            {
                object rawGameData = gameActionLabel.DataContext;
                Dictionary<String, Object> gameData = ((Dictionary<String, Object>)(rawGameData));
                BuyGame(gameData);
            }
        }

        public void BuyGame (Dictionary<String, Object> gameData)
        {
            object rawGameId = gameData["id"];
            string gameId = ((string)(rawGameId));
            object rawPrice = gameData["price"];
            int price = ((int)(rawPrice));
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
                    if (isOkStatus)
                    {
                        User user = myobj.user;
                        int amount = user.amount;
                        bool isCanBuy = amount >= price;
                        string msgContent = "";
                        if (isCanBuy)
                        {
                            HttpWebRequest innerWebRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/games/relations/add/?id=" + gameId + @"&user=" + currentUserId + @"&price=" + price);
                            innerWebRequest.Method = "GET";
                            innerWebRequest.UserAgent = ".NET Framework Test Client";
                            using (HttpWebResponse innerWebResponse = (HttpWebResponse)innerWebRequest.GetResponse())
                            {
                                using (var innerReader = new StreamReader(innerWebResponse.GetResponseStream()))
                                {
                                    js = new JavaScriptSerializer();
                                    objText = innerReader.ReadToEnd();
                                    UserResponseInfo myInnerObj = (UserResponseInfo)js.Deserialize(objText, typeof(UserResponseInfo));
                                    status = myInnerObj.status;
                                    isOkStatus = status == "OK";
                                    if (isOkStatus)
                                    {
                                        SelectGame(gameData);
                                        msgContent = "Поздравляем с приобретением игры!";
                                    }
                                }
                            }
                        }
                        else
                        {
                            msgContent = "На вашем счете недостаточно средств!";
                        }
                        MessageBox.Show(msgContent, "Внимание");
                    }
                }
            }
        }

        private void RemoveGameHandler(object sender, RoutedEventArgs e)
        {
            PackIcon icon = ((PackIcon)(sender));
            object data = icon.DataContext;
            string name = ((string)(data));
            RemoveGame(name);
        }

        public void RemoveGameFromCollectionsMenuHandler(object sender, RoutedEventArgs e)
        {
            MenuItem icon = ((MenuItem)(sender));
            object rawMenuItemData = icon.DataContext;
            Dictionary<String, Object> menuItemData = ((Dictionary<String, Object>)(rawMenuItemData));
            RemoveGameFromCollectionMenu(menuItemData);
        }

        public void RemoveGameFromCollectionMenu(Dictionary<String, Object> menuItemData)
        {
            string game = ((string)(menuItemData["game"]));
            string collection = ((string)(menuItemData["collection"]));
            RemoveGame(game);
            /*GetGameCollections();
            GetHiddenGames();*/
            GetGameCollectionItems(collection);
        }

        public void RemoveGame(string name)
        {
            Environment.SpecialFolder localApplicationDataFolder = Environment.SpecialFolder.LocalApplicationData;
            string localApplicationDataFolderPath = Environment.GetFolderPath(localApplicationDataFolder);
            string saveDataFilePath = localApplicationDataFolderPath + @"\OfficeWare\GameManager\" + currentUserId + @"\save-data.txt";

            JavaScriptSerializer js = new JavaScriptSerializer();
            string saveDataFileContent = File.ReadAllText(saveDataFilePath);
            SavedContent loadedContent = js.Deserialize<SavedContent>(saveDataFileContent);
            List<Game> updatedGames = loadedContent.games;
            List<FriendSettings> currentFriends = loadedContent.friends;
            Settings currentSettings = loadedContent.settings;
            List<string> currentCollections = loadedContent.collections;
            Notifications currentNotifications = loadedContent.notifications;
            List<Game> results = updatedGames.Where((Game someGame) =>
            {

                string someGameId = someGame.id;

                string gameName = name;
                string someGameName = someGame.name;
                bool isCurrentGame = someGameName == gameName;
                return isCurrentGame;
            }).ToList();
            int countResults = results.Count;
            bool isHaveResults = countResults >= 1;
            bool isGameWasRemoved = false;
            if (isHaveResults)
            {
                Game result = results[0];
                string resultId = result.id;
                string resultName = result.name;
                FileInfo fileInfo = new FileInfo(result.path);
                string gameFolder = fileInfo.DirectoryName;
                string gameScreenShotsFolder = localApplicationDataFolderPath + @"\OfficeWare\GameManager\" + currentUserId + @"\screenshots\" + resultName;
                try
                {
                    Debugger.Log(0, "debug", Environment.NewLine + "gameScreenShotsFolder " + gameScreenShotsFolder + Environment.NewLine);
                    bool isCustomGame = resultId == "mockId";
                    if (isCustomGame)
                    {
                        string gameCache = localApplicationDataFolderPath + @"\OfficeWare\GameManager\" + currentUserId + @"\games\" + resultName;
                        Directory.Delete(gameCache, true);
                        Directory.Delete(gameScreenShotsFolder, true);
                        Debugger.Log(0, "debug", Environment.NewLine + "удаляю локальную игру " + gameCache + Environment.NewLine);
                    }
                    else
                    {
                        Debugger.Log(0, "debug", Environment.NewLine + "удаляю магазинную игру " + gameFolder + Environment.NewLine);
                        Directory.Delete(gameFolder, true);
                        Directory.Delete(gameScreenShotsFolder, true);
                    }
                    isGameWasRemoved = true;
                }
                catch (Exception e)
                {
                    isGameWasRemoved = false;
                    MessageBox.Show("Игра запущена. Закройте ее и попробуйте удалить заново", "Ошибка");
                    Debugger.Log(0, "debug", Environment.NewLine + "Delete exception " + e + Environment.NewLine);
                }

            }
            if (isGameWasRemoved)
            {
                updatedGames = updatedGames.Where((Game someGame) =>
                {

                    string someGameId = someGame.id;

                    // string gameName = gameNameLabel.Text;
                    string gameName = name;
                    string someGameName = someGame.name;
                    bool isCurrentGame = someGameName == gameName;
                    bool isOtherGame = !isCurrentGame;
                    string someGamePath = someGame.path;
                    if (isCurrentGame)
                    {
                        FileInfo fileInfo = new FileInfo(someGamePath);
                        string gameFolder = fileInfo.DirectoryName;

                        string gameScreenShotsFolder = localApplicationDataFolderPath + @"\OfficeWare\GameManager\" + currentUserId + @"\screenshots\" + gameName;

                        /*
                        try
                        {
                            bool isCustomGame = someGameId == "mockId";
                            if (isCustomGame)
                            {
                                string gameCache = localApplicationDataFolderPath + @"\OfficeWare\GameManager\" + currentUserId + @"\games\" + gameName;
                                Directory.Delete(gameCache, true);
                                Debugger.Log(0, "debug", Environment.NewLine + "удаляю локальную игру " + gameCache +  Environment.NewLine);
                            }
                            else
                            {
                                Debugger.Log(0, "debug", Environment.NewLine + "удаляю магазинную игру " + gameFolder + Environment.NewLine);
                                Directory.Delete(gameFolder, true);
                            }

                            Directory.Delete(gameScreenShotsFolder, true);

                        }
                        catch (Exception)
                        {
                            isOtherGame = true;
                            MessageBox.Show("Игра запущена. Закройте ее и попробуйте удалить заново", "Ошибка");
                        }
                        */
                    }
                    return isOtherGame;
                }).ToList();
            }

            string savedContent = js.Serialize(new SavedContent
            {
                games = updatedGames,
                friends = currentFriends,
                settings = currentSettings,
                collections = currentCollections,
                notifications = currentNotifications
            });
            File.WriteAllText(saveDataFilePath, savedContent);
            string keywords = keywordsLabel.Text;
            GetGamesList(keywords);
            // GetGameCollections()

            GetGameCollections();
            GetHiddenGames();

        }

        public void OpenGamesLibraryHandler(object sender, RoutedEventArgs e)
        {
            OpenGamesLibrary();
        }

        public void OpenGamesLibrary()
        {
            mainControl.SelectedIndex = 0;
        }


        public void GameDownloadedHandler(object sender, EventArgs e)
        {
            Dialogs.DownloadGameDialog dialog = ((Dialogs.DownloadGameDialog)(sender));
            object dialogData = dialog.DataContext;
            Dictionary<String, Object> parsedDialogData = ((Dictionary<String, Object>)(dialogData));
            object rawStatus = parsedDialogData["status"];
            object rawId = parsedDialogData["id"];
            string status = ((string)(rawStatus));
            string id = ((string)(rawId));
            GameDownloaded(status, id);
        }

        public void GameDownloaded(string status, string id)
        {
            bool isOkStatus = status == "OK";
            if (isOkStatus)
            {
                GameSuccessDownloaded(id);
            }
            gameActionLabel.IsEnabled = true;
        }

        private void UserMenuItemSelectedHandler(object sender, RoutedEventArgs e)
        {
            ComboBox userMenu = ((ComboBox)(sender));
            int userMenuItemIndex = userMenu.SelectedIndex;
            UserMenuItemSelected(userMenuItemIndex);
        }

        public void UserMenuItemSelected(int index)
        {
            bool isMyProfile = index == 1;
            bool isAboutAccount = index == 2;
            bool isExit = index == 3;
            bool isStoreSettings = index == 4;
            bool isWallet = index == 5;
            if (isMyProfile)
            {
                ReturnToProfile();
            }
            else if (isAboutAccount)
            {
                GetAccountSettings();
                mainControl.SelectedIndex = 15;
            }
            else if (isExit)
            {
                Logout();
            }
            else if (isStoreSettings)
            {
                OpenStoreSettings();
            }
            else if (isWallet)
            {
                OpenIncreaseAmount();
            }
            ResetMenu();
        }

        public void GetIcons ()
        {
            icons.Children.Clear();
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/icons/all/?id=" + currentUserId);
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (var reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        var objText = reader.ReadToEnd();
                        IconsResponseInfo myobj = (IconsResponseInfo)js.Deserialize(objText, typeof(IconsResponseInfo));
                        string status = myobj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
                            List<Icon> totalIcons = myobj.icons;
                            foreach (Icon totalIconsItem in totalIcons)
                            {
                                string id = totalIconsItem._id;
                                string title = totalIconsItem.title;
                                /*PackIcon icon = new PackIcon();
                                icon.Kind = PackIconKind.Circle;
                                icon.Width = 50;
                                icon.Height = 50;
                                icon.Margin = new Thickness(10);
                                icon.ToolTip = title;*/
                                Ellipse icon = new Ellipse();
                                icon.Width = 50;
                                icon.Height = 50;
                                icon.Margin = new Thickness(10);
                                icon.ToolTip = title;
                                Uri src = new Uri("http://localhost:4000/api/icon/photo/?id=" + id);
                                BitmapImage iconBrushSrc = new BitmapImage(src);
                                ImageBrush iconBrush = new ImageBrush(iconBrushSrc);
                                icon.Fill = iconBrush;
                                icons.Children.Add(icon);
                            }
                            int totalIconsCount = totalIcons.Count;
                            string rawtotalIconsCount = totalIconsCount.ToString();
                            string newLine = Environment.NewLine;
                            HttpWebRequest innerWebRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/icons/relations/all");
                            innerWebRequest.Method = "GET";
                            innerWebRequest.UserAgent = ".NET Framework Test Client";
                            using (HttpWebResponse innerWebResponse = (HttpWebResponse)innerWebRequest.GetResponse())
                            {
                                using (var innerReader = new StreamReader(innerWebResponse.GetResponseStream()))
                                {
                                    js = new JavaScriptSerializer();
                                    objText = innerReader.ReadToEnd();
                                    IconRelationsResponseInfo myInnerObj = (IconRelationsResponseInfo)js.Deserialize(objText, typeof(IconRelationsResponseInfo));
                                    status = myInnerObj.status;
                                    isOkStatus = status == "OK";
                                    if (isOkStatus)
                                    {
                                        List<IconRelation> relations = myInnerObj.relations;
                                        List<IconRelation> myIconRelations = relations.Where<IconRelation>((IconRelation relation) =>
                                        {
                                            string userId = relation.user;
                                            bool isMyIconRelation = userId == currentUserId;
                                            return isMyIconRelation;
                                        }).ToList<IconRelation>();
                                        int myIconRelationsCount = myIconRelations.Count;
                                        string rawMyIconRelationsCount = myIconRelationsCount.ToString();
                                        int leftIconsCount = totalIconsCount - myIconRelationsCount;
                                        string rawLeftIconsCount = leftIconsCount.ToString();
                                        int iconLevel = myIconRelationsCount + 1;
                                        string rawIconLevel = iconLevel.ToString();
                                        string totalIconsCompletedLabelContent = @"Выполнено " + rawMyIconRelationsCount + " из " + rawtotalIconsCount + @" заданий сообщества " + newLine + "Office ware game manager. Завершите еще 0, чтобы получить" + newLine + "значок " + rawIconLevel + " уровня";
                                        totalIconsCompletedLabel.Text = totalIconsCompletedLabelContent;
                                        string notCompletedIconsLabelContent = @"Заданий" + newLine + "осталось: " + rawLeftIconsCount;
                                        notCompletedIconsLabel.Text = notCompletedIconsLabelContent;
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

        private void OpenAddFriendDialogHandler(object sender, RoutedEventArgs e)
        {
            OpenAddFriendDialog();
        }

        public void OpenAddFriendDialog()
        {
            Dialogs.AddFriendDialog dialog = new Dialogs.AddFriendDialog(currentUserId, client, mainControl);
            dialog.Show();
        }

        private void OpenFriendsDialogHandler(object sender, RoutedEventArgs e)
        {
            OpenFriendsDialog();
        }

        public void OpenFriendsDialog()
        {
            Dialogs.FriendsDialog dialog = new Dialogs.FriendsDialog(currentUserId, client, mainControl);
            dialog.Closed += JoinToGameHandler;
            dialog.Show();
        }

        public void JoinToGameHandler(object sender, EventArgs e)
        {
            Dialogs.FriendsDialog dialog = ((Dialogs.FriendsDialog)(sender));
            object dialogData = dialog.DataContext;
            bool isDialogDataExists = dialogData != null;
            if (isDialogDataExists)
            {
                string friend = ((string)(dialogData));
                RunGame(gameNameLabel.Text);
            }
            else
            {
                /*string userId = ((string)(mainControl.DataContext));
                GetUserInfo(userId, userId == currentUserId);*/
                ToggleWindow();
            }
        }

        public void CloseFriendRequestHandler(object sender, RoutedEventArgs e)
        {
            PackIcon btn = ((PackIcon)(sender));
            object btnData = btn.DataContext;
            Popup request = ((Popup)(btnData));
            CloseFriendRequest(request);
        }


        public void CloseFriendRequest(Popup request)
        {
            friendRequests.Children.Remove(request);
        }

        public void RejectFriendRequestHandler(object sender, RoutedEventArgs e)
        {
            Button btn = ((Button)(sender));
            object rawBtnData = btn.DataContext;
            Dictionary<String, Object> btnData = ((Dictionary<String, Object>)(rawBtnData));
            string friendId = ((string)(btnData["friendId"]));
            string requestId = ((string)(btnData["requestId"]));
            Popup request = ((Popup)(btnData["request"]));
            RejectFriendRequest(friendId, requestId, request);
        }

        public void GetFriendRequestsForMe()
        {
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/friends/requests/get/?id=" + currentUserId);
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (var reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        var objText = reader.ReadToEnd();
                        FriendRequestsResponseInfo myobj = (FriendRequestsResponseInfo)js.Deserialize(objText, typeof(FriendRequestsResponseInfo));
                        string status = myobj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
                            List<FriendRequest> requestsForMe = new List<FriendRequest>();
                            List<FriendRequest> requests = myobj.requests;
                            foreach (FriendRequest request in requests)
                            {
                                string recepientId = request.friend;
                                bool isRequestForMe = currentUserId == recepientId;
                                if (isRequestForMe)
                                {
                                    requestsForMe.Add(request);
                                }
                            }
                            friendRequestsForMe.Children.Clear();
                            int countRequestsForMe = requestsForMe.Count;
                            bool isHaveRequests = countRequestsForMe >= 1;
                            if (isHaveRequests)
                            {
                                foreach (FriendRequest requestForMe in requestsForMe)
                                {
                                    string requestId = requestForMe._id;
                                    string senderId = requestForMe.user;
                                    string friendId = requestForMe.friend;
                                    HttpWebRequest innerWebRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/users/get/?id=" + senderId);
                                    innerWebRequest.Method = "GET";
                                    innerWebRequest.UserAgent = ".NET Framework Test Client";
                                    using (HttpWebResponse innerWebResponse = (HttpWebResponse)innerWebRequest.GetResponse())
                                    {
                                        using (var innerReader = new StreamReader(innerWebResponse.GetResponseStream()))
                                        {
                                            js = new JavaScriptSerializer();
                                            objText = innerReader.ReadToEnd();
                                            UserResponseInfo myInnerObj = (UserResponseInfo)js.Deserialize(objText, typeof(UserResponseInfo));
                                            status = myInnerObj.status;
                                            isOkStatus = status == "OK";
                                            if (isOkStatus)
                                            {
                                                User user = myInnerObj.user;
                                                string senderLogin = user.login;
                                                string senderName = user.name;
                                                string insensitiveCaseSenderName = senderName.ToLower();
                                                string friendRequestsForMeBoxContent = friendRequestsForMeBox.Text;
                                                string insensitiveCaseKeywords = friendRequestsForMeBoxContent.ToLower();
                                                bool isFriendFound = insensitiveCaseSenderName.Contains(insensitiveCaseKeywords);
                                                int insensitiveCaseKeywordsLength = insensitiveCaseKeywords.Length;
                                                bool isFilterDisabled = insensitiveCaseKeywordsLength <= 0;
                                                bool isRequestMatch = isFriendFound || isFilterDisabled;
                                                if (isRequestMatch)
                                                {
                                                    StackPanel friend = new StackPanel();
                                                    friend.Margin = new Thickness(15);
                                                    friend.Width = 250;
                                                    friend.Height = 50;
                                                    friend.Orientation = Orientation.Horizontal;
                                                    friend.Background = System.Windows.Media.Brushes.DarkCyan;
                                                    Image friendIcon = new Image();
                                                    friendIcon.Width = 50;
                                                    friendIcon.Height = 50;
                                                    friendIcon.BeginInit();
                                                    friendIcon.Source = new BitmapImage(new Uri(@"https://cdn2.iconfinder.com/data/icons/ios-7-icons/50/user_male-128.png"));
                                                    friendIcon.EndInit();
                                                    friendIcon.ImageFailed += SetDefautAvatarHandler;
                                                    friend.Children.Add(friendIcon);
                                                    Separator friendStatus = new Separator();
                                                    friendStatus.BorderBrush = System.Windows.Media.Brushes.LightGray;
                                                    friendStatus.LayoutTransform = new RotateTransform(90);
                                                    friend.Children.Add(friendStatus);
                                                    TextBlock friendNameLabel = new TextBlock();
                                                    friendNameLabel.Margin = new Thickness(10, 0, 10, 0);
                                                    friendNameLabel.VerticalAlignment = VerticalAlignment.Center;
                                                    friendNameLabel.Text = senderLogin;
                                                    friend.Children.Add(friendNameLabel);
                                                    friendRequestsForMe.Children.Add(friend);
                                                    ContextMenu friendContextMenu = new ContextMenu();
                                                    MenuItem friendContextMenuItem = new MenuItem();
                                                    friendContextMenuItem.Header = "Принять";
                                                    Dictionary<String, Object> friendContextMenuItemData = new Dictionary<String, Object>();
                                                    friendContextMenuItemData.Add("friendId", senderId);
                                                    friendContextMenuItemData.Add("requestId", requestId);
                                                    friendContextMenuItem.DataContext = friendContextMenuItemData;
                                                    friendContextMenuItem.Click += AcceptFriendRequestFromSettingsHandler;
                                                    friendContextMenu.Items.Add(friendContextMenuItem);
                                                    friendContextMenuItem = new MenuItem();
                                                    friendContextMenuItem.Header = "Отклонить";
                                                    friendContextMenuItemData = new Dictionary<String, Object>();
                                                    friendContextMenuItemData.Add("friendId", senderId);
                                                    friendContextMenuItemData.Add("requestId", requestId);
                                                    friendContextMenuItem.DataContext = friendContextMenuItemData;
                                                    friendContextMenuItem.Click += RejectFriendRequestFromSettingsHandler;
                                                    friendContextMenu.Items.Add(friendContextMenuItem);
                                                    friend.ContextMenu = friendContextMenu;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                TextBlock requestsNotFoundLabel = new TextBlock();
                                requestsNotFoundLabel.Margin = new Thickness(15);
                                requestsNotFoundLabel.FontSize = 18;
                                requestsNotFoundLabel.Text = "Извините, здесь никого нет.";
                                friendRequestsForMe.Children.Add(requestsNotFoundLabel);
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

        public void AcceptGroupRequestFromSettingsHandler(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = ((MenuItem)(sender));
            object rawMenuItemData = menuItem.DataContext;
            Dictionary<String, Object> menuItemData = ((Dictionary<String, Object>)(rawMenuItemData));
            string groupId = ((string)(menuItemData["groupId"]));
            string userId = ((string)(menuItemData["userId"]));
            string requestId = ((string)(menuItemData["requestId"]));
            AcceptGroupRequestFromSettings(groupId, userId, requestId);
        }

        public void AcceptGroupRequestFromSettings(string groupId, string userId, string requestId)
        {
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/groups/relations/add/?id=" + groupId + @"&user=" + userId + "&request=" + requestId);
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (var reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        var objText = reader.ReadToEnd();
                        UserResponseInfo myobj = (UserResponseInfo)js.Deserialize(objText, typeof(UserResponseInfo));
                        string status = myobj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
                            HttpWebRequest innerWebRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/groups/get/?id=" + groupId);
                            innerWebRequest.Method = "GET";
                            innerWebRequest.UserAgent = ".NET Framework Test Client";
                            using (HttpWebResponse innerWebResponse = (HttpWebResponse)innerWebRequest.GetResponse())
                            {
                                using (StreamReader innerReader = new StreamReader(innerWebResponse.GetResponseStream()))
                                {
                                    js = new JavaScriptSerializer();
                                    objText = innerReader.ReadToEnd();
                                    GroupResponseInfo myInnerObj = (GroupResponseInfo)js.Deserialize(objText, typeof(GroupResponseInfo));
                                    status = myInnerObj.status;
                                    isOkStatus = status == "OK";
                                    if (isOkStatus)
                                    {
                                        Group group = myInnerObj.group;
                                        string groupName = group.name;
                                        string msgContent = "Вы были успешно добавлены в группу " + groupName;
                                        MessageBox.Show(msgContent, "Внимание");
                                    }
                                }
                            }
                        }
                        else
                        {
                            MessageBox.Show("Не удалось принять приглашение", "Ошибка");
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

        public void RejectGroupRequestFromSettingsHandler(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = ((MenuItem)(sender));
            object rawMenuItemData = menuItem.DataContext;
            Dictionary<String, Object> menuItemData = ((Dictionary<String, Object>)(rawMenuItemData));
            string groupId = ((string)(menuItemData["groupId"]));
            string userId = ((string)(menuItemData["userId"]));
            string requestId = ((string)(menuItemData["requestId"]));
            RejectGroupRequestFromSettings(groupId, userId, requestId);
        }

        public void RejectGroupRequestFromSettings(string groupId, string userId, string requestId)
        {
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/groups/requests/reject/?id=" + requestId);
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (var reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        var objText = reader.ReadToEnd();
                        UserResponseInfo myobj = (UserResponseInfo)js.Deserialize(objText, typeof(UserResponseInfo));
                        string status = myobj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
                            webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/users/get/?id=" + userId);
                            webRequest.Method = "GET";
                            webRequest.UserAgent = ".NET Framework Test Client";
                            using (HttpWebResponse innerWebResponse = (HttpWebResponse)webRequest.GetResponse())
                            {
                                using (StreamReader innerReader = new StreamReader(innerWebResponse.GetResponseStream()))
                                {
                                    js = new JavaScriptSerializer();
                                    objText = innerReader.ReadToEnd();
                                    myobj = (UserResponseInfo)js.Deserialize(objText, typeof(UserResponseInfo));
                                    status = myobj.status;
                                    isOkStatus = status == "OK";
                                    if (isOkStatus)
                                    {
                                        User friend = myobj.user;
                                        string friendLogin = friend.login;
                                        string msgContent = "Вы отклонили приглашение в группу";
                                        GetGroupRequests();
                                        GetFriendsSettings();
                                        MessageBox.Show(msgContent, "Внимание");
                                    }
                                }
                            }
                        }
                        else
                        {
                            MessageBox.Show("Не удалось отклонить приглашение", "Ошибка");
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

        public void AcceptFriendRequestFromSettingsHandler(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = ((MenuItem)(sender));
            object rawMenuItemData = menuItem.DataContext;
            Dictionary<String, Object> menuItemData = ((Dictionary<String, Object>)(rawMenuItemData));
            string friendId = ((string)(menuItemData["friendId"]));
            string requestId = ((string)(menuItemData["requestId"]));
            AcceptFriendRequestFromSettings(friendId, requestId);
        }

        public void AcceptFriendRequestFromSettings(string friendId, string requestId)
        {
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/friends/add/?id=" + currentUserId + @"&friend=" + friendId + "&request=" + requestId);
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (var reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        var objText = reader.ReadToEnd();
                        UserResponseInfo myobj = (UserResponseInfo)js.Deserialize(objText, typeof(UserResponseInfo));
                        string status = myobj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
                            webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/users/get/?id=" + friendId);
                            webRequest.Method = "GET";
                            webRequest.UserAgent = ".NET Framework Test Client";
                            using (HttpWebResponse innerWebResponse = (HttpWebResponse)webRequest.GetResponse())
                            {
                                using (StreamReader innerReader = new StreamReader(innerWebResponse.GetResponseStream()))
                                {
                                    js = new JavaScriptSerializer();
                                    objText = innerReader.ReadToEnd();
                                    myobj = (UserResponseInfo)js.Deserialize(objText, typeof(UserResponseInfo));
                                    status = myobj.status;
                                    isOkStatus = status == "OK";
                                    if (isOkStatus)
                                    {
                                        User friend = myobj.user;
                                        string friendName = friend.name;
                                        string msgContent = "Пользователь " + friendName + " был добавлен в друзья";
                                        Environment.SpecialFolder localApplicationDataFolder = Environment.SpecialFolder.LocalApplicationData;
                                        string localApplicationDataFolderPath = Environment.GetFolderPath(localApplicationDataFolder);
                                        string saveDataFilePath = localApplicationDataFolderPath + @"\OfficeWare\GameManager\" + currentUserId + @"\save-data.txt";
                                        js = new JavaScriptSerializer();
                                        string saveDataFileContent = File.ReadAllText(saveDataFilePath);
                                        SavedContent loadedContent = js.Deserialize<SavedContent>(saveDataFileContent);
                                        List<Game> currentGames = loadedContent.games;
                                        Settings currentSettings = loadedContent.settings;
                                        List<FriendSettings> currentFriends = loadedContent.friends;
                                        List<string> currentCollections = loadedContent.collections;
                                        Notifications currentNotifications = loadedContent.notifications; 
                                        List<FriendSettings> updatedFriends = currentFriends;
                                        updatedFriends.Add(new FriendSettings()
                                        {
                                            id = friendId,
                                            isFriendOnlineNotification = true,
                                            isFriendOnlineSound = true,
                                            isFriendPlayedNotification = true,
                                            isFriendPlayedSound = true,
                                            isFriendSendMsgNotification = true,
                                            isFriendSendMsgSound = true,
                                            isFavoriteFriend = false
                                        });
                                        string savedContent = js.Serialize(new SavedContent
                                        {
                                            games = currentGames,
                                            friends = updatedFriends,
                                            settings = currentSettings,
                                            collections = currentCollections,
                                            notifications = currentNotifications
                                        });
                                        File.WriteAllText(saveDataFilePath, savedContent);
                                        GetFriendsSettings();
                                        GetFriendRequests();
                                        MessageBox.Show(msgContent, "Внимание");
                                    }
                                }
                            }
                        }
                        else
                        {
                            MessageBox.Show("Не удалось принять приглашение", "Ошибка");
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

        public void RejectFriendRequestFromSettingsHandler(object sender, RoutedEventArgs e)
        {

            MenuItem menuItem = ((MenuItem)(sender));
            object rawMenuItemData = menuItem.DataContext;
            Dictionary<String, Object> menuItemData = ((Dictionary<String, Object>)(rawMenuItemData));
            string friendId = ((string)(menuItemData["friendId"]));
            string requestId = ((string)(menuItemData["requestId"]));
            RejectFriendRequestFromSettings(friendId, requestId);
        }

        public void RejectFriendRequestFromSettings(string friendId, string requestId)
        {
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/friends/requests/reject/?id=" + requestId);
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (var reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        var objText = reader.ReadToEnd();

                        UserResponseInfo myobj = (UserResponseInfo)js.Deserialize(objText, typeof(UserResponseInfo));

                        string status = myobj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
                            webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/users/get/?id=" + friendId);
                            webRequest.Method = "GET";
                            webRequest.UserAgent = ".NET Framework Test Client";
                            using (HttpWebResponse innerWebResponse = (HttpWebResponse)webRequest.GetResponse())
                            {
                                using (StreamReader innerReader = new StreamReader(innerWebResponse.GetResponseStream()))
                                {
                                    js = new JavaScriptSerializer();
                                    objText = innerReader.ReadToEnd();

                                    myobj = (UserResponseInfo)js.Deserialize(objText, typeof(UserResponseInfo));

                                    status = myobj.status;
                                    isOkStatus = status == "OK";
                                    if (isOkStatus)
                                    {
                                        User friend = myobj.user;
                                        string friendLogin = friend.login;
                                        string msgContent = "Вы отклонили приглашение в друзья";
                                        GetFriendsSettings();
                                        GetFriendRequests();
                                        MessageBox.Show(msgContent, "Внимание");
                                    }
                                }
                            }
                        }
                        else
                        {
                            MessageBox.Show("Не удалось отклонить приглашение", "Ошибка");
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

        public void RejectFriendRequests()
        {

            friendRequests.Children.Clear();
            GetFriendRequestsForMe();
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/friends/requests/get/?id=" + currentUserId);
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (var reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        var objText = reader.ReadToEnd();
                        FriendRequestsResponseInfo myobj = (FriendRequestsResponseInfo)js.Deserialize(objText, typeof(FriendRequestsResponseInfo));
                        string status = myobj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
                            List<FriendRequest> myRequests = new List<FriendRequest>();
                            List<FriendRequest> requests = myobj.requests;
                            foreach (FriendRequest request in requests)
                            {
                                string recepientId = request.friend;
                                bool isRequestForMe = currentUserId == recepientId;
                                if (isRequestForMe)
                                {
                                    myRequests.Add(request);
                                }
                            }
                            foreach (FriendRequest myRequest in myRequests)
                            {
                                string requestId = myRequest._id;
                                string friendId = myRequest.user;
                                HttpWebRequest innerWebRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/friends/requests/reject/?id=" + requestId);
                                innerWebRequest.Method = "GET";
                                innerWebRequest.UserAgent = ".NET Framework Test Client";
                                using (HttpWebResponse innerWebResponse = (HttpWebResponse)innerWebRequest.GetResponse())
                                {
                                    using (var innerReader = new StreamReader(innerWebResponse.GetResponseStream()))
                                    {
                                        js = new JavaScriptSerializer();
                                        objText = reader.ReadToEnd();
                                        UserResponseInfo myInnerObj = (UserResponseInfo)js.Deserialize(objText, typeof(UserResponseInfo));
                                        status = myobj.status;
                                        isOkStatus = status == "OK";
                                        if (isOkStatus)
                                        {
                                            HttpWebRequest nestedWebRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/users/get/?id=" + friendId);
                                            nestedWebRequest.Method = "GET";
                                            nestedWebRequest.UserAgent = ".NET Framework Test Client";
                                            using (HttpWebResponse nestedWebResponse = (HttpWebResponse)nestedWebRequest.GetResponse())
                                            {
                                                using (StreamReader nestedReader = new StreamReader(nestedWebResponse.GetResponseStream()))
                                                {
                                                    js = new JavaScriptSerializer();
                                                    objText = nestedReader.ReadToEnd();
                                                    var myNestedObj = (UserResponseInfo)js.Deserialize(objText, typeof(UserResponseInfo));
                                                    status = myNestedObj.status;
                                                    isOkStatus = status == "OK";
                                                    if (isOkStatus)
                                                    {

                                                    }
                                                    else
                                                    {
                                                        MessageBox.Show("Не удалось отклонить приглашение", "Ошибка");
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

            /*
            int countFriendRequests = friendRequests.Children.Count;
            for (int i = 0; i < countFriendRequests; i++)
            {
                UIElement rawFriendRequest = friendRequests.Children[i];
                Popup friendRequest = ((Popup)(rawFriendRequest));
                object rawFriendRequestData = friendRequest.DataContext;
                Dictionary<String, Object> friendRequestData = ((Dictionary<String, Object>)(rawFriendRequestData));
                string friendId = ((string)(friendRequestData["friendId"]));
                string requestId = ((string)(friendRequestData["requestId"]));
                RejectFriendRequest(friendId, requestId, friendRequest);
            }
            */

        }

        public void RejectFriendRequest(string friendId, string requestId, Popup request)
        {
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/friends/requests/reject/?id=" + requestId);
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (var reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        var objText = reader.ReadToEnd();

                        UserResponseInfo myobj = (UserResponseInfo)js.Deserialize(objText, typeof(UserResponseInfo));

                        string status = myobj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
                            webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/users/get/?id=" + friendId);
                            webRequest.Method = "GET";
                            webRequest.UserAgent = ".NET Framework Test Client";
                            using (HttpWebResponse innerWebResponse = (HttpWebResponse)webRequest.GetResponse())
                            {
                                using (StreamReader innerReader = new StreamReader(innerWebResponse.GetResponseStream()))
                                {
                                    js = new JavaScriptSerializer();
                                    objText = innerReader.ReadToEnd();

                                    myobj = (UserResponseInfo)js.Deserialize(objText, typeof(UserResponseInfo));

                                    status = myobj.status;
                                    isOkStatus = status == "OK";
                                    if (isOkStatus)
                                    {
                                        CloseFriendRequest(request);
                                        User friend = myobj.user;
                                        string friendLogin = friend.login;
                                        string msgContent = "Вы отклонили приглашение в друзья";
                                        MessageBox.Show(msgContent, "Внимание");
                                    }
                                }
                            }
                        }
                        else
                        {
                            MessageBox.Show("Не удалось отклонить приглашение", "Ошибка");
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

        public void AcceptFriendRequestHandler(object sender, RoutedEventArgs e)
        {
            Button btn = ((Button)(sender));
            object rawBtnData = btn.DataContext;
            Dictionary<String, Object> btnData = ((Dictionary<String, Object>)(rawBtnData));
            string friendId = ((string)(btnData["friendId"]));
            string requestId = ((string)(btnData["requestId"]));
            Popup request = ((Popup)(btnData["request"]));
            AcceptFriendRequest(friendId, requestId, request);
        }

        public void AcceptFriendRequest(string friendId, string requestId, Popup request)
        {
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/friends/add/?id=" + currentUserId + @"&friend=" + friendId + "&request=" + requestId);
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (var reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        var objText = reader.ReadToEnd();

                        UserResponseInfo myobj = (UserResponseInfo)js.Deserialize(objText, typeof(UserResponseInfo));

                        string status = myobj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
                            webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/users/get/?id=" + friendId);
                            webRequest.Method = "GET";
                            webRequest.UserAgent = ".NET Framework Test Client";
                            using (HttpWebResponse innerWebResponse = (HttpWebResponse)webRequest.GetResponse())
                            {
                                using (StreamReader innerReader = new StreamReader(innerWebResponse.GetResponseStream()))
                                {
                                    js = new JavaScriptSerializer();
                                    objText = innerReader.ReadToEnd();

                                    myobj = (UserResponseInfo)js.Deserialize(objText, typeof(UserResponseInfo));

                                    status = myobj.status;
                                    isOkStatus = status == "OK";
                                    if (isOkStatus)
                                    {
                                        CloseFriendRequest(request);
                                        User friend = myobj.user;
                                        string friendName = friend.name;
                                        string msgContent = "Пользователь " + friendName + " был добавлен в друзья";
                                        Environment.SpecialFolder localApplicationDataFolder = Environment.SpecialFolder.LocalApplicationData;
                                        string localApplicationDataFolderPath = Environment.GetFolderPath(localApplicationDataFolder);
                                        string saveDataFilePath = localApplicationDataFolderPath + @"\OfficeWare\GameManager\" + currentUserId + @"\save-data.txt";
                                        js = new JavaScriptSerializer();
                                        string saveDataFileContent = File.ReadAllText(saveDataFilePath);
                                        SavedContent loadedContent = js.Deserialize<SavedContent>(saveDataFileContent);
                                        List<Game> currentGames = loadedContent.games;
                                        Settings currentSettings = loadedContent.settings;
                                        List<FriendSettings> currentFriends = loadedContent.friends;
                                        List<string> currentCollections = loadedContent.collections;
                                        Notifications currentNotifications = loadedContent.notifications; 
                                        List<FriendSettings> updatedFriends = currentFriends;
                                        updatedFriends.Add(new FriendSettings()
                                        {
                                            id = friendId,
                                            isFriendOnlineNotification = true,
                                            isFriendOnlineSound = true,
                                            isFriendPlayedNotification = true,
                                            isFriendPlayedSound = true,
                                            isFriendSendMsgNotification = true,
                                            isFriendSendMsgSound = true,
                                            isFavoriteFriend = false
                                        });
                                        string savedContent = js.Serialize(new SavedContent
                                        {
                                            games = currentGames,
                                            friends = updatedFriends,
                                            settings = currentSettings,
                                            collections = currentCollections,
                                            notifications = currentNotifications
                                        });
                                        File.WriteAllText(saveDataFilePath, savedContent);
                                        MessageBox.Show(msgContent, "Внимание");
                                    }
                                }
                            }
                        }
                        else
                        {
                            MessageBox.Show("Не удалось принять приглашение", "Ошибка");
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

        public void AcceptGroupRequestHandler(object sender, RoutedEventArgs e)
        {
            Button btn = ((Button)(sender));
            object rawBtnData = btn.DataContext;
            Dictionary<String, Object> btnData = ((Dictionary<String, Object>)(rawBtnData));
            string groupId = ((string)(btnData["groupId"]));
            string userId = ((string)(btnData["userId"]));
            string requestId = ((string)(btnData["requestId"]));
            Popup request = ((Popup)(btnData["request"]));
            AcceptGroupRequest(groupId, userId, requestId, request);
        }

        public void AcceptGroupRequest(string groupId, string userId, string requestId, Popup request)
        {
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/groups/relations/add/?id=" + groupId + @"&user=" + userId + "&request=" + requestId);
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (var reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        var objText = reader.ReadToEnd();

                        UserResponseInfo myobj = (UserResponseInfo)js.Deserialize(objText, typeof(UserResponseInfo));

                        string status = myobj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
                            HttpWebRequest innerWebRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/groups/get/?id=" + groupId);
                            innerWebRequest.Method = "GET";
                            innerWebRequest.UserAgent = ".NET Framework Test Client";
                            using (HttpWebResponse innerWebResponse = (HttpWebResponse)innerWebRequest.GetResponse())
                            {
                                using (StreamReader innerReader = new StreamReader(innerWebResponse.GetResponseStream()))
                                {
                                    js = new JavaScriptSerializer();
                                    objText = innerReader.ReadToEnd();
                                    GroupResponseInfo myInnerObj = (GroupResponseInfo)js.Deserialize(objText, typeof(GroupResponseInfo));
                                    status = myInnerObj.status;
                                    isOkStatus = status == "OK";
                                    if (isOkStatus)
                                    {
                                        CloseGroupRequest(request);
                                        Group group = myInnerObj.group;
                                        string groupName = group.name;
                                        string msgContent = "Вы были успешно добавлены в группу " + groupName;
                                        request.IsOpen = false;
                                        GetGroupRequests();
                                        GetFriendsSettings();
                                        MessageBox.Show(msgContent, "Внимание");
                                    }
                                }
                            }
                        }
                        else
                        {
                            MessageBox.Show("Не удалось принять приглашение", "Ошибка");
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

        public CustomPopupPlacement[] PointsStoreItemsPopupPlacementHandler (Size popupSize, Size targetSize, Point offset)
        {
            return new CustomPopupPlacement[]
            {
                new CustomPopupPlacement(new Point(0, 250), PopupPrimaryAxis.Vertical),
                new CustomPopupPlacement(new Point(400, 20), PopupPrimaryAxis.Horizontal)
            };
        }

        public CustomPopupPlacement[] FriendRequestPlacementHandler(Size popupSize, Size targetSize, Point offset)
        {
            return new CustomPopupPlacement[]
            {
                new CustomPopupPlacement(new Point(-50, 100), PopupPrimaryAxis.Vertical),
                new CustomPopupPlacement(new Point(10, 20), PopupPrimaryAxis.Horizontal)
            };
        }

        private void FilterGamesHandler(object sender, TextChangedEventArgs e)
        {
            FilterGames();
        }

        public void FilterGames()
        {
            string keywords = keywordsLabel.Text;
            GetGamesList(keywords);
        }

        private void ProfileItemSelectedHandler(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = ((ComboBox)(sender));
            int selectedIndex = comboBox.SelectedIndex;
            ProfileItemSelected(selectedIndex);
        }

        private void ProfileItemSelected(int index)
        {
            if (isAppInit)
            {
                bool isActivity = index == 1;
                bool isProfile = index == 2;
                bool isFriends = index == 3;
                bool isGroups = index == 4;
                bool isContent = index == 5;
                bool isIcons = index == 6;
                if (isActivity)
                {
                    mainControl.SelectedIndex = 13;
                    AddHistoryRecord();
                }
                else if (isProfile)
                {
                    ReturnToProfile();
                }
                else if (isFriends)
                {
                    OpenFriendsSettings();
                }
                else if (isGroups)
                {
                    OpenGroupsSettings();
                }
                else if (isContent)
                {
                    mainControl.SelectedIndex = 5;
                    AddHistoryRecord();
                }
                else if (isIcons)
                {
                    mainControl.SelectedIndex = 12;
                    AddHistoryRecord();
                }
                ResetMenu();
            }
        }

        public void OpenGroupsSettings()
        {
            mainControl.SelectedIndex = 16;
            friendsSettingsControl.SelectedIndex = 8;
            GetGroups();
        }

        public void OpenFriendsSettings()
        {
            mainControl.SelectedIndex = 16;
            friendsSettingsControl.SelectedIndex = 0;
            GetFriendsSettings();
        }

        public void AddHistoryRecord()
        {
            int selectedWindowIndex = mainControl.SelectedIndex;
            historyCursor++;
            history.Add(selectedWindowIndex);
            arrowBackBtn.Foreground = enabledColor;
            arrowForwardBtn.Foreground = disabledColor;
        }

        private void LibraryItemSelectedHandler(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = ((ComboBox)(sender));
            int selectedIndex = comboBox.SelectedIndex;
            LibraryItemSelected(selectedIndex);
        }

        private void LibraryItemSelected(int index)
        {
            bool isHome = index == 1;
            bool isCollections = index == 2;
            bool isDownloads = index == 4;
            if (isHome)
            {
                OpenGamesLibrary();
                AddHistoryRecord();
            }
            else if (isCollections)
            {
                mainControl.SelectedIndex = 9;
                AddHistoryRecord();
            }
            else if (isDownloads)
            {
                mainControl.SelectedIndex = 4;
                AddHistoryRecord();
            }
            ResetMenu();
        }

        public void ResetMenu()
        {
            if (isAppInit)
            {
                storeMenu.SelectedIndex = 0;
                libraryMenu.SelectedIndex = 0;
                communityMenu.SelectedIndex = 0;
                profileMenu.SelectedIndex = 0;
                userMenu.SelectedIndex = 0;
            }
        }

        private void ClientLoadedHandler(object sender, RoutedEventArgs e)
        {
            ClientLoaded();
        }

        public void ClientLoaded()
        {
            isAppInit = true;
            mainControl.DataContext = currentUserId;
            ListenSockets();
            IncreaseUserToStats();

            // SetUserStatus("online");
            UpdateUserStatus("online");

        }

        public void GetRequestsCount()
        {
            int countRequests = 0;

            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/friends/requests/get/?id=" + currentUserId);
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (var reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        var objText = reader.ReadToEnd();
                        FriendRequestsResponseInfo myobj = (FriendRequestsResponseInfo)js.Deserialize(objText, typeof(FriendRequestsResponseInfo));
                        string status = myobj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
                            List<FriendRequest> myRequests = new List<FriendRequest>();
                            List<FriendRequest> requests = myobj.requests;
                            foreach (FriendRequest request in requests)
                            {
                                string recepientId = request.friend;
                                bool isRequestForMe = currentUserId == recepientId;
                                if (isRequestForMe)
                                {
                                    myRequests.Add(request);
                                }
                            }
                            foreach (FriendRequest myRequest in myRequests)
                            {
                                countRequests++;
                            }
                            HttpWebRequest innerWebRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/groups/requests/all");
                            innerWebRequest.Method = "GET";
                            innerWebRequest.UserAgent = ".NET Framework Test Client";
                            using (HttpWebResponse innerWebResponse = (HttpWebResponse)innerWebRequest.GetResponse())
                            {
                                using (var innerReader = new StreamReader(innerWebResponse.GetResponseStream()))
                                {
                                    js = new JavaScriptSerializer();
                                    objText = innerReader.ReadToEnd();
                                    GroupRequestsResponseInfo myInnerObj = (GroupRequestsResponseInfo)js.Deserialize(objText, typeof(GroupRequestsResponseInfo));
                                    status = myInnerObj.status;
                                    isOkStatus = status == "OK";
                                    if (isOkStatus)
                                    {
                                        List<GroupRequest> localMyRequests = new List<GroupRequest>();
                                        List<GroupRequest> localRequests = myInnerObj.requests;
                                        foreach (GroupRequest request in localRequests)
                                        {
                                            string recepientId = request.user;
                                            bool isRequestForMe = currentUserId == recepientId;
                                            if (isRequestForMe)
                                            {
                                                localMyRequests.Add(request);
                                            }
                                        }
                                        foreach (GroupRequest myRequest in localMyRequests)
                                        {
                                            countRequests++;
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

            string rawCountRequests = countRequests.ToString();
            string countNewRequestsLabelContent = "Новых приглашений: " + rawCountRequests;
            countNewRequestsLabel.Text = countNewRequestsLabelContent;
        }

        public void SetUserStatus(string userStatus)
        {
            if (client != null)
            {
                try
                {
                    HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/user/status/set/?id=" + currentUserId + "&status=" + userStatus);
                    webRequest.Method = "GET";
                    webRequest.UserAgent = ".NET Framework Test Client";
                    using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                    {
                        using (var reader = new StreamReader(webResponse.GetResponseStream()))
                        {
                            JavaScriptSerializer js = new JavaScriptSerializer();
                            var objText = reader.ReadToEnd();

                            RegisterResponseInfo myobj = (RegisterResponseInfo)js.Deserialize(objText, typeof(RegisterResponseInfo));

                            string status = myobj.status;
                            bool isErrorStatus = status == "Error";
                            if (isErrorStatus)
                            {
                                MessageBox.Show("Не удается подключиться к серверу", "Ошибка");
                            }

                            // client.EmitAsync("user_is_toggle_status", userStatus);

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

        public void IncreaseUserToStats()
        {
            HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/users/stats/increase");
            webRequest.Method = "GET";
            webRequest.UserAgent = ".NET Framework Test Client";
            using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
            {
                using (var reader = new StreamReader(webResponse.GetResponseStream()))
                {
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    var objText = reader.ReadToEnd();

                    RegisterResponseInfo myobj = (RegisterResponseInfo)js.Deserialize(objText, typeof(RegisterResponseInfo));

                    string status = myobj.status;
                    bool isErrorStatus = status == "Error";
                    if (isErrorStatus)
                    {
                        MessageBox.Show("Не удается подключиться к серверу", "Ошибка");
                    }
                }
            }
        }

        private void CommunityItemSelectedHandler(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = ((ComboBox)(sender));
            int selectedIndex = comboBox.SelectedIndex;
            CommunityItemSelected(selectedIndex);
        }

        public void CommunityItemSelected(int index)
        {
            bool isMain = index == 1;
            bool isDiscussions = index == 2;
            bool isWorkshop = index == 3;
            bool isPlatform = index == 4;
            bool isBroadcasts = index == 5;
            if (isMain)
            {
                mainControl.SelectedIndex = 20;
                GetCommunityInfo();
            }
            else if (isDiscussions)
            {
                mainControl.SelectedIndex = 6;
                AddHistoryRecord();
            }
            else if (isWorkshop)
            {
                mainControl.SelectedIndex = 38;
                AddHistoryRecord();
            }
            else if (isPlatform)
            {
                mainControl.SelectedIndex = 39;
                AddHistoryRecord();
            }
            else if (isBroadcasts)
            {
                OpenCommunityInfo();
                communityControl.SelectedIndex = 3;

            }
            ResetMenu();
        }

        private void StoreItemSelectedHandler(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = ((ComboBox)(sender));
            int selectedIndex = comboBox.SelectedIndex;
            StoreItemSelected(selectedIndex);
        }

        public void GetNews()
        {
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/news/get");
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (var reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        var objText = reader.ReadToEnd();
                        NewsResponseInfo myobj = (NewsResponseInfo)js.Deserialize(objText, typeof(NewsResponseInfo));
                        string status = myobj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
                            List<News> newsList = myobj.news;
                            news.Children.Clear();
                            foreach (News newsListItem in newsList)
                            {
                                string title = newsListItem.title;
                                string content = newsListItem.content;
                                DateTime date = newsListItem.date;
                                string game = newsListItem.game;
                                string newsGameName = "";
                                HttpWebRequest innerWebRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/games/get");
                                innerWebRequest.Method = "GET";
                                innerWebRequest.UserAgent = ".NET Framework Test Client";
                                using (HttpWebResponse innerWebResponse = (HttpWebResponse)innerWebRequest.GetResponse())
                                {
                                    using (var innerReader = new StreamReader(innerWebResponse.GetResponseStream()))
                                    {
                                        js = new JavaScriptSerializer();
                                        objText = innerReader.ReadToEnd();
                                        GamesListResponseInfo myInnerObj = (GamesListResponseInfo)js.Deserialize(objText, typeof(GamesListResponseInfo));
                                        status = myobj.status;
                                        isOkStatus = status == "OK";
                                        if (isOkStatus)
                                        {
                                            List<GameResponseInfo> games = myInnerObj.games;
                                            int gameIndex = games.FindIndex((GameResponseInfo localGame) =>
                                            {
                                                string localGameId = localGame._id;
                                                bool isFound = localGameId == game;
                                                return isFound;
                                            });
                                            bool isGameFound = gameIndex >= 0;
                                            if (isGameFound)
                                            {
                                                GameResponseInfo myGame = games[gameIndex];
                                                newsGameName = myGame.name;
                                            }
                                        }
                                    }
                                }
                                StackPanel newsItem = new StackPanel();
                                newsItem.Margin = new Thickness(50);
                                Border newsItemBody = new Border();
                                newsItemBody.Margin = new Thickness(0, 15, 0, 15);
                                newsItemBody.Background = System.Windows.Media.Brushes.DarkCyan;
                                newsItemBody.CornerRadius = new CornerRadius(5);
                                DockPanel newsItemBodyWrap = new DockPanel();
                                StackPanel newsItemBodyWrapAside = new StackPanel();
                                newsItemBodyWrapAside.Margin = new Thickness(25);
                                StackPanel newsItemBodyWrapAsideHeader = new StackPanel();
                                newsItemBodyWrapAsideHeader.Orientation = Orientation.Horizontal;
                                Image newsItemBodyWrapAsideHeaderIcon = new Image();
                                newsItemBodyWrapAsideHeaderIcon.HorizontalAlignment = HorizontalAlignment.Right;
                                newsItemBodyWrapAsideHeaderIcon.Margin = new Thickness(15);
                                newsItemBodyWrapAsideHeaderIcon.Width = 25;
                                newsItemBodyWrapAsideHeaderIcon.Height = 25;
                                newsItemBodyWrapAsideHeaderIcon.BeginInit();
                                newsItemBodyWrapAsideHeaderIcon.Source = new BitmapImage(new Uri(@"http://localhost:4000/api/game/thumbnail/?name=" + newsGameName));
                                newsItemBodyWrapAsideHeaderIcon.EndInit();
                                newsItemBodyWrapAsideHeaderIcon.ImageFailed += SetDefaultThumbnailHandler;
                                newsItemBodyWrapAsideHeader.Children.Add(newsItemBodyWrapAsideHeaderIcon);
                                TextBlock newsItemBodyWrapAsideHeaderLabel = new TextBlock();
                                newsItemBodyWrapAsideHeaderLabel.VerticalAlignment = VerticalAlignment.Center;
                                newsItemBodyWrapAsideHeaderLabel.Text = newsGameName;
                                newsItemBodyWrapAsideHeader.Children.Add(newsItemBodyWrapAsideHeaderLabel);
                                newsItemBodyWrapAside.Children.Add(newsItemBodyWrapAsideHeader);
                                TextBlock newsItemBodyWrapAsideTitleLabel = new TextBlock();
                                newsItemBodyWrapAsideTitleLabel.FontSize = 24;
                                newsItemBodyWrapAsideTitleLabel.Foreground = System.Windows.Media.Brushes.White;
                                newsItemBodyWrapAsideTitleLabel.Text = title;
                                newsItemBodyWrapAside.Children.Add(newsItemBodyWrapAsideTitleLabel);
                                TextBlock newsItemBodyWrapAsideDateLabel = new TextBlock();
                                newsItemBodyWrapAsideDateLabel.Foreground = System.Windows.Media.Brushes.White;
                                string rawDate = date.ToLongDateString();
                                newsItemBodyWrapAsideDateLabel.Text = rawDate;
                                newsItemBodyWrapAside.Children.Add(newsItemBodyWrapAsideDateLabel);
                                TextBlock newsItemBodyWrapAsideContentLabel = new TextBlock();
                                newsItemBodyWrapAsideContentLabel.Width = 250;
                                newsItemBodyWrapAsideContentLabel.TextWrapping = TextWrapping.Wrap;
                                newsItemBodyWrapAsideContentLabel.Foreground = System.Windows.Media.Brushes.White;
                                newsItemBodyWrapAsideContentLabel.Text = content;
                                newsItemBodyWrapAside.Children.Add(newsItemBodyWrapAsideContentLabel);
                                newsItemBodyWrap.Children.Add(newsItemBodyWrapAside);
                                Image newsItemBodyWrapImg = new Image();
                                newsItemBodyWrapImg.HorizontalAlignment = HorizontalAlignment.Right;
                                newsItemBodyWrapImg.Margin = new Thickness(15);
                                newsItemBodyWrapImg.Width = 200;
                                newsItemBodyWrapImg.Height = 200;
                                newsItemBodyWrapImg.ImageFailed += SetDefaultThumbnailHandler;
                                newsItemBodyWrapImg.BeginInit();
                                newsItemBodyWrapImg.Source = new BitmapImage(new Uri(@"http://localhost:4000/api/game/thumbnail/?name=" + newsGameName));
                                newsItemBodyWrapImg.EndInit();
                                newsItemBodyWrap.Children.Add(newsItemBodyWrapImg);
                                newsItemBody.Child = newsItemBodyWrap;
                                newsItem.Children.Add(newsItemBody);
                                StackPanel newsItemFooter = new StackPanel();
                                newsItemFooter.Orientation = Orientation.Horizontal;
                                newsItemFooter.HorizontalAlignment = HorizontalAlignment.Right;
                                PackIcon newsItemFooterIcon = new PackIcon();
                                newsItemFooterIcon.Kind = PackIconKind.ThumbUp;
                                newsItemFooterIcon.Margin = new Thickness(5, 0, 5, 0);
                                TextBlock newsItemFooterLabel = new TextBlock();
                                newsItemFooterLabel.Margin = new Thickness(5, 0, 5, 0);
                                newsItemFooterLabel.Text = "0";
                                newsItemFooterIcon = new PackIcon();
                                newsItemFooterIcon.Kind = PackIconKind.ThumbUp;
                                newsItemFooterIcon.Margin = new Thickness(5, 0, 5, 0);
                                newsItemFooterLabel = new TextBlock();
                                newsItemFooterLabel.Margin = new Thickness(5, 0, 5, 0);
                                newsItemFooterLabel.Text = "0";
                                newsItemFooterLabel = new TextBlock();
                                newsItemFooterLabel.Margin = new Thickness(5, 0, 5, 0);
                                newsItemFooterLabel.Text = "0";
                                newsItem.Children.Add(newsItemFooter);
                                news.Children.Add(newsItem);
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

        public void StoreItemSelected(int index)
        {
            bool isPopular = index == 1;
            bool isHint = index == 2;
            bool isWant = index == 3;
            bool isPointsStore = index == 4;
            bool isNews = index == 5;
            bool isGamesStats = index == 6;
            if (isPopular)
            {
                OpenPopularGames();
            }
            else if (isHint)
            {
                mainControl.SelectedIndex = 28;
            }
            else if (isWant)
            {
                GetWantGames();
            }
            else if (isPointsStore)
            {
                mainControl.SelectedIndex = 34;
                GetPoints();
            }
            else if (isNews)
            {
                OpenNews();
            }
            else if (isGamesStats)
            {
                mainControl.SelectedIndex = 3;
                GetGamesStats();

                AddHistoryRecord();

            }
            ResetMenu();
        }

        public void GetWantGamesHandler (object sender, TextChangedEventArgs e)
        {
            GetWantGames();
        }

        public void GetWantGames ()
        {
            mainControl.SelectedIndex = 33;
            wantGamesList.Children.Clear();
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/games/get");
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (var reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        var objText = reader.ReadToEnd();
                        GamesListResponseInfo myobj = (GamesListResponseInfo)js.Deserialize(objText, typeof(GamesListResponseInfo));
                        string status = myobj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
                            List<GameResponseInfo> totalGames = myobj.games;
                            totalGames = totalGames.Where((GameResponseInfo game) =>
                            {
                                return false;
                            }).ToList<GameResponseInfo>();
                            int totalGamesCount = totalGames.Count;
                            bool isGamesExists = totalGamesCount >= 1;
                            if (isGamesExists)
                            {
                                wantGamesList.HorizontalAlignment = HorizontalAlignment.Left;
                            }
                            else
                            {
                                StackPanel notFound = new StackPanel();
                                notFound.Margin = new Thickness(0, 15, 0, 15);
                                TextBlock notFoundLabel = new TextBlock();
                                notFoundLabel.HorizontalAlignment = HorizontalAlignment.Center;
                                notFoundLabel.TextAlignment = TextAlignment.Center;
                                notFoundLabel.FontSize = 18;
                                notFoundLabel.Text = "ОЙ, ТУТ НИЧЕГО НЕТ";
                                notFound.Children.Add(notFoundLabel);
                                TextBlock notFoundSubLabel = new TextBlock();
                                notFoundSubLabel.HorizontalAlignment = HorizontalAlignment.Center;
                                notFoundSubLabel.TextAlignment = TextAlignment.Center;
                                notFoundSubLabel.FontSize = 18;
                                notFoundSubLabel.Text = "Ни один продукт из вашего списка желаемого не подходит под указанные фильтры.";
                                notFound.Children.Add(notFoundSubLabel);
                                wantGamesList.HorizontalAlignment = HorizontalAlignment.Center;
                                wantGamesList.Children.Add(notFound);
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

        private void OpenEditProfileHandler(object sender, RoutedEventArgs e)
        {
            OpenEditProfile();
        }

        public void OpenEditProfile()
        {
            mainControl.SelectedIndex = 2;
            AddHistoryRecord();
        }

        private void SaveUserInfoHandler(object sender, RoutedEventArgs e)
        {
            SaveUserInfo();
        }

        async public void SaveUserInfo()
        {

            string userNameBoxContent = userNameBox.Text;
            int selectedCountryIndex = userCountryBox.SelectedIndex;
            ItemCollection userCountryBoxItems = userCountryBox.Items;
            object rawSelectedUserCountryBoxItem = userCountryBoxItems[selectedCountryIndex];
            ComboBoxItem selectedUserCountryBoxItem = ((ComboBoxItem)(rawSelectedUserCountryBoxItem));
            object rawUserCountryBoxContent = selectedUserCountryBoxItem.Content;
            string userCountryBoxContent = ((string)(rawUserCountryBoxContent));
            string userAboutBoxContent = userAboutBox.Text;

            int userFriendsSettingsIndex = userFriendsSettingsSelector.SelectedIndex;
            ItemCollection userFriendsSettingsSelectorItems = userFriendsSettingsSelector.Items;
            object rawSelectedUserFriendsSettingsItem = userFriendsSettingsSelectorItems[userFriendsSettingsIndex];
            ComboBoxItem selectedUserFriendsSettingsItem = ((ComboBoxItem)(rawSelectedUserFriendsSettingsItem));
            object selectedUserFriendsSettingsItemData = selectedUserFriendsSettingsItem.DataContext;
            string userFriendsSettings = ((string)(selectedUserFriendsSettingsItemData));
            int userGamesSettingsIndex = userGamesSettingsSelector.SelectedIndex;
            ItemCollection userGamesSettingsSelectorItems = userGamesSettingsSelector.Items;
            object rawSelectedUserGamesSettingsItem = userGamesSettingsSelectorItems[userGamesSettingsIndex];
            ComboBoxItem selectedUserGamesSettingsItem = ((ComboBoxItem)(rawSelectedUserGamesSettingsItem));
            object selectedUserGamesSettingsItemData = selectedUserGamesSettingsItem.DataContext;
            string userGamesSettings = ((string)(selectedUserGamesSettingsItemData));
            int userEquipmentSettingsIndex = userEquipmentSettingsSelector.SelectedIndex;
            ItemCollection userEquipmentSettingsSelectorItems = userEquipmentSettingsSelector.Items;
            object rawSelectedUserEquipmentSettingsItem = userEquipmentSettingsSelectorItems[userEquipmentSettingsIndex];
            ComboBoxItem selectedUserEquipmentSettingsItem = ((ComboBoxItem)(rawSelectedUserEquipmentSettingsItem));
            object selectedUserEquipmentSettingsItemData = selectedUserEquipmentSettingsItem.DataContext;
            string userEquipmentSettings = ((string)(selectedUserEquipmentSettingsItemData));
            int userCommentsSettingsIndex = userCommentsSettingsSelector.SelectedIndex;
            ItemCollection userCommentsSettingsSelectorItems = userCommentsSettingsSelector.Items;
            object rawSelectedUserCommentsSettingsItem = userCommentsSettingsSelectorItems[userCommentsSettingsIndex];
            ComboBoxItem selectedUserCommentsSettingsItem = ((ComboBoxItem)(rawSelectedUserCommentsSettingsItem));
            object selectedUserCommentsSettingsItemData = selectedUserCommentsSettingsItem.DataContext;
            string userCommentsSettings = ((string)(selectedUserCommentsSettingsItemData));

            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("User-Agent", "C# App");
            MultipartFormDataContent form = new MultipartFormDataContent();
            ImageSource source = editProfileAvatarImg.Source;
            BitmapImage bitmapImage = ((BitmapImage)(source));
            byte[] imagebytearraystring = getPngFromImageControl(bitmapImage);
            form.Add(new ByteArrayContent(imagebytearraystring, 0, imagebytearraystring.Count()), "profile_pic", "mock.png");

            // string url = @"http://localhost:4000/api/user/edit/?id=" + currentUserId + "&name=" + userNameBoxContent + "&country=" + userCountryBoxContent + "&about=" + userAboutBoxContent + "&friends=" + userFriendsSettings + "&games=" + userGamesSettings + "&equipment=" + userEquipmentSettings + "&comments=" + userCommentsSettings;
            string url = @"http://localhost:4000/api/user/edit/?id=" + currentUserId + "&name=" + userNameBoxContent + "&country=" + userCountryBoxContent + "&about=" + userAboutBoxContent + "&friends=" + userFriendsSettings + "&games=" + userGamesSettings + "&equipment=" + userEquipmentSettings + "&comments=" + userCommentsSettings;

            HttpResponseMessage response = httpClient.PostAsync(url, form).Result;
            httpClient.Dispose();
            string sd = response.Content.ReadAsStringAsync().Result;
            /**/
            JavaScriptSerializer js = new JavaScriptSerializer();
            RegisterResponseInfo myobj = (RegisterResponseInfo)js.Deserialize(sd, typeof(RegisterResponseInfo));
            string status = myobj.status;
            bool isOkStatus = status == "OK";
            // bool isOkStatus = true;
            if (isOkStatus)
            {
                /*GetUser(currentUserId);
                GetUserInfo(currentUserId, true);
                GetEditInfo();*/
                MessageBox.Show("Профиль был обновлен", "Внимание");
            }
            else
            {
                MessageBox.Show("Не удается обновить профиль", "Ошибка");
            }

            /*HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/user/edit/?id=" + currentUserId + "&name=" + userNameBoxContent + "&country=" + userCountryBoxContent + "&about=" + userAboutBoxContent);
            webRequest.Method = "POST";
            webRequest.ContentType = "multipart/form-data";
            webRequest.UserAgent = ".NET Framework Test Client";
            using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
            {
                using (var reader = new StreamReader(webResponse.GetResponseStream()))
                {
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    var objText = reader.ReadToEnd();

                    RegisterResponseInfo myobj = (RegisterResponseInfo)js.Deserialize(objText, typeof(RegisterResponseInfo));

                    string status = myobj.status;
                    bool isOkStatus = status == "OK";
                    if (isOkStatus)
                    {
                        MessageBox.Show("Профиль был обновлен", "Внимание");
                        GetUser(currentUserId);
                        GetUserInfo(currentUserId, true);
                        GetEditInfo();
                    }
                    else
                    {
                        MessageBox.Show("Не удается редактировать профиль", "Ошибка");
                    }
                }
            }*/

            Environment.SpecialFolder localApplicationDataFolder = Environment.SpecialFolder.LocalApplicationData;
            string localApplicationDataFolderPath = Environment.GetFolderPath(localApplicationDataFolder);
            string saveDataFilePath = localApplicationDataFolderPath + @"\OfficeWare\GameManager\" + currentUserId + @"\save-data.txt";
            js = new JavaScriptSerializer();
            string saveDataFileContent = File.ReadAllText(saveDataFilePath);
            SavedContent loadedContent = js.Deserialize<SavedContent>(saveDataFileContent);
            List<Game> currentGames = loadedContent.games;
            List<FriendSettings> currentFriends = loadedContent.friends;
            Settings updatedSettings = loadedContent.settings;
            List<string> currentCollections = loadedContent.collections;
            Notifications currentNotifications = loadedContent.notifications; 
            foreach (StackPanel profileTheme in profileThemes.Children)
            {
                bool isSelectedTheme = ((TextBlock)(profileTheme.Children[1])).Foreground == System.Windows.Media.Brushes.Blue;
                if (isSelectedTheme)
                {

                    /*object rawThemeName = editProfileThemeName.DataContext;
                    string themeName = rawThemeName.ToString();*/

                    object rawThemeName = profileTheme.DataContext;
                    string themeName = rawThemeName.ToString();

                    updatedSettings.profileTheme = themeName;
                    string savedContent = js.Serialize(new SavedContent
                    {
                        games = currentGames,
                        friends = currentFriends,
                        settings = updatedSettings,
                        collections = currentCollections,
                        notifications = currentNotifications
                    });
                    File.WriteAllText(saveDataFilePath, savedContent);
                    break;
                }
            }

            GetUser(currentUserId);
            GetUserInfo(currentUserId, true);
            GetEditInfo();

        }

        public byte[] getPngFromImageControl(BitmapImage imageC)
        {
            MemoryStream memStream = new MemoryStream();
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(imageC));
            encoder.Save(memStream);
            return memStream.ToArray();
        }

        public async void ListenSockets()
        {
            try
            {
                /*
                 * glitch выдает ошибку с сокетами
                 * client = new SocketIO("http://localhost:4000/");
                */

                client = new SocketIO("http://localhost:4000/");
                // client = new SocketIO("https://digitaldistributtionservice.herokuapp.com/");

                client.OnConnected += async (sender, e) =>
                {
                    Debugger.Log(0, "debug", "client socket conntected");
                    await client.EmitAsync("user_is_online", currentUserId);
                };
                client.On("friend_is_played", response =>
                {
                    var rawResult = response.GetValue<string>();
                    string[] result = rawResult.Split(new char[] { '|' });
                    string userId = result[0];
                    string gameName = result[1];
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
                                        HttpWebRequest innerWebRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/users/get/?id=" + userId);
                                        innerWebRequest.Method = "GET";
                                        innerWebRequest.UserAgent = ".NET Framework Test Client";
                                        using (HttpWebResponse innerWebResponse = (HttpWebResponse)innerWebRequest.GetResponse())
                                        {
                                            using (var innerReader = new StreamReader(innerWebResponse.GetResponseStream()))
                                            {
                                                js = new JavaScriptSerializer();
                                                objText = innerReader.ReadToEnd();
                                                UserResponseInfo myInnerObj = (UserResponseInfo)js.Deserialize(objText, typeof(UserResponseInfo));
                                                status = myInnerObj.status;
                                                isOkStatus = status == "OK";
                                                if (isOkStatus)
                                                {
                                                    User sender = myInnerObj.user;
                                                    string senderName = sender.name;
                                                    Environment.SpecialFolder localApplicationDataFolder = Environment.SpecialFolder.LocalApplicationData;
                                                    string localApplicationDataFolderPath = Environment.GetFolderPath(localApplicationDataFolder);
                                                    string saveDataFilePath = localApplicationDataFolderPath + @"\OfficeWare\GameManager\" + currentUserId + @"\save-data.txt";
                                                    js = new JavaScriptSerializer();
                                                    string saveDataFileContent = File.ReadAllText(saveDataFilePath);
                                                    SavedContent loadedContent = js.Deserialize<SavedContent>(saveDataFileContent);
                                                    List<Game> currentGames = loadedContent.games;
                                                    List<FriendSettings> updatedFriends = loadedContent.friends;
                                                    List<FriendSettings> cachedFriends = updatedFriends.Where<FriendSettings>((FriendSettings friend) =>
                                                    {
                                                        return friend.id == userId;
                                                    }).ToList();
                                                    int countCachedFriends = cachedFriends.Count;
                                                    bool isCachedFriendsExists = countCachedFriends >= 1;
                                                    if (isCachedFriendsExists)
                                                    {
                                                        FriendSettings cachedFriend = cachedFriends[0];
                                                        bool isNotificationEnabled = cachedFriend.isFriendPlayedNotification;
                                                        if (isNotificationEnabled)
                                                        {
                                                            this.Dispatcher.Invoke(async () =>
                                                            {
                                                                Popup friendNotification = new Popup();
                                                                friendNotification.Placement = PlacementMode.Custom;
                                                                friendNotification.CustomPopupPlacementCallback = new CustomPopupPlacementCallback(FriendRequestPlacementHandler);
                                                                friendNotification.PlacementTarget = this;
                                                                friendNotification.Width = 225;
                                                                friendNotification.Height = 275;
                                                                StackPanel friendNotificationBody = new StackPanel();
                                                                friendNotificationBody.Background = friendRequestBackground;
                                                                Image friendNotificationBodySenderAvatar = new Image();
                                                                friendNotificationBodySenderAvatar.Width = 100;
                                                                friendNotificationBodySenderAvatar.Height = 100;
                                                                friendNotificationBodySenderAvatar.BeginInit();
                                                                Uri friendNotificationBodySenderAvatarUri = new Uri("https://cdn2.iconfinder.com/data/icons/ios-7-icons/50/user_male-128.png");
                                                                BitmapImage friendNotificationBodySenderAvatarImg = new BitmapImage(friendNotificationBodySenderAvatarUri);
                                                                friendNotificationBodySenderAvatar.Source = friendNotificationBodySenderAvatarImg;
                                                                friendNotificationBodySenderAvatar.EndInit();
                                                                friendNotificationBody.Children.Add(friendNotificationBodySenderAvatar);
                                                                TextBlock friendNotificationBodySenderLoginLabel = new TextBlock();
                                                                friendNotificationBodySenderLoginLabel.Margin = new Thickness(10);
                                                                string newLine = Environment.NewLine;
                                                                friendNotificationBodySenderLoginLabel.Text = "Пользователь " + senderName + newLine + " играет в " + newLine + gameName;
                                                                friendNotificationBody.Children.Add(friendNotificationBodySenderLoginLabel);
                                                                friendNotification.Child = friendNotificationBody;
                                                                friendRequests.Children.Add(friendNotification);
                                                                friendNotification.IsOpen = true;
                                                                friendNotification.StaysOpen = false;
                                                                friendNotification.PopupAnimation = PopupAnimation.Fade;
                                                                friendNotification.AllowsTransparency = true;
                                                                DispatcherTimer timer = new DispatcherTimer();
                                                                timer.Interval = TimeSpan.FromSeconds(3);
                                                                timer.Tick += delegate
                                                                {
                                                                    friendNotification.IsOpen = false;
                                                                    timer.Stop();
                                                                };
                                                                timer.Start();
                                                                friendNotifications.Children.Add(friendNotification);
                                                            });
                                                            // MessageBox.Show("Пользователь " + senderName + " играет в " + gameName, "Внимание");
                                                        }
                                                        bool isSoundEnabled = cachedFriend.isFriendPlayedSound;
                                                        if (isSoundEnabled)
                                                        {
                                                            Application.Current.Dispatcher.Invoke(() =>
                                                            {
                                                                mainAudio.LoadedBehavior = MediaState.Play;
                                                                mainAudio.Source = new Uri(@"C:\wpf_projects\GamaManager\GamaManager\Sounds\notification.wav");
                                                            });
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
                });

                client.On("friend_is_online", response =>
                {
                    var result = response.GetValue<string>();
                    Debugger.Log(0, "debug", Environment.NewLine + "friend is online: " + result + Environment.NewLine);
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
                                        string userId = joint.user;
                                        bool isMyFriend = userId == currentUserId;
                                        return isMyFriend;
                                    }).ToList<Friend>();
                                    List<string> friendsIds = new List<string>();
                                    foreach (Friend myFriend in myFriends)
                                    {
                                        string friendId = myFriend.friend;
                                        friendsIds.Add(friendId);
                                    }
                                    bool isMyFriendOnline = friendsIds.Contains(result);
                                    Debugger.Log(0, "debug", "myFriends: " + myFriends.Count.ToString());
                                    Debugger.Log(0, "debug", "friendsIds: " + String.Join("|", friendsIds));
                                    Debugger.Log(0, "debug", "isMyFriendOnline: " + isMyFriendOnline);
                                    if (isMyFriendOnline)
                                    {
                                        HttpWebRequest innerWebRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/users/get/?id=" + result);
                                        innerWebRequest.Method = "GET";
                                        innerWebRequest.UserAgent = ".NET Framework Test Client";
                                        using (HttpWebResponse innerWebResponse = (HttpWebResponse)innerWebRequest.GetResponse())
                                        {
                                            using (var innerReader = new StreamReader(innerWebResponse.GetResponseStream()))
                                            {
                                                js = new JavaScriptSerializer();
                                                objText = innerReader.ReadToEnd();

                                                UserResponseInfo myInnerObj = (UserResponseInfo)js.Deserialize(objText, typeof(UserResponseInfo));

                                                status = myInnerObj.status;
                                                isOkStatus = status == "OK";
                                                if (isOkStatus)
                                                {
                                                    User sender = myInnerObj.user;
                                                    string senderName = sender.name;

                                                    Environment.SpecialFolder localApplicationDataFolder = Environment.SpecialFolder.LocalApplicationData;
                                                    string localApplicationDataFolderPath = Environment.GetFolderPath(localApplicationDataFolder);
                                                    string saveDataFilePath = localApplicationDataFolderPath + @"\OfficeWare\GameManager\" + currentUserId + @"\save-data.txt";
                                                    js = new JavaScriptSerializer();
                                                    string saveDataFileContent = File.ReadAllText(saveDataFilePath);
                                                    SavedContent loadedContent = js.Deserialize<SavedContent>(saveDataFileContent);
                                                    List<Game> currentGames = loadedContent.games;
                                                    List<FriendSettings> updatedFriends = loadedContent.friends;
                                                    List<FriendSettings> cachedFriends = updatedFriends.Where<FriendSettings>((FriendSettings friend) =>
                                                    {
                                                        return friend.id == result;
                                                    }).ToList();
                                                    int countCachedFriends = cachedFriends.Count;
                                                    bool isCachedFriendsExists = countCachedFriends >= 1;
                                                    if (isCachedFriendsExists)
                                                    {
                                                        FriendSettings cachedFriend = cachedFriends[0];
                                                        bool isNotificationEnabled = cachedFriend.isFriendOnlineNotification;
                                                        if (isNotificationEnabled)
                                                        {

                                                            this.Dispatcher.Invoke(async () =>
                                                            {
                                                                Popup friendNotification = new Popup();
                                                                friendNotification.Placement = PlacementMode.Custom;
                                                                friendNotification.CustomPopupPlacementCallback = new CustomPopupPlacementCallback(FriendRequestPlacementHandler);
                                                                friendNotification.PlacementTarget = this;
                                                                friendNotification.Width = 225;
                                                                friendNotification.Height = 275;
                                                                StackPanel friendNotificationBody = new StackPanel();
                                                                friendNotificationBody.Background = friendRequestBackground;
                                                                Image friendNotificationBodySenderAvatar = new Image();
                                                                friendNotificationBodySenderAvatar.Width = 100;
                                                                friendNotificationBodySenderAvatar.Height = 100;
                                                                friendNotificationBodySenderAvatar.BeginInit();
                                                                Uri friendNotificationBodySenderAvatarUri = new Uri("https://cdn2.iconfinder.com/data/icons/ios-7-icons/50/user_male-128.png");
                                                                BitmapImage friendNotificationBodySenderAvatarImg = new BitmapImage(friendNotificationBodySenderAvatarUri);
                                                                friendNotificationBodySenderAvatar.Source = friendNotificationBodySenderAvatarImg;
                                                                friendNotificationBodySenderAvatar.EndInit();
                                                                friendNotificationBody.Children.Add(friendNotificationBodySenderAvatar);
                                                                TextBlock friendNotificationBodySenderLoginLabel = new TextBlock();
                                                                friendNotificationBodySenderLoginLabel.Margin = new Thickness(10);
                                                                friendNotificationBodySenderLoginLabel.Text = "Пользователь " + Environment.NewLine + senderName + Environment.NewLine + " теперь в сети";
                                                                friendNotificationBody.Children.Add(friendNotificationBodySenderLoginLabel);
                                                                friendNotification.Child = friendNotificationBody;
                                                                friendRequests.Children.Add(friendNotification);
                                                                friendNotification.IsOpen = true;
                                                                friendNotification.StaysOpen = false;
                                                                friendNotification.PopupAnimation = PopupAnimation.Fade;
                                                                friendNotification.AllowsTransparency = true;
                                                                DispatcherTimer timer = new DispatcherTimer();
                                                                timer.Interval = TimeSpan.FromSeconds(3);
                                                                timer.Tick += delegate
                                                                {
                                                                    friendNotification.IsOpen = false;
                                                                    timer.Stop();
                                                                };
                                                                timer.Start();
                                                                friendNotifications.Children.Add(friendNotification);
                                                            });
                                                        }
                                                        bool isSoundEnabled = cachedFriend.isFriendOnlineSound;
                                                        if (isSoundEnabled)
                                                        {
                                                            Application.Current.Dispatcher.Invoke(() =>
                                                            {
                                                                mainAudio.LoadedBehavior = MediaState.Play;
                                                                mainAudio.Source = new Uri(@"C:\wpf_projects\GamaManager\GamaManager\Sounds\notification.wav");
                                                            });
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
                    // здесь GetFriends();
                });
                client.On("friend_send_msg", async response =>
                {
                    var rawResult = response.GetValue<string>();
                    string[] result = rawResult.Split(new char[] { '|' });
                    string userId = result[0];
                    string msg = result[1];
                    string chatId = result[2];
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
                                        string currentFriendId = userId;
                                        bool isCurrentChat = currentFriendId == userId;
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
                                                            js = new JavaScriptSerializer();
                                                            objText = innerReader.ReadToEnd();

                                                            UserResponseInfo myInnerObj = (UserResponseInfo)js.Deserialize(objText, typeof(UserResponseInfo));

                                                            status = myobj.status;
                                                            isOkStatus = status == "OK";
                                                            if (isOkStatus)
                                                            {
                                                                User friend = myInnerObj.user;
                                                                string senderName = friend.name;
                                                                Application app = Application.Current;
                                                                WindowCollection windows = app.Windows;
                                                                IEnumerable<Window> myWindows = windows.OfType<Window>();
                                                                int countChatWindows = myWindows.Count(window =>
                                                                {
                                                                    string windowTitle = window.Title;
                                                                    bool isChatWindow = windowTitle == "Чат";
                                                                    return isChatWindow;
                                                                });
                                                                bool isNotOpenedChatWindows = countChatWindows <= 0;
                                                                if (isNotOpenedChatWindows)
                                                                {

                                                                    Environment.SpecialFolder localApplicationDataFolder = Environment.SpecialFolder.LocalApplicationData;
                                                                    string localApplicationDataFolderPath = Environment.GetFolderPath(localApplicationDataFolder);
                                                                    string saveDataFilePath = localApplicationDataFolderPath + @"\OfficeWare\GameManager\" + currentUserId + @"\save-data.txt";
                                                                    js = new JavaScriptSerializer();
                                                                    string saveDataFileContent = File.ReadAllText(saveDataFilePath);
                                                                    SavedContent loadedContent = js.Deserialize<SavedContent>(saveDataFileContent);
                                                                    List<Game> currentGames = loadedContent.games;
                                                                    List<FriendSettings> updatedFriends = loadedContent.friends;
                                                                    List<FriendSettings> cachedFriends = updatedFriends.Where<FriendSettings>((FriendSettings localFriend) =>
                                                                    {
                                                                        return localFriend.id == userId;
                                                                    }).ToList();
                                                                    int countCachedFriends = cachedFriends.Count;
                                                                    bool isCachedFriendsExists = countCachedFriends >= 1;
                                                                    if (isCachedFriendsExists)
                                                                    {
                                                                        FriendSettings cachedFriend = cachedFriends[0];
                                                                        bool isNotificationEnabled = cachedFriend.isFriendSendMsgNotification;
                                                                        if (isNotificationEnabled)
                                                                        {
                                                                            Application.Current.Dispatcher.Invoke(async () =>
                                                                            {
                                                                                if (chatId == currentUserId && friendsIds.Contains(userId))
                                                                                {
                                                                                    Popup friendNotification = new Popup();
                                                                                    friendNotification.DataContext = friend._id;
                                                                                    friendNotification.MouseLeftButtonUp += OpenChatFromPopupHandler;
                                                                                    friendNotification.Placement = PlacementMode.Custom;
                                                                                    friendNotification.CustomPopupPlacementCallback = new CustomPopupPlacementCallback(FriendRequestPlacementHandler);
                                                                                    friendNotification.PlacementTarget = this;
                                                                                    friendNotification.Width = 225;
                                                                                    friendNotification.Height = 275;
                                                                                    StackPanel friendNotificationBody = new StackPanel();
                                                                                    friendNotificationBody.Background = friendRequestBackground;
                                                                                    Image friendNotificationBodySenderAvatar = new Image();
                                                                                    friendNotificationBodySenderAvatar.Width = 100;
                                                                                    friendNotificationBodySenderAvatar.Height = 100;
                                                                                    friendNotificationBodySenderAvatar.BeginInit();
                                                                                    Uri friendNotificationBodySenderAvatarUri = new Uri("https://cdn2.iconfinder.com/data/icons/ios-7-icons/50/user_male-128.png");
                                                                                    BitmapImage friendNotificationBodySenderAvatarImg = new BitmapImage(friendNotificationBodySenderAvatarUri);
                                                                                    friendNotificationBodySenderAvatar.Source = friendNotificationBodySenderAvatarImg;
                                                                                    friendNotificationBodySenderAvatar.EndInit();
                                                                                    friendNotificationBody.Children.Add(friendNotificationBodySenderAvatar);
                                                                                    TextBlock friendNotificationBodySenderLoginLabel = new TextBlock();
                                                                                    friendNotificationBodySenderLoginLabel.Margin = new Thickness(10);
                                                                                    friendNotificationBodySenderLoginLabel.Text = "Пользователь " + Environment.NewLine + senderName + Environment.NewLine + " оставил вам сообщение";
                                                                                    friendNotificationBody.Children.Add(friendNotificationBodySenderLoginLabel);
                                                                                    friendNotification.Child = friendNotificationBody;
                                                                                    friendRequests.Children.Add(friendNotification);
                                                                                    friendNotification.IsOpen = true;
                                                                                    friendNotification.StaysOpen = false;
                                                                                    friendNotification.PopupAnimation = PopupAnimation.Fade;
                                                                                    friendNotification.AllowsTransparency = true;
                                                                                    DispatcherTimer timer = new DispatcherTimer();
                                                                                    timer.Interval = TimeSpan.FromSeconds(3);
                                                                                    timer.Tick += delegate
                                                                                    {
                                                                                        friendNotification.IsOpen = false;
                                                                                        timer.Stop();
                                                                                    };
                                                                                    timer.Start();
                                                                                    friendNotifications.Children.Add(friendNotification);
                                                                                }
                                                                            });
                                                                        }
                                                                        bool isSoundEnabled = cachedFriend.isFriendSendMsgSound;
                                                                        if (isSoundEnabled)
                                                                        {
                                                                            Application.Current.Dispatcher.Invoke(() =>
                                                                            {
                                                                                mainAudio.LoadedBehavior = MediaState.Play;
                                                                                mainAudio.Source = new Uri(@"C:\wpf_projects\GamaManager\GamaManager\Sounds\notification.wav");
                                                                            });
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
                                            });
                                        }
                                    }

                                    {
                                        HttpWebRequest innerWebRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/talks/all");
                                        innerWebRequest.Method = "GET";
                                        innerWebRequest.UserAgent = ".NET Framework Test Client";
                                        using (HttpWebResponse innerWebResponse = (HttpWebResponse)innerWebRequest.GetResponse())
                                        {
                                            using (var innerReader = new StreamReader(innerWebResponse.GetResponseStream()))
                                            {
                                                js = new JavaScriptSerializer();
                                                objText = innerReader.ReadToEnd();
                                                TalksResponseInfo myInnerObj = (TalksResponseInfo)js.Deserialize(objText, typeof(TalksResponseInfo));
                                                status = myobj.status;
                                                isOkStatus = status == "OK";
                                                if (isOkStatus)
                                                {
                                                    List<Talk> totalTalks = myInnerObj.talks;
                                                    HttpWebRequest nestedWebRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/talks/relations/all");
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
                                                                List<TalkRelation> myTalks = relations.Where<TalkRelation>((TalkRelation relation) =>
                                                                {
                                                                    string relationTalk = relation.talk;
                                                                    string relationUser = relation.user;
                                                                    bool isCurrentUser = relationUser == currentUserId;
                                                                    return isCurrentUser;
                                                                }).ToList<TalkRelation>();
                                                                int countMyTalks = myTalks.Count;
                                                                bool isHaveTalks = countMyTalks >= 1;
                                                                if (isHaveTalks)
                                                                {
                                                                    Debugger.Log(0, "debug", Environment.NewLine + "Пришло сообщение проверяю беседы " + isHaveTalks.ToString() + Environment.NewLine);
                                                                    /*foreach (Talk talk in totalTalks)
                                                                    {
                                                                        string talkId = talk._id;
                                                                        bool isMyTalk = false;
                                                                        List<TalkRelation> results = relations.Where<TalkRelation>((TalkRelation relation) =>
                                                                        {
                                                                            string relationTalk = relation.talk;
                                                                            string relationUser = relation.user;
                                                                            bool isCurrentTalk = relationTalk == talkId;
                                                                            bool isCurrentUser = relationUser == currentUserId;
                                                                            bool isLocalMyTalk = isCurrentUser && isCurrentTalk;
                                                                            return isLocalMyTalk;
                                                                        }).ToList<TalkRelation>();
                                                                        int countResults = results.Count;
                                                                        isMyTalk = countResults >= 1;
                                                                        if (isMyTalk)
                                                                        {

                                                                        }
    {                                                                    }*/
                                                                    List<string> myTalkIds = new List<string>();
                                                                    foreach (TalkRelation myTalkRelation in myTalks)
                                                                    {
                                                                        string talkId = myTalkRelation.talk;
                                                                        myTalkIds.Add(talkId);
                                                                    }
                                                                    bool isMsgForMe = myTalkIds.Contains(chatId);
                                                                    if (isMsgForMe)
                                                                    {
                                                                        Debugger.Log(0, "debug", Environment.NewLine + "Пришло сообщение из беседы" + Environment.NewLine);
                                                                        Application.Current.Dispatcher.Invoke(async () =>
                                                                        {

                                                                            HttpWebRequest innserNestedWebRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/talks/get/?id=" + chatId);
                                                                            innserNestedWebRequest.Method = "GET";
                                                                            innserNestedWebRequest.UserAgent = ".NET Framework Test Client";
                                                                            using (HttpWebResponse innserNestedWebResponse = (HttpWebResponse)innserNestedWebRequest.GetResponse())
                                                                            {
                                                                                using (var innserNestedReader = new StreamReader(innserNestedWebResponse.GetResponseStream()))
                                                                                {
                                                                                    js = new JavaScriptSerializer();
                                                                                    objText = innserNestedReader.ReadToEnd();
                                                                                    TalkResponseInfo myInnserNestedObj = (TalkResponseInfo)js.Deserialize(objText, typeof(TalkResponseInfo));
                                                                                    status = myInnserNestedObj.status;
                                                                                    isOkStatus = status == "OK";
                                                                                    if (isOkStatus)
                                                                                    {
                                                                                        HttpWebRequest userWebRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/users/get/?id=" + userId);
                                                                                        userWebRequest.Method = "GET";
                                                                                        userWebRequest.UserAgent = ".NET Framework Test Client";
                                                                                        using (HttpWebResponse userWebResponse = (HttpWebResponse)userWebRequest.GetResponse())
                                                                                        {
                                                                                            using (var userReader = new StreamReader(userWebResponse.GetResponseStream()))
                                                                                            {
                                                                                                js = new JavaScriptSerializer();
                                                                                                objText = userReader.ReadToEnd();
                                                                                                UserResponseInfo myUserObj = (UserResponseInfo)js.Deserialize(objText, typeof(UserResponseInfo));
                                                                                                status = myUserObj.status;
                                                                                                isOkStatus = status == "OK";
                                                                                                if (isOkStatus)
                                                                                                {
                                                                                                    Application localApp = Application.Current;
                                                                                                    WindowCollection localWindows = localApp.Windows;
                                                                                                    IEnumerable<Window> myWindows = localWindows.OfType<Window>();
                                                                                                    int countTalkWindows = myWindows.Count(window =>
                                                                                                    {
                                                                                                        string windowTitle = window.Title;
                                                                                                        bool isTalkWindow = windowTitle == "Беседа";
                                                                                                        return isTalkWindow;
                                                                                                    });
                                                                                                    bool isNotOpenedTalkWindows = countTalkWindows <= 0;
                                                                                                    bool isMock = true;
                                                                                                    if (isNotOpenedTalkWindows || isMock)
                                                                                                    {   User user = myUserObj.user;
                                                                                                        string userName = user.name;
                                                                                                        Talk talk = myInnserNestedObj.talk;
                                                                                                        string talkTitle = talk.title;
                                                                                                        Popup talkNotification = new Popup();
                                                                                                        talkNotification.DataContext = chatId;
                                                                                                        talkNotification.MouseLeftButtonUp += OpenTalkFromPopupHandler;
                                                                                                        talkNotification.Placement = PlacementMode.Custom;
                                                                                                        talkNotification.CustomPopupPlacementCallback = new CustomPopupPlacementCallback(FriendRequestPlacementHandler);
                                                                                                        talkNotification.PlacementTarget = this;
                                                                                                        talkNotification.Width = 225;
                                                                                                        talkNotification.Height = 275;
                                                                                                        StackPanel talkNotificationBody = new StackPanel();
                                                                                                        talkNotificationBody.Background = friendRequestBackground;
                                                                                                        Image talkNotificationBodySenderAvatar = new Image();
                                                                                                        talkNotificationBodySenderAvatar.Width = 100;
                                                                                                        talkNotificationBodySenderAvatar.Height = 100;
                                                                                                        talkNotificationBodySenderAvatar.BeginInit();
                                                                                                        Uri talkNotificationBodySenderAvatarUri = new Uri("https://cdn2.iconfinder.com/data/icons/ios-7-icons/50/user_male-128.png");
                                                                                                        BitmapImage talkNotificationBodySenderAvatarImg = new BitmapImage(talkNotificationBodySenderAvatarUri);
                                                                                                        talkNotificationBodySenderAvatar.Source = talkNotificationBodySenderAvatarImg;
                                                                                                        talkNotificationBodySenderAvatar.EndInit();
                                                                                                        talkNotificationBody.Children.Add(talkNotificationBodySenderAvatar);
                                                                                                        TextBlock talkNotificationBodySenderLoginLabel = new TextBlock();
                                                                                                        talkNotificationBodySenderLoginLabel.Margin = new Thickness(10);
                                                                                                        talkNotificationBodySenderLoginLabel.Text = userName + " в" + Environment.NewLine + "беседе " + talkTitle + Environment.NewLine + "оставил вам сообщение";
                                                                                                        talkNotificationBody.Children.Add(talkNotificationBodySenderLoginLabel);
                                                                                                        talkNotification.Child = talkNotificationBody;
                                                                                                        friendRequests.Children.Add(talkNotification);
                                                                                                        talkNotification.IsOpen = true;
                                                                                                        talkNotification.StaysOpen = false;
                                                                                                        talkNotification.PopupAnimation = PopupAnimation.Fade;
                                                                                                        talkNotification.AllowsTransparency = true;
                                                                                                        DispatcherTimer timer = new DispatcherTimer();
                                                                                                        timer.Interval = TimeSpan.FromSeconds(3);
                                                                                                        timer.Tick += delegate
                                                                                                        {
                                                                                                            talkNotification.IsOpen = false;
                                                                                                            timer.Stop();
                                                                                                        };
                                                                                                        timer.Start();
                                                                                                        talkNotifications.Children.Add(talkNotification);

                                                                                                        mainAudio.LoadedBehavior = MediaState.Play;
                                                                                                        mainAudio.Source = new Uri(@"C:\wpf_projects\GamaManager\GamaManager\Sounds\notification.wav");
                                                                                                    
                                                                                                    }

                                                                                                }
                                                                                            }
                                                                                        }
                                                                                    }
                                                                                }
                                                                            }
                                                                        });
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
                        }
                    }
                    catch (System.Net.WebException)
                    {
                        MessageBox.Show("Не удается подключиться к серверу", "Ошибка");
                        this.Close();
                    }
                });
                client.On("user_receive_friend_request", async response =>
                {
                    var rawResult = response.GetValue<string>();
                    string[] result = rawResult.Split(new char[] { '|' });
                    string friendId = result[0];
                    string userId = result[1];
                    bool isRequestForMe = userId == currentUserId;
                    if (isRequestForMe)
                    {
                        Application.Current.Dispatcher.Invoke(() => GetFriendRequests());
                    }
                });
                client.On("user_send_msg_to_my_topic", async response =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        var rawResult = response.GetValue<string>();
                        string[] result = rawResult.Split(new char[] { '|' });
                        string forumId = result[0];
                        string topicId = result[1];
                        string userId = result[2];
                        bool isOtherSender = userId != currentUserId;
                        if (isOtherSender)
                        {
                            int selectedWindowIndex = mainControl.SelectedIndex;
                            bool isForumsWindow = selectedWindowIndex == 6;
                            bool isForumTopicsWindow = selectedWindowIndex == 7;
                            bool isForumTopicMsgsWindow = selectedWindowIndex == 8;
                            if (isForumsWindow)
                            {
                                string keywords = forumsKeywordsBox.Text;
                                GetForums(keywords);
                            }
                            else if (isForumTopicsWindow)
                            {
                                SelectForum(forumId);
                            }
                            else if (isForumTopicMsgsWindow)
                            {
                                SelectTopic(topicId);
                            }
                        }
                    });
                });
                client.On("user_receive_group_request", async response =>
                {
                    var rawResult = response.GetValue<string>();
                    string[] result = rawResult.Split(new char[] { '|' });
                    string groupId = result[0];
                    string userId = result[1];
                    bool isRequestForMe = userId == currentUserId;
                    if (isRequestForMe)
                    {
                        Application.Current.Dispatcher.Invoke(() => GetGroupRequests());
                    }
                });
                client.On("user_receive_comment", async response =>
                {
                    var rawResult = response.GetValue<string>();
                    string[] result = rawResult.Split(new char[] { '|' });
                    string userId = result[0];
                    string profileId = result[0];
                    bool isRequestForMe = profileId == cachedUserProfileId && userId != currentUserId;
                    Debugger.Log(0, "debug", Environment.NewLine + "profileId: " + profileId + ", userId: " + userId + ", cachedUserProfileId: " + cachedUserProfileId + Environment.NewLine);
                    if (isRequestForMe)
                    {
                        Application.Current.Dispatcher.Invoke(() => GetComments(profileId));
                    }
                });
                await client.ConnectAsync();
            }
            catch (System.Net.WebSockets.WebSocketException)
            {
                Debugger.Log(0, "debug", "Ошибка сокетов");
                await client.ConnectAsync();
            }
        }

        public void OnAnimationCompleted(object sender, EventArgs e)
        {
            Storyboard storyboard = ((Storyboard)(sender));
        }

        private void ClientClosedHandler(object sender, EventArgs e)
        {
            ClientClosed();
        }

        public void ClientClosed()
        {
            DecreaseUserToStats();

            // SetUserStatus("offline");
            UpdateUserStatus("offline");

            client.EmitAsync("user_is_toggle_status", "offline");
        }

        public void DecreaseUserToStats()
        {
            try {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/users/stats/decrease");
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (var reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        var objText = reader.ReadToEnd();

                        RegisterResponseInfo myobj = (RegisterResponseInfo)js.Deserialize(objText, typeof(RegisterResponseInfo));

                        string status = myobj.status;
                        bool isErrorStatus = status == "Error";
                        if (isErrorStatus)
                        {
                            MessageBox.Show("Не удается подключиться к серверу", "Ошибка");
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

        private void OpenSystemInfoDialogHandler(object sender, RoutedEventArgs e)
        {
            OpenSystemInfoDialog();
        }

        public void OpenSystemInfoDialog()
        {
            Dialogs.SystemInfoDialog dialog = new Dialogs.SystemInfoDialog();
            dialog.Show();
        }

        private void ToggleWindowHandler(object sender, SelectionChangedEventArgs e)
        {
            ToggleWindow();
        }

        public void ToggleWindow()
        {
            if (isAppInit)
            {
                int selectedWindowIndex = mainControl.SelectedIndex;

                bool isProfileWindow = selectedWindowIndex == 1;
                if (isProfileWindow || true)
                {
                    object mainControlData = mainControl.DataContext;
                    string userId = ((string)(mainControlData));
                    bool isLocalUser = userId == currentUserId;
                    GetUserInfo(userId, isLocalUser);
                }
            }
        }

        private void BackForHistoryHandler(object sender, MouseButtonEventArgs e)
        {
            BackForHistory();
        }

        public void AddIllustationHandler (object sender, RoutedEventArgs e)
        {
            AddIllustation();
        }

        public void AddIllustation ()
        {
            try
            {
                string illustrationNameBoxContent = illustrationNameBox.Text;
                string illustrationDescBoxContent = illustrationDescBox.Text;
                object rawIsChecked = drmBox.IsChecked;
                bool isChecked = ((bool)(rawIsChecked));
                bool isDrm = false;
                string rawIsDrm = "false";
                if (isChecked)
                {
                    isDrm = true;
                    rawIsDrm = "true";
                }
                string url = "http://localhost:4000/api/illustrations/add/?id=" + currentUserId + @"&title=" + illustrationNameBoxContent + @"&desc=" + illustrationDescBoxContent + @"&drm=" + rawIsDrm + @"&ext=" + manualAttachmentExt;
                HttpClient httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("User-Agent", "C# App");
                MultipartFormDataContent form = new MultipartFormDataContent();
                byte[] imagebytearraystring = manualAttachment;
                form.Add(new ByteArrayContent(imagebytearraystring, 0, imagebytearraystring.Count()), "profile_pic", "mock.png");
                HttpResponseMessage response = httpClient.PostAsync(url, form).Result;
                httpClient.Dispose();

                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/users/points/increase/?id=" + currentUserId);
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (var reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        var objText = reader.ReadToEnd();
                        UserResponseInfo myobj = (UserResponseInfo)js.Deserialize(objText, typeof(UserResponseInfo));
                        string status = myobj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
                            mainControl.SelectedIndex = 20;
                            GetCommunityInfo();
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

        public void BackForHistory()
        {
            int countHistoryRecords = history.Count;
            bool isBackForHistoryRecords = countHistoryRecords >= 2;
            if (isBackForHistoryRecords)
            {
                bool isCanMoveCursor = historyCursor >= 1;
                if (isCanMoveCursor)
                {
                    historyCursor--;
                    int windowIndex = history[historyCursor];
                    mainControl.SelectedIndex = windowIndex;
                    bool isFirstRecord = historyCursor <= 0;
                    arrowForwardBtn.Foreground = enabledColor;
                    if (isFirstRecord)
                    {
                        arrowBackBtn.Foreground = disabledColor;
                    }
                }
            }
            Debugger.Log(0, "debug", Environment.NewLine + "historyCursor: " + historyCursor.ToString() + ", historyCount: " + history.Count().ToString() + Environment.NewLine);
        }

        private void ForwardForHistoryHandler(object sender, MouseButtonEventArgs e)
        {
            ForwardForHistory();
        }

        public void ForwardForHistory()
        {
            int countHistoryRecords = history.Count;
            bool isCanMoveCursor = historyCursor < countHistoryRecords - 1;
            if (isCanMoveCursor)
            {
                historyCursor++;
                int windowIndex = history[historyCursor];
                mainControl.SelectedIndex = windowIndex;
                bool isLastRecord = historyCursor == countHistoryRecords - 1;
                arrowBackBtn.Foreground = enabledColor;
                if (isLastRecord)
                {
                    arrowForwardBtn.Foreground = disabledColor;
                }
            }
            Debugger.Log(0, "debug", Environment.NewLine + "historyCursor: " + historyCursor.ToString() + ", historyCount: " + history.Count().ToString() + Environment.NewLine);
        }

        private void LoginToAnotherAccountHandler(object sender, RoutedEventArgs e)
        {
            LoginToAnotherAccount();
        }

        public void LoginToAnotherAccount()
        {
            Dialogs.AcceptExitDialog dialog = new Dialogs.AcceptExitDialog();
            dialog.Closed += AcceptExitDialogHandler;
            dialog.Show();
        }

        public void AcceptExitDialogHandler(object sender, EventArgs e)
        {
            Dialogs.AcceptExitDialog dialog = ((Dialogs.AcceptExitDialog)(sender));
            object data = dialog.DataContext;
            string dialogData = ((string)(data));
            AcceptExitDialog(dialogData);
        }


        public void AcceptExitDialog(string dialogData)
        {
            bool isAccept = dialogData == "OK";
            if (isAccept)
            {
                Logout();
            }
        }

        public void Logout()
        {
            Dialogs.LoginDialog dialog = new Dialogs.LoginDialog();
            dialog.Show();
            this.Close();
        }

        private void OpenPlayerHandler(object sender, RoutedEventArgs e)
        {
            OpenPlayer();
        }

        public void OpenPlayer()
        {
            Dialogs.PlayerDialog dialog = new Dialogs.PlayerDialog(currentUserId);
            dialog.Show();
        }

        public void OpenTalkFromPopupHandler (object sender, RoutedEventArgs e)
        {
            Popup popup = ((Popup)(sender));
            object popupData = popup.DataContext;
            string talkId = ((string)(popupData));
            OpenTalkFromPopup(talkId, popup);
        }

        public void OpenTalkFromPopup (string id, Popup popup)
        {
            Application app = Application.Current;
            WindowCollection windows = app.Windows;
            IEnumerable<Window> myWindows = windows.OfType<Window>();
            List<Window> chatWindows = myWindows.Where<Window>(window =>
            {
                string windowTitle = window.Title;
                bool isChatWindow = windowTitle == "Беседа";
                object windowData = window.DataContext;
                bool isWindowDataExists = windowData != null;
                bool isChatExists = true;
                if (isWindowDataExists && isChatWindow)
                {
                    string localFriend = ((string)(windowData));
                    isChatExists = id == localFriend;
                }
                return isWindowDataExists && isChatWindow && isChatExists;
            }).ToList<Window>();
            int countChatWindows = chatWindows.Count;
            bool isNotOpenedChatWindows = countChatWindows <= 0;
            if (isNotOpenedChatWindows)
            {
                Dialogs.ChatDialog dialog = new Dialogs.ChatDialog(currentUserId, client, id, false);
                dialog.Show();
                popup.IsOpen = false;
            }
        }

        public void OpenChatFromPopupHandler(object sender, RoutedEventArgs e)
        {
            Popup popup = ((Popup)(sender));
            object popupData = popup.DataContext;
            string friendId = ((string)(popupData));
            OpenChatFromPopup(friendId, popup);
        }

        public void OpenChatFromPopup(string id, Popup popup)
        {
            Application app = Application.Current;
            WindowCollection windows = app.Windows;
            IEnumerable<Window> myWindows = windows.OfType<Window>();
            List<Window> chatWindows = myWindows.Where<Window>(window =>
            {
                string windowTitle = window.Title;
                bool isChatWindow = windowTitle == "Чат";
                object windowData = window.DataContext;
                bool isWindowDataExists = windowData != null;
                bool isChatExists = true;
                if (isWindowDataExists && isChatWindow)
                {
                    string localFriend = ((string)(windowData));
                    isChatExists = id == localFriend;
                }
                return isWindowDataExists && isChatWindow && isChatExists;
            }).ToList<Window>();
            int countChatWindows = chatWindows.Count;
            bool isNotOpenedChatWindows = countChatWindows <= 0;
            if (isNotOpenedChatWindows)
            {
                Dialogs.ChatDialog dialog = new Dialogs.ChatDialog(currentUserId, client, id, false);
                dialog.Show();
                popup.IsOpen = false;
            }
        }

        private void ToggleFullScreenModeHandler(object sender, MouseButtonEventArgs e)
        {
            ToggleFullScreenMode();
        }

        public void ToggleFullScreenMode()
        {
            isFullScreenMode = !isFullScreenMode;
            if (isFullScreenMode)
            {
                this.WindowStyle = WindowStyle.None;
            }
            else
            {
                this.WindowStyle = WindowStyle.SingleBorderWindow;
            }
        }

        public void JoinToGameFromPopupHandler(object sender, RoutedEventArgs e)
        {
            Popup popup = ((Popup)(sender));
            object popupData = popup.DataContext;
            string gameName = ((string)(popupData));
            JoinToGameFromPopup(gameName, popup);
        }

        public void JoinToGameFromPopup(string gameName, Popup popup)
        {
            Environment.SpecialFolder localApplicationDataFolder = Environment.SpecialFolder.LocalApplicationData;
            string localApplicationDataFolderPath = Environment.GetFolderPath(localApplicationDataFolder);
            string saveDataFilePath = localApplicationDataFolderPath + @"\OfficeWare\GameManager\" + currentUserId + @"\save-data.txt";
            JavaScriptSerializer js = new JavaScriptSerializer();
            string saveDataFileContent = File.ReadAllText(saveDataFilePath);
            SavedContent loadedContent = js.Deserialize<SavedContent>(saveDataFileContent);
            List<Game> myGames = loadedContent.games;
            List<string> myGamesNames = new List<string>();
            foreach (Game myGame in myGames)
            {
                string myGameName = myGame.name;
                myGamesNames.Add(myGameName);
            }
            bool isSameGameForMe = myGamesNames.Contains(gameName);
            if (isSameGameForMe)
            {
                RunGame(gameName);
                popup.IsOpen = false;
            }
        }

        private void SetNameOrAvatarHandler(object sender, RoutedEventArgs e)
        {
            SetNameOrAvatar();
        }

        public void SetNameOrAvatar()
        {
            mainControl.SelectedIndex = 2;
            AddHistoryRecord();
        }

        private void UpdateUserStatusHandler(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = ((MenuItem)(sender));
            object data = menuItem.DataContext;
            string status = ((string)(data));
            UpdateUserStatus(status);
        }

        public void UpdateUserStatus(string status)
        {
            bool isOnlineStatus = status == "online";
            bool isOfflineStatus = status == "offline";
            if (isOnlineStatus)
            {
                offlineUserStatusMenuItem.IsChecked = false;
            }
            else if (isOfflineStatus)
            {
                onlineUserStatusMenuItem.IsChecked = false;
            }
            else
            {
                onlineUserStatusMenuItem.IsChecked = false;
                offlineUserStatusMenuItem.IsChecked = false;
            }
            SetUserStatus(status);
        }

        public void ToggleScreenShotsSortHandler (object sender, SelectionChangedEventArgs e)
        {
            if (isAppInit)
            {
                ToggleScreenShotsSort();
            }
        }

        public void ToggleScreenShotsSort ()
        {
            GetScreenShots("", false);
        }

        public void GetScreenShots (string filter, bool isInit)
        {
            List<Image> unSortedScreenShots = new List<Image>();
            Environment.SpecialFolder localApplicationDataFolder = Environment.SpecialFolder.LocalApplicationData;
            string localApplicationDataFolderPath = Environment.GetFolderPath(localApplicationDataFolder);
            string appPath = localApplicationDataFolderPath + @"\OfficeWare\GameManager\" + currentUserId + @"\screenshots\";
            string[] games = Directory.GetDirectories(appPath);
            screenShots.Children.Clear();
            foreach (string game in games)
            {
                DirectoryInfo gameInfo = new DirectoryInfo(game);
                string gameName = gameInfo.Name;
                if (isInit)
                {
                    ComboBoxItem screenShotsFilterItem = new ComboBoxItem();
                    screenShotsFilterItem.Content = gameName;
                    screenShotsFilter.Items.Add(screenShotsFilterItem);
                }
                string[] files = Directory.GetFileSystemEntries(game);
                foreach (string file in files)
                {
                    string ext = System.IO.Path.GetExtension(file);
                    bool isScreenShot = ext == ".jpg";
                    if (isScreenShot)
                    {
                        Image screenShot = new Image();
                        screenShot.Margin = new Thickness(15);
                        screenShot.Width = 250;
                        screenShot.Height = 250;
                        screenShot.BeginInit();
                        Uri screenShotUri = new Uri(file);
                        screenShot.Source = new BitmapImage(screenShotUri);
                        screenShot.EndInit();
                        string insensitiveCaseFilter = filter.ToLower();
                        string insensitiveCaseGameName = gameName.ToLower();
                        int filterLength = filter.Length;
                        bool isNotFilter = filterLength <= 0;
                        bool isWordsMatches = insensitiveCaseGameName.Contains(insensitiveCaseFilter);
                        bool isFilterMatches = isWordsMatches || isNotFilter;
                        if (isFilterMatches)
                        {
                            /*                          
                            UIElementCollection screenShotsContainerChildren = screenShotsContainer.Children;
                            UIElement container = screenShotsContainerChildren[2];
                            bool isWall = container is StackPanel;
                            if (isWall)
                            {
                                StackPanel wallContainer = ((StackPanel)(container));
                                wallContainer.Children.Add(screenShot);
                            }
                            else
                            {
                                WrapPanel gridContainer = ((WrapPanel)(container));
                                gridContainer.Children.Add(screenShot);
                            }
                            */
                            Dictionary<String, Object> screenShotData = new Dictionary<String, Object>();
                            FileInfo info = new FileInfo(file);
                            DateTime date = info.CreationTime;
                            screenShotData.Add("date", date);
                            screenShot.DataContext = screenShotData;
                            unSortedScreenShots.Add(screenShot);
                        }
                    }
                }
            }

            List<Image> sortedScreenShots = new List<Image>();
            sortedScreenShots = unSortedScreenShots;
            int sortIndex = screenShotsSortBox.SelectedIndex;
            bool isAsc = sortIndex == 1;
            bool isDesc = sortIndex == 2;
            if (isAsc)
            {
                sortedScreenShots = unSortedScreenShots.OrderBy(screenShot =>
                {
                    object data = screenShot.DataContext;
                    Dictionary<String, Object> screenShotData = ((Dictionary<String, Object>)(data));
                    object rawDate = screenShotData["date"];
                    DateTime date = ((DateTime)(rawDate));
                    return date;
                }).ToList<Image>();
            }
            else if (isDesc)
            {
                sortedScreenShots = unSortedScreenShots.OrderByDescending(screenShot =>
                {
                    object data = screenShot.DataContext;
                    Dictionary<String, Object> screenShotData = ((Dictionary<String, Object>)(data));
                    object rawDate = screenShotData["date"];
                    DateTime date = ((DateTime)(rawDate));
                    return date;
                }).ToList<Image>();
            }
            UIElementCollection screenShotsContainerChildren = screenShotsContainer.Children;
            UIElement container = screenShotsContainerChildren[2];
            bool isWall = container is StackPanel;
            foreach (UIElement screenShot in sortedScreenShots)
            {
                if (isWall)
                {
                    StackPanel wallContainer = ((StackPanel)(container));
                    wallContainer.Children.Add(screenShot);
                }
                else
                {
                    WrapPanel gridContainer = ((WrapPanel)(container));
                    gridContainer.Children.Add(screenShot);
                }
            }

            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/users/get/?id=" + currentUserId);
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (var reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        var objText = reader.ReadToEnd();
                        UserResponseInfo myobj = (UserResponseInfo)js.Deserialize(objText, typeof(UserResponseInfo));
                        string status = myobj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
                            User user = myobj.user;
                            string userName = user.name;
                            userNameContentLabel.Text = userName;
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

        private void SelectScreenShotsFilterHandler(object sender, SelectionChangedEventArgs e)
        {
            int selectedIndex = screenShotsFilter.SelectedIndex;
            SelectScreenShotsFilter(selectedIndex);
        }

        public void SelectScreenShotsFilter(int selectedIndex)
        {
            if (isAppInit)
            {
                bool isSecondItem = selectedIndex == 1;
                if (isSecondItem)
                {
                    screenShotsFilter.SelectedIndex = 0;
                    GetScreenShots("", false);
                }
                else
                {
                    object rawSelectedItem = screenShotsFilter.Items[selectedIndex];
                    ComboBoxItem selectedItem = ((ComboBoxItem)(rawSelectedItem));
                    object rawFilter = selectedItem.Content;
                    string filter = rawFilter.ToString();
                    GetScreenShots(filter, false);
                }
            }
        }

        private void SetEditProfileTabHandler(object sender, MouseButtonEventArgs e)
        {
            StackPanel tab = ((StackPanel)(sender));
            object tabData = tab.DataContext;
            string tabIndex = tabData.ToString();
            int parsedTabIndex = Int32.Parse(tabIndex);
            SetEditProfileTabHandler(parsedTabIndex);
        }

        public void SetEditProfileTabHandler(int index)
        {
            editProfileTabControl.SelectedIndex = index;
        }

        private void UploadAvatarHandler(object sender, RoutedEventArgs e)
        {
            UploadAvatar();
        }

        public void UploadAvatar()
        {
            Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
            ofd.Title = "Выберите аватар";
            ofd.Filter = "Png documents (.png)|*.png";
            bool? res = ofd.ShowDialog();
            bool isOpened = res != false;
            if (isOpened)
            {
                string filePath = ofd.FileName;
                editProfileAvatarImg.BeginInit();
                editProfileAvatarImg.Source = new BitmapImage(new Uri(filePath));
                editProfileAvatarImg.EndInit();
            }
        }

        private static byte[] GetMultipartFormData(Dictionary<string, object> postParameters, string boundary)
        {
            Stream formDataStream = new System.IO.MemoryStream();
            bool needsCLRF = false;
            Encoding encoding = Encoding.UTF8;

            foreach (var param in postParameters)
            {

                if (needsCLRF)
                {
                    formDataStream.Write(encoding.GetBytes("\r\n"), 0, encoding.GetByteCount("\r\n"));
                }
                needsCLRF = true;

                if (param.Value is FileParameter) // to check if parameter if of file type
                {
                    FileParameter fileToUpload = (FileParameter)param.Value;

                    // Add just the first part of this param, since we will write the file data directly to the Stream
                    string header = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"; filename=\"{2}\"\r\nContent-Type: {3}\r\n\r\n",
                        boundary,
                        param.Key,
                        fileToUpload.FileName ?? param.Key,
                        fileToUpload.ContentType ?? "application/octet-stream");

                    formDataStream.Write(encoding.GetBytes(header), 0, encoding.GetByteCount(header));
                    // Write the file data directly to the Stream, rather than serializing it to a string.
                    formDataStream.Write(fileToUpload.File, 0, fileToUpload.File.Length);
                }
                else
                {
                    string postData = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"\r\n\r\n{2}",
                        boundary,
                        param.Key,
                        param.Value);
                    formDataStream.Write(encoding.GetBytes(postData), 0, encoding.GetByteCount(postData));
                }
            }

            // Add the end of the request.  Start with a newline
            string footer = "\r\n--" + boundary + "--\r\n";
            formDataStream.Write(encoding.GetBytes(footer), 0, encoding.GetByteCount(footer));

            // Dump the Stream into a byte[]
            formDataStream.Position = 0;
            byte[] formData = new byte[formDataStream.Length];
            formDataStream.Read(formData, 0, formData.Length);
            formDataStream.Close();

            return formData;
        }

        private void SetDefautAvatarHandler(object sender, ExceptionRoutedEventArgs e)
        {
            Image avatar = ((Image)(sender));
            SetDefautAvatar(avatar);
        }

        public void SetDefautAvatar(Image avatar)
        {
            avatar.BeginInit();
            avatar.Source = new BitmapImage(new Uri(@"https://cdn2.iconfinder.com/data/icons/ios-7-icons/50/user_male-128.png"));
            avatar.EndInit();
        }

        public void SetDefaultThumbnailHandler(object sender, ExceptionRoutedEventArgs e)
        {
            Image avatar = ((Image)(sender));
            SetDefaultThumbnail(avatar);
        }

        public void SetDefaultThumbnail(Image avatar)
        {
            avatar.BeginInit();
            avatar.Source = new BitmapImage(new Uri(@"https://cdn3.iconfinder.com/data/icons/solid-locations-icon-set/64/Games_2-256.png"));
            avatar.EndInit();
        }

        private byte[] ImageFileToByteArray(string fullFilePath)
        {
            FileStream fs = File.OpenRead(fullFilePath);
            byte[] bytes = new byte[fs.Length];
            fs.Read(bytes, 0, Convert.ToInt32(fs.Length));
            fs.Close();
            return bytes;
        }

        private void GenerateDatas()
        {
            this.Collection = new ObservableCollection<Model>();
            this.Collection.Add(new Model(10, 1, 5, 4));
            this.Collection.Add(new Model(10, 1, 5, 4));
            this.Collection.Add(new Model(10, 1, 5, 4));
            this.Collection.Add(new Model(10, 1, 5, 4));
        }

        private void AddDiscussionHandler(object sender, RoutedEventArgs e)
        {
            object btnData = addDiscussionBtn.DataContext;
            string forumId = ((string)(btnData));
            AddDiscussion(forumId);
        }

        public void AddDiscussion(string forumId)
        {
            try
            {
                string title = discussionTitleBox.Text;
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/forums/topics/create/?forum=" + forumId + "&title=" + title + "&user=" + currentUserId);
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (var reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        var objText = reader.ReadToEnd();
                        ForumResponseInfo myobj = (ForumResponseInfo)js.Deserialize(objText, typeof(ForumResponseInfo));
                        string status = myobj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
                            addDiscussionDialog.Visibility = invisible;
                            discussionTitleBox.Text = "";
                            discussionQuestionBox.Text = "";
                            SelectForum(forumId);

                            string eventData = forumId + "|" + "mockTopicId" + "|" + currentUserId;
                            client.EmitAsync("user_send_msg_to_forum", eventData);

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

        public void OpenAddDiscussionDialogHandler(object sender, RoutedEventArgs e)
        {
            OpenAddDiscussionDialog();
        }

        public void OpenAddDiscussionDialog()
        {
            mainControl.SelectedIndex = 7;
            addDiscussionDialog.Visibility = visible;

            addDiscussionUserAvatar.Source = new BitmapImage(new Uri(@"http://localhost:4000/api/user/avatar/?id=" + currentUserId));

        }

        private void SendMsgToTopicHandler(object sender, RoutedEventArgs e)
        {
            object topicData = addDiscussionMsgBtn.DataContext;
            string topicId = ((string)(topicData));
            SendMsgToTopic(topicId);
        }

        public void SendMsgToTopic(string topicId)
        {
            string newMsgContent = forumTopicMsgBox.Text;
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/forums/topics/msgs/create/?user=" + currentUserId + "&topic=" + topicId + "&content=" + newMsgContent);
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (var reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        var objText = reader.ReadToEnd();
                        ForumResponseInfo myobj = (ForumResponseInfo)js.Deserialize(objText, typeof(ForumResponseInfo));
                        string status = myobj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
                            forumTopicMsgBox.Text = "";
                            SelectTopic(topicId);

                            /*
                                пытался здесь добавить перепрыгивание на последнюю страницу после добавления сообщения, но не работает
                                UIElementCollection forumTopicPagesChildren = forumTopicPages.Children;
                                int countForumTopicPagesChildren = forumTopicPagesChildren.Count;
                                int lastPageLabelIndex = countForumTopicPagesChildren - 1;
                                UIElement lastPage = forumTopicPages.Children[lastPageLabelIndex];
                                TextBlock lastPageLabel = ((TextBlock)(lastPage));
                                SetCountMsgsOnForumTopicPage(topicId, countForumTopicPagesChildren, lastPageLabel);
                            */

                            object btnData = addDiscussionBtn.DataContext;
                            string forumId = ((string)(btnData));
                            string eventData = forumId + "|" + topicId + "|" + currentUserId;
                            client.EmitAsync("user_send_msg_to_forum", eventData);
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

        private void FilterForumsHandler(object sender, TextChangedEventArgs e)
        {
            TextBox box = ((TextBox)(sender));
            FilterForums(box);
        }

        public void FilterForums(TextBox box)
        {
            string keywords = box.Text;
            GetForums(keywords);
        }

        private void SetCountMsgsOnForumTopicPageHandler(object sender, MouseButtonEventArgs e)
        {
            TextBlock countLabel = ((TextBlock)(sender));
            object countLabelData = countLabel.DataContext;
            string rawCountLabelData = countLabelData.ToString();
            int countMsgs = Int32.Parse(rawCountLabelData);
            object addDiscussionMsgBtnData = addDiscussionMsgBtn.DataContext;
            string topicId = ((string)(addDiscussionMsgBtnData));

            TextBlock firstPageLabel = ((TextBlock)(forumTopicPages.Children[0]));
            SelectForumTopicPage(1, topicId, firstPageLabel);

            UpdatePaginationPointers(1);
            SetCountMsgsOnForumTopicPage(topicId, countMsgs, countLabel);
        }

        public void SetCountMsgsOnForumTopicPage(string topicId, int countMsgs, TextBlock label)
        {
            forumTopicCountMsgs.DataContext = countMsgs;
            forumTopic15CountMsgs.Foreground = System.Windows.Media.Brushes.White;
            forumTopic30CountMsgs.Foreground = System.Windows.Media.Brushes.White;
            forumTopic50CountMsgs.Foreground = System.Windows.Media.Brushes.White;
            label.Foreground = System.Windows.Media.Brushes.LightGray;
            SelectTopic(topicId);
        }

        public void SelectForumTopicPageHandler(object sender, RoutedEventArgs e)
        {
            TextBlock pageLabel = ((TextBlock)(sender));
            object pageLabelData = pageLabel.DataContext;
            int pageNumber = ((int)(pageLabelData));
            object addDiscussionMsgBtnData = addDiscussionMsgBtn.DataContext;
            string topicId = ((string)(addDiscussionMsgBtnData));

            UpdatePaginationPointers(pageNumber);

            SelectForumTopicPage(pageNumber, topicId, pageLabel);
        }

        public void UpdatePaginationPointers(int pageNumber)
        {
            forumTopicPrevPaginationBtn.Foreground = System.Windows.Media.Brushes.Black;
            forumTopicNextPaginationBtn.Foreground = System.Windows.Media.Brushes.Black;
            bool isFirstPage = pageNumber == 1;
            int countPages = forumTopicPages.Children.Count;
            bool isLastPage = pageNumber == countPages;
            if (isFirstPage)
            {
                forumTopicPrevPaginationBtn.Foreground = System.Windows.Media.Brushes.LightGray;
            }
            if (isLastPage)
            {
                forumTopicNextPaginationBtn.Foreground = System.Windows.Media.Brushes.LightGray;
            }
        }

        public void SelectForumTopicPage(int pageNumber, string topicId, TextBlock pageLabel)
        {
            string rawPageNumber = pageNumber.ToString();
            forumTopicPages.DataContext = rawPageNumber;
            foreach (TextBlock somePageLabel in forumTopicPages.Children)
            {
                somePageLabel.Foreground = System.Windows.Media.Brushes.White;
            }
            pageLabel.Foreground = System.Windows.Media.Brushes.DarkCyan;
            SelectTopic(topicId);
        }

        private void GoToPrevForumTopicPageHandler(object sender, MouseButtonEventArgs e)
        {
            GoToPrevForumTopicPage();
        }

        public void GoToPrevForumTopicPage()
        {
            object topicData = addDiscussionMsgBtn.DataContext;
            string topicId = ((string)(topicData));
            int countPages = forumTopicPages.Children.Count;
            object currentPageNumber = forumTopicPages.DataContext;
            string rawCurrentPageNumber = currentPageNumber.ToString();
            int currentPage = Int32.Parse(rawCurrentPageNumber);
            int currentPageIndex = currentPage - 1;
            bool isCanGo = currentPage > 1;
            if (isCanGo)
            {
                TextBlock pageLabel = ((TextBlock)(forumTopicPages.Children[currentPageIndex - 1]));
                SelectForumTopicPage(currentPageIndex, topicId, pageLabel);
            }
        }

        private void GoToNextForumTopicPageHandler(object sender, MouseButtonEventArgs e)
        {
            GoToNextForumTopicPage();
        }

        public void GoToNextForumTopicPage()
        {
            object topicData = addDiscussionMsgBtn.DataContext;
            string topicId = ((string)(topicData));
            int countPages = forumTopicPages.Children.Count;
            object currentPageNumber = forumTopicPages.DataContext;
            string rawCurrentPageNumber = currentPageNumber.ToString();
            int currentPage = Int32.Parse(rawCurrentPageNumber);
            int currentPageIndex = currentPage - 1;
            bool isCanGo = currentPage < countPages;
            if (isCanGo)
            {
                TextBlock pageLabel = ((TextBlock)(forumTopicPages.Children[currentPageIndex + 1]));
                SelectForumTopicPage(currentPageIndex + 2, topicId, pageLabel);
            }
        }

        private void SelectProfileThemeHandler(object sender, MouseButtonEventArgs e)
        {
            StackPanel theme = ((StackPanel)(sender));
            object themeData = theme.DataContext;
            string themeName = themeData.ToString();
            SelectProfileTheme(themeName, theme);
        }

        public void SelectProfileTheme(string themeName, StackPanel theme)
        {

            foreach (StackPanel profileTheme in profileThemes.Children)
            {
                UIElement rawProfileThemeNameLabel = profileTheme.Children[1];
                TextBlock profileThemeNameLabel = ((TextBlock)(rawProfileThemeNameLabel));
                profileThemeNameLabel.Foreground = System.Windows.Media.Brushes.LightGray;
            }
            UIElement rawThemeNameLabel = theme.Children[1];
            TextBlock themeNameLabel = ((TextBlock)(rawThemeNameLabel));
            themeNameLabel.Foreground = System.Windows.Media.Brushes.Blue;
            editProfileThemeName.DataContext = themeName;

        }

        private void SetAllAccessSettingsHandler(object sender, SelectionChangedEventArgs e)
        {
            ComboBox selector = ((ComboBox)(sender));
            int index = selector.SelectedIndex;
            SetAllAccessSettings(index);
        }

        private void SetAllAccessSettings(int index)
        {
            if (isAppInit)
            {
                userFriendsSettingsSelector.SelectedIndex = index;
                userGamesSettingsSelector.SelectedIndex = index;
                userEquipmentSettingsSelector.SelectedIndex = index;
                userCommentsSettingsSelector.SelectedIndex = index;
            }
        }

        private void CreateCollectionHandler(object sender, RoutedEventArgs e)
        {
            Dictionary<String, Object> mockData = new Dictionary<String, Object>();
            mockData.Add("name", "");
            mockData.Add("collection", "");
            CreateCollection(mockData);
        }

        private void CreateCollectionFromMenuHandler(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = ((MenuItem)(sender));
            object menuItemData = menuItem.DataContext;
            Dictionary<String, Object> data = ((Dictionary<String, Object>)(menuItemData));
            CreateCollection(data);
        }

        public void CreateCollection(Dictionary<String, Object> data)
        {
            Dialogs.AddCollectiionDialog dialog = new Dialogs.AddCollectiionDialog(currentUserId);
            dialog.DataContext = data;
            dialog.Closed += RefreshGameCollectionsHandler;
            dialog.Show();
        }

        public void RefreshGameCollectionsHandler(object sender, EventArgs e)
        {
            Dialogs.AddCollectiionDialog dialog = ((Dialogs.AddCollectiionDialog)(sender));
            object rawData = dialog.DataContext;
            Dictionary<String, Object> data = ((Dictionary<String, Object>)(rawData));
            string name = ((string)(data["name"]));
            string collection = ((string)(data["collection"]));
            int nameLength = name.Length;
            bool isAddToNewCollection = nameLength >= 1;
            if (isAddToNewCollection)
            {
                AddGameToCollection(data);
                SelectGameCollection(collection);
            }
            RefreshGameCollections();
        }

        public void RefreshGameCollections()
        {
            GetGameCollections();
            GetGamesList("");
        }

        private void ReturnToProfileHandler(object sender, MouseButtonEventArgs e)
        {
            ReturnToProfile();
        }

        public void ReturnToProfile()
        {
            object mainControlData = mainControl.DataContext;
            string userId = ((string)(mainControlData));
            // string userId = currentUserId;
            bool isLocalUser = userId == currentUserId;
            mainControl.SelectedIndex = 1;
            GetUserInfo(userId, isLocalUser);
            AddHistoryRecord();
        }

        private void ToggleRenameBtnHandler(object sender, MouseButtonEventArgs e)
        {
            PackIcon icon = ((PackIcon)(sender));
            ToggleRenameBtn(icon);
        }

        public void ToggleRenameBtn(PackIcon icon)
        {
            bool isEdit = gameCollectionNameLabel.IsEnabled;
            bool toggleMode = !isEdit;
            if (isEdit)
            {
                gameCollectionNameLabel.Background = System.Windows.Media.Brushes.Transparent;
                gameCollectionNameLabel.BorderThickness = new Thickness(0);
                icon.Kind = PackIconKind.Edit;
            }
            else
            {
                gameCollectionNameLabel.Background = System.Windows.Media.Brushes.White;
                gameCollectionNameLabel.BorderThickness = new Thickness(1);
                icon.Kind = PackIconKind.Close;
            }
            gameCollectionNameLabel.IsEnabled = toggleMode;
        }

        private void SaveGameCollectionNameHandler(object sender, KeyboardFocusChangedEventArgs e)
        {

            SaveGameCollectionName();
        }

        public void SaveGameCollectionName()
        {
            Environment.SpecialFolder localApplicationDataFolder = Environment.SpecialFolder.LocalApplicationData;
            string localApplicationDataFolderPath = Environment.GetFolderPath(localApplicationDataFolder);
            string saveDataFilePath = localApplicationDataFolderPath + @"\OfficeWare\GameManager\" + currentUserId + @"\save-data.txt";
            JavaScriptSerializer js = new JavaScriptSerializer();
            string saveDataFileContent = File.ReadAllText(saveDataFilePath);
            SavedContent loadedContent = js.Deserialize<SavedContent>(saveDataFileContent);
            List<Game> updatedGames = loadedContent.games;
            List<FriendSettings> currentFriends = loadedContent.friends;
            Settings currentSettings = loadedContent.settings;
            List<string> updatedCollections = loadedContent.collections;
            string gameCollectionNameLabelContent = gameCollectionNameLabel.Text;
            object rawCurrentGameCollection = gameCollectionNameLabel.DataContext;
            string currentGameCollection = ((string)(rawCurrentGameCollection));
            int collectionIndex = -1;
            foreach (string updatedCollection in updatedCollections)
            {
                collectionIndex++;
                bool isCollectionFound = currentGameCollection == updatedCollection;
                if (isCollectionFound)
                {
                    updatedCollections[collectionIndex] = gameCollectionNameLabelContent;
                    break;
                }
            }
            foreach (Game updatedGame in updatedGames)
            {
                List<string> updatedGameCollections = updatedGame.collections;
                collectionIndex = -1;
                foreach (string updatedGameCollection in updatedGameCollections)
                {
                    collectionIndex++;
                    bool isCollectionFound = currentGameCollection == updatedGameCollection;
                    if (isCollectionFound)
                    {
                        updatedGame.collections[collectionIndex] = gameCollectionNameLabelContent;
                        break;
                    }
                }
            }
            string savedContent = js.Serialize(new SavedContent
            {
                games = updatedGames,
                friends = currentFriends,
                settings = currentSettings,
                collections = updatedCollections
            });
            File.WriteAllText(saveDataFilePath, savedContent);
            GetGamesList("");
            gameCollectionNameLabel.DataContext = gameCollectionNameLabelContent;
            ToggleRenameBtn(renameIcon);
            GetGameCollections();
        }

        public void CreateShortcutHandler(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = ((MenuItem)(sender));
            object menuItemData = menuItem.DataContext;
            string name = ((string)(menuItemData));
            CreateShortcut(name);
        }

        private void CreateShortcut(string name)
        {
            Environment.SpecialFolder localApplicationDataFolder = Environment.SpecialFolder.LocalApplicationData;
            string localApplicationDataFolderPath = Environment.GetFolderPath(localApplicationDataFolder);
            string saveDataFilePath = localApplicationDataFolderPath + @"\OfficeWare\GameManager\" + currentUserId + @"\save-data.txt";
            JavaScriptSerializer js = new JavaScriptSerializer();
            string saveDataFileContent = File.ReadAllText(saveDataFilePath);
            SavedContent loadedContent = js.Deserialize<SavedContent>(saveDataFileContent);
            List<Game> currentGames = loadedContent.games;
            List<Game> results = currentGames.Where<Game>((Game game) =>
            {
                string gameName = game.name;
                bool isFound = gameName == name;
                return isFound;
            }).ToList();
            int countResults = results.Count;
            bool isHaveResults = countResults >= 1;
            if (isHaveResults)
            {
                Game result = results[0];
                string gamePath = result.path;
                string link = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + System.IO.Path.DirectorySeparatorChar + name + ".lnk";
                var shell = new IWshRuntimeLibrary.WshShell();
                var shortcut = shell.CreateShortcut(link) as IWshRuntimeLibrary.IWshShortcut;
                shortcut.TargetPath = gamePath;
                shortcut.WorkingDirectory = System.Windows.Forms.Application.StartupPath;
                shortcut.Save();
            }
        }

        private void OpenHiddenGamesHandler(object sender, RoutedEventArgs e)
        {
            OpenHiddenGames();
        }

        public void OpenHiddenGames()
        {
            mainControl.SelectedIndex = 11;
            GetHiddenGames();
            Environment.SpecialFolder localApplicationDataFolder = Environment.SpecialFolder.LocalApplicationData;
            string localApplicationDataFolderPath = Environment.GetFolderPath(localApplicationDataFolder);
            string saveDataFilePath = localApplicationDataFolderPath + @"\OfficeWare\GameManager\" + currentUserId + @"\save-data.txt";
            JavaScriptSerializer js = new JavaScriptSerializer();
            string saveDataFileContent = File.ReadAllText(saveDataFilePath);
            SavedContent loadedContent = js.Deserialize<SavedContent>(saveDataFileContent);
            List<Game> currentGames = loadedContent.games;
            List<Game> shadowGames = currentGames.Where<Game>((Game game) =>
            {
                bool isHidden = game.isHidden;
                return isHidden;
            }).ToList<Game>();
            int countHiddenGames = shadowGames.Count;
            string rawCountHiddenGames = countHiddenGames.ToString();
            countHiddenGamesLabel.Text = "(" + rawCountHiddenGames + ")";
        }

        public void RunGameHandler(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = ((MenuItem)(sender));
            object menuItemData = menuItem.DataContext;
            string name = ((string)(menuItemData));
            RunGame(name);
        }

        public void GetHiddenGames()
        {
            Environment.SpecialFolder localApplicationDataFolder = Environment.SpecialFolder.LocalApplicationData;
            string localApplicationDataFolderPath = Environment.GetFolderPath(localApplicationDataFolder);
            string saveDataFilePath = localApplicationDataFolderPath + @"\OfficeWare\GameManager\" + currentUserId + @"\save-data.txt";
            JavaScriptSerializer js = new JavaScriptSerializer();
            string saveDataFileContent = File.ReadAllText(saveDataFilePath);
            SavedContent loadedContent = js.Deserialize<SavedContent>(saveDataFileContent);
            List<Game> currentGames = loadedContent.games;
            List<string> currentCollections = loadedContent.collections;
            List<Game> shadowGames = currentGames.Where<Game>((Game game) =>
            {
                bool isHidden = game.isHidden;
                return isHidden;
            }).ToList();
            int hiddenGamesCount = shadowGames.Count;
            bool isHaveGames = hiddenGamesCount >= 1;
            hiddenGames.HorizontalAlignment = HorizontalAlignment.Left;
            hiddenGames.Children.Clear();
            if (isHaveGames)
            {
                foreach (Game hiddenGame in shadowGames)
                {
                    string currentGameId = hiddenGame.id;
                    string currentGameName = hiddenGame.name;
                    List<string> currentGameCollections = hiddenGame.collections;
                    string currentGameCover = hiddenGame.cover;

                    Image gameCollectionItem = new Image();
                    gameCollectionItem.Width = 100;
                    gameCollectionItem.Height = 100;
                    gameCollectionItem.Margin = new Thickness(25);
                    gameCollectionItem.BeginInit();
                    bool IsCoverSet = currentGameCover != "";
                    bool isCoverFound = File.Exists(currentGameCover);
                    bool isCoverExists = IsCoverSet && isCoverFound;
                    Uri coverUri = null;
                    if (isCoverExists)
                    {
                        coverUri = new Uri(currentGameCover);
                    }
                    else
                    {
                        coverUri = new Uri(@"http://localhost:4000/api/game/thumbnail/?name=" + currentGameName);
                    }
                    gameCollectionItem.Source = new BitmapImage(coverUri);
                    gameCollectionItem.ImageFailed += SetDefaultThumbnailHandler;
                    gameCollectionItem.EndInit();
                    hiddenGames.Children.Add(gameCollectionItem);
                    gameCollectionItem.DataContext = currentGameName;
                    gameCollectionItem.MouseLeftButtonUp += SelectGameCollectionItemHandler;
                    ContextMenu gameCollectionItemContextMenu = new ContextMenu();

                    MenuItem gameCollectionItemContextMenuItem = new MenuItem();
                    gameCollectionItemContextMenuItem.Header = "Играть";
                    gameCollectionItemContextMenuItem.DataContext = currentGameName;
                    gameCollectionItemContextMenuItem.Click += RunGameHandler;
                    gameCollectionItemContextMenu.Items.Add(gameCollectionItemContextMenuItem);

                    gameCollectionItemContextMenuItem = new MenuItem();
                    gameCollectionItemContextMenuItem.Header = "Добавить в";
                    MenuItem gameCollectionItemNestedContextMenuItem;
                    Dictionary<String, Object> gameCollectionItemNestedContextMenuItemData;
                    foreach (string currentCollection in currentCollections)
                    {
                        gameCollectionItemNestedContextMenuItem = new MenuItem();
                        gameCollectionItemNestedContextMenuItem.Header = currentCollection;
                        gameCollectionItemNestedContextMenuItemData = new Dictionary<String, Object>();
                        gameCollectionItemNestedContextMenuItemData.Add("name", currentGameName);
                        gameCollectionItemNestedContextMenuItemData.Add("collection", currentCollection);
                        gameCollectionItemNestedContextMenuItem.DataContext = gameCollectionItemNestedContextMenuItemData;
                        gameCollectionItemNestedContextMenuItem.Click += AddGameToCollectionHandler;
                        gameCollectionItemContextMenuItem.Items.Add(gameCollectionItemNestedContextMenuItem);
                        bool isDisabledCollection = currentGameCollections.Contains(currentCollection);
                        if (isDisabledCollection)
                        {
                            gameCollectionItemNestedContextMenuItem.IsEnabled = false;
                        }
                    }
                    gameCollectionItemNestedContextMenuItem = new MenuItem();
                    gameCollectionItemNestedContextMenuItem.Header = "Создать коллекцию";
                    gameCollectionItemNestedContextMenuItemData = new Dictionary<String, Object>();
                    gameCollectionItemNestedContextMenuItemData.Add("name", currentGameName);
                    gameCollectionItemNestedContextMenuItemData.Add("collection", "mockCollectionName");
                    gameCollectionItemNestedContextMenuItem.DataContext = gameCollectionItemNestedContextMenuItemData;
                    gameCollectionItemNestedContextMenuItem.Click += CreateCollectionFromMenuHandler;
                    gameCollectionItemContextMenuItem.Items.Add(gameCollectionItemNestedContextMenuItem);
                    gameCollectionItemContextMenu.Items.Add(gameCollectionItemContextMenuItem);

                    gameCollectionItemContextMenuItem = new MenuItem();
                    string gameCollectionItemContextMenuItemHeaderContent = "Убрать из ";
                    gameCollectionItemContextMenuItem.Header = gameCollectionItemContextMenuItemHeaderContent;

                    foreach (string hiddenGameCollection in hiddenGame.collections)
                    {
                        gameCollectionItemNestedContextMenuItem = new MenuItem();
                        gameCollectionItemNestedContextMenuItem.Header = hiddenGameCollection;
                        gameCollectionItemNestedContextMenuItemData = new Dictionary<String, Object>();
                        gameCollectionItemNestedContextMenuItemData.Add("game", currentGameName);
                        gameCollectionItemNestedContextMenuItemData.Add("collection", hiddenGameCollection);
                        gameCollectionItemNestedContextMenuItem.DataContext = gameCollectionItemNestedContextMenuItemData;
                        gameCollectionItemNestedContextMenuItem.Click += RemoveGameFromCollectionHandler;
                        gameCollectionItemContextMenuItem.Items.Add(gameCollectionItemNestedContextMenuItem);
                    }
                    gameCollectionItemContextMenu.Items.Add(gameCollectionItemContextMenuItem);

                    gameCollectionItemContextMenuItem = new MenuItem();
                    gameCollectionItemContextMenuItem.Header = "Управление";

                    gameCollectionItemNestedContextMenuItem = new MenuItem();
                    gameCollectionItemNestedContextMenuItem.Header = "Создать ярлык на рабочем столе";
                    gameCollectionItemNestedContextMenuItem.DataContext = currentGameName;
                    gameCollectionItemNestedContextMenuItem.Click += CreateShortcutHandler;
                    gameCollectionItemContextMenuItem.Items.Add(gameCollectionItemNestedContextMenuItem);

                    gameCollectionItemNestedContextMenuItem = new MenuItem();
                    if (IsCoverSet)
                    {
                        gameCollectionItemNestedContextMenuItem.Header = "Удалить свою обложку";
                    }
                    else
                    {
                        gameCollectionItemNestedContextMenuItem.Header = "Задать свою обложку";
                    }
                    gameCollectionItemNestedContextMenuItemData = new Dictionary<String, Object>();
                    gameCollectionItemNestedContextMenuItemData.Add("game", currentGameName);
                    gameCollectionItemNestedContextMenuItemData.Add("collection", "");
                    gameCollectionItemNestedContextMenuItemData.Add("cover", currentGameCover);
                    gameCollectionItemNestedContextMenuItem.DataContext = gameCollectionItemNestedContextMenuItemData;
                    gameCollectionItemNestedContextMenuItem.Click += ToggleGameCoverHandler;
                    gameCollectionItemContextMenuItem.Items.Add(gameCollectionItemNestedContextMenuItem);

                    gameCollectionItemNestedContextMenuItem = new MenuItem();
                    gameCollectionItemNestedContextMenuItem.Header = "Просмотреть локальные файлы";
                    gameCollectionItemNestedContextMenuItem.DataContext = currentGameName;
                    gameCollectionItemNestedContextMenuItem.Click += ShowGamesLocalFilesHandler;
                    gameCollectionItemContextMenuItem.Items.Add(gameCollectionItemNestedContextMenuItem);

                    gameCollectionItemNestedContextMenuItem = new MenuItem();
                    bool isHiddenGame = hiddenGame.isHidden;
                    if (isHiddenGame)
                    {
                        gameCollectionItemNestedContextMenuItem.Header = "Убрать из скрытого";
                    }
                    else
                    {
                        gameCollectionItemNestedContextMenuItem.Header = "Скрыть игру";
                    }

                    gameCollectionItemNestedContextMenuItemData = new Dictionary<String, Object>();
                    gameCollectionItemNestedContextMenuItemData.Add("game", currentGameName);
                    gameCollectionItemNestedContextMenuItemData.Add("collection", "");
                    gameCollectionItemNestedContextMenuItem.DataContext = gameCollectionItemNestedContextMenuItemData;
                    gameCollectionItemNestedContextMenuItem.Click += ToggleGameVisibilityHandler;
                    gameCollectionItemContextMenuItem.Items.Add(gameCollectionItemNestedContextMenuItem);

                    gameCollectionItemNestedContextMenuItem = new MenuItem();
                    gameCollectionItemNestedContextMenuItem.Header = "Удалить с утройства";
                    gameCollectionItemNestedContextMenuItemData = new Dictionary<String, Object>();
                    gameCollectionItemNestedContextMenuItemData.Add("game", currentGameName);
                    gameCollectionItemNestedContextMenuItemData.Add("collection", "");
                    gameCollectionItemNestedContextMenuItem.DataContext = gameCollectionItemNestedContextMenuItemData;
                    gameCollectionItemNestedContextMenuItem.Click += RemoveGameFromCollectionsMenuHandler;
                    gameCollectionItemContextMenuItem.Items.Add(gameCollectionItemNestedContextMenuItem);

                    gameCollectionItemContextMenu.Items.Add(gameCollectionItemContextMenuItem);

                    gameCollectionItemContextMenuItem = new MenuItem();
                    gameCollectionItemContextMenuItem.Header = "Свойства";
                    Dictionary<String, Object> gameCollectionItemContextMenuItemData = new Dictionary<String, Object>();
                    gameCollectionItemContextMenuItemData.Add("game", currentGameName);
                    bool isCustomGame = currentGameId == "mockId";
                    gameCollectionItemContextMenuItemData.Add("isCustomGame", isCustomGame);
                    gameCollectionItemContextMenuItem.DataContext = currentGameName;
                    gameCollectionItemContextMenuItem.Click += OpenGameSettingsHandler;
                    gameCollectionItemContextMenu.Items.Add(gameCollectionItemContextMenuItem);

                    gameCollectionItem.ContextMenu = gameCollectionItemContextMenu;
                }
            }
            else
            {
                hiddenGames.HorizontalAlignment = HorizontalAlignment.Center;
                TextBlock gameCollectionsNotFoundLabel = new TextBlock();
                gameCollectionsNotFoundLabel.Text = "Срытых игр не обнаружено.";
                hiddenGames.Children.Add(gameCollectionsNotFoundLabel);
            }
        }

        public void ToggleGameVisibilityHandler(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = ((MenuItem)(sender));
            object menuItemData = menuItem.DataContext;
            Dictionary<String, Object> data = ((Dictionary<String, Object>)(menuItemData));
            ToggleGameVisibility(data);
        }

        public void ToggleGameVisibility(Dictionary<String, Object> data)
        {
            string name = ((string)(data["game"]));
            string collection = ((string)(data["collection"]));

            Environment.SpecialFolder localApplicationDataFolder = Environment.SpecialFolder.LocalApplicationData;
            string localApplicationDataFolderPath = Environment.GetFolderPath(localApplicationDataFolder);
            string saveDataFilePath = localApplicationDataFolderPath + @"\OfficeWare\GameManager\" + currentUserId + @"\save-data.txt";
            JavaScriptSerializer js = new JavaScriptSerializer();
            string saveDataFileContent = File.ReadAllText(saveDataFilePath);
            SavedContent loadedContent = js.Deserialize<SavedContent>(saveDataFileContent);
            List<Game> updatedGames = loadedContent.games;
            List<FriendSettings> currentFriends = loadedContent.friends;
            Settings currentSettings = loadedContent.settings;
            List<string> currentCollections = loadedContent.collections;
            Notifications currentNotifications = loadedContent.notifications;
            List<Game> results = updatedGames.Where<Game>((Game game) =>
            {
                string gameName = game.name;
                bool isFound = gameName == name;
                return isFound;
            }).ToList();
            int countResults = results.Count;
            bool isHaveResults = countResults >= 1;
            if (isHaveResults)
            {
                Game result = results[0];
                bool isHidden = result.isHidden;
                bool toggledVisibility = !isHidden;
                result.isHidden = toggledVisibility;
                string savedContent = js.Serialize(new SavedContent
                {
                    games = updatedGames,
                    friends = currentFriends,
                    settings = currentSettings,
                    collections = currentCollections,
                    notifications = currentNotifications
                });
                File.WriteAllText(saveDataFilePath, savedContent);
            }

            GetGameCollectionItems(collection);
            GetGamesList("");
            GetHiddenGames();

        }

        public void AddExternalGameHandler(object sender, RoutedEventArgs e)
        {
            AddExternalGame();
        }

        public void AddExternalGame()
        {
            Dialogs.AddExternalGameDialog dialog = new Dialogs.AddExternalGameDialog(currentUserId);
            dialog.Closed += GetGamesListHandler;
            dialog.Show();
        }

        public void GetGamesListHandler(object sender, EventArgs e)
        {
            GetGamesList("");
            GetDownloads();
            GetScreenShots("", false);

        }

        private void OpenGameActivationHandler(object sender, RoutedEventArgs e)
        {
            OpenGameActivation();
        }

        public void OpenGameActivation()
        {
            Dialogs.ActivationGameDialog dialog = new Dialogs.ActivationGameDialog();
            dialog.Show();
        }

        private void OpenNewsHandler (object sender, MouseButtonEventArgs e)
        {
            OpenNews();
        }

        public void OpenNews ()
        {
            mainControl.SelectedIndex = 14;
            GetNews();
            AddHistoryRecord();
        }

        private void ToggleNotificationsPopupHandler(object sender, MouseButtonEventArgs e)
        {
            ToggleNotificationsPopup();
        }

        public void ToggleNotificationsPopup()
        {
            /*bool isOpen = notificationsPopup.IsOpen;
            bool toggleValue = !isOpen;
            notificationsPopup.IsOpen = toggleValue;*/
            notificationsPopup.IsOpen = true;
        }

        private void SelectAccountSettingsItemHandler(object sender, MouseButtonEventArgs e)
        {
            StackPanel item = ((StackPanel)(sender));
            object data = item.DataContext;
            string rawIndex = data.ToString();
            int index = Int32.Parse(rawIndex);
            SelectAccountSettingsItem(index);
        }

        public void SelectAccountSettingsItem (int index)
        {
            accountSettingsControl.SelectedIndex = index;
            foreach (StackPanel accountSettingsTab in accountSettingsTabs.Children)
            {
                accountSettingsTab.Background = System.Windows.Media.Brushes.Transparent;
            }
            UIElementCollection accountSettingsTabsChildren = accountSettingsTabs.Children;
            UIElement rawActiveAccountSettingsTab = accountSettingsTabsChildren[index];
            StackPanel activeAccountSettingsTab = ((StackPanel)(rawActiveAccountSettingsTab));
            activeAccountSettingsTab.Background = System.Windows.Media.Brushes.LightGray;

            bool isAbout = index == 0;
            bool isLang = index == 2;
            if (isAbout)
            {
                GetAboutAccountSettings();
            }
            else if (isLang)
            {
                GetLangSettings();
            }

        }

        private void SelectFriendSettingsItemHandler(object sender, MouseButtonEventArgs e)
        {
            StackPanel item = ((StackPanel)(sender));
            object data = item.DataContext;
            string rawIndex = data.ToString();
            int index = Int32.Parse(rawIndex);
            SelectFriendSettingsItem(index);
        }

        public void SelectFriendSettingsItem(int index)
        {
            friendsSettingsControl.SelectedIndex = index;
        }

        private void DeleteAccountHandler(object sender, MouseButtonEventArgs e)
        {
            DeleteAccount();
        }

        public void DeleteAccount()
        {
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/user/delete/?id=" + currentUserId);
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (var reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        var objText = reader.ReadToEnd();
                        ForumsListResponseInfo myobj = (ForumsListResponseInfo)js.Deserialize(objText, typeof(ForumsListResponseInfo));
                        string status = myobj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
                            Environment.SpecialFolder localApplicationDataFolder = Environment.SpecialFolder.LocalApplicationData;
                            string localApplicationDataFolderPath = Environment.GetFolderPath(localApplicationDataFolder);
                            string userPath = localApplicationDataFolderPath + @"\OfficeWare\GameManager\" + currentUserId;
                            try
                            {
                                Directory.Delete(userPath, true);
                                Logout();
                            }
                            catch (System.Net.WebException)
                            {
                                MessageBox.Show("Не удается удалить каталог пользователя", "Ошибка");
                            }
                        }
                        else
                        {
                            MessageBox.Show("Не удается удалить аккаунт", "Ошибка");
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

        public void GetAccountSettings()
        {
            string idLabelContent = @"Office ware game manager ID: " + currentUserId;
            idLabel.Text = idLabelContent;
            GetAboutAccountSettings();
            GetLangSettings();
        }

        public void GetAboutAccountSettings ()
        {
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/users/get/?id=" + currentUserId);
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (var reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        var objText = reader.ReadToEnd();
                        UserResponseInfo myobj = (UserResponseInfo)js.Deserialize(objText, typeof(UserResponseInfo));
                        string status = myobj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
                            User user = myobj.user;
                            string country = user.country;
                            int amount = user.amount;
                            bool isEmailConfirmed = user.isEmailConfirmed;
                            string accountSettingsCountryLabelContent = "Страна: " + country;
                            accountSettingsCountryLabel.Text = accountSettingsCountryLabelContent;
                            string rawAmount = amount.ToString();
                            string amountMeasure = "руб.";
                            string accountSettingsAmountLabelContent = rawAmount + " " + amountMeasure;
                            accountSettingsAmountLabel.Text = accountSettingsAmountLabelContent;
                            if (isEmailConfirmed)
                            {
                                emailConfirmedLabel.Text = "Подтвержден";
                            }
                            else
                            {
                                emailConfirmedLabel.Text = "Не подтвержден";
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

        private void RejectFriendRequestsHandler(object sender, RoutedEventArgs e)
        {
            RejectFriendRequests();
        }

        private void friendRequestsForMeBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void OpenBlackListManagementHandler(object sender, RoutedEventArgs e)
        {
            OpenBlackListManagement();
        }

        public void OpenBlackListManagement()
        {

        }

        private void OpenFriendProfilePopupHandler(object sender, RoutedEventArgs e)
        {
            OpenFriendProfilePopup();
        }

        public void OpenFriendProfilePopup()
        {
            friendProfilePopup.IsOpen = true;
        }

        private void FriendsSettingsControlItemSelectedHandler(object sender, SelectionChangedEventArgs e)
        {
            FriendsSettingsControlItemSelected();
        }

        public void FriendsSettingsControlItemSelected()
        {
            int selectedIndex = friendsSettingsControl.SelectedIndex;
            bool isGroups = selectedIndex == 10;
            bool isAddGroup = selectedIndex == 11;
            if (isGroups)
            {
                mainControl.SelectedIndex = 17;
            }
            else if (isAddGroup)
            {
                mainControl.SelectedIndex = 18;
            }
        }

        private void ToggleFriendsListManagementHandler(object sender, RoutedEventArgs e)
        {
            ToggleFriendsListManagement();
        }

        public void ToggleFriendsListManagement()
        {
            Visibility friendsListManagementVisibility = friendsListManagement.Visibility;
            bool isVisible = friendsListManagementVisibility == visible;
            if (isVisible)
            {
                friendsListManagement.Visibility = invisible;
                foreach (StackPanel onlineFriendsListItem in onlineFriendsList.Children)
                {
                    foreach (UIElement onlineFriendsListItemElement in onlineFriendsListItem.Children)
                    {
                        bool isCheckBox = onlineFriendsListItemElement is CheckBox;
                        if (isCheckBox)
                        {
                            CheckBox checkBox = onlineFriendsListItemElement as CheckBox;
                            checkBox.Visibility = invisible;
                        }
                    }
                }
                foreach (StackPanel offlineFriendsListItem in offlineFriendsList.Children)
                {
                    foreach (UIElement offlineFriendsListItemElement in offlineFriendsListItem.Children)
                    {
                        bool isCheckBox = offlineFriendsListItemElement is CheckBox;
                        if (isCheckBox)
                        {
                            CheckBox checkBox = offlineFriendsListItemElement as CheckBox;
                            checkBox.Visibility = invisible;
                        }
                    }
                }
            }
            else
            {
                friendsListManagement.Visibility = visible;
                foreach (StackPanel onlineFriendsListItem in onlineFriendsList.Children)
                {
                    foreach (UIElement onlineFriendsListItemElement in onlineFriendsListItem.Children)
                    {
                        bool isCheckBox = onlineFriendsListItemElement is CheckBox;
                        if (isCheckBox)
                        {
                            CheckBox checkBox = onlineFriendsListItemElement as CheckBox;
                            checkBox.Visibility = visible;
                        }
                    }
                }
                foreach (StackPanel offlineFriendsListItem in offlineFriendsList.Children)
                {
                    foreach (UIElement offlineFriendsListItemElement in offlineFriendsListItem.Children)
                    {
                        bool isCheckBox = offlineFriendsListItemElement is CheckBox;
                        if (isCheckBox)
                        {
                            CheckBox checkBox = offlineFriendsListItemElement as CheckBox;
                            checkBox.Visibility = visible;
                        }
                    }
                }
            }
        }

        private void RemoveFriendsHandler(object sender, RoutedEventArgs e)
        {
            RemoveFriends();
        }

        public void RemoveFriends()
        {
            foreach (StackPanel onlineFriendsListItem in onlineFriendsList.Children)
            {
                foreach (UIElement onlineFriendsListItemElement in onlineFriendsListItem.Children)
                {
                    bool isCheckBox = onlineFriendsListItemElement is CheckBox;
                    if (isCheckBox)
                    {
                        CheckBox checkBox = onlineFriendsListItemElement as CheckBox;
                        object rawIsChecked = checkBox.IsChecked;
                        bool isChecked = ((bool)(rawIsChecked));
                        if (isChecked)
                        {
                            object friendData = onlineFriendsListItem.DataContext;
                            string friendId = ((string)(friendData));
                            try
                            {
                                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/friends/remove/?id=" + currentUserId + "&friend=" + friendId);
                                webRequest.Method = "GET";
                                webRequest.UserAgent = ".NET Framework Test Client";
                                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                                {
                                    using (var reader = new StreamReader(webResponse.GetResponseStream()))
                                    {
                                        JavaScriptSerializer js = new JavaScriptSerializer();
                                        var objText = reader.ReadToEnd();
                                        UserResponseInfo myobj = (UserResponseInfo)js.Deserialize(objText, typeof(UserResponseInfo));
                                        string status = myobj.status;
                                        bool isOkStatus = status == "OK";
                                        if (isOkStatus)
                                        {
                                            Environment.SpecialFolder localApplicationDataFolder = Environment.SpecialFolder.LocalApplicationData;
                                            string localApplicationDataFolderPath = Environment.GetFolderPath(localApplicationDataFolder);
                                            string saveDataFilePath = localApplicationDataFolderPath + @"\OfficeWare\GameManager\" + currentUserId + @"\save-data.txt";
                                            js = new JavaScriptSerializer();
                                            string saveDataFileContent = File.ReadAllText(saveDataFilePath);
                                            SavedContent loadedContent = js.Deserialize<SavedContent>(saveDataFileContent);
                                            List<Game> currentGames = loadedContent.games;
                                            List<FriendSettings> updatedFriends = loadedContent.friends;
                                            Settings currentSettings = loadedContent.settings;
                                            List<string> currentCollections = loadedContent.collections;
                                            Notifications currentNotifications = loadedContent.notifications;
                                            List<FriendSettings> cachedFriends = updatedFriends.Where<FriendSettings>((FriendSettings friend) =>
                                            {
                                                return friend.id == friendId;
                                            }).ToList();
                                            int countCachedFriends = cachedFriends.Count;
                                            bool isCachedFriendsExists = countCachedFriends >= 1;
                                            if (isCachedFriendsExists)
                                            {
                                                FriendSettings cachedFriend = cachedFriends[0];
                                                updatedFriends.Remove(cachedFriend);
                                                string savedContent = js.Serialize(new SavedContent
                                                {
                                                    games = currentGames,
                                                    friends = updatedFriends,
                                                    settings = currentSettings,
                                                    collections = currentCollections,
                                                    notifications = currentNotifications
                                                });
                                                File.WriteAllText(saveDataFilePath, savedContent);
                                                GetOnlineFriends();
                                            }
                                        }
                                        else
                                        {
                                            MessageBox.Show("Не удается удалить друга", "Ошибка");
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
            foreach (StackPanel offlineFriendsListItem in offlineFriendsList.Children)
            {
                foreach (UIElement offlineFriendsListItemElement in offlineFriendsListItem.Children)
                {
                    bool isCheckBox = offlineFriendsListItemElement is CheckBox;
                    if (isCheckBox)
                    {
                        CheckBox checkBox = offlineFriendsListItemElement as CheckBox;
                        object rawIsChecked = checkBox.IsChecked;
                        bool isChecked = ((bool)(rawIsChecked));
                        if (isChecked)
                        {
                            object friendData = offlineFriendsListItem.DataContext;
                            string friendId = ((string)(friendData));
                            try
                            {
                                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/friends/remove/?id=" + currentUserId + "&friend=" + friendId);
                                webRequest.Method = "GET";
                                webRequest.UserAgent = ".NET Framework Test Client";
                                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                                {
                                    using (var reader = new StreamReader(webResponse.GetResponseStream()))
                                    {
                                        JavaScriptSerializer js = new JavaScriptSerializer();
                                        var objText = reader.ReadToEnd();
                                        UserResponseInfo myobj = (UserResponseInfo)js.Deserialize(objText, typeof(UserResponseInfo));
                                        string status = myobj.status;
                                        bool isOkStatus = status == "OK";
                                        if (isOkStatus)
                                        {
                                            Environment.SpecialFolder localApplicationDataFolder = Environment.SpecialFolder.LocalApplicationData;
                                            string localApplicationDataFolderPath = Environment.GetFolderPath(localApplicationDataFolder);
                                            string saveDataFilePath = localApplicationDataFolderPath + @"\OfficeWare\GameManager\" + currentUserId + @"\save-data.txt";
                                            js = new JavaScriptSerializer();
                                            string saveDataFileContent = File.ReadAllText(saveDataFilePath);
                                            SavedContent loadedContent = js.Deserialize<SavedContent>(saveDataFileContent);
                                            List<Game> currentGames = loadedContent.games;
                                            List<FriendSettings> updatedFriends = loadedContent.friends;
                                            Settings currentSettings = loadedContent.settings;
                                            List<string> currentCollections = loadedContent.collections;
                                            Notifications currentNotifications = loadedContent.notifications;
                                            List<FriendSettings> cachedFriends = updatedFriends.Where<FriendSettings>((FriendSettings friend) =>
                                            {
                                                return friend.id == friendId;
                                            }).ToList();
                                            int countCachedFriends = cachedFriends.Count;
                                            bool isCachedFriendsExists = countCachedFriends >= 1;
                                            if (isCachedFriendsExists)
                                            {
                                                FriendSettings cachedFriend = cachedFriends[0];
                                                updatedFriends.Remove(cachedFriend);
                                                string savedContent = js.Serialize(new SavedContent
                                                {
                                                    games = currentGames,
                                                    friends = updatedFriends,
                                                    settings = currentSettings,
                                                    collections = currentCollections,
                                                    notifications = currentNotifications
                                                });
                                                File.WriteAllText(saveDataFilePath, savedContent);
                                            }
                                        }
                                        else
                                        {
                                            MessageBox.Show("Не удается удалить друга", "Ошибка");
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
            GetFriendsSettings();
        }

        private void SelectFriendsTypeHandler(object sender, MouseButtonEventArgs e)
        {
            TextBlock typeLabel = ((TextBlock)(sender));
            object typeLabelData = typeLabel.DataContext;
            string type = typeLabelData.ToString();
            SelectFriendsType(type);
        }

        public void SelectFriendsType(string type)
        {
            bool isAll = type == "All";
            bool isNothing = type == "Nothing";
            bool isInvert = type == "Invert";
            if (isAll)
            {
                foreach (StackPanel onlineFriendsListItem in onlineFriendsList.Children)
                {
                    foreach (UIElement onlineFriendsListItemElement in onlineFriendsListItem.Children)
                    {
                        bool isCheckBox = onlineFriendsListItemElement is CheckBox;
                        if (isCheckBox)
                        {
                            CheckBox checkBox = onlineFriendsListItemElement as CheckBox;
                            checkBox.IsChecked = true;
                        }
                    }
                }
                foreach (StackPanel offlineFriendsListItem in offlineFriendsList.Children)
                {
                    foreach (UIElement offlineFriendsListItemElement in offlineFriendsListItem.Children)
                    {
                        bool isCheckBox = offlineFriendsListItemElement is CheckBox;
                        if (isCheckBox)
                        {
                            CheckBox checkBox = offlineFriendsListItemElement as CheckBox;
                            checkBox.IsChecked = true;
                        }
                    }
                }
            }
            else if (isNothing)
            {
                foreach (StackPanel onlineFriendsListItem in onlineFriendsList.Children)
                {
                    foreach (UIElement onlineFriendsListItemElement in onlineFriendsListItem.Children)
                    {
                        bool isCheckBox = onlineFriendsListItemElement is CheckBox;
                        if (isCheckBox)
                        {
                            CheckBox checkBox = onlineFriendsListItemElement as CheckBox;
                            checkBox.IsChecked = false;
                        }
                    }
                }
                foreach (StackPanel offlineFriendsListItem in offlineFriendsList.Children)
                {
                    foreach (UIElement offlineFriendsListItemElement in offlineFriendsListItem.Children)
                    {
                        bool isCheckBox = offlineFriendsListItemElement is CheckBox;
                        if (isCheckBox)
                        {
                            CheckBox checkBox = offlineFriendsListItemElement as CheckBox;
                            checkBox.IsChecked = false;
                        }
                    }
                }
            }
            else if (isInvert)
            {
                foreach (StackPanel onlineFriendsListItem in onlineFriendsList.Children)
                {
                    foreach (UIElement onlineFriendsListItemElement in onlineFriendsListItem.Children)
                    {
                        bool isCheckBox = onlineFriendsListItemElement is CheckBox;
                        if (isCheckBox)
                        {
                            CheckBox checkBox = onlineFriendsListItemElement as CheckBox;
                            object rawIsChecked = checkBox.IsChecked;
                            bool isChecked = ((bool)(rawIsChecked));
                            checkBox.IsChecked = !isChecked;
                        }
                    }
                }
                foreach (StackPanel offlineFriendsListItem in offlineFriendsList.Children)
                {
                    foreach (UIElement offlineFriendsListItemElement in offlineFriendsListItem.Children)
                    {
                        bool isCheckBox = offlineFriendsListItemElement is CheckBox;
                        if (isCheckBox)
                        {
                            CheckBox checkBox = offlineFriendsListItemElement as CheckBox;
                            object rawIsChecked = checkBox.IsChecked;
                            bool isChecked = ((bool)(rawIsChecked));
                            checkBox.IsChecked = !isChecked;
                        }
                    }
                }
            }
        }

        private void OpenDiscussionsHandler(object sender, MouseButtonEventArgs e)
        {
            OpenDiscussions();
        }

        public void OpenDiscussions()
        {
            string currentGameName = gameNameLabel.Text;
            forumsKeywordsBox.Text = currentGameName;
            mainControl.SelectedIndex = 6;
        }

        private void SearchGroupsHandler(object sender, MouseButtonEventArgs e)
        {
            SearchGroups();
        }

        public void SearchGroups()
        {
            mainControl.SelectedIndex = 17;
        }

        private void GetSearchedGroupsHandler(object sender, TextChangedEventArgs e)
        {
            GetSearchedGroups();
        }

        private void GetGroupsHandler(object sender, TextChangedEventArgs e)
        {
            GetGroups();
        }

        private void AddGroupRequestHandler(object sender, MouseButtonEventArgs e)
        {
            AddGroupRequest();
        }

        public void AddGroupRequest()
        {
            /*object mainControlData = mainControl.DataContext;
            string friendId = ((string)(mainControlData));*/
            string friendId = cachedUserProfileId;
            Dialogs.AddGroupRequestDialog dialog = new Dialogs.AddGroupRequestDialog(currentUserId, friendId, client);
            dialog.Show();
            friendProfilePopup.IsOpen = false;
        }

        private void AddGroupHandler(object sender, RoutedEventArgs e)
        {
            AddGroup();
        }

        public void AddGroup()
        {
            try
            {
                string groupNameBoxContent = groupNameBox.Text;
                int groupLangSelectorIndex = groupLangSelector.SelectedIndex;
                ItemCollection groupLangSelectorItems = groupLangSelector.Items;
                object rawGroupLangSelectorItem = groupLangSelectorItems[groupLangSelectorIndex];
                ComboBoxItem groupLangSelectorItem = ((ComboBoxItem)(rawGroupLangSelectorItem));
                object groupLangSelectorItemData = groupLangSelectorItem.DataContext;
                string groupLang = groupLangSelectorItemData.ToString();
                int groupCountrySelectorIndex = groupCountrySelector.SelectedIndex;
                ItemCollection groupCountrySelectorItems = groupCountrySelector.Items;
                object rawGroupCountrySelectorItem = groupCountrySelectorItems[groupCountrySelectorIndex];
                ComboBoxItem groupCountrySelectorItem = ((ComboBoxItem)(rawGroupCountrySelectorItem));
                object groupCountrySelectorItemData = groupCountrySelectorItem.DataContext;
                string groupCountry = groupCountrySelectorItemData.ToString();
                string groupFanPageBoxContent = groupFanPageBox.Text;
                string groupTwitchBoxContent = groupTwitchBox.Text;
                string groupYoutubeBoxContent = groupYoutubeBox.Text;
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/groups/add/?name=" + groupNameBoxContent + "&id=" + currentUserId + "&lang=" + groupLang + "&country=" + groupCountry + "&fanpage=" + groupFanPageBoxContent + "&twitch=" + groupTwitchBoxContent + "&youtube=" + groupYoutubeBoxContent);
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (var reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        var objText = reader.ReadToEnd();
                        UserResponseInfo myobj = (UserResponseInfo)js.Deserialize(objText, typeof(UserResponseInfo));
                        string status = myobj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
                            MessageBox.Show("Группа была создана", "Внимание");
                            groupNameBox.Text = "";
                            groupLangSelector.SelectedIndex = 0;
                            groupCountrySelector.SelectedIndex = 0;
                            groupFanPageBox.Text = "";
                            groupTwitchBox.Text = "";
                            groupYoutubeBox.Text = "";
                            GetFriendsSettings();
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

        private void GetGroupRequestsForMeHandler(object sender, TextChangedEventArgs e)
        {
            GetGroupRequestsForMe();
        }

        public void GetGroupRequestsForMe()
        {
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/groups/requests/all");
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (var reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        var objText = reader.ReadToEnd();
                        GroupRequestsResponseInfo myobj = (GroupRequestsResponseInfo)js.Deserialize(objText, typeof(GroupRequestsResponseInfo));
                        string status = myobj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
                            List<GroupRequest> requestsForMe = new List<GroupRequest>();
                            List<GroupRequest> requests = myobj.requests;
                            foreach (GroupRequest request in requests)
                            {
                                string recepientId = request.user;
                                bool isRequestForMe = currentUserId == recepientId;
                                if (isRequestForMe)
                                {
                                    requestsForMe.Add(request);
                                }
                            }
                            myGroupRequests.Children.Clear();
                            int countRequestsForMe = requestsForMe.Count;
                            bool isHaveRequests = countRequestsForMe >= 1;
                            if (isHaveRequests)
                            {
                                foreach (GroupRequest requestForMe in requestsForMe)
                                {
                                    string requestId = requestForMe._id;
                                    string groupId = requestForMe.group;
                                    string userId = requestForMe.user;
                                    HttpWebRequest innerWebRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/groups/get/?id=" + groupId);
                                    innerWebRequest.Method = "GET";
                                    innerWebRequest.UserAgent = ".NET Framework Test Client";
                                    using (HttpWebResponse innerWebResponse = (HttpWebResponse)innerWebRequest.GetResponse())
                                    {
                                        using (var innerReader = new StreamReader(innerWebResponse.GetResponseStream()))
                                        {
                                            js = new JavaScriptSerializer();
                                            objText = innerReader.ReadToEnd();
                                            GroupResponseInfo myInnerObj = (GroupResponseInfo)js.Deserialize(objText, typeof(GroupResponseInfo));
                                            status = myInnerObj.status;
                                            isOkStatus = status == "OK";
                                            if (isOkStatus)
                                            {
                                                Group group = myInnerObj.group;
                                                string groupName = group.name;
                                                string insensitiveCaseSenderName = groupName.ToLower();
                                                string myGroupRequestsBoxContent = myGroupRequestsBox.Text;
                                                string insensitiveCaseKeywords = myGroupRequestsBoxContent.ToLower();
                                                bool isGroupFound = insensitiveCaseSenderName.Contains(insensitiveCaseKeywords);
                                                int insensitiveCaseKeywordsLength = insensitiveCaseKeywords.Length;
                                                bool isFilterDisabled = insensitiveCaseKeywordsLength <= 0;
                                                bool isRequestMatch = isGroupFound || isFilterDisabled;
                                                if (isRequestMatch)
                                                {
                                                    StackPanel myGroupRequest = new StackPanel();
                                                    myGroupRequest.Margin = new Thickness(15);
                                                    myGroupRequest.Width = 250;
                                                    myGroupRequest.Height = 50;
                                                    myGroupRequest.Orientation = Orientation.Horizontal;
                                                    myGroupRequest.Background = System.Windows.Media.Brushes.DarkCyan;
                                                    Image myGroupRequestIcon = new Image();
                                                    myGroupRequestIcon.Width = 50;
                                                    myGroupRequestIcon.Height = 50;
                                                    myGroupRequestIcon.BeginInit();
                                                    myGroupRequestIcon.Source = new BitmapImage(new Uri(@"https://cdn2.iconfinder.com/data/icons/ios-7-icons/50/user_male-128.png"));
                                                    myGroupRequestIcon.EndInit();
                                                    myGroupRequestIcon.ImageFailed += SetDefautAvatarHandler;
                                                    myGroupRequest.Children.Add(myGroupRequestIcon);
                                                    TextBlock myGroupRequestGroupNameLabel = new TextBlock();
                                                    myGroupRequestGroupNameLabel.Margin = new Thickness(10, 0, 10, 0);
                                                    myGroupRequestGroupNameLabel.VerticalAlignment = VerticalAlignment.Center;
                                                    myGroupRequestGroupNameLabel.Text = groupName;
                                                    myGroupRequest.Children.Add(myGroupRequestGroupNameLabel);
                                                    myGroupRequests.Children.Add(myGroupRequest);
                                                    ContextMenu myGroupRequestContextMenu = new ContextMenu();
                                                    MenuItem myGroupRequestContextMenuItem = new MenuItem();
                                                    myGroupRequestContextMenuItem.Header = "Принять";
                                                    Dictionary<String, Object> myGroupRequestContextMenuItemData = new Dictionary<String, Object>();
                                                    myGroupRequestContextMenuItemData.Add("groupId", groupId);
                                                    myGroupRequestContextMenuItemData.Add("userId", userId);
                                                    myGroupRequestContextMenuItemData.Add("requestId", requestId);
                                                    myGroupRequestContextMenuItem.DataContext = myGroupRequestContextMenuItemData;
                                                    myGroupRequestContextMenuItem.Click += AcceptGroupRequestFromSettingsHandler;
                                                    myGroupRequestContextMenu.Items.Add(myGroupRequestContextMenuItem);
                                                    myGroupRequestContextMenuItem = new MenuItem();
                                                    myGroupRequestContextMenuItem.Header = "Отклонить";
                                                    myGroupRequestContextMenuItemData = new Dictionary<String, Object>();
                                                    myGroupRequestContextMenuItemData.Add("groupId", groupId);
                                                    myGroupRequestContextMenuItemData.Add("userId", userId);
                                                    myGroupRequestContextMenuItemData.Add("requestId", requestId);
                                                    myGroupRequestContextMenuItem.DataContext = myGroupRequestContextMenuItemData;
                                                    myGroupRequestContextMenuItem.Click += RejectGroupRequestFromSettingsHandler;
                                                    myGroupRequestContextMenu.Items.Add(myGroupRequestContextMenuItem);
                                                    myGroupRequest.ContextMenu = myGroupRequestContextMenu;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                TextBlock requestsNotFoundLabel = new TextBlock();
                                requestsNotFoundLabel.Margin = new Thickness(15);
                                requestsNotFoundLabel.FontSize = 18;
                                requestsNotFoundLabel.Text = "Извините, здесь ничего нет.";
                                myGroupRequests.Children.Add(requestsNotFoundLabel);
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

        private void ToggleCommentFooterHandler(object sender, TextChangedEventArgs e)
        {
            ToggleCommentFooter();
        }

        public void ToggleCommentFooter()
        {
            string userProfileCommentBoxContent = userProfileCommentBox.Text;
            int userProfileCommentBoxContentLength = userProfileCommentBoxContent.Length;
            bool isContentExists = userProfileCommentBoxContentLength >= 1;
            if (isContentExists)
            {
                userProfileCommentFooter.Visibility = visible;
            }
            else
            {
                userProfileCommentFooter.Visibility = invisible;
            }
        }

        private void SendCommentHandler(object sender, RoutedEventArgs e)
        {
            SendComment();
        }

        public void SendComment()
        {
            string userProfileCommentBoxContent = userProfileCommentBox.Text;
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/user/comments/add/?id=" + cachedUserProfileId + "&msg=" + userProfileCommentBoxContent);
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (var reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        var objText = reader.ReadToEnd();
                        UserResponseInfo myobj = (UserResponseInfo)js.Deserialize(objText, typeof(UserResponseInfo));
                        string status = myobj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
                            userProfileCommentBox.Text = "";
                            GetComments(cachedUserProfileId);
                            string eventData = currentUserId + "|" + cachedUserProfileId;
                            client.EmitAsync("user_send_comment", eventData);
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

        public void GetComments(string id)
        {
            comments.Children.Clear();
            string userProfileCommentBoxContent = userProfileCommentBox.Text;
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/user/comments/get/?id=" + id);
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (var reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        var objText = reader.ReadToEnd();
                        CommentsResponseInfo myobj = (CommentsResponseInfo)js.Deserialize(objText, typeof(CommentsResponseInfo));
                        string status = myobj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
                            List<Comment> commentsList = myobj.comments;
                            foreach (Comment commentsListItem in commentsList)
                            {
                                string msg = commentsListItem.msg;
                                StackPanel comment = new StackPanel();
                                comment.Background = System.Windows.Media.Brushes.AliceBlue;
                                comment.Margin = new Thickness(15);
                                comment.Orientation = Orientation.Horizontal;
                                Image commentAvatar = new Image();
                                commentAvatar.Margin = new Thickness(15);
                                commentAvatar.Width = 50;
                                commentAvatar.Height = 50;
                                commentAvatar.BeginInit();
                                commentAvatar.Source = new BitmapImage(new Uri(@"https://cdn2.iconfinder.com/data/icons/ios-7-icons/50/user_male-128.png"));
                                commentAvatar.EndInit();
                                comment.Children.Add(commentAvatar);
                                TextBlock commenMsgLabel = new TextBlock();
                                commenMsgLabel.Margin = new Thickness(15);
                                commenMsgLabel.Text = msg;
                                comment.Children.Add(commenMsgLabel);
                                comments.Children.Add(comment);
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

        private void PickFileHandler (object sender, RoutedEventArgs e)
        {
            PickFile();
        }

        public void PickFile ()
        {
            Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
            ofd.Title = "Выберите обложку";
            ofd.Filter = "Png documents (.png)|*.png";
            bool? res = ofd.ShowDialog();
            bool isOpened = res != false;
            if (isOpened)
            {
                string path = ofd.FileName;
                manualAttachmentExt = System.IO.Path.GetExtension(path);
                manualAttachment = ImageFileToByteArray(path);
            }
        }

        private void ShowScreenShotUploadersDialogHandler (object sender, RoutedEventArgs e)
        {
            ShowScreenShotUploadersDialog();
        }

        public void ShowScreenShotUploadersDialog ()
        {
            Dialogs.ScreenShotsUploaderDialog dialog = new Dialogs.ScreenShotsUploaderDialog(currentUserId);
            dialog.Show();
        }

        private void OpenAddReviewHandler (object sender, RoutedEventArgs e)
        {
            OpenAddReview();
        }

        public void AddReviewHandler (object sender, RoutedEventArgs e)
        {
            AddReview();
        }

        public void AddReview ()
        {
            try
            {
                object rawGame = reviewGameLabel.DataContext;
                Game game = ((Game)(rawGame));
                string hours = game.hours;
                string gameId = game.id;
                string reviewDescBoxContent = reviewDescBox.Text;
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/reviews/add/?id=" + currentUserId + @"&game=" + gameId + @"&hours=" + hours + @"&desc=" + reviewDescBoxContent);
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (var reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        var objText = reader.ReadToEnd();
                        UserResponseInfo myobj = (UserResponseInfo)js.Deserialize(objText, typeof(UserResponseInfo));
                        string status = myobj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {

                            HttpWebRequest innerWebRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/users/points/increase/?id=" + currentUserId);
                            innerWebRequest.Method = "GET";
                            innerWebRequest.UserAgent = ".NET Framework Test Client";
                            using (HttpWebResponse innerWebResponse = (HttpWebResponse)innerWebRequest.GetResponse())
                            {
                                using (var innerReader = new StreamReader(innerWebResponse.GetResponseStream()))
                                {
                                    js = new JavaScriptSerializer();
                                    objText = innerReader.ReadToEnd();
                                    UserResponseInfo myInnerObj = (UserResponseInfo)js.Deserialize(objText, typeof(UserResponseInfo));
                                    status = myInnerObj.status;
                                    isOkStatus = status == "OK";
                                    if (isOkStatus)
                                    {
                                        mainControl.SelectedIndex = 20;
                                        GetCommunityInfo();
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

        public void OpenAddReview ()
        {
            mainControl.SelectedIndex = 26;
            string name = gameNameLabel.Text;
            Environment.SpecialFolder localApplicationDataFolder = Environment.SpecialFolder.LocalApplicationData;
            string localApplicationDataFolderPath = Environment.GetFolderPath(localApplicationDataFolder);
            string saveDataFilePath = localApplicationDataFolderPath + @"\OfficeWare\GameManager\" + currentUserId + @"\save-data.txt";
            JavaScriptSerializer js = new JavaScriptSerializer();
            string saveDataFileContent = File.ReadAllText(saveDataFilePath);
            SavedContent loadedContent = js.Deserialize<SavedContent>(saveDataFileContent);
            List<Game> loadedGames = loadedContent.games;
            List<FriendSettings> currentFriends = loadedContent.friends;
            Settings currentSettings = loadedContent.settings;
            List<string> currentCollections = loadedContent.collections;
            List<Game> results = loadedGames.Where<Game>((Game game) =>
            {
                return game.name == name;
            }).ToList<Game>();
            int resultsCount = results.Count;
            bool isFound = resultsCount >= 1;
            if (isFound)
            {
                Game result = results[0];
                string gameId = result.id;
                reviewGameLabel.Text = name;
                reviewGameLabel.DataContext = result;
            }
        }

        private void OpenLabsHandler (object sender, MouseButtonEventArgs e)
        {
            OpenLabs();
        }

        public void OpenLabs ()
        {
            GetExperiments();
            mainControl.SelectedIndex = 29;
        }

        private void OpenPointsStoreHandler (object sender, MouseButtonEventArgs e)
        {
            OpenPointsStore();
        }

        public void OpenPointsStore ()
        {
            mainControl.SelectedIndex = 31;
            GetPoints();
        }

        public void GetPoints ()
        {
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/users/get/?id=" + currentUserId);
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (var reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        var objText = reader.ReadToEnd();
                        UserResponseInfo myobj = (UserResponseInfo)js.Deserialize(objText, typeof(UserResponseInfo));
                        string status = myobj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
                            User user = myobj.user;
                            int points = user.points;
                            string rawPoints = points.ToString();
                            userPointsLabel.Text = rawPoints;
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

        public void ShowStoreMenuHandler (object sender, MouseEventArgs e)
        {
            StackPanel panel = ((StackPanel)(sender));
            ShowStoreMenu(panel);
        }

        public void ShowStoreMenu (StackPanel panel)
        {
            if (panel.IsMouseOver)
            {
                storeMenuPopup.IsOpen = true;
            }
            else
            {
                storeMenuPopup.IsOpen = false;
            }
        }

        public void HideStoreMenuHandler (object sender, MouseEventArgs e)
        {
            HideStoreMenu();
        }

        public void HideStoreMenu ()
        {
            storeMenuPopup.IsOpen = false;
        }

        public void ShowNewMenuHandler(object sender, MouseEventArgs e)
        {
            StackPanel panel = ((StackPanel)(sender));
            ShowNewMenu(panel);
        }

        public void ShowNewMenu(StackPanel panel)
        {
            if (panel.IsMouseOver)
            {
                newMenuPopup.IsOpen = true;
            }
            else
            {
                newMenuPopup.IsOpen = false;
            }
        }

        public void HideNewMenuHandler(object sender, MouseEventArgs e)
        {
            StackPanel panel = ((StackPanel)(sender));
            HideNewMenu(panel);
        }

        public void HideNewMenu (StackPanel panel)
        {
            if (!panel.IsMouseOver)
            {
                newMenuPopup.IsOpen = false;
            }
        }

        public void ShowCategoriesMenuHandler(object sender, MouseEventArgs e)
        {
            StackPanel panel = ((StackPanel)(sender));
            ShowCategoriesMenu(panel);
        }

        public void ShowCategoriesMenu (StackPanel panel)
        {
            if (panel.IsMouseOver)
            {
                categoriesMenuPopup.IsOpen = true;
            }
            else
            {
                categoriesMenuPopup.IsOpen = false;
            }
        }

        public void HideCategoriesMenuHandler (object sender, MouseEventArgs e)
        {
            HideCategoriesMenu();
        }

        public void HideCategoriesMenu()
        {
            categoriesMenuPopup.IsOpen = false;
        }

        public void OpenPopularGames ()
        {
            mainControl.SelectedIndex = 32;
            GetPopularGames();
        }

        public void GetPopularGames ()
        {
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/games/get");
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (var reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        var objText = reader.ReadToEnd();
                        GamesListResponseInfo myobj = (GamesListResponseInfo)js.Deserialize(objText, typeof(GamesListResponseInfo));
                        string status = myobj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
                            List<GameResponseInfo> totalGames = myobj.games;
                            totalGames = totalGames.OrderByDescending(x => x.likes).ToList<GameResponseInfo>();
                            int gamesCount = totalGames.Count;
                            bool isGamesExists = gamesCount >= 1;
                            if (isGamesExists)
                            {
                                popularGame.Visibility = visible;
                                GameResponseInfo popularGamesItem = totalGames[0];
                                string gameName = popularGamesItem.name;
                                popularGame.BeginInit();
                                popularGame.Source = new BitmapImage(new Uri(@"http://localhost:4000/api/game/thumbnail/?name=" + gameName));
                                popularGame.EndInit();
                            }
                            else
                            {
                                popularGame.Visibility = invisible;
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

        private void ToggleWantListSettingsHandler (object sender, RoutedEventArgs e)
        {
            ToggleButton btn = ((ToggleButton)(sender));
            ToggleWantListSettings(btn);
        }

        public void ToggleWantListSettings (ToggleButton btn)
        {
            object isRawChecked = btn.IsChecked;
            bool isChecked = ((bool)(isRawChecked));
            wantListPopup.IsOpen = isChecked;
        }

        private void ToggleSortWantGamesHandler (object sender, SelectionChangedEventArgs e)
        {
            ComboBox list = ((ComboBox)(sender));
            ToggleSortWantGames(list);
        }

        public void ToggleSortWantGames (ComboBox list)
        {
            if (isAppInit)
            {
                int selectedIndex = list.SelectedIndex;
                ItemCollection listItems = list.Items;
                object rawSelectedItem = listItems[selectedIndex];
                ComboBoxItem selectedItem = ((ComboBoxItem)(rawSelectedItem));
                object rawSelectedItemContent = selectedItem.Content;
                string selectedItemContent = rawSelectedItemContent.ToString();
                list.SelectedIndex = 0;
                wantListSelectedItemLabel.Text = selectedItemContent;
            }
        }

        private void OpenPointsHistoryHandler (object sender, MouseButtonEventArgs e)
        {
            OpenPointsHistory();
        }

        public void OpenPointsHistory ()
        {
            mainControl.SelectedIndex = 35;
        }

        public void OpenSmileysHandler (object sender, RoutedEventArgs e)
        {
            OpenSmileys();
        }

        public void OpenSmileys()
        {
            smileys.Children.Clear();
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/points/items/all");
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (var reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        var objText = reader.ReadToEnd();
                        PointsStoreItemsResponseInfo myobj = (PointsStoreItemsResponseInfo)js.Deserialize(objText, typeof(PointsStoreItemsResponseInfo));
                        string status = myobj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
                            List<PointsStoreItem> items = myobj.items;
                            foreach (PointsStoreItem item in items)
                            {
                                string type = item.type;
                                bool isSmileys = type == "smileys";
                                if (isSmileys)
                                {
                                    string id = item._id;
                                    string title = item.title;
                                    string desc = item.desc;
                                    int price = item.price;
                                    string rawPrice = price.ToString();
                                    Border element = new Border();
                                    element.Background = System.Windows.Media.Brushes.LightGray;
                                    element.Width = 175;
                                    element.Height = 175;
                                    element.Margin = new Thickness(15);
                                    element.CornerRadius = new CornerRadius(5);
                                    StackPanel elementBody = new StackPanel();
                                    Image elementPhoto = new Image();
                                    elementPhoto.Width = 80;
                                    elementPhoto.Height = 80;
                                    elementPhoto.Margin = new Thickness(15, 5, 15, 5);
                                    elementPhoto.BeginInit();
                                    elementPhoto.Source = new BitmapImage(new Uri(@"http://localhost:4000/api/points/item/photo/?id=" + id));
                                    elementPhoto.EndInit();
                                    elementBody.Children.Add(elementPhoto);
                                    Separator elementSeparator = new Separator();
                                    elementSeparator.BorderBrush = System.Windows.Media.Brushes.Black;
                                    elementSeparator.BorderThickness = new Thickness(1);
                                    elementBody.Children.Add(elementSeparator);
                                    TextBlock elementTitleLabel = new TextBlock();
                                    elementTitleLabel.Text = title;
                                    elementTitleLabel.Margin = new Thickness(15, 5, 15, 5);
                                    elementBody.Children.Add(elementTitleLabel);
                                    TextBlock elementDescLabel = new TextBlock();
                                    elementDescLabel.Text = desc;
                                    elementDescLabel.Margin = new Thickness(15, 5, 15, 5);
                                    elementBody.Children.Add(elementDescLabel);
                                    StackPanel elementPrice = new StackPanel();
                                    elementPrice.HorizontalAlignment = HorizontalAlignment.Right;
                                    elementPrice.Orientation = Orientation.Horizontal;
                                    elementPrice.Margin = new Thickness(0, 5, 0, 5);
                                    PackIcon elementPriceIcon = new PackIcon();
                                    elementPriceIcon.Kind = PackIconKind.Circle;
                                    elementPriceIcon.Width = 15;
                                    elementPriceIcon.Height = 15;
                                    elementPriceIcon.Margin = new Thickness(15, 0, 15, 0);
                                    elementPrice.Children.Add(elementPriceIcon);
                                    TextBlock elementPriceLabel = new TextBlock();
                                    elementPriceLabel.Text = rawPrice;
                                    elementPriceLabel.Margin = new Thickness(15, 0, 15, 0);
                                    elementPrice.Children.Add(elementPriceLabel);
                                    elementBody.Children.Add(elementPrice);
                                    element.Child = elementBody;
                                    smileys.Children.Add(element);
                                    element.DataContext = id;
                                    element.MouseLeftButtonUp += OpenPointsStoreItemHandler;
                                }
                            }
                            pointsStoreControl.SelectedIndex = 13;
                        }
                    }
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Произошла ошибка при отправке письма", "Ошибка");
            }
        }

        public void OpenChatEffectsHandler (object sender, RoutedEventArgs e)
        {
            OpenChatEffects();
        }

        public void OpenChatEffects ()
        {
            chatEffects.Children.Clear();
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/points/items/all");
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (var reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        var objText = reader.ReadToEnd();
                        PointsStoreItemsResponseInfo myobj = (PointsStoreItemsResponseInfo)js.Deserialize(objText, typeof(PointsStoreItemsResponseInfo));
                        string status = myobj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
                            List<PointsStoreItem> items = myobj.items;
                            foreach (PointsStoreItem item in items)
                            {
                                string type = item.type;
                                bool isChatEffects = type == "chatEffects";
                                if (isChatEffects)
                                {
                                    string id = item._id;
                                    string title = item.title;
                                    string desc = item.desc;
                                    int price = item.price;
                                    string rawPrice = price.ToString();
                                    Border element = new Border();
                                    element.Background = System.Windows.Media.Brushes.LightGray;
                                    element.Width = 175;
                                    element.Height = 175;
                                    element.Margin = new Thickness(15);
                                    element.CornerRadius = new CornerRadius(5);
                                    StackPanel elementBody = new StackPanel();
                                    Image elementPhoto = new Image();
                                    elementPhoto.Width = 80;
                                    elementPhoto.Height = 80;
                                    elementPhoto.Margin = new Thickness(15, 5, 15, 5);
                                    elementPhoto.BeginInit();
                                    elementPhoto.Source = new BitmapImage(new Uri(@"http://localhost:4000/api/points/item/photo/?id=" + id));
                                    elementPhoto.EndInit();
                                    elementBody.Children.Add(elementPhoto);
                                    Separator elementSeparator = new Separator();
                                    elementSeparator.BorderBrush = System.Windows.Media.Brushes.Black;
                                    elementSeparator.BorderThickness = new Thickness(1);
                                    elementBody.Children.Add(elementSeparator);
                                    TextBlock elementTitleLabel = new TextBlock();
                                    elementTitleLabel.Text = title;
                                    elementTitleLabel.Margin = new Thickness(15, 5, 15, 5);
                                    elementBody.Children.Add(elementTitleLabel);
                                    TextBlock elementDescLabel = new TextBlock();
                                    elementDescLabel.Text = desc;
                                    elementDescLabel.Margin = new Thickness(15, 5, 15, 5);
                                    elementBody.Children.Add(elementDescLabel);
                                    StackPanel elementPrice = new StackPanel();
                                    elementPrice.HorizontalAlignment = HorizontalAlignment.Right;
                                    elementPrice.Orientation = Orientation.Horizontal;
                                    elementPrice.Margin = new Thickness(0, 5, 0, 5);
                                    PackIcon elementPriceIcon = new PackIcon();
                                    elementPriceIcon.Kind = PackIconKind.Circle;
                                    elementPriceIcon.Width = 15;
                                    elementPriceIcon.Height = 15;
                                    elementPriceIcon.Margin = new Thickness(15, 0, 15, 0);
                                    elementPrice.Children.Add(elementPriceIcon);
                                    TextBlock elementPriceLabel = new TextBlock();
                                    elementPriceLabel.Text = rawPrice;
                                    elementPriceLabel.Margin = new Thickness(15, 0, 15, 0);
                                    elementPrice.Children.Add(elementPriceLabel);
                                    elementBody.Children.Add(elementPrice);
                                    element.Child = elementBody;
                                    chatEffects.Children.Add(element);
                                    element.DataContext = id;
                                    element.MouseLeftButtonUp += OpenPointsStoreItemHandler;
                                }
                            }
                            pointsStoreControl.SelectedIndex = 12;
                        }
                    }
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Произошла ошибка при отправке письма", "Ошибка");
            }
        }

        public void OpenStickersHandler(object sender, RoutedEventArgs e)
        {
            OpenStickers();
        }

        public void OpenStickers ()
        {
            stickers.Children.Clear();
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/points/items/all");
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (var reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        var objText = reader.ReadToEnd();
                        PointsStoreItemsResponseInfo myobj = (PointsStoreItemsResponseInfo)js.Deserialize(objText, typeof(PointsStoreItemsResponseInfo));
                        string status = myobj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
                            List<PointsStoreItem> items = myobj.items;
                            foreach (PointsStoreItem item in items)
                            {
                                string type = item.type;
                                bool isSticker = type == "sticker";
                                if (isSticker)
                                {
                                    string id = item._id;
                                    string title = item.title;
                                    string desc = item.desc;
                                    int price = item.price;
                                    string rawPrice = price.ToString();
                                    Border element = new Border();
                                    element.Background = System.Windows.Media.Brushes.LightGray;
                                    element.Width = 175;
                                    element.Height = 175;
                                    element.Margin = new Thickness(15);
                                    element.CornerRadius = new CornerRadius(5);
                                    StackPanel elementBody = new StackPanel();
                                    Image elementPhoto = new Image();
                                    elementPhoto.Width = 80;
                                    elementPhoto.Height = 80;
                                    elementPhoto.Margin = new Thickness(15, 5, 15, 5);
                                    elementPhoto.BeginInit();
                                    elementPhoto.Source = new BitmapImage(new Uri(@"http://localhost:4000/api/points/item/photo/?id=" + id));
                                    elementPhoto.EndInit();
                                    elementBody.Children.Add(elementPhoto);
                                    Separator elementSeparator = new Separator();
                                    elementSeparator.BorderBrush = System.Windows.Media.Brushes.Black;
                                    elementSeparator.BorderThickness = new Thickness(1);
                                    elementBody.Children.Add(elementSeparator);
                                    TextBlock elementTitleLabel = new TextBlock();
                                    elementTitleLabel.Text = title;
                                    elementTitleLabel.Margin = new Thickness(15, 5, 15, 5);
                                    elementBody.Children.Add(elementTitleLabel);
                                    TextBlock elementDescLabel = new TextBlock();
                                    elementDescLabel.Text = desc;
                                    elementDescLabel.Margin = new Thickness(15, 5, 15, 5);
                                    elementBody.Children.Add(elementDescLabel);
                                    StackPanel elementPrice = new StackPanel();
                                    elementPrice.HorizontalAlignment = HorizontalAlignment.Right;
                                    elementPrice.Orientation = Orientation.Horizontal;
                                    elementPrice.Margin = new Thickness(0, 5, 0, 5);
                                    PackIcon elementPriceIcon = new PackIcon();
                                    elementPriceIcon.Kind = PackIconKind.Circle;
                                    elementPriceIcon.Width = 15;
                                    elementPriceIcon.Height = 15;
                                    elementPriceIcon.Margin = new Thickness(15, 0, 15, 0);
                                    elementPrice.Children.Add(elementPriceIcon);
                                    TextBlock elementPriceLabel = new TextBlock();
                                    elementPriceLabel.Text = rawPrice;
                                    elementPriceLabel.Margin = new Thickness(15, 0, 15, 0);
                                    elementPrice.Children.Add(elementPriceLabel);
                                    elementBody.Children.Add(elementPrice);
                                    element.Child = elementBody;
                                    stickers.Children.Add(element);
                                    element.DataContext = id;
                                    element.MouseLeftButtonUp += OpenPointsStoreItemHandler;
                                }
                            }
                            pointsStoreControl.SelectedIndex = 11;
                        }
                    }
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Произошла ошибка при отправке письма", "Ошибка");
            }
        }

        public void OpenShowCaseProfileHandler (object sender, RoutedEventArgs e)
        {
            OpenShowCaseProfile();
        }

        public void OpenShowCaseProfile ()
        {
            profileShowCases.Children.Clear();
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/points/items/all");
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (var reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        var objText = reader.ReadToEnd();
                        PointsStoreItemsResponseInfo myobj = (PointsStoreItemsResponseInfo)js.Deserialize(objText, typeof(PointsStoreItemsResponseInfo));
                        string status = myobj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
                            List<PointsStoreItem> items = myobj.items;
                            foreach (PointsStoreItem item in items)
                            {
                                string type = item.type;
                                bool isProfileShowCases = type == "profileShowCases";
                                if (isProfileShowCases)
                                {
                                    string id = item._id;
                                    string title = item.title;
                                    string desc = item.desc;
                                    int price = item.price;
                                    string rawPrice = price.ToString();
                                    Border element = new Border();
                                    element.Background = System.Windows.Media.Brushes.LightGray;
                                    element.Width = 175;
                                    element.Height = 175;
                                    element.Margin = new Thickness(15);
                                    element.CornerRadius = new CornerRadius(5);
                                    StackPanel elementBody = new StackPanel();
                                    Image elementPhoto = new Image();
                                    elementPhoto.Width = 80;
                                    elementPhoto.Height = 80;
                                    elementPhoto.Margin = new Thickness(15, 5, 15, 5);
                                    elementPhoto.BeginInit();
                                    elementPhoto.Source = new BitmapImage(new Uri(@"http://localhost:4000/api/points/item/photo/?id=" + id));
                                    elementPhoto.EndInit();
                                    elementBody.Children.Add(elementPhoto);
                                    Separator elementSeparator = new Separator();
                                    elementSeparator.BorderBrush = System.Windows.Media.Brushes.Black;
                                    elementSeparator.BorderThickness = new Thickness(1);
                                    elementBody.Children.Add(elementSeparator);
                                    TextBlock elementTitleLabel = new TextBlock();
                                    elementTitleLabel.Text = title;
                                    elementTitleLabel.Margin = new Thickness(15, 5, 15, 5);
                                    elementBody.Children.Add(elementTitleLabel);
                                    TextBlock elementDescLabel = new TextBlock();
                                    elementDescLabel.Text = desc;
                                    elementDescLabel.Margin = new Thickness(15, 5, 15, 5);
                                    elementBody.Children.Add(elementDescLabel);
                                    StackPanel elementPrice = new StackPanel();
                                    elementPrice.HorizontalAlignment = HorizontalAlignment.Right;
                                    elementPrice.Orientation = Orientation.Horizontal;
                                    elementPrice.Margin = new Thickness(0, 5, 0, 5);
                                    PackIcon elementPriceIcon = new PackIcon();
                                    elementPriceIcon.Kind = PackIconKind.Circle;
                                    elementPriceIcon.Width = 15;
                                    elementPriceIcon.Height = 15;
                                    elementPriceIcon.Margin = new Thickness(15, 0, 15, 0);
                                    elementPrice.Children.Add(elementPriceIcon);
                                    TextBlock elementPriceLabel = new TextBlock();
                                    elementPriceLabel.Text = rawPrice;
                                    elementPriceLabel.Margin = new Thickness(15, 0, 15, 0);
                                    elementPrice.Children.Add(elementPriceLabel);
                                    elementBody.Children.Add(elementPrice);
                                    element.Child = elementBody;
                                    profileShowCases.Children.Add(element);
                                    element.DataContext = id;
                                    element.MouseLeftButtonUp += OpenPointsStoreItemHandler;
                                }
                            }
                            pointsStoreControl.SelectedIndex = 10;
                        }
                    }
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Произошла ошибка при отправке письма", "Ошибка");
            }
        }

        public void OpenArtistProfilesHandler(object sender, RoutedEventArgs e)
        {
            OpenArtistProfiles();
        }

        public void OpenArtistProfiles ()
        {
            artistProfiles.Children.Clear();
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/points/items/all");
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (var reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        var objText = reader.ReadToEnd();
                        PointsStoreItemsResponseInfo myobj = (PointsStoreItemsResponseInfo)js.Deserialize(objText, typeof(PointsStoreItemsResponseInfo));
                        string status = myobj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
                            List<PointsStoreItem> items = myobj.items;
                            foreach (PointsStoreItem item in items)
                            {
                                string type = item.type;
                                bool isArtistProfiles = type == "artistProfiles";
                                if (isArtistProfiles)
                                {
                                    string id = item._id;
                                    string title = item.title;
                                    string desc = item.desc;
                                    int price = item.price;
                                    string rawPrice = price.ToString();
                                    Border element = new Border();
                                    element.Background = System.Windows.Media.Brushes.LightGray;
                                    element.Width = 175;
                                    element.Height = 175;
                                    element.Margin = new Thickness(15);
                                    element.CornerRadius = new CornerRadius(5);
                                    StackPanel elementBody = new StackPanel();
                                    Image elementPhoto = new Image();
                                    elementPhoto.Width = 80;
                                    elementPhoto.Height = 80;
                                    elementPhoto.Margin = new Thickness(15, 5, 15, 5);
                                    elementPhoto.BeginInit();
                                    elementPhoto.Source = new BitmapImage(new Uri(@"http://localhost:4000/api/points/item/photo/?id=" + id));
                                    elementPhoto.EndInit();
                                    elementBody.Children.Add(elementPhoto);
                                    Separator elementSeparator = new Separator();
                                    elementSeparator.BorderBrush = System.Windows.Media.Brushes.Black;
                                    elementSeparator.BorderThickness = new Thickness(1);
                                    elementBody.Children.Add(elementSeparator);
                                    TextBlock elementTitleLabel = new TextBlock();
                                    elementTitleLabel.Text = title;
                                    elementTitleLabel.Margin = new Thickness(15, 5, 15, 5);
                                    elementBody.Children.Add(elementTitleLabel);
                                    TextBlock elementDescLabel = new TextBlock();
                                    elementDescLabel.Text = desc;
                                    elementDescLabel.Margin = new Thickness(15, 5, 15, 5);
                                    elementBody.Children.Add(elementDescLabel);
                                    StackPanel elementPrice = new StackPanel();
                                    elementPrice.HorizontalAlignment = HorizontalAlignment.Right;
                                    elementPrice.Orientation = Orientation.Horizontal;
                                    elementPrice.Margin = new Thickness(0, 5, 0, 5);
                                    PackIcon elementPriceIcon = new PackIcon();
                                    elementPriceIcon.Kind = PackIconKind.Circle;
                                    elementPriceIcon.Width = 15;
                                    elementPriceIcon.Height = 15;
                                    elementPriceIcon.Margin = new Thickness(15, 0, 15, 0);
                                    elementPrice.Children.Add(elementPriceIcon);
                                    TextBlock elementPriceLabel = new TextBlock();
                                    elementPriceLabel.Text = rawPrice;
                                    elementPriceLabel.Margin = new Thickness(15, 0, 15, 0);
                                    elementPrice.Children.Add(elementPriceLabel);
                                    elementBody.Children.Add(elementPrice);
                                    element.Child = elementBody;
                                    artistProfiles.Children.Add(element);
                                    element.DataContext = id;
                                    element.MouseLeftButtonUp += OpenPointsStoreItemHandler;
                                }
                            }
                            pointsStoreControl.SelectedIndex = 9;
                        }
                    }
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Произошла ошибка при отправке письма", "Ошибка");
            }
        }

        public void OpenGameProfilesHandler (object sender, RoutedEventArgs e)
        {
            OpenGameProfiles();
        }

        public void OpenGameProfiles()
        {
            gameProfiles.Children.Clear();
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/points/items/all");
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (var reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        var objText = reader.ReadToEnd();
                        PointsStoreItemsResponseInfo myobj = (PointsStoreItemsResponseInfo)js.Deserialize(objText, typeof(PointsStoreItemsResponseInfo));
                        string status = myobj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
                            List<PointsStoreItem> items = myobj.items;
                            foreach (PointsStoreItem item in items)
                            {
                                string type = item.type;
                                bool isGameProfiles = type == "gameProfiles";
                                if (isGameProfiles)
                                {
                                    string id = item._id;
                                    string title = item.title;
                                    string desc = item.desc;
                                    int price = item.price;
                                    string rawPrice = price.ToString();
                                    Border element = new Border();
                                    element.Background = System.Windows.Media.Brushes.LightGray;
                                    element.Width = 175;
                                    element.Height = 175;
                                    element.Margin = new Thickness(15);
                                    element.CornerRadius = new CornerRadius(5);
                                    StackPanel elementBody = new StackPanel();
                                    Image elementPhoto = new Image();
                                    elementPhoto.Width = 80;
                                    elementPhoto.Height = 80;
                                    elementPhoto.Margin = new Thickness(15, 5, 15, 5);
                                    elementPhoto.BeginInit();
                                    elementPhoto.Source = new BitmapImage(new Uri(@"http://localhost:4000/api/points/item/photo/?id=" + id));
                                    elementPhoto.EndInit();
                                    elementBody.Children.Add(elementPhoto);
                                    Separator elementSeparator = new Separator();
                                    elementSeparator.BorderBrush = System.Windows.Media.Brushes.Black;
                                    elementSeparator.BorderThickness = new Thickness(1);
                                    elementBody.Children.Add(elementSeparator);
                                    TextBlock elementTitleLabel = new TextBlock();
                                    elementTitleLabel.Text = title;
                                    elementTitleLabel.Margin = new Thickness(15, 5, 15, 5);
                                    elementBody.Children.Add(elementTitleLabel);
                                    TextBlock elementDescLabel = new TextBlock();
                                    elementDescLabel.Text = desc;
                                    elementDescLabel.Margin = new Thickness(15, 5, 15, 5);
                                    elementBody.Children.Add(elementDescLabel);
                                    StackPanel elementPrice = new StackPanel();
                                    elementPrice.HorizontalAlignment = HorizontalAlignment.Right;
                                    elementPrice.Orientation = Orientation.Horizontal;
                                    elementPrice.Margin = new Thickness(0, 5, 0, 5);
                                    PackIcon elementPriceIcon = new PackIcon();
                                    elementPriceIcon.Kind = PackIconKind.Circle;
                                    elementPriceIcon.Width = 15;
                                    elementPriceIcon.Height = 15;
                                    elementPriceIcon.Margin = new Thickness(15, 0, 15, 0);
                                    elementPrice.Children.Add(elementPriceIcon);
                                    TextBlock elementPriceLabel = new TextBlock();
                                    elementPriceLabel.Text = rawPrice;
                                    elementPriceLabel.Margin = new Thickness(15, 0, 15, 0);
                                    elementPrice.Children.Add(elementPriceLabel);
                                    elementBody.Children.Add(elementPrice);
                                    element.Child = elementBody;
                                    gameProfiles.Children.Add(element);
                                    element.DataContext = id;
                                    element.MouseLeftButtonUp += OpenPointsStoreItemHandler;
                                }
                            }
                            pointsStoreControl.SelectedIndex = 8;
                        }
                    }
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Произошла ошибка при отправке письма", "Ошибка");
            }

        }

        public void OpenSeasonIconHandler (object sender, RoutedEventArgs e)
        {
            OpenSeasonIcon();
        }

        public void OpenSeasonIcon()
        {
            seasonPoints.Children.Clear();
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/points/items/all");
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (var reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        var objText = reader.ReadToEnd();
                        PointsStoreItemsResponseInfo myobj = (PointsStoreItemsResponseInfo)js.Deserialize(objText, typeof(PointsStoreItemsResponseInfo));
                        string status = myobj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
                            List<PointsStoreItem> items = myobj.items;
                            foreach (PointsStoreItem item in items)
                            {
                                string type = item.type;
                                bool isSeasonIcon = type == "seasonIcon";
                                if (isSeasonIcon)
                                {
                                    string id = item._id;
                                    string title = item.title;
                                    string desc = item.desc;
                                    int price = item.price;
                                    string rawPrice = price.ToString();
                                    Border element = new Border();
                                    element.Background = System.Windows.Media.Brushes.LightGray;
                                    element.Width = 175;
                                    element.Height = 175;
                                    element.Margin = new Thickness(15);
                                    element.CornerRadius = new CornerRadius(5);
                                    StackPanel elementBody = new StackPanel();
                                    Image elementPhoto = new Image();
                                    elementPhoto.Width = 80;
                                    elementPhoto.Height = 80;
                                    elementPhoto.Margin = new Thickness(15, 5, 15, 5);
                                    elementPhoto.BeginInit();
                                    elementPhoto.Source = new BitmapImage(new Uri(@"http://localhost:4000/api/points/item/photo/?id=" + id));
                                    elementPhoto.EndInit();
                                    elementBody.Children.Add(elementPhoto);
                                    Separator elementSeparator = new Separator();
                                    elementSeparator.BorderBrush = System.Windows.Media.Brushes.Black;
                                    elementSeparator.BorderThickness = new Thickness(1);
                                    elementBody.Children.Add(elementSeparator);
                                    TextBlock elementTitleLabel = new TextBlock();
                                    elementTitleLabel.Text = title;
                                    elementTitleLabel.Margin = new Thickness(15, 5, 15, 5);
                                    elementBody.Children.Add(elementTitleLabel);
                                    TextBlock elementDescLabel = new TextBlock();
                                    elementDescLabel.Text = desc;
                                    elementDescLabel.Margin = new Thickness(15, 5, 15, 5);
                                    elementBody.Children.Add(elementDescLabel);
                                    StackPanel elementPrice = new StackPanel();
                                    elementPrice.HorizontalAlignment = HorizontalAlignment.Right;
                                    elementPrice.Orientation = Orientation.Horizontal;
                                    elementPrice.Margin = new Thickness(0, 5, 0, 5);
                                    PackIcon elementPriceIcon = new PackIcon();
                                    elementPriceIcon.Kind = PackIconKind.Circle;
                                    elementPriceIcon.Width = 15;
                                    elementPriceIcon.Height = 15;
                                    elementPriceIcon.Margin = new Thickness(15, 0, 15, 0);
                                    elementPrice.Children.Add(elementPriceIcon);
                                    TextBlock elementPriceLabel = new TextBlock();
                                    elementPriceLabel.Text = rawPrice;
                                    elementPriceLabel.Margin = new Thickness(15, 0, 15, 0);
                                    elementPrice.Children.Add(elementPriceLabel);
                                    elementBody.Children.Add(elementPrice);
                                    element.Child = elementBody;
                                    seasonPoints.Children.Add(element);
                                    element.DataContext = id;
                                    element.MouseLeftButtonUp += OpenPointsStoreItemHandler;
                                }
                            }
                            pointsStoreControl.SelectedIndex = 7;
                        }
                    }
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Произошла ошибка при отправке письма", "Ошибка");
            }
        }

        public void OpenBackgroundsHandler(object sender, RoutedEventArgs e)
        {
            OpenBackgrounds();
        }

        public void OpenBackgrounds()
        {
            backgrounds.Children.Clear();
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/points/items/all");
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (var reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        var objText = reader.ReadToEnd();
                        PointsStoreItemsResponseInfo myobj = (PointsStoreItemsResponseInfo)js.Deserialize(objText, typeof(PointsStoreItemsResponseInfo));
                        string status = myobj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
                            List<PointsStoreItem> items = myobj.items;
                            foreach (PointsStoreItem item in items)
                            {
                                string type = item.type;
                                bool isBackground = type == "background";
                                if (isBackground)
                                {
                                    string id = item._id;
                                    string title = item.title;
                                    string desc = item.desc;
                                    int price = item.price;
                                    string rawPrice = price.ToString();
                                    Border element = new Border();
                                    element.Background = System.Windows.Media.Brushes.LightGray;
                                    element.Width = 175;
                                    element.Height = 175;
                                    element.Margin = new Thickness(15);
                                    element.CornerRadius = new CornerRadius(5);
                                    StackPanel elementBody = new StackPanel();
                                    Image elementPhoto = new Image();
                                    elementPhoto.Width = 80;
                                    elementPhoto.Height = 80;
                                    elementPhoto.Margin = new Thickness(15, 5, 15, 5);
                                    elementPhoto.BeginInit();
                                    elementPhoto.Source = new BitmapImage(new Uri(@"http://localhost:4000/api/points/item/photo/?id=" + id));
                                    elementPhoto.EndInit();
                                    elementBody.Children.Add(elementPhoto);
                                    Separator elementSeparator = new Separator();
                                    elementSeparator.BorderBrush = System.Windows.Media.Brushes.Black;
                                    elementSeparator.BorderThickness = new Thickness(1);
                                    elementBody.Children.Add(elementSeparator);
                                    TextBlock elementTitleLabel = new TextBlock();
                                    elementTitleLabel.Text = title;
                                    elementTitleLabel.Margin = new Thickness(15, 5, 15, 5);
                                    elementBody.Children.Add(elementTitleLabel);
                                    TextBlock elementDescLabel = new TextBlock();
                                    elementDescLabel.Text = desc;
                                    elementDescLabel.Margin = new Thickness(15, 5, 15, 5);
                                    elementBody.Children.Add(elementDescLabel);
                                    StackPanel elementPrice = new StackPanel();
                                    elementPrice.HorizontalAlignment = HorizontalAlignment.Right;
                                    elementPrice.Orientation = Orientation.Horizontal;
                                    elementPrice.Margin = new Thickness(0, 5, 0, 5);
                                    PackIcon elementPriceIcon = new PackIcon();
                                    elementPriceIcon.Kind = PackIconKind.Circle;
                                    elementPriceIcon.Width = 15;
                                    elementPriceIcon.Height = 15;
                                    elementPriceIcon.Margin = new Thickness(15, 0, 15, 0);
                                    elementPrice.Children.Add(elementPriceIcon);
                                    TextBlock elementPriceLabel = new TextBlock();
                                    elementPriceLabel.Text = rawPrice;
                                    elementPriceLabel.Margin = new Thickness(15, 0, 15, 0);
                                    elementPrice.Children.Add(elementPriceLabel);
                                    elementBody.Children.Add(elementPrice);
                                    element.Child = elementBody;
                                    backgrounds.Children.Add(element);
                                    element.DataContext = id;
                                    element.MouseLeftButtonUp += OpenPointsStoreItemHandler;
                                }
                            }
                            pointsStoreControl.SelectedIndex = 6;
                        }
                    }
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Произошла ошибка при отправке письма", "Ошибка");
            }
        }

        public void OpenAvatarHandler (object sender, RoutedEventArgs e)
        {
            OpenAvatar();
        }

        public void OpenAvatar()
        {
            accountItems.Children.Clear();
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/points/items/all");
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (var reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        var objText = reader.ReadToEnd();
                        PointsStoreItemsResponseInfo myobj = (PointsStoreItemsResponseInfo)js.Deserialize(objText, typeof(PointsStoreItemsResponseInfo));
                        string status = myobj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
                            List<PointsStoreItem> items = myobj.items;
                            foreach (PointsStoreItem item in items)
                            {
                                string type = item.type;
                                bool isAvatar = type == "avatar";
                                if (isAvatar)
                                {
                                    string id = item._id;
                                    string title = item.title;
                                    string desc = item.desc;
                                    int price = item.price;
                                    string rawPrice = price.ToString();
                                    Border element = new Border();
                                    element.Background = System.Windows.Media.Brushes.LightGray;
                                    element.Width = 175;
                                    element.Height = 175;
                                    element.Margin = new Thickness(15);
                                    element.CornerRadius = new CornerRadius(5);
                                    StackPanel elementBody = new StackPanel();
                                    Image elementPhoto = new Image();
                                    elementPhoto.Width = 80;
                                    elementPhoto.Height = 80;
                                    elementPhoto.Margin = new Thickness(15, 5, 15, 5);
                                    elementPhoto.BeginInit();
                                    elementPhoto.Source = new BitmapImage(new Uri(@"http://localhost:4000/api/points/item/photo/?id=" + id));
                                    elementPhoto.EndInit();
                                    elementBody.Children.Add(elementPhoto);
                                    Separator elementSeparator = new Separator();
                                    elementSeparator.BorderBrush = System.Windows.Media.Brushes.Black;
                                    elementSeparator.BorderThickness = new Thickness(1);
                                    elementBody.Children.Add(elementSeparator);
                                    TextBlock elementTitleLabel = new TextBlock();
                                    elementTitleLabel.Text = title;
                                    elementTitleLabel.Margin = new Thickness(15, 5, 15, 5);
                                    elementBody.Children.Add(elementTitleLabel);
                                    TextBlock elementDescLabel = new TextBlock();
                                    elementDescLabel.Text = desc;
                                    elementDescLabel.Margin = new Thickness(15, 5, 15, 5);
                                    elementBody.Children.Add(elementDescLabel);
                                    StackPanel elementPrice = new StackPanel();
                                    elementPrice.HorizontalAlignment = HorizontalAlignment.Right;
                                    elementPrice.Orientation = Orientation.Horizontal;
                                    elementPrice.Margin = new Thickness(0, 5, 0, 5);
                                    PackIcon elementPriceIcon = new PackIcon();
                                    elementPriceIcon.Kind = PackIconKind.Circle;
                                    elementPriceIcon.Width = 15;
                                    elementPriceIcon.Height = 15;
                                    elementPriceIcon.Margin = new Thickness(15, 0, 15, 0);
                                    elementPrice.Children.Add(elementPriceIcon);
                                    TextBlock elementPriceLabel = new TextBlock();
                                    elementPriceLabel.Text = rawPrice;
                                    elementPriceLabel.Margin = new Thickness(15, 0, 15, 0);
                                    elementPrice.Children.Add(elementPriceLabel);
                                    elementBody.Children.Add(elementPrice);
                                    element.Child = elementBody;
                                    accountItems.Children.Add(element);
                                    element.DataContext = id;
                                    element.MouseLeftButtonUp += OpenPointsStoreItemHandler;
                                }
                            }
                            pointsStoreControl.SelectedIndex = 5;
                        }
                    }
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Произошла ошибка при отправке письма", "Ошибка");
            }
        }

        public void OpenPointsStoreItemHandler (object sender, RoutedEventArgs e)
        {
            Border element = ((Border)(sender));
            object elementData = element.DataContext;
            string id = ((string)(elementData));
            OpenPointsStoreItem(id);
        }

        public void OpenPointsStoreItem (string id)
        {

            pointsStorePopup.Placement = PlacementMode.Custom;
            pointsStorePopup.CustomPopupPlacementCallback = new CustomPopupPlacementCallback(PointsStoreItemsPopupPlacementHandler);
            pointsStorePopup.PlacementTarget = this;

            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/points/items/get/?id=" + id);
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (var reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        var objText = reader.ReadToEnd();
                        PointsStoreItemResponseInfo myobj = (PointsStoreItemResponseInfo)js.Deserialize(objText, typeof(PointsStoreItemResponseInfo));
                        string status = myobj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
                            PointsStoreItem item = myobj.item;
                            string title = item.title;
                            string desc = item.desc;
                            string type = item.type;
                            int price = item.price;
                            string rawPrice = price.ToString();
                            pointsStorePopupTitleLabel.Text = title;
                            pointsStorePopupDescLabel.Text = desc;
                            HttpWebRequest innerWebRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/users/get?id=" + currentUserId);
                            innerWebRequest.Method = "GET";
                            innerWebRequest.UserAgent = ".NET Framework Test Client";
                            using (HttpWebResponse innerWebResponse = (HttpWebResponse)innerWebRequest.GetResponse())
                            {
                                using (var innerReader = new StreamReader(innerWebResponse.GetResponseStream()))
                                {
                                    js = new JavaScriptSerializer();
                                    objText = innerReader.ReadToEnd();
                                    UserResponseInfo myInnerObj = (UserResponseInfo)js.Deserialize(objText, typeof(UserResponseInfo));
                                    status = myInnerObj.status;
                                    isOkStatus = status == "OK";
                                    if (isOkStatus)
                                    {
                                        User user = myInnerObj.user;
                                        int points = user.points;
                                        string rawPoints = points.ToString();
                                        pointsStorePopupCountLabel.Text = rawPoints;
                                        pointsStorePopupPreview.BeginInit();
                                        pointsStorePopupPreview.Source = new BitmapImage(new Uri(@"http://localhost:4000/api/points/item/photo?id=" + id));
                                        pointsStorePopupPreview.EndInit();
                                        string pointsStorePopupFooterLabelContent = "";
                                        bool isAvatar = type == "avatar";
                                        string newLine = Environment.NewLine;
                                        if (isAvatar)
                                        {
                                            pointsStorePopupFooterLabelContent = "Приобретите этот анимированный аватар за очки Steam. Анимация всегда" + newLine + "воспроизводится в ваших профиле и мини-профиле, а также" + newLine + "ненадолго отображается при смене статуса или отправке сообщения в чате Steam.";
                                        }
                                        pointsStorePopupFooterLabel.Text = pointsStorePopupFooterLabelContent;
                                        string pointsStorePopupNotPayedLabelContent = "";
                                        bool isNotPoints = points < price;
                                        if (isNotPoints)
                                        {
                                            buyPointsStoreItemBtn.Content = @"Как получить очки";
                                            buyPointsStoreItemBtn.DataContext = "not points";
                                            int leftPoints = price - points;
                                            string rawLeftPoints = leftPoints.ToString();
                                            pointsStorePopupNotPayedLabelContent = "Для получения этого предмета не хватает " + rawLeftPoints + " очк.";
                                        }
                                        else
                                        {
                                            buyPointsStoreItemBtn.Content = @"Купить";
                                            buyPointsStoreItemBtn.DataContext = "have points";
                                            string measure = "очков";
                                            pointsStorePopupNotPayedLabelContent = "При покупке предмета будут потрачены " + rawPrice + " " + measure;
                                        }
                                        pointsStorePopupNotPayedLabel.Text = pointsStorePopupNotPayedLabelContent;
                                        pointsStorePopup.IsOpen = true;
                                        buyPointsStoreItemBtn.IsEnabled = true;
                                        pointsStorePopup.DataContext = id;
                                        HttpWebRequest nestedWebRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/points/items/relations/all");
                                        nestedWebRequest.Method = "GET";
                                        nestedWebRequest.UserAgent = ".NET Framework Test Client";
                                        using (HttpWebResponse nestedWebResponse = (HttpWebResponse)nestedWebRequest.GetResponse())
                                        {
                                            using (var nestedReader = new StreamReader(nestedWebResponse.GetResponseStream()))
                                            {
                                                js = new JavaScriptSerializer();
                                                objText = nestedReader.ReadToEnd();
                                                PointsStoreItemRelationsResponseInfo myNestedObj = (PointsStoreItemRelationsResponseInfo)js.Deserialize(objText, typeof(PointsStoreItemRelationsResponseInfo));
                                                status = myobj.status;
                                                isOkStatus = status == "OK";
                                                if (isOkStatus)
                                                {
                                                    List<PointsStoreItemRelation> relations = myNestedObj.relations;
                                                    List<PointsStoreItemRelation> myPayedRelations = relations.Where<PointsStoreItemRelation>((PointsStoreItemRelation relation) =>
                                                    {
                                                        string userId = relation.user;
                                                        string itemId = relation.item;
                                                        bool isMyItem = currentUserId == userId;
                                                        bool isCurrentItem = id == itemId;
                                                        bool isMyPayedItem = isCurrentItem && isMyItem;
                                                        return isMyPayedItem;
                                                    }).ToList<PointsStoreItemRelation>();
                                                    int myPayedRelationsCount = myPayedRelations.Count;
                                                    bool isPayed = myPayedRelationsCount >= 1;
                                                    if (isPayed)
                                                    {
                                                        buyPointsStoreItemBtn.IsEnabled = false;
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
            catch (Exception)
            {
                MessageBox.Show("Произошла ошибка при отправке письма", "Ошибка");
            }
        }

        public void OpenCommunityRewardsHandler(object sender, RoutedEventArgs e)
        {
            OpenCommunityRewards();
        }

        public void OpenCommunityRewards()
        {
            communityRewards.Children.Clear();
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/points/items/all");
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (var reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        var objText = reader.ReadToEnd();
                        PointsStoreItemsResponseInfo myobj = (PointsStoreItemsResponseInfo)js.Deserialize(objText, typeof(PointsStoreItemsResponseInfo));
                        string status = myobj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
                            List<PointsStoreItem> items = myobj.items;
                            foreach (PointsStoreItem item in items)
                            {
                                string type = item.type;
                                bool isAvatar = type == "avatar";
                                if (isAvatar)
                                {
                                    string id = item._id;
                                    string title = item.title;
                                    string desc = item.desc;
                                    int price = item.price;
                                    string rawPrice = price.ToString();
                                    Border element = new Border();
                                    element.Background = System.Windows.Media.Brushes.LightGray;
                                    element.Width = 175;
                                    element.Height = 175;
                                    element.Margin = new Thickness(15);
                                    element.CornerRadius = new CornerRadius(5);
                                    StackPanel elementBody = new StackPanel();
                                    Image elementPhoto = new Image();
                                    elementPhoto.Width = 80;
                                    elementPhoto.Height = 80;
                                    elementPhoto.Margin = new Thickness(15, 5, 15, 5);
                                    elementPhoto.BeginInit();
                                    elementPhoto.Source = new BitmapImage(new Uri(@"http://localhost:4000/api/points/item/photo/?id=" + id));
                                    elementPhoto.EndInit();
                                    elementBody.Children.Add(elementPhoto);
                                    Separator elementSeparator = new Separator();
                                    elementSeparator.BorderBrush = System.Windows.Media.Brushes.Black;
                                    elementSeparator.BorderThickness = new Thickness(1);
                                    elementBody.Children.Add(elementSeparator);
                                    TextBlock elementTitleLabel = new TextBlock();
                                    elementTitleLabel.Text = title;
                                    elementTitleLabel.Margin = new Thickness(15, 5, 15, 5);
                                    elementBody.Children.Add(elementTitleLabel);
                                    TextBlock elementDescLabel = new TextBlock();
                                    elementDescLabel.Text = desc;
                                    elementDescLabel.Margin = new Thickness(15, 5, 15, 5);
                                    elementBody.Children.Add(elementDescLabel);
                                    StackPanel elementPrice = new StackPanel();
                                    elementPrice.HorizontalAlignment = HorizontalAlignment.Right;
                                    elementPrice.Orientation = Orientation.Horizontal;
                                    elementPrice.Margin = new Thickness(0, 5, 0, 5);
                                    PackIcon elementPriceIcon = new PackIcon();
                                    elementPriceIcon.Kind = PackIconKind.Circle;
                                    elementPriceIcon.Width = 15;
                                    elementPriceIcon.Height = 15;
                                    elementPriceIcon.Margin = new Thickness(15, 0, 15, 0);
                                    elementPrice.Children.Add(elementPriceIcon);
                                    TextBlock elementPriceLabel = new TextBlock();
                                    elementPriceLabel.Text = rawPrice;
                                    elementPriceLabel.Margin = new Thickness(15, 0, 15, 0);
                                    elementPrice.Children.Add(elementPriceLabel);
                                    elementBody.Children.Add(elementPrice);
                                    element.Child = elementBody;
                                    communityRewards.Children.Add(element);
                                    element.DataContext = id;
                                    element.MouseLeftButtonUp += OpenPointsStoreItemHandler;
                                }
                            }
                            pointsStoreControl.SelectedIndex = 4;
                        }
                    }
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Произошла ошибка при отправке письма", "Ошибка");
            }
        }

        public void OpenSubjectsSetHandler (object sender, RoutedEventArgs e)
        {
            OpenSubjectsSet();
        }

        public void OpenSubjectsSet()
        {
            subjectsSet.Children.Clear();
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/points/items/all");
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (var reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        var objText = reader.ReadToEnd();
                        PointsStoreItemsResponseInfo myobj = (PointsStoreItemsResponseInfo)js.Deserialize(objText, typeof(PointsStoreItemsResponseInfo));
                        string status = myobj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
                            List<PointsStoreItem> items = myobj.items;
                            foreach (PointsStoreItem item in items)
                            {
                                string type = item.type;
                                bool isSubjectsSet = type == "subjectsSet";
                                if (isSubjectsSet)
                                {
                                    string id = item._id;
                                    string title = item.title;
                                    string desc = item.desc;
                                    int price = item.price;
                                    string rawPrice = price.ToString();
                                    Border element = new Border();
                                    element.Background = System.Windows.Media.Brushes.LightGray;
                                    element.Width = 175;
                                    element.Height = 175;
                                    element.Margin = new Thickness(15);
                                    element.CornerRadius = new CornerRadius(5);
                                    StackPanel elementBody = new StackPanel();
                                    Image elementPhoto = new Image();
                                    elementPhoto.Width = 80;
                                    elementPhoto.Height = 80;
                                    elementPhoto.Margin = new Thickness(15, 5, 15, 5);
                                    elementPhoto.BeginInit();
                                    elementPhoto.Source = new BitmapImage(new Uri(@"http://localhost:4000/api/points/item/photo/?id=" + id));
                                    elementPhoto.EndInit();
                                    elementBody.Children.Add(elementPhoto);
                                    Separator elementSeparator = new Separator();
                                    elementSeparator.BorderBrush = System.Windows.Media.Brushes.Black;
                                    elementSeparator.BorderThickness = new Thickness(1);
                                    elementBody.Children.Add(elementSeparator);
                                    TextBlock elementTitleLabel = new TextBlock();
                                    elementTitleLabel.Text = title;
                                    elementTitleLabel.Margin = new Thickness(15, 5, 15, 5);
                                    elementBody.Children.Add(elementTitleLabel);
                                    TextBlock elementDescLabel = new TextBlock();
                                    elementDescLabel.Text = desc;
                                    elementDescLabel.Margin = new Thickness(15, 5, 15, 5);
                                    elementBody.Children.Add(elementDescLabel);
                                    StackPanel elementPrice = new StackPanel();
                                    elementPrice.HorizontalAlignment = HorizontalAlignment.Right;
                                    elementPrice.Orientation = Orientation.Horizontal;
                                    elementPrice.Margin = new Thickness(0, 5, 0, 5);
                                    PackIcon elementPriceIcon = new PackIcon();
                                    elementPriceIcon.Kind = PackIconKind.Circle;
                                    elementPriceIcon.Width = 15;
                                    elementPriceIcon.Height = 15;
                                    elementPriceIcon.Margin = new Thickness(15, 0, 15, 0);
                                    elementPrice.Children.Add(elementPriceIcon);
                                    TextBlock elementPriceLabel = new TextBlock();
                                    elementPriceLabel.Text = rawPrice;
                                    elementPriceLabel.Margin = new Thickness(15, 0, 15, 0);
                                    elementPrice.Children.Add(elementPriceLabel);
                                    elementBody.Children.Add(elementPrice);
                                    element.Child = elementBody;
                                    subjectsSet.Children.Add(element);
                                    element.DataContext = id;
                                    element.MouseLeftButtonUp += OpenPointsStoreItemHandler;
                                }
                            }
                            pointsStoreControl.SelectedIndex = 3;
                        }
                    }
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Произошла ошибка при отправке письма", "Ошибка");
            }
        }

        public void OpenGameSubjectsHandler (object sender, RoutedEventArgs e)
        {
            OpenGameSubjects();
        }

        public void OpenGameSubjects()
        {
            gameSubjects.Children.Clear();
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/points/items/all");
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (var reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        var objText = reader.ReadToEnd();
                        PointsStoreItemsResponseInfo myobj = (PointsStoreItemsResponseInfo)js.Deserialize(objText, typeof(PointsStoreItemsResponseInfo));
                        string status = myobj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
                            List<PointsStoreItem> items = myobj.items;
                            foreach (PointsStoreItem item in items)
                            {
                                string type = item.type;
                                bool isGameSubject = type == "gameSubject";
                                if (isGameSubject)
                                {
                                    string id = item._id;
                                    string title = item.title;
                                    string desc = item.desc;
                                    int price = item.price;
                                    string rawPrice = price.ToString();
                                    Border element = new Border();
                                    element.Background = System.Windows.Media.Brushes.LightGray;
                                    element.Width = 175;
                                    element.Height = 175;
                                    element.Margin = new Thickness(15);
                                    element.CornerRadius = new CornerRadius(5);
                                    StackPanel elementBody = new StackPanel();
                                    Image elementPhoto = new Image();
                                    elementPhoto.Width = 80;
                                    elementPhoto.Height = 80;
                                    elementPhoto.Margin = new Thickness(15, 5, 15, 5);
                                    elementPhoto.BeginInit();
                                    elementPhoto.Source = new BitmapImage(new Uri(@"http://localhost:4000/api/points/item/photo/?id=" + id));
                                    elementPhoto.EndInit();
                                    elementBody.Children.Add(elementPhoto);
                                    Separator elementSeparator = new Separator();
                                    elementSeparator.BorderBrush = System.Windows.Media.Brushes.Black;
                                    elementSeparator.BorderThickness = new Thickness(1);
                                    elementBody.Children.Add(elementSeparator);
                                    TextBlock elementTitleLabel = new TextBlock();
                                    elementTitleLabel.Text = title;
                                    elementTitleLabel.Margin = new Thickness(15, 5, 15, 5);
                                    elementBody.Children.Add(elementTitleLabel);
                                    TextBlock elementDescLabel = new TextBlock();
                                    elementDescLabel.Text = desc;
                                    elementDescLabel.Margin = new Thickness(15, 5, 15, 5);
                                    elementBody.Children.Add(elementDescLabel);
                                    StackPanel elementPrice = new StackPanel();
                                    elementPrice.HorizontalAlignment = HorizontalAlignment.Right;
                                    elementPrice.Orientation = Orientation.Horizontal;
                                    elementPrice.Margin = new Thickness(0, 5, 0, 5);
                                    PackIcon elementPriceIcon = new PackIcon();
                                    elementPriceIcon.Kind = PackIconKind.Circle;
                                    elementPriceIcon.Width = 15;
                                    elementPriceIcon.Height = 15;
                                    elementPriceIcon.Margin = new Thickness(15, 0, 15, 0);
                                    elementPrice.Children.Add(elementPriceIcon);
                                    TextBlock elementPriceLabel = new TextBlock();
                                    elementPriceLabel.Text = rawPrice;
                                    elementPriceLabel.Margin = new Thickness(15, 0, 15, 0);
                                    elementPrice.Children.Add(elementPriceLabel);
                                    elementBody.Children.Add(elementPrice);
                                    element.Child = elementBody;
                                    accountItems.Children.Add(element);
                                    element.DataContext = id;
                                    element.MouseLeftButtonUp += OpenPointsStoreItemHandler;
                                }
                            }
                            pointsStoreControl.SelectedIndex = 2;
                        }
                    }
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Произошла ошибка при отправке письма", "Ошибка");
            }
        }

        public void OpenFavoriteSubjectsHandler(object sender, RoutedEventArgs e)
        {
            OpenFavoriteSubjects();
        }

        public void OpenFavoriteSubjects()
        {
            pointsStoreControl.SelectedIndex = 1;
        }

        public void OpenPointsHelpHandler (object sender, RoutedEventArgs e)
        {
            OpenPointsHelp();
        }

        public void OpenPointsHelp()
        {
            pointsStoreControl.SelectedIndex = 0;
        }

        private void SaveLangSettingsHandler (object sender, RoutedEventArgs e)
        {
            SaveLangSettings();
        }

        public void GetLangSettings ()
        {
            Environment.SpecialFolder localApplicationDataFolder = Environment.SpecialFolder.LocalApplicationData;
            string localApplicationDataFolderPath = Environment.GetFolderPath(localApplicationDataFolder);
            string saveDataFilePath = localApplicationDataFolderPath + @"\OfficeWare\GameManager\" + currentUserId + @"\save-data.txt";
            JavaScriptSerializer js = new JavaScriptSerializer();
            string saveDataFileContent = File.ReadAllText(saveDataFilePath);
            SavedContent loadedContent = js.Deserialize<SavedContent>(saveDataFileContent);
            Settings currentSettings = loadedContent.settings;
            string currentLang = currentSettings.language;
            ItemCollection langSelectorItems = langSelector.Items;
            foreach (ComboBoxItem langSelectorItem in langSelectorItems)
            {
                object rawSelectedLangData = langSelectorItem.DataContext;
                string selectedLangData = ((string)(rawSelectedLangData));
                bool isLangFound = selectedLangData == currentLang;
                if (isLangFound)
                {
                    int currentLangIndex = langSelectorItems.IndexOf(langSelectorItem);
                    langSelector.SelectedIndex = currentLangIndex;
                    break;
                }
            }
        }

        public void SaveLangSettings ()
        {
            Environment.SpecialFolder localApplicationDataFolder = Environment.SpecialFolder.LocalApplicationData;
            string localApplicationDataFolderPath = Environment.GetFolderPath(localApplicationDataFolder);
            string saveDataFilePath = localApplicationDataFolderPath + @"\OfficeWare\GameManager\" + currentUserId + @"\save-data.txt";
            JavaScriptSerializer js = new JavaScriptSerializer();
            string saveDataFileContent = File.ReadAllText(saveDataFilePath);
            SavedContent loadedContent = js.Deserialize<SavedContent>(saveDataFileContent);
            List<Game> currentGames = loadedContent.games;
            List<FriendSettings> currentFriends = loadedContent.friends;
            Settings updatedSettings = loadedContent.settings;
            List<string> currentCollections = loadedContent.collections;
            Notifications currentNotifications = loadedContent.notifications;
            int selectedLangIndex = langSelector.SelectedIndex;
            ItemCollection langSelectorItems = langSelector.Items;
            object rawSelectedLang = langSelectorItems[selectedLangIndex];
            ComboBoxItem selectedLang = ((ComboBoxItem)(rawSelectedLang));
            object rawSelectedLangData = selectedLang.DataContext;
            string selectedLangData = ((string)(rawSelectedLangData));
            updatedSettings.language = selectedLangData;
            string savedContent = js.Serialize(new SavedContent
            {
                games = currentGames,
                friends = currentFriends,
                settings = updatedSettings,
                collections = currentCollections,
                notifications = currentNotifications
            });
            File.WriteAllText(saveDataFilePath, savedContent);
            this.Close();
        }

        private void OpenUpdateEmailHandler (object sender, MouseButtonEventArgs e)
        {
            OpenUpdateEmail();
        }

        public void OpenUpdateEmail()
        {
            Random rd = new Random();
            int rand_num = rd.Next(0, 1000);
            string code = rand_num.ToString();
            emailUpdateControl.DataContext = code;
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/users/get/?id=" + currentUserId);
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (var reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        var objText = reader.ReadToEnd();
                        UserResponseInfo myobj = (UserResponseInfo)js.Deserialize(objText, typeof(UserResponseInfo));
                        string status = myobj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
                            User user = myobj.user;
                            string email = user.login;
                            HttpWebRequest innerWebRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/users/email/set/accept/?id=" + currentUserId + @"&code=" + code + @"&to=" + email);
                            innerWebRequest.Method = "GET";
                            innerWebRequest.UserAgent = ".NET Framework Test Client";
                            using (HttpWebResponse innerWebResponse = (HttpWebResponse)innerWebRequest.GetResponse())
                            {
                                using (var innerReader = new StreamReader(innerWebResponse.GetResponseStream()))
                                {
                                    js = new JavaScriptSerializer();
                                    objText = innerReader.ReadToEnd();
                                    UserResponseInfo myInnerObj = (UserResponseInfo)js.Deserialize(objText, typeof(UserResponseInfo));
                                    status = myInnerObj.status;
                                    isOkStatus = status == "OK";
                                    if (isOkStatus)
                                    {
                                        mainControl.SelectedIndex = 36;
                                        try
                                        {
                                            MailMessage message = new MailMessage();
                                            SmtpClient smtp = new SmtpClient();
                                            message.From = new System.Net.Mail.MailAddress("glebdyakov2000@gmail.com");
                                            message.To.Add(new System.Net.Mail.MailAddress(email));
                                            string subjectBoxContent = @"Подтверждение аккаунта Office ware game manager";
                                            message.Subject = subjectBoxContent;
                                            message.IsBodyHtml = true; //to make message body as html  
                                            string messageBodyBoxContent = "<h3>Здравствуйте, " + email + "!</h3><p>" + code + "</p><p>Код для смены E-mail вашего аккаунта Office ware game manager</p>";
                                            message.Body = messageBodyBoxContent;
                                            smtp.Port = 587;
                                            smtp.Host = "smtp.gmail.com"; //for gmail host  
                                            smtp.EnableSsl = true;
                                            smtp.UseDefaultCredentials = false;
                                            smtp.Credentials = new NetworkCredential("glebdyakov2000@gmail.com", "ttolpqpdzbigrkhz");
                                            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                                            smtp.Send(message);
                                        }
                                        catch (Exception)
                                        {
                                            MessageBox.Show("Произошла ошибка при отправке письма", "Ошибка");
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

        private void OpenUpdatePasswordHandler (object sender, MouseButtonEventArgs e)
        {
            OpenUpdatePassword();
        }

        public void OpenUpdatePassword ()
        {
            Random rd = new Random();
            int rand_num = rd.Next(0, 1000);
            string code = rand_num.ToString();
            passwordUpdateControl.DataContext = code;
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/users/get/?id=" + currentUserId);
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (var reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        var objText = reader.ReadToEnd();
                        UserResponseInfo myobj = (UserResponseInfo)js.Deserialize(objText, typeof(UserResponseInfo));
                        string status = myobj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
                            User user = myobj.user;
                            string email = user.login;
                            HttpWebRequest innerWebRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/users/password/set/accept/?id=" + currentUserId + @"&code=" + code + @"&to=" + email);
                            innerWebRequest.Method = "GET";
                            innerWebRequest.UserAgent = ".NET Framework Test Client";
                            using (HttpWebResponse innerWebResponse = (HttpWebResponse)innerWebRequest.GetResponse())
                            {
                                using (var innerReader = new StreamReader(innerWebResponse.GetResponseStream()))
                                {
                                    js = new JavaScriptSerializer();
                                    objText = innerReader.ReadToEnd();
                                    UserResponseInfo myInnerObj = (UserResponseInfo)js.Deserialize(objText, typeof(UserResponseInfo));
                                    status = myInnerObj.status;
                                    isOkStatus = status == "OK";
                                    if (isOkStatus)
                                    {
                                        mainControl.SelectedIndex = 37;

                                        try
                                        {
                                            MailMessage message = new MailMessage();
                                            SmtpClient smtp = new SmtpClient();
                                            message.From = new System.Net.Mail.MailAddress("glebdyakov2000@gmail.com");
                                            message.To.Add(new System.Net.Mail.MailAddress(email));
                                            string subjectBoxContent = @"Подтверждение аккаунта Office ware game manager";
                                            message.Subject = subjectBoxContent;
                                            message.IsBodyHtml = true; //to make message body as html  
                                            string messageBodyBoxContent = "<h3>Здравствуйте, " + email + "!</h3><p>" + code + "</p><p>Код для смены пароля вашего аккаунта Office ware game manager</p>";
                                            message.Body = messageBodyBoxContent;
                                            smtp.Port = 587;
                                            smtp.Host = "smtp.gmail.com"; //for gmail host  
                                            smtp.EnableSsl = true;
                                            smtp.UseDefaultCredentials = false;
                                            smtp.Credentials = new NetworkCredential("glebdyakov2000@gmail.com", "ttolpqpdzbigrkhz");
                                            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                                            smtp.Send(message);
                                        }
                                        catch (Exception)
                                        {
                                            MessageBox.Show("Произошла ошибка при отправке письма", "Ошибка");
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

        private void AcceptEmailUpdateHandler (object sender, RoutedEventArgs e)
        {
            AcceptEmailUpdate();
        }

        public void AcceptEmailUpdate()
        {
            object emailUpdateControlData = emailUpdateControl.DataContext;
            bool isDataExists = emailUpdateControlData != null;
            if (isDataExists)
            {
                string code = ((string)(emailUpdateControlData));
                string emailCodeBoxContent = emailCodeBox.Text;
                bool isCodeMatches = code == emailCodeBoxContent;
                if (isCodeMatches)
                {
                    emailUpdateControl.SelectedIndex = 1;
                }
                else
                {
                    MessageBox.Show("Код введен неправильно", "Ошибка");
                }
            }
        }

        private void AcceptPasswordUpdateHandler(object sender, RoutedEventArgs e)
        {
            AcceptPasswordUpdate();
        }

        public void AcceptPasswordUpdate()
        {
            object passwordUpdateControlData = passwordUpdateControl.DataContext;
            bool isDataExists = passwordUpdateControlData != null;
            if (isDataExists)
            {
                string code = ((string)(passwordUpdateControlData));
                string passwordCodeBoxContent = passwordCodeBox.Text;
                bool isCodeMatches = code == passwordCodeBoxContent;
                if (isCodeMatches)
                {
                    passwordUpdateControl.SelectedIndex = 1;
                }
                else
                {
                    MessageBox.Show("Код введен неправильно", "Ошибка");
                }
            }
        }

        public void EmailUpdateHandler (object sender, RoutedEventArgs e)
        {
            EmailUpdate();
        }

        public void EmailUpdate ()
        {
            string updatedEmailBoxContent = updatedEmailBox.Text;
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/users/email/set/?id=" + currentUserId + @"&email=" + updatedEmailBoxContent);
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (var reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        var objText = reader.ReadToEnd();
                        UserResponseInfo myobj = (UserResponseInfo)js.Deserialize(objText, typeof(UserResponseInfo));
                        string status = myobj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
                            mainControl.SelectedIndex = 0;
                            emailUpdateControl.SelectedIndex = 0;
                            MessageBox.Show("E-mail был обновлен", "Внимание");
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

        public void PasswordUpdateHandler(object sender, RoutedEventArgs e)
        {
            PasswordUpdate();
        }

        public void PasswordUpdate()
        {
            string updatedPasswordBoxContent = updatedPasswordBox.Password;
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/users/password/set/?id=" + currentUserId + @"&password=" + updatedPasswordBoxContent);
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (var reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        var objText = reader.ReadToEnd();
                        UserResponseInfo myobj = (UserResponseInfo)js.Deserialize(objText, typeof(UserResponseInfo));
                        string status = myobj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
                            mainControl.SelectedIndex = 0;
                            passwordUpdateControl.SelectedIndex = 0;
                            MessageBox.Show("Пароль был обновлен", "Внимание");
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

        private void OpenStoreSettingsHandler (object sender, MouseButtonEventArgs e)
        {
            OpenStoreSettings();
        }

        public void OpenStoreSettings()
        {

            mainControl.SelectedIndex = 15;
            
            // accountSettingsControl.SelectedIndex = 1;
            SelectAccountSettingsItem(1);
        }

        private void ToggleScreenShotsManagementHandler (object sender, RoutedEventArgs e)
        {
            ToggleScreenShotsManagement();
        }

        public void ToggleScreenShotsDisplayHandler (object sender, RoutedEventArgs e)
        {
            ComboBox selector = ((ComboBox)(sender));
            ToggleScreenShotsDisplay(selector);
        }

        public void ToggleScreenShotsDisplay (ComboBox selector)
        {
            if (isAppInit)
            {
                int selectedIndex = selector.SelectedIndex;
                bool isWall = selectedIndex == 0;
                screenShotsContainer.Children.RemoveAt(2);
                if (isWall)
                {
                    StackPanel container = new StackPanel();
                    container.Margin = new Thickness(25);
                    screenShotsContainer.Children.Add(container);
                }
                else
                {
                    WrapPanel container = new WrapPanel();
                    container.Margin = new Thickness(25);
                    screenShotsContainer.Children.Add(container);
                }
                GetScreenShots("", false);
            }
        }

        public void ToggleScreenShotsManagement ()
        {
            Visibility screenShotsManagementVisibility = screenShotsManagement.Visibility;
            bool isVisible = screenShotsManagementVisibility == visible;
            if (isVisible)
            {
                screenShotsManagement.Visibility = invisible;
            }
            else
            {
                screenShotsManagement.Visibility = visible;
            }
        }

        private void OpenAboutManagerDialogHandler (object sender, RoutedEventArgs e)
        {
            OpenAboutManagerDialog();
        }

        public void OpenAboutManagerDialog ()
        {
            Dialogs.AboutManagerDialog dialog = new Dialogs.AboutManagerDialog();
            dialog.Show();
        }

        private void OpenSupportServiceHandler (object sender, RoutedEventArgs e)
        {
            OpenSupportService();
        }

        public void OpenSupportService()
        {
            mainControl.SelectedIndex = 40;
        }

        private void OpenPrivacyInfoHandler (object sender, RoutedEventArgs e)
        {
            OpenPrivacyInfo();
        }

        public void OpenPrivacyInfo ()
        {
            mainControl.SelectedIndex = 41;
        }

        private void OpenLawInfoHandler(object sender, RoutedEventArgs e)
        {
            OpenLawInfo();
        }

        public void OpenLawInfo ()
        {
            mainControl.SelectedIndex = 42;
        }

        private void OpenSubAggermentHandler (object sender, RoutedEventArgs e)
        {
            OpenSubAggerment();
        }

        public void OpenSubAggerment ()
        {
            mainControl.SelectedIndex = 43;
        }

        private void SelectGamesForFamilyViewOrSetAddressHandler (object sender, RoutedEventArgs e)
        {
            SelectGamesForFamilyViewOrSetAddress();
        }

        public void SelectGamesForFamilyViewOrSetAddress ()
        {
            object rawIsSelectedGames = familyViewSelectedGamesRadionBtn.IsChecked;
            bool isSelectedGames = ((bool)(rawIsSelectedGames));
            if (isSelectedGames)
            {
                SelectGamesForFamilyView();
            }
            else
            {
                SetFamilyViewAddress();
            }
        }

        public void SetFamilyViewAddressHandler (object sender, RoutedEventArgs e)
        {
            SetFamilyViewAddress();
        }

        public void SelectGamesForFamilyView ()
        {
            familyViewManagementControl.SelectedIndex = 1;
            GetFamilyViewGames();
        }

        public void GetFamilyViewGames ()
        {
            Environment.SpecialFolder localApplicationDataFolder = Environment.SpecialFolder.LocalApplicationData;
            string localApplicationDataFolderPath = Environment.GetFolderPath(localApplicationDataFolder);
            string saveDataFilePath = localApplicationDataFolderPath + @"\OfficeWare\GameManager\" + currentUserId + @"\save-data.txt";
            JavaScriptSerializer js = new JavaScriptSerializer();
            string saveDataFileContent = File.ReadAllText(saveDataFilePath);
            SavedContent loadedContent = js.Deserialize<SavedContent>(saveDataFileContent);
            List<Game> currentGames = loadedContent.games;
            familyViewGames.RowDefinitions.Clear();
            familyViewGames.Children.Clear();
            foreach (Game game in currentGames)
            {
                string gameId = game.id;
                string gameName = game.name;
                string insensitiveCaseGameName = gameName.ToLower();
                string keywords = familyViewGamesBox.Text;
                string insensitiveCaseKeywords = keywords.ToLower();
                bool isKeywordsMatch = insensitiveCaseGameName.Contains(insensitiveCaseKeywords);
                int insensitiveCaseKeywordsLength = insensitiveCaseKeywords.Length;
                bool isFilterDisabled = insensitiveCaseKeywordsLength < 0; ;
                bool isAddGame = isKeywordsMatch || isFilterDisabled;
                if (isAddGame)
                {
                    RowDefinition row = new RowDefinition();
                    familyViewGames.RowDefinitions.Add(row);
                    RowDefinitionCollection rows = familyViewGames.RowDefinitions;
                    int countRows = rows.Count;
                    int lastRowIndex = countRows - 1;
                    CheckBox checkBox = new CheckBox();
                    familyViewGames.Children.Add(checkBox);
                    Grid.SetRow(checkBox, lastRowIndex);
                    Grid.SetColumn(checkBox, 0);
                    TextBlock gameNameLabel = new TextBlock();
                    gameNameLabel.DataContext = gameId;
                    gameNameLabel.Text = gameName;
                    familyViewGames.Children.Add(gameNameLabel);
                    Grid.SetRow(gameNameLabel, lastRowIndex);
                    Grid.SetColumn(gameNameLabel, 1);
                }
            }
        }

        public void SetFamilyViewAddress ()
        {
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/users/get/?id=" + currentUserId);
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (var reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        var objText = reader.ReadToEnd();
                        UserResponseInfo myobj = (UserResponseInfo)js.Deserialize(objText, typeof(UserResponseInfo));
                        string status = myobj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
                            User user = myobj.user;
                            string email = user.login;
                            familyViewRecoveryEmailBox.Text = email;
                            familyViewManagementControl.SelectedIndex = 2;
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

        private void SetFamilyViewPinCodeHandler (object sender, RoutedEventArgs e)
        {
            SetFamilyViewPinCode();
        }

        public void SetFamilyViewPinCode ()
        {
            familyViewPinCodeErrorsLabel.Visibility = invisible;
            familyViewManagementControl.SelectedIndex = 3;
        }

        private void CheckFamilyViewPinCodeHandler (object sender, RoutedEventArgs e)
        {
            CheckFamilyViewPinCode();
        }

        public void CheckFamilyViewPinCode ()
        {
            string familyViewRecoveryEmailBoxContent = familyViewRecoveryEmailBox.Text;
            string familyViewPinCodeBoxContent = familyViewPinCodeBox.Password;
            string familyViewPinCodeConfirmBoxContent = familyViewPinCodeConfirmBox.Password;
            bool isCodeMatch = familyViewPinCodeBoxContent == familyViewPinCodeConfirmBoxContent;
            if (isCodeMatch)
            {

                Random rd = new Random();
                int rand_num = rd.Next(0, 1000);
                string secret = rand_num.ToString();
                familyViewManagementControl.DataContext = secret;
                try
                {
                    MailMessage message = new MailMessage();
                    SmtpClient smtp = new SmtpClient();
                    message.From = new System.Net.Mail.MailAddress("glebdyakov2000@gmail.com");
                    message.To.Add(new System.Net.Mail.MailAddress(familyViewRecoveryEmailBoxContent));
                    string subjectBoxContent = @"Ваш аккаунт Office ware game manager: активация семейного просмотра";
                    message.Subject = subjectBoxContent;
                    message.IsBodyHtml = true; //to make message body as html  
                    string messageBodyBoxContent = "<h3>Здравствуйте, " + familyViewRecoveryEmailBoxContent + "!</h3><p>Мы получили запрос на включение семейного просмотра на вашем аккаунте Steam.</p><p>Вот ваш код:</p><p>" + secret + "</p>";
                    message.Body = messageBodyBoxContent;
                    smtp.Port = 587;
                    smtp.Host = "smtp.gmail.com"; //for gmail host  
                    smtp.EnableSsl = true;
                    smtp.UseDefaultCredentials = false;
                    smtp.Credentials = new NetworkCredential("glebdyakov2000@gmail.com", "ttolpqpdzbigrkhz");
                    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtp.Send(message);

                    string familyViewRecoveryEmailLabelContent = @"Чтобы включить семейный просмотр на вашем аккаунте, введите код,&#10;отправленный на адресс эл. почты " + familyViewRecoveryEmailBoxContent + ".";
                    familyViewRecoveryEmailLabel.Text = familyViewRecoveryEmailLabelContent;
                    familyViewManagementControl.SelectedIndex = 4;

                }
                catch (Exception)
                {
                    MessageBox.Show("Произошла ошибка при отправке письма", "Ошибка");
                }

            }
            else
            {
                familyViewPinCodeErrorsLabel.Visibility = visible;
            }
        }

        private void CheckFamilyViewSecretCodeHandler (object sender, RoutedEventArgs e)
        {
            CheckFamilyViewSecretCode();
        }

        public void CheckFamilyViewSecretCode ()
        {
            object secretData = familyViewManagementControl.DataContext;
            string secret = ((string)(secretData));
            string familyViewSecretCodeBoxContent = familyViewSecretCodeBox.Password;
            bool isSecretMatch = familyViewSecretCodeBoxContent == secret;
            if (isSecretMatch)
            {
                Environment.SpecialFolder localApplicationDataFolder = Environment.SpecialFolder.LocalApplicationData;
                string localApplicationDataFolderPath = Environment.GetFolderPath(localApplicationDataFolder);
                string saveDataFilePath = localApplicationDataFolderPath + @"\OfficeWare\GameManager\" + currentUserId + @"\save-data.txt";
                JavaScriptSerializer js = new JavaScriptSerializer();
                string saveDataFileContent = File.ReadAllText(saveDataFilePath);
                SavedContent loadedContent = js.Deserialize<SavedContent>(saveDataFileContent);
                List<Game> currentGames = loadedContent.games;
                List<FriendSettings> currentFriends = loadedContent.friends;
                Settings updatedSettings = loadedContent.settings;
                List<string> currentCollections = loadedContent.collections;
                Notifications currentNotifications = loadedContent.notifications;
                updatedSettings.familyView = true;
                string familyViewPinCodeBoxContent = familyViewPinCodeBox.Password;
                updatedSettings.familyViewCode = familyViewPinCodeBoxContent;
                List<string> selectedGames = new List<string>();
                foreach (UIElement familyViewGameItem in familyViewGames.Children)
                {
                    bool isCheckBox = familyViewGameItem is CheckBox;
                    if (isCheckBox)
                    {
                        CheckBox checkBox = ((CheckBox)(familyViewGameItem));
                        object rawIsChecked = checkBox.IsChecked;
                        bool isChecked = ((bool)(rawIsChecked));
                        if (isChecked)
                        {
                            int checkBoxIndex = familyViewGames.Children.IndexOf(familyViewGameItem);
                            int labelIndex = checkBoxIndex + 1;
                            UIElement label = familyViewGames.Children[labelIndex];
                            TextBlock gameNameLabel = ((TextBlock)(label));
                            object labelData = gameNameLabel.DataContext;
                            string id = ((string)(labelData));
                            selectedGames.Add(id);
                        }
                    }
                }
                updatedSettings.familyViewGames = selectedGames;
                string savedContent = js.Serialize(new SavedContent
                {
                    games = currentGames,
                    friends = currentFriends,
                    settings = updatedSettings,
                    collections = currentCollections,
                    notifications = currentNotifications
                });
                File.WriteAllText(saveDataFilePath, savedContent);
                GetFamilyView();
                familyViewManagementControl.SelectedIndex = 5;
            }
        }

        private void ToggleFamilyViewModeHandler (object sender, RoutedEventArgs e)
        {
            ToggleFamilyViewMode();
        }

        public void ShowFamilyViewPopupHandler(object sender, RoutedEventArgs e)
        {
            ShowFamilyViewPopup();
        }

        public void ShowFamilyViewPopup ()
        {
            familyViewPopup.IsOpen = true;
            if (isFamilyViewMode)
            {
                familyViewPopupControl.SelectedIndex = 0;
            }
            else
            {
                familyViewPopupControl.SelectedIndex = 1;
            }
        }

        public void ToggleFamilyViewMode ()
        {
            if (isFamilyViewMode)
            {
                Environment.SpecialFolder localApplicationDataFolder = Environment.SpecialFolder.LocalApplicationData;
                string localApplicationDataFolderPath = Environment.GetFolderPath(localApplicationDataFolder);
                string saveDataFilePath = localApplicationDataFolderPath + @"\OfficeWare\GameManager\" + currentUserId + @"\save-data.txt";
                JavaScriptSerializer js = new JavaScriptSerializer();
                string saveDataFileContent = File.ReadAllText(saveDataFilePath);
                SavedContent loadedContent = js.Deserialize<SavedContent>(saveDataFileContent);
                Settings currentSettings = loadedContent.settings;
                string familyViewCode = currentSettings.familyViewCode;
                string familyViewPopupCodeBoxContent = familyViewPopupCodeBox.Text;
                bool isCodeMatch = familyViewPopupCodeBoxContent == familyViewCode;
                if (isCodeMatch)
                {
                    isFamilyViewMode = !isFamilyViewMode;
                    if (isFamilyViewMode)
                    {
                        familyViewPopupControl.SelectedIndex = 0;
                        familyViewIcon.Foreground = System.Windows.Media.Brushes.Green;
                    }
                    else
                    {
                        familyViewPopupControl.SelectedIndex = 1;
                        familyViewIcon.Foreground = System.Windows.Media.Brushes.Orange;
                    }
                    CloseFamilyViewPopup();
                }
            }
            else
            {
                isFamilyViewMode = !isFamilyViewMode;
                if (isFamilyViewMode)
                {
                    familyViewPopupControl.SelectedIndex = 0;
                    familyViewIcon.Foreground = System.Windows.Media.Brushes.Green;
                }
                else
                {
                    familyViewPopupControl.SelectedIndex = 1;
                    familyViewIcon.Foreground = System.Windows.Media.Brushes.Orange;
                }
                CloseFamilyViewPopup();
            }
        }

        private void CloseFamilyViewPopupHandler (object sender, RoutedEventArgs e)
        {
            CloseFamilyViewPopup();
        }

        public void CloseFamilyViewPopup ()
        {
            familyViewPopup.IsOpen = false;
        }

        private void DisableFamilyViewHandler (object sender, RoutedEventArgs e)
        {
            DisableFamilyView();
        }

        public void DisableFamilyView ()
        {
            Environment.SpecialFolder localApplicationDataFolder = Environment.SpecialFolder.LocalApplicationData;
            string localApplicationDataFolderPath = Environment.GetFolderPath(localApplicationDataFolder);
            string saveDataFilePath = localApplicationDataFolderPath + @"\OfficeWare\GameManager\" + currentUserId + @"\save-data.txt";
            JavaScriptSerializer js = new JavaScriptSerializer();
            string saveDataFileContent = File.ReadAllText(saveDataFilePath);
            SavedContent loadedContent = js.Deserialize<SavedContent>(saveDataFileContent);
            List<Game> currentGames = loadedContent.games;
            List<FriendSettings> currentFriends = loadedContent.friends;
            Settings updatedSettings = loadedContent.settings;
            List<string> currentCollections = loadedContent.collections;
            Notifications currentNotifications = loadedContent.notifications;
            updatedSettings.familyView = false;
            string familyViewPinCodeBoxContent = "";
            updatedSettings.familyViewCode = familyViewPinCodeBoxContent;
            string savedContent = js.Serialize(new SavedContent
            {
                games = currentGames,
                friends = currentFriends,
                settings = updatedSettings,
                collections = currentCollections,
                notifications = currentNotifications
            });
            File.WriteAllText(saveDataFilePath, savedContent);
            GetFamilyView();
            familyViewManagementControl.SelectedIndex = 0;
        }

        private void OpenFamilyViewDisableHandler (object sender, RoutedEventArgs e)
        {
            OpenFamilyViewDisable();
        }

        public void OpenFamilyViewDisable ()
        {
            familyViewManagementControl.SelectedIndex = 6;
        }

        private void OpenIncreaseAmountHandler (object sender, MouseButtonEventArgs e)
        {
            OpenIncreaseAmount();
        }

        public void OpenIncreaseAmount ()
        {
            mainControl.SelectedIndex = 45;
            GetAmountInfo();
        }

        public void GetAmountInfo()
        {
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/users/get/?id=" + currentUserId);
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (var reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        var objText = reader.ReadToEnd();
                        UserResponseInfo myobj = (UserResponseInfo)js.Deserialize(objText, typeof(UserResponseInfo));
                        string status = myobj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
                            User user = myobj.user;
                            int amount = user.amount;
                            string rawAmount = amount.ToString();
                            string measure = "руб.";
                            string amountLabelContent = rawAmount + " " + measure;
                            amountLabel.Text = amountLabelContent;
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

        private void IncreaseAmountHandler (object sender, RoutedEventArgs e)
        {
            Button btn = ((Button)(sender));
            object btnData = btn.DataContext;
            string amount = btnData.ToString();
            // string rawAmount = btnData.ToString();
            // int amount = Int32.Parse(rawAmount);
            IncreaseAmount(amount);
        }

        public void IncreaseAmount (string amount)
        {
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/users/amount/increase/?id=" + currentUserId + @"&amount=" + amount);
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (var reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        var objText = reader.ReadToEnd();
                        UserResponseInfo myobj = (UserResponseInfo)js.Deserialize(objText, typeof(UserResponseInfo));
                        string status = myobj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
                            GetAmountInfo();
                            MessageBox.Show("Счет был пополнен.", "Внимание");
                        }
                        else
                        {
                            MessageBox.Show("Не удалось пополнить счет.", "Ошибка");
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

        private void RefreshFamilyViewGamesHandler (object sender, TextChangedEventArgs e)
        {
            RefreshFamilyViewGames();
        }

        public void RefreshFamilyViewGames ()
        {
            GetFamilyViewGames();
        }

        private void OpenUpdatePhoneHandler (object sender, MouseButtonEventArgs e)
        {
            OpenUpdatePhone();
        }

        public void OpenUpdatePhone ()
        {
            mainControl.SelectedIndex = 46;
        }

        private void TogglePhoneCountryCodeHandler (object sender, SelectionChangedEventArgs e)
        {
            ComboBox selector = ((ComboBox)(sender));
            ItemCollection selectorItems = selector.Items;
            int selectedIndex = selector.SelectedIndex;
            object selectedItem = selectorItems[selectedIndex];
            ComboBoxItem selectedCountryCodeItem = ((ComboBoxItem)(selectedItem));
            object selectedCountryCodeItemData = selectedCountryCodeItem.DataContext;
            string countryCode = selectedCountryCodeItemData.ToString();
            TogglePhoneCountryCode(countryCode);
        }

        public void TogglePhoneCountryCode (string countryCode)
        {
            if (isAppInit)
            {
                phoneBox.Text = countryCode;
                string phoneFormatLabelContent = countryCode + "9123456789";
                phoneFormatLabel.Text = phoneFormatLabelContent;
            }
        }

        private void UpdatePhoneHandler (object sender, RoutedEventArgs e)
        {
            UpdatePhone();
        }

        public void UpdatePhone ()
        {
            string phone = phoneBox.Text;
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/users/phone/set/?id=" + currentUserId + @"&phone=" + phone);
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (var reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        var objText = reader.ReadToEnd();
                        UserResponseInfo myobj = (UserResponseInfo)js.Deserialize(objText, typeof(UserResponseInfo));
                        string status = myobj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
                            GetAccountSettings();
                            MessageBox.Show("Номер телефона был обновлен.", "Внимание");
                        }
                        else
                        {
                            MessageBox.Show("Не удалось обновить номер телефона.", "Ошибка");
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

        private void CancelUpdatePhoneHandler (object sender, RoutedEventArgs e)
        {
            CancelUpdatePhone();
        }

        public void CancelUpdatePhone ()
        {
            phoneBox.Text = "";
        }

        private void OpenEmailSettingsHandler (object sender, MouseButtonEventArgs e)
        {
            OpenEmailSettings();
        }

        public void OpenEmailSettings ()
        {
            GetEmailSettings();
            mainControl.SelectedIndex = 47;
        }

        public void GetEmailSettings ()
        {
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/users/get/?id=" + currentUserId);
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (var reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        var objText = reader.ReadToEnd();
                        UserResponseInfo myobj = (UserResponseInfo)js.Deserialize(objText, typeof(UserResponseInfo));
                        string status = myobj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
                            User user = myobj.user;
                            string email = user.login;
                            string emailSettingsLoginLabelContent = "НАСТРОЙКИ РАССЫЛКИ ДЛЯ " + email + ":";
                            emailSettingsLoginLabel.Text = emailSettingsLoginLabelContent;
                            Environment.SpecialFolder localApplicationDataFolder = Environment.SpecialFolder.LocalApplicationData;
                            string localApplicationDataFolderPath = Environment.GetFolderPath(localApplicationDataFolder);
                            string saveDataFilePath = localApplicationDataFolderPath + @"\OfficeWare\GameManager\" + currentUserId + @"\save-data.txt";
                            js = new JavaScriptSerializer();
                            string saveDataFileContent = File.ReadAllText(saveDataFilePath);
                            SavedContent loadedContent = js.Deserialize<SavedContent>(saveDataFileContent);
                            Notifications currentNotifications = loadedContent.notifications;
                            bool isNotificationsEnabled = currentNotifications.isNotificationsEnabled;
                            if (isNotificationsEnabled)
                            {
                                notificationsEnabledCheckBox.IsChecked = isNotificationsEnabled;
                            }
                            else
                            {
                                bool isNotificationsDisabled = !isNotificationsEnabled;
                                notificationsDisabledCheckBox.IsChecked = isNotificationsDisabled;
                            }
                            notificationsProductFromWantListWithDiscountCheckBox.IsChecked = currentNotifications.notificationsProductFromWantListWithDiscount;
                            notificationsProductFromWantListUpdateAcccessCheckBox.IsChecked = currentNotifications.notificationsProductFromWantListUpdateAcccess;
                            notificationsProductFromSubsOrFavoritesUpdateAcccessCheckBox.IsChecked = currentNotifications.notificationsProductFromSubsOrFavoritesUpdateAcccess;
                            notificationsProductFromDeveloperUpdateAcccessCheckBox.IsChecked = currentNotifications.notificationsProductFromDeveloperUpdateAcccess;
                            notificationsStartYearlyDiscountCheckBox.IsChecked = currentNotifications.notificationsStartYearlyDiscount;
                            notificationsGroupUpdateGameReviewCheckBox.IsChecked = currentNotifications.notificationsGroupUpdateGameReview;
                            notificationsUpdateIconCheckBox.IsChecked = currentNotifications.notificationsUpdateIcon;
                            notificationsUpdateGamesCheckBox.IsChecked = currentNotifications.notificationsUpdateGames;
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

        private void RejectEmailSubsHandler (object sender, RoutedEventArgs e)
        {
            RejectEmailSubs();
        }

        public void RejectEmailSubs ()
        {
            if (isAppInit)
            {
                notificationsProductFromWantListWithDiscountCheckBox.IsEnabled = false;
                notificationsProductFromWantListUpdateAcccessCheckBox.IsEnabled = false;
                notificationsProductFromSubsOrFavoritesUpdateAcccessCheckBox.IsEnabled = false;
                notificationsProductFromDeveloperUpdateAcccessCheckBox.IsEnabled = false;
                notificationsStartYearlyDiscountCheckBox.IsEnabled = false;
                notificationsGroupUpdateGameReviewCheckBox.IsEnabled = false;
                notificationsUpdateIconCheckBox.IsEnabled = false;
                notificationsUpdateGamesCheckBox.IsEnabled = false;
            }
        }

        private void AcceptEmailSubsHandler (object sender, RoutedEventArgs e)
        {
            AcceptEmailSubs();
        }

        public void AcceptEmailSubs()
        {
            if (isAppInit)
            {
                notificationsProductFromWantListWithDiscountCheckBox.IsEnabled = true;
                notificationsProductFromWantListUpdateAcccessCheckBox.IsEnabled = true;
                notificationsProductFromSubsOrFavoritesUpdateAcccessCheckBox.IsEnabled = true;
                notificationsProductFromDeveloperUpdateAcccessCheckBox.IsEnabled = true;
                notificationsStartYearlyDiscountCheckBox.IsEnabled = true;
                notificationsGroupUpdateGameReviewCheckBox.IsEnabled = true;
                notificationsUpdateIconCheckBox.IsEnabled = true;
                notificationsUpdateGamesCheckBox.IsEnabled = true;
            }
        }

        private void SaveEmailSettigsHandler(object sender, RoutedEventArgs e)
        {
            SaveEmailSettigs();
        }

        public void SaveEmailSettigs ()
        {
            Environment.SpecialFolder localApplicationDataFolder = Environment.SpecialFolder.LocalApplicationData;
            string localApplicationDataFolderPath = Environment.GetFolderPath(localApplicationDataFolder);
            string saveDataFilePath = localApplicationDataFolderPath + @"\OfficeWare\GameManager\" + currentUserId + @"\save-data.txt";
            JavaScriptSerializer js = new JavaScriptSerializer();
            string saveDataFileContent = File.ReadAllText(saveDataFilePath);
            SavedContent loadedContent = js.Deserialize<SavedContent>(saveDataFileContent);
            List<Game> currentGames = loadedContent.games;
            List<FriendSettings> currentFriends = loadedContent.friends;
            Settings currentSettings = loadedContent.settings;
            List<string> currentCollections = loadedContent.collections;
            Notifications updatedNotifications = loadedContent.notifications;
            updatedNotifications.isNotificationsEnabled = ((bool)(notificationsEnabledCheckBox.IsChecked));
            updatedNotifications.notificationsProductFromWantListWithDiscount = ((bool)(notificationsProductFromWantListWithDiscountCheckBox.IsChecked));
            updatedNotifications.notificationsProductFromWantListUpdateAcccess = ((bool)(notificationsProductFromWantListUpdateAcccessCheckBox.IsChecked));
            updatedNotifications.notificationsProductFromSubsOrFavoritesUpdateAcccess = ((bool)(notificationsProductFromSubsOrFavoritesUpdateAcccessCheckBox.IsChecked));
            updatedNotifications.notificationsProductFromDeveloperUpdateAcccess = ((bool)(notificationsProductFromDeveloperUpdateAcccessCheckBox.IsChecked));
            updatedNotifications.notificationsStartYearlyDiscount = ((bool)(notificationsStartYearlyDiscountCheckBox.IsChecked));
            updatedNotifications.notificationsGroupUpdateGameReview = ((bool)(notificationsGroupUpdateGameReviewCheckBox.IsChecked));
            updatedNotifications.notificationsUpdateIcon = ((bool)(notificationsUpdateIconCheckBox.IsChecked));
            updatedNotifications.notificationsUpdateGames = ((bool)(notificationsUpdateGamesCheckBox.IsChecked));
            string savedContent = js.Serialize(new SavedContent
            {
                games = currentGames,
                friends = currentFriends,
                settings = currentSettings,
                collections = currentCollections,
                notifications = updatedNotifications
            });
            File.WriteAllText(saveDataFilePath, savedContent);
        }

        private void OpenAddPaymentVariantHandler (object sender, MouseButtonEventArgs e)
        {
            OpenAddPaymentVariant();
        }

        public void OpenAddPaymentVariant ()
        {
            mainControl.SelectedIndex = 48;
        }

        private void ClosePointsStorePopupHandler (object sender, RoutedEventArgs e)
        {
            ClosePointsStorePopup();
        }

        public void ClosePointsStorePopup ()
        {
            pointsStorePopup.IsOpen = false;
        }

        private void BuyPointsStoreItemHandler (object sender, RoutedEventArgs e)
        {
            BuyPointsStoreItem();
        }

        public void BuyPointsStoreItem ()
        {
            object pointsStorePopupData = pointsStorePopup.DataContext;
            string id = ((string)(pointsStorePopupData));
            object rawBtnData = buyPointsStoreItemBtn.DataContext;
            string btnData = ((string)(rawBtnData));
            bool isCanBuy = btnData == "have points";
            if (isCanBuy)
            {
                try
                {
                    HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/points/items/relations/get/?id=" + id);
                    webRequest.Method = "GET";
                    webRequest.UserAgent = ".NET Framework Test Client";
                    using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                    {
                        using (var reader = new StreamReader(webResponse.GetResponseStream()))
                        {
                            JavaScriptSerializer js = new JavaScriptSerializer();
                            var objText = reader.ReadToEnd();
                            PointsStoreItemResponseInfo myobj = (PointsStoreItemResponseInfo)js.Deserialize(objText, typeof(PointsStoreItemResponseInfo));
                            string status = myobj.status;
                            bool isOkStatus = status == "OK";
                            if (isOkStatus)
                            {
                                PointsStoreItem item = myobj.item;
                                int price = item.price;
                                string rawPrice = price.ToString();
                                HttpWebRequest innerWebRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/points/items/relations/add/?id=" + id + @"&user=" + currentUserId + @"&price=" + rawPrice);
                                innerWebRequest.Method = "GET";
                                innerWebRequest.UserAgent = ".NET Framework Test Client";
                                using (HttpWebResponse innerWebResponse = (HttpWebResponse)innerWebRequest.GetResponse())
                                {
                                    using (var innerReader = new StreamReader(innerWebResponse.GetResponseStream()))
                                    {
                                        js = new JavaScriptSerializer();
                                        objText = innerReader.ReadToEnd();
                                        UserResponseInfo myInnerObj = (UserResponseInfo)js.Deserialize(objText, typeof(UserResponseInfo));
                                        status = myobj.status;
                                        isOkStatus = status == "OK";
                                        if (isOkStatus)
                                        {
                                            MessageBox.Show("Спасибо за приобретение!", "Внимание");
                                        }
                                        else
                                        {
                                            MessageBox.Show("Не удалось купить товар!", "Внимание");
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
            else
            {
                OpenPointsHelp();
            }
            ClosePointsStorePopup();
        }

    }

    class SavedContent
    {
        public List<Game> games;
        public List<FriendSettings> friends;
        public Settings settings;
        public List<String> collections;
        public Notifications notifications;
    }

    class Notifications
    {
        public bool isNotificationsEnabled;
        public bool notificationsProductFromWantListWithDiscount;
        public bool notificationsProductFromWantListUpdateAcccess;
        public bool notificationsProductFromSubsOrFavoritesUpdateAcccess;
        public bool notificationsProductFromDeveloperUpdateAcccess;
        public bool notificationsStartYearlyDiscount;
        public bool notificationsGroupUpdateGameReview;
        public bool notificationsUpdateIcon;
        public bool notificationsUpdateGames;
    }

    class FriendSettings
    {
        public string id;
        public bool isFriendOnlineNotification;
        public bool isFriendOnlineSound;
        public bool isFriendPlayedNotification;
        public bool isFriendPlayedSound;
        public bool isFriendSendMsgNotification;
        public bool isFriendSendMsgSound;
        public bool isFavoriteFriend;
    }

    class Game
    {
        public string id;
        public string name;
        public string path;
        public string hours;
        public string date;
        public string installDate;
        public List<string> collections;
        public bool isHidden;
        public string cover;
        public bool overlay;
    }

    class GamesListResponseInfo
    {
        public string status;
        public List<GameResponseInfo> games;
    }

    class GameResponseInfo
    {
        public string _id;
        public string name;
        public string url;
        public string image;
        public int users;
        public int maxUsers;
        public int likes;
        public int price;
    }

    class UserResponseInfo
    {
        public string status;
        public User user;
    }

    class User
    {
        public string _id;
        public string login;
        public string password;
        public string name;
        public string country;
        public string about;
        public string status;
        public string friendsListSettings;
        public string gamesSettings;
        public string equipmentSettings;
        public string commentsSettings;
        public int points;
        public int amount;
        public bool isEmailConfirmed;
    }

    class FriendRequestsResponseInfo
    {
        public string status;
        public List<FriendRequest> requests;
    }

    public class FriendRequest
    {
        public string _id;
        public string user;
        public string friend;
    }

    public class GamesStatsResponseInfo
    {
        public string status;
        public int users;
        public int maxUsers;
    }

    public class Settings
    {
        public string language;
        public int startWindow;
        public string overlayHotKey;
        public MusicSettings music;
        public string profileTheme;
        public string screenShotsHotKey;
        public string frames;
        public bool showScreenShotsNotification;
        public bool playScreenShotsNotification;
        public bool saveScreenShotsCopy;
        public bool showOverlay;
        public bool familyView;
        public string familyViewCode;
        public List<string> familyViewGames;
    }

    public class MusicSettings
    {
        public double volume;
        public List<string> paths;
    }

    public class FileParameter
    {
        public byte[] File { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public FileParameter(byte[] file) : this(file, null) { }
        public FileParameter(byte[] file, string filename) : this(file, filename, null) { }
        public FileParameter(byte[] file, string filename, string contenttype)
        {
            File = file;
            FileName = filename;
            ContentType = contenttype;
        }
    }

    public class CPU
    {
        private DateTime time;
        public DateTime Time
        {
            get { return time; }
            set { time = value; }
        }

        private double percentage;
        public double Percentage
        {
            get { return percentage; }
            set { percentage = value; }
        }

        private double memoryUsage;
        public double MemoryUsage
        {
            get { return memoryUsage; }
            set { memoryUsage = value; }
        }

        public CPU()
        {
        }
        public CPU(DateTime time, double percentage, double memoryUsage)
        {
            this.Time = time;
            this.Percentage = percentage;
            this.MemoryUsage = memoryUsage;
        }

    }

    public class Model
    {
        public double High { get; set; }
        public double Low { get; set; }
        public double Open { get; set; }
        public double Close { get; set; }

        public Model(double high, double low, double open, double close)
        {
            High = high;
            Low = low;
            Open = open;
            Close = close;
        }

    }

    public class ForumsListResponseInfo
    {
        public List<Forum> forums;
        public string status;
    }

    public class Forum
    {
        public string _id;
        public string title;
    }

    public class ForumResponseInfo
    {
        public Forum forum;
        public string status;
    }

    public class ForumTopicsResponseInfo
    {
        public List<Topic> topics;
        public string status;
    }

    public class Topic
    {
        public string _id;
        public string title;
        public string forum;
        public string user;
    }

    class ForumTopicResponseInfo
    {
        public Topic topic;
        public string status;
    }

    class ForumTopicMsgsResponseInfo
    {
        public List<ForumTopicMsg> msgs;
        public string status;
    }

    class ForumTopicMsg
    {
        public string _id;
        public string content;
        public string topic;
        public DateTime date;
        public string user;
    }

    class NewsResponseInfo
    {
        public string status;
        public List<News> news;
    }

    class News
    {
        public string game;
        public string title;
        public string content;
        public DateTime date;
    }

    class GroupsResponseInfo
    {
        public List<Group> groups;
        public string status;
    }

    class GroupResponseInfo
    {
        public Group group;
        public string status;
    }

    class Group
    {
        public string _id;
        public string name;
        public string owner;
        public DateTime date;
        public string lang;
        public string country;
        public string fanPage;
        public string twitch;
        public string youtube;
    }

    class GroupRelationsResponseInfo
    {
        public List<GroupRelation> relations;
        public string status;
    }

    class GroupRelation
    {
        public string group;
        public string user;
    }

    class GroupRequestsResponseInfo
    {
        public List<GroupRequest> requests;
        public string status;
    }

    class GroupRequest
    {
        public string _id;
        public string group;
        public string user;
    }

    class CommentsResponseInfo
    {
        public string status;
        public List<Comment> comments;
    }

    class Comment
    {
        public string user;
        public string msg;
        public DateTime date;
    }

    class ManualsResponseInfo
    {
        public List<Manual> manuals;
        public string status;
    }

    class ManualResponseInfo
    {
        public Manual manual;
        public string status;
    }

    class Manual
    {
        public string _id;
        public string title;
        public string desc;
        public string user;
        public string categories;
        public string lang;
        public bool isDrm;
        public DateTime date;
    }

    class IllustrationsResponseInfo
    {
        public List<Illustration> illustrations;
        public string status;
    }

    class IllustrationResponseInfo
    {
        public Illustration illustration;
        public string status;
    }

    class Illustration
    {
        public string _id;
        public string title;
        public string desc;
        public string user;
        public bool isDrm;
        public DateTime date;
    }

    class ScreenShotsResponseInfo
    {
        public List<ScreenShot> screenShots;
        public string status;
    }

    class ScreenShotResponseInfo
    {
        public ScreenShot screenShot;
        public string status;
    }

    class ScreenShot
    {
        public string _id;
        public DateTime date;
    }

    class BlackListRelationsResponseInfo
    {
        public List<BlackListRelation> relations;
        public string status;
    }

    class BlackListRelation
    {
        public string user;
        public string friend;
    }

    class ReviewsResponseInfo
    {
        public List<Review> reviews;
        public string status;
    }

    class ReviewResponseInfo
    {
        public Review review;
        public string status;
    }

    class Review
    {
        public string _id;
        public string game;
        public string user;
        public string desc;
        public string hours;
        public DateTime date;
    }

    class ExperimentsResponseInfo
    {
        public List<Experiment> experiments;
        public string status;
    }

    class ExperimentResponseInfo
    {
        public Experiment experiment;
        public string status;
    }
    
    class Experiment
    {
        public string _id;
        public string title;
        public string desc;
    }

    class IconsResponseInfo
    {
        public string status;
        public List<Icon> icons;
    }

    class IconResponseInfo
    {
        public string status;
        public Icon icon;
    }

    class Icon
    {
        public string _id;
        public string title;
        public string desc;
    }

    class IconRelationsResponseInfo
    {
        public string status;
        public List<IconRelation> relations;
    }

    class IconRelationResponseInfo
    {
        public string status;
        public IconRelation relation;
    }

    class IconRelation
    {
        public string icon;
        public string user;
        public string date;
    }

    class GameRelationsResponseInfo {
        public string status;
        public List<GameRelation> relations;
    }

    class GameRelation {
        public string game;
        public string user;
    }

    class PointsStoreItemsResponseInfo
    {
        public string status;
        public List<PointsStoreItem> items;
    }

    class PointsStoreItemResponseInfo
    {
        public string status;
        public PointsStoreItem item;
    }

    class PointsStoreItem
    {
        public string _id;
        public string title;
        public string desc;
        public string type;
        public int price;
        public DateTime date;
    }

    class PointsStoreItemRelationsResponseInfo
    {
        public string status;
        public List<PointsStoreItemRelation> relations;
    }

    class PointsStoreItemRelationResponseInfo
    {
        public string status;
        public PointsStoreItemRelation relation;
    }

    class PointsStoreItemRelation
    {
        public string _id;
        public string item;
        public string user;
        public DateTime date;
    }
    
    class TalksResponseInfo
    {
        public List<Talk> talks;
        public string status;
    }

    class TalkResponseInfo
    {
        public Talk talk;
        public string status;
    }

    class Talk
    {
        public string _id;
        public string title;
        public string owner;
    }

    class TalkRelationsResponseInfo
    {
        public List<TalkRelation> relations;
        public string status;
    }

    class TalkRelationResponseInfo
    {
        public TalkRelation relation;
        public string status;
    }
    class TalkRelation
    {
        public string _id;
        public string talk;
        public string user;
    }

    class TalkCreateResponseInfo
    {
        public string id;
        public string status;

    }
}