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

        public string filename = @"C:\Gleb\game-manager\game.exe";

        public MainWindow()
        {
            InitializeComponent();

            Initialize();

        }

        public void Initialize()
        {

            InitCache();
            ShowOffers();
            GetGamesList();

        }

        public void GetGamesList ()
        {
            try
            {
                // HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/games/get");
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
                        System.Diagnostics.Debugger.Log(0, "debug", "status: " + myobj.status + Environment.NewLine);
                        if (myobj.status == "OK")
                        {
                            if (myobj.games.Count >= 1)
                            {
                                games.Children.Clear();
                                foreach (GameResponseInfo gamesListItem in myobj.games)
                                {
                                    StackPanel newGame = new StackPanel();
                                    newGame.MouseLeftButtonUp += SelectGameHandler;
                                    newGame.Orientation = Orientation.Horizontal;
                                    newGame.Height = 35;
                                    // string newGameData = gamesListItem.name + "|" + gamesListItem.url + "|" + gamesListItem.image;
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
                                    newGamePhoto.Source = new BitmapImage(new Uri(gamesListItem.image));
                                    newGamePhoto.EndInit();
                                    newGame.Children.Add(newGamePhoto);
                                    TextBlock newGameLabel = new TextBlock();
                                    newGameLabel.Margin = new Thickness(5);
                                    newGameLabel.Text = gamesListItem.name;
                                    newGame.Children.Add(newGameLabel);
                                    games.Children.Add(newGame);
                                }
                                GameResponseInfo firstGame = myobj.games[0];
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

        public void InitCache ()
        {
            Environment.SpecialFolder localApplicationDataFolder = Environment.SpecialFolder.LocalApplicationData;
            string localApplicationDataFolderPath = Environment.GetFolderPath(localApplicationDataFolder);
            string saveDataFilePath = localApplicationDataFolderPath + @"\OfficeWare\GameManager\save-data.txt";
            string cachePath = localApplicationDataFolderPath + @"\OfficeWare\GameManager";
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
            GameWindow window = new GameWindow();
            window.DataContext = gameActionLabel.DataContext;
            window.Show();
        }

        private void InstallGameHandler(object sender, RoutedEventArgs e)
        {
            InstallGame();
        }

        public void InstallGame()
        {
            object rawGameActionLabelData = gameActionLabel.DataContext;
            Dictionary<String, Object> dataParts = ((Dictionary<String, Object>)(rawGameActionLabelData));
            string gameName = ((string)(dataParts["name"]));
            string gameUrl = ((string)(dataParts["url"]));
            string gameImg = ((string)(dataParts["image"]));
            Uri uri = new Uri(gameUrl);
            WebClient wc = new WebClient();
            Environment.SpecialFolder localApplicationDataFolder = Environment.SpecialFolder.LocalApplicationData;
            string localApplicationDataFolderPath = Environment.GetFolderPath(localApplicationDataFolder);
            string appFolder = localApplicationDataFolderPath + @"\OfficeWare\GameManager\";
            string cachePath = appFolder + gameName;
            Directory.CreateDirectory(cachePath);
            filename = cachePath + @"\game.exe";
            wc.DownloadFileAsync(uri, filename);
            gameNameLabel.DataContext = ((string)(filename));
            wc.DownloadProgressChanged += new DownloadProgressChangedEventHandler(wc_DownloadProgressChanged);
            wc.DownloadFileCompleted += new AsyncCompletedEventHandler(wc_DownloadFileCompleted);
        }

        private void wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            gameInstalledProgress.Value = e.ProgressPercentage;
            double progress = gameInstalledProgress.Value;
            double maxProgress = gameInstalledProgress.Maximum;
            bool isDownloaded = progress == maxProgress;
            if (isDownloaded)
            {
                gameInstalledProgress.Value = 0;
            }
        }
        private void wc_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            Exception downloadError = e.Error;
            bool isErrorsNotFound = downloadError == null;
            if (isErrorsNotFound)
            {
                Environment.SpecialFolder localApplicationDataFolder = Environment.SpecialFolder.LocalApplicationData;
                string localApplicationDataFolderPath = Environment.GetFolderPath(localApplicationDataFolder);
                string saveDataFilePath = localApplicationDataFolderPath + @"\OfficeWare\GameManager\save-data.txt";
                string gameName = gameNameLabel.Text;
                string appPath = localApplicationDataFolderPath + @"\OfficeWare\GameManager\";
                string cachePath = appPath + gameName;
                Directory.CreateDirectory(cachePath);
                filename = cachePath + @"\game.exe";
                JavaScriptSerializer js = new JavaScriptSerializer();
                string saveDataFileContent = File.ReadAllText(saveDataFilePath);
                SavedContent loadedContent = js.Deserialize<SavedContent>(saveDataFileContent);
                List<Game> updatedGames = loadedContent.games;
                object gameNameLabelData = gameNameLabel.DataContext;
                string gameUloadedPath = ((string)(gameNameLabelData));
                updatedGames.Add(new Game()
                {
                    name = gameName,
                    path = gameUloadedPath
                });
                string savedContent = js.Serialize(new SavedContent
                {
                    games = updatedGames
                });
                File.WriteAllText(saveDataFilePath, savedContent);
                gameActionLabel.Content = "Играть";
                string gamePath = ((string)(gameNameLabel.DataContext));
                gameActionLabel.DataContext = filename;
                gameInstalledProgress.Visibility = Visibility.Hidden;

                MessageBox.Show("Игра загружена", "Готово");
            }
            else
            {
                MessageBox.Show("Не удалось загрузить игру", "Ошибка");
            }
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
            string saveDataFilePath = localApplicationDataFolderPath + @"\OfficeWare\GameManager\save-data.txt";
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
                Visibility invisible = Visibility.Hidden;
                gameInstalledProgress.Visibility = invisible;
            }
            else
            {
                gameActionLabel.Content = "Установить";
                gameActionLabel.DataContext = gameData;
                Visibility visible = Visibility.Visible;
                gameInstalledProgress.Visibility = visible;
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



}
