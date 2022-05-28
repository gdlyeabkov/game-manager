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
    /// Логика взаимодействия для CategoryManagementDialog.xaml
    /// </summary>
    public partial class CategoryManagementDialog : Window
    {

        public string currentUserId = "";
        public string friendId = "";
        
        public CategoryManagementDialog(string currentUserId, string friendId)
        {
            InitializeComponent();

            Initialize(currentUserId, friendId);

        }

        public void Initialize(string currentUserId, string friendId)
        {
            this.currentUserId = currentUserId;
            this.friendId = friendId;
            GetFriendInfo();
            GetCategories();
        }

        public void GetFriendInfo()
        {
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("https://loud-reminiscent-jackrabbit.glitch.me/api/users/get/?id=" + friendId);
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
                            User friend = myobj.user;
                            userProfileAvatar.BeginInit();
                            userProfileAvatar.Source = new BitmapImage(new Uri(@"https://loud-reminiscent-jackrabbit.glitch.me/api/user/avatar/?id=" + friendId));
                            userProfileAvatar.EndInit();
                            string friendName = friend.name;
                            userProfileNameLabel.Text = friendName;
                            string friendStatus = friend.status;
                            userProfileStatusLabel.Text = friendStatus;
                        }
                    }
                }
            }
            catch (System.Net.WebException)
            {
                MessageBox.Show("Не удается подключиться к серверу", "Ошибка");
                this.Close();
            }
        }

        public void GetCategories ()
        {
            categories.Children.Clear();
            Environment.SpecialFolder localApplicationDataFolder = Environment.SpecialFolder.LocalApplicationData;
            string localApplicationDataFolderPath = Environment.GetFolderPath(localApplicationDataFolder);
            string saveDataFilePath = localApplicationDataFolderPath + @"\OfficeWare\GameManager\" + currentUserId + @"\save-data.txt";
            JavaScriptSerializer js = new JavaScriptSerializer();
            string saveDataFileContent = File.ReadAllText(saveDataFilePath);
            SavedContent loadedContent = js.Deserialize<SavedContent>(saveDataFileContent);
            List<FriendSettings> currentFriends = loadedContent.friends;
            List<string> currentCategories = loadedContent.categories;
            foreach (string currentCategory in currentCategories)
            {
                CheckBox checkBox = new CheckBox();
                checkBox.Content = currentCategory;
                checkBox.DataContext = currentCategory;
                checkBox.Margin = new Thickness(15);
                categories.Children.Add(checkBox);
                int foundIndex = currentFriends.FindIndex((FriendSettings friend) =>
                {
                    string localFriendId = friend.id;
                    bool isFriendFound = localFriendId == friendId;
                    return isFriendFound;
                });
                bool isFound = foundIndex >= 0;
                if (isFound)
                {
                    FriendSettings foundedFriend = currentFriends[foundIndex];
                    List<string> foundedFriendCategories = foundedFriend.categories;
                    bool isHaveCategory = foundedFriendCategories.Contains(currentCategory);
                    checkBox.IsChecked = isHaveCategory;
                }
                ContextMenu checkBoxContextMenu = new ContextMenu();
                MenuItem checkBoxContextMenuItem = new MenuItem();
                checkBoxContextMenuItem.Header = "Настроить категорию";
                checkBoxContextMenuItem.DataContext = currentCategory;
                checkBoxContextMenuItem.Click += OpenCategorySettingsHandler;
                checkBoxContextMenu.Items.Add(checkBoxContextMenuItem);
                checkBoxContextMenuItem = new MenuItem();
                checkBoxContextMenuItem.Header = "Удалить категорию";
                checkBoxContextMenuItem.DataContext = currentCategory;
                checkBoxContextMenuItem.Click += RemoveCategoryHandler;
                checkBoxContextMenu.Items.Add(checkBoxContextMenuItem);
                checkBoxContextMenuItem = new MenuItem();
                checkBoxContextMenuItem.Header = "Создать категорию";
                checkBoxContextMenuItem.DataContext = currentCategory;
                checkBoxContextMenuItem.Click += CreateCategoryHandler;
                checkBoxContextMenu.Items.Add(checkBoxContextMenuItem);
                checkBox.ContextMenu = checkBoxContextMenu;
            }
        }

        private void CreateCategoryHandler (object sender, RoutedEventArgs e)
        {
            CreateCategory();
        }

        public void CreateCategory ()
        {
            List<string> friendIds = new List<string>() {
                friendId
            };
            Dialogs.CreateCategoryDialog dialog = new Dialogs.CreateCategoryDialog(currentUserId, friendIds);
            dialog.Closed += GetCategoriesHandler;
            dialog.Show();
        }

        public void GetCategoriesHandler (object sender, EventArgs e)
        {
            GetCategories();
        }

        private void SetDefaultAvatarHandler(object sender, ExceptionRoutedEventArgs e)
        {
            Image avatar = ((Image)(sender));
            SetDefaultAvatar(avatar);
        }

        public void SetDefaultAvatar(Image avatar)
        {
            avatar.BeginInit();
            avatar.Source = new BitmapImage(new Uri(@"https://cdn2.iconfinder.com/data/icons/ios-7-icons/50/user_male-128.png"));
            avatar.EndInit();
        }

        private void CancelHandler(object sender, RoutedEventArgs e)
        {
            Cancel();
        }

        public void Cancel()
        {
            this.Close();
        }

        private void AcceptHandler(object sender, RoutedEventArgs e)
        {
            Accept();
        }

        public void Accept ()
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
            string currentLogoutDate = loadedContent.logoutDate;
            List<string> currentSections = loadedContent.sections;
            foreach (CheckBox checkBox in categories.Children)
            {
                object rawCurrentCategory = checkBox.DataContext;
                string currentCategory = ((string)(rawCurrentCategory));
                object rawIsChecked = checkBox.IsChecked;
                bool isChecked = ((bool)(rawIsChecked));
                int foundIndex = updatedFriends.FindIndex((FriendSettings friend) =>
                {
                    string localFriendId = friend.id;
                    bool isFriendFound = localFriendId == friendId;
                    return isFriendFound;
                });
                bool isFound = foundIndex >= 0;
                if (isFound)
                {
                    FriendSettings foundedFriend = updatedFriends[foundIndex];
                    List<string> foundedFriendCategories = foundedFriend.categories;
                    bool isHaveCategory = foundedFriendCategories.Contains(currentCategory);
                    if (isHaveCategory && !isChecked)
                    {
                        foundedFriendCategories.Remove(currentCategory);
                    }
                    else if (!isHaveCategory && isChecked)
                    {
                        foundedFriendCategories.Add(currentCategory);
                    }
                }
            }

            string savedContent = js.Serialize(new SavedContent
            {
                games = currentGames,
                friends = updatedFriends,
                settings = currentSettings,
                collections = currentCollections,
                notifications = currentNotifications,
                categories = currentCategories,
                recentChats = currentRecentChats,
                recommendations = currentRecommendations,
                logoutDate = currentLogoutDate,
                sections = currentSections
            });
            File.WriteAllText(saveDataFilePath, savedContent);

            Cancel();
        }

        public void RemoveCategoryHandler (object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = ((MenuItem)(sender));
            object menuItemData = menuItem.DataContext;
            string category = ((string)(menuItemData));
            RemoveCategory(category);
        }

        public void RemoveCategory (string category)
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
            List<string> updatedCategories = loadedContent.categories;
            List<string> currentRecentChats = loadedContent.categories;
            Recommendations currentRecommendations = loadedContent.recommendations;
            string currentLogoutDate = loadedContent.logoutDate;
            List<string> currentSections = loadedContent.sections;
            int countCategories = updatedCategories.Count;
            int countFriends = updatedFriends.Count;
            for (int i = 0; i < countCategories;  i++)
            {
                string categoryName = updatedCategories[i];
                bool isFound = categoryName == category;
                if (isFound)
                {
                    updatedCategories.RemoveAt(i);
                    break;
                }
            }
            for (int i = 0; i < countFriends; i++)
            {
                FriendSettings friend = updatedFriends[i];
                List<string> categories = friend.categories;
                int localCountCategories = categories.Count;
                for (int j = 0; j < localCountCategories; j++) 
                {
                    string localCategoryName = categories[j];
                    bool isCategoryFound = localCategoryName == category;
                    if (isCategoryFound)
                    {
                        updatedFriends[i].categories.RemoveAt(j);
                        break;
                    }
                }
                GetCategories();
            }

            string savedContent = js.Serialize(new SavedContent
            {
                games = currentGames,
                friends = updatedFriends,
                settings = currentSettings,
                collections = currentCollections,
                notifications = currentNotifications,
                categories = updatedCategories,
                recentChats = currentRecentChats,
                recommendations = currentRecommendations,
                logoutDate = currentLogoutDate,
                sections = currentSections
            });
            File.WriteAllText(saveDataFilePath, savedContent);
        }

        public void OpenCategorySettingsHandler (object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = ((MenuItem)(sender));
            object menuItemData = menuItem.DataContext;
            string category = ((string)(menuItemData));
            OpenCategorySettings(category);
        }

        public void OpenCategorySettings (string category)
        {
            List<string> friendIds = new List<string>()
            {
                friendId
            };
            Dialogs.CategorySettingsDialog dialog = new Dialogs.CategorySettingsDialog(currentUserId, category, friendIds);
            dialog.Closed += GetCategoriesHandler;
            dialog.Show();
        }

    }
}
