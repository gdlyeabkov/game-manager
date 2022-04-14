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
            GetGamesList();

        }

        public void GetUser (string userId)
        {
            currentUserId = userId;
            System.Diagnostics.Debugger.Log(0, "debug", "userId: " + userId + Environment.NewLine);
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

        public void CloseManager ()
        {
            MessageBox.Show("Не удалось подключиться", "Ошибка");
            this.Close();
        }

        public void InitConstants ()
        {
            visible = Visibility.Visible;
            invisible = Visibility.Collapsed;
        }

        public void GetGamesList ()
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
                            List<GameResponseInfo> loadedGames = myobj.games;
                            int countLoadedGames = loadedGames.Count;
                            bool isGamesExists = countLoadedGames >= 1;
                            if (isGamesExists)
                            {
                                games.Children.Clear();
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
            GameWindow window = new GameWindow(currentUserId);
            window.DataContext = gameActionLabel.DataContext;
            window.Show();
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
            updatedGames.Add(new Game()
            {
                name = gameName,
                path = gameUploadedPath
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
            GetGamesList();
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

        private void OpenAddFriendHandler (object sender, RoutedEventArgs e)
        {
            OpenAddFriend();
        }

        public void OpenAddFriend()
        {
            Dialogs.LoginDialog dialog = new Dialogs.LoginDialog();
            dialog.Show();
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
        public string login;
        public string password;
    }

}
