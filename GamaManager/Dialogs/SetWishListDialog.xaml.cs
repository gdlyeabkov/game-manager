using MaterialDesignThemes.Wpf;
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
    /// Логика взаимодействия для SetWishListDialog.xaml
    /// </summary>
    public partial class SetWishListDialog : Window
    {

        public string currentUserId = "";

        public SetWishListDialog(string currentUserId)
        {
            InitializeComponent();

            Initialize(currentUserId);

        }

        public void Initialize(string currentUserId)
        {
            this.currentUserId = currentUserId;
            LoadRecommendations();
        }


        private void OkHandler (object sender, RoutedEventArgs e)
        {
            Ok();
        }

        public void Ok ()
        {
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("https://loud-reminiscent-jackrabbit.glitch.me/api/activities/add/?id=" + currentUserId + @"&content=addGameToWishList&data=addGameToWishList");
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (var reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        var objText = reader.ReadToEnd();
                        GameTagsResponseInfo myObj = (GameTagsResponseInfo)js.Deserialize(objText, typeof(GameTagsResponseInfo));
                        string status = myObj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
                            Environment.SpecialFolder localApplicationDataFolder = Environment.SpecialFolder.LocalApplicationData;
                            string localApplicationDataFolderPath = Environment.GetFolderPath(localApplicationDataFolder);
                            string saveDataFilePath = localApplicationDataFolderPath + @"\OfficeWare\GameManager\" + currentUserId + @"\save-data.txt";
                            js = new JavaScriptSerializer();
                            string saveDataFileContent = File.ReadAllText(saveDataFilePath);
                            SavedContent loadedContent = js.Deserialize<SavedContent>(saveDataFileContent);
                            List<Game> currentGames = loadedContent.games;
                            List<FriendSettings> updatedFriends = loadedContent.friends;
                            Settings currentSettings = loadedContent.settings;
                            List<string> currentCollections = loadedContent.collections;
                            Notifications currentNotifications = loadedContent.notifications;
                            List<string> currentCategories = loadedContent.categories;
                            List<string> currentRecentChats = loadedContent.recentChats;
                            Recommendations updatedRecommendations = loadedContent.recommendations;
                            string currentLogoutDate = loadedContent.logoutDate;
                            List<string> currentSections = loadedContent.sections;
                            object rawIsChecked = earlyAccessCheckBox.IsChecked;
                            bool isChecked = ((bool)(rawIsChecked));
                            updatedRecommendations.isEarlyAccess = isChecked;
                            rawIsChecked = notReleasesCheckBox.IsChecked;
                            isChecked = ((bool)(rawIsChecked));
                            updatedRecommendations.isNotReleases = isChecked;
                            rawIsChecked = softWareCheckBox.IsChecked;
                            isChecked = ((bool)(rawIsChecked));
                            updatedRecommendations.isSoftWare = isChecked;
                            rawIsChecked = soundTracksCheckBox.IsChecked;
                            isChecked = ((bool)(rawIsChecked));
                            updatedRecommendations.isSoundTracks = isChecked;
                            rawIsChecked = videoCheckBox.IsChecked;
                            isChecked = ((bool)(rawIsChecked));
                            updatedRecommendations.isVideo = isChecked;
                            updatedRecommendations.exceptTags.Clear();
                            foreach (DockPanel tag in tags.Children)
                            {
                                object tagData = tag.DataContext;
                                string title = ((string)(tagData));
                                updatedRecommendations.exceptTags.Add(title);
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
                                recommendations = updatedRecommendations,
                                logoutDate = currentLogoutDate,
                                sections = currentSections
                            });
                            File.WriteAllText(saveDataFilePath, savedContent);
                            Cancel();
                        }
                    }
                }
            }
            catch (System.Net.WebException exception)
            {
                MessageBox.Show("Не удается подключиться к серверу", "Ошибка");
                this.Close();
            }
        }

        private void CancelHandler(object sender, RoutedEventArgs e)
        {
            Cancel();
        }

        public void Cancel ()
        {
            this.Close();
        }

        private void AddTagHandler (object sender, RoutedEventArgs e)
        {
            string tagBoxContent = tagBox.Text;
            AddTag(tagBoxContent);
        }

        public void AddTag (string tagName)
        {
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("https://loud-reminiscent-jackrabbit.glitch.me/api/games/tags/all");
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (var reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        var objText = reader.ReadToEnd();
                        GameTagsResponseInfo myObj = (GameTagsResponseInfo)js.Deserialize(objText, typeof(GameTagsResponseInfo));
                        string status = myObj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
                            List<GameTag> totalTags = myObj.tags;
                            int index = totalTags.FindIndex((GameTag someTag) =>
                            {
                                string someTagTitle = someTag.title;
                                bool isTagMatch = tagName == someTagTitle;
                                return isTagMatch;
                            });
                            bool isTagsFound = index >= 0;
                            bool isCanAdd = true;
                            foreach (DockPanel localTag in tags.Children)
                            {
                                object localTagData = localTag.DataContext;
                                string localTagName = ((string)(localTagData));
                                bool isCanNotAdd = localTagName == tagName;
                                if (isCanNotAdd)
                                {
                                    isCanAdd = false;
                                    break;
                                }
                            }
                            bool isAddTag = isTagsFound && isCanAdd;
                            if (isAddTag)
                            {
                                GameTag localTag = totalTags[index];
                                string localTagName = localTag.title;
                                DockPanel tag = new DockPanel();
                                tag.DataContext = localTagName;
                                tag.Background = System.Windows.Media.Brushes.LightGray;
                                Button tagBtn = new Button();
                                tagBtn.Margin = new Thickness(10, 5, 10, 5);
                                TextBlock tagBtnContent = new TextBlock();
                                tagBtnContent.Margin = new Thickness(10, 5, 10, 5);
                                tagBtnContent.Text = localTagName;
                                tagBtn.Content = tagBtnContent;
                                tag.Children.Add(tagBtn);
                                PackIcon tagIcon = new PackIcon();
                                tagIcon.Margin = new Thickness(15);
                                tagIcon.Kind = PackIconKind.Check;
                                tagIcon.HorizontalAlignment = HorizontalAlignment.Right;
                                tagIcon.DataContext = tag;
                                tagIcon.MouseLeftButtonUp += RemoveTagHandler;
                                tag.Children.Add(tagIcon);
                                tags.Children.Add(tag);
                            }
                        }
                    }
                }
            }
            catch (System.Net.WebException exception)
            {
                MessageBox.Show("Не удается подключиться к серверу", "Ошибка");
                this.Close();
            }
        }

        private void GetTagsHandler (object sender, TextChangedEventArgs e)
        {
            GetTags();
        }

        public void GetTags ()
        {
            tagBoxPopupBody.Children.Clear();
            string tagBoxContent = tagBox.Text;
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("https://loud-reminiscent-jackrabbit.glitch.me/api/games/tags/all");
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (var reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        var objText = reader.ReadToEnd();
                        GameTagsResponseInfo myObj = (GameTagsResponseInfo)js.Deserialize(objText, typeof(GameTagsResponseInfo));
                        string status = myObj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
                            List<GameTag> totalTags = myObj.tags;
                            List<GameTag> foundedTags = totalTags.Where((GameTag someTag) =>
                            {
                                string someTagTitle = someTag.title;
                                bool isTagMatch = someTagTitle.Contains(tagBoxContent);
                                return isTagMatch;
                            }).ToList<GameTag>();
                            int foundedTagsCount = foundedTags.Count;
                            bool isTagsFound = foundedTagsCount >= 1;
                            foreach (GameTag localTag in foundedTags)
                            {
                                string localTagName = localTag.title;
                                TextBlock tagLabel = new TextBlock();
                                tagLabel.Margin = new Thickness(15, 5, 15, 5);
                                tagLabel.Text = localTagName;
                                tagLabel.DataContext = localTagName;
                                tagLabel.MouseLeftButtonUp += SelectTagHandler;
                                tagBoxPopupBody.Children.Add(tagLabel);
                            }
                            tagBoxPopup.IsOpen = isTagsFound;
                        }
                    }
                }
            }
            catch (System.Net.WebException exception)
            {
                MessageBox.Show("Не удается подключиться к серверу", "Ошибка");
                this.Close();
            }
        }

        public void SelectTagHandler (object sender, RoutedEventArgs e)
        {
            TextBlock label = ((TextBlock)(sender));
            object labelData = label.DataContext;
            string tagName = ((string)(labelData));
            SelectTag(tagName);
        }

        public void SelectTag (string tagName)
        {
            tagBox.Text = tagName;
        }

        public void RemoveTagHandler (object sender, RoutedEventArgs e)
        {
            PackIcon icon = ((PackIcon)(sender));
            object iconData = icon.DataContext;
            DockPanel tag = ((DockPanel)(iconData));
            RemoveTag(tag);
        }

        public void RemoveTag (DockPanel tag)
        {
            tags.Children.Remove(tag);
            GetTags();
        }

        public void LoadRecommendations ()
        {
            Environment.SpecialFolder localApplicationDataFolder = Environment.SpecialFolder.LocalApplicationData;
            string localApplicationDataFolderPath = Environment.GetFolderPath(localApplicationDataFolder);
            string saveDataFilePath = localApplicationDataFolderPath + @"\OfficeWare\GameManager\" + currentUserId + @"\save-data.txt";
            JavaScriptSerializer js = new JavaScriptSerializer();
            string saveDataFileContent = File.ReadAllText(saveDataFilePath);
            SavedContent loadedContent = js.Deserialize<SavedContent>(saveDataFileContent);
            Recommendations currentRecommendations = loadedContent.recommendations;
            string currentLogoutDate = loadedContent.logoutDate;
            List<string> currentSections = loadedContent.sections;
            bool isEarlyAccess = currentRecommendations.isEarlyAccess;
            bool isNotReleases = currentRecommendations.isNotReleases;
            bool isSoftWare = currentRecommendations.isSoftWare;
            bool isSoundTracks = currentRecommendations.isSoundTracks;
            bool isVideo = currentRecommendations.isVideo;
            List<string> exceptTags = currentRecommendations.exceptTags;
            earlyAccessCheckBox.IsChecked = isEarlyAccess;
            notReleasesCheckBox.IsChecked = isNotReleases;
            softWareCheckBox.IsChecked = isSoftWare;
            soundTracksCheckBox.IsChecked = isSoundTracks;
            videoCheckBox.IsChecked = isVideo;
            foreach (string exceptTag in exceptTags)
            {
                AddTag(exceptTag);
            }
        }

    }
}
