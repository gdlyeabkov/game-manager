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
using System.Windows.Shapes;

using System.Speech.Synthesis;
using System.Net;
using System.IO;
using System.Web.Script.Serialization;
using System.Windows.Interop;
using System.Drawing;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using System.Diagnostics;
using System.Threading;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using MouseKeyboardActivityMonitor.WinApi;
using MouseKeyboardActivityMonitor;
using System.Management;

namespace GamaManager
{
    /// <summary>
    /// Логика взаимодействия для GameWindow.xaml
    /// </summary>
    public partial class GameWindow : Window
    {

        public SpeechSynthesizer debugger;
        public Visibility visible;
        public Visibility invisible;
        public bool isAppInit = false;
        public string currentUserId = "";
        private User currentUser;
        public string currentGameName = "";
        public System.Windows.Media.Brush notificationBackground;
        bool isOverlayEnabled = true;
        public GlobalHooker globalHooker;
        public KeyboardHookListener keyboardHookListener;
        private GameIntegrationManager control;

        public GameWindow(string userId)
        {
            InitializeComponent();

            Initialize(userId);

        }

        public void Initialize(string userId)
        {
            InitConstants(userId);
            GetUser(userId);

            globalHooker = new GlobalHooker();
            keyboardHookListener = new KeyboardHookListener(globalHooker);
            keyboardHookListener.Enabled = true;
            keyboardHookListener.KeyPress += KeyboardHookListenerOnKeyUp;

        }

