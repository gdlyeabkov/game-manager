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

namespace GamaManager
{
    /// <summary>
    /// Логика взаимодействия для FriendActivityAsideControl.xaml
    /// </summary>
    public partial class FriendActivityAsideControl : UserControl
    {
        public FriendActivityAsideControl()
        {
            InitializeComponent();
        }

        private void SetDefaultAvatarHandler (object sender, ExceptionRoutedEventArgs e)
        {
            Image avatar = ((Image)(sender));
            SetDefaultAvatar(avatar);
        }

        public void SetDefaultAvatar (Image avatar)
        {
            avatar.BeginInit();
            avatar.Source = new BitmapImage(new Uri(@"https://cdn2.iconfinder.com/data/icons/ios-7-icons/50/user_male-128.png"));
            avatar.EndInit();
        }

        public void ReturnToProfileHandler (object sender, MouseButtonEventArgs e)
        {
            object rawMainControl = this.DataContext;
            MainWindow mainWindow = ((MainWindow)(rawMainControl));
            mainWindow.ReturnToProfile();
        }

        public void OpenTradeOffersHandler (object sender, RoutedEventArgs e)
        {
            object rawMainControl = this.DataContext;
            MainWindow mainWindow = ((MainWindow)(rawMainControl));
            mainWindow.OpenTradeOffers();
        }

        public void OpenCommentsHistoryHandler (object sender, RoutedEventArgs e)
        {
            object rawMainControl = this.DataContext;
            MainWindow mainWindow = ((MainWindow)(rawMainControl));
            mainWindow.OpenCommentsHistory();
        }

        public void OpenAllEventsHandler (object sender, RoutedEventArgs e)
        {
            object rawMainControl = this.DataContext;
            MainWindow mainWindow = ((MainWindow)(rawMainControl));
            mainWindow.OpenAllEvents();
        }

        public void OpenContentTabHandler(object sender, RoutedEventArgs e)
        {
            StackPanel activityShortCut = ((StackPanel)(sender));
            object activityShortCutData = activityShortCut.DataContext;
            string rawIndex = activityShortCutData.ToString();
            int index = Int32.Parse(rawIndex);
            object rawMainControl = this.DataContext;
            MainWindow mainWindow = ((MainWindow)(rawMainControl));
            mainWindow.OpenContentTab(index);
        }

        public void OpenEquipmentHandler(object sender, RoutedEventArgs e)
        {
            object rawMainControl = this.DataContext;
            MainWindow mainWindow = ((MainWindow)(rawMainControl));
            string currentUserId = mainWindow.currentUserId;
            mainWindow.OpenEquipment(currentUserId);
        }

        public void OpenFriendsSettingsHandler(object sender, RoutedEventArgs e)
        {
            object rawMainControl = this.DataContext;
            MainWindow mainWindow = ((MainWindow)(rawMainControl));
            mainWindow.OpenFriendsSettings();
        }

        public void OpenGroupsSettingsHandler(object sender, RoutedEventArgs e)
        {
            object rawMainControl = this.DataContext;
            MainWindow mainWindow = ((MainWindow)(rawMainControl));
            mainWindow.OpenGroupsSettings();
        }

        private void OpenEditProfileHandler(object sender, RoutedEventArgs e)
        {
            object rawMainControl = this.DataContext;
            MainWindow mainWindow = ((MainWindow)(rawMainControl));
            mainWindow.OpenEditProfile();
        }

        private void OpenGameRecommendationsHandler(object sender, MouseButtonEventArgs e)
        {
            object rawMainControl = this.DataContext;
            MainWindow mainWindow = ((MainWindow)(rawMainControl));
            mainWindow.OpenGameRecommendations();
        }

        private void DetectFriendSearchHandler(object sender, KeyEventArgs e)
        {
            TextBox box = ((TextBox)(sender));
            Key key = e.Key;
            object rawMainControl = this.DataContext;
            MainWindow mainWindow = ((MainWindow)(rawMainControl));
            mainWindow.DetectFriendSearch(key, box);
        }

    }
}
