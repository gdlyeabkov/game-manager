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
using System.Windows.Shapes;

namespace GamaManager.Dialogs
{
    /// <summary>
    /// Логика взаимодействия для FriendNotificationsDialog.xaml
    /// </summary>
    public partial class FriendNotificationsDialog : Window
    {

        public string currentUserId = "";
        public string currentFriendId = "";

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
            Environment.SpecialFolder localApplicationDataFolder = Environment.SpecialFolder.LocalApplicationData;
            string localApplicationDataFolderPath = Environment.GetFolderPath(localApplicationDataFolder);
            string saveDataFilePath = localApplicationDataFolderPath + @"\OfficeWare\GameManager\" + currentUserId + @"\save-data.txt";
            JavaScriptSerializer js = new JavaScriptSerializer();
            string saveDataFileContent = File.ReadAllText(saveDataFilePath);
            SavedContent loadedContent = js.Deserialize<SavedContent>(saveDataFileContent);
            List<Game> currentGames = loadedContent.games;
            List<FriendSettings> updatedFriends = loadedContent.friends;
            Settings currentSettings = loadedContent.settings;
            List<string> currentCollections = loadedContent.collections;
            Notifications currentNotifications = loadedContent.notifications;
            List<string> currentCategories = loadedContent.categories;
            List<string> currentRecentChats = loadedContent.recentChats;
            Recommendations currentRecommendations = loadedContent.recommendations;
            List<FriendSettings> cachedFriends = updatedFriends.Where<FriendSettings>((FriendSettings friend) =>
            {
                return friend.id == currentFriendId;
            }).ToList();
            int countCachedFriends = cachedFriends.Count;
            bool isCachedFriendsExists = countCachedFriends >= 1;
            if (isCachedFriendsExists)
            {
                FriendSettings cachedFriend = cachedFriends[0];
                object rawCheckBoxState = onlineNotificationCheckBox.IsChecked;
                bool checkBoxState = ((bool)(rawCheckBoxState));
                cachedFriend.isFriendOnlineNotification = checkBoxState;
                rawCheckBoxState = onlineSoundCheckBox.IsChecked;
                checkBoxState = ((bool)(rawCheckBoxState));
                cachedFriend.isFriendOnlineSound = checkBoxState;
                rawCheckBoxState = playedNotificationCheckBox.IsChecked;
                checkBoxState = ((bool)(rawCheckBoxState));
                cachedFriend.isFriendPlayedNotification = checkBoxState;
                rawCheckBoxState = playedSoundCheckBox.IsChecked;
                checkBoxState = ((bool)(rawCheckBoxState));
                cachedFriend.isFriendPlayedSound = checkBoxState;
                rawCheckBoxState = sendMsgNotificationCheckBox.IsChecked;
                checkBoxState = ((bool)(rawCheckBoxState));
                cachedFriend.isFriendSendMsgNotification = checkBoxState;
                rawCheckBoxState = sendMsgSoundCheckBox.IsChecked;
                checkBoxState = ((bool)(rawCheckBoxState));
                cachedFriend.isFriendSendMsgSound = checkBoxState;
                string savedContent = js.Serialize(new SavedContent
                {
                    games = currentGames,
                    friends = updatedFriends,
                    settings = currentSettings,
                    collections = currentCollections,
                    notifications = currentNotifications,
                    categories = currentCategories,
                    recentChats = currentRecentChats,
                    recommendations = currentRecommendations
                });
                File.WriteAllText(saveDataFilePath, savedContent);
                MessageBox.Show("Уведомления для друга были обновлены", "Внимание");
                CloseDialog();
            }
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
            currentFriendId = friendId;
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/users/get/?id=" + friendId);
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse innerWebResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (StreamReader innerReader = new StreamReader(innerWebResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        string objText = innerReader.ReadToEnd();

                        UserResponseInfo myobj = (UserResponseInfo)js.Deserialize(objText, typeof(UserResponseInfo));

                        string status = myobj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
                            
                            User friend = myobj.user;
                            string friendName = friend.name;
                            friendNameLabel.Text = friendName;

                            Environment.SpecialFolder localApplicationDataFolder = Environment.SpecialFolder.LocalApplicationData;
                            string localApplicationDataFolderPath = Environment.GetFolderPath(localApplicationDataFolder);
                            string saveDataFilePath = localApplicationDataFolderPath + @"\OfficeWare\GameManager\" + currentUserId + @"\save-data.txt";

                            js = new JavaScriptSerializer();
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
                                bool isOnlineNotification = currentFriend.isFriendOnlineNotification;
                                bool isOnlineSound = currentFriend.isFriendOnlineSound;
                                bool isPlayedNotification = currentFriend.isFriendPlayedNotification;
                                bool isPlayedSound = currentFriend.isFriendPlayedSound;
                                bool isSendMsgNotification = currentFriend.isFriendSendMsgNotification;
                                bool isSendMsgSound = currentFriend.isFriendSendMsgSound;
                                onlineNotificationCheckBox.IsChecked = isOnlineNotification;
                                onlineSoundCheckBox.IsChecked = isOnlineSound;
                                playedNotificationCheckBox.IsChecked = isPlayedNotification;
                                playedSoundCheckBox.IsChecked = isPlayedSound;
                                sendMsgNotificationCheckBox.IsChecked = isSendMsgNotification;
                                sendMsgSoundCheckBox.IsChecked = isSendMsgSound;
                            }
                        }
                    }
                }
            }
            catch (System.Net.WebException)
            {
                MessageBox.Show("Не удается подключиться к серверу", "Ошибка");
            }
        }

        private void CloseHandler (object sender, RoutedEventArgs e)
        {
            CloseDialog();
        }

        public void CloseDialog () {
            this.Close();
        }

    }

}
