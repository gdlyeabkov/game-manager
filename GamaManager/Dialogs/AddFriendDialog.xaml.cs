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
    /// Логика взаимодействия для AddFriendDialog.xaml
    /// </summary>
    public partial class AddFriendDialog : Window
    {

        public string currentUserId = "";
        public List<string> usersIds;
        private User currentUser = null;
        public Brush disabledColor;

        public AddFriendDialog(string currentUserId)
        {
            InitializeComponent();

            Initialize(currentUserId);

        }

        public void Initialize (string currentUserId)
        {
            InitializeConstants(currentUserId);
            GetUser();
            GetUsers();
        }

        public void InitializeConstants(string currentUserId)
        {
            this.currentUserId = currentUserId;
            disabledColor = System.Windows.Media.Brushes.LightGray;
        }

        public void GetUser ()
        {
            HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("https://digitaldistributtionservice.herokuapp.com/api/users/get/?id=" + currentUserId);
            webRequest.Method = "GET";
            webRequest.UserAgent = ".NET Framework Test Client";
            using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
            {
                using (StreamReader reader = new StreamReader(webResponse.GetResponseStream()))
                {
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    var objText = reader.ReadToEnd();

                    UserResponseInfo myobj = (UserResponseInfo)js.Deserialize(objText, typeof(UserResponseInfo));

                    string status = myobj.status;
                    bool isOkStatus = status == "OK";
                    if (isOkStatus)
                    {
                        currentUser = myobj.user;
                    }
                }
            }
        }

        public void GetUsers ()
        {
            HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("https://digitaldistributtionservice.herokuapp.com/api/users/all");
            webRequest.Method = "GET";
            webRequest.UserAgent = ".NET Framework Test Client";
            using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
            {
                using (var reader = new StreamReader(webResponse.GetResponseStream()))
                {
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    var objText = reader.ReadToEnd();

                    UsersResponseInfo myobj = (UsersResponseInfo)js.Deserialize(objText, typeof(UsersResponseInfo));

                    string status = myobj.status;
                    bool isOkStatus = status == "OK";
                    if (isOkStatus)
                    {
                        usersIds = new List<string>();
                        foreach (User user in myobj.users)
                        {
                            string userId = user._id;
                            bool isMe = userId == currentUserId;
                            bool isNotMe = !isMe;
                            if (isNotMe)
                            {
                                StackPanel usersItem = new StackPanel();
                                usersItem.Orientation = Orientation.Horizontal;
                                usersItem.Height = 35;
                                usersItem.Margin = new Thickness(0, 5, 0, 5);
                                string userLogin = user.login;
                                Uri userAvatarUri = new Uri("https://cdn2.iconfinder.com/data/icons/ios-7-icons/50/user_male-128.png");
                                Image userAvatar = new Image();
                                userAvatar.Width = 25;
                                userAvatar.Height = 25;
                                userAvatar.Margin = new Thickness(5);
                                userAvatar.BeginInit();
                                userAvatar.Source = new BitmapImage(userAvatarUri);
                                userAvatar.EndInit();
                                usersItem.Children.Add(userAvatar);
                                TextBlock usersItemLoginLabel = new TextBlock();
                                usersItemLoginLabel.Margin = new Thickness(5, 5, 5, 5);
                                usersItemLoginLabel.Text = userLogin;
                                usersItem.Children.Add(usersItemLoginLabel);
                                users.Children.Add(usersItem);
                                usersItem.DataContext = userId;
                                List<FriendResponseInfo> myFriends = currentUser.friends;
                                List<string> friendsIds = new List<string>();
                                foreach (FriendResponseInfo myFriend in myFriends)
                                {
                                    string friendId = myFriend.id;
                                    friendsIds.Add(friendId);
                                }
                                bool isMyFriend = friendsIds.Contains(userId);
                                if (isMyFriend)
                                {
                                    usersItemLoginLabel.Foreground = disabledColor;
                                }
                                else
                                {
                                    usersItem.MouseLeftButtonUp += ShowFriendCodeHandler;
                                }
                                usersIds.Add(userId);
                            }
                        }
                    }
                }
            }
        }

        private void SendFriendRequestHandler (object sender, RoutedEventArgs e)
        {
            string userId = friendCodeLabel.Text;
            SendFriendRequest(userId);
        }

        private void SendFriendRequest (string friendId)
        {

            /*
            HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("https://digitaldistributtionservice.herokuapp.com/api/friends/add/?id=" + currentUserId + @"&friend=" + friendId);
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
                        webRequest = (HttpWebRequest)HttpWebRequest.Create("https://digitaldistributtionservice.herokuapp.com/api/users/get/?id=" + friendId);
                        webRequest.Method = "GET";
                        webRequest.UserAgent = ".NET Framework Test Client";
                        using (HttpWebResponse innerWebResponse = (HttpWebResponse)webRequest.GetResponse())
                        {
                            using (StreamReader innerReader = new StreamReader(innerWebResponse.GetResponseStream()))
                            {
                                js = new JavaScriptSerializer();
                                objText = innerReader.ReadToEnd();

                                myobj = (UserResponseInfo)js.Deserialize(objText, typeof(UserResponseInfo));

                                status = myobj.status;
                                isOkStatus = status == "OK";
                                if (isOkStatus)
                                {
                                    User friend = myobj.user;
                                    string friendLogin = friend.login;
                                    string msgContent = "Приглашение пользователю " + friendLogin + " было отправлено";
                                    MessageBox.Show(msgContent, "Приглашение отправлено");
                                    this.Close();
                                }
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("Не удалось отправить приглашение", "Ошибка");
                    }
                }
            }
            */
            HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("https://digitaldistributtionservice.herokuapp.com/api/friends/requests/add/?id=" + currentUserId + @"&friend=" + friendId);
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
                        webRequest = (HttpWebRequest)HttpWebRequest.Create("https://digitaldistributtionservice.herokuapp.com/api/users/get/?id=" + friendId);
                        webRequest.Method = "GET";
                        webRequest.UserAgent = ".NET Framework Test Client";
                        using (HttpWebResponse innerWebResponse = (HttpWebResponse)webRequest.GetResponse())
                        {
                            using (StreamReader innerReader = new StreamReader(innerWebResponse.GetResponseStream()))
                            {
                                js = new JavaScriptSerializer();
                                objText = innerReader.ReadToEnd();

                                myobj = (UserResponseInfo)js.Deserialize(objText, typeof(UserResponseInfo));

                                status = myobj.status;
                                isOkStatus = status == "OK";
                                if (isOkStatus)
                                {
                                    User friend = myobj.user;
                                    string friendLogin = friend.login;
                                    string msgContent = "Приглашение пользователю " + friendLogin + " было отправлено";
                                    MessageBox.Show(msgContent, "Приглашение отправлено");
                                    this.Close();
                                }
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("Не удалось отправить приглашение", "Ошибка");
                    }
                }
            }
        }

        public void ShowFriendCodeHandler (object sender, RoutedEventArgs e)
        {
            StackPanel user = ((StackPanel)(sender));
            object rawUserId = user.DataContext;
            string userId = ((string)(rawUserId));
            ShowFriendCode(userId);
        }

        public void ShowFriendCode (string userId)
        {
            friendCodeLabel.Text = userId;
        }

        private void DetectFriendCodeContentHandler (object sender, TextChangedEventArgs e)
        {
            DetectFriendCodeContent();
        }

        public void DetectFriendCodeContent()
        {
            string friendCodeLabelContent = friendCodeLabel.Text;
            int friendCodeLabelContentLength = friendCodeLabelContent.Length;
            bool isContentExists = friendCodeLabelContentLength >= 1;
            bool isUserIdSetted = usersIds.Contains(friendCodeLabelContent);
            bool isFriendCodeSetted = isContentExists && isUserIdSetted;
            if (isFriendCodeSetted)
            {
                sendFriendRequestBtn.IsEnabled = true;
            }
            else
            {
                sendFriendRequestBtn.IsEnabled = false;
            }
        }

    }

    class UsersResponseInfo
    {
        public string status;
        public List<User> users;
    }

}
