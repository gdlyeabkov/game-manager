using MaterialDesignThemes.Wpf;
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
        public TabControl mainControl;
        public List<string> chats = new List<string>();
        public MainWindow mainWindow;

        public FriendsDialog(string currentUserId, SocketIO client, TabControl mainControl, MainWindow mainWindow)
        {
            InitializeComponent();

            Initialize(currentUserId, client, mainControl, mainWindow);

        }

        public void Initialize (string currentUserId, SocketIO client, TabControl mainControl, MainWindow mainWindow)
        {
            InitializeConstants(client, mainControl, mainWindow);
            GetFriends(currentUserId, "");
            GetTalks();
            GetUserInfo();
            GetCategories();
        }

        public void GetCategories ()
        {

            categories.Children.Clear();
            Environment.SpecialFolder localApplicationDataFolder = Environment.SpecialFolder.LocalApplicationData;
            string localApplicationDataFolderPath = Environment.GetFolderPath(localApplicationDataFolder);
            string saveDataFilePath = localApplicationDataFolderPath + @"\OfficeWare\GameManager\" + currentUserId + @"\save-data.txt";
            JavaScriptSerializer js = new JavaScriptSerializer();
            string saveDataFileContent = File.ReadAllText(saveDataFilePath);
            SavedContent loadedContent = js.Deserialize<SavedContent>(saveDataFileContent);
            List<FriendSettings> currentFriends = loadedContent.friends;
            List<string> currentCategories = loadedContent.categories;
            Settings currentSettings = loadedContent.settings;
            bool isHideOfflineFriendsFromCategories = currentSettings.isHideOfflineFriendsFromCategories;
            foreach (string currentCategory in currentCategories)
            {
                TextBlock categoryLabel = new TextBlock();
                categoryLabel.FontSize = 14;
                categoryLabel.Margin = new Thickness(15);
                categoryLabel.Text = currentCategory;
                categories.Children.Add(categoryLabel);
                StackPanel category = new StackPanel();
                foreach (FriendSettings friend in currentFriends)
                {
                    List<string> friendCategories = friend.categories;
                    bool isHaveCategory = friendCategories.Contains(currentCategory);
                    if (isHaveCategory)
                    {
                        string friendId = friend.id;
                        try
                        {
                            HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/users/get/?id=" + friendId);
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
                                        string friendLogin = user.login;
                                        string friendStatus = user.status;
                                        bool isShowOfflineFriendsFromCategories = !isHideOfflineFriendsFromCategories;
                                        bool isOffline = friendStatus == "offline";
                                        bool isNotOffline = !isOffline;
                                        bool isAddFriend = isShowOfflineFriendsFromCategories || (isHideOfflineFriendsFromCategories && !isNotOffline);
                                        if (isAddFriend)
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
                                            friendStatusLabel.Text = "Не в сети";
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
                                            ContextMenu friendsItemContextMenu = new ContextMenu();
                                            MenuItem friendsItemContextMenuItem = new MenuItem();
                                            friendsItemContextMenuItem.Header = "Присоединиться к игре";
                                            friendsItemContextMenuItem.DataContext = friend.id;
                                            friendsItemContextMenuItem.Click += JoinToGameHandler;
                                            bool isGameSameForMe = true;
                                            if (isGameSameForMe)
                                            {
                                                friendsItemContextMenu.Items.Add(friendsItemContextMenuItem);
                                            }
                                            friendsItemContextMenuItem = new MenuItem();
                                            friendsItemContextMenuItem.Header = "Отправить сообщение";
                                            friendsItemContextMenuItem.DataContext = friendId;
                                            friendsItemContextMenuItem.Click += OpenChatHandler;
                                            friendsItemContextMenu.Items.Add(friendsItemContextMenuItem);
                                            friendsItemContextMenuItem = new MenuItem();
                                            friendsItemContextMenuItem.Header = "Открыть профиль";
                                            friendsItemContextMenuItem.DataContext = friendId;
                                            friendsItemContextMenuItem.Click += OpenFriendProfileHandler;
                                            friendsItemContextMenu.Items.Add(friendsItemContextMenuItem);
                                            friendsItemContextMenuItem = new MenuItem();
                                            friendsItemContextMenuItem.Header = "Управление";
                                            
                                            MenuItem innerFriendsItemContextMenuItem = new MenuItem();
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
                                                        List<Friend> friends = myInnerObj.friends;
                                                        int foundedIndex = friends.FindIndex((Friend localFriend) =>
                                                        {
                                                            bool isMyFriend = localFriend.user == currentUserId;
                                                            bool isCurrentFriend = localFriend.friend == friendId;
                                                            bool isCurrentFriendRelation = isMyFriend && isCurrentFriend;
                                                            return isCurrentFriendRelation;
                                                        });
                                                        bool isFound = foundedIndex >= 0;
                                                        if (isFound)
                                                        {
                                                            Friend currentFriend = friends[foundedIndex];
                                                            string currentFriendRelation = currentFriend._id;
                                                            string currentFriendAlias = currentFriend.alias;
                                                            int currentFriendAliasLength = currentFriendAlias.Length;
                                                            bool isAddNick = currentFriendAliasLength <= 0;
                                                            innerFriendsItemContextMenuItem.DataContext = currentFriendRelation;
                                                            if (isAddNick)
                                                            {
                                                                innerFriendsItemContextMenuItem.Header = "Добавить ник";
                                                                innerFriendsItemContextMenuItem.Click += AddFriendNickHandler;
                                                            }
                                                            else
                                                            {
                                                                innerFriendsItemContextMenuItem.Header = "Изменить ник";
                                                                innerFriendsItemContextMenuItem.Click += UpdateFriendNickHandler;
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            friendsItemContextMenuItem.Items.Add(innerFriendsItemContextMenuItem);
                                            innerFriendsItemContextMenuItem = new MenuItem();
                                            innerFriendsItemContextMenuItem.Header = "Удалить из друзей";
                                            innerFriendsItemContextMenuItem.DataContext = friendId;
                                            innerFriendsItemContextMenuItem.Click += RemoveFriendHandler;
                                            friendsItemContextMenuItem.Items.Add(innerFriendsItemContextMenuItem);
                                            innerFriendsItemContextMenuItem = new MenuItem();
                                            innerFriendsItemContextMenuItem.DataContext = friendId;
                                            bool isFavoriteFriend = false;
                                            List<Game> currentGames = loadedContent.games;
                                            List<FriendSettings> updatedFriends = currentFriends;
                                            List<FriendSettings> cachedFriends = updatedFriends.Where<FriendSettings>((FriendSettings localFriend) =>
                                            {
                                                return localFriend.id == friendId;
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
                                                innerFriendsItemContextMenuItem.Header = "Удалить из избранных";
                                                innerFriendsItemContextMenuItem.Click += RemoveFriendFromFavoriteHandler;
                                            }
                                            else
                                            {
                                                innerFriendsItemContextMenuItem.Header = "Добавить в избранные";
                                                innerFriendsItemContextMenuItem.Click += AddFriendToFavoriteHandler;
                                            }
                                            friendsItemContextMenuItem.Items.Add(innerFriendsItemContextMenuItem);

                                            innerFriendsItemContextMenuItem = new MenuItem();
                                            innerFriendsItemContextMenuItem.Header = "Добавить в категорию";
                                            innerFriendsItemContextMenuItem.DataContext = friendId;
                                            innerFriendsItemContextMenuItem.Click += OpenCategoryDialogHandler;
                                            friendsItemContextMenuItem.Items.Add(innerFriendsItemContextMenuItem);

                                            innerFriendsItemContextMenuItem = new MenuItem();
                                            innerFriendsItemContextMenuItem.Header = "Уведомления";
                                            innerFriendsItemContextMenuItem.DataContext = friendId;
                                            innerFriendsItemContextMenuItem.Click += OpenFriendNotificationsDialogHandler;
                                            friendsItemContextMenuItem.Items.Add(innerFriendsItemContextMenuItem);
                                            innerFriendsItemContextMenuItem = new MenuItem();
                                            try
                                            {
                                                HttpWebRequest innerNestedWebRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/blacklist/relations/all");
                                                innerNestedWebRequest.Method = "GET";
                                                innerNestedWebRequest.UserAgent = ".NET Framework Test Client";
                                                using (HttpWebResponse innerNestedWebResponse = (HttpWebResponse)innerNestedWebRequest.GetResponse())
                                                {
                                                    using (var innerNestedReader = new StreamReader(innerNestedWebResponse.GetResponseStream()))
                                                    {
                                                        js = new JavaScriptSerializer();
                                                        objText = innerNestedReader.ReadToEnd();
                                                        BlackListRelationsResponseInfo myInnerNestedObj = (BlackListRelationsResponseInfo)js.Deserialize(objText, typeof(BlackListRelationsResponseInfo));
                                                        status = myInnerNestedObj.status;
                                                        isOkStatus = status == "OK";
                                                        if (isOkStatus)
                                                        {
                                                            List<BlackListRelation> relations = myInnerNestedObj.relations;
                                                            List<BlackListRelation> results = relations.Where<BlackListRelation>((BlackListRelation relation) =>
                                                            {
                                                                bool isMyFriendInBlackList = relation.user == currentUserId && relation.friend == friendId;
                                                                return isMyFriendInBlackList;
                                                            }).ToList<BlackListRelation>();
                                                            int countResults = results.Count;
                                                            bool isHaveResults = countResults >= 1;
                                                            if (isHaveResults)
                                                            {
                                                                innerFriendsItemContextMenuItem.Header = "Удалить из черного списка";
                                                                innerFriendsItemContextMenuItem.Click += RemoveFromBlackListHandler;
                                                            }
                                                            else
                                                            {
                                                                innerFriendsItemContextMenuItem.Header = "Добавить в черный список";
                                                                innerFriendsItemContextMenuItem.Click += AddToBlackListHandler;
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
                                            innerFriendsItemContextMenuItem.DataContext = friend.id;
                                            friendsItemContextMenuItem.Items.Add(innerFriendsItemContextMenuItem);

                                            friendsItemContextMenu.Items.Add(friendsItemContextMenuItem);
                                            friendsItem.ContextMenu = friendsItemContextMenu;
                                            category.Children.Add(friendsItem);
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
                categories.Children.Add(category);
            }
        }

        public void GetUserInfo ()
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
                            string userName = user.name;
                            string userStatus = user.status;
                            userProfileAvatar.BeginInit();
                            userProfileAvatar.Source = new BitmapImage(new Uri(@"http://localhost:4000/api/user/avatar/?id=" + currentUserId));
                            userProfileAvatar.EndInit();
                            userProfileNameLabel.Text = userName;
                            userProfileStatusLabel.Text = userStatus;
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

        public void InitializeConstants (SocketIO client, TabControl mainControl, MainWindow mainWindow)
        {
            this.DataContext = null;
            this.client = client;
            onlineBrush = System.Windows.Media.Brushes.Blue;
            playedBrush = System.Windows.Media.Brushes.Green;
            offlineBrush = System.Windows.Media.Brushes.LightGray;
            this.mainControl = mainControl;
            this.mainWindow = mainWindow;
        }

        public void GetFriends(string currentUserId, string keywords)
        {

            Environment.SpecialFolder localApplicationDataFolder = Environment.SpecialFolder.LocalApplicationData;
            string localApplicationDataFolderPath = Environment.GetFolderPath(localApplicationDataFolder);
            string saveDataFilePath = localApplicationDataFolderPath + @"\OfficeWare\GameManager\" + currentUserId + @"\save-data.txt";
            JavaScriptSerializer js = new JavaScriptSerializer();
            string saveDataFileContent = File.ReadAllText(saveDataFilePath);
            SavedContent loadedContent = js.Deserialize<SavedContent>(saveDataFileContent);
            Settings currentSettings = loadedContent.settings;

            this.currentUserId = currentUserId;
            try
            {
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
                            customPlayedCategories.Children.Clear();
                            // playedFriends.Children.Clear();
                            onlineFriends.Children.Clear();
                            offlineFriends.Children.Clear();
                            friends.Children.Clear();
                            User currentUser = myobj.user;

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
                                        List<string> friendsIds = new List<string>();
                                        foreach (Friend friendInfo in myFriends)
                                        {
                                            string friendId = friendInfo.friend;
                                            friendsIds.Add(friendId);
                                        }
                                        int friendsPlayCursor = 0;
                                        int friendsOnlineCursor = 0;
                                        int friendsOfflineCursor = 0;

                                        List<string> playedCategories = new List<string>();
                                        foreach (Friend friendInfo in myFriends)
                                        {
                                            string friendId = friendInfo.friend;
                                            bool isMyFriend = friendsIds.Contains(friendId);
                                            if (isMyFriend)
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
                                                            string localFriendId = friend._id;
                                                            string friendLogin = friend.login;
                                                            string friendStatus = friend.status;
                                                            string friendIgnoreCaseLogin = friendLogin.ToLower();
                                                            string ignoreCaseKeywords = keywords.ToLower();
                                                            bool isFriendMatch = friendIgnoreCaseLogin.Contains(ignoreCaseKeywords);
                                                            if (isFriendMatch)
                                                            {
                                                                bool isFriendPlayed = friendStatus == "played";
                                                                if (isFriendPlayed)
                                                                {
                                                                    string lastGameId = friend.lastGame;
                                                                    playedCategories.Add(lastGameId);
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        playedCategories = playedCategories.OrderBy(category => category).ToList<string>();
                                        foreach (string playedCategory in playedCategories)
                                        {
                                            HttpWebRequest innerNestedWebRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/games/get");
                                            innerNestedWebRequest.Method = "GET";
                                            innerNestedWebRequest.UserAgent = ".NET Framework Test Client";
                                            using (HttpWebResponse innerNestedWebResponse = (HttpWebResponse)innerNestedWebRequest.GetResponse())
                                            {
                                                using (var innerNestedReader = new StreamReader(innerNestedWebResponse.GetResponseStream()))
                                                {
                                                    js = new JavaScriptSerializer();
                                                    objText = innerNestedReader.ReadToEnd();
                                                    GamesListResponseInfo myInnerNestedObj = (GamesListResponseInfo)js.Deserialize(objText, typeof(GamesListResponseInfo));
                                                    status = myInnerNestedObj.status;
                                                    isOkStatus = status == "OK";
                                                    if (isOkStatus)
                                                    {
                                                        List<GameResponseInfo> games = myInnerNestedObj.games;
                                                        int foundIndex = games.FindIndex((GameResponseInfo game) =>
                                                        {
                                                            string gameId = game._id;
                                                            bool isGameFound = gameId == playedCategory;
                                                            return isGameFound;
                                                        });
                                                        bool isFound = foundIndex >= 0;
                                                        if (isFound)
                                                        {
                                                            GameResponseInfo game = games[foundIndex];
                                                            string gameName = game.name;
                                                            TextBlock categoryLabel = new TextBlock();
                                                            categoryLabel.FontSize = 14;
                                                            categoryLabel.Margin = new Thickness(15);
                                                            categoryLabel.Text = gameName;
                                                            customPlayedCategories.Children.Add(categoryLabel);
                                                            int countCurrentGamePlayedFriendsCursor = 0;

                                                            foreach (Friend friendInfo in myFriends)
                                                            {
                                                                string friendId = friendInfo.friend;
                                                                bool isMyFriend = friendsIds.Contains(friendId);
                                                                if (isMyFriend)
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
                                                                                string localFriendId = friend._id;
                                                                                string localFriendLastGame = friend.lastGame;
                                                                                string friendLogin = friend.login;
                                                                                string friendStatus = friend.status;
                                                                                string friendIgnoreCaseLogin = friendLogin.ToLower();
                                                                                string ignoreCaseKeywords = keywords.ToLower();
                                                                                bool isFriendMatch = friendIgnoreCaseLogin.Contains(ignoreCaseKeywords);
                                                                                bool isCurrentGame = localFriendLastGame == playedCategory;
                                                                                bool isAddFriend = isFriendMatch && isCurrentGame;
                                                                                if (isAddFriend)
                                                                                {
                                                                                    
                                                                                    countCurrentGamePlayedFriendsCursor++;
                                                                                    
                                                                                    StackPanel friendsItem = new StackPanel();

                                                                                    int friendsItemHeight = 35;
                                                                                    bool isFriendlistAndChatsCompactView = currentSettings.isFriendListAndChatsCompactView;
                                                                                    if (isFriendlistAndChatsCompactView)
                                                                                    {
                                                                                        friendsItemHeight = 35;
                                                                                    }
                                                                                    else
                                                                                    {
                                                                                        friendsItemHeight = 50;
                                                                                    }
                                                                                    friendsItem.Height = friendsItemHeight;

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
                                                                                    friendStatusLabel.Text = "Не в сети";
                                                                                    bool isFriendOnline = friendStatus == "online";
                                                                                    bool isFriendPlayed = friendStatus == "played";
                                                                                    bool isFriendOffline = friendStatus == "offline";
                                                                                    friendStatusLabel.Foreground = offlineBrush;
                                                                                    if (isFriendPlayed)
                                                                                    {
                                                                                        friendsPlayCursor++;
                                                                                        friendLoginLabel.Foreground = playedBrush;
                                                                                        friendStatusLabel.Foreground = playedBrush;
                                                                                        friendStatusLabel.Text = "играет";
                                                                                        customPlayedCategories.Children.Add(friendsItem);
                                                                                    }
                                                                                    friendsItem.Children.Add(friendStatusLabel);
                                                                                    ContextMenu friendsItemContextMenu = new ContextMenu();
                                                                                    MenuItem friendsItemContextMenuItem = new MenuItem();
                                                                                    friendsItemContextMenuItem.Header = "Присоединиться к игре";
                                                                                    friendsItemContextMenuItem.DataContext = friendId;
                                                                                    friendsItemContextMenuItem.Click += JoinToGameHandler;
                                                                                    bool isGameSameForMe = true;
                                                                                    if (isGameSameForMe)
                                                                                    {
                                                                                        friendsItemContextMenu.Items.Add(friendsItemContextMenuItem);
                                                                                    }
                                                                                    friendsItemContextMenuItem = new MenuItem();
                                                                                    friendsItemContextMenuItem.Header = "Отправить сообщение";
                                                                                    friendsItemContextMenuItem.DataContext = friendId;
                                                                                    friendsItemContextMenuItem.Click += OpenChatHandler;
                                                                                    friendsItemContextMenu.Items.Add(friendsItemContextMenuItem);
                                                                                    friendsItemContextMenuItem = new MenuItem();
                                                                                    friendsItemContextMenuItem.Header = "Открыть профиль";
                                                                                    friendsItemContextMenuItem.DataContext = friendId;
                                                                                    friendsItemContextMenuItem.Click += OpenFriendProfileHandler;
                                                                                    friendsItemContextMenu.Items.Add(friendsItemContextMenuItem);
                                                                                    friendsItemContextMenuItem = new MenuItem();
                                                                                    friendsItemContextMenuItem.Header = "Управление";

                                                                                    MenuItem innerFriendsItemContextMenuItem = new MenuItem();
                                                                                    HttpWebRequest friendRelationsWebRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/friends/get");
                                                                                    friendRelationsWebRequest.Method = "GET";
                                                                                    friendRelationsWebRequest.UserAgent = ".NET Framework Test Client";
                                                                                    using (HttpWebResponse friendRelationsWebResponse = (HttpWebResponse)friendRelationsWebRequest.GetResponse())
                                                                                    {
                                                                                        using (var friendRelationsReader = new StreamReader(friendRelationsWebResponse.GetResponseStream()))
                                                                                        {
                                                                                            js = new JavaScriptSerializer();
                                                                                            objText = friendRelationsReader.ReadToEnd();
                                                                                            FriendsResponseInfo myFriendRelationsObj = (FriendsResponseInfo)js.Deserialize(objText, typeof(FriendsResponseInfo));
                                                                                            status = myFriendRelationsObj.status;
                                                                                            isOkStatus = status == "OK";
                                                                                            if (isOkStatus)
                                                                                            {
                                                                                                List<Friend> friends = myFriendRelationsObj.friends;
                                                                                                int foundedIndex = friends.FindIndex((Friend localFriend) =>
                                                                                                {
                                                                                                    bool isLocalMyFriend = localFriend.user == currentUserId;
                                                                                                    bool isCurrentFriend = localFriend.friend == friendId;
                                                                                                    bool isCurrentFriendRelation = isLocalMyFriend && isCurrentFriend;
                                                                                                    return isCurrentFriendRelation;
                                                                                                });
                                                                                                bool isFriendFound = foundedIndex >= 0;
                                                                                                if (isFriendFound)
                                                                                                {
                                                                                                    Friend currentFriend = friends[foundedIndex];
                                                                                                    string currentFriendRelation = currentFriend._id;
                                                                                                    string currentFriendAlias = currentFriend.alias;
                                                                                                    int currentFriendAliasLength = currentFriendAlias.Length;
                                                                                                    bool isAddNick = currentFriendAliasLength <= 0;
                                                                                                    innerFriendsItemContextMenuItem.DataContext = currentFriendRelation;
                                                                                                    if (isAddNick)
                                                                                                    {
                                                                                                        innerFriendsItemContextMenuItem.Header = "Добавить ник";
                                                                                                        innerFriendsItemContextMenuItem.Click += AddFriendNickHandler;
                                                                                                    }
                                                                                                    else
                                                                                                    {
                                                                                                        innerFriendsItemContextMenuItem.Header = "Изменить ник";
                                                                                                        innerFriendsItemContextMenuItem.Click += UpdateFriendNickHandler;
                                                                                                    }
                                                                                                }
                                                                                            }
                                                                                        }
                                                                                    }
                                                                                    friendsItemContextMenuItem.Items.Add(innerFriendsItemContextMenuItem);
                                                                                    innerFriendsItemContextMenuItem = new MenuItem();

                                                                                    innerFriendsItemContextMenuItem.Header = "Удалить из друзей";
                                                                                    innerFriendsItemContextMenuItem.DataContext = friendId;
                                                                                    innerFriendsItemContextMenuItem.Click += RemoveFriendHandler;
                                                                                    friendsItemContextMenuItem.Items.Add(innerFriendsItemContextMenuItem);
                                                                                    innerFriendsItemContextMenuItem = new MenuItem();
                                                                                    innerFriendsItemContextMenuItem.DataContext = friendId;
                                                                                    bool isFavoriteFriend = false;

                                                                                    List<Game> currentGames = loadedContent.games;
                                                                                    List<FriendSettings> currentFriends = loadedContent.friends;
                                                                                    List<FriendSettings> updatedFriends = currentFriends;
                                                                                    List<FriendSettings> cachedFriends = updatedFriends.Where<FriendSettings>((FriendSettings localFriend) =>
                                                                                    {
                                                                                        return localFriend.id == friendId;
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
                                                                                        innerFriendsItemContextMenuItem.Header = "Удалить из избранных";
                                                                                        innerFriendsItemContextMenuItem.Click += RemoveFriendFromFavoriteHandler;
                                                                                    }
                                                                                    else
                                                                                    {
                                                                                        innerFriendsItemContextMenuItem.Header = "Добавить в избранные";
                                                                                        innerFriendsItemContextMenuItem.Click += AddFriendToFavoriteHandler;
                                                                                    }
                                                                                    friendsItemContextMenuItem.Items.Add(innerFriendsItemContextMenuItem);

                                                                                    innerFriendsItemContextMenuItem = new MenuItem();
                                                                                    innerFriendsItemContextMenuItem.Header = "Добавить в категорию";
                                                                                    innerFriendsItemContextMenuItem.DataContext = friendId;
                                                                                    innerFriendsItemContextMenuItem.Click += OpenCategoryDialogHandler;
                                                                                    friendsItemContextMenuItem.Items.Add(innerFriendsItemContextMenuItem);

                                                                                    innerFriendsItemContextMenuItem = new MenuItem();
                                                                                    innerFriendsItemContextMenuItem.Header = "Уведомления";
                                                                                    innerFriendsItemContextMenuItem.DataContext = friendId;
                                                                                    innerFriendsItemContextMenuItem.Click += OpenFriendNotificationsDialogHandler;
                                                                                    friendsItemContextMenuItem.Items.Add(innerFriendsItemContextMenuItem);
                                                                                    innerFriendsItemContextMenuItem = new MenuItem();
                                                                                    try
                                                                                    {
                                                                                        HttpWebRequest friendWebRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/blacklist/relations/all");
                                                                                        friendWebRequest.Method = "GET";
                                                                                        friendWebRequest.UserAgent = ".NET Framework Test Client";
                                                                                        using (HttpWebResponse friendWebResponse = (HttpWebResponse)friendWebRequest.GetResponse())
                                                                                        {
                                                                                            using (var friendReader = new StreamReader(friendWebResponse.GetResponseStream()))
                                                                                            {
                                                                                                js = new JavaScriptSerializer();
                                                                                                objText = friendReader.ReadToEnd();
                                                                                                BlackListRelationsResponseInfo myFriendObj = (BlackListRelationsResponseInfo)js.Deserialize(objText, typeof(BlackListRelationsResponseInfo));
                                                                                                status = myFriendObj.status;
                                                                                                isOkStatus = status == "OK";
                                                                                                if (isOkStatus)
                                                                                                {
                                                                                                    List<BlackListRelation> relations = myFriendObj.relations;
                                                                                                    List<BlackListRelation> results = relations.Where<BlackListRelation>((BlackListRelation relation) =>
                                                                                                    {
                                                                                                        bool isMyFriendInBlackList = relation.user == currentUserId && relation.friend == friendId;
                                                                                                        return isMyFriendInBlackList;
                                                                                                    }).ToList<BlackListRelation>();
                                                                                                    int countResults = results.Count;
                                                                                                    bool isHaveResults = countResults >= 1;
                                                                                                    if (isHaveResults)
                                                                                                    {
                                                                                                        innerFriendsItemContextMenuItem.Header = "Удалить из черного списка";
                                                                                                        innerFriendsItemContextMenuItem.Click += RemoveFromBlackListHandler;
                                                                                                    }
                                                                                                    else
                                                                                                    {
                                                                                                        innerFriendsItemContextMenuItem.Header = "Добавить в черный список";
                                                                                                        innerFriendsItemContextMenuItem.Click += AddToBlackListHandler;
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
                                                                                    innerFriendsItemContextMenuItem.DataContext = friendId;
                                                                                    friendsItemContextMenuItem.Items.Add(innerFriendsItemContextMenuItem);

                                                                                    friendsItemContextMenu.Items.Add(friendsItemContextMenuItem);
                                                                                    friendsItem.ContextMenu = friendsItemContextMenu;
                                                                                }
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }

                                                            string rawCountCurrentGamePlayedFriendsCursor = countCurrentGamePlayedFriendsCursor.ToString();
                                                            string categoryLabelContent = gameName + " (" + rawCountCurrentGamePlayedFriendsCursor + ")";
                                                            categoryLabel.Text = categoryLabelContent;

                                                            Separator categorySeparator = new Separator();
                                                            customPlayedCategories.Children.Add(categorySeparator);

                                                        }
                                                    }
                                                }
                                            }
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
                                                            User friend = myobj.user;
                                                            string localFriendId = friend._id;
                                                            string friendLogin = friend.login;
                                                            string friendStatus = friend.status;
                                                            string friendIgnoreCaseLogin = friendLogin.ToLower();
                                                            string ignoreCaseKeywords = keywords.ToLower();
                                                            bool isFriendMatch = friendIgnoreCaseLogin.Contains(ignoreCaseKeywords);
                                                            if (isFriendMatch || true)
                                                            {
                                                                StackPanel friendsItem = new StackPanel();

                                                                int friendsItemHeight = 35;
                                                                bool isFriendlistAndChatsCompactView = currentSettings.isFriendListAndChatsCompactView;
                                                                if (isFriendlistAndChatsCompactView)
                                                                {
                                                                    friendsItemHeight = 35;
                                                                }
                                                                else
                                                                {
                                                                    friendsItemHeight = 50;
                                                                }
                                                                friendsItem.Height = friendsItemHeight;
                                                                
                                                                
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

                                                                HttpWebRequest innerNestedWebRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/friends/get");
                                                                innerNestedWebRequest.Method = "GET";
                                                                innerNestedWebRequest.UserAgent = ".NET Framework Test Client";
                                                                using (HttpWebResponse innerNestedWebResponse = (HttpWebResponse)innerNestedWebRequest.GetResponse())
                                                                {
                                                                    using (var innerNestedReader = new StreamReader(innerNestedWebResponse.GetResponseStream()))
                                                                    {
                                                                        js = new JavaScriptSerializer();
                                                                        objText = innerNestedReader.ReadToEnd();
                                                                        FriendsResponseInfo myInnerNestedObj = (FriendsResponseInfo)js.Deserialize(objText, typeof(FriendsResponseInfo));
                                                                        status = myInnerNestedObj.status;
                                                                        isOkStatus = status == "OK";
                                                                        if (isOkStatus)
                                                                        {
                                                                            List<Friend> friendRelations = myInnerNestedObj.friends;
                                                                            bool isAddNickAfterFriendNames = currentSettings.isAddNickAfterFriendNames;
                                                                            if (isAddNickAfterFriendNames)
                                                                            {
                                                                                int foundIndex = friendRelations.FindIndex((Friend localFriend) =>
                                                                                {
                                                                                    string localUserRelationId = localFriend.user;
                                                                                    string localFriendRelationId = localFriend.friend;
                                                                                    bool isCurrentUser = currentUserId == localUserRelationId;
                                                                                    bool isCurrentFriend = friendId == localFriendRelationId;
                                                                                    return isCurrentUser && isCurrentFriend;
                                                                                });
                                                                                bool isFound = foundIndex >= 0;
                                                                                if (isFound)
                                                                                {
                                                                                    Friend currentFriendRelation = friendRelations[foundIndex];
                                                                                    string friendAlias = currentFriendRelation.alias;
                                                                                    int friendAliasLength = friendAlias.Length;
                                                                                    bool isAliasExists = friendAliasLength >= 1;
                                                                                    if (isAliasExists)
                                                                                    {
                                                                                        friendLoginLabel.Text += @" (" + friendAlias + ")";
                                                                                    }
                                                                                }
                                                                            }
                                                                        }
                                                                    }
                                                                }

                                                                TextBlock friendStatusLabel = new TextBlock();
                                                                friendStatusLabel.Height = 25;
                                                                friendStatusLabel.VerticalAlignment = VerticalAlignment.Center;
                                                                friendStatusLabel.Margin = new Thickness(10, 5, 10, 5);
                                                                friendStatusLabel.Text = "Не в сети";
                                                                bool isFriendOnline = friendStatus == "online";
                                                                bool isFriendPlayed = friendStatus == "played";
                                                                bool isFriendOffline = friendStatus == "offline";
                                                                friendStatusLabel.Foreground = offlineBrush;
                                                                if (isFriendOnline)
                                                                {
                                                                    friendsOnlineCursor++;
                                                                    friendLoginLabel.Foreground = onlineBrush;
                                                                    friendStatusLabel.Foreground = onlineBrush;
                                                                    friendStatusLabel.Text = "в сети";
                                                                    onlineFriends.Children.Add(friendsItem);
                                                                }
                                                                else if (isFriendPlayed)
                                                                {
                                                                    friendsPlayCursor++;
                                                                    friendLoginLabel.Foreground = playedBrush;
                                                                    friendStatusLabel.Foreground = playedBrush;
                                                                    friendStatusLabel.Text = "играет";
                                                                    // playedFriends.Children.Add(friendsItem);
                                                                }
                                                                else if (isFriendOffline)
                                                                {
                                                                    friendsOfflineCursor++;
                                                                    friendLoginLabel.Foreground = offlineBrush;
                                                                    friendStatusLabel.Foreground = offlineBrush;
                                                                    friendStatusLabel.Text = "не в сети";
                                                                    offlineFriends.Children.Add(friendsItem);
                                                                }
                                                                friendsItem.Children.Add(friendStatusLabel);
                                                                // friends.Children.Add(friendsItem);
                                                                ContextMenu friendsItemContextMenu = new ContextMenu();
                                                                MenuItem friendsItemContextMenuItem = new MenuItem();
                                                                friendsItemContextMenuItem.Header = "Присоединиться к игре";
                                                                friendsItemContextMenuItem.DataContext = friendId;
                                                                friendsItemContextMenuItem.Click += JoinToGameHandler;
                                                                bool isGameSameForMe = true;
                                                                if (isGameSameForMe)
                                                                {
                                                                    friendsItemContextMenu.Items.Add(friendsItemContextMenuItem);
                                                                }
                                                                friendsItemContextMenuItem = new MenuItem();
                                                                friendsItemContextMenuItem.Header = "Отправить сообщение";
                                                                friendsItemContextMenuItem.DataContext = friendId;
                                                                friendsItemContextMenuItem.Click += OpenChatHandler;
                                                                friendsItemContextMenu.Items.Add(friendsItemContextMenuItem);
                                                                friendsItemContextMenuItem = new MenuItem();
                                                                friendsItemContextMenuItem.Header = "Открыть профиль";
                                                                friendsItemContextMenuItem.DataContext = friendId;
                                                                friendsItemContextMenuItem.Click += OpenFriendProfileHandler;
                                                                friendsItemContextMenu.Items.Add(friendsItemContextMenuItem);

                                                                friendsItemContextMenuItem = new MenuItem();
                                                                friendsItemContextMenuItem.Header = "Обмен";
                                                                friendsItemContextMenuItem.DataContext = friendId;
                                                                friendsItemContextMenuItem.Click += OpenUserProfileHandler;
                                                                MenuItem innerFriendsItemContextMenuItem = new MenuItem();
                                                                innerFriendsItemContextMenuItem.Header = "Открыть инвентарь";
                                                                friendsItemContextMenuItem.Items.Add(innerFriendsItemContextMenuItem);
                                                                innerFriendsItemContextMenuItem = new MenuItem();
                                                                innerFriendsItemContextMenuItem.Header = "Отправить предложение обмена";
                                                                friendsItemContextMenuItem.Items.Add(innerFriendsItemContextMenuItem);
                                                                friendsItemContextMenu.Items.Add(friendsItemContextMenuItem);

                                                                friendsItemContextMenuItem = new MenuItem();
                                                                friendsItemContextMenuItem.Header = "Управление";

                                                                innerFriendsItemContextMenuItem = new MenuItem();
                                                                HttpWebRequest friendRelationsWebRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/friends/get");
                                                                friendRelationsWebRequest.Method = "GET";
                                                                friendRelationsWebRequest.UserAgent = ".NET Framework Test Client";
                                                                using (HttpWebResponse friendRelationsWebResponse = (HttpWebResponse)friendRelationsWebRequest.GetResponse())
                                                                {
                                                                    using (var friendRelationsReader = new StreamReader(friendRelationsWebResponse.GetResponseStream()))
                                                                    {
                                                                        js = new JavaScriptSerializer();
                                                                        objText = friendRelationsReader.ReadToEnd();
                                                                        FriendsResponseInfo myFriendRelationsObj = (FriendsResponseInfo)js.Deserialize(objText, typeof(FriendsResponseInfo));
                                                                        status = myFriendRelationsObj.status;
                                                                        isOkStatus = status == "OK";
                                                                        if (isOkStatus)
                                                                        {
                                                                            List<Friend> friends = myFriendRelationsObj.friends;
                                                                            int foundedIndex = friends.FindIndex((Friend localFriend) =>
                                                                            {
                                                                                bool isLocalMyFriend = localFriend.user == currentUserId;
                                                                                bool isCurrentFriend = localFriend.friend == friendId;
                                                                                bool isCurrentFriendRelation = isLocalMyFriend && isCurrentFriend;
                                                                                return isCurrentFriendRelation;
                                                                            });
                                                                            bool isFriendFound = foundedIndex >= 0;
                                                                            if (isFriendFound)
                                                                            {
                                                                                Friend currentFriend = friends[foundedIndex];
                                                                                string currentFriendRelation = currentFriend._id;
                                                                                string currentFriendAlias = currentFriend.alias;
                                                                                int currentFriendAliasLength = currentFriendAlias.Length;
                                                                                bool isAddNick = currentFriendAliasLength <= 0;
                                                                                innerFriendsItemContextMenuItem.DataContext = currentFriendRelation;
                                                                                if (isAddNick)
                                                                                {
                                                                                    innerFriendsItemContextMenuItem.Header = "Добавить ник";
                                                                                    innerFriendsItemContextMenuItem.Click += AddFriendNickHandler;
                                                                                }
                                                                                else
                                                                                {
                                                                                    innerFriendsItemContextMenuItem.Header = "Изменить ник";
                                                                                    innerFriendsItemContextMenuItem.Click += UpdateFriendNickHandler;
                                                                                }
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                                friendsItemContextMenuItem.Items.Add(innerFriendsItemContextMenuItem);
                                                                innerFriendsItemContextMenuItem = new MenuItem();

                                                                innerFriendsItemContextMenuItem.Header = "Удалить из друзей";
                                                                innerFriendsItemContextMenuItem.DataContext = friendId;
                                                                innerFriendsItemContextMenuItem.Click += RemoveFriendHandler;
                                                                friendsItemContextMenuItem.Items.Add(innerFriendsItemContextMenuItem);
                                                                innerFriendsItemContextMenuItem = new MenuItem();
                                                                innerFriendsItemContextMenuItem.DataContext = friendId;
                                                                bool isFavoriteFriend = false;
                                                                
                                                                List<Game> currentGames = loadedContent.games;
                                                                List<FriendSettings> currentFriends = loadedContent.friends;
                                                                List<FriendSettings> updatedFriends = currentFriends;
                                                                List<FriendSettings> cachedFriends = updatedFriends.Where<FriendSettings>((FriendSettings localFriend) =>
                                                                {
                                                                    return localFriend.id == friendId;
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
                                                                    innerFriendsItemContextMenuItem.Header = "Удалить из избранных";
                                                                    innerFriendsItemContextMenuItem.Click += RemoveFriendFromFavoriteHandler;
                                                                }
                                                                else
                                                                {
                                                                    innerFriendsItemContextMenuItem.Header = "Добавить в избранные";
                                                                    innerFriendsItemContextMenuItem.Click += AddFriendToFavoriteHandler;
                                                                }
                                                                friendsItemContextMenuItem.Items.Add(innerFriendsItemContextMenuItem);

                                                                innerFriendsItemContextMenuItem = new MenuItem();
                                                                innerFriendsItemContextMenuItem.Header = "Добавить в категорию";
                                                                innerFriendsItemContextMenuItem.DataContext = friendId;
                                                                innerFriendsItemContextMenuItem.Click += OpenCategoryDialogHandler;
                                                                friendsItemContextMenuItem.Items.Add(innerFriendsItemContextMenuItem);

                                                                innerFriendsItemContextMenuItem = new MenuItem();
                                                                innerFriendsItemContextMenuItem.Header = "Уведомления";
                                                                innerFriendsItemContextMenuItem.DataContext = friendId;
                                                                innerFriendsItemContextMenuItem.Click += OpenFriendNotificationsDialogHandler;
                                                                friendsItemContextMenuItem.Items.Add(innerFriendsItemContextMenuItem);
                                                                innerFriendsItemContextMenuItem = new MenuItem();
                                                                try
                                                                {
                                                                    innerNestedWebRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/blacklist/relations/all");
                                                                    innerNestedWebRequest.Method = "GET";
                                                                    innerNestedWebRequest.UserAgent = ".NET Framework Test Client";
                                                                    using (HttpWebResponse innerNestedWebResponse = (HttpWebResponse)innerNestedWebRequest.GetResponse())
                                                                    {
                                                                        using (var innerNestedReader = new StreamReader(innerNestedWebResponse.GetResponseStream()))
                                                                        {
                                                                            js = new JavaScriptSerializer();
                                                                            objText = innerNestedReader.ReadToEnd();
                                                                            BlackListRelationsResponseInfo myInnerNestedObj = (BlackListRelationsResponseInfo)js.Deserialize(objText, typeof(BlackListRelationsResponseInfo));
                                                                            status = myInnerNestedObj.status;
                                                                            isOkStatus = status == "OK";
                                                                            if (isOkStatus)
                                                                            {
                                                                                List<BlackListRelation> relations = myInnerNestedObj.relations;
                                                                                List<BlackListRelation> results = relations.Where<BlackListRelation>((BlackListRelation relation) =>
                                                                                {
                                                                                    bool isMyFriendInBlackList = relation.user == currentUserId && relation.friend == friendId; 
                                                                                    return isMyFriendInBlackList;
                                                                                }).ToList<BlackListRelation>();
                                                                                int countResults = results.Count;
                                                                                bool isHaveResults = countResults >= 1;
                                                                                if (isHaveResults)
                                                                                {
                                                                                    innerFriendsItemContextMenuItem.Header = "Удалить из черного списка";
                                                                                    innerFriendsItemContextMenuItem.Click += RemoveFromBlackListHandler;
                                                                                }
                                                                                else
                                                                                {
                                                                                    innerFriendsItemContextMenuItem.Header = "Добавить в черный список";
                                                                                    innerFriendsItemContextMenuItem.Click += AddToBlackListHandler;
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
                                                                innerFriendsItemContextMenuItem.DataContext = friendId;
                                                                friendsItemContextMenuItem.Items.Add(innerFriendsItemContextMenuItem);

                                                                friendsItemContextMenu.Items.Add(friendsItemContextMenuItem);
                                                                friendsItem.ContextMenu = friendsItemContextMenu;
                                                            }

                                                            favoriteFriends.Children.Clear();
                                                            favoriteFriendsWrap.Visibility = Visibility.Collapsed;
                                                            Environment.SpecialFolder favoriteLocalApplicationDataFolder = Environment.SpecialFolder.LocalApplicationData;
                                                            string favoriteLocalApplicationDataFolderPath = Environment.GetFolderPath(favoriteLocalApplicationDataFolder);
                                                            string favoriteSaveDataFilePath = favoriteLocalApplicationDataFolderPath + @"\OfficeWare\GameManager\" + currentUserId + @"\save-data.txt";
                                                            js = new JavaScriptSerializer();
                                                            string favoriteSaveDataFileContent = File.ReadAllText(favoriteSaveDataFilePath);
                                                            SavedContent favoriteLoadedContent = js.Deserialize<SavedContent>(favoriteSaveDataFileContent);
                                                            List<FriendSettings> favoriteCurrentFriends = favoriteLoadedContent.friends;
                                                            List<FriendSettings> localFavoriteFriends = favoriteCurrentFriends.Where<FriendSettings>((FriendSettings settings) =>
                                                            {
                                                                return settings.isFavoriteFriend;
                                                            }).ToList<FriendSettings>();
                                                            List<string> localFavoriteFriendIds = new List<string>();
                                                            foreach (FriendSettings localFavoriteFriend in localFavoriteFriends)
                                                            {
                                                                string localFavoriteFriendId = localFavoriteFriend.id;
                                                                localFavoriteFriendIds.Add(localFavoriteFriendId);
                                                            }
                                                            bool isLocalFavoriteFriend = localFavoriteFriendIds.Contains(localFriendId);
                                                            if (isLocalFavoriteFriend)
                                                            {
                                                                favoriteFriendsWrap.Visibility = Visibility.Visible;
                                                                StackPanel favoriteFriendsItem = new StackPanel();

                                                                int favoriteFriendsItemHeight = 65;
                                                                bool isFavoriteCompactView = currentSettings.isFavoriteCompactView;
                                                                if (isFavoriteCompactView)
                                                                {
                                                                    favoriteFriendsItemHeight = 50;
                                                                }
                                                                else
                                                                {
                                                                    favoriteFriendsItemHeight = 65;
                                                                }
                                                                favoriteFriendsItem.Height = favoriteFriendsItemHeight;
                                                                
                                                                Image favoriteFriendAvatar = new Image();
                                                                Setter favoriteEffectSetter = new Setter();
                                                                favoriteEffectSetter.Property = ScrollViewer.EffectProperty;
                                                                favoriteEffectSetter.Value = new DropShadowEffect
                                                                {
                                                                    ShadowDepth = 4,
                                                                    Direction = 330,
                                                                    Color = Colors.Green,
                                                                    Opacity = 0.5,
                                                                    BlurRadius = 4
                                                                };
                                                                Style favoriteDropShadowScrollViewerStyle = new Style(typeof(ScrollViewer));
                                                                favoriteDropShadowScrollViewerStyle.Setters.Add(favoriteEffectSetter);
                                                                favoriteFriendAvatar.Resources.Add(typeof(ScrollViewer), favoriteDropShadowScrollViewerStyle);
                                                                favoriteFriendAvatar.Width = 25;
                                                                favoriteFriendAvatar.Height = 25;
                                                                favoriteFriendAvatar.Margin = new Thickness(5);
                                                                favoriteFriendAvatar.BeginInit();
                                                                Uri favoriteFriendAvatarUri = new Uri("https://cdn2.iconfinder.com/data/icons/ios-7-icons/50/user_male-128.png");
                                                                favoriteFriendAvatar.Source = new BitmapImage(favoriteFriendAvatarUri);
                                                                favoriteFriendAvatar.EndInit();
                                                                favoriteFriendsItem.Children.Add(favoriteFriendAvatar);
                                                                TextBlock favoriteFriendLoginLabel = new TextBlock();
                                                                favoriteFriendLoginLabel.Height = 25;
                                                                favoriteFriendLoginLabel.VerticalAlignment = VerticalAlignment.Center;
                                                                favoriteFriendLoginLabel.Margin = new Thickness(10, 5, 10, 5);
                                                                favoriteFriendLoginLabel.Text = friendLogin;
                                                                favoriteFriendsItem.Children.Add(favoriteFriendLoginLabel);
                                                                bool isFavoriteFriendOnline = friendStatus == "online";
                                                                bool isFavoriteFriendPlayed = friendStatus == "played";
                                                                bool isFavoriteFriendOffline = friendStatus == "offline";
                                                                if (isFavoriteFriendOnline)
                                                                {
                                                                    favoriteFriendLoginLabel.Foreground = onlineBrush;
                                                                }
                                                                else if (isFavoriteFriendPlayed)
                                                                {
                                                                    favoriteFriendLoginLabel.Foreground = playedBrush;
                                                                }
                                                                else if (isFavoriteFriendOffline)
                                                                {
                                                                    favoriteFriendLoginLabel.Foreground = offlineBrush;
                                                                }
                                                                favoriteFriends.Children.Add(favoriteFriendsItem);
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        string rawCountOfflineFriends = friendsOfflineCursor.ToString();
                                        string rawCountOnlineFriends = friendsOnlineCursor.ToString();
                                        string rawCountPlayedFriends = friendsPlayCursor.ToString();
                                        string playedFriendsCountLabelContent = "Играют (" + rawCountPlayedFriends + ")";
                                        // playedFriendsCountLabel.Text = playedFriendsCountLabelContent;
                                        string onlineFriendsCountLabelContent = "Друзья в сети (" + rawCountOnlineFriends + ")";
                                        onlineFriendsCountLabel.Text = onlineFriendsCountLabelContent;
                                        string offlineFriendsCountLabelContent = "Друзья не в сети (" + rawCountOfflineFriends + ")";
                                        offlineFriendsCountLabel.Text = offlineFriendsCountLabelContent;
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

        public void RemoveFriendHandler(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = ((MenuItem)(sender));
            object menuItemData = menuItem.DataContext;
            string friendId = ((string)(menuItemData));
            RemoveFriend(friendId);
        }

        public void RemoveFriend(string friendId)
        {
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
                            List<string> currentCategories = loadedContent.categories;
                            List<string> currentRecentChats = loadedContent.recentChats;
                            Recommendations currentRecommendations = loadedContent.recommendations;
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
                                    notifications = currentNotifications,
                                    categories = currentCategories,
                                    recentChats = currentRecentChats,
                                    recommendations = currentRecommendations
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

        private void FilterFriendsHandler(object sender, TextChangedEventArgs e)
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
            this.mainControl.SelectedIndex = 16;
            mainWindow.friendsSettingsControl.SelectedIndex = 1;
        }

        public void OpenChatHandler(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = ((MenuItem)(sender));
            object menuItemData = menuItem.DataContext;
            string friend = ((string)(menuItemData));
            OpenChat(friend);
        }

        async public void OpenChat(string friend)
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
            Notifications currentNotifications = loadedContent.notifications;
            List<string> currentCategories = loadedContent.categories;
            List<string> updatedRecentChats = loadedContent.recentChats;
            Recommendations currentRecommendations = loadedContent.recommendations;
            bool isOpenNewChatInNewWindow = currentSettings.isOpenNewChatInNewWindow;

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
                    // isChatExists = friend == localFriend;
                    isChatExists = chats.Contains(friend);
                }
                return isWindowDataExists && isChatWindow && isChatExists;
            }).ToList<Window>();
            int countChatWindows = chatWindows.Count;
            bool isNotOpenedChatWindows = countChatWindows <= 0;
            
            chats.Add(friend); 
            
            if (isNotOpenedChatWindows)
            {
                chatWindows = myWindows.Where<Window>(window =>
                {
                    string windowTitle = window.Title;
                    bool isChatWindow = windowTitle == "Чат";
                    return isChatWindow;
                }).ToList<Window>();
                countChatWindows = chatWindows.Count;
                isNotOpenedChatWindows = countChatWindows <= 0;
                if (isNotOpenedChatWindows)
                {
                    // Dialogs.ChatDialog dialog = new Dialogs.ChatDialog(currentUserId, client, friend, false, chats, mainWindow);
                    Dialogs.ChatDialog dialog = new Dialogs.ChatDialog(currentUserId, client, friend, false, mainWindow);
                    dialog.DataContext = friend;
                    dialog.Show();

                    // восстанавливаем окна чата из кеша
                    bool isResoreChats = currentSettings.isRestoreChats;
                    if (isResoreChats)
                    {
                        foreach (string updatedRecentChat in updatedRecentChats)
                        {
                            bool isChatExists = chats.Contains(updatedRecentChat);
                            bool isChatNotExists = !isChatExists;
                            if (isChatNotExists)
                            {
                                chats.Add(updatedRecentChat);
                                dialog.Focus();
                                // dialog.AddChat();
                                dialog.AddChat(friend);
                            }
                        }
                        updatedRecentChats.Clear();
                        string savedContent = js.Serialize(new SavedContent
                        {
                            games = currentGames,
                            friends = currentFriends,
                            settings = currentSettings,
                            collections = currentCollections,
                            notifications = currentNotifications,
                            categories = currentCategories,
                            recentChats = updatedRecentChats,
                            recommendations = currentRecommendations
                        });
                        File.WriteAllText(saveDataFilePath, savedContent);
                        dialog.SelectChat(friend);
                    }

                }
                else
                {
                    if (isOpenNewChatInNewWindow)
                    {
                        // Dialogs.ChatDialog dialog = new Dialogs.ChatDialog(currentUserId, client, friend, false, chats, mainWindow);
                        Dialogs.ChatDialog dialog = new Dialogs.ChatDialog(currentUserId, client, friend, false, mainWindow);
                        dialog.DataContext = friend;
                        dialog.Show();
                    }
                    else
                    {
                        Dialogs.ChatDialog chatWindow = ((ChatDialog)(chatWindows[0]));
                        chatWindow.Focus();
                        // chatWindow.AddChat();
                        chatWindow.AddChat(friend);
                    }
                }
            }
            else
            {
                if (isOpenNewChatInNewWindow)
                {
                    // Dialogs.ChatDialog dialog = new Dialogs.ChatDialog(currentUserId, client, friend, false, chats, mainWindow);
                    Dialogs.ChatDialog dialog = new Dialogs.ChatDialog(currentUserId, client, friend, false, mainWindow);
                    dialog.DataContext = friend;
                    dialog.Show();
                }
                else
                {
                    Dialogs.ChatDialog chatWindow = ((ChatDialog)(chatWindows[0]));
                    chatWindow.Focus();
                    chatWindow.SelectChat(friend);
                }
            }
        }

        public void InitSocketsHandler(object sender, RoutedEventArgs e)
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

        async public void InitSockets()
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
            Settings currentSettings = loadedContent.settings;
            List<string> currentCollections = loadedContent.collections;
            Notifications currentNotifications = loadedContent.notifications;
            List<string> currentCategories = loadedContent.categories;
            List<string> currentRecentChats = loadedContent.recentChats;
            Recommendations currentRecommendations = loadedContent.recommendations;
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
                    friends = updatedFriends,
                    settings = currentSettings,
                    collections = currentCollections,
                    notifications = currentNotifications,
                    categories = currentCategories,
                    recentChats = currentRecentChats,
                    recommendations = currentRecommendations
                });
                File.WriteAllText(saveDataFilePath, savedContent);
                GetFriends(currentUserId, "");
            }
        }

        public void RemoveTalkFromFavoriteHandler  (object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = ((MenuItem)(sender));
            object menuItemData = menuItem.DataContext;
            string talkId = ((string)(menuItemData));
            RemoveTalkFromFavorite(talkId);
        }

        public void RemoveTalkFromFavorite (string currentTalkId)
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
            Notifications currentNotifications = loadedContent.notifications;
            List<string> currentCategories = loadedContent.categories;
            List<string> currentRecentChats = loadedContent.recentChats;
            Recommendations currentRecommendations = loadedContent.recommendations;
            List<FriendSettings> updatedFriends = currentFriends;
            List<FriendSettings> cachedFriends = updatedFriends.Where<FriendSettings>((FriendSettings friend) =>
            {
                return friend.id == currentTalkId;
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
                    friends = updatedFriends,
                    settings = currentSettings,
                    collections = currentCollections,
                    notifications = currentNotifications,
                    categories = currentCategories,
                    recentChats = currentRecentChats,
                    recommendations = currentRecommendations
                });
                File.WriteAllText(saveDataFilePath, savedContent);
                GetFriends(currentUserId, "");
                GetTalks();
            }
        }

        public void RemoveFriendFromFavoriteHandler (object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = ((MenuItem)(sender));
            object menuItemData = menuItem.DataContext;
            string friend = ((string)(menuItemData));
            RemoveFriendFromFavorite(friend);
        }

        public void RemoveFriendFromFavorite(string currentFriendId)
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
            Notifications currentNotifications = loadedContent.notifications;
            List<string> currentCategories = loadedContent.categories;
            List<string> currentRecentChats = loadedContent.recentChats;
            Recommendations currentRecommendations = loadedContent.recommendations;
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
                    friends = updatedFriends,
                    settings = currentSettings,
                    collections = currentCollections,
                    notifications = currentNotifications,
                    categories = currentCategories,
                    recentChats = currentRecentChats,
                    recommendations = currentRecommendations
                });
                File.WriteAllText(saveDataFilePath, savedContent);
                GetFriends(currentUserId, "");
            }
        }

        public void OpenFriendProfileHandler(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = ((MenuItem)(sender));
            object menuItemData = menuItem.DataContext;
            string friend = ((string)(menuItemData));
            OpenFriendProfile(friend);
        }

        async public void OpenFriendProfile(string friendId)
        {
            mainControl.DataContext = friendId;
            mainControl.SelectedIndex = 1;
            this.Close();
        }

        public void JoinToGameHandler(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = ((MenuItem)(sender));
            object menuItemData = menuItem.DataContext;
            string friend = ((string)(menuItemData));
            JoinToGame(friend);
        }

        public void JoinToGame(string friendId)
        {
            this.DataContext = friendId;
            this.Close();
        }

        public void AddToBlackListHandler (object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = ((MenuItem)(sender));
            object menuItemData = menuItem.DataContext;
            string friendId = ((string)(menuItemData));
            AddToBlackList(friendId);
        }

        public void AddToBlackList (string friendId)
        {
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/blacklist/relations/add/?id=" + currentUserId + @"&friend=" + friendId);
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
                            GetFriends(currentUserId, "");
                            MessageBox.Show("Друг был добавлен в черный список", "Внимание");
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

        public void RemoveFromBlackListHandler (object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = ((MenuItem)(sender));
            object menuItemData = menuItem.DataContext;
            string friendId = ((string)(menuItemData));
            RemoveFromBlackList(friendId);
        }

        public void RemoveFromBlackList (string friendId)
        {
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/blacklist/relations/remove/?id=" + currentUserId + @"&friend=" + friendId);
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
                            GetFriends(currentUserId, "");
                            MessageBox.Show("Друг был удален из черного списка", "Внимание");
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

        private void OpenCreateTalkDialogHandler (object sender, MouseButtonEventArgs e)
        {
            OpenCreateTalkDialog();
        }

        public void OpenCreateTalkDialog ()
        {
            Dialogs.CreateTalkDialog dialog = new Dialogs.CreateTalkDialog(currentUserId);
            dialog.Closed += GetTalksHandler;
            dialog.Show();
        }

        public void GetTalks ()
        {
            talks.Children.Clear();
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/talks/all");
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (var reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        var objText = reader.ReadToEnd();
                        TalksResponseInfo myobj = (TalksResponseInfo)js.Deserialize(objText, typeof(TalksResponseInfo));
                        string status = myobj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
                            List<Talk> totalTalks = myobj.talks;
                            HttpWebRequest innerWebRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/talks/relations/all");
                            innerWebRequest.Method = "GET";
                            innerWebRequest.UserAgent = ".NET Framework Test Client";
                            using (HttpWebResponse innerWebResponse = (HttpWebResponse)innerWebRequest.GetResponse())
                            {
                                using (var innerReader = new StreamReader(innerWebResponse.GetResponseStream()))
                                {
                                    js = new JavaScriptSerializer();
                                    objText = innerReader.ReadToEnd();
                                    TalkRelationsResponseInfo myInnerObj = (TalkRelationsResponseInfo)js.Deserialize(objText, typeof(TalkRelationsResponseInfo));
                                    status = myInnerObj.status;
                                    isOkStatus = status == "OK";
                                    if (isOkStatus)
                                    {
                                        List<TalkRelation> relations = myInnerObj.relations;
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
                                            talksWrap.Visibility = Visibility.Visible;
                                            foreach (Talk talk in totalTalks)
                                            {
                                                string talkId = talk._id;
                                                string talkTitle = talk.title;
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
                                                    StackPanel totalTalksItem = new StackPanel();
                                                    

                                                    totalTalksItem.Height = 35;
                                                    
                                                    
                                                    totalTalksItem.Orientation = Orientation.Horizontal;
                                                    PackIcon totalTalksItemIcon = new PackIcon();
                                                    totalTalksItemIcon.VerticalAlignment = VerticalAlignment.Center;
                                                    totalTalksItemIcon.Width = 24;
                                                    totalTalksItemIcon.Height = 24;
                                                    totalTalksItemIcon.Margin = new Thickness(15, 0, 15, 0);
                                                    totalTalksItemIcon.Kind = PackIconKind.Circle;
                                                    totalTalksItem.Children.Add(totalTalksItemIcon);
                                                    TextBlock totalTalksItemLabel = new TextBlock();
                                                    totalTalksItemLabel.VerticalAlignment = VerticalAlignment.Center;
                                                    totalTalksItemLabel.Margin = new Thickness(15, 0, 15, 0);
                                                    totalTalksItemLabel.Text = talkTitle;
                                                    totalTalksItem.Children.Add(totalTalksItemLabel);
                                                    
                                                    totalTalksItem.DataContext = talkId;
                                                    /*Dictionary<String, Object> dialogData = new Dictionary<String, Object>();
                                                    dialogData.Add("talk", talkId);
                                                    dialogData.Add("channel", "mockChannelId");
                                                    totalTalksItem.DataContext = dialogData;*/
                                                    
                                                    totalTalksItem.MouseLeftButtonUp += OpenTalkHandler;
                                                    talks.Children.Add(totalTalksItem);
                                                    ContextMenu totalTalksItemContextMenu = new ContextMenu();
                                                    MenuItem totalTalksItemContextMenuItem = new MenuItem();
                                                    totalTalksItemContextMenuItem.Header = "Открыть окно чата";

                                                    totalTalksItemContextMenuItem.DataContext = talkId;
                                                    /*Dictionary<String, Object> dialogData = new Dictionary<String, Object>();
                                                    dialogData.Add("talk", talkId);
                                                    dialogData.Add("channel", "mockChannelId");
                                                    totalTalksItem.DataContext = dialogData;*/

                                                    totalTalksItemContextMenuItem.Click += OpenTalkFromMenuHandler;
                                                    totalTalksItemContextMenu.Items.Add(totalTalksItemContextMenuItem);
                                                    totalTalksItemContextMenuItem = new MenuItem();
                                                    totalTalksItemContextMenuItem.Header = "Выйти из группового чата";
                                                    totalTalksItemContextMenuItem.DataContext = talkId;
                                                    totalTalksItemContextMenuItem.Click += LogoutFromTalkHandler;
                                                    totalTalksItemContextMenu.Items.Add(totalTalksItemContextMenuItem);
                                                    
                                                    totalTalksItemContextMenuItem = new MenuItem();

                                                    bool isFavoriteTalk = false;
                                                    Environment.SpecialFolder localApplicationDataFolder = Environment.SpecialFolder.LocalApplicationData;
                                                    string localApplicationDataFolderPath = Environment.GetFolderPath(localApplicationDataFolder);
                                                    string saveDataFilePath = localApplicationDataFolderPath + @"\OfficeWare\GameManager\" + currentUserId + @"\save-data.txt";
                                                    js = new JavaScriptSerializer();
                                                    string saveDataFileContent = File.ReadAllText(saveDataFilePath);
                                                    SavedContent loadedContent = js.Deserialize<SavedContent>(saveDataFileContent);
                                                    List<Game> currentGames = loadedContent.games;
                                                    List<FriendSettings> currentFriends = loadedContent.friends;
                                                    List<FriendSettings> updatedFriends = currentFriends;
                                                    List<FriendSettings> cachedFriends = updatedFriends.Where<FriendSettings>((FriendSettings localFriend) =>
                                                    {
                                                        return localFriend.id == talkId;
                                                    }).ToList();
                                                    int countCachedFriends = cachedFriends.Count;
                                                    bool isCachedFriendsExists = countCachedFriends >= 1;
                                                    if (isCachedFriendsExists)
                                                    {
                                                        FriendSettings cachedFriend = cachedFriends[0];
                                                        isFavoriteTalk = cachedFriend.isFavoriteFriend;
                                                    }
                                                    bool isTalkInFavorites = isFavoriteTalk;
                                                    if (isTalkInFavorites)
                                                    {
                                                        totalTalksItemContextMenuItem.Header = "Удалить из избранных";
                                                        totalTalksItemContextMenuItem.Click += RemoveTalkFromFavoriteHandler;
                                                    }
                                                    else
                                                    {
                                                        totalTalksItemContextMenuItem.Header = "Добавить в избранные";
                                                        totalTalksItemContextMenuItem.Click += AddTalkToFavoriteHandler;
                                                    }

                                                    // totalTalksItemContextMenuItem.Header = "Добавить в избранное";
                                                    totalTalksItemContextMenuItem.DataContext = talkId;
                                                    totalTalksItemContextMenuItem.Click += AddTalkToFavoriteHandler;
                                                    totalTalksItemContextMenu.Items.Add(totalTalksItemContextMenuItem);
                                                    totalTalksItem.ContextMenu = totalTalksItemContextMenu;

                                                    // List<Game> currentGames = loadedContent.games;
                                                    Settings currentSettings = loadedContent.settings;
                                                    // List<FriendSettings> currentFriends = loadedContent.friends;
                                                    List<string> currentCollections = loadedContent.collections;
                                                    Notifications currentNotifications = loadedContent.notifications;
                                                    List<string> currentCategories = loadedContent.categories;
                                                    List<string> currentRecentChats = loadedContent.recentChats;
                                                    Recommendations currentRecommendations = loadedContent.recommendations;
                                                    // List<FriendSettings> updatedFriends = currentFriends;
                                                    int countTalkResults = updatedFriends.Count<FriendSettings>((FriendSettings someFriendSettingsItem) =>
                                                    {
                                                        string someFriendSettingsItemId = someFriendSettingsItem.id;
                                                        return someFriendSettingsItemId == talkId;
                                                    });
                                                    bool isHaveNotTalkResults = countTalkResults <= 0;
                                                    if (isHaveNotTalkResults)
                                                    { 
                                                        updatedFriends.Add(new FriendSettings()
                                                        {
                                                            id = talkId,
                                                            isFriendOnlineNotification = true,
                                                            isFriendOnlineSound = true,
                                                            isFriendPlayedNotification = true,
                                                            isFriendPlayedSound = true,
                                                            isFriendSendMsgNotification = true,
                                                            isFriendSendMsgSound = true,
                                                            isFavoriteFriend = false,
                                                            categories = new List<string>()
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
                                                            recommendations = currentRecommendations
                                                        });
                                                        File.WriteAllText(saveDataFilePath, savedContent);
                                                    }

                                                    favoriteFriendsWrap.Visibility = Visibility.Collapsed;
                                                    Environment.SpecialFolder favoriteLocalApplicationDataFolder = Environment.SpecialFolder.LocalApplicationData;
                                                    string favoriteLocalApplicationDataFolderPath = Environment.GetFolderPath(favoriteLocalApplicationDataFolder);
                                                    string favoriteSaveDataFilePath = favoriteLocalApplicationDataFolderPath + @"\OfficeWare\GameManager\" + currentUserId + @"\save-data.txt";
                                                    js = new JavaScriptSerializer();
                                                    string favoriteSaveDataFileContent = File.ReadAllText(favoriteSaveDataFilePath);
                                                    SavedContent favoriteLoadedContent = js.Deserialize<SavedContent>(favoriteSaveDataFileContent);
                                                    List<FriendSettings> favoriteCurrentFriends = favoriteLoadedContent.friends;
                                                    List<FriendSettings> localFavoriteFriends = favoriteCurrentFriends.Where<FriendSettings>((FriendSettings settings) =>
                                                    {
                                                        return settings.isFavoriteFriend;
                                                    }).ToList<FriendSettings>();
                                                    List<string> localFavoriteFriendIds = new List<string>();
                                                    foreach (FriendSettings localFavoriteFriend in localFavoriteFriends)
                                                    {
                                                        string localFavoriteFriendId = localFavoriteFriend.id;
                                                        localFavoriteFriendIds.Add(localFavoriteFriendId);
                                                    }
                                                    bool isLocalFavoriteFriend = localFavoriteFriendIds.Contains(talkId);
                                                    if (isLocalFavoriteFriend)
                                                    {
                                                        favoriteFriendsWrap.Visibility = Visibility.Visible;
                                                        StackPanel favoriteFriendsItem = new StackPanel();

                                                        int favoriteFriendsItemHeight = 65;
                                                        bool isFavoriteCompactView = currentSettings.isFavoriteCompactView;
                                                        if (isFavoriteCompactView)
                                                        {
                                                            favoriteFriendsItemHeight = 50;
                                                        }
                                                        else
                                                        {
                                                            favoriteFriendsItemHeight = 65;
                                                        }
                                                        favoriteFriendsItem.Height = favoriteFriendsItemHeight;

                                                        Image favoriteFriendAvatar = new Image();
                                                        Setter favoriteEffectSetter = new Setter();
                                                        favoriteEffectSetter.Property = ScrollViewer.EffectProperty;
                                                        favoriteEffectSetter.Value = new DropShadowEffect
                                                        {
                                                            ShadowDepth = 4,
                                                            Direction = 330,
                                                            Color = Colors.Green,
                                                            Opacity = 0.5,
                                                            BlurRadius = 4
                                                        };
                                                        Style favoriteDropShadowScrollViewerStyle = new Style(typeof(ScrollViewer));
                                                        favoriteDropShadowScrollViewerStyle.Setters.Add(favoriteEffectSetter);
                                                        favoriteFriendAvatar.Resources.Add(typeof(ScrollViewer), favoriteDropShadowScrollViewerStyle);
                                                        favoriteFriendAvatar.Width = 25;
                                                        favoriteFriendAvatar.Height = 25;
                                                        favoriteFriendAvatar.Margin = new Thickness(5);
                                                        favoriteFriendAvatar.BeginInit();
                                                        Uri favoriteFriendAvatarUri = new Uri("https://cdn2.iconfinder.com/data/icons/ios-7-icons/50/user_male-128.png");
                                                        favoriteFriendAvatar.Source = new BitmapImage(favoriteFriendAvatarUri);
                                                        favoriteFriendAvatar.EndInit();
                                                        favoriteFriendsItem.Children.Add(favoriteFriendAvatar);
                                                        TextBlock favoriteFriendLoginLabel = new TextBlock();
                                                        favoriteFriendLoginLabel.Height = 25;
                                                        favoriteFriendLoginLabel.VerticalAlignment = VerticalAlignment.Center;
                                                        favoriteFriendLoginLabel.Margin = new Thickness(10, 5, 10, 5);
                                                        favoriteFriendLoginLabel.Text = talkTitle;
                                                        favoriteFriendsItem.Children.Add(favoriteFriendLoginLabel);
                                                        favoriteFriends.Children.Add(favoriteFriendsItem);
                                                    }

                                                }
                                            }
                                        }
                                        else
                                        {
                                            talksWrap.Visibility = Visibility.Collapsed;
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

        public void AddTalkToFavoriteHandler (object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = ((MenuItem)(sender));
            object menuItemData = menuItem.DataContext;
            string talkId = ((string)(menuItemData));
            AddTalkToFavorite(talkId);
        }

        public void AddTalkToFavorite (string talkId)
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
            Settings currentSettings = loadedContent.settings;
            List<string> currentCollections = loadedContent.collections;
            Notifications currentNotifications = loadedContent.notifications;
            List<string> currentCategories = loadedContent.categories;
            List<string> currentRecentChats = loadedContent.recentChats;
            Recommendations currentRecommendations = loadedContent.recommendations;
            List<FriendSettings> cachedFriends = updatedFriends.Where<FriendSettings>((FriendSettings friend) =>
            {
                return friend.id == talkId;
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
                    friends = updatedFriends,
                    settings = currentSettings,
                    collections = currentCollections,
                    notifications = currentNotifications,
                    categories = currentCategories,
                    recentChats = currentRecentChats,
                    recommendations = currentRecommendations
                });
                File.WriteAllText(saveDataFilePath, savedContent);
                GetFriends(currentUserId, "");
                GetTalks();
            }
        }

        public void LogoutFromTalkHandler (object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = ((MenuItem)(sender));
            object menuItemData = menuItem.DataContext;
            string talkId = ((string)(menuItemData));
            LogoutFromTalk(talkId);
        }

        public void LogoutFromTalk (string talkId)
        {
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/talks/relations/delete/?id=" + talkId + @"&user=" + currentUserId);
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
                            GetTalks();
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

        public void OpenTalkFromMenuHandler (object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = ((MenuItem)(sender));
            object talkData = menuItem.DataContext;
            string talkId = ((string)(talkData));
            OpenTalk(talkId);
        }

        public void OpenTalkHandler (object sender, RoutedEventArgs e)
        {
            StackPanel talk = ((StackPanel)(sender));
            object talkData = talk.DataContext;
            string talkId = ((string)(talkData));
            OpenTalk(talkId);
        }

        public void OpenTalk (string talkId)
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
            Notifications currentNotifications = loadedContent.notifications;
            List<string> currentCategories = loadedContent.categories;
            List<string> updatedRecentChats = loadedContent.recentChats;
            bool isOpenNewChatInNewWindow = currentSettings.isOpenNewChatInNewWindow;


            Application app = Application.Current;
            WindowCollection windows = app.Windows;
            IEnumerable<Window> myWindows = windows.OfType<Window>();
            List<Window> talkWindows = myWindows.Where<Window>(window =>
            {
                string windowTitle = window.Title;
                bool isTalkWindow = windowTitle == "Беседа";
                object windowData = window.DataContext;
                bool isWindowDataExists = windowData != null;
                bool isTalkExists = true;
                if (isWindowDataExists && isTalkWindow)
                {
                    string localFriend = ((string)(windowData));
                    isTalkExists = chats.Contains(talkId);
                }
                return isWindowDataExists && isTalkWindow && isTalkExists;
            }).ToList<Window>();

            int countTalkWindows = talkWindows.Count;
            bool isNotOpenedTalkWindows = countTalkWindows <= 0;

            chats.Add(talkId);

            /*if (isNotOpenedTalkWindows)
            {
                talkWindows = myWindows.Where<Window>(window =>
                {
                    string windowTitle = window.Title;
                    bool isTalkWindow = windowTitle == "Беседа";
                    return isTalkWindow;
                }).ToList<Window>();
                countTalkWindows = talkWindows.Count;
                isNotOpenedTalkWindows = countTalkWindows <= 0;
                if (isNotOpenedTalkWindows)
                {
                    Dialogs.TalkDialog dialog = new Dialogs.TalkDialog(currentUserId, talkId, client, false);
                    dialog.DataContext = talkId;
                    dialog.Show();

                    // восстанавливаем окна чата из кеша

                }
            }
            else
            {
                if (isOpenNewChatInNewWindow)
                {
                    Dialogs.TalkDialog dialog = new Dialogs.TalkDialog(currentUserId, talkId, client, false);
                    dialog.DataContext = talkId;
                    dialog.Show();
                }
                else
                {
                    Dialogs.TalkDialog talkWindow = ((TalkDialog)(talkWindows[0]));
                    talkWindow.Focus();
                    talkWindow.AddChat(talkId);
                }
            }*/

            if (isNotOpenedTalkWindows)
            {
                talkWindows = myWindows.Where<Window>(window =>
                {
                    string windowTitle = window.Title;
                    bool isTalkWindow = windowTitle == "Беседа";
                    return isTalkWindow;
                }).ToList<Window>();
                countTalkWindows = talkWindows.Count;
                isNotOpenedTalkWindows = countTalkWindows <= 0;
                if (isNotOpenedTalkWindows)
                {
                    Dialogs.TalkDialog dialog = new Dialogs.TalkDialog(currentUserId, talkId, client, false);

                    // dialog.DataContext = talkId;
                    Dictionary<String, Object> dialogData = new Dictionary<String, Object>();
                    dialogData.Add("talk", talkId);
                    dialogData.Add("channel", "mockChannelId");
                    dialog.DataContext = dialogData;

                    dialog.Show();

                    // восстанавливаем окна чата из кеша
                    bool isResoreChats = currentSettings.isRestoreChats;
                    if (isResoreChats)
                    {
                        /*foreach (string updatedRecentChat in updatedRecentChats)
                        {
                            bool isChatExists = chats.Contains(updatedRecentChat);
                            bool isChatNotExists = !isChatExists;
                            if (isChatNotExists)
                            {
                                chats.Add(updatedRecentChat);
                                dialog.Focus();
                                // dialog.AddChat();
                                dialog.AddChat(friend);
                            }
                        }
                        updatedRecentChats.Clear();
                        string savedContent = js.Serialize(new SavedContent
                        {
                            games = currentGames,
                            friends = currentFriends,
                            settings = currentSettings,
                            collections = currentCollections,
                            notifications = currentNotifications,
                            categories = currentCategories,
                            recentChats = updatedRecentChats
                        });
                        File.WriteAllText(saveDataFilePath, savedContent);*/
                        dialog.SelectChat(talkId);
                    }

                }
                else
                {
                    if (isOpenNewChatInNewWindow)
                    {
                        Dialogs.TalkDialog dialog = new Dialogs.TalkDialog(currentUserId, talkId, client, false);

                        // dialog.DataContext = talkId;
                        Dictionary<String, Object> dialogData = new Dictionary<String, Object>();
                        dialogData.Add("talk", talkId);
                        dialogData.Add("channel", "mockChannelId");
                        dialog.DataContext = dialogData;

                        dialog.Show();
                    }
                    else
                    {
                        Dialogs.TalkDialog talkWindow = ((TalkDialog)(talkWindows[0]));
                        talkWindow.Focus();
                        talkWindow.AddChat(talkId);
                    }
                }
            }
            else
            {
                if (isOpenNewChatInNewWindow)
                {
                    Dialogs.TalkDialog dialog = new Dialogs.TalkDialog(currentUserId, talkId, client, false);

                    // dialog.DataContext = talkId;
                    Dictionary<String, Object> dialogData = new Dictionary<String, Object>();
                    dialogData.Add("talk", talkId);
                    dialogData.Add("channel", "mockChannelId");
                    dialog.DataContext = dialogData;

                    dialog.Show();
                }
                else
                {
                    Dialogs.TalkDialog talkWindow = ((TalkDialog)(talkWindows[0]));
                    talkWindow.Focus();
                    talkWindow.SelectChat(talkId);
                }
            }


            /*Dialogs.TalkDialog dialog = new Dialogs.TalkDialog(currentUserId, talkId, client, false);
            dialog.Closed += GetTalksHandler;
            dialog.Show();*/
        }

        public void GetTalksHandler (object sender, EventArgs e)
        {
            GetFriends(currentUserId, "");
            GetTalks();
        }

        private void SetDefaultAvatarHandler(object sender, ExceptionRoutedEventArgs e)
        {
            Image avatar = ((Image)(sender));
            SetDefaultAvatar (avatar);
        }

        public void SetDefaultAvatar (Image avatar)
        {
            avatar.BeginInit();
            avatar.Source = new BitmapImage(new Uri(@"https://cdn2.iconfinder.com/data/icons/ios-7-icons/50/user_male-128.png"));
            avatar.EndInit();
        }

        private void OpenFriendSettingsHandler (object sender, MouseButtonEventArgs e)
        {
            OpenFriendSettings();
        }

        public void OpenFriendSettings ()
        {
            Dialogs.FriendsSettingsDialog dialog = new Dialogs.FriendsSettingsDialog(currentUserId);
            dialog.Closed += RefreshFriendsHandler;
            dialog.Show();
        }

        private void OpenProfilePopupHandler(object sender, MouseButtonEventArgs e)
        {
            OpenProfilePopup();
        }

        public void OpenProfilePopup ()
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
                            string userStatus = user.status;
                            userProfilePopup.IsOpen = true;
                            bool isOnline = userStatus == "online";
                            bool isOffline = userStatus == "offline";
                            bool isInvisible = userStatus == "";
                            if (isOnline)
                            {
                                onlineStatusLabel.Foreground = System.Windows.Media.Brushes.SkyBlue;
                                offlineStatusLabel.Foreground = System.Windows.Media.Brushes.Black;
                                invisibleStatusLabel.Foreground = System.Windows.Media.Brushes.Black;
                            }
                            else if (isOffline)
                            {
                                onlineStatusLabel.Foreground = System.Windows.Media.Brushes.Black;
                                offlineStatusLabel.Foreground = System.Windows.Media.Brushes.SkyBlue;
                                invisibleStatusLabel.Foreground = System.Windows.Media.Brushes.Black;
                            }
                            else if (isInvisible)
                            {
                                onlineStatusLabel.Foreground = System.Windows.Media.Brushes.Black;
                                offlineStatusLabel.Foreground = System.Windows.Media.Brushes.Black;
                                invisibleStatusLabel.Foreground = System.Windows.Media.Brushes.SkyBlue;
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

        private void SetUserStatusHandler (object sender, RoutedEventArgs e)
        {
            TextBlock label = ((TextBlock)(sender));
            object rawStatus = label.DataContext;
            string status = rawStatus.ToString();
            SetUserStatus(status);
        }

        public void SetUserStatus (string userStatus)
        {
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/user/status/set/?id=" + currentUserId + @"&status=" + userStatus);
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
                            // OpenProfilePopup();
                            userProfilePopup.IsOpen = false;
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

        public void OpenCategoryDialogHandler (object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = ((MenuItem)(sender));
            object menuItemData = menuItem.DataContext;
            string friendId = ((string)(menuItemData));
            OpenCategoryDialog(friendId);
        }

        public void OpenCategoryDialog (string friendId)
        {
            Environment.SpecialFolder localApplicationDataFolder = Environment.SpecialFolder.LocalApplicationData;
            string localApplicationDataFolderPath = Environment.GetFolderPath(localApplicationDataFolder);
            string saveDataFilePath = localApplicationDataFolderPath + @"\OfficeWare\GameManager\" + currentUserId + @"\save-data.txt";
            JavaScriptSerializer js = new JavaScriptSerializer();
            string saveDataFileContent = File.ReadAllText(saveDataFilePath);
            SavedContent loadedContent = js.Deserialize<SavedContent>(saveDataFileContent);
            List<FriendSettings> currentFriendSettings = loadedContent.friends;
            List<FriendSettings> results = currentFriendSettings.Where<FriendSettings>((FriendSettings friend) =>
            {
                string localFriendId = friend.id;
                List<string> friendCategories = friend.categories;
                bool isAddFriend = localFriendId == friendId;
                return isAddFriend;
            }).ToList<FriendSettings>();
            int countResults = results.Count;
            bool isHaveResults = countResults >= 1;
            if (isHaveResults)
            {
                FriendSettings result = results[0];
                List<string> categories = result.categories;
                int countCategories = categories.Count;
                bool isHaveCategories = countCategories >= 1;
                if (isHaveCategories)
                {
                    Dialogs.CategoryManagementDialog dialog = new Dialogs.CategoryManagementDialog(currentUserId, friendId);
                    dialog.Closed += GetCategoriesHandler;
                    dialog.Show();
                }
                else
                {
                    List<string> friendIds = new List<string>() {
                        friendId
                    };
                    Dialogs.CreateCategoryDialog dialog = new Dialogs.CreateCategoryDialog(currentUserId, friendIds);
                    dialog.Closed += GetCategoriesHandler;
                    dialog.Show();
                }
            }
        }

        public void GetCategoriesHandler (object sender, EventArgs e)
        {
            GetCategories();
        }

        private void OpenUpdateProfileNameHandler (object sender, MouseButtonEventArgs e)
        {
            OpenUpdateProfileName();
        }

        public void OpenUpdateProfileName ()
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
                            string userName = user.name;
                            updateProfilePopupNameBox.Text = userName;
                            updateProfilePopup.IsOpen = true;
                            // userProfilePopup.IsOpen = false;
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

        private void CancelUpdateProfilePopupHandler (object sender, RoutedEventArgs e)
        {
            CancelUpdateProfilePopup();
        }

        public void CancelUpdateProfilePopup ()
        {
            updateProfilePopup.IsOpen = false;
            userProfilePopup.IsOpen = false;
        }

        private void UpdateProfileNameHandler(object sender, RoutedEventArgs e)
        {
            UpdateProfileName();
        }

        public void UpdateProfileName ()
        {
            string updateProfilePopupNameBoxContent = updateProfilePopupNameBox.Text;
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/user/name/set/?id=" + currentUserId + @"&name=" + updateProfilePopupNameBoxContent);
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
                            CancelUpdateProfilePopup();
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

        public void AddFriendNickHandler (object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = ((MenuItem)(sender));
            object menuItemData = menuItem.DataContext;
            string id = ((string)(menuItemData)); 
            AddFriendNick(id);
        }

        public void AddFriendNick (string id)
        {
            Dialogs.AddNickDialog dialog = new AddNickDialog(id);
            dialog.Closed += RefreshFriendsHandler;
            dialog.Show();
        }

        public void UpdateFriendNickHandler(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = ((MenuItem)(sender));
            object menuItemData = menuItem.DataContext;
            string id = ((string)(menuItemData));
            UpdateFriendNick(id);
        }

        public void UpdateFriendNick (string id)
        {
            Dialogs.UpdateNickDialog dialog = new UpdateNickDialog(id);
            dialog.Closed += RefreshFriendsHandler;
            dialog.Show();
        }

        public void RefreshFriendsHandler (object sender, EventArgs e)
        {
            GetFriends(currentUserId, "");
            GetTalks();
        }

        public void OpenUserProfileHandler(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = ((MenuItem)(sender));
            object menuItemData = menuItem.DataContext;
            string id = ((string)(menuItemData));
            OpenUserProfile(id);
        }

        public void OpenUserProfile(string id)
        {
            mainWindow.mainControl.DataContext = id;
            mainWindow.ReturnToProfile();
        }

    }

}