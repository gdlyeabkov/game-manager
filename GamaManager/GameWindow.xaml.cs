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

        public GameWindow(string userId)
        {
            InitializeComponent();

            Initialize(userId);

        }

        public void Initialize (string userId)
        {
            currentUserId = userId;
            GetUser(userId);
        }

        public void GetUser(string userId)
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
                string gameName = System.IO.Path.GetDirectoryName(gameFolder);
                control.DataContext = gameName;
                control.Loaded += GameLoadedHandler;
                control.Unloaded += GameUnloadedHandler;
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
            var shiftModifier = Keyboard.Modifiers & ModifierKeys.Shift;
            bool isShiftModifierEnabled = shiftModifier > 0;
            Key currentKey = e.Key;
            Key tabKey = Key.Tab;
            bool isTabKey = currentKey == tabKey;
            bool isToggleAside = isTabKey && isShiftModifierEnabled;
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
        }

        private void game_LayoutUpdated(object sender, EventArgs e)
        {
            if (isAppInit)
            {
                // this.Close();
            }
        }

        public void CloseGame()
        {
            this.Close();
        }

    }
}
