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

        public GameWindow(string userId)
        {
            InitializeComponent();

            Initialize(userId);

        }

        public void Initialize (string userId)
        {
            InitConstants(userId);
            GetUser(userId);
        }

        public void InitConstants (string userId)
        {
            currentUserId = userId;
            notificationBackground = System.Windows.Media.Brushes.AliceBlue;
        }

        public void GetUser(string userId)
        {
            currentUserId = userId;
            System.Diagnostics.Debugger.Log(0, "debug", "userId: " + userId + Environment.NewLine);
            HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("https://loud-reminiscent-jackrabbit.glitch.me/api/users/get/?id=" + userId);
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

        public void CloseManager()
        {
            MessageBox.Show("Не удалось подключиться", "Ошибка");
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            debugger = new SpeechSynthesizer();
            visible = Visibility.Visible;
            invisible = Visibility.Collapsed;
            object gameData = this.DataContext;
            string gamePath = ((string)(gameData));
            try
            {
                GameIntegrationManager control = new GameIntegrationManager(this, gamePath);
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

        public void GameLoadedHandler(object sender, EventArgs e)
        {
            isAppInit = true;
            GameIntegrationManager manager = ((GameIntegrationManager)(sender));
            object managerData = manager.DataContext;
            string gameName = ((string)(managerData));
            gameNameLabel.Text = gameName;
        }

        private void GlobalHotKeyHandler (object sender, KeyEventArgs e)
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

            Key currentKey = e.Key;
            string rawCurrentKey = currentKey.ToString();
            
            string overlayKey = rawCurrentKey;
            
            string screenShotsKey = rawCurrentKey;
            
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
            
            bool isOverlayKey = rawCurrentKey == overlayKey;
            bool isToggleAside = isOverlayKey && ((isShiftModifierEnabled && isNeedShiftModifier) || !isNeedShiftModifier) && ((isCtrlModifierEnabled && isNeedCtrlModifier) || !isNeedCtrlModifier);
            /*Key sKey = Key.S;
            bool isSKey = currentKey == sKey;
            bool isTakeScreenShot = isShiftModifierEnabled && isSKey;*/

            bool isScreenShotsKey = rawCurrentKey == screenShotsKey;
            bool isTakeScreenShot = isScreenShotsKey && ((isShiftModifierEnabled && isScreenShotsNeedShiftModifier) || !isScreenShotsNeedShiftModifier) && ((isCtrlModifierEnabled && isScreenShotsNeedCtrlModifier) || !isScreenShotsNeedCtrlModifier);
            // bool isTakeScreenShot = isShiftModifierEnabled && isSKey;

            if (isToggleAside)
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
            else if (isTakeScreenShot)
            {
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
                    string cachePath = appPath + currentGameName;
                    DateTimeOffset currentDateTime = DateTimeOffset.UtcNow;
                    long unixTimeMilliseconds = currentDateTime.ToUnixTimeMilliseconds();
                    string rawMillis = unixTimeMilliseconds.ToString();
                    string generatedScreenShotName = rawMillis + @".jpg";
                    string bitmapPath = cachePath + @"\" + generatedScreenShotName;
                    bitmap.Save(bitmapPath);
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        audio.LoadedBehavior = MediaState.Play;
                        audio.Source = new Uri(@"C:\wpf_projects\GamaManager\GamaManager\Sounds\screenshot.wav");
                    });

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
                catch (Exception)
                {

                }
            }
        }

        private void game_LayoutUpdated(object sender, EventArgs e)
        {
            if (isAppInit)
            {
                
            }
        }

        public void CloseGame()
        {
            this.Close();
        }

        public CustomPopupPlacement[] NotificationPlacementHandler (System.Windows.Size popupSize, System.Windows.Size targetSize, System.Windows.Point offset)
        {
            return new CustomPopupPlacement[]
            {
                new CustomPopupPlacement(new System.Windows.Point(-50, 100), PopupPrimaryAxis.Vertical),
                new CustomPopupPlacement(new System.Windows.Point(10, 20), PopupPrimaryAxis.Horizontal)
            };
        }

    }
}
