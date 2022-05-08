using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
    /// Логика взаимодействия для GameSettingsDialog.xaml
    /// </summary>
    public partial class GameSettingsDialog : Window
    {

        public string currentUserId = "";
        public string gameName = "";
        public bool isCustomGame = false;

        public GameSettingsDialog(string currentUserId, string name, bool isCustomGame)
        {
            InitializeComponent();

            Initialize(currentUserId, name, isCustomGame);

        }

        public void Initialize(string currentUserId, string name, bool isCustomGame)
        {
            this.currentUserId = currentUserId;
            this.gameName = name;
            this.isCustomGame = isCustomGame;
            gameNameLabel.Text = gameName;
            Environment.SpecialFolder localApplicationDataFolder = Environment.SpecialFolder.LocalApplicationData;
            string localApplicationDataFolderPath = Environment.GetFolderPath(localApplicationDataFolder);
            string userPath = localApplicationDataFolderPath + @"\OfficeWare\GameManager\";
            string saveDataFilePath = userPath + currentUserId + @"\save-data.txt";
            string currentGamePath = userPath + currentUserId + @"\games\" + gameName + @"\game.exe";

            JavaScriptSerializer js = new JavaScriptSerializer();
            string saveDataFileContent = File.ReadAllText(saveDataFilePath);
            SavedContent loadedContent = js.Deserialize<SavedContent>(saveDataFileContent);
            List<Game> currentGames = loadedContent.games;
            List<Game> results = currentGames.Where<Game>((Game game) =>
            {
                string localGameName = game.name;
                bool isFound = localGameName == gameName;
                return isFound;
            }).ToList();
            int countResults = results.Count;
            bool isHaveResults = countResults >= 1;
            Game result = null;
            if (isHaveResults)
            {
                result = results[0];
            }
            if (isCustomGame)
            {
                if (isHaveResults)
                {
                    string resultPath = result.path;
                    FileInfo currentGameInfo = new FileInfo(resultPath);
                    long currentGameSize = currentGameInfo.Length;
                    double currentGameSizeInGb = currentGameSize / 1024 / 1024 / 1024;
                    string rawCurrentGameSize = currentGameSizeInGb + " Гб";
                    string newLine = Environment.NewLine;
                    string volumeLabel = System.IO.Path.GetPathRoot(currentGamePath);
                    string gameFilesSizeLabelContent = rawCurrentGameSize + @" на диске" + newLine + volumeLabel;
                    gameFilesSizeLabel.Text = gameFilesSizeLabelContent;
                }
            }
            else
            {
                FileInfo currentGameInfo = new FileInfo(currentGamePath);
                long currentGameSize = currentGameInfo.Length;
                double currentGameSizeInGb = currentGameSize / 1024 / 1024 / 1024;
                string rawCurrentGameSize = currentGameSizeInGb + " Гб";
                string newLine = Environment.NewLine;
                string volumeLabel = System.IO.Path.GetPathRoot(currentGamePath);
                string gameFilesSizeLabelContent = rawCurrentGameSize + @" на диске" + newLine + volumeLabel;
                gameFilesSizeLabel.Text = gameFilesSizeLabelContent;
            }
            if (isHaveResults)
            {
                bool isOverlayEnabled = result.overlay;
                overlayCheckBox.IsChecked = isOverlayEnabled;
            }
        }

        private void ToggleSettingsHandler(object sender, MouseButtonEventArgs e)
        {
            StackPanel settingsItem = ((StackPanel)(sender));
            object settingsItemData = settingsItem.DataContext;
            string rawIndex = settingsItemData.ToString();
            int index = Int32.Parse(rawIndex);
            ToggleSettings(index);
        }

        public void ToggleSettings(int index)
        {
            settingsControl.SelectedIndex = index;
        }

        private void ShowLocalFilesHandler(object sender, RoutedEventArgs e)
        {
            ShowLocalFiles();
        }

        public void ShowLocalFiles()
        {

            Environment.SpecialFolder localApplicationDataFolder = Environment.SpecialFolder.LocalApplicationData;
            string localApplicationDataFolderPath = Environment.GetFolderPath(localApplicationDataFolder);
            string appPath = localApplicationDataFolderPath + @"\OfficeWare\GameManager\";
            string gamePath = appPath + currentUserId + @"\games\" + gameName;
            string saveDataFilePath = appPath + currentUserId + @"\save-data.txt";
            JavaScriptSerializer js = new JavaScriptSerializer();
            string saveDataFileContent = File.ReadAllText(saveDataFilePath);
            SavedContent loadedContent = js.Deserialize<SavedContent>(saveDataFileContent);
            List<Game> updatedGames = loadedContent.games;
            List<FriendSettings> currentFriends = loadedContent.friends;
            Settings currentSettings = loadedContent.settings;
            List<string> currentCollections = loadedContent.collections;
            List<Game> results = updatedGames.Where<Game>((Game game) =>
            {
                string localGameName = game.name;
                bool isFound = gameName == localGameName;
                return isFound;
            }).ToList();
            int countResults = results.Count;
            bool isHaveResults = countResults >= 1;
            if (isHaveResults)
            {
                Game myGame = results[0];
                if (isCustomGame)
                {
                    string filePath = myGame.path;
                    FileInfo info = new FileInfo(filePath);
                    DirectoryInfo dirInfo = info.Directory;
                    gamePath = dirInfo.FullName;
                }
            }
            System.Diagnostics.Process.Start(new ProcessStartInfo
            {
                FileName = "explorer",
                Arguments = gamePath,
                UseShellExecute = true
            });
        }

        private void ToggleOverlayHandler(object sender, RoutedEventArgs e)
        {
            ToggleOverlay();
        }

        private void ToggleOverlay()
        {
            object rawIsOverlayEnabled = overlayCheckBox.IsChecked;
            bool isOverlayEnabled = ((bool)(rawIsOverlayEnabled));
            Environment.SpecialFolder localApplicationDataFolder = Environment.SpecialFolder.LocalApplicationData;
            string localApplicationDataFolderPath = Environment.GetFolderPath(localApplicationDataFolder);
            string userPath = localApplicationDataFolderPath + @"\OfficeWare\GameManager\";
            string saveDataFilePath = userPath + currentUserId + @"\save-data.txt";
            JavaScriptSerializer js = new JavaScriptSerializer();
            string saveDataFileContent = File.ReadAllText(saveDataFilePath);
            SavedContent loadedContent = js.Deserialize<SavedContent>(saveDataFileContent);
            List<Game> updatedGames = loadedContent.games;
            List<FriendSettings> currentFriends = loadedContent.friends;
            Settings currentSettings = loadedContent.settings;
            List<string> currentCollections = loadedContent.collections;
            Notifications currentNotifications = loadedContent.notifications;
            List<string> currentCategories = loadedContent.categories;
            List<Game> results = updatedGames.Where<Game>((Game game) =>
            {
                string localGameName = game.name;
                bool isFound = gameName == localGameName;
                return isFound;
            }).ToList();
            int countResults = results.Count;
            bool isHaveResults = countResults >= 1;
            if (isHaveResults)
            {
                Game result = results[0];
                result.overlay = isOverlayEnabled;
                string savedContent = js.Serialize(new SavedContent
                {
                    games = updatedGames,
                    friends = currentFriends,
                    settings = currentSettings,
                    collections = currentCollections,
                    notifications = currentNotifications,
                    categories = currentCategories
                });
                File.WriteAllText(saveDataFilePath, savedContent);
            }
        }
    }
}