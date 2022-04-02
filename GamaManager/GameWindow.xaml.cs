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

        public GameWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            debugger = new SpeechSynthesizer();
            visible = Visibility.Visible;
            invisible = Visibility.Collapsed;
            object gameData = this.DataContext;
            string gamePath = ((string)(gameData));
            GameIntegrationManager control = new GameIntegrationManager(this, gamePath);
            game.Children.Add(control);
            DockPanel.SetDock(control, Dock.Top);
            control.Loaded += GameLoadedHandler;
            control.Unloaded += GameUnloadedHandler;
        }

        public void GameUnloadedHandler (object sender, EventArgs e)
        {
            this.Close();
            debugger.Speak("закрываю");
        }

        public void GameLoadedHandler(object sender, EventArgs e)
        {
            isAppInit = false;
        }

        private void GlobalHotKeyHandler(object sender, KeyEventArgs e)
        {
            // debugger.Speak("Открываю вспомогательное меню");
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
                this.Close();
                debugger.Speak("Закрылась");
            }
        }

        public void CloseGame()
        {
            this.Close();
            debugger.Speak("Закрылась");
        }

    }
}
