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
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace GamaManager.Dialogs
{
    /// <summary>
    /// Логика взаимодействия для FriendsDialog.xaml
    /// </summary>
    public partial class FriendsDialog : Window
    {

        public string currentUserId = "";
        public SocketIO client;
        public Brush onlineBrush;
        public Brush playedBrush;
        public Brush offlineBrush;

        public FriendsDialog(string currentUserId, SocketIO client)
        {
            InitializeComponent();

            Initialize(currentUserId, client);
        
        }

        public void Initialize(string currentUserId, SocketIO client)
        {
            InitializeConstants(client);
            GetFriends(currentUserId, "");
        }

        public void InitializeConstants(SocketIO client)
        {
            this.client = client;
            onlineBrush = System.Windows.Media.Brushes.Blue;
            playedBrush = System.Windows.Media.Brushes.Green;
            offlineBrush = System.Windows.Media.Brushes.LightGray;
        }

        public void GetFriends (string currentUserId, string keywords)
        {
            this.currentUserId = currentUserId;
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("https://digitaldistributtionservice.herokuapp.com/api/users/get/?id=" + currentUserId);
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
                            friends.Children.Clear();
                            User currentUser = myobj.user;

                            webRequest = (HttpWebRequest)HttpWebRequest.Create("https://digitaldistributtionservice.herokuapp.com/api/friends/get");
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
                                        List<Friend> friendsIds = myInnerObj.friends;
                                        foreach (Friend friendInfo in friendsIds)
                                        {
                                            string friendId = friendInfo.friend;
                                            if (friendId != currentUserId)
                                            {
                                                webRequest = (HttpWebRequest)HttpWebRequest.Create("https://digitaldistributtionservice.herokuapp.com/api/users/get/?id=" + friendId);
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
                                                            string friendStatus = friend.status;
                                                            string friendIgnoreCaseLogin = friendLogin.ToLower();
                                                            string ignoreCaseKeywords = keywords.ToLower();
                                                            bool isFriendMatch = friendIgnoreCaseLogin.Contains(ignoreCaseKeywords);
                                                            if (isFriendMatch)
                                                            {
                                                                StackPanel friendsItem = new StackPanel();
                                                                friendsItem.Height = 35;
                                                                friendsItem.Orientation = Orientation.Horizontal;
                                                                Image friendAvatar = new Image();

                                                                Setter effectSetter = new Setter();
                                                                effectSetter.Property = ScrollViewer.EffectProperty;
                                                                effectSetter.Value = new DropShadowEffect
                                                                {
                                                                    ShadowDepth = 4,
                                                                    Direction = 330,
                                                                    Color = Colors.Green,
                                                                    Opacity = 0.5,
                                                                    BlurRadius = 4
                                                                };
                                                                Style dropShadowScrollViewerStyle = new Style(typeof(ScrollViewer));
                                                                dropShadowScrollViewerStyle.Setters.Add(effectSetter);
                                                                friendAvatar.Resources.Add(typeof(ScrollViewer), dropShadowScrollViewerStyle);

                                                                friendAvatar.Width = 25;
                                                                friendAvatar.Height = 25;
                                                                friendAvatar.Margin = new Thickness(5);
                                                                friendAvatar.BeginInit();
                                                                Uri friendAvatarUri = new Uri("https://cdn2.iconfinder.com/data/icons/ios-7-icons/50/user_male-128.png");
                                                                friendAvatar.Source = new BitmapImage(friendAvatarUri);
                                                                friendAvatar.EndInit();
                                                                friendsItem.Children.Add(friendAvatar);
                                                                TextBlock friendLoginLabel = new TextBlock();
                                                                friendLoginLabel.Height = 25;
                                                                friendLoginLabel.VerticalAlignment = VerticalAlignment.Center;
                                                                friendLoginLabel.Margin = new Thickness(10, 5, 10, 5);
                                                                friendLoginLabel.Text = friendLogin;
                                                                friendsItem.Children.Add(friendLoginLabel);
                                                                TextBlock friendStatusLabel = new TextBlock();
                                                                friendStatusLabel.Height = 25;
                                                                friendStatusLabel.VerticalAlignment = VerticalAlignment.Center;
                                                                friendStatusLabel.Margin = new Thickness(10, 5, 10, 5);
                                                                friendStatusLabel.Text = "не в сети";
                                                                bool isFriendOnline = friendStatus == "online";
                                                                bool isFriendPlayed = friendStatus == "played";
                                                                bool isFriendOffline = friendStatus == "offline";
                                                                friendStatusLabel.Foreground = offlineBrush;
                                                                if (isFriendOnline)
                                                                {
                                                                    friendLoginLabel.Foreground = onlineBrush;
                                                                    friendStatusLabel.Foreground = onlineBrush;
                                                                    friendStatusLabel.Text = "в сети";
                                                                }
                                                                else if (isFriendPlayed)
                                                                {
                                                                    friendLoginLabel.Foreground = playedBrush;
                                                                    friendStatusLabel.Foreground = playedBrush;
                                                                    friendStatusLabel.Text = "играет";
                                                                }
                                                                else if (isFriendOffline)
                                                                {
                                                                    friendLoginLabel.Foreground = offlineBrush;
                                                                    friendStatusLabel.Foreground = offlineBrush;
                                                                    friendStatusLabel.Text = "не в сети";
                                                                }
                                                                friendsItem.Children.Add(friendStatusLabel);
                                                                friends.Children.Add(friendsItem);
                                                                ContextMenu friendsItemContextMenu = new ContextMenu();
                                                                MenuItem friendsItemContextMenuItem = new MenuItem();
                                                                friendsItemContextMenuItem.Header = "Отправить сообщение";
                                                                friendsItemContextMenuItem.DataContext = friendId;
                                                                friendsItemContextMenuItem.Click += OpenChatHandler;
                                                                friendsItemContextMenu.Items.Add(friendsItemContextMenuItem);
                                                                friendsItemContextMenuItem = new MenuItem();
                                                                friendsItemContextMenuItem.DataContext = friendId;

                                                                bool isFavoriteFriend = false;
                                                                Environment.SpecialFolder localApplicationDataFolder = Environment.SpecialFolder.LocalApplicationData;
                                                                string localApplicationDataFolderPath = Environment.GetFolderPath(localApplicationDataFolder);
                                                                string saveDataFilePath = localApplicationDataFolderPath + @"\OfficeWare\GameManager\" + currentUserId + @"\save-data.txt";
                                                                js = new JavaScriptSerializer();
                                                                string saveDataFileContent = File.ReadAllText(saveDataFilePath);
                                                                SavedContent loadedContent = js.Deserialize<SavedContent>(saveDataFileContent);
                                                                List<Game> currentGames = loadedContent.games;
                                                                List<FriendSettings> currentFriends = loadedContent.friends;
                                                                List<FriendSettings> updatedFriends = currentFriends;
                                                                List<FriendSettings> cachedFriends = updatedFriends.Where<FriendSettings>((FriendSettings friend) =>
                                                                {
                                                                    return friend.id == friendId;
                                                                }).ToList();
                                                                int countCachedFriends = cachedFriends.Count;
                                                                bool isCachedFriendsExists = countCachedFriends >= 1;
                                                                if (isCachedFriendsExists)
                                                                {
                                                                    FriendSettings cachedFriend = cachedFriends[0];
                                                                    isFavoriteFriend = cachedFriend.isFavoriteFriend;
                                                                }

                                                                bool isFriendInFavorites = isFavoriteFriend;
                                                                if (isFriendInFavorites)
                                                                {
                                                                    friendsItemContextMenuItem.Header = "Добавить в избранные";
                                                                    friendsItemContextMenuItem.Click += AddFriendToFavoriteHandler;
                                                                }
                                                                else
                                                                {
                                                                    friendsItemContextMenuItem.Header = "Удалить из избранных";
                                                                    friendsItemContextMenuItem.Click += RemoveFriendFromFavoriteHandler;
                                                                }
                                                                friendsItemContextMenu.Items.Add(friendsItemContextMenuItem);
                                                                friendsItemContextMenuItem = new MenuItem();
                                                                friendsItemContextMenuItem.Header = "Управление";
                                                                MenuItem innerFriendsItemContextMenuItem = new MenuItem();
                                                                innerFriendsItemContextMenuItem.Header = "Удалить из друзей";
                                                                innerFriendsItemContextMenuItem.DataContext = friendId;
                                                                innerFriendsItemContextMenuItem.Click += RemoveFriendHandler;
                                                                friendsItemContextMenuItem.Items.Add(innerFriendsItemContextMenuItem);
                                                                innerFriendsItemContextMenuItem = new MenuItem();
                                                                innerFriendsItemContextMenuItem.Header = "Уведомления";
                                                                innerFriendsItemContextMenuItem.DataContext = friendId;
                                                                innerFriendsItemContextMenuItem.Click += OpenFriendNotificationsDialogHandler;
                                                                friendsItemContextMenuItem.Items.Add(innerFriendsItemContextMenuItem);
                                                                friendsItemContextMenu.Items.Add(friendsItemContextMenuItem);
                                                                friendsItem.ContextMenu = friendsItemContextMenu;
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

        public void RemoveFriendHandler (object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = ((MenuItem)(sender));
            object menuItemData = menuItem.DataContext;
            string friendId = ((string)(menuItemData));
            RemoveFriend(friendId);
        }

        public void RemoveFriend (string friendId)
        {
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("https://digitaldistributtionservice.herokuapp.com/api/friends/remove/?id=" + currentUserId + "&friend=" + friendId);
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
                                    friends = updatedFriends
                                });
                                File.WriteAllText(saveDataFilePath, savedContent);
                            }

                            MessageBox.Show("Друг был удален", "Внимание");
                            GetFriends(currentUserId, "");
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

        private void FilterFriendsHandler (object sender, TextChangedEventArgs e)
        {
            FilterFriends();
        }

        public void FilterFriends()
        {
            string keywords = keywordsLabel.Text;
            GetFriends(currentUserId, keywords);
        }

        private void OpenAddFriendDialogHandler(object sender, RoutedEventArgs e)
        {
            OpenAddFriendDialog();
        }

        public void OpenAddFriendDialog()
        {
            Dialogs.AddFriendDialog dialog = new Dialogs.AddFriendDialog(currentUserId);
            dialog.Show();
        }

        public void OpenChatHandler (object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = ((MenuItem)(sender));
            object menuItemData = menuItem.DataContext;
            string friend = ((string)(menuItemData));
            OpenChat(friend);
        }

        async public void OpenChat (string friend)
        {
            // await client.DisconnectAsync();
            Dialogs.ChatDialog dialog = new Dialogs.ChatDialog(currentUserId, client, friend);
            dialog.Show();
        }

        public void InitSocketsHandler (object sender, RoutedEventArgs e)
        {
            InitSockets();
        }

        public void OpenFriendNotificationsDialogHandler (object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = ((MenuItem)(sender));
            object menuItemData = menuItem.DataContext;
            string friendId = ((string)(menuItemData));
            OpenFriendNotificationsDialog(friendId);
        }

        public void OpenFriendNotificationsDialog (string friendId)
        {
            Dialogs.FriendNotificationsDialog dialog = new Dialogs.FriendNotificationsDialog(currentUserId);
            dialog.DataContext = friendId;
            dialog.Show();
        }

        async public void InitSockets ()
        {
            try
            {
                client.On("friend_is_online", async response =>
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        GetFriends(currentUserId, "");
                    });
                });
                client.On("friend_is_played", async response =>
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        GetFriends(currentUserId, "");
                    });
                });
                client.On("friend_is_toggle_status", async response =>
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        GetFriends(currentUserId, "");
                    });
                });
            }
            catch (System.Net.WebSockets.WebSocketException)
            {
                Debugger.Log(0, "debug", "Ошибка сокетов");
                await client.ConnectAsync();
            }
            catch (Exception)
            {
                Debugger.Log(0, "debug", "поток занят");
                await client.ConnectAsync();
            }
        }

        public void AddFriendToFavoriteHandler (object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = ((MenuItem)(sender));
            object menuItemData = menuItem.DataContext;
            string friend = ((string)(menuItemData));
            AddFriendToFavorite(friend);
        }

        public void AddFriendToFavorite (string currentFriendId)
        {
            Environment.SpecialFolder localApplicationDataFolder = Environment.SpecialFolder.LocalApplicationData;
            string localApplicationDataFolderPath = Environment.GetFolderPath(localApplicationDataFolder);
            string saveDataFilePath = localApplicationDataFolderPath + @"\OfficeWare\GameManager\" + currentUserId + @"\save-data.txt";
            JavaScriptSerializer js = new JavaScriptSerializer();
            string saveDataFileContent = File.ReadAllText(saveDataFilePath);
            SavedContent loadedContent = js.Deserialize<SavedContent>(saveDataFileContent);
            List<Game> currentGames = loadedContent.games;
            List<FriendSettings> currentFriends = loadedContent.friends;
            List<FriendSettings> updatedFriends = currentFriends;
            List<FriendSettings> cachedFriends = updatedFriends.Where<FriendSettings>((FriendSettings friend) =>
            {
                return friend.id == currentFriendId;
            }).ToList();
            int countCachedFriends = cachedFriends.Count;
            bool isCachedFriendsExists = countCachedFriends >= 1;
            if (isCachedFriendsExists)
            {
                FriendSettings cachedFriend = cachedFriends[0];
                cachedFriend.isFavoriteFriend = true;
                string savedContent = js.Serialize(new SavedContent
                {
                    games = currentGames,
                    friends = updatedFriends
                });
                File.WriteAllText(saveDataFilePath, savedContent);
            }
        }

        public void RemoveFriendFromFavoriteHandler (object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = ((MenuItem)(sender));
            object menuItemData = menuItem.DataContext;
            string friend = ((string)(menuItemData));
            RemoveFriendFromFavorite (friend);
        }

        public void RemoveFriendFromFavorite (string currentFriendId)
        {
            Environment.SpecialFolder localApplicationDataFolder = Environment.SpecialFolder.LocalApplicationData;
            string localApplicationDataFolderPath = Environment.GetFolderPath(localApplicationDataFolder);
            string saveDataFilePath = localApplicationDataFolderPath + @"\OfficeWare\GameManager\" + currentUserId + @"\save-data.txt";
            JavaScriptSerializer js = new JavaScriptSerializer();
            string saveDataFileContent = File.ReadAllText(saveDataFilePath);
            SavedContent loadedContent = js.Deserialize<SavedContent>(saveDataFileContent);
            List<Game> currentGames = loadedContent.games;
            List<FriendSettings> currentFriends = loadedContent.friends;
            List<FriendSettings> updatedFriends = currentFriends;
            List<FriendSettings> cachedFriends = updatedFriends.Where<FriendSettings>((FriendSettings friend) =>
            {
                return friend.id == currentFriendId;
            }).ToList();
            int countCachedFriends = cachedFriends.Count;
            bool isCachedFriendsExists = countCachedFriends >= 1;
            if (isCachedFriendsExists)
            {
                FriendSettings cachedFriend = cachedFriends[0];
                cachedFriend.isFavoriteFriend = false;
                string savedContent = js.Serialize(new SavedContent
                {
                    games = currentGames,
                    friends = updatedFriends
                });
                File.WriteAllText(saveDataFilePath, savedContent);
            }
        }

    }
}
