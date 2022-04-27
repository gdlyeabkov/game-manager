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
            GetScreenShots("", true);
            GetForums("");
            GetGameCollections();/**/
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
                        string currentGameName = gameForCollection.name;
                        string currentGameCover = gameForCollection.cover;
                        // Loads the images to tile (no need to specify PngBitmapDecoder, the correct decoder is automatically selected)
                        bool isCoverSet = currentGameCover != "";
                        bool isCoverFound = File.Exists(currentGameCover);
                        bool isCoverExists = isCoverSet && isCoverFound;
                        Uri coverUri = null;
                        if (isCoverExists)
                        {
                            coverUri = new Uri(currentGameCover);
                        }
                        else
                        {
                            coverUri = new Uri(@"http://localhost:4000/api/game/thumbnail/?name=" + currentGameName);
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
                collections = updatedCollections
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

            /*mainControl.SelectedIndex = 0;
            gamesLibraryControl.SelectedIndex = 2;*/

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
            List<Game> collectionGames = currentGames.Where<Game>((Game game) =>
            {
                List<string> gameCollections = game.collections;
                bool isGameForCollection = gameCollections.Contains(name);
                return isGameForCollection;
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
                    if (isGameForCurrentCollection)
                    {
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

                        MenuItem gameCollectionItemNestedContextMenuItem = new MenuItem();
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
                        Dictionary<String, Object> gameCollectionItemNestedContextMenuItemData = new Dictionary<String, Object>();
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
                        bool isHiddenGame = currentGame.isHidden;
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
                    collections = currentCollections
                });
                File.WriteAllText(saveDataFilePath, savedContent);
                GetGameCollections();
                GetGameCollectionItems(collection);
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
                collections = currentCollections
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
                mainControl.SelectedIndex = 0;
                SelectGame(myGameData);
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
                                collections = currentCollections
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
                // HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/users/get/?id=" + id);
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
                                        string currentUserCountry = user.country;
                                        userProfileCountryLabel.Text = currentUserCountry;

                                        userProfileAvatar.BeginInit();
                                        Uri avatar = new Uri(@"http://localhost:4000/api/user/avatar/?id=" + id);
                                        userProfileAvatar.Source = new BitmapImage(avatar);
                                        userProfileAvatar.EndInit();


                                        gamesSettings = user.gamesSettings;
                                        friendsSettings = user.friendsListSettings;

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
            editProfileBtn.IsEnabled = isLocalUser;

            /*Visibility visibility = Visibility.Collapsed;
            if (isLocalUser)
            {
                visibility = Visibility.Visible;
            }
            else
            {
                visibility = Visibility.Collapsed;
            }
            userProfileDetails.Visibility = visibility;*/
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

            mainControl.DataContext = currentUserId;

        }

        public void GetFriendRequests()
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

                                            // Uri friendRequestBodySenderAvatarUri = new Uri("https://cdn2.iconfinder.com/data/icons/ios-7-icons/50/user_male-128.png");
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
                                        Dictionary<String, Object> newGameData = new Dictionary<String, Object>();
                                        newGameData.Add("id", gamesListItemId);
                                        newGameData.Add("name", gamesListItemName);
                                        newGameData.Add("url", gamesListItemUrl);
                                        newGameData.Add("image", gamesListItemImage);
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

                                        ContextMenu newGameContextMenu = new ContextMenu();
                                        MenuItem newGameContextMenuItem = new MenuItem();
                                        newGameContextMenuItem.Header = "Добавить в коллекцию";
                                        List<Game> gameSearchResults = currentGames.Where<Game>((Game game) =>
                                        {
                                            string gameName = game.name;
                                            bool isGameFound = gameName == gamesListItemName;
                                            return isGameFound;
                                        }).ToList<Game>();
                                        int countGameSearchResults = gameSearchResults.Count;
                                        bool isGameInstalled = countGameSearchResults >= 1;
                                        bool isGameNotInstalled = !isGameInstalled;
                                        if (isGameNotInstalled)
                                        {
                                            newGameContextMenuItem.IsEnabled = false;
                                        }
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
                                        newGame.ContextMenu = newGameContextMenu;
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

                                        Debugger.Log(0, "debug", Environment.NewLine + "firstGameId: " + firstGameId + Environment.NewLine);
                                        firstGameData.Add("id", firstGameId);
                                        firstGameData.Add("name", firstGameName);
                                        firstGameData.Add("url", firstGameUrl);
                                        firstGameData.Add("image", firstGameImage);
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
                    collections = currentCollections
                });
                File.WriteAllText(saveDataFilePath, saveDataFileContent);
                GetGameCollections();
                GetGamesList("");
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
            string cacheScreenShotsPath = localApplicationDataFolderPath + @"\OfficeWare\GameManager\" + id + @"\screenshots"; ;

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
                        showOverlay = true
                    },
                    collections = new List<string>() { }
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
            dialog.Show();
        }

        async public void RunGame(string joinedGameName = "")
        {
            StartDetectGameHours();
            GameWindow window = new GameWindow(currentUserId);
            window.DataContext = gameActionLabel.DataContext;
            window.Closed += ComputeGameHoursHandler;
            window.Show();
            string gameName = gameNameLabel.Text;
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
            Environment.SpecialFolder localApplicationDataFolder = Environment.SpecialFolder.LocalApplicationData;
            string localApplicationDataFolderPath = Environment.GetFolderPath(localApplicationDataFolder);
            string saveDataFilePath = localApplicationDataFolderPath + @"\OfficeWare\GameManager\" + currentUserId + @"\save-data.txt";
            string currentGameName = joinedGameName;
            int joinedGameNameLength = joinedGameName.Length;
            bool isNotJoinedGame = joinedGameNameLength <= 0;
            if (isNotJoinedGame)
            {
                currentGameName = gameNameLabel.Text;
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
                    cover = ""
                };
                string savedContent = js.Serialize(new SavedContent
                {
                    games = updatedGames,
                    friends = currentFriends,
                    settings = currentSettings,
                    collections = currentCollections
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


        public void GameSuccessDownloaded(string id)
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
                cover = ""
            });
            string savedContent = js.Serialize(new SavedContent
            {
                games = updatedGames,
                friends = currentFriends,
                settings = currentSettings,
                collections = currentCollections
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

            MessageBox.Show(gameUploadedLabelContent, attentionLabelContent);
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
            Application.Current.Dispatcher.Invoke(() =>
            {
                // gamePhoto.BeginInit();
                Uri gameImageUri = new Uri(gameImg);
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
            }
            else
            {
                gameActionLabel.Content = Properties.Resources.installBtnLabelContent;
                gameActionLabel.DataContext = gameData;
                removeGameBtn.Visibility = invisible;
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

        public void GameAction()
        {
            object rawGameActionLabelContent = gameActionLabel.Content;
            string gameActionLabelContent = rawGameActionLabelContent.ToString();
            bool isPlayAction = gameActionLabelContent == Properties.Resources.playBtnLabelContent;
            bool isInstallAction = gameActionLabelContent == Properties.Resources.installBtnLabelContent;
            if (isPlayAction)
            {
                RunGame();
            }
            else if (isInstallAction)
            {
                InstallGame();
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
            updatedGames = updatedGames.Where((Game someGame) =>
            {
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

                    try
                    {
                        Directory.Delete(gameFolder, true);

                        Directory.Delete(gameScreenShotsFolder, true);

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
                games = updatedGames,
                friends = currentFriends,
                settings = currentSettings,
                collections = currentCollections
            });
            File.WriteAllText(saveDataFilePath, savedContent);
            string keywords = keywordsLabel.Text;
            GetGamesList(keywords);
            GetGameCollections();
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
            bool isExit = index == 1;
            if (isExit)
            {
                Logout();
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
                RunGame();
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
                                            settings = currentSettings
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
                bool isProfile = index == 2;
                bool isContent = index == 5;
                if (isProfile)
                {
                    object mainControlData = mainControl.DataContext;
                    string userId = ((string)(mainControlData));
                    // string userId = currentUserId;
                    bool isLocalUser = userId == currentUserId;
                    mainControl.SelectedIndex = 1;
                    GetUserInfo(userId, isLocalUser);
                    AddHistoryRecord();
                }
                else if (isContent)
                {
                    mainControl.SelectedIndex = 5;
                    AddHistoryRecord();
                }
                ResetMenu();
            }
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
            bool isDownloads = index == 3;
            if (isHome)
            {
                mainControl.SelectedIndex = 0;

                AddHistoryRecord();

            }
            else if (isCollections)
            {

                mainControl.SelectedIndex = 9;

                /*mainControl.SelectedIndex = 0;
                gamesLibraryControl.SelectedIndex = 1;*/

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
            bool isDiscussions = index == 2;
            if (isDiscussions)
            {
                mainControl.SelectedIndex = 6;

                AddHistoryRecord();

            }
            ResetMenu();
        }

        private void StoreItemSelectedHandler(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = ((ComboBox)(sender));
            int selectedIndex = comboBox.SelectedIndex;
            StoreItemSelected(selectedIndex);
        }

        public void StoreItemSelected(int index)
        {
            bool isGamesStats = index == 6;
            if (isGamesStats)
            {
                mainControl.SelectedIndex = 3;
                GetGamesStats();

                AddHistoryRecord();

            }
            ResetMenu();
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
                        settings = updatedSettings
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

                // client = new SocketIO("http://localhost:4000/");
                client = new SocketIO("https://digitaldistributtionservice.herokuapp.com/");

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

        public void GetScreenShots(string filter, bool isInit)
        {
            Environment.SpecialFolder localApplicationDataFolder = Environment.SpecialFolder.LocalApplicationData;
            string localApplicationDataFolderPath = Environment.GetFolderPath(localApplicationDataFolder);
            // string appPath = localApplicationDataFolderPath + @"\OfficeWare\GameManager\" + currentUserId + @"\";
            // string appPath = localApplicationDataFolderPath + @"\OfficeWare\GameManager\" + currentUserId + @"\games\";
            string appPath = localApplicationDataFolderPath + @"\OfficeWare\GameManager\" + currentUserId + @"\screenshots\";
            string[] games = Directory.GetDirectories(appPath);
            screenShots.Children.Clear();
            foreach (string game in games)
            {
                // FileInfo gameInfo = new FileInfo(game);
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
                            screenShots.Children.Add(screenShot);
                        }
                    }
                }
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

        private void CreateCollectionHandler(object sender, MouseButtonEventArgs e)
        {
            CreateCollection();
        }

        public void CreateCollection()
        {
            Dialogs.AddCollectiionDialog dialog = new Dialogs.AddCollectiionDialog(currentUserId);
            dialog.Closed += RefreshGameCollectionsHandler;
            dialog.Show();
        }

        public void RefreshGameCollectionsHandler(object sender, EventArgs e)
        {
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
            mainControl.SelectedIndex = 1;
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

        public void GetHiddenGames()
        {
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
            }).ToList();
            int hiddenGamesCount = shadowGames.Count;
            bool isHaveGames = hiddenGamesCount >= 1;
            hiddenGames.HorizontalAlignment = HorizontalAlignment.Left;
            hiddenGames.Children.Clear();
            if (isHaveGames)
            {
                foreach (Game hiddenGame in shadowGames)
                {
                    string currentGameName = hiddenGame.name;
                    Image gameCollectionItem = new Image();
                    gameCollectionItem.Width = 100;
                    gameCollectionItem.Height = 100;
                    gameCollectionItem.Margin = new Thickness(25);
                    gameCollectionItem.BeginInit();
                    gameCollectionItem.Source = new BitmapImage(new Uri(@"http://localhost:4000/api/game/thumbnail/?name=" + currentGameName));
                    gameCollectionItem.ImageFailed += SetDefaultThumbnailHandler;
                    gameCollectionItem.EndInit();
                    hiddenGames.Children.Add(gameCollectionItem);
                    gameCollectionItem.DataContext = currentGameName;
                    gameCollectionItem.MouseLeftButtonUp += SelectGameCollectionItemHandler;
                    ContextMenu gameCollectionItemContextMenu = new ContextMenu();
                    MenuItem gameCollectionItemContextMenuItem = new MenuItem();
                    string gameCollectionItemContextMenuItemHeaderContent = "Убрать из ";
                    gameCollectionItemContextMenuItem.Header = gameCollectionItemContextMenuItemHeaderContent;
                    MenuItem gameCollectionItemNestedContextMenuItem;
                    Dictionary<String, Object> gameCollectionItemNestedContextMenuItemData;
                    foreach (string hiddenGameCollection in hiddenGame.collections)
                    {
                        gameCollectionItemNestedContextMenuItem = new MenuItem();
                        gameCollectionItemNestedContextMenuItemData = new Dictionary<String, Object>();
                        gameCollectionItemNestedContextMenuItemData.Add("game", currentGameName);
                        gameCollectionItemNestedContextMenuItemData.Add("collection", hiddenGameCollection);
                        gameCollectionItemNestedContextMenuItem.DataContext = gameCollectionItemNestedContextMenuItemData;
                        gameCollectionItemNestedContextMenuItem.Click += RemoveGameFromCollectionHandler;
                        gameCollectionItemContextMenu.Items.Add(gameCollectionItemNestedContextMenuItem);
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

                    gameCollectionItemContextMenu.Items.Add(gameCollectionItemContextMenuItem);

                    gameCollectionItem.ContextMenu = gameCollectionItemContextMenu;
                }
            }
            else
            {
                gameCollectionItems.HorizontalAlignment = HorizontalAlignment.Center;
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
                    collections = currentCollections
                });
                File.WriteAllText(saveDataFilePath, savedContent);
            }

            GetGameCollectionItems(collection);
            GetGamesList("");

        }

    }

    class SavedContent
    {
        public List<Game> games;
        public List<FriendSettings> friends;
        public Settings settings;
        public List<String> collections;
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

}