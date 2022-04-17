using System;
using System.Collections.Generic;
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
    /// Логика взаимодействия для FriendNotificationsDialog.xaml
    /// </summary>
    public partial class FriendNotificationsDialog : Window
    {

        public string currentUserId = "";

        public FriendNotificationsDialog (string currentUserId)
        {
            InitializeComponent();
            this.currentUserId = currentUserId;
        }

        private void SaveFriendNotificationsHandler (object sender, RoutedEventArgs e)
        {
            SaveFriendNotifications();
        }

        public void SaveFriendNotifications()
        {

        }

        private void LoadFriendSettingsHandler (object sender, RoutedEventArgs e)
        {
            Dialogs.FriendNotificationsDialog dialog = ((Dialogs.FriendNotificationsDialog)(sender));
            object dialogData = dialog.DataContext;
            string friendId = ((string)(dialogData));
            LoadFriendSettings(friendId);
        }

        private void LoadFriendSettings (string friendId)
        {
            Environment.SpecialFolder localApplicationDataFolder = Environment.SpecialFolder.LocalApplicationData;
            string localApplicationDataFolderPath = Environment.GetFolderPath(localApplicationDataFolder);
            string saveDataFilePath = localApplicationDataFolderPath + @"\OfficeWare\GameManager\" + currentUserId + @"\save-data.txt";

            JavaScriptSerializer js = new JavaScriptSerializer();
            string saveDataFileContent = File.ReadAllText(saveDataFilePath);
            SavedContent loadedContent = js.Deserialize<SavedContent>(saveDataFileContent);
            List<FriendSettings> loadedFriends = loadedContent.friends;
            FriendSettings currentFriend = null;
            foreach (FriendSettings loadedFriend in loadedFriends)
            {
                string localFriendId = loadedFriend.id;
                bool isCurrentFriend = localFriendId == friendId;
                if (isCurrentFriend)
                {
                    currentFriend = loadedFriend;
                }
            }
            bool isFriendExists = currentFriend != null;
            if (isFriendExists)
            {
                bool isOnline = currentFriend.isFriendOnlineNotification;
                bool isPlayed = currentFriend.isFriendPlayedNotification;
                bool isSendMsg = currentFriend.isFriendSendMsgNotification;
                onlineNotificationCheckBox.IsChecked = isOnline;
                playedNotificationCheckBox.IsChecked = isPlayed;
                sendMsgNotificationCheckBox.IsChecked = isSendMsg;

            }
        }

    }
}
