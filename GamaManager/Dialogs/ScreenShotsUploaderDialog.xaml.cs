﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
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

                            string saveDataFilePath = localApplicationDataFolderPath + @"\OfficeWare\GameManager\" + currentUserId + @"\save-data.txt";
                            JavaScriptSerializer js = new JavaScriptSerializer();
                            string saveDataFileContent = File.ReadAllText(saveDataFilePath);
                            SavedContent loadedContent = js.Deserialize<SavedContent>(saveDataFileContent);
                            List<Game> currentGames = loadedContent.games;
                            List<Game> results = currentGames.Where<Game>((Game someGame) =>
                            {
                                string someGameName = someGame.name;
                                return someGameName == gameName;
                            }).ToList<Game>();
                            int countResults = results.Count;
                            bool isHaveResults = countResults >= 1;
                            if (isHaveResults)
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

                                // screenShot.DataContext = file;

                            
                                Game currentGame = results[0];
                                string currentGameId = currentGame.id;
                                Dictionary<String, Object> screenShotData = new Dictionary<String, Object>();
                                screenShotData.Add("name", file);
                                screenShotData.Add("id", currentGameId);
                                screenShot.DataContext = screenShotData;

                                screenShot.MouseLeftButtonUp += SelectScreenShotHandler;

                            }

                        }
                    }
                }
            }
        }

        public void SelectScreenShotHandler (object sender, RoutedEventArgs e)
        {
            Image screenShot = ((Image)(sender));
            object data = screenShot.DataContext;
            // string name = ((string)(data));
            Dictionary<String, Object> screenShotData = ((Dictionary<String, Object>)(data));
            string name = ((string)(screenShotData["name"]));
            string id = ((string)(screenShotData["id"]));
            SelectScreenShot(name, id, screenShotData);
        }

        public void SelectScreenShot (string name, string id, Dictionary<String, Object> screenShotData)
        {
            screenShotsControl.SelectedIndex = 1;
            actionBtns.Visibility = Visibility.Visible;
            mainScreenShot.Source = new BitmapImage(new Uri(name));
            FileInfo info = new FileInfo(name);
            DateTime creationTime = info.CreationTime;
            string rawCreationTime = creationTime.ToLongDateString();
            mainScreenShotDateLabel.Text = rawCreationTime;
            long size = info.Length;
            string measure = "Б";
            double updatedSize = size / 1024;
            bool isUpdateSize = updatedSize > 0;
            if (isUpdateSize)
            {
                size = ((int)(updatedSize));
                measure = "Кб";
            }
            updatedSize = size / 1024;
            isUpdateSize = updatedSize > 0;
            if (isUpdateSize)
            {
                size = ((int)(updatedSize));
                measure = "Мб";
            }
            updatedSize = size / 1024;
            isUpdateSize = updatedSize > 0;
            if (isUpdateSize)
            {
                size = ((int)(updatedSize));
                measure = "Гб";
            }
            string rawSize = size.ToString();
            string mainScreenShotSizeLabelContent = rawSize + " " + measure;
            mainScreenShotSizeLabel.Text = mainScreenShotSizeLabelContent;

            // mainScreenShot.DataContext = name;
            mainScreenShot.DataContext = screenShotData;

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
        
        public void UploadScreenShotHandler (object sender, RoutedEventArgs e)
        {
            UploadScreenShot();
        }

        public void UploadScreenShot ()
        {
            try
            {
                object mainScreenShotData = mainScreenShot.DataContext;

                // string path = ((string)(mainScreenShotData));
                Dictionary<String, Object> screenShotData = ((Dictionary<String, Object>)(mainScreenShotData));
                string path = ((string)(screenShotData["name"]));
                string id = ((string)(screenShotData["id"]));

                string ext = System.IO.Path.GetExtension(path);
                string desc = descBox.Text;
                string spoiler = "false";
                object rawIsChecked = spoilerCheckBox.IsChecked;
                bool isChecked = ((bool)(rawIsChecked));
                if (isChecked)
                {
                    spoiler = "true";
                }
                string url = "http://localhost:4000/api/screenshots/add/?id=" + currentUserId + @"&desc=" + desc + @"&spoiler=" + spoiler + @"&ext=" + ext + @"&game=" + id;
                HttpClient httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("User-Agent", "C# App");
                MultipartFormDataContent form = new MultipartFormDataContent();
                byte[] imagebytearraystring = getPngFromImageControl(((BitmapImage)(mainScreenShot.Source)));
                string uploadedFileName = "hash." + ext;
                form.Add(new ByteArrayContent(imagebytearraystring, 0, imagebytearraystring.Count()), "profile_pic", uploadedFileName);
                HttpResponseMessage response = httpClient.PostAsync(url, form).Result;
                httpClient.Dispose();

                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/users/points/increase/?id=" + currentUserId);
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
                            this.Close();
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

        public byte[] getPngFromImageControl (BitmapImage imageC)
        {
            MemoryStream memStream = new MemoryStream();
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(imageC));
            encoder.Save(memStream);
            return memStream.ToArray();
        }

        private void DetectDescChangedHandler (object sender, TextCompositionEventArgs e)
        {
            DetectDescChanged(e);
        }

        public void DetectDescChanged (TextCompositionEventArgs e)
        {
            string desc = descBox.Text;
            int descLength = desc.Length;
            bool isCanInput = descLength < 150;
            if (isCanInput)
            {
                bool isHaveDesc = descLength > 0;
                if (isHaveDesc)
                {
                    int charsLeft = 149 - descLength;
                    string rawCharsLeft = charsLeft.ToString();
                    charsLeftLabel.Text = rawCharsLeft + " символов осталось";
                    charsLeftLabel.Visibility = Visibility.Visible;
                }
                else
                {
                    charsLeftLabel.Visibility = Visibility.Collapsed;
                }
                e.Handled = false;
            }
            else
            {
                e.Handled = true;
            }
        }

    }
}
