using System;
using System.Collections.Generic;
using System.IO;
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

namespace GamaManager.Dialogs
{
    /// <summary>
    /// Логика взаимодействия для ScreenShotsUploaderDialog.xaml
    /// </summary>
    public partial class ScreenShotsUploaderDialog : Window
    {

        string currentUserId = "";
        public bool isAppInit = false;

        public ScreenShotsUploaderDialog(string currentUserId)
        {
            InitializeComponent();

            Initialize(currentUserId);
        
        }

        public void Initialize (string currentUserId)
        {
            InitConstants(currentUserId);
            GetScreenShots("");
        }

        public void GetScreenShots (string filter)
        {
            Environment.SpecialFolder localApplicationDataFolder = Environment.SpecialFolder.LocalApplicationData;
            string localApplicationDataFolderPath = Environment.GetFolderPath(localApplicationDataFolder);
            string appPath = localApplicationDataFolderPath + @"\OfficeWare\GameManager\" + currentUserId + @"\screenshots\";
            string[] games = Directory.GetDirectories(appPath);
            screenShots.Children.Clear();
            foreach (string game in games)
            {
                DirectoryInfo gameInfo = new DirectoryInfo(game);
                string gameName = gameInfo.Name;
                ComboBoxItem screenShotsFilterItem = new ComboBoxItem();
                screenShotsFilterItem.Content = gameName;
                screenShotsFilter.Items.Add(screenShotsFilterItem);
                string[] files = Directory.GetFileSystemEntries(game);
                foreach (string file in files)
                {
                    string ext = System.IO.Path.GetExtension(file);
                    bool isScreenShot = ext == ".jpg";
                    if (isScreenShot)
                    {
                        string insensitiveCaseFilter = filter.ToLower();
                        string insensitiveCaseGameName = gameName.ToLower();
                        int filterLength = filter.Length;
                        bool isNotFilter = filterLength <= 0;
                        bool isWordsMatches = insensitiveCaseGameName.Contains(insensitiveCaseFilter);
                        bool isFilterMatches = isWordsMatches || isNotFilter;
                        if (isFilterMatches)
                        {
                            FileInfo info = new FileInfo(file);
                            Image screenShot = new Image();
                            screenShot.Margin = new Thickness(15);
                            screenShot.Width = 85;
                            screenShot.Height = 85;
                            screenShot.BeginInit();
                            Uri screenShotUri = new Uri(file);
                            screenShot.Source = new BitmapImage(screenShotUri);
                            screenShot.EndInit();
                            screenShots.Children.Add(screenShot);
                            // screenShot.DataContext = info.Name;
                            screenShot.DataContext = file;
                            screenShot.MouseLeftButtonUp += SelectScreenShotHandler;
                        }
                    }
                }
            }
        }

        public void SelectScreenShotHandler (object sender, RoutedEventArgs e)
        {
            Image screenShot = ((Image)(sender));
            object data = screenShot.DataContext;
            string name = ((string)(data));
            SelectScreenShot(name);
        }

        public void SelectScreenShot (string name)
        {
            screenShotsControl.SelectedIndex = 1;
            actionBtns.Visibility = Visibility.Visible;
            mainScreenShot.Source = new BitmapImage(new Uri(name));
        }

        public void InitConstants (string currentUserId)
        {
            this.currentUserId = currentUserId;
        }

        private void SelectScreenShotsFilterHandler (object sender, SelectionChangedEventArgs e)
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
                    GetScreenShots("");
                }
                else
                {
                    object rawSelectedItem = screenShotsFilter.Items[selectedIndex];
                    ComboBoxItem selectedItem = ((ComboBoxItem)(rawSelectedItem));
                    object rawFilter = selectedItem.Content;
                    string filter = rawFilter.ToString();
                    GetScreenShots(filter);
                }
            }
        }

        private void AppLoadedHandler (object sender, RoutedEventArgs e)
        {
            AppLoaded();
        }

        public void AppLoaded ()
        {
            isAppInit = true;
        }


    }
}
