﻿using System;
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

        public MainWindow(string id)
        {
            InitializeComponent();

            Initialize(id);

        }

        public void Initialize(string id)
        {
            
            GetUser(id);
            InitConstants();
            ShowOffers();
            GetGamesList("");
            GetFriendRequests();
            GetGamesInfo();
            GetUserInfo();
            GetEditInfo();
        }

        public void GetEditInfo ()
        {
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("https://digitaldistributtionservice.herokuapp.com/api/friends/get");
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

        public void GetGamesInfo ()
        {

            Environment.SpecialFolder localApplicationDataFolder = Environment.SpecialFolder.LocalApplicationData;
            string localApplicationDataFolderPath = Environment.GetFolderPath(localApplicationDataFolder);
            string saveDataFilePath = localApplicationDataFolderPath + @"\OfficeWare\GameManager\" + currentUserId + @"\save-data.txt";
            string saveDataFileContent = File.ReadAllText(saveDataFilePath);
            JavaScriptSerializer js = new JavaScriptSerializer();
            SavedContent loadedContent = js.Deserialize<SavedContent>(saveDataFileContent);
            List<Game> myGames = loadedContent.games;
            foreach (Game myGame in myGames)
            {
                string myGameName = myGame.name;
                string myGameHours = myGame.hours;
                string myGameLastLaunchDate = myGame.date;
                DockPanel gameStats = new DockPanel();
                gameStats.Height = 150;
                gameStats.Background = System.Windows.Media.Brushes.DarkGray;
                Image gameStatsImg = new Image();
                gameStatsImg.Width = 125;
                gameStatsImg.Height = 125;
                gameStatsImg.Margin = new Thickness(10);
                gameStatsImg.Source = new BitmapImage(new Uri("https://cdn2.iconfinder.com/data/icons/ios-7-icons/50/user_male-128.png"));
                gameStats.Children.Add(gameStatsImg);
                TextBlock gameStatsNameLabel = new TextBlock();
                gameStatsNameLabel.Margin = new Thickness(10);
                gameStatsNameLabel.FontSize = 18;
                gameStatsNameLabel.Text = myGameName;
                gameStats.Children.Add(gameStatsNameLabel);
                StackPanel gameStatsInfo = new StackPanel();
                gameStatsInfo.Margin = new Thickness(10);
                gameStatsInfo.VerticalAlignment = VerticalAlignment.Bottom;
                gameStatsInfo.HorizontalAlignment = HorizontalAlignment.Right;
                TextBlock gameStatsInfoHoursLabel = new TextBlock();
                gameStatsInfoHoursLabel.Margin = new Thickness(0, 5, 0, 5);
                string gameStatsInfoHoursLabelContent = myGameHours + " часов всего";
                gameStatsInfoHoursLabel.Text = gameStatsInfoHoursLabelContent;
                gameStatsInfo.Children.Add(gameStatsInfoHoursLabel);
                TextBlock gameStatsInfoLastLaunchLabel = new TextBlock();
                gameStatsInfoLastLaunchLabel.Margin = new Thickness(0, 5, 0, 5);
                string gameStatsInfoLastLaunchLabelContent = "Последний запуск " + myGameLastLaunchDate;
                gameStatsInfoLastLaunchLabel.Text = gameStatsInfoLastLaunchLabelContent;
                gameStatsInfo.Children.Add(gameStatsInfoLastLaunchLabel);
                gameStats.Children.Add(gameStatsInfo);
                gamesInfo.Children.Add(gameStats);
            }
        }

        public void GetUserInfo ()
        {
            Environment.SpecialFolder localApplicationDataFolder = Environment.SpecialFolder.LocalApplicationData;
            string localApplicationDataFolderPath = Environment.GetFolderPath(localApplicationDataFolder);
            string saveDataFilePath = localApplicationDataFolderPath + @"\OfficeWare\GameManager\" + currentUserId + @"\save-data.txt";
            string saveDataFileContent = File.ReadAllText(saveDataFilePath);
            JavaScriptSerializer js = new JavaScriptSerializer();
            SavedContent loadedContent = js.Deserialize<SavedContent>(saveDataFileContent);
            List<Game> myGames = loadedContent.games;
            int countGames = myGames.Count;
            string rawCountGames = countGames.ToString();
            countGamesLabel.Text = rawCountGames;

            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("https://digitaldistributtionservice.herokuapp.com/api/friends/get");
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
                            userProfileNameLabel.Text = currentUserName;
                            string currentUserCountry = currentUser.country;
                            userProfileCountryLabel.Text = currentUserCountry;
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

        public void GetFriendRequests ()
        {
            try {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("https://digitaldistributtionservice.herokuapp.com/api/friends/requests/get/?id=" + currentUserId);
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
                                HttpWebRequest innerWebRequest = (HttpWebRequest)HttpWebRequest.Create("https://digitaldistributtionservice.herokuapp.com/api/users/get/?id=" + senderId);
                                webRequest.Method = "GET";
                                webRequest.UserAgent = ".NET Framework Test Client";
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
                                            Uri friendRequestBodySenderAvatarUri = new Uri("https://cdn2.iconfinder.com/data/icons/ios-7-icons/50/user_male-128.png");
                                            BitmapImage friendRequestBodySenderAvatarImg = new BitmapImage(friendRequestBodySenderAvatarUri);
                                            friendRequestBodySenderAvatar.Source = friendRequestBodySenderAvatarImg;
                                            friendRequestBodySenderAvatar.EndInit();
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
                                            Dictionary<String, Object>  acceptActionBtnData = new Dictionary<String, Object>();
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
            catch (System.Net.WebException)
            {
                MessageBox.Show("Не удается подключиться к серверу", "Ошибка");
                this.Close();
            }
        }

        public void GetUser (string userId)
        {
            currentUserId = userId;
            System.Diagnostics.Debugger.Log(0, "debug", "userId: " + userId + Environment.NewLine);
            try {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("https://digitaldistributtionservice.herokuapp.com/api/users/get/?id=" + userId);
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
                                string userLogin = currentUser.login;
                                ItemCollection userMenuItems = userMenu.Items;
                                ComboBoxItem userLoginLabel = ((ComboBoxItem)(userMenuItems[0]));
                                userLoginLabel.Content = userLogin;
                                ItemCollection profileMenuItems = profileMenu.Items;
                                object rawMainProfileMenuItem = profileMenuItems[0];
                                ComboBoxItem mainProfileMenuItem = ((ComboBoxItem)(rawMainProfileMenuItem));
                                mainProfileMenuItem.Content = userLogin;
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

        public void CloseManager ()
        {
            MessageBox.Show("Не удалось подключиться", "Ошибка");
            this.Close();
        }

        public void InitConstants ()
        {
            visible = Visibility.Visible;
            invisible = Visibility.Collapsed;
            friendRequestBackground = System.Windows.Media.Brushes.AliceBlue;
        }

        public void GetGamesList (string keywords)
        {
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("https://digitaldistributtionservice.herokuapp.com/api/games/get");
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
                            bool isGamesExists = countLoadedGames >= 1;
                            if (isGamesExists)
                            {
                                foreach (GameResponseInfo gamesListItem in loadedGames)
                                {
                                    StackPanel newGame = new StackPanel();
                                    newGame.MouseLeftButtonUp += SelectGameHandler;
                                    newGame.Orientation = Orientation.Horizontal;
                                    newGame.Height = 35;
                                    string gamesListItemName = gamesListItem.name;
                                    string gamesListItemUrl = gamesListItem.url;
                                    string gamesListItemImage = gamesListItem.image;
                                    Dictionary<String, Object> newGameData = new Dictionary<String, Object>();
                                    newGameData.Add("name", gamesListItemName);
                                    newGameData.Add("url", gamesListItemUrl);
                                    newGameData.Add("image", gamesListItemImage);
                                    newGame.DataContext = newGameData;
                                    Image newGamePhoto = new Image();
                                    newGamePhoto.Margin = new Thickness(5);
                                    newGamePhoto.Width = 25;
                                    newGamePhoto.Height = 25;
                                    newGamePhoto.BeginInit();
                                    Uri newGamePhotoUri = new Uri(gamesListItemImage);
                                    newGamePhoto.Source = new BitmapImage(newGamePhotoUri);
                                    newGamePhoto.EndInit();
                                    newGame.Children.Add(newGamePhoto);
                                    TextBlock newGameLabel = new TextBlock();
                                    newGameLabel.Margin = new Thickness(5);
                                    newGameLabel.Text = gamesListItem.name;
                                    newGame.Children.Add(newGameLabel);
                                    games.Children.Add(newGame);
                                }
                                GameResponseInfo firstGame = loadedGames[0];
                                Dictionary<String, Object> firstGameData = new Dictionary<String, Object>();
                                string firstGameName = firstGame.name;
                                string firstGameUrl = firstGame.url;
                                string firstGameImage = firstGame.image;
                                firstGameData.Add("name", firstGameName);
                                firstGameData.Add("url", firstGameUrl);
                                firstGameData.Add("image", firstGameImage);
                                SelectGame(firstGameData);
                            }
                        }
                    }
                }
            }
            catch
            {

            }
        }

        public void InitCache (string id)
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
                    games = new List<Game>()
                });
                File.WriteAllText(saveDataFilePath, savedContent);
            }
        }

        public void ShowOffers()
        {
            Dialogs.OffersDialog dialog = new Dialogs.OffersDialog();
            dialog.Show();
        }

        public void RunGame()
        {
            StartDetectGameHours();
            GameWindow window = new GameWindow(currentUserId);
            window.DataContext = gameActionLabel.DataContext;
            window.Closed += ComputeGameHoursHandler;
            window.Show();
        }

        public void StartDetectGameHours ()
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromHours(1);
            timer.Tick += GameHoursUpdateHandler;
            timer.Start();
            timerHours = 0;
        }

        public void GameHoursUpdateHandler (object sender, EventArgs e)
        {
            timerHours++;
        }

        public void ComputeGameHoursHandler (object sender, EventArgs e)
        {
            ComputeGameHours();
        }

        public void ComputeGameHours ()
        {
            timer.Stop();
            Environment.SpecialFolder localApplicationDataFolder = Environment.SpecialFolder.LocalApplicationData;
            string localApplicationDataFolderPath = Environment.GetFolderPath(localApplicationDataFolder);
            string saveDataFilePath = localApplicationDataFolderPath + @"\OfficeWare\GameManager\" + currentUserId + @"\save-data.txt";
            string gameName = gameNameLabel.Text;
            string appPath = localApplicationDataFolderPath + @"\OfficeWare\GameManager\" + currentUserId + @"\";
            string cachePath = appPath + gameName;
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
                string currentGameName = currentGame.name;
                string currentGamePath = currentGame.path;
                updatedGames[gameIndex] = new Game()
                {
                    name = currentGameName,
                    path = currentGamePath,
                    hours = rawTimerHours,
                    date = gameLastLaunchDate,
                };
                string savedContent = js.Serialize(new SavedContent
                {
                    games = updatedGames
                });
                File.WriteAllText(saveDataFilePath, savedContent);
            }
        }

        private void InstallGameHandler(object sender, RoutedEventArgs e)
        {
            InstallGame();
        }

        public void InstallGame()
        {

            /*
             * 
            */

            object rawGameActionLabelData = gameActionLabel.DataContext;
            Dictionary<String, Object> dataParts = ((Dictionary<String, Object>)(rawGameActionLabelData));
            string gameName = ((string)(dataParts["name"]));
            string gameUrl = ((string)(dataParts["url"]));
            string gameImg = ((string)(dataParts["image"]));

            Dialogs.DownloadGameDialog dialog = new Dialogs.DownloadGameDialog(currentUserId);
            dialog.DataContext = dataParts;
            dialog.Closed += GameDownloadedHandler;
            dialog.Show();
Environment.SpecialFolder localApplicationDataFolder = Environment.SpecialFolder.LocalApplicationData;
            string localApplicationDataFolderPath = Environment.GetFolderPath(localApplicationDataFolder);
            string appFolder = localApplicationDataFolderPath + @"\OfficeWare\GameManager\" + currentUserId + @"\";
            string cachePath = appFolder + gameName;
            string filename = cachePath + @"\game.exe";
            gameNameLabel.DataContext = ((string)(filename));
            gameActionLabel.IsEnabled = false;
        }

        private void wc_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            Exception downloadError = e.Error;
            bool isErrorsNotFound = downloadError == null;
            if (isErrorsNotFound)
            {
                GameSuccessDownloaded();
            }
            else
            {
                MessageBox.Show("Не удалось загрузить игру", "Ошибка");
            }
        }

        public void GameSuccessDownloaded ()
        {
            Environment.SpecialFolder localApplicationDataFolder = Environment.SpecialFolder.LocalApplicationData;
            string localApplicationDataFolderPath = Environment.GetFolderPath(localApplicationDataFolder);
            string saveDataFilePath = localApplicationDataFolderPath + @"\OfficeWare\GameManager\" + currentUserId + @"\save-data.txt";
            string gameName = gameNameLabel.Text;
            string appPath = localApplicationDataFolderPath + @"\OfficeWare\GameManager\" + currentUserId + @"\";
            string cachePath = appPath + gameName;
            Directory.CreateDirectory(cachePath);
            string filename = cachePath + @"\game.exe";
            JavaScriptSerializer js = new JavaScriptSerializer();
            string saveDataFileContent = File.ReadAllText(saveDataFilePath);
            SavedContent loadedContent = js.Deserialize<SavedContent>(saveDataFileContent);
            List<Game> updatedGames = loadedContent.games;
            object gameNameLabelData = gameNameLabel.DataContext;
            string gameUploadedPath = ((string)(gameNameLabelData));
            string gameHours = "0";
            DateTime currentDate = DateTime.Now;
            string gameLastLaunchDate = currentDate.ToLongDateString();
            updatedGames.Add(new Game()
            {
                name = gameName,
                path = gameUploadedPath,
                hours = gameHours,
                date = gameLastLaunchDate,
            });
            string savedContent = js.Serialize(new SavedContent
            {
                games = updatedGames
            });
            File.WriteAllText(saveDataFilePath, savedContent);
            gameActionLabel.Content = "Играть";
            gameActionLabel.IsEnabled = true;
            removeGameBtn.Visibility = visible;
            string gamePath = ((string)(gameNameLabel.DataContext));
            gameActionLabel.DataContext = filename;
            MessageBox.Show("Игра загружена", "Готово");
        }

        private void SelectGameHandler (object sender, MouseButtonEventArgs e)
        {
            StackPanel game = ((StackPanel)(sender));
            object rawGameData = game.DataContext;
            Dictionary<String, Object> gameData = ((Dictionary<String, Object>)(rawGameData));
            SelectGame(gameData);
        }

        public void SelectGame (Dictionary<String, Object> gameData)
        {
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
            string gameName = ((string)(dataParts["name"]));
            string gameUrl = ((string)(dataParts["url"]));
            string gameImg = ((string)(dataParts["image"]));
            gamePhoto.BeginInit();
            Uri gameImageUri = new Uri(gameImg);
            gamePhoto.Source = new BitmapImage(gameImageUri);
            gamePhoto.EndInit();
            bool isGameExists = gameNames.Contains(gameName);
            if (isGameExists)
            {
                gameActionLabel.Content = "Играть";
                int gameIndex = gameNames.IndexOf(gameName);
                Game loadedGame = loadedGames[gameIndex];
                string gamePath = loadedGame.path;
                gameActionLabel.DataContext = gamePath;
                removeGameBtn.Visibility = visible;
            }
            else
            {
                gameActionLabel.Content = "Установить";
                gameActionLabel.DataContext = gameData;
                removeGameBtn.Visibility = invisible;
            }
            gameNameLabel.Text = gameName;
        }

        private void GameActionHandler(object sender, RoutedEventArgs e)
        {
            GameAction();
        }

        public void GameAction()
        {
            object rawGameActionLabelContent = gameActionLabel.Content;
            string gameActionLabelContent = rawGameActionLabelContent.ToString();
            bool isPlayAction = gameActionLabelContent == "Играть";
            bool isInstallAction = gameActionLabelContent == "Установить";
            if (isPlayAction)
            {
                RunGame();
            }
            else if (isInstallAction)
            {
                InstallGame();
            }
        }

        private void RemoveGameHandler (object sender, RoutedEventArgs e)
        {
            RemoveGame();
        }

        public void RemoveGame ()
        {
            Environment.SpecialFolder localApplicationDataFolder = Environment.SpecialFolder.LocalApplicationData;
            string localApplicationDataFolderPath = Environment.GetFolderPath(localApplicationDataFolder);
            string saveDataFilePath = localApplicationDataFolderPath + @"\OfficeWare\GameManager\" + currentUserId + @"\save-data.txt";

            JavaScriptSerializer js = new JavaScriptSerializer();
            string saveDataFileContent = File.ReadAllText(saveDataFilePath);
            SavedContent loadedContent = js.Deserialize<SavedContent>(saveDataFileContent);
            List<Game> updatedGames = loadedContent.games;
            updatedGames = updatedGames.Where((Game someGame) =>
            {
                string gameName = gameNameLabel.Text;
                string someGameName = someGame.name;
                bool isCurrentGame = someGameName == gameName;
                bool isOtherGame = !isCurrentGame;
                string someGamePath = someGame.path;
                if (isCurrentGame)
                {
                    FileInfo fileInfo = new FileInfo(someGamePath);
                    string gameFolder = fileInfo.DirectoryName;
                    try
                    {
                        Directory.Delete(gameFolder, true);
                    }
                    catch (Exception)
                    {
                        isOtherGame = true;
                        MessageBox.Show("Игра запущена. Закройте ее и попробуйте удалить заново", "Ошибка");
                    }
                }
                return isOtherGame;
            }).ToList();
            string savedContent = js.Serialize(new SavedContent
            {
                games = updatedGames
            });
            File.WriteAllText(saveDataFilePath, savedContent);
            string keywords = keywordsLabel.Text;
            GetGamesList(keywords);
        }

        public void GameDownloadedHandler (object sender, EventArgs e)
        {
            Dialogs.DownloadGameDialog dialog = ((Dialogs.DownloadGameDialog)(sender));
            object dialogData = dialog.DataContext;
            string status = ((string)(dialogData));
            GameDownloaded(status);
        }

        public void GameDownloaded (string status)
        {
            bool isOkStatus = status == "OK";
            if (isOkStatus)
            {
                GameSuccessDownloaded();
            }
        }

        private void UserMenuItemSelectedHandler(object sender, RoutedEventArgs e)
        {
            ComboBox userMenu =  ((ComboBox)(sender));
            int userMenuItemIndex = userMenu.SelectedIndex;
            UserMenuItemSelected(userMenuItemIndex);
        }

        public void UserMenuItemSelected(int index)
        {
            bool isExit = index == 1;
            if (isExit)
            {
                Dialogs.LoginDialog dialog = new Dialogs.LoginDialog();
                dialog.Show();
                this.Close();
            }
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

        private void OpenFriendsDialogHandler (object sender, MouseButtonEventArgs e)
        {
            OpenFriendsDialog();
        }

        public void OpenFriendsDialog ()
        {
            Dialogs.FriendsDialog dialog = new Dialogs.FriendsDialog(currentUserId);
            dialog.Show();
        }

        public void CloseFriendRequestHandler (object sender, RoutedEventArgs e)
        {
            PackIcon btn = ((PackIcon)(sender));
            object btnData = btn.DataContext;
            Popup request = ((Popup)(btnData));
            CloseFriendRequest(request);
        }

            
        public void CloseFriendRequest (Popup request)
        {
            friendRequests.Children.Remove(request);
        }

        public void RejectFriendRequestHandler (object sender, RoutedEventArgs e)
        {
            Button btn = ((Button)(sender));
            object rawBtnData = btn.DataContext;
            Dictionary<String, Object> btnData = ((Dictionary<String, Object>)(rawBtnData));
            string friendId = ((string)(btnData["friendId"]));
            string requestId = ((string)(btnData["requestId"]));
            Popup request = ((Popup)(btnData["request"]));
            RejectFriendRequest(friendId, requestId, request);
        }

        public void RejectFriendRequest (string friendId, string requestId, Popup request)
        {
            try {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("https://digitaldistributtionservice.herokuapp.com/api/friends/requests/reject/?id=" + requestId);
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

        public void AcceptFriendRequestHandler (object sender, RoutedEventArgs e)
        {
            Button btn = ((Button)(sender));
            object rawBtnData = btn.DataContext;
            Dictionary<String, Object> btnData = ((Dictionary<String, Object>)(rawBtnData));
            string friendId = ((string)(btnData["friendId"]));
            string requestId = ((string)(btnData["requestId"]));
            Popup request = ((Popup)(btnData["request"]));
            AcceptFriendRequest(friendId, requestId, request);
        }

        public void AcceptFriendRequest (string friendId, string requestId, Popup request)
        {
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("https://digitaldistributtionservice.herokuapp.com/api/friends/add/?id=" + currentUserId + @"&friend=" + friendId + "&request=" + requestId);
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
                                        CloseFriendRequest(request);
                                        User friend = myobj.user;
                                        string friendLogin = friend.login;
                                        string msgContent = "Пользователь " + friendLogin + " был добавлен в друзья";
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

        public CustomPopupPlacement[] FriendRequestPlacementHandler (Size popupSize, Size targetSize, Point offset)
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

        public void FilterGames ()
        {
            string keywords = keywordsLabel.Text;
            GetGamesList(keywords);
        }

        private void ProfileItemSelectedHandler (object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = ((ComboBox)(sender));
            int selectedIndex = comboBox.SelectedIndex;
            ProfileItemSelected(selectedIndex);
        }

        private void ProfileItemSelected (int index)
        {
            bool isProfile = index == 2;
            if (isProfile) {
                mainControl.SelectedIndex = 1;
            }
            ResetMenu();
        }

        private void LibraryItemSelectedHandler (object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = ((ComboBox)(sender));
            int selectedIndex = comboBox.SelectedIndex;
            LibraryItemSelected(selectedIndex);
        }

        private void LibraryItemSelected (int index)
        {
            bool isHome = index == 1;
            if (isHome)
            {
                mainControl.SelectedIndex = 0;
            }
            ResetMenu();
        }

        public void ResetMenu ()
        {
            if (isAppInit)
            {
                storeMenu.SelectedIndex = 0;
                libraryMenu.SelectedIndex = 0;
                communityMenu.SelectedIndex = 0;
                profileMenu.SelectedIndex = 0;
            }
        }

        private void ClientLoadedHandler (object sender, RoutedEventArgs e)
        {
            ClientLoaded();
        }

        public void ClientLoaded ()
        {
            isAppInit = true;
            ListenSockets();
        }

        private void CommunityItemSelectedHandler(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = ((ComboBox)(sender));
            int selectedIndex = comboBox.SelectedIndex;
            CommunityItemSelected(selectedIndex);
        }

        public void CommunityItemSelected (int index) {
            bool isHome = index == 1;
            if (isHome)
            {
                mainControl.SelectedIndex = 0;
            }
            ResetMenu();
        }

        private void StoreItemSelectedHandler (object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = ((ComboBox)(sender));
            int selectedIndex = comboBox.SelectedIndex;
            StoreItemSelected(selectedIndex);
        }

        public void StoreItemSelected (int index)
        {
            bool isHome = index == 1;
            if (isHome)
            {
                mainControl.SelectedIndex = 0;
            }
            ResetMenu();
        }

        private void OpenEditProfileHandler (object sender, RoutedEventArgs e)
        {
            OpenEditProfile();
        }

        public void OpenEditProfile ()
        {
            mainControl.SelectedIndex = 2;
        }

        private void SaveUserInfoHandler (object sender, RoutedEventArgs e)
        {
            SaveUserInfo();
        }

        public void SaveUserInfo ()
        {
            string userNameBoxContent = userNameBox.Text;
            int selectedCountryIndex = userCountryBox.SelectedIndex;
            ItemCollection userCountryBoxItems = userCountryBox.Items;
            object rawSelectedUserCountryBoxItem = userCountryBoxItems[selectedCountryIndex];
            ComboBoxItem selectedUserCountryBoxItem = ((ComboBoxItem)(rawSelectedUserCountryBoxItem));
            object rawUserCountryBoxContent = selectedUserCountryBoxItem.Content;
            string userCountryBoxContent = ((string)(rawUserCountryBoxContent));
            string userAboutBoxContent = userAboutBox.Text;
            HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/user/edit/?id=" + currentUserId + "&name=" + userNameBoxContent + "&country=" + userCountryBoxContent + "&about=" + userAboutBoxContent);
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
                    bool isOkStatus = status == "OK";
                    if (isOkStatus)
                    {
                        MessageBox.Show("Профиль был обновлен", "Внимание");
                        GetUserInfo();
                        GetEditInfo();
                    }
                    else
                    {
                        MessageBox.Show("Не удается редактировать профиль", "Ошибка");
                    }
                }
            }
        }

        public async void ListenSockets()
        {

            var client = new SocketIO("http://localhost:4000/");
            client.OnConnected += async (sender, e) =>
            {
                Debugger.Log(0, "debug", "client socket conntected");
                client.EmitAsync("user_is_online", currentUserId);
            };
            client.On("friend_is_online", response =>
            {
                Debugger.Log(0, "debug", response.ToString());
                var result = response.GetValue<string>();
                Debugger.Log(0, "debug", result);
            });
            await client.ConnectAsync();
        }

    }

    class SavedContent
    {
        public List<Game> games;
    }

    class Game
    {
        public string name;
        public string path;
        public string hours;
        public string date;
    }

    class GamesListResponseInfo
    {
        public string status;
        public List<GameResponseInfo> games;
    }

    class GameResponseInfo
    {
        public string name;
        public string url;
        public string image;
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

}
