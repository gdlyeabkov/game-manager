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
    /// Логика взаимодействия для ActivityShareDialog.xaml
    /// </summary>
    public partial class ActivityShareDialog : Window
    {

        public string currentUserId = "";
        public string type = "";
        public string id  = "";

        public ActivityShareDialog (string currentUserId, string type, string id)
        {
            InitializeComponent();

            Initialize(currentUserId, type, id);
        
        }

        public void Initialize (string currentUserId, string type, string id)
        {
            this.currentUserId = currentUserId;
            this.type = type;
            this.id = id;
            GenerateLink();
            InitUserAvatar();
        }

        public void InitUserAvatar ()
        {
            userAvatar.BeginInit();
            Uri avatar = new Uri(@"http://localhost:4000/api/user/avatar/?id=" + currentUserId);
            userAvatar.Source = new BitmapImage(avatar);
            userAvatar.EndInit();
        }

        public void GenerateLink ()
        {
            linkBox.Text = "http://";
        }

        public void OpenShareAreaHandler (object sender, RoutedEventArgs e)
        {
            OpenShareArea();
        }

        public void OpenShareArea ()
        {
            mainControl.SelectedIndex = 1;
        }

        private void SetDefaultAvatarHandler (object sender, ExceptionRoutedEventArgs e)
        {
            Image avatar = ((Image)(sender));
            SetDefaultAvatar(avatar);
        }

        public void SetDefaultAvatar (Image avatar)
        {
            avatar.BeginInit();
            avatar.Source = new BitmapImage(new Uri(@"https://cdn2.iconfinder.com/data/icons/ios-7-icons/50/user_male-128.png"));
            avatar.EndInit();
        }

        public void PublishHandler (object sender, RoutedEventArgs e)
        {
            Publish();
        }

        public void Publish ()
        {
            string msgBoxContent = msgBox.Text;
            try
            {
                HttpWebRequest innerNestedWebRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/activities/add/?id=" + currentUserId + @"&content=receiveComment&data=" + msgBoxContent);
                innerNestedWebRequest.Method = "GET";
                innerNestedWebRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse innerNestedWebResponse = (HttpWebResponse)innerNestedWebRequest.GetResponse())
                {
                    using (var innerNestedReader = new StreamReader(innerNestedWebResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        string objText = innerNestedReader.ReadToEnd();
                        UserResponseInfo myInnerNestedObj = (UserResponseInfo)js.Deserialize(objText, typeof(UserResponseInfo));
                        string status = myInnerNestedObj.status;
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

    }
}
