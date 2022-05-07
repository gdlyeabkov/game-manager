using MaterialDesignThemes.Wpf;
using System;
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
    /// Логика взаимодействия для TalkSettingsDialog.xaml
    /// </summary>
    public partial class TalkSettingsDialog : Window
    {

        public string currentUserId = "";
        public string talkId = "";
        public string attachmentExt = "png";

        public TalkSettingsDialog(string currentUserId, string talkId)
        {
            InitializeComponent();

            Initialize(currentUserId, talkId);
            
        }

        public void Initialize (string currentUserId, string talkId)
        {
            this.currentUserId = currentUserId;
            this.talkId = talkId;
            SetTalkTitleLabel();
            SetTalkTitleSlogan();
            SetTalkAvatar();
            SetOwnerInfo();
        }

        public void SetTalkAvatar ()
        {
            talkAvatar.BeginInit();
            talkAvatar.Source = new BitmapImage(new Uri(@"http://localhost:4000/api/talk/photo/?id=" + talkId));
            talkAvatar.EndInit();
        }

        public void SetOwnerInfo ()
        {
            Talk talk = GetTalkInfo();
            string talkOwner = talk.owner;
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/users/get/?id=" + talkOwner);
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
                            User owner = myobj.user;
                            string ownerName = owner.name;
                            string ownerStatus = owner.status;
                            talkOwnerAvatar.BeginInit();
                            talkOwnerAvatar.Source = new BitmapImage(new Uri(@"http://localhost:4000/api/user/avatar/?id=" + talkOwner));
                            talkOwnerAvatar.EndInit();
                            talkOwnerNameLabel.Text = ownerName;
                            talkOwnerStatusLabel.Text = ownerStatus;
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

        public void SetTalkTitleLabel ()
        {
            Talk talk = GetTalkInfo();
            string talkTitle = talk.title;
            talkTitleBox.Text = talkTitle;
        }

        public void SetTalkTitleSlogan ()
        {
            Talk talk = GetTalkInfo();
            string talkSlogan = talk.slogan;
            talkSloganBox.Text = talkSlogan;
        }

        private void SelectSettingsItemHandler (object sender, MouseButtonEventArgs e)
        {
            TextBlock item = ((TextBlock)(sender));
            object data = item.DataContext;
            string rawIndex = data.ToString();
            int index = Int32.Parse(rawIndex);
            SelectSettingsItem(index);
        }

        public void SelectSettingsItem (int index)
        {
            bool isChannels = index == 1;
            if (isChannels)
            {
                GetChannels();
            }
            settingsControl.SelectedIndex = index;
        }

        public void GetChannels ()
        {
            channels.Children.Clear();
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/talks/channels/all");
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (var reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        var objText = reader.ReadToEnd();
                        TalkChannelsResponseInfo myobj = (TalkChannelsResponseInfo)js.Deserialize(objText, typeof(TalkChannelsResponseInfo));
                        string status = myobj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
                            List<TalkChannel> totalChannels = myobj.channels;
                            foreach (TalkChannel channel in totalChannels)
                            {
                                string channelTalkId = channel.talk;
                                string channelId = channel._id;
                                string channelTitle = channel.title;
                                bool isMainChannel = channelTitle == "Основной";
                                bool isNotMainChannel = !isMainChannel;
                                bool isCurrentTalkChannel = channelTalkId == talkId;
                                bool isAddChannel = isNotMainChannel && isCurrentTalkChannel;
                                if (isAddChannel)
                                {
                                    DockPanel channelsItem = new DockPanel();
                                    channelsItem.Background = System.Windows.Media.Brushes.LightGray;
                                    StackPanel channelsItemAside = new StackPanel();
                                    channelsItemAside.Orientation = Orientation.Horizontal;
                                    channelsItemAside.Margin = new Thickness(10);
                                    PackIcon channelsItemAsideIcon = new PackIcon();
                                    channelsItemAsideIcon.Margin = new Thickness(15, 0, 15, 0);
                                    channelsItemAsideIcon.Kind = PackIconKind.FormatAlignLeft;
                                    channelsItemAside.Children.Add(channelsItemAsideIcon);
                                    TextBlock channelsItemAsideTitleLabel = new TextBlock();
                                    channelsItemAsideTitleLabel.Margin = new Thickness(15, 0, 15, 0);
                                    channelsItemAsideTitleLabel.Text = channelTitle;
                                    channelsItemAside.Children.Add(channelsItemAsideTitleLabel);
                                    channelsItem.Children.Add(channelsItemAside);
                                    TextBlock channelsItemRemoveLabel = new TextBlock();
                                    channelsItemRemoveLabel.Margin = new Thickness(15, 0, 15, 0);
                                    channelsItemRemoveLabel.Text = "Удалить";
                                    channelsItemRemoveLabel.HorizontalAlignment = HorizontalAlignment.Right;
                                    channelsItemRemoveLabel.VerticalAlignment = VerticalAlignment.Center;
                                    channelsItem.Children.Add(channelsItemRemoveLabel);
                                    channels.Children.Add(channelsItem);
                                    channelsItemRemoveLabel.DataContext = channelId;
                                    channelsItemRemoveLabel.MouseLeftButtonUp += RemoveChannelHandler;
                                }
                            }
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

        private void LogoutFromTalkHandler (object sender, MouseButtonEventArgs e)
        {
            LogoutFromTalk();
        }

        public void LogoutFromTalk ()
        {
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/talks/relations/delete/?id=" + talkId + @"&user=" + currentUserId);
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
                            Cancel();
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

        public void Cancel()
        {
            this.Close();
        }

        private Talk GetTalkInfo()
        {
            Talk talk = null;
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/talks/get/?id=" + talkId);
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (var reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        var objText = reader.ReadToEnd();
                        TalkResponseInfo myobj = (TalkResponseInfo)js.Deserialize(objText, typeof(TalkResponseInfo));
                        string status = myobj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
                            talk = myobj.talk;
                        }
                    }
                }
            }
            catch (System.Net.WebException)
            {
                MessageBox.Show("Не удается подключиться к серверу", "Ошибка");
                this.Close();
            }
            return talk;
        }

        private void SaveTalkInfoHandler (object sender, EventArgs e)
        {
            SaveTalkInfo();
        }

        private void SaveTalkInfo ()
        {
            string talkTitleBoxContent = talkTitleBox.Text;
            string talkSloganBoxContent = talkSloganBox.Text;
            try
            {
                string url = @"http://localhost:4000/api/talk/edit/?id=" + talkId + @"&title=" + talkTitleBoxContent + @"&slogan=" + talkSloganBoxContent + @"&ext=" + attachmentExt;
                HttpClient httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("User-Agent", "C# App");
                MultipartFormDataContent form = new MultipartFormDataContent();
                ImageSource source = talkAvatar.Source;
                BitmapImage bitmapImage = ((BitmapImage)(source));
                byte[] imagebytearraystring = getPngFromImageControl(bitmapImage);
                form.Add(new ByteArrayContent(imagebytearraystring, 0, imagebytearraystring.Count()), "profile_pic", "mock.png");
                HttpResponseMessage response = httpClient.PostAsync(url, form).Result;
                httpClient.Dispose();
            }
            catch (System.Net.WebException)
            {
                MessageBox.Show("Не удается подключиться к серверу", "Ошибка");
                this.Close();
            }
        }

        public byte[] getPngFromImageControl(BitmapImage imageC)
        {
            MemoryStream memStream = new MemoryStream();
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(imageC));
            encoder.Save(memStream);
            return memStream.ToArray();
        }

        private void CreateTextChannelHandler (object sender, RoutedEventArgs e)
        {
            CreateTextChannel();
        }

        public void CreateTextChannel ()
        {
            Dialogs.CreateTextChannelDialog dialog = new Dialogs.CreateTextChannelDialog(talkId);
            dialog.Closed += GetChannelsHandler;
            dialog.Show();
        }

        public void GetChannelsHandler (object sender, EventArgs e)
        {
            GetChannels();
        }

        private void AddIconHandler (object sender, RoutedEventArgs e)
        {
            AddIcon();
        }

        private void AddIcon ()
        {
            Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
            ofd.Title = "Выберите иконку";
            ofd.Filter = "BMP|*.bmp|GIF|*.gif|JPG|*.jpg;*.jpeg|PNG|*.png|TIFF|*.tif;*.tiff";
            bool? res = ofd.ShowDialog();
            bool isOpened = res != false;
            if (isOpened)
            {
                string path = ofd.FileName;
                attachmentExt = System.IO.Path.GetExtension(path);
                talkAvatar.BeginInit();
                talkAvatar.Source = new BitmapImage(new Uri(path));
                talkAvatar.EndInit();
            }
        }

        public void RemoveChannelHandler (object sender, RoutedEventArgs e)
        {
            TextBlock label = ((TextBlock)(sender));
            object labelData = label.DataContext;
            string id = ((string)(labelData));
            RemoveChannel(id);
        }

        public void RemoveChannel (string id)
        {
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/talks/channels/remove/?id=" + id);
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
                            GetChannels();
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

    }
}
