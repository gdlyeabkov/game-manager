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

                            HttpWebRequest rolesRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/talks/roles/all");
                            rolesRequest.Method = "GET";
                            rolesRequest.UserAgent = ".NET Framework Test Client";
                            using (HttpWebResponse rolesResponse = (HttpWebResponse)rolesRequest.GetResponse())
                            {
                                using (StreamReader rolesReader = new StreamReader(rolesResponse.GetResponseStream()))
                                {
                                    js = new JavaScriptSerializer();
                                    objText = rolesReader.ReadToEnd();
                                    TalkRolesResponseInfo myRolesObj = (TalkRolesResponseInfo)js.Deserialize(objText, typeof(TalkRolesResponseInfo));
                                    status = myRolesObj.status;
                                    isOkStatus = status == "OK";
                                    if (isOkStatus)
                                    {
                                        List<Role> talkRoles = myRolesObj.roles;
                                        HttpWebRequest roleRelationsWebRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/talks/roles/relations/all");
                                        roleRelationsWebRequest.Method = "GET";
                                        roleRelationsWebRequest.UserAgent = ".NET Framework Test Client";
                                        using (HttpWebResponse roleRelationsWebResponse = (HttpWebResponse)roleRelationsWebRequest.GetResponse())
                                        {
                                            using (StreamReader roleRelationsReader = new StreamReader(roleRelationsWebResponse.GetResponseStream()))
                                            {
                                                js = new JavaScriptSerializer();
                                                objText = roleRelationsReader.ReadToEnd();
                                                TalkRoleRelationsResponseInfo myRoleRelationsObj = (TalkRoleRelationsResponseInfo)js.Deserialize(objText, typeof(TalkRoleRelationsResponseInfo));
                                                status = myRoleRelationsObj.status;
                                                isOkStatus = status == "OK";
                                                if (isOkStatus)
                                                {
                                                    List<TalkRoleRelation> talkRoleRelations = myRoleRelationsObj.relations;
                                                    List<TalkRoleRelation> currentTalkRoleRelations = talkRoleRelations.Where<TalkRoleRelation>((TalkRoleRelation talkRoleRelation) =>
                                                    {
                                                        string talkRoleRelationRoleId = talkRoleRelation.role;
                                                        string talkRoleRelationTalkId = talkRoleRelation.talk;
                                                        string talkRoleRelationUserId = talkRoleRelation.user;
                                                        bool isCurrentUser = talkRoleRelationUserId == currentUserId;
                                                        bool isCurrentTalk = talkRoleRelationTalkId == talkId;
                                                        bool isCurrentTalkRoleRelation = isCurrentUser && isCurrentTalk;
                                                        return isCurrentTalkRoleRelation;
                                                    }).ToList<TalkRoleRelation>();
                                                    List<string> currentTalkRoleRelationsRoles = new List<string>();
                                                    foreach (TalkRoleRelation currentTalkRoleRelation in currentTalkRoleRelations)
                                                    {
                                                        string currentTalkRoleRelationRoleId = currentTalkRoleRelation.role;
                                                        currentTalkRoleRelationsRoles.Add(currentTalkRoleRelationRoleId);
                                                    }
                                                    List<Role> myRoles = talkRoles.Where<Role>((Role talkRole) =>
                                                    {
                                                        string talkRoleId = talkRole._id;
                                                        bool isRoleFound = currentTalkRoleRelationsRoles.Contains(talkRoleId);
                                                        bool isCanUpdateRoles = talkRole.updateRoles;
                                                        bool isCanAssignRoles = talkRole.assignRoles;
                                                        bool isCanBlock = talkRole.block;
                                                        bool isCanKick = talkRole.kick;
                                                        bool isRoleMatch = isRoleFound && (isCanUpdateRoles || isCanAssignRoles || isCanBlock || isCanKick);
                                                        return isRoleMatch;
                                                    }).ToList<Role>();
                                                    int myRolesCount = myRoles.Count;
                                                    bool isHaveRoles = myRolesCount >= 1;
                                                    bool isOwner = currentUserId == talkOwner;
                                                    bool isCanViewRoles = isHaveRoles || isOwner;
                                                    if (isCanViewRoles)
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
            bool isPermissions = index == 2;
            if (isChannels)
            {
                GetChannels();
            }
            else if (isPermissions)
            {
                GetRoles();
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
            try
            {
                HttpWebRequest rolesRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/talks/roles/all");
                rolesRequest.Method = "GET";
                rolesRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse rolesResponse = (HttpWebResponse)rolesRequest.GetResponse())
                {
                    using (StreamReader rolesReader = new StreamReader(rolesResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        string objText = rolesReader.ReadToEnd();
                        TalkRolesResponseInfo myRolesObj = (TalkRolesResponseInfo)js.Deserialize(objText, typeof(TalkRolesResponseInfo));
                        string status = myRolesObj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
                            List<Role> talkRoles = myRolesObj.roles;
                            HttpWebRequest roleRelationsWebRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/talks/roles/relations/all");
                            roleRelationsWebRequest.Method = "GET";
                            roleRelationsWebRequest.UserAgent = ".NET Framework Test Client";
                            using (HttpWebResponse roleRelationsWebResponse = (HttpWebResponse)roleRelationsWebRequest.GetResponse())
                            {
                                using (StreamReader roleRelationsReader = new StreamReader(roleRelationsWebResponse.GetResponseStream()))
                                {
                                    js = new JavaScriptSerializer();
                                    objText = roleRelationsReader.ReadToEnd();
                                    TalkRoleRelationsResponseInfo myRoleRelationsObj = (TalkRoleRelationsResponseInfo)js.Deserialize(objText, typeof(TalkRoleRelationsResponseInfo));
                                    status = myRoleRelationsObj.status;
                                    isOkStatus = status == "OK";
                                    if (isOkStatus)
                                    {
                                        List<TalkRoleRelation> talkRoleRelations = myRoleRelationsObj.relations;
                                        List<TalkRoleRelation> currentTalkRoleRelations = talkRoleRelations.Where<TalkRoleRelation>((TalkRoleRelation talkRoleRelation) =>
                                        {
                                            string talkRoleRelationRoleId = talkRoleRelation.role;
                                            string talkRoleRelationTalkId = talkRoleRelation.talk;
                                            string talkRoleRelationUserId = talkRoleRelation.user;
                                            bool isCurrentUser = talkRoleRelationUserId == currentUserId;
                                            bool isCurrentTalk = talkRoleRelationTalkId == talkId;
                                            bool isCurrentTalkRoleRelation = isCurrentUser && isCurrentTalk;
                                            return isCurrentTalkRoleRelation;
                                        }).ToList<TalkRoleRelation>();
                                        List<string> currentTalkRoleRelationsRoles = new List<string>();
                                        foreach (TalkRoleRelation currentTalkRoleRelation in currentTalkRoleRelations)
                                        {
                                            string currentTalkRoleRelationRoleId = currentTalkRoleRelation.role;
                                            currentTalkRoleRelationsRoles.Add(currentTalkRoleRelationRoleId);
                                        }
                                        List<Role> myRoles = talkRoles.Where<Role>((Role talkRole) =>
                                        {
                                            string talkRoleId = talkRole._id;
                                            bool isRoleFound = currentTalkRoleRelationsRoles.Contains(talkRoleId);
                                            bool isLocalCanCreateChannels = talkRole.createAndUpdateChannels;
                                            bool isRoleMatch = isRoleFound && isLocalCanCreateChannels;
                                            return isRoleMatch;
                                        }).ToList<Role>();
                                        int myRolesCount = myRoles.Count;
                                        bool isHaveRoles = myRolesCount >= 1;
                                        Talk talk = GetTalkInfo();
                                        string owner = talk.owner;
                                        bool isOwner = owner == currentUserId;
                                        bool isCanCreateChannels = isHaveRoles || isOwner;
                                        if (isCanCreateChannels)
                                        {
                                            Dialogs.CreateTextChannelDialog dialog = new Dialogs.CreateTextChannelDialog(talkId);
                                            dialog.Closed += GetChannelsHandler;
                                            dialog.Show();
                                        }
                                        else
                                        {
                                            MessageBox.Show("У вас нет разрешения создавать каналы. Обратитесь к владельцу беседы.", "Внимание");
                                        }
                                    }
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
                HttpWebRequest rolesRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/talks/roles/all");
                rolesRequest.Method = "GET";
                rolesRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse rolesResponse = (HttpWebResponse)rolesRequest.GetResponse())
                {
                    using (StreamReader rolesReader = new StreamReader(rolesResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        string objText = rolesReader.ReadToEnd();
                        TalkRolesResponseInfo myRolesObj = (TalkRolesResponseInfo)js.Deserialize(objText, typeof(TalkRolesResponseInfo));
                        string status = myRolesObj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
                            List<Role> talkRoles = myRolesObj.roles;
                            HttpWebRequest roleRelationsWebRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/talks/roles/relations/all");
                            roleRelationsWebRequest.Method = "GET";
                            roleRelationsWebRequest.UserAgent = ".NET Framework Test Client";
                            using (HttpWebResponse roleRelationsWebResponse = (HttpWebResponse)roleRelationsWebRequest.GetResponse())
                            {
                                using (StreamReader roleRelationsReader = new StreamReader(roleRelationsWebResponse.GetResponseStream()))
                                {
                                    js = new JavaScriptSerializer();
                                    objText = roleRelationsReader.ReadToEnd();
                                    TalkRoleRelationsResponseInfo myRoleRelationsObj = (TalkRoleRelationsResponseInfo)js.Deserialize(objText, typeof(TalkRoleRelationsResponseInfo));
                                    status = myRoleRelationsObj.status;
                                    isOkStatus = status == "OK";
                                    if (isOkStatus)
                                    {
                                        List<TalkRoleRelation> talkRoleRelations = myRoleRelationsObj.relations;
                                        List<TalkRoleRelation> currentTalkRoleRelations = talkRoleRelations.Where<TalkRoleRelation>((TalkRoleRelation talkRoleRelation) =>
                                        {
                                            string talkRoleRelationRoleId = talkRoleRelation.role;
                                            string talkRoleRelationTalkId = talkRoleRelation.talk;
                                            string talkRoleRelationUserId = talkRoleRelation.user;
                                            bool isCurrentUser = talkRoleRelationUserId == currentUserId;
                                            bool isCurrentTalk = talkRoleRelationTalkId == talkId;
                                            bool isCurrentTalkRoleRelation = isCurrentUser && isCurrentTalk;
                                            return isCurrentTalkRoleRelation;
                                        }).ToList<TalkRoleRelation>();
                                        List<string> currentTalkRoleRelationsRoles = new List<string>();
                                        foreach (TalkRoleRelation currentTalkRoleRelation in currentTalkRoleRelations)
                                        {
                                            string currentTalkRoleRelationRoleId = currentTalkRoleRelation.role;
                                            currentTalkRoleRelationsRoles.Add(currentTalkRoleRelationRoleId);
                                        }
                                        List<Role> myRoles = talkRoles.Where<Role>((Role talkRole) =>
                                        {
                                            string talkRoleId = talkRole._id;
                                            bool isRoleFound = currentTalkRoleRelationsRoles.Contains(talkRoleId);
                                            bool isLocalCanRemoveChannels = talkRole.createAndUpdateChannels;
                                            bool isRoleMatch = isRoleFound && isLocalCanRemoveChannels;
                                            return isRoleMatch;
                                        }).ToList<Role>();
                                        int myRolesCount = myRoles.Count;
                                        bool isHaveRoles = myRolesCount >= 1;
                                        Talk talk = GetTalkInfo();
                                        string owner = talk.owner;
                                        bool isOwner = owner == currentUserId;
                                        bool isCanRemoveChannels = isHaveRoles || isOwner;
                                        if (isCanRemoveChannels)
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
                                                        js = new JavaScriptSerializer();
                                                        objText = reader.ReadToEnd();
                                                        UserResponseInfo myobj = (UserResponseInfo)js.Deserialize(objText, typeof(UserResponseInfo));
                                                        status = myobj.status;
                                                        isOkStatus = status == "OK";
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
                                        else
                                        {
                                            MessageBox.Show("У вас нет разрешения обновлять каналы. Обратитесь к владельцу беседы.", "Внимание");
                                        }
                                    }
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
                HttpWebRequest rolesRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/talks/roles/all");
                rolesRequest.Method = "GET";
                rolesRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse rolesResponse = (HttpWebResponse)rolesRequest.GetResponse())
                {
                    using (StreamReader rolesReader = new StreamReader(rolesResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        string objText = rolesReader.ReadToEnd();
                        TalkRolesResponseInfo myRolesObj = (TalkRolesResponseInfo)js.Deserialize(objText, typeof(TalkRolesResponseInfo));
                        string status = myRolesObj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
                            List<Role> talkRoles = myRolesObj.roles;
                            HttpWebRequest roleRelationsWebRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/talks/roles/relations/all");
                            roleRelationsWebRequest.Method = "GET";
                            roleRelationsWebRequest.UserAgent = ".NET Framework Test Client";
                            using (HttpWebResponse roleRelationsWebResponse = (HttpWebResponse)roleRelationsWebRequest.GetResponse())
                            {
                                using (StreamReader roleRelationsReader = new StreamReader(roleRelationsWebResponse.GetResponseStream()))
                                {
                                    js = new JavaScriptSerializer();
                                    objText = roleRelationsReader.ReadToEnd();
                                    TalkRoleRelationsResponseInfo myRoleRelationsObj = (TalkRoleRelationsResponseInfo)js.Deserialize(objText, typeof(TalkRoleRelationsResponseInfo));
                                    status = myRoleRelationsObj.status;
                                    isOkStatus = status == "OK";
                                    if (isOkStatus)
                                    {
                                        List<TalkRoleRelation> talkRoleRelations = myRoleRelationsObj.relations;
                                        List<TalkRoleRelation> currentTalkRoleRelations = talkRoleRelations.Where<TalkRoleRelation>((TalkRoleRelation talkRoleRelation) =>
                                        {
                                            string talkRoleRelationRoleId = talkRoleRelation.role;
                                            string talkRoleRelationTalkId = talkRoleRelation.talk;
                                            string talkRoleRelationUserId = talkRoleRelation.user;
                                            bool isCurrentUser = talkRoleRelationUserId == currentUserId;
                                            bool isCurrentTalk = talkRoleRelationTalkId == talkId;
                                            bool isCurrentTalkRoleRelation = isCurrentUser && isCurrentTalk;
                                            return isCurrentTalkRoleRelation;
                                        }).ToList<TalkRoleRelation>();
                                        List<string> currentTalkRoleRelationsRoles = new List<string>();
                                        foreach (TalkRoleRelation currentTalkRoleRelation in currentTalkRoleRelations)
                                        {
                                            string currentTalkRoleRelationRoleId = currentTalkRoleRelation.role;
                                            currentTalkRoleRelationsRoles.Add(currentTalkRoleRelationRoleId);
                                        }
                                        List<Role> myRoles = talkRoles.Where<Role>((Role talkRole) =>
                                        {
                                            string talkRoleId = talkRole._id;
                                            bool isRoleFound = currentTalkRoleRelationsRoles.Contains(talkRoleId);
                                            bool isCanUpdateRoles = talkRole.updateRoles;
                                            bool isRoleMatch = isRoleFound && isCanUpdateRoles;
                                            return isRoleMatch;
                                        }).ToList<Role>();
                                        int myRolesCount = myRoles.Count;
                                        bool isHaveRoles = myRolesCount >= 1;
                                        Talk talk = GetTalkInfo();
                                        string owner = talk.owner;
                                        bool isOwner = owner == currentUserId;
                                        bool isCanUpdate = isHaveRoles || isOwner;
                                        if (isCanUpdate)
                                        {
                                            HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/talks/roles/get/?id=" + id);
                                            webRequest.Method = "GET";
                                            webRequest.UserAgent = ".NET Framework Test Client";
                                            using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                                            {
                                                using (StreamReader reader = new StreamReader(webResponse.GetResponseStream()))
                                                {
                                                    js = new JavaScriptSerializer();
                                                    objText = reader.ReadToEnd();
                                                    TalkRoleResponseInfo myObj = (TalkRoleResponseInfo)js.Deserialize(objText, typeof(TalkRoleResponseInfo));
                                                    status = myObj.status;
                                                    isOkStatus = status == "OK";
                                                    if (isOkStatus)
                                                    {
                                                        Role role = myObj.role;
                                                        string roleTitle = role.title;
                                                        bool isSendMsgs = role.sendMsgs;
                                                        bool isNotifyAllUsers = role.notifyAllUsers;
                                                        bool isBindAndUnbindStreams = role.bindAndUnbindStreams;
                                                        bool isKick = role.kick;
                                                        bool isBlock = role.block;
                                                        bool isInvite = role.invite;
                                                        bool isUpdateRoles = role.updateRoles;
                                                        bool isAssignRoles = role.assignRoles;
                                                        bool isUpdateTalkTitleSloganAndAvatar = role.updateTalkTitleSloganAndAvatar;
                                                        bool isCreateAndUpdateChannels = role.createAndUpdateChannels;
                                                        bool isCustomRole = role.isCustom;
                                                        mainRoleTitleLabel.Text = roleTitle;
                                                        mainRoleSendMsgsCheckBox.IsChecked = isSendMsgs;
                                                        mainRoleNotifyAllUsersCheckBox.IsChecked = isNotifyAllUsers;
                                                        mainRoleBindAndUnbindStreamsCheckBox.IsChecked = isBindAndUnbindStreams;
                                                        mainRoleKickCheckBox.IsChecked = isKick;
                                                        mainRoleBlockCheckBox.IsChecked = isBlock;
                                                        mainRoleInviteCheckBox.IsChecked = isInvite;
                                                        mainRoleUpdateRolesCheckBox.IsChecked = isUpdateRoles;
                                                        mainRoleAssignRolesCheckBox.IsChecked = isAssignRoles;
                                                        mainRoleUpdateTalkTitleSloganAndAvatarCheckBox.IsChecked = isUpdateTalkTitleSloganAndAvatar;
                                                        mainRoleCreateAndUpdateChannelsCheckBox.IsChecked = isCreateAndUpdateChannels;

                                                        mainRoleTabItem.DataContext = id;
                                                        if (isCustomRole)
                                                        {
                                                            removeRoleBtn.Visibility = Visibility.Visible;
                                                        }
                                                        else
                                                        {
                                                            removeRoleBtn.Visibility = Visibility.Collapsed;
                                                        }

                                                        permissionsControl.SelectedIndex = 2;
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            MessageBox.Show("У вас нет разрешения обновлять роли. Обратитесь к владельцу беседы.", "Внимание");
                                        }
                                    }
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

        private void CancelUpdateRolesHandler(object sender, MouseButtonEventArgs e)
        {
            CancelUpdateRoles();
        }

        public void CancelUpdateRoles ()
        {
            GetRoles();
            permissionsControl.SelectedIndex = 1;
        }

        private void OpenCreatePermissionPopupHandler (object sender, RoutedEventArgs e)
        {
            OpenCreatePermissionPopup();
        }

        public void OpenCreatePermissionPopup()
        {
            try
            {
                HttpWebRequest rolesRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/talks/roles/all");
                rolesRequest.Method = "GET";
                rolesRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse rolesResponse = (HttpWebResponse)rolesRequest.GetResponse())
                {
                    using (StreamReader rolesReader = new StreamReader(rolesResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        string objText = rolesReader.ReadToEnd();
                        TalkRolesResponseInfo myRolesObj = (TalkRolesResponseInfo)js.Deserialize(objText, typeof(TalkRolesResponseInfo));
                        string status = myRolesObj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
                            List<Role> talkRoles = myRolesObj.roles;
                            HttpWebRequest roleRelationsWebRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/talks/roles/relations/all");
                            roleRelationsWebRequest.Method = "GET";
                            roleRelationsWebRequest.UserAgent = ".NET Framework Test Client";
                            using (HttpWebResponse roleRelationsWebResponse = (HttpWebResponse)roleRelationsWebRequest.GetResponse())
                            {
                                using (StreamReader roleRelationsReader = new StreamReader(roleRelationsWebResponse.GetResponseStream()))
                                {
                                    js = new JavaScriptSerializer();
                                    objText = roleRelationsReader.ReadToEnd();
                                    TalkRoleRelationsResponseInfo myRoleRelationsObj = (TalkRoleRelationsResponseInfo)js.Deserialize(objText, typeof(TalkRoleRelationsResponseInfo));
                                    status = myRoleRelationsObj.status;
                                    isOkStatus = status == "OK";
                                    if (isOkStatus)
                                    {
                                        List<TalkRoleRelation> talkRoleRelations = myRoleRelationsObj.relations;
                                        List<TalkRoleRelation> currentTalkRoleRelations = talkRoleRelations.Where<TalkRoleRelation>((TalkRoleRelation talkRoleRelation) =>
                                        {
                                            string talkRoleRelationRoleId = talkRoleRelation.role;
                                            string talkRoleRelationTalkId = talkRoleRelation.talk;
                                            string talkRoleRelationUserId = talkRoleRelation.user;
                                            bool isCurrentUser = talkRoleRelationUserId == currentUserId;
                                            bool isCurrentTalk = talkRoleRelationTalkId == talkId;
                                            bool isCurrentTalkRoleRelation = isCurrentUser && isCurrentTalk;
                                            return isCurrentTalkRoleRelation;
                                        }).ToList<TalkRoleRelation>();
                                        List<string> currentTalkRoleRelationsRoles = new List<string>();
                                        foreach (TalkRoleRelation currentTalkRoleRelation in currentTalkRoleRelations)
                                        {
                                            string currentTalkRoleRelationRoleId = currentTalkRoleRelation.role;
                                            currentTalkRoleRelationsRoles.Add(currentTalkRoleRelationRoleId);
                                        }
                                        List<Role> myRoles = talkRoles.Where<Role>((Role talkRole) =>
                                        {
                                            string talkRoleId = talkRole._id;
                                            bool isRoleFound = currentTalkRoleRelationsRoles.Contains(talkRoleId);
                                            bool isCanAssignRoles = talkRole.assignRoles;
                                            bool isRoleMatch = isRoleFound && isCanAssignRoles;
                                            return isRoleMatch;
                                        }).ToList<Role>();
                                        int myRolesCount = myRoles.Count;
                                        bool isHaveRoles = myRolesCount >= 1;
                                        Talk talk = GetTalkInfo();
                                        string talkOwner = talk.owner;
                                        bool isOwner = currentUserId == talkOwner;
                                        bool isCanCreateRoles = isHaveRoles || isOwner;
                                        if (isCanCreateRoles)
                                        {
                                            createPermissionPopup.IsOpen = true;
                                        }
                                        else
                                        {
                                            MessageBox.Show("У вас нет разрешения создавать новые роли. Обратитесь к владельцу беседы.", "Внимание");
                                        }
                                    }
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
            MenuItem roleInnerContextMenuItem;
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
                            HttpWebRequest innerWebRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/talks/relations/all");
                            innerWebRequest.Method = "GET";
                            innerWebRequest.UserAgent = ".NET Framework Test Client";
                            using (HttpWebResponse innerWebResponse = (HttpWebResponse)innerWebRequest.GetResponse())
                            {
                                using (StreamReader innerReader = new StreamReader(innerWebResponse.GetResponseStream()))
                                {
                                    js = new JavaScriptSerializer();
                                    objText = innerReader.ReadToEnd();
                                    TalkRelationsResponseInfo myInnerObj = (TalkRelationsResponseInfo)js.Deserialize(objText, typeof(TalkRelationsResponseInfo));
                                    status = myInnerObj.status;
                                    isOkStatus = status == "OK";
                                    if (isOkStatus)
                                    {
                                        List<Role> talkRoles = myObj.roles;
                                        List<TalkRelation> relations = myInnerObj.relations;
                                        foreach (Role talkRole in talkRoles)
                                        {
                                            string roleId = talkRole._id;
                                            string roleTitle = talkRole.title;
                                            string roleTalk = talkRole.talk;
                                            bool isCurrentTalk = roleTalk == talkId;
                                            if (isCurrentTalk)
                                            {
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
                                                ContextMenu roleContextMenu = new ContextMenu();
                                                MenuItem roleContextMenuItem = new MenuItem();
                                                roleContextMenuItem.Header = "Добавить";
                                                HttpWebRequest rolesRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/talks/roles/all");
                                                rolesRequest.Method = "GET";
                                                rolesRequest.UserAgent = ".NET Framework Test Client";
                                                using (HttpWebResponse rolesResponse = (HttpWebResponse)rolesRequest.GetResponse())
                                                {
                                                    using (StreamReader rolesReader = new StreamReader(rolesResponse.GetResponseStream()))
                                                    {
                                                        js = new JavaScriptSerializer();
                                                        objText = rolesReader.ReadToEnd();
                                                        TalkRolesResponseInfo myRolesObj = (TalkRolesResponseInfo)js.Deserialize(objText, typeof(TalkRolesResponseInfo));
                                                        status = myRolesObj.status;
                                                        isOkStatus = status == "OK";
                                                        if (isOkStatus)
                                                        {
                                                            HttpWebRequest roleRelationsWebRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/talks/roles/relations/all");
                                                            roleRelationsWebRequest.Method = "GET";
                                                            roleRelationsWebRequest.UserAgent = ".NET Framework Test Client";
                                                            using (HttpWebResponse roleRelationsWebResponse = (HttpWebResponse)roleRelationsWebRequest.GetResponse())
                                                            {
                                                                using (StreamReader roleRelationsReader = new StreamReader(roleRelationsWebResponse.GetResponseStream()))
                                                                {
                                                                    js = new JavaScriptSerializer();
                                                                    objText = roleRelationsReader.ReadToEnd();
                                                                    TalkRoleRelationsResponseInfo myRoleRelationsObj = (TalkRoleRelationsResponseInfo)js.Deserialize(objText, typeof(TalkRoleRelationsResponseInfo));
                                                                    status = myRoleRelationsObj.status;
                                                                    isOkStatus = status == "OK";
                                                                    if (isOkStatus)
                                                                    {
                                                                        List<TalkRoleRelation> talkRoleRelations = myRoleRelationsObj.relations;
                                                                        List<TalkRoleRelation> currentTalkRoleRelations = talkRoleRelations.Where<TalkRoleRelation>((TalkRoleRelation talkRoleRelation) =>
                                                                        {
                                                                            string talkRoleRelationRoleId = talkRoleRelation.role;
                                                                            string talkRoleRelationTalkId = talkRoleRelation.talk;
                                                                            string talkRoleRelationUserId = talkRoleRelation.user;
                                                                            bool isCurrentUser = talkRoleRelationUserId == currentUserId;
                                                                            bool isLocalCurrentTalk = talkRoleRelationTalkId == talkId;
                                                                            bool isCurrentTalkRoleRelation = isCurrentUser && isLocalCurrentTalk;
                                                                            return isCurrentTalkRoleRelation;
                                                                        }).ToList<TalkRoleRelation>();
                                                                        List<string> currentTalkRoleRelationsRoles = new List<string>();
                                                                        foreach (TalkRoleRelation currentTalkRoleRelation in currentTalkRoleRelations)
                                                                        {
                                                                            string currentTalkRoleRelationRoleId = currentTalkRoleRelation.role;
                                                                            currentTalkRoleRelationsRoles.Add(currentTalkRoleRelationRoleId);
                                                                        }
                                                                        List<Role> myRoles = talkRoles.Where<Role>((Role localTalkRole) =>
                                                                        {
                                                                            string talkRoleId = localTalkRole._id;
                                                                            bool isRoleFound = currentTalkRoleRelationsRoles.Contains(talkRoleId);
                                                                            bool isCanInvite = localTalkRole.invite;
                                                                            bool isRoleMatch = isRoleFound && isCanInvite;
                                                                            return isRoleMatch;
                                                                        }).ToList<Role>();
                                                                        int myRolesCount = myRoles.Count;
                                                                        bool isHaveRoles = myRolesCount >= 1;
                                                                        Talk talk = GetTalkInfo();
                                                                        string talkOwner = talk.owner;
                                                                        bool isOwner = currentUserId == talkOwner;
                                                                        bool isCanInviteUsers = isHaveRoles || isOwner;
                                                                        if (isCanInviteUsers)
                                                                        {
                                                                            foreach (TalkRelation relation in relations)
                                                                            {
                                                                                string talkRelationTalkId = relation.talk;
                                                                                bool isUserForCurrentTalk = talkRelationTalkId == talkId;
                                                                                if (isUserForCurrentTalk)
                                                                                {
                                                                                    talk = GetTalkInfo();
                                                                                    talkOwner = talk.owner;
                                                                                    string talkRelationUserId = relation.user;
                                                                                    bool isNotOwner = talkOwner != talkRelationUserId;
                                                                                    if (isNotOwner)
                                                                                    {
                                                                                        bool isNotMe = currentUserId != talkRelationUserId;
                                                                                        if (isNotMe)
                                                                                        {
                                                                                            HttpWebRequest nestedWebRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/talks/roles/relations/all");
                                                                                            nestedWebRequest.Method = "GET";
                                                                                            nestedWebRequest.UserAgent = ".NET Framework Test Client";
                                                                                            using (HttpWebResponse nestedWebResponse = (HttpWebResponse)nestedWebRequest.GetResponse())
                                                                                            {
                                                                                                using (StreamReader nestedReader = new StreamReader(nestedWebResponse.GetResponseStream()))
                                                                                                {
                                                                                                    js = new JavaScriptSerializer();
                                                                                                    objText = nestedReader.ReadToEnd();
                                                                                                    TalkRoleRelationsResponseInfo myNestedObj = (TalkRoleRelationsResponseInfo)js.Deserialize(objText, typeof(TalkRoleRelationsResponseInfo));
                                                                                                    status = myNestedObj.status;
                                                                                                    isOkStatus = status == "OK";
                                                                                                    if (isOkStatus)
                                                                                                    {
                                                                                                        List<TalkRoleRelation> roleRelations = myNestedObj.relations;
                                                                                                        List<TalkRoleRelation> currentRoleRelations = roleRelations.Where<TalkRoleRelation>((TalkRoleRelation roleRelation) =>
                                                                                                        {
                                                                                                            string localRoleId = roleRelation.role;
                                                                                                            bool isCurrentRoleRelation = localRoleId == roleId;
                                                                                                            return isCurrentRoleRelation;
                                                                                                        }).ToList<TalkRoleRelation>();
                                                                                                        List<string> currentRoleRelationUsers = new List<string>();
                                                                                                        foreach (TalkRoleRelation currentRoleRelation in currentRoleRelations)
                                                                                                        {
                                                                                                            string currentRoleRelationUserId = currentRoleRelation.user;
                                                                                                            currentRoleRelationUsers.Add(currentRoleRelationUserId);
                                                                                                        }
                                                                                                        bool isUserAlreadyHaveRole = currentRoleRelationUsers.Contains(talkRelationUserId);
                                                                                                        bool isUserNotHaveRole = !isUserAlreadyHaveRole;
                                                                                                        if (isUserNotHaveRole)
                                                                                                        {
                                                                                                            HttpWebRequest innerNestedWebRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/users/get/?id=" + talkRelationUserId);
                                                                                                            innerNestedWebRequest.Method = "GET";
                                                                                                            innerNestedWebRequest.UserAgent = ".NET Framework Test Client";
                                                                                                            using (HttpWebResponse innerNestedWebResponse = (HttpWebResponse)innerNestedWebRequest.GetResponse())
                                                                                                            {
                                                                                                                using (StreamReader innerNestedReader = new StreamReader(innerNestedWebResponse.GetResponseStream()))
                                                                                                                {
                                                                                                                    js = new JavaScriptSerializer();
                                                                                                                    objText = innerNestedReader.ReadToEnd();
                                                                                                                    UserResponseInfo myInnerNestedObj = (UserResponseInfo)js.Deserialize(objText, typeof(UserResponseInfo));
                                                                                                                    status = myInnerNestedObj.status;
                                                                                                                    isOkStatus = status == "OK";
                                                                                                                    if (isOkStatus)
                                                                                                                    {
                                                                                                                        User user = myInnerNestedObj.user;
                                                                                                                        string userName = user.name;
                                                                                                                        roleInnerContextMenuItem = new MenuItem();
                                                                                                                        roleInnerContextMenuItem.Header = userName;
                                                                                                                        Dictionary<String, Object> roleInnerContextMenuItemData = new Dictionary<String, Object>();
                                                                                                                        roleInnerContextMenuItemData.Add("role", roleId);
                                                                                                                        roleInnerContextMenuItemData.Add("user", talkRelationUserId);
                                                                                                                        roleInnerContextMenuItem.DataContext = roleInnerContextMenuItemData;
                                                                                                                        roleInnerContextMenuItem.Click += AssignRoleToUserHandler;
                                                                                                                        roleContextMenuItem.Items.Add(roleInnerContextMenuItem);
                                                                                                                    }
                                                                                                                }
                                                                                                            }
                                                                                                        }
                                                                                                    }
                                                                                                }
                                                                                            }
                                                                                        }
                                                                                    }
                                                                                }
                                                                            }
                                                                        }
                                                                        else
                                                                        {
                                                                            roleContextMenuItem.IsEnabled = false;
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                                roleContextMenu.Items.Add(roleContextMenuItem);
                                                roleContextMenuItem = new MenuItem();
                                                roleContextMenuItem.Header = "Удалить";
                                                rolesRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/talks/roles/all");
                                                rolesRequest.Method = "GET";
                                                rolesRequest.UserAgent = ".NET Framework Test Client";
                                                using (HttpWebResponse rolesResponse = (HttpWebResponse)rolesRequest.GetResponse())
                                                {
                                                    using (StreamReader rolesReader = new StreamReader(rolesResponse.GetResponseStream()))
                                                    {
                                                        js = new JavaScriptSerializer();
                                                        objText = rolesReader.ReadToEnd();
                                                        TalkRolesResponseInfo myRolesObj = (TalkRolesResponseInfo)js.Deserialize(objText, typeof(TalkRolesResponseInfo));
                                                        status = myRolesObj.status;
                                                        isOkStatus = status == "OK";
                                                        if (isOkStatus)
                                                        {
                                                            HttpWebRequest roleRelationsWebRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/talks/roles/relations/all");
                                                            roleRelationsWebRequest.Method = "GET";
                                                            roleRelationsWebRequest.UserAgent = ".NET Framework Test Client";
                                                            using (HttpWebResponse roleRelationsWebResponse = (HttpWebResponse)roleRelationsWebRequest.GetResponse())
                                                            {
                                                                using (StreamReader roleRelationsReader = new StreamReader(roleRelationsWebResponse.GetResponseStream()))
                                                                {
                                                                    js = new JavaScriptSerializer();
                                                                    objText = roleRelationsReader.ReadToEnd();
                                                                    TalkRoleRelationsResponseInfo myRoleRelationsObj = (TalkRoleRelationsResponseInfo)js.Deserialize(objText, typeof(TalkRoleRelationsResponseInfo));
                                                                    status = myRoleRelationsObj.status;
                                                                    isOkStatus = status == "OK";
                                                                    if (isOkStatus)
                                                                    {
                                                                        List<TalkRoleRelation> talkRoleRelations = myRoleRelationsObj.relations;
                                                                        List<TalkRoleRelation> currentTalkRoleRelations = talkRoleRelations.Where<TalkRoleRelation>((TalkRoleRelation talkRoleRelation) =>
                                                                        {
                                                                            string talkRoleRelationRoleId = talkRoleRelation.role;
                                                                            string talkRoleRelationTalkId = talkRoleRelation.talk;
                                                                            string talkRoleRelationUserId = talkRoleRelation.user;
                                                                            bool isCurrentUser = talkRoleRelationUserId == currentUserId;
                                                                            bool isLocalCurrentTalk = talkRoleRelationTalkId == talkId;
                                                                            bool isCurrentTalkRoleRelation = isCurrentUser && isLocalCurrentTalk;
                                                                            return isCurrentTalkRoleRelation;
                                                                        }).ToList<TalkRoleRelation>();
                                                                        List<string> currentTalkRoleRelationsRoles = new List<string>();
                                                                        foreach (TalkRoleRelation currentTalkRoleRelation in currentTalkRoleRelations)
                                                                        {
                                                                            string currentTalkRoleRelationRoleId = currentTalkRoleRelation.role;
                                                                            currentTalkRoleRelationsRoles.Add(currentTalkRoleRelationRoleId);
                                                                        }
                                                                        List<Role> myRoles = talkRoles.Where<Role>((Role localTalkRole) =>
                                                                        {
                                                                            string talkRoleId = localTalkRole._id;
                                                                            bool isRoleFound = currentTalkRoleRelationsRoles.Contains(talkRoleId);
                                                                            bool isCanKick = localTalkRole.kick;
                                                                            bool isRoleMatch = isRoleFound && isCanKick;
                                                                            return isRoleMatch;
                                                                        }).ToList<Role>();
                                                                        int myRolesCount = myRoles.Count;
                                                                        bool isHaveRoles = myRolesCount >= 1;
                                                                        Talk talk = GetTalkInfo();
                                                                        string talkOwner = talk.owner;
                                                                        bool isOwner = currentUserId == talkOwner;
                                                                        bool isCanKickUsers = isHaveRoles || isOwner;
                                                                        if (isCanKickUsers)
                                                                        {

                                                                            foreach (TalkRelation relation in relations)
                                                                            {
                                                                                string talkRelationTalkId = relation.talk;
                                                                                bool isUserForCurrentTalk = talkRelationTalkId == talkId;
                                                                                if (isUserForCurrentTalk)
                                                                                {
                                                                                    string talkRelationUserId = relation.user;
                                                                                    bool isNotOwner = talkOwner != talkRelationUserId;
                                                                                    if (isNotOwner)
                                                                                    {
                                                                                        bool isNotMe = currentUserId != talkRelationUserId;
                                                                                        if (isNotMe)
                                                                                        {
                                                                                            HttpWebRequest nestedWebRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/talks/roles/relations/all");
                                                                                            nestedWebRequest.Method = "GET";
                                                                                            nestedWebRequest.UserAgent = ".NET Framework Test Client";
                                                                                            using (HttpWebResponse nestedWebResponse = (HttpWebResponse)nestedWebRequest.GetResponse())
                                                                                            {
                                                                                                using (StreamReader nestedReader = new StreamReader(nestedWebResponse.GetResponseStream()))
                                                                                                {
                                                                                                    js = new JavaScriptSerializer();
                                                                                                    objText = nestedReader.ReadToEnd();
                                                                                                    TalkRoleRelationsResponseInfo myNestedObj = (TalkRoleRelationsResponseInfo)js.Deserialize(objText, typeof(TalkRoleRelationsResponseInfo));
                                                                                                    status = myNestedObj.status;
                                                                                                    isOkStatus = status == "OK";
                                                                                                    if (isOkStatus)
                                                                                                    {
                                                                                                        List<TalkRoleRelation> roleRelations = myNestedObj.relations;
                                                                                                        List<TalkRoleRelation> currentRoleRelations = roleRelations.Where<TalkRoleRelation>((TalkRoleRelation roleRelation) =>
                                                                                                        {
                                                                                                            string localRoleId = roleRelation.role;
                                                                                                            bool isCurrentRoleRelation = localRoleId == roleId;
                                                                                                            return isCurrentRoleRelation;
                                                                                                        }).ToList<TalkRoleRelation>();
                                                                                                        List<string> currentRoleRelationUsers = new List<string>();
                                                                                                        foreach (TalkRoleRelation currentRoleRelation in currentRoleRelations)
                                                                                                        {
                                                                                                            string currentRoleRelationUserId = currentRoleRelation.user;
                                                                                                            currentRoleRelationUsers.Add(currentRoleRelationUserId);
                                                                                                        }
                                                                                                        bool isUserAlreadyHaveRole = currentRoleRelationUsers.Contains(talkRelationUserId);
                                                                                                        if (isUserAlreadyHaveRole)
                                                                                                        {
                                                                                                            int foundIndex = currentRoleRelations.FindIndex((TalkRoleRelation roleRelation) =>
                                                                                                            {
                                                                                                                string localUserId = roleRelation.user;
                                                                                                                bool isCurrentUserRoleRelation = localUserId == talkRelationUserId;
                                                                                                                return isCurrentUserRoleRelation;
                                                                                                            });
                                                                                                            bool isFound = foundIndex >= 0;
                                                                                                            if (isFound)
                                                                                                            {
                                                                                                                TalkRoleRelation currentUserRoleRelation = currentRoleRelations[foundIndex];
                                                                                                                string relationId = currentUserRoleRelation._id;
                                                                                                                HttpWebRequest innerNestedWebRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/users/get/?id=" + talkRelationUserId);
                                                                                                                innerNestedWebRequest.Method = "GET";
                                                                                                                innerNestedWebRequest.UserAgent = ".NET Framework Test Client";
                                                                                                                using (HttpWebResponse innerNestedWebResponse = (HttpWebResponse)innerNestedWebRequest.GetResponse())
                                                                                                                {
                                                                                                                    using (StreamReader innerNestedReader = new StreamReader(innerNestedWebResponse.GetResponseStream()))
                                                                                                                    {
                                                                                                                        js = new JavaScriptSerializer();
                                                                                                                        objText = innerNestedReader.ReadToEnd();
                                                                                                                        UserResponseInfo myInnerNestedObj = (UserResponseInfo)js.Deserialize(objText, typeof(UserResponseInfo));
                                                                                                                        status = myInnerNestedObj.status;
                                                                                                                        isOkStatus = status == "OK";
                                                                                                                        if (isOkStatus)
                                                                                                                        {
                                                                                                                            User user = myInnerNestedObj.user;
                                                                                                                            string userName = user.name;
                                                                                                                            roleInnerContextMenuItem = new MenuItem();
                                                                                                                            roleInnerContextMenuItem.Header = userName;
                                                                                                                            Dictionary<String, Object> roleInnerContextMenuItemData = new Dictionary<String, Object>();
                                                                                                                            roleInnerContextMenuItemData.Add("relation", relationId);
                                                                                                                            roleInnerContextMenuItem.DataContext = roleInnerContextMenuItemData;
                                                                                                                            roleInnerContextMenuItem.Click += RemoveRoleFromUserHandler;
                                                                                                                            roleContextMenuItem.Items.Add(roleInnerContextMenuItem);
                                                                                                                        }
                                                                                                                    }
                                                                                                                }
                                                                                                            }
                                                                                                        }
                                                                                                    }
                                                                                                }
                                                                                            }
                                                                                        }
                                                                                    }
                                                                                }
                                                                            }
                                                                        }
                                                                        else
                                                                        {
                                                                            roleContextMenuItem.IsEnabled = false;
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }

                                                roleContextMenu.Items.Add(roleContextMenuItem);
                                                roleContextMenuItem = new MenuItem();
                                                roleContextMenuItem.Header = "Блокировать";
                                                
                                                /*roleInnerContextMenuItem = new MenuItem();
                                                roleInnerContextMenuItem.Header = "userName";
                                                roleInnerContextMenuItem.DataContext = "";
                                                roleInnerContextMenuItem.Click += RemoveRoleFromUserHandler;
                                                roleContextMenuItem.Items.Add(roleInnerContextMenuItem);*/
                                                
                                                roleContextMenu.Items.Add(roleContextMenuItem);
                                                role.ContextMenu = roleContextMenu;
                                            }
                                        }
                                    }
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

            try
            {
                HttpWebRequest rolesRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/talks/roles/all");
                rolesRequest.Method = "GET";
                rolesRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse rolesResponse = (HttpWebResponse)rolesRequest.GetResponse())
                {
                    using (StreamReader rolesReader = new StreamReader(rolesResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        string objText = rolesReader.ReadToEnd();
                        TalkRolesResponseInfo myRolesObj = (TalkRolesResponseInfo)js.Deserialize(objText, typeof(TalkRolesResponseInfo));
                        string status = myRolesObj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
                            List<Role> talkRoles = myRolesObj.roles;
                            HttpWebRequest roleRelationsWebRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/talks/roles/relations/all");
                            roleRelationsWebRequest.Method = "GET";
                            roleRelationsWebRequest.UserAgent = ".NET Framework Test Client";
                            using (HttpWebResponse roleRelationsWebResponse = (HttpWebResponse)roleRelationsWebRequest.GetResponse())
                            {
                                using (StreamReader roleRelationsReader = new StreamReader(roleRelationsWebResponse.GetResponseStream()))
                                {
                                    js = new JavaScriptSerializer();
                                    objText = roleRelationsReader.ReadToEnd();
                                    TalkRoleRelationsResponseInfo myRoleRelationsObj = (TalkRoleRelationsResponseInfo)js.Deserialize(objText, typeof(TalkRoleRelationsResponseInfo));
                                    status = myRoleRelationsObj.status;
                                    isOkStatus = status == "OK";
                                    if (isOkStatus)
                                    {
                                        List<TalkRoleRelation> talkRoleRelations = myRoleRelationsObj.relations;
                                        List<TalkRoleRelation> currentTalkRoleRelations = talkRoleRelations.Where<TalkRoleRelation>((TalkRoleRelation talkRoleRelation) =>
                                        {
                                            string talkRoleRelationRoleId = talkRoleRelation.role;
                                            string talkRoleRelationTalkId = talkRoleRelation.talk;
                                            string talkRoleRelationUserId = talkRoleRelation.user;
                                            bool isCurrentUser = talkRoleRelationUserId == currentUserId;
                                            bool isCurrentTalk = talkRoleRelationTalkId == talkId;
                                            bool isCurrentTalkRoleRelation = isCurrentUser && isCurrentTalk;
                                            return isCurrentTalkRoleRelation;
                                        }).ToList<TalkRoleRelation>();
                                        List<string> currentTalkRoleRelationsRoles = new List<string>();
                                        foreach (TalkRoleRelation currentTalkRoleRelation in currentTalkRoleRelations)
                                        {
                                            string currentTalkRoleRelationRoleId = currentTalkRoleRelation.role;
                                            currentTalkRoleRelationsRoles.Add(currentTalkRoleRelationRoleId);
                                        }
                                        List<Role> myRoles = talkRoles.Where<Role>((Role talkRole) =>
                                        {
                                            string talkRoleId = talkRole._id;
                                            bool isRoleFound = currentTalkRoleRelationsRoles.Contains(talkRoleId);
                                            bool isCanEditInfo = talkRole.updateTalkTitleSloganAndAvatar;
                                            bool isRoleMatch = isRoleFound && isCanEditInfo;
                                            return isRoleMatch;
                                        }).ToList<Role>();
                                        int myRolesCount = myRoles.Count;
                                        bool isHaveRoles = myRolesCount >= 1;
                                        Talk talk = GetTalkInfo();
                                        string owner = talk.owner;
                                        bool isOwner = owner == currentUserId;
                                        bool isCanEdit = isHaveRoles || isOwner;
                                        talkTitleBox.IsEnabled = isCanEdit;
                                        talkSloganBox.IsEnabled = isCanEdit;
                                        talkSloganBox.IsEnabled = isCanEdit;
                                        addIconBtn.IsEnabled = isCanEdit;
                                    }
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

        public void RemoveRoleFromUserHandler (object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = ((MenuItem)(sender));
            object data = menuItem.DataContext;
            Dictionary<String, Object> menuItemData = ((Dictionary<String, Object>)(data));
            RemoveRoleFromUser(menuItemData);
        }

        public void RemoveRoleFromUser (Dictionary<String, Object> menuItemData)
        {
            object rawRelationId = menuItemData["relation"];
            string relationId = ((string)(rawRelationId));
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/talks/roles/relations/remove/?id=" + relationId);
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (StreamReader reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        string objText = reader.ReadToEnd();
                        UserResponseInfo myObj = (UserResponseInfo)js.Deserialize(objText, typeof(UserResponseInfo));
                        string status = myObj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
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

        public void AssignRoleToUserHandler (object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = ((MenuItem)(sender));
            object data = menuItem.DataContext;
            Dictionary <String, Object> menuItemData = ((Dictionary<String, Object>)(data));
            AssignRoleToUser(menuItemData);
        }

        public void AssignRoleToUser (Dictionary<String, Object> menuItemData)
        {
            object rawRoleId = menuItemData["role"];
            string roleId = ((string)(rawRoleId));
            object rawUserId = menuItemData["user"];
            string userId = ((string)(rawUserId));
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/talks/roles/relations/create/?id=" + roleId + @"&user=" + userId + @"&talk=" + talkId);
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

        private void RemoveRoleHandler (object sender, RoutedEventArgs e)
        {
            object roleData = mainRoleTabItem.DataContext;
            string id = ((string)(roleData));
            RemoveRole(id);
        }

        public void RemoveRole (string id)
        {
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/talks/roles/remove/?id=" + id);
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
                            GetRoles();
                            CancelUpdateRoles();
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

        private void ToggleMainRoleSettingsHandler(object sender, RoutedEventArgs e)
        {

            ToggleMainRoleSettings();
        }

        public void ToggleMainRoleSettings ()
        {
            object roleData = mainRoleTabItem.DataContext;
            string id = ((string)(roleData));
            object rawIsChecked = mainRoleSendMsgsCheckBox.IsChecked;
            bool isChecked = ((bool)(rawIsChecked));
            string rawSendMsgs = "false";
            if (isChecked)
            {
                rawSendMsgs = "true";
            }
            rawIsChecked = mainRoleNotifyAllUsersCheckBox.IsChecked;
            isChecked = ((bool)(rawIsChecked));
            string rawNotifyAllUsers = "false";
            if (isChecked)
            {
                rawNotifyAllUsers = "true";
            }
            rawIsChecked = mainRoleBindAndUnbindStreamsCheckBox.IsChecked;
            isChecked = ((bool)(rawIsChecked));
            string rawBindAndUnbindStreams = "false";
            if (isChecked)
            {
                rawBindAndUnbindStreams = "true";
            }
            rawIsChecked = mainRoleKickCheckBox.IsChecked;
            isChecked = ((bool)(rawIsChecked));
            string rawKick = "false";
            if (isChecked)
            {
                rawKick = "true";
            }
            rawIsChecked = mainRoleBlockCheckBox.IsChecked;
            isChecked = ((bool)(rawIsChecked));
            string rawBlock = "false";
            if (isChecked)
            {
                rawBlock = "true";
            }
            rawIsChecked = mainRoleInviteCheckBox.IsChecked;
            isChecked = ((bool)(rawIsChecked));
            string rawInvite = "false";
            if (isChecked)
            {
                rawInvite = "true";
            }
            rawIsChecked = mainRoleUpdateRolesCheckBox.IsChecked;
            isChecked = ((bool)(rawIsChecked));
            string rawUpdateRoles = "false";
            if (isChecked)
            {
                rawUpdateRoles = "true";
            }
            rawIsChecked = mainRoleAssignRolesCheckBox.IsChecked;
            isChecked = ((bool)(rawIsChecked));
            string rawAssignRoles = "false";
            if (isChecked)
            {
                rawAssignRoles = "true";
            }
            rawIsChecked = mainRoleUpdateTalkTitleSloganAndAvatarCheckBox.IsChecked;
            isChecked = ((bool)(rawIsChecked));
            string rawUpdateTalkTitleSloganAndAvatar = "false";
            if (isChecked)
            {
                rawUpdateTalkTitleSloganAndAvatar = "true";
            }
            rawIsChecked = mainRoleCreateAndUpdateChannelsCheckBox.IsChecked;
            isChecked = ((bool)(rawIsChecked));
            string rawCreateAndUpdateChannels = "false";
            if (isChecked)
            {
                rawCreateAndUpdateChannels = "true";
            }
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/talks/roles/edit/?id=" + id + @"&sendMsgs=" + rawSendMsgs + @"&notifyAllUsers=" + rawNotifyAllUsers+ @"&bindAndUnbindStreams=" + rawBindAndUnbindStreams+ @"&kick=" + rawKick + @"&block=" + rawBlock + @"&invite=" + rawInvite + @"&updateRoles=" + rawUpdateRoles + @"&assignRoles=" + rawAssignRoles + @"&updateTalkTitleSloganAndAvatar=" + rawUpdateTalkTitleSloganAndAvatar + @"&createAndUpdateChannels=" + rawCreateAndUpdateChannels);
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
