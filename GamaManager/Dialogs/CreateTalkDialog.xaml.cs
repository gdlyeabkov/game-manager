using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
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
    /// Логика взаимодействия для CreateTalkDialog.xaml
    /// </summary>
    public partial class CreateTalkDialog : Window
    {

        public string currentUserId = "";

        public CreateTalkDialog(string currentUserId)
        {
            InitializeComponent();

            Initialize(currentUserId);
        
        }

        public void Initialize(string currentUserId)
        {
            this.currentUserId = currentUserId;
            GetFriends();
        }

        private void FilterFriendsHandler (object sender, TextChangedEventArgs e)
        {
            FilterFriends();
        }

        public void FilterFriends ()
        {
            GetFriends();
        }

        public void GetFriends ()
        {
            string keywords = filterBox.Text;
            string insensitiveCaseKeywords = keywords.ToLower();
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/users/get/?id=" + currentUserId);
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
                            friends.Children.Clear();
                            User currentUser = myobj.user;
                            webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/friends/get");
                            webRequest.Method = "GET";
                            webRequest.UserAgent = ".NET Framework Test Client";
                            using (HttpWebResponse nestedWebResponse = (HttpWebResponse)webRequest.GetResponse())
                            {
                                using (var nestedReader = new StreamReader(nestedWebResponse.GetResponseStream()))
                                {
                                    js = new JavaScriptSerializer();
                                    objText = nestedReader.ReadToEnd();
                                    FriendsResponseInfo myInnerObj = (FriendsResponseInfo)js.Deserialize(objText, typeof(FriendsResponseInfo));
                                    status = myobj.status;
                                    isOkStatus = status == "OK";
                                    if (isOkStatus)
                                    {
                                        List<Friend> receivedFriends = myInnerObj.friends;
                                        List<Friend> myFriends = receivedFriends.Where<Friend>((Friend friend) =>
                                        {
                                            return friend.user == currentUserId;
                                        }).ToList<Friend>();
                                        List<string> friendsIds = new List<string>();
                                        foreach (Friend friendInfo in myFriends)
                                        {
                                            string friendId = friendInfo.friend;
                                            friendsIds.Add(friendId);
                                        }
                                        foreach (Friend myFriend in myFriends)
                                        {
                                            string userId = myFriend.friend;
                                            HttpWebRequest innerWebRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/users/get/?id=" + userId);
                                            innerWebRequest.Method = "GET";
                                            innerWebRequest.UserAgent = ".NET Framework Test Client";
                                            using (HttpWebResponse innerWebResponse = (HttpWebResponse)innerWebRequest.GetResponse())
                                            {
                                                using (var innerReader = new StreamReader(innerWebResponse.GetResponseStream()))
                                                {
                                                    js = new JavaScriptSerializer();
                                                    objText = innerReader.ReadToEnd();
                                                    UserResponseInfo myNestedObj = (UserResponseInfo)js.Deserialize(objText, typeof(UserResponseInfo));
                                                    status = myNestedObj.status;
                                                    isOkStatus = status == "OK";
                                                    if (isOkStatus)
                                                    {
                                                        User user = myNestedObj.user;
                                                        string friendName = user.name;
                                                        string insensitiveCaseFriendName = friendName.ToLower();
                                                        string friendStatus = user.status;
                                                        List<String>  friendsRequests = new List<String>();
                                                        foreach (Border request in requests.Children)
                                                        {
                                                            object requestData = request.DataContext;
                                                            string id = ((string)(requestData));
                                                            friendsRequests.Add(id);
                                                        }
                                                        bool isFriendInRequests = friendsRequests.Contains(userId);
                                                        bool isFriendNotInRequests = !isFriendInRequests;
                                                        int insensitiveCaseKeywordsLength = insensitiveCaseKeywords.Length;
                                                        bool isFilterDisabled = insensitiveCaseKeywordsLength <= 0;
                                                        bool isKeywordsMatch = insensitiveCaseFriendName.Contains(insensitiveCaseKeywords);
                                                        bool isAddFriend = (isFilterDisabled || isKeywordsMatch) && isFriendNotInRequests;
                                                        if (isAddFriend)
                                                        {
                                                            StackPanel friend = new StackPanel();
                                                            friend.Orientation = Orientation.Horizontal;
                                                            friend.Height = 65;
                                                            Image friendAvatar = new Image();
                                                            friendAvatar.Width = 35;
                                                            friendAvatar.Height = 35;
                                                            friendAvatar.Margin = new Thickness(10);
                                                            friendAvatar.BeginInit();
                                                            friendAvatar.Source = new BitmapImage(new Uri(@"http://localhost:4000/api/user/avatar/?id=" + userId));
                                                            friendAvatar.EndInit();
                                                            friendAvatar.ImageFailed += SetDefaultAvatarHandler;
                                                            friend.Children.Add(friendAvatar);
                                                            StackPanel friendAside = new StackPanel();
                                                            friendAside.Margin = new Thickness(15, 0, 15, 0);
                                                            TextBlock friendAsideNameLabel = new TextBlock();
                                                            friendAsideNameLabel.Text = friendName;
                                                            friendAsideNameLabel.Margin = new Thickness(0, 5, 0, 5);
                                                            friendAside.Children.Add(friendAsideNameLabel);
                                                            TextBlock friendAsideStatusLabel = new TextBlock();
                                                            friendAsideStatusLabel.Text = friendStatus;
                                                            friendAsideStatusLabel.Margin = new Thickness(0, 5, 0, 5);
                                                            friendAside.Children.Add(friendAsideStatusLabel);
                                                            friend.Children.Add(friendAside);
                                                            friends.Children.Add(friend);
                                                            friend.DataContext = userId;
                                                            friend.MouseLeftButtonUp += InviteFriendHandler;
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
            catch (System.Net.WebException)
            {
                MessageBox.Show("Не удается подключиться к серверу", "Ошибка");
                this.Close();
            }
        }

        public void InviteFriendHandler (object sender, RoutedEventArgs e)
        {
            StackPanel friend = ((StackPanel)(sender));
            object friendData = friend.DataContext;
            string friendId = ((string)(friendData));
            InviteFriend(friendId, friend);
        }

        public void InviteFriend (string friendId, StackPanel friend)
        {
            friends.Children.Remove(friend);
            requests.Visibility = Visibility.Visible;
            filterBox.Visibility = Visibility.Collapsed;
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/users/get/?id=" + friendId);
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
                            User user = myobj.user;
                            string friendName = user.name;
                            Border request = new Border();
                            request.Margin = new Thickness(15, 0, 15, 0);
                            request.CornerRadius = new CornerRadius(5);
                            request.Background = System.Windows.Media.Brushes.LightGray;
                            StackPanel requestBody = new StackPanel();
                            requestBody.Orientation = Orientation.Horizontal;
                            TextBlock requestBodyLabel = new TextBlock();
                            requestBodyLabel.Text = friendName;
                            requestBodyLabel.Margin = new Thickness(15, 5, 15, 5);
                            requestBody.Children.Add(requestBodyLabel);
                            PackIcon requestBodyIcon = new PackIcon();
                            requestBodyIcon.Kind = PackIconKind.Close;
                            requestBodyIcon.Width = 15;
                            requestBodyIcon.Height = 15;
                            requestBodyIcon.Margin = new Thickness(15, 5, 15, 5);
                            requestBody.Children.Add(requestBodyIcon);
                            request.Child = requestBody;
                            requests.Children.Add(request);
                            request.DataContext = friendId;
                            request.MouseLeftButtonUp += RemoveRequestHandler;
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

        public void RemoveRequestHandler (object sender, RoutedEventArgs e)
        {
            Border friend = ((Border)(sender));
            object friendData = friend.DataContext;
            string friendId = ((string)(friendData));
            RemoveRequest(friend);
        }

        public void RemoveRequest (Border friend)
        {
            requests.Children.Remove(friend);
            UIElementCollection requestsChildren = requests.Children;
            int requestsChildrenCount = requestsChildren.Count;
            bool isNotHaveRequests = requestsChildrenCount <= 0;
            if (isNotHaveRequests)
            {
                filterBox.Visibility = Visibility.Visible;
                requests.Visibility = Visibility.Collapsed;
            }
            GetFriends();
        }

        private void DetectChatNameHandler (object sender, TextChangedEventArgs e)
        {
            DetectChatName();
        }

        public void DetectChatName ()
        {
            string chatNameBoxContent = chatNameBox.Text;
            int chatNameBoxContentLength = chatNameBoxContent.Length;
            bool isNameNotExists = chatNameBoxContentLength <= 0;
            bool isDisabled = !isNameNotExists;
            createTalkBtn.IsEnabled = isDisabled;
        }

        private void CancelHandler (object sender, RoutedEventArgs e)
        {
            Cancel();
        }

        public void Cancel ()
        {
            this.Close();
        }

        private void CreateTalkHandler (object sender, RoutedEventArgs e)
        {
            CreateTalk();
        }

        public void CreateTalk ()
        {
            string chatNameBoxContent = chatNameBox.Text;
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/talks/create/?title=" + chatNameBoxContent + @"&owner=" + currentUserId);
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (var reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        var objText = reader.ReadToEnd();
                        TalkCreateResponseInfo myobj = (TalkCreateResponseInfo)js.Deserialize(objText, typeof(TalkCreateResponseInfo));
                        string status = myobj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
                            string talkId = myobj.id;
                            SendInvites(talkId);
                            Cancel();

                            Environment.SpecialFolder localApplicationDataFolder = Environment.SpecialFolder.LocalApplicationData;
                            string localApplicationDataFolderPath = Environment.GetFolderPath(localApplicationDataFolder);
                            string saveDataFilePath = localApplicationDataFolderPath + @"\OfficeWare\GameManager\" + currentUserId + @"\save-data.txt";
                            js = new JavaScriptSerializer();
                            string saveDataFileContent = File.ReadAllText(saveDataFilePath);
                            SavedContent loadedContent = js.Deserialize<SavedContent>(saveDataFileContent);
                            List<Game> currentGames = loadedContent.games;
                            Settings currentSettings = loadedContent.settings;
                            List<FriendSettings> currentFriends = loadedContent.friends;
                            List<string> currentCollections = loadedContent.collections;
                            Notifications currentNotifications = loadedContent.notifications;
                            List<string> currentCategories = loadedContent.categories;
                            List<string> currentRecentChats = loadedContent.recentChats;
                            List<FriendSettings> updatedFriends = currentFriends;
                            updatedFriends.Add(new FriendSettings()
                            {
                                id = talkId,
                                isFriendOnlineNotification = true,
                                isFriendOnlineSound = true,
                                isFriendPlayedNotification = true,
                                isFriendPlayedSound = true,
                                isFriendSendMsgNotification = true,
                                isFriendSendMsgSound = true,
                                isFavoriteFriend = false,
                                categories = new List<string>()
                            });
                            string savedContent = js.Serialize(new SavedContent
                            {
                                games = currentGames,
                                friends = updatedFriends,
                                settings = currentSettings,
                                collections = currentCollections,
                                notifications = currentNotifications,
                                categories = currentCategories,
                                recentChats = currentRecentChats
                            });
                            File.WriteAllText(saveDataFilePath, savedContent);

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

        public void SendInvites (string talkId)
        {
            foreach (Border request in requests.Children)
            {
                object requestData = request.DataContext;
                string friendId = ((string)(requestData));
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/users/get/?id=" + friendId);
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
                            User user = myobj.user;
                            string email = user.login;
                            /*
                             * отправка приглашения на почту
                            MailMessage message = new MailMessage();
                            SmtpClient smtp = new SmtpClient();
                            message.From = new System.Net.Mail.MailAddress("glebdyakov2000@gmail.com");
                            message.To.Add(new System.Net.Mail.MailAddress(email));
                            string subjectBoxContent = @"Приглашение в беседу Office ware game manager";
                            message.Subject = subjectBoxContent;
                            message.IsBodyHtml = true; //to make message body as html  
                            string messageBodyBoxContent = "<h3>Здравствуйте, " + email + "!</h3><p>Вам предлагают вступить в беседу \"\"</p><a href=\"http://localhost:4000/api/talks/relations/add/?id=" + talkId + "&user=" + friendId + "\">Вступить</a>\"";
                            message.Body = messageBodyBoxContent;
                            smtp.Port = 587;
                            smtp.Host = "smtp.gmail.com"; //for gmail host  
                            smtp.EnableSsl = true;
                            smtp.UseDefaultCredentials = false;
                            smtp.Credentials = new NetworkCredential("glebdyakov2000@gmail.com", "ttolpqpdzbigrkhz");
                            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                            smtp.Send(message);
                            */
                            string newMsgType = "link";
                            // string newMsgCntent = "\"http://localhost:4000/api/talks/relations/add/?id=" + talkId + "&user=" + friendId + "\"";
                            string newMsgCntent = talkId;
                            HttpWebRequest innerWebRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/msgs/add/?user=" + currentUserId + "&friend=" + friendId + "&content=" + newMsgCntent + "&type=" + newMsgType + "&channel=mockChannelId");
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
                                        Cancel();
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }


        public void SetDefaultAvatar(Image avatar)
        {
            avatar.BeginInit();
            avatar.Source = new BitmapImage(new Uri(@"https://cdn2.iconfinder.com/data/icons/ios-7-icons/50/user_male-128.png"));
            avatar.EndInit();
        }

    }
}
