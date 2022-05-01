using SocketIOClient;
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
    /// Логика взаимодействия для AddGroupRequestDialog.xaml
    /// </summary>
    public partial class AddGroupRequestDialog : Window
    {

        public string currentUserId = "";
        public string friendId = "";
        public SocketIO client = null;

        public AddGroupRequestDialog(string currentUserId, string friendId, SocketIO client)
        {
            InitializeComponent();

            Initialize(currentUserId, friendId, client);

        }

        public void Initialize(string currentUserId, string friendId, SocketIO client)
        {
            InitConstants(currentUserId, friendId, client);
            GetGroups();
        }

        public void InitConstants(string currentUserId, string friendId, SocketIO client)
        {
            this.currentUserId = currentUserId;
            this.friendId = friendId;
            this.client = client;
        }

        public void GetGroups()
        {
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/groups/all");
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (var reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        var objText = reader.ReadToEnd();
                        GroupsResponseInfo myObj = (GroupsResponseInfo)js.Deserialize(objText, typeof(GroupsResponseInfo));
                        string status = myObj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
                            groups.Children.Clear();
                            List<Group> totalGroups = myObj.groups;
                            foreach (Group group in totalGroups)
                            {
                                string id = group._id;
                                string owner = group.owner;
                                string name = group.name;
                                bool isMyGroup = owner == currentUserId;
                                if (isMyGroup)
                                {
                                    StackPanel localGroup = new StackPanel();
                                    localGroup.Margin = new Thickness(15);
                                    localGroup.Height = 50;
                                    localGroup.Background = System.Windows.Media.Brushes.LightGray;
                                    TextBlock localGroupNameLabel = new TextBlock();
                                    localGroupNameLabel.FontSize = 20;
                                    localGroupNameLabel.Margin = new Thickness(15);
                                    localGroupNameLabel.Text = name;
                                    localGroup.Children.Add(localGroupNameLabel);
                                    groups.Children.Add(localGroup);
                                    localGroup.DataContext = id;
                                    localGroup.MouseLeftButtonUp += AddRequestHandler;
                                }
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

        public void AddRequestHandler(object sender, RoutedEventArgs e)
        {
            StackPanel group = ((StackPanel)(sender));
            object groupData = group.DataContext;
            string id = ((string)(groupData));
            AddRequest(id);
        }

        public void AddRequest (string id)
        {
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/groups/requests/add/?id=" + id + "&user=" + friendId);
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (var reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        var objText = reader.ReadToEnd();
                        GroupsResponseInfo myObj = (GroupsResponseInfo)js.Deserialize(objText, typeof(GroupsResponseInfo));
                        string status = myObj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
                            string eventData = id + "|" + friendId;
                            client.EmitAsync("user_send_friend_request", eventData);
                            this.Close();
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

    }
}
