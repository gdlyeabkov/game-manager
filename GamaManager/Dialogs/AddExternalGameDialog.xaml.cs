using Microsoft.Win32;
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
    /// Логика взаимодействия для AddExternalGameDialog.xaml
    /// </summary>
    public partial class AddExternalGameDialog : Window
    {

        public string currentUserId = "";

        public AddExternalGameDialog(string currentUserId)
        {
            InitializeComponent();

            Initialize(currentUserId);

        }

        public void Initialize(string currentUserId)
        {
            this.currentUserId = currentUserId;
            GetInstalledApps();
        }

        public void GetInstalledApps()
        {
            string uninstallKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
            using (RegistryKey rk = Registry.LocalMachine.OpenSubKey(uninstallKey))
            {
                foreach (string skName in rk.GetSubKeyNames())
                {
                    using (RegistryKey sk = rk.OpenSubKey(skName))
                    {
                        try
                        {
                            object rawDisplayName = sk.GetValue("DisplayName");
                            string displayName = ((string)(rawDisplayName));
                            RowDefinition row = new RowDefinition();
                            row.Height = new GridLength(35);
                            apps.RowDefinitions.Add(row);
                            RowDefinitionCollection rows = apps.RowDefinitions;
                            int rowsCount = rows.Count;
                            int lastRowIndex = rowsCount - 1;
                            CheckBox checkBox = new CheckBox();
                            checkBox.DataContext = null;
                            checkBox.VerticalAlignment = VerticalAlignment.Center;
                            apps.Children.Add(checkBox);
                            checkBox.Click += ReComputeSelectedAppsHandler;
                            Grid.SetRow(checkBox, lastRowIndex);
                            Grid.SetColumn(checkBox, 0);
                            StackPanel app = new StackPanel();
                            app.VerticalAlignment = VerticalAlignment.Center;
                            app.Orientation = Orientation.Horizontal;
                            Image appIcon = new Image();
                            appIcon.VerticalAlignment = VerticalAlignment.Center;
                            appIcon.Width = 20;
                            appIcon.Height = 20;
                            appIcon.BeginInit();
                            object displayIcon = sk.GetValue("DisplayIcon");
                            bool isIconExists = displayIcon != null;
                            if (isIconExists)
                            {
                                string appIconSource = displayIcon.ToString();
                                appIcon.Source = new BitmapImage(new Uri(appIconSource));
                                checkBox.DataContext = appIconSource;
                            }
                            else
                            {
                                appIcon.Source = new BitmapImage(new Uri(@"https://cdn3.iconfinder.com/data/icons/solid-locations-icon-set/64/Games_2-256.png"));
                            }
                            appIcon.EndInit();
                            app.Children.Add(appIcon);
                            TextBlock appNameLabel = new TextBlock();
                            appNameLabel.VerticalAlignment = VerticalAlignment.Center;
                            appNameLabel.Text = displayName;
                            app.Children.Add(appNameLabel);
                            apps.Children.Add(app);
                            Grid.SetRow(app, lastRowIndex);
                            Grid.SetColumn(app, 1);
                            TextBlock appDestinationLabel = new TextBlock();
                            appDestinationLabel.VerticalAlignment = VerticalAlignment.Center;
                            appDestinationLabel.Text = @"";
                            string installPath = "";
                            object rawInstallPath = sk.GetValue("InstallLocation");
                            if (rawInstallPath != null)
                            {
                                installPath = rawInstallPath.ToString();
                            }
                            appDestinationLabel.Text = installPath;
                            apps.Children.Add(appDestinationLabel);
                            Grid.SetRow(appDestinationLabel, lastRowIndex);
                            Grid.SetColumn(appDestinationLabel, 2);
                            checkBox.DataContext = installPath;
                        }
                        catch (Exception ex)
                        { }
                    }
                }
            }
        }

        private void BrowseHandler(object sender, RoutedEventArgs e)
        {
            Browse();
        }

        public void Browse()
        {
            Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
            ofd.Title = "Добавление игры";
            ofd.Filter = "Program files (.exe)|*.exe";
            bool? res = ofd.ShowDialog();
            bool isOpened = res != false;
            if (isOpened)
            {
                string path = ofd.FileName;
                AddGameToLibrary(path);
                Cancel();
            }
        }

        public void AddGameToLibrary(string path)
        {
            Environment.SpecialFolder localApplicationDataFolder = Environment.SpecialFolder.LocalApplicationData;
            string localApplicationDataFolderPath = Environment.GetFolderPath(localApplicationDataFolder);
            string saveDataFilePath = localApplicationDataFolderPath + @"\OfficeWare\GameManager\" + currentUserId + @"\save-data.txt";
            FileInfo info = new FileInfo(path);

            // string gameName = info.Name;
            DirectoryInfo gameInfo = info.Directory;
            string gameName = gameInfo.Name;

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
            List<string> currentCategories = loadedContent.categories;
            List<string> currentRecentChats = loadedContent.recentChats;
            object gameNameLabelData = path;
            string gameUploadedPath = ((string)(gameNameLabelData));
            string gameHours = "0";
            DateTime currentDate = DateTime.Now;
            string gameLastLaunchDate = currentDate.ToLongDateString();
            updatedGames.Add(new Game()
            {
                id = "mockId",
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
                notifications = currentNotifications,
                categories = currentCategories,
                recentChats = currentRecentChats
            });
            File.WriteAllText(saveDataFilePath, savedContent);
            string gamePath = path;
            string gameUploadedLabelContent = Properties.Resources.gameUploadedLabelContent;
            string attentionLabelContent = Properties.Resources.attentionLabelContent;
            MessageBox.Show(gameUploadedLabelContent, attentionLabelContent);
        }

        private void CancelHandler(object sender, RoutedEventArgs e)
        {
            Cancel();
        }

        public void Cancel()
        {
            this.Close();
        }

        public void ReComputeSelectedAppsHandler(object sender, RoutedEventArgs e)
        {
            ReComputeSelectedApps();
        }

        public void ReComputeSelectedApps()
        {
            addSelectedAppsBtn.IsEnabled = false;
            foreach (UIElement appItem in apps.Children)
            {
                bool isCheckBox = appItem is CheckBox;
                if (isCheckBox)
                {
                    CheckBox checkBox = ((CheckBox)(appItem));
                    object rawIsSelected = checkBox.IsChecked;
                    bool isSelected = ((bool)(rawIsSelected));
                    if (isSelected)
                    {
                        addSelectedAppsBtn.IsEnabled = true;
                        break;
                    }
                }
            }
        }

        private void AddSelectedAppsHandler(object sender, RoutedEventArgs e)
        {
            AddSelectedApps();
        }

        public void AddSelectedApps()
        {
            foreach (UIElement appItem in apps.Children)
            {
                bool isCheckBox = appItem is CheckBox;
                if (isCheckBox)
                {
                    CheckBox checkBox = ((CheckBox)(appItem));
                    object rawIsSelected = checkBox.IsChecked;
                    bool isSelected = ((bool)(rawIsSelected));
                    if (isSelected)
                    {
                        object data = checkBox.DataContext;
                        bool isDataExists = data != null;
                        if (isDataExists)
                        {
                            string path = ((string)(data));
                            Debugger.Log(0, "debug", Environment.NewLine + "path: " + path + Environment.NewLine);
                            foreach (string appDirFile in Directory.GetFileSystemEntries(path))
                            {
                                FileInfo info = new FileInfo(appDirFile);
                                string fileName = info.Name;
                                string ext = System.IO.Path.GetExtension(appDirFile);
                                bool isExe = ext == ".exe";
                                Debugger.Log(0, "debug", Environment.NewLine + "appDirFile: " + appDirFile + "; isExe: " + isExe.ToString() + Environment.NewLine);
                                if (isExe)
                                {
                                    string ignoreCaseFileName = fileName.ToLower();
                                    bool isUninstall = ignoreCaseFileName.Contains("unins");
                                    bool isInstall = ignoreCaseFileName.Contains("setup");
                                    bool isRunner = !isInstall && !isUninstall;
                                    Debugger.Log(0, "debug", Environment.NewLine + "appDirFile: " + appDirFile + Environment.NewLine);
                                    if (isRunner)
                                    {
                                        string filePath = info.FullName;
                                        AddGameToLibrary(filePath);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            Cancel();
        }

    }
}
