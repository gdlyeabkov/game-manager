using SocketIOClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace GamaManager.Dialogs
{
    /// <summary>
    /// Логика взаимодействия для AddFriendDialog.xaml
    /// </summary>
    public partial class AddFriendDialog : Window
    {

        public string currentUserId = "";
        public List<string> usersIds;
        private User currentUser = null;
        public Brush disabledColor;
        public SocketIO client;
        public TabControl mainControl;

        public AddFriendDialog(string currentUserId, SocketIO client, TabControl mainControl)
        {
            InitializeComponent();

            Initialize(currentUserId, client, mainControl);

        }

        public void Initialize(string currentUserId, SocketIO client, TabControl mainControl)
        {
            InitializeConstants(currentUserId, client, mainControl);
            GetUser();
            GetUsers();
        }

        public void InitializeConstants (string currentUserId, SocketIO client, TabControl mainControl)
        {
            this.currentUserId = currentUserId;
            disabledColor = System.Windows.Media.Brushes.LightGray;
            this.client = client;
            this.mainControl = mainControl;
        }

        public void GetUser()
        {
            try
            {
                // HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("https://loud-reminiscent-jackrabbit.glitch.me/api/users/get/?id=" + currentUserId);
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("https://loud-reminiscent-jackrabbit.glitch.me/api/users/get/?id=" + currentUserId);
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (StreamReader reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        var objText = reader.ReadToEnd();

                        UserResponseInfo myobj = (UserResponseInfo)js.Deserialize(objText, typeof(UserResponseInfo));

                        string status = myobj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
                            currentUser = myobj.user;
                        }
                    }
                }
            }
            catch (System.Net.WebException)
            {
                Debugger.Log(0, "debug", "ошибка сервера 1");
                MessageBox.Show("Не удается подключиться к серверу", "Ошибка");
                this.Close();
            }
        }

        public void GetUsers()
        {
            try
            {
                // HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("https://loud-reminiscent-jackrabbit.glitch.me/api/users/all");
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("https://loud-reminiscent-jackrabbit.glitch.me/api/users/all");
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (var reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        var objText = reader.ReadToEnd();

                        UsersResponseInfo myobj = (UsersResponseInfo)js.Deserialize(objText, typeof(UsersResponseInfo));

                        string status = myobj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
                            usersIds = new List<string>();
                            foreach (User user in myobj.users)
                            {
                                string userId = user._id;
                                bool isMe = userId == currentUserId;
                                bool isNotMe = !isMe;
                                if (isNotMe)
                                {
                                    StackPanel usersItem = new StackPanel();
                                    usersItem.Orientation = Orientation.Horizontal;
                                    usersItem.Height = 35;
                                    usersItem.Margin = new Thickness(0, 5, 0, 5);
                                    string userLogin = user.login;

                                    Uri userAvatarUri = new Uri("https://cdn2.iconfinder.com/data/icons/ios-7-icons/50/user_male-128.png");
                                    // Uri userAvatarUri = new Uri("https://loud-reminiscent-jackrabbit.glitch.me/api/user/avatar/?id=" + userId);

                                    Image userAvatar = new Image();
                                    userAvatar.Width = 25;
                                    userAvatar.Height = 25;
                                    userAvatar.Margin = new Thickness(5);
                                    userAvatar.BeginInit();
                                    userAvatar.Source = new BitmapImage(userAvatarUri);
                                    userAvatar.EndInit();
                                    usersItem.Children.Add(userAvatar);
                                    TextBlock usersItemLoginLabel = new TextBlock();
                                    usersItemLoginLabel.Margin = new Thickness(5, 5, 5, 5);
                                    usersItemLoginLabel.Text = userLogin;
                                    usersItem.Children.Add(usersItemLoginLabel);
                                    users.Children.Add(usersItem);
                                    usersItem.DataContext = userId;
                                    ContextMenu usersItemContextMenu = new ContextMenu();
                                    MenuItem usersItemContextMenuItem = new MenuItem();
                                    usersItemContextMenuItem.Header = "Открыть профиль";
                                    usersItemContextMenuItem.DataContext = userId;
                                    usersItemContextMenuItem.Click += OpenUserProfileHandler;
                                    usersItemContextMenu.Items.Add(usersItemContextMenuItem);
                                    usersItem.ContextMenu = usersItemContextMenu;

                                    HttpWebRequest innerWebRequest = (HttpWebRequest)HttpWebRequest.Create("https://loud-reminiscent-jackrabbit.glitch.me/api/friends/get");
                                    innerWebRequest.Method = "GET";
                                    innerWebRequest.UserAgent = ".NET Framework Test Client";
                                    using (HttpWebResponse innerWebResponse = (HttpWebResponse)innerWebRequest.GetResponse())
                                    {
                                        using (var innerReader = new StreamReader(innerWebResponse.GetResponseStream()))
                                        {
                                            js = new JavaScriptSerializer();
                                            objText = innerReader.ReadToEnd();

                                            FriendsResponseInfo myInnerObj = (FriendsResponseInfo)js.Deserialize(objText, typeof(FriendsResponseInfo));

                                            status = myobj.status;
                                            isOkStatus = status == "OK";
                                            if (isOkStatus)
                                            {
                                                List<Friend> friends = myInnerObj.friends;

                                                List<Friend> myFriends = friends.Where<Friend>((Friend friend) =>
                                                {
                                                    return friend.user == currentUserId;
                                                }).ToList<Friend>();

                                                List<string> friendsIds = new List<string>();
                                                foreach (Friend myFriend in myFriends)
                                                {
                                                    string friendId = myFriend.friend;
                                                    friendsIds.Add(friendId);
                                                }
                                                bool isMyFriend = friendsIds.Contains(userId);
                                                if (isMyFriend)
                                                {
                                                    usersItemLoginLabel.Foreground = disabledColor;
                                                }
                                                else
                                                {
                                                    usersItem.MouseLeftButtonUp += ShowFriendCodeHandler;
                                                }
                                                usersIds.Add(userId);
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
                Debugger.Log(0, "debug", "ошибка сервера 2");
                MessageBox.Show("Не удается подключиться к серверу", "Ошибка");
                this.Close();
            }
        }

        private void SendFriendRequestHandler(object sender, RoutedEventArgs e)
        {
            string userId = friendCodeLabel.Text;
            SendFriendRequest(userId);
        }

        private void SendFriendRequest(string friendId)
        {
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("https://loud-reminiscent-jackrabbit.glitch.me/api/friends/requests/add/?id=" + currentUserId + @"&friend=" + friendId);
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
                            // webRequest = (HttpWebRequest)HttpWebRequest.Create("https://loud-reminiscent-jackrabbit.glitch.me/api/users/get/?id=" + friendId);
                            webRequest = (HttpWebRequest)HttpWebRequest.Create("https://loud-reminiscent-jackrabbit.glitch.me/api/users/get/?id=" + friendId);
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
                                        string msgContent = "Приглашение пользователю " + friendLogin + " было отправлено";
                                        Environment.SpecialFolder localApplicationDataFolder = Environment.SpecialFolder.LocalApplicationData;
                                        string localApplicationDataFolderPath = Environment.GetFolderPath(localApplicationDataFolder);
                                        string saveDataFilePath = localApplicationDataFolderPath + @"\OfficeWare\GameManager\" + currentUserId + @"\save-data.txt";
                                        js = new JavaScriptSerializer();
                                        string saveDataFileContent = File.ReadAllText(saveDataFilePath);
                                        SavedContent loadedContent = js.Deserialize<SavedContent>(saveDataFileContent);
                                        List<Game> currentGames = loadedContent.games;
                                        List<FriendSettings> currentFriends = loadedContent.friends;
                                        List<FriendSettings> updatedFriends = currentFriends;
                                        Settings currentSettings = loadedContent.settings;
                                        List<string> currentCollections = loadedContent.collections;
                                        Notifications currentNotifications = loadedContent.notifications;
                                        List<string> currentCategories = loadedContent.categories;
                                        List<string> currentRecentChats = loadedContent.recentChats;
                                        Recommendations currentRecommendations = loadedContent.recommendations;
                                        string currentLogoutDate = loadedContent.logoutDate;
                                        List<string> currentSections = loadedContent.sections;
                                        updatedFriends.Add(new FriendSettings()
                                        {
                                            id = friendId,
                                            isFriendOnlineNotification = true,
                                            isFriendOnlineSound = true,
                                            isFriendPlayedNotification = true,
                                            isFriendPlayedSound = true,
                                            isFriendSendMsgNotification = true,
                                            isFriendSendMsgSound = true,
                                            isFavoriteFriend = false,
                                            categories = new List<string>() { }
                                        });
                                        string savedContent = js.Serialize(new SavedContent
                                        {
                                            games = currentGames,
                                            friends = updatedFriends,
                                            settings = currentSettings,
                                            collections = currentCollections,
                                            notifications = currentNotifications,
                                            categories = currentCategories,
                                            recentChats = currentRecentChats,
                                            recommendations = currentRecommendations,
                                            logoutDate = currentLogoutDate,
                                            sections = currentSections
                                        });
                                        File.WriteAllText(saveDataFilePath, savedContent);
                                        string eventData = currentUserId + "|" + friendId;
                                        client.EmitAsync("user_send_friend_request", eventData);
                                        MessageBox.Show(msgContent, "Приглашение отправлено");
                                        this.Close();
                                    }
                                }
                            }
                        }
                        else
                        {
                            MessageBox.Show("Не удалось отправить приглашение", "Ошибка");
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

        public void ShowFriendCodeHandler(object sender, RoutedEventArgs e)
        {
            StackPanel user = ((StackPanel)(sender));
            object rawUserId = user.DataContext;
            string userId = ((string)(rawUserId));
            ShowFriendCode(userId);
        }

        public void ShowFriendCode(string userId)
        {
            friendCodeLabel.Text = userId;
        }

        private void DetectFriendCodeContentHandler(object sender, TextChangedEventArgs e)
        {
            DetectFriendCodeContent();
        }

        public void DetectFriendCodeContent()
        {
            string friendCodeLabelContent = friendCodeLabel.Text;
            int friendCodeLabelContentLength = friendCodeLabelContent.Length;
            bool isContentExists = friendCodeLabelContentLength >= 1;
            bool isUserIdSetted = usersIds.Contains(friendCodeLabelContent);
            bool isFriendCodeSetted = isContentExists && isUserIdSetted;
            if (isFriendCodeSetted)
            {
                sendFriendRequestBtn.IsEnabled = true;
            }
            else
            {
                sendFriendRequestBtn.IsEnabled = false;
            }
        }

        public void OpenUserProfileHandler(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = ((MenuItem)(sender));
            object menuItemData = menuItem.DataContext;
            string user = ((string)(menuItemData));
            OpenUserProfile(user);
        }

        async public void OpenUserProfile(string userId)
        {
            mainControl.DataContext = userId;
            mainControl.SelectedIndex = 1;
            this.Close();
        }

    }

    class UsersResponseInfo
    {
        public string status;
        public List<User> users;
    }

    class FriendsResponseInfo
    {
        public string status;
        public List<Friend> friends;
    }

    class Friend
    {
        public string _id;
        public string user;
        public string friend;
        public string alias;
    }

}
