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
    /// Логика взаимодействия для AddNickDialog.xaml
    /// </summary>
    public partial class AddNickDialog : Window
    {

        public string id = "";

        public AddNickDialog(string id)
        {
            InitializeComponent();
                
            Initialize(id);

        }

        public void Initialize(string id)
        {
            this.id = id;
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/friends/get");
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (var reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        string objText = reader.ReadToEnd();
                        FriendsResponseInfo myObj = (FriendsResponseInfo)js.Deserialize(objText, typeof(FriendsResponseInfo));
                        string status = myObj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
                            List<Friend> friends = myObj.friends;
                            int foundIndex = friends.FindIndex((Friend localFriend) =>
                            {
                                string relationId = localFriend._id;
                                bool isCurrentFriendRelation = relationId == id;
                                return isCurrentFriendRelation;
                            });
                            bool isFound = foundIndex >= 0;
                            if (isFound)
                            {
                                Friend relation = friends[foundIndex];
                                string friendId = relation.friend;
                                friendAvatar.BeginInit();
                                friendAvatar.Source = new BitmapImage(new Uri(@"http://localhost:4000/api/user/avatar/?id=" + friendId));
                                friendAvatar.EndInit();
                                webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/users/get/?id=" + friendId);
                                webRequest.Method = "GET";
                                webRequest.UserAgent = ".NET Framework Test Client";
                                using (HttpWebResponse innerWebResponse = (HttpWebResponse)webRequest.GetResponse())
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
                                            User friend = myInnerObj.user;
                                            string friendName = friend.name;
                                            friendNameLabel.Text = friendName;
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

        private void AcceptHandler (object sender, RoutedEventArgs e)
        {
            Accept();
        }

        public void Accept ()
        {
            string friendAliasBoxContent = friendAliasBox.Text;
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/friend/alias/set/?id=" + id + @"&alias=" + friendAliasBoxContent);
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (var reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        string objText = reader.ReadToEnd();
                        UserResponseInfo myObj = (UserResponseInfo)js.Deserialize(objText, typeof(UserResponseInfo));
                        string status = myObj.status;
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

        private void CancelHandler (object sender, RoutedEventArgs e)
        {
            Cancel();
        }

        public void Cancel ()
        {
            this.Close();
        }

    }
}
