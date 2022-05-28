using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GamaManager
{
    /// <summary>
    /// Логика взаимодействия для HeaderControl.xaml
    /// </summary>
    public partial class HeaderControl : UserControl
    {

        // public TabControl mainControl;

        public HeaderControl()
        {
            InitializeComponent();
        }

        public void ShowStoreMenuHandler(object sender, MouseEventArgs e)
        {
            StackPanel panel = ((StackPanel)(sender));
            ShowStoreMenu(panel);
        }

        public void ShowStoreMenu(StackPanel panel)
        {
            if (panel.IsMouseOver)
            {
                storeMenuPopup.IsOpen = true;
            }
            else
            {
                storeMenuPopup.IsOpen = false;
            }
        }

        public void ShowNewMenuHandler(object sender, MouseEventArgs e)
        {
            StackPanel panel = ((StackPanel)(sender));
            ShowNewMenu(panel);
        }

        public void ShowNewMenu(StackPanel panel)
        {
            if (panel.IsMouseOver)
            {
                newMenuPopup.IsOpen = true;
            }
            else
            {
                newMenuPopup.IsOpen = false;
            }
        }

        private void OpenLabsHandler(object sender, MouseButtonEventArgs e)
        {
            OpenLabs();
        }

        public void OpenLabs()
        {
            object rawMainControl = this.DataContext;
            MainWindow mainWindow = ((MainWindow)(rawMainControl));
            TabControl mainControl = mainWindow.mainControl;
            mainWindow.GetExperiments();
            mainControl.SelectedIndex = 29;
        }

        private void OpenNewsHandler(object sender, MouseButtonEventArgs e)
        {
            OpenNews();
        }

        public void OpenNews()
        {
            object rawMainControl = this.DataContext;
            MainWindow mainWindow = ((MainWindow)(rawMainControl));
            TabControl mainControl = mainWindow.mainControl;
            mainControl.SelectedIndex = 14;
            mainWindow.GetNews();
            mainWindow.AddHistoryRecord();
        }

        private void OpenPointsStoreHandler(object sender, MouseButtonEventArgs e)
        {
            OpenPointsStore();
        }

        public void OpenPointsStore()
        {
            object rawMainControl = this.DataContext;
            MainWindow mainWindow = ((MainWindow)(rawMainControl));
            TabControl mainControl = mainWindow.mainControl;
            mainControl.SelectedIndex = 31;
            mainWindow.GetPoints();
        }

        private void SelectAccountSettingsItemHandler(object sender, MouseButtonEventArgs e)
        {
            StackPanel item = ((StackPanel)(sender));
            object data = item.DataContext;
            string rawIndex = data.ToString();
            int index = Int32.Parse(rawIndex);
            SelectAccountSettingsItem(index);
        }

        public void SelectAccountSettingsItem(int index)
        {
            object rawMainControl = this.DataContext;
            MainWindow mainWindow = ((MainWindow)(rawMainControl));
            TabControl accountSettingsControl = mainWindow.accountSettingsControl;
            StackPanel accountSettingsTabs = mainWindow.accountSettingsTabs;
            accountSettingsControl.SelectedIndex = index;
            foreach (StackPanel accountSettingsTab in accountSettingsTabs.Children)
            {
                accountSettingsTab.Background = System.Windows.Media.Brushes.Transparent;
            }
            UIElementCollection accountSettingsTabsChildren = accountSettingsTabs.Children;
            UIElement rawActiveAccountSettingsTab = accountSettingsTabsChildren[index];
            StackPanel activeAccountSettingsTab = ((StackPanel)(rawActiveAccountSettingsTab));
            activeAccountSettingsTab.Background = System.Windows.Media.Brushes.LightGray;

            bool isAbout = index == 0;
            bool isLang = index == 2;
            if (isAbout)
            {
                mainWindow.GetAboutAccountSettings();
            }
            else if (isLang)
            {
                mainWindow.GetLangSettings();
            }

        }

        private void SearchGameHandler(object sender, TextChangedEventArgs e)
        {
            TextBox box = ((TextBox)(sender));
            string boxContent = box.Text;
            SearchGame(boxContent);
        }

        private void SearchGame(string boxContent)
        {
            int gameCursor = -1;
            searchGameBoxPopupBody.Children.Clear();
            string keywords = boxContent.ToLower();
            int keywordsLength = keywords.Length;
            bool isFilterEnabled = keywordsLength >= 1;
            if (isFilterEnabled)
            {
                try
                {
                    HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("https://loud-reminiscent-jackrabbit.glitch.me/api/games/get");
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
                                List<GameResponseInfo> totalGames = myobj.games;
                                foreach (GameResponseInfo someGame in totalGames)
                                {
                                    string someGameName = someGame.name;
                                    bool isGameFound = someGameName.Contains(keywords);
                                    if (isGameFound)
                                    {
                                        gameCursor++;
                                        int someGamePrice = someGame.price;
                                        bool isNotFirstGame = gameCursor >= 1;
                                        if (isNotFirstGame)
                                        {
                                            Separator separator = new Separator();
                                            separator.BorderBrush = System.Windows.Media.Brushes.Black;
                                            separator.BorderThickness = new Thickness(1);
                                            separator.Margin = new Thickness(25, 5, 25, 5);
                                            searchGameBoxPopupBody.Children.Add(separator);
                                        }
                                        StackPanel searchedGame = new StackPanel();
                                        searchedGame.Margin = new Thickness(15);
                                        searchedGame.Orientation = Orientation.Horizontal;
                                        Image searchedGameThumbnail = new Image();
                                        searchedGameThumbnail.Width = 75;
                                        searchedGameThumbnail.Height = 75;
                                        searchedGameThumbnail.BeginInit();
                                        searchedGameThumbnail.Source = new BitmapImage(new Uri(@"https://loud-reminiscent-jackrabbit.glitch.me/api/game/thumbnail/?name=" + someGameName));
                                        searchedGameThumbnail.EndInit();
                                        searchedGame.Children.Add(searchedGameThumbnail);
                                        StackPanel searchedGameAside = new StackPanel();
                                        searchedGameAside.Margin = new Thickness(15);
                                        TextBlock someGameNameLabel = new TextBlock();
                                        someGameNameLabel.FontSize = 14;
                                        someGameNameLabel.Text = someGameName;
                                        searchedGameAside.Children.Add(someGameNameLabel);
                                        TextBlock someGamePriceLabel = new TextBlock();
                                        string rawSomeGamePrice = someGamePrice.ToString();
                                        string measure = "Р";
                                        string someGamePriceLabelContent = rawSomeGamePrice + " " + measure;
                                        bool isFreeGame = someGamePrice <= 0;
                                        if (isFreeGame)
                                        {
                                            someGamePriceLabelContent = "Бесплатная";
                                        }
                                        someGamePriceLabel.Text = someGamePriceLabelContent;
                                        searchedGameAside.Children.Add(someGamePriceLabel);
                                        searchedGame.Children.Add(searchedGameAside);
                                        searchGameBoxPopupBody.Children.Add(searchedGame);
                                    }
                                }
                            }
                        }
                    }
                }
                catch (System.Net.WebException)
                {
                    object rawMainControl = this.DataContext;
                    MainWindow mainWindow = ((MainWindow)(rawMainControl));
                    MessageBox.Show("Не удается подключиться к серверу", "Ошибка");
                    mainWindow.Close();
                }
            }
            searchGameBoxPopup.IsOpen = isFilterEnabled;
        }

        public void ShowCategoriesMenuHandler(object sender, MouseEventArgs e)
        {
            StackPanel panel = ((StackPanel)(sender));
            ShowCategoriesMenu(panel);
        }

        public void ShowCategoriesMenu(StackPanel panel)
        {
            if (panel.IsMouseOver)
            {
                categoriesMenuPopup.IsOpen = true;
            }
            else
            {
                categoriesMenuPopup.IsOpen = false;
            }
        }

    }
}