        public void KeyboardHookListenerOnKeyUp (object sender, System.Windows.Forms.KeyPressEventArgs e)
        {
            Environment.SpecialFolder localApplicationDataFolder = Environment.SpecialFolder.LocalApplicationData;
            string localApplicationDataFolderPath = Environment.GetFolderPath(localApplicationDataFolder);
            string saveDataFilePath = localApplicationDataFolderPath + @"\OfficeWare\GameManager\" + currentUserId + @"\save-data.txt";
            JavaScriptSerializer js = new JavaScriptSerializer();
            string saveDataFileContent = File.ReadAllText(saveDataFilePath);
            SavedContent loadedContent = js.Deserialize<SavedContent>(saveDataFileContent);
            Settings currentSettings = loadedContent.settings;
            string overlayHotKey = currentSettings.overlayHotKey;
            string screenShotsHotKey = currentSettings.screenShotsHotKey;
            bool isModifiersApplied = overlayHotKey.Contains("+");
            bool isNeedShiftModifier = false;
            bool isNeedCtrlModifier = false;
            bool isScreenShotsNeedShiftModifier = false;
            bool isScreenShotsNeedCtrlModifier = false;
            char rawCurrentKey = e.KeyChar;
            string overlayKey = rawCurrentKey.ToString();
            string screenShotsKey = rawCurrentKey.ToString();
            if (isModifiersApplied)
            {
                string[] overlayHotKeyParts = overlayHotKey.Split(new Char[] { '+' });
                string overlayModifier = overlayHotKeyParts[0];
                overlayModifier = overlayModifier.Trim();
                isNeedShiftModifier = overlayModifier == "Shift";
                isNeedCtrlModifier = overlayModifier == "Ctrl";
                overlayKey = overlayHotKeyParts[1];
                overlayKey = overlayKey.Trim();


                string[] screenShotsHotKeyParts = screenShotsHotKey.Split(new Char[] { '+' });
                string screenShotsModifier = screenShotsHotKeyParts[0];
                screenShotsModifier = screenShotsModifier.Trim();
                isScreenShotsNeedShiftModifier = screenShotsModifier == "Shift";
                isScreenShotsNeedCtrlModifier = screenShotsModifier == "Ctrl";
                screenShotsKey = screenShotsHotKeyParts[1];
                screenShotsKey = screenShotsKey.Trim();

            }
            else
            {
                overlayKey = overlayHotKey;

                screenShotsKey = screenShotsHotKey;
            }
            var shiftModifier = Keyboard.Modifiers & ModifierKeys.Shift;
            bool isShiftModifierEnabled = shiftModifier > 0;
            var ctrlModifier = Keyboard.Modifiers & ModifierKeys.Control;
            bool isCtrlModifierEnabled = ctrlModifier > 0;
            bool isOverlayKey = rawCurrentKey.ToString() == overlayKey;
            bool isToggleAside = isOverlayKey && ((isShiftModifierEnabled && isNeedShiftModifier) || !isNeedShiftModifier) && ((isCtrlModifierEnabled && isNeedCtrlModifier) || !isNeedCtrlModifier);
            bool isScreenShotsKey = rawCurrentKey.ToString() == screenShotsKey;
            bool isTakeScreenShot = isScreenShotsKey && ((isShiftModifierEnabled && isScreenShotsNeedShiftModifier) || !isScreenShotsNeedShiftModifier) && ((isCtrlModifierEnabled && isScreenShotsNeedCtrlModifier) || !isScreenShotsNeedCtrlModifier);
            if (isToggleAside)
            {
                bool isShowOverlay = currentSettings.showOverlay;
                bool isCanToggleAside = isShowOverlay && isOverlayEnabled;
                if (isCanToggleAside)
                {
                    Visibility currentVisibility = gameManagerAside.Visibility;
                    bool isVisible = currentVisibility == visible;
                    if (isVisible)
                    {
                        gameManagerAside.Visibility = invisible;
                    }
                    else
                    {
                        gameManagerAside.Visibility = visible;
                    }
                }
            }
            else if (isTakeScreenShot)
            {

                bool isShowScreenShotsNotification = currentSettings.showScreenShotsNotification;
                bool isPlayScreenShotsNotification = currentSettings.playScreenShotsNotification;
                Bitmap bitmap = new Bitmap((int)SystemParameters.PrimaryScreenWidth, (int)SystemParameters.PrimaryScreenHeight, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    g.CopyFromScreen(0, 0, 0, 0, bitmap.Size);
                }
                IntPtr handle = IntPtr.Zero;
                try
                {
                    handle = bitmap.GetHbitmap();
                    System.Windows.Controls.Image img = new System.Windows.Controls.Image();
                    img.Source = Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
                    localApplicationDataFolder = Environment.SpecialFolder.LocalApplicationData;
                    localApplicationDataFolderPath = Environment.GetFolderPath(localApplicationDataFolder);
                    string appPath = localApplicationDataFolderPath + @"\OfficeWare\GameManager\" + currentUserId + @"\";
                    // string cachePath = appPath + currentGameName;
                    string cachePath = appPath + @"games\" + currentGameName;
                    DateTimeOffset currentDateTime = DateTimeOffset.UtcNow;
                    long unixTimeMilliseconds = currentDateTime.ToUnixTimeMilliseconds();
                    string rawMillis = unixTimeMilliseconds.ToString();
                    string generatedScreenShotName = rawMillis + @".jpg";
                    string bitmapPath = appPath + @"screenshots\" + currentGameName + @"\" + generatedScreenShotName;
                    bitmap.Save(bitmapPath);
                    if (isPlayScreenShotsNotification)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            audio.LoadedBehavior = MediaState.Play;
                            audio.Source = new Uri(@"C:\wpf_projects\GamaManager\GamaManager\Sounds\screenshot.wav");
                        });
                    }
                    if (isShowScreenShotsNotification)
                    {
                        Popup notification = new Popup();
                        notification.Placement = PlacementMode.Custom;
                        notification.CustomPopupPlacementCallback = new CustomPopupPlacementCallback(NotificationPlacementHandler);
                        notification.PlacementTarget = this;
                        notification.Width = 225;
                        notification.Height = 275;
                        System.Windows.Controls.Image notificationBodySenderAvatar = new System.Windows.Controls.Image();
                        notificationBodySenderAvatar.Width = 100;
                        notificationBodySenderAvatar.Height = 100;
                        notificationBodySenderAvatar.BeginInit();
                        Uri notificationBodySenderAvatarUri = new Uri(bitmapPath);
                        BitmapImage notificationBodySenderAvatarImg = new BitmapImage(notificationBodySenderAvatarUri);
                        notificationBodySenderAvatar.Source = notificationBodySenderAvatarImg;
                        notificationBodySenderAvatar.EndInit();
                        notification.Child = notificationBodySenderAvatar;
                        notifications.Children.Add(notification);
                        notification.IsOpen = true;
                        notification.StaysOpen = false;
                        notification.PopupAnimation = PopupAnimation.Fade;
                        notification.AllowsTransparency = true;
                        DispatcherTimer timer = new DispatcherTimer();
                        timer.Interval = TimeSpan.FromSeconds(3);
                        timer.Tick += delegate
                        {
                            notification.IsOpen = false;
                            timer.Stop();
                        };
                        timer.Start();
                        notifications.Children.Add(notification);
                    }
                }
                catch (Exception)
                {

                }
            }

        }

        public void InitConstants(string userId)
        {
            currentUserId = userId;
            notificationBackground = System.Windows.Media.Brushes.AliceBlue;
        }

        public void GetUser (string userId)
        {
            currentUserId = userId;
            System.Diagnostics.Debugger.Log(0, "debug", "userId: " + userId + Environment.NewLine);
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
                            string userLogin = currentUser.login;
                            userLoginLabel.Text = userLogin;
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

        private void Window_Loaded (object sender, RoutedEventArgs e)
        {
            debugger = new SpeechSynthesizer();
            visible = Visibility.Visible;
            invisible = Visibility.Collapsed;
            object gameData = this.DataContext;
            string gamePath = ((string)(gameData));
            try
            {
                control = new GameIntegrationManager(this, gamePath);
                game.Children.Add(control);
                DockPanel.SetDock(control, Dock.Top);
                FileInfo gameFileInfo = new FileInfo(gamePath);
                string gameFolder = gameFileInfo.DirectoryName;
                DirectoryInfo dirInfo = new DirectoryInfo(gameFolder);
                string gameName = dirInfo.Name;
                control.DataContext = gameName;
                control.Loaded += GameLoadedHandler;
                control.Unloaded += GameUnloadedHandler;

                currentGameName = gameName;

                Environment.SpecialFolder localApplicationDataFolder = Environment.SpecialFolder.LocalApplicationData;
                string localApplicationDataFolderPath = Environment.GetFolderPath(localApplicationDataFolder);
                string saveDataFilePath = localApplicationDataFolderPath + @"\OfficeWare\GameManager\" + currentUserId + @"\save-data.txt";
                JavaScriptSerializer js = new JavaScriptSerializer();
                string saveDataFileContent = File.ReadAllText(saveDataFilePath);
                SavedContent loadedContent = js.Deserialize<SavedContent>(saveDataFileContent);
                List<Game> currentGames = loadedContent.games;
                List<Game> results = currentGames.Where<Game>((Game game) =>
                {
                    string localGameName = game.name;
                    bool isFound = localGameName == currentGameName;
                    return isFound;
                }).ToList();
                int countResults = results.Count;
                bool isHaveResults = countResults >= 1;
                if (isHaveResults)
                {
                    Game result = results[0];
                    isOverlayEnabled = result.overlay;
                }

            }
            catch (Exception)
            {
                // при неудачном открытии игры
                this.Close();
            }
        }

        public void GameUnloadedHandler (object sender, EventArgs e)
        {
            this.Close();
        }

        public void GameLoadedHandler (object sender, EventArgs e)
        {
            isAppInit = true;
            GameIntegrationManager manager = ((GameIntegrationManager)(sender));
            object managerData = manager.DataContext;
            string gameName = ((string)(managerData));
            gameNameLabel.Text = gameName;

            UpdateFps();

        }

        public void UpdateFps ()
        {
            Environment.SpecialFolder localApplicationDataFolder = Environment.SpecialFolder.LocalApplicationData;
            string localApplicationDataFolderPath = Environment.GetFolderPath(localApplicationDataFolder);
            string saveDataFilePath = localApplicationDataFolderPath + @"\OfficeWare\GameManager\" + currentUserId + @"\save-data.txt";
            JavaScriptSerializer js = new JavaScriptSerializer();
            string saveDataFileContent = File.ReadAllText(saveDataFilePath);
            SavedContent loadedContent = js.Deserialize<SavedContent>(saveDataFileContent);
            Settings currentSettings = loadedContent.settings;
            string frames = currentSettings.frames;
            bool isShowFps = frames != "Disabled";
            if (isShowFps)
            {
                var frameRates = new ObservableCollection<Helpers.FrameRate>();
                var calculator = new Helpers.FrameRateCalculator();
                calculator.Start();
                var frameSubscription = calculator.Observable
                    .ObserveOn(SynchronizationContext.Current)
                    .Subscribe(fr =>
                    {
                        frameRates.Add(fr);
                        string rawFps = fr.Frames.ToString();
                        fpsLabel.Text = rawFps;
                    });
            }
            else
            {
                fpsLabel.Visibility = invisible;
            }

        }

        private void game_LayoutUpdated(object sender, EventArgs e)
        {
            if (isAppInit)
            {

            }
        }

        public void CloseGame ()
        {
            this.Close();
        }

        public CustomPopupPlacement[] NotificationPlacementHandler(System.Windows.Size popupSize, System.Windows.Size targetSize, System.Windows.Point offset)
        {
            return new CustomPopupPlacement[]
            {
                new CustomPopupPlacement(new System.Windows.Point(-50, 100), PopupPrimaryAxis.Vertical),
                new CustomPopupPlacement(new System.Windows.Point(10, 20), PopupPrimaryAxis.Horizontal)
            };
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            control._process.Kill();
            int pId = this.control._process.Id;
            Debugger.Log(0, "debug", Environment.NewLine + "закрываю игру 2 " + pId.ToString() + Environment.NewLine);
            // KillProcessAndChildren(pId);
        }

        private static void KillProcessAndChildren(int pid)
        {
            // Cannot close 'system idle process'.
            if (pid == 0)
            {
                return;
            }
            ManagementObjectSearcher searcher = new ManagementObjectSearcher
                    ("Select * From Win32_Process Where ParentProcessID=" + pid);
            ManagementObjectCollection moc = searcher.Get();
            foreach (ManagementObject mo in moc)
            {
                KillProcessAndChildren(Convert.ToInt32(mo["ProcessID"]));
            }
            try
            {
                Process proc = Process.GetProcessById(pid);
                proc.Kill();
            }
            catch (ArgumentException)
            {
                // Process already exited.
            }
        }

    }
}
