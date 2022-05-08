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
            GetInvitedUsers();
            GenerateLink();
            GetRoles();
        }

        public void GetInvitedUsers ()
        {
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/msgs/get");
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (StreamReader reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        string objText = reader.ReadToEnd();
                        MsgsResponseInfo myObj = (MsgsResponseInfo)js.Deserialize(objText, typeof(MsgsResponseInfo));
                        string status = myObj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
                            List<Msg> msgs = myObj.msgs;
                            int countInvites = msgs.Count<Msg>((Msg localMsg) =>
                            {
                                string localMsgType = localMsg.type;
                                bool isLocalLink = localMsgType == "link";
                                string localMsgContent = localMsg.content;
                                bool isCurrentTalk = talkId == localMsgContent;
                                bool isInvite = isLocalLink && isCurrentTalk;
                                return isInvite;
                            });
                            bool isHaveInvites = countInvites >= 1;
                            invitedUsers.Children.Clear();
                            if (isHaveInvites)
                            {
                                foreach (Msg msg in msgs)
                                {
                                    string msgType = msg.type;
                                    bool isLink = msgType == "link";
                                    if (isLink)
                                    {
                                        string msgContent = msg.content;
                                        bool isCurrentTalk = talkId == msgContent;
                                        if (isCurrentTalk)
                                        {
                                            string userId = msg.friend;
                                            HttpWebRequest innerWebRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/users/get/?id=" + userId);
                                            innerWebRequest.Method = "GET";
                                            innerWebRequest.UserAgent = ".NET Framework Test Client";
                                            using (HttpWebResponse innerWebResponse = (HttpWebResponse)innerWebRequest.GetResponse())
                                            {
                                                using (StreamReader innerReader = new StreamReader(innerWebResponse.GetResponseStream()))
                                                {
                                                    js = new JavaScriptSerializer();
                                                    objText = innerReader.ReadToEnd();
                                                    UserResponseInfo myInnerObj = (UserResponseInfo)js.Deserialize(objText, typeof(UserResponseInfo));
                                                    status = myInnerObj.status;
                                                    isOkStatus = status == "OK";
                                                    if (isOkStatus)
                                                    {
                                                        User user = myInnerObj.user;
                                                        string userName = user.name;
                                                        StackPanel invitedUser = new StackPanel();
                                                        invitedUser.Orientation = Orientation.Horizontal;
                                                        invitedUser.Height = 65;
                                                        Image invitedUserAvatar = new Image();
                                                        invitedUserAvatar.Width = 25;
                                                        invitedUserAvatar.Height = 25;
                                                        invitedUserAvatar.Margin = new Thickness(15);
                                                        invitedUserAvatar.VerticalAlignment = VerticalAlignment.Center;
                                                        invitedUserAvatar.BeginInit();
                                                        invitedUserAvatar.Source = new BitmapImage(new Uri(@"http://localhost:4000/api/user/avatar?id=" + userId));
                                                        invitedUserAvatar.EndInit();
                                                        invitedUser.Children.Add(invitedUserAvatar);
                                                        TextBlock invitedUserNameLabel = new TextBlock();
                                                        invitedUserNameLabel.Margin = new Thickness(15);
                                                        invitedUserNameLabel.VerticalAlignment = VerticalAlignment.Center;
                                                        invitedUserNameLabel.Text = userName;
                                                        invitedUser.Children.Add(invitedUserNameLabel);
                                                        invitedUsers.Children.Add(invitedUser);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                TextBlock notFoundLabel = new TextBlock();
                                notFoundLabel.Text = "Никто не приглашён";
                                notFoundLabel.Margin = new Thickness(15);
                                notFoundLabel.FontSize = 14;
                                invitedUsers.Children.Add(notFoundLabel);
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

                            bool isOwner = currentUserId == talkOwner;
                            if (isOwner)
                            {
                                permissionsControl.SelectedIndex = 1;
                            }
                            else
                            {
                                permissionsControl.SelectedIndex = 0;
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

        private void CopyLinkHandler (object sender, RoutedEventArgs e)
        {
            CopyLink();
            mainLinkBox.Copy();
        }

        public void CopyLink ()
        {
            mainLinkBox.SelectAll();
            mainLinkBox.Copy();
            mainLinkBox.Select(0, 0);
        }

        public void GenerateLink ()
        {
            mainLinkBox.Text = "http://localhost:4000/?talk=" + talkId;
        }

        private void UpdateRoleHandler (object sender, RoutedEventArgs e)
        {
            DockPanel role = ((DockPanel)(sender));
            object roleData = role.DataContext;
            string id = ((string)(roleData));
            UpdateRole(id);
        }

        public void UpdateRole (string id)
        {
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/talks/roles/get/?id=" + id);
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (StreamReader reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        string objText = reader.ReadToEnd();
                        TalkRoleResponseInfo myObj = (TalkRoleResponseInfo)js.Deserialize(objText, typeof(TalkRoleResponseInfo));
                        string status = myObj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
                            Role role = myObj.role;
                            string roleTitle = role.title;
                            mainRoleTitleLabel.Text = roleTitle;
                            permissionsControl.SelectedIndex = 2;
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

        private void CancelUpdateRolesHandler(object sender, MouseButtonEventArgs e)
        {
            CancelUpdateRoles();
        }

        public void CancelUpdateRoles()
        {
            permissionsControl.SelectedIndex = 1;
        }

        private void OpenCreatePermissionPopupHandler (object sender, RoutedEventArgs e)
        {
            OpenCreatePermissionPopup();
        }

        public void OpenCreatePermissionPopup ()
        {
            createPermissionPopup.IsOpen = true;
        }

        private void CloseCreatePermissionPopupHandler(object sender, RoutedEventArgs e)
        {
            CloseCreatePermissionPopup();
        }

        public void CloseCreatePermissionPopup()
        {
            createPermissionPopup.IsOpen = false;
        }

        private void CreatePermissionHandler (object sender, RoutedEventArgs e)
        {
            CreatePermission();
        }

        public void GetRoles ()
        {
            roles.Children.Clear();
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/talks/roles/all");
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (StreamReader reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        string objText = reader.ReadToEnd();
                        TalkRolesResponseInfo myObj = (TalkRolesResponseInfo)js.Deserialize(objText, typeof(TalkRolesResponseInfo));
                        string status = myObj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
                            List<Role> talkRoles = myObj.roles;
                            foreach (Role talkRole in talkRoles)
                            {
                                string roleId = talkRole._id;
                                string roleTitle = talkRole.title;
                                DockPanel role = new DockPanel();
                                role.Background = System.Windows.Media.Brushes.LightGray;
                                role.Margin = new Thickness(0, 15, 0, 15);
                                TextBlock roleTitleLabel = new TextBlock();
                                roleTitleLabel.Text = roleTitle;
                                roleTitleLabel.Margin = new Thickness(15);
                                role.Children.Add(roleTitleLabel);
                                TextBlock updateRoleLabel = new TextBlock();
                                updateRoleLabel.HorizontalAlignment = HorizontalAlignment.Right;
                                updateRoleLabel.Text = "Изменить";
                                updateRoleLabel.Margin = new Thickness(15);
                                role.Children.Add(updateRoleLabel);
                                role.DataContext = roleId;
                                role.MouseLeftButtonUp += UpdateRoleHandler;
                                roles.Children.Add(role);
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

        public void CreatePermission ()
        {
            string talkRoleTitleBoxContent = talkRoleTitleBox.Text;
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/talks/roles/create/?title=" + talkRoleTitleBoxContent + @"&id=" + talkId);
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (StreamReader reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        string objText = reader.ReadToEnd();
                        MsgsResponseInfo myObj = (MsgsResponseInfo)js.Deserialize(objText, typeof(MsgsResponseInfo));
                        string status = myObj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
                            talkRoleTitleBox.Text = "";
                            CloseCreatePermissionPopup();
                            GetRoles();
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
