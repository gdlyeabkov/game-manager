﻿using SocketIOClient;
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
    /// Логика взаимодействия для FriendsDialog.xaml
    /// </summary>
    public partial class FriendsDialog : Window
    {

        public string currentUserId = "";
        public SocketIO client;

        public FriendsDialog(string currentUserId, SocketIO client)
        {
            InitializeComponent();

            Initialize(currentUserId, client);
        
        }

        public void Initialize (string currentUserId, SocketIO client)
        {
            this.client = client;
            GetFriends(currentUserId, "");
        }

        public void GetFriends (string currentUserId, string keywords)
        {
            this.currentUserId = currentUserId;
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
                                        List<Friend> friendsIds = myInnerObj.friends;
                                        foreach (Friend friendInfo in friendsIds)
                                        {
                                            string friendId = friendInfo.friend;
                                            if (friendId != currentUserId)
                                            {
                                                webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/users/get/?id=" + friendId);
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
                                                            string friendIgnoreCaseLogin = friendLogin.ToLower();
                                                            string ignoreCaseKeywords = keywords.ToLower();
                                                            bool isFriendMatch = friendIgnoreCaseLogin.Contains(ignoreCaseKeywords);
                                                            if (isFriendMatch)
                                                            {
                                                                StackPanel friendsItem = new StackPanel();
                                                                friendsItem.Height = 35;
                                                                friendsItem.Orientation = Orientation.Horizontal;
                                                                Image friendAvatar = new Image();
                                                                friendAvatar.Width = 25;
                                                                friendAvatar.Height = 25;
                                                                friendAvatar.Margin = new Thickness(5);
                                                                friendAvatar.BeginInit();
                                                                Uri friendAvatarUri = new Uri("https://cdn2.iconfinder.com/data/icons/ios-7-icons/50/user_male-128.png");
                                                                friendAvatar.Source = new BitmapImage(friendAvatarUri);
                                                                friendAvatar.EndInit();
                                                                friendsItem.Children.Add(friendAvatar);
                                                                TextBlock friendLoginLabel = new TextBlock();
                                                                friendLoginLabel.Height = 25;
                                                                friendLoginLabel.VerticalAlignment = VerticalAlignment.Center;
                                                                friendLoginLabel.Margin = new Thickness(10, 5, 10, 5);
                                                                friendLoginLabel.Text = friendLogin;
                                                                friendsItem.Children.Add(friendLoginLabel);
                                                                friends.Children.Add(friendsItem);
                                                                ContextMenu friendsItemContextMenu = new ContextMenu();
                                                                MenuItem friendsItemContextMenuItem = new MenuItem();
                                                                friendsItemContextMenuItem.Header = "Отправить сообщение";
                                                                friendsItemContextMenuItem.DataContext = friendId;
                                                                friendsItemContextMenuItem.Click += OpenChatHandler;
                                                                friendsItemContextMenu.Items.Add(friendsItemContextMenuItem); 
                                                                friendsItemContextMenuItem = new MenuItem();
                                                                friendsItemContextMenuItem.Header = "Управление";
                                                                MenuItem innerFriendsItemContextMenuItem = new MenuItem();
                                                                innerFriendsItemContextMenuItem.Header = "Удалить из друзей";
                                                                innerFriendsItemContextMenuItem.DataContext = friendId;
                                                                innerFriendsItemContextMenuItem.Click += RemoveFriendHandler;
                                                                friendsItemContextMenuItem.Items.Add(innerFriendsItemContextMenuItem);
                                                                friendsItemContextMenu.Items.Add(friendsItemContextMenuItem);
                                                                friendsItem.ContextMenu = friendsItemContextMenu;
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
            catch (System.Net.WebException)
            {
                MessageBox.Show("Не удается подключиться к серверу", "Ошибка");
                this.Close();
            }
        }

        public void RemoveFriendHandler (object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = ((MenuItem)(sender));
            object menuItemData = menuItem.DataContext;
            string friendId = ((string)(menuItemData));
            RemoveFriend(friendId);
        }

        public void RemoveFriend (string friendId)
        {
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/friends/remove/?id=" + currentUserId + "&friend=" + friendId);
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
                            MessageBox.Show("Друг был удален", "Внимание");
                            GetFriends(currentUserId, "");
                        }
                        else
                        {
                            MessageBox.Show("Не удается удалить друга", "Ошибка");
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

        private void FilterFriendsHandler (object sender, TextChangedEventArgs e)
        {
            FilterFriends();
        }

        public void FilterFriends()
        {
            string keywords = keywordsLabel.Text;
            GetFriends(currentUserId, keywords);
        }

        private void OpenAddFriendDialogHandler(object sender, RoutedEventArgs e)
        {
            OpenAddFriendDialog();
        }

        public void OpenAddFriendDialog()
        {
            Dialogs.AddFriendDialog dialog = new Dialogs.AddFriendDialog(currentUserId);
            dialog.Show();
        }

        public void OpenChatHandler (object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = ((MenuItem)(sender));
            object menuItemData = menuItem.DataContext;
            string friend = ((string)(menuItemData));
            OpenChat(friend);
        }

        async public void OpenChat (string friend)
        {
            // await client.DisconnectAsync();
            Dialogs.ChatDialog dialog = new Dialogs.ChatDialog(currentUserId, client, friend);
            dialog.Show();
        }

    }
}