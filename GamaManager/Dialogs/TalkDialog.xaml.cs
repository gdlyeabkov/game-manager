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
    /// Логика взаимодействия для TalkDialog.xaml
    /// </summary>
    public partial class TalkDialog : Window
    {

        public string currentUserId = "";
        public string talkId = "";
        public int activeChatIndex = -1;
        public DateTime lastInputTimeStamp; 
        public Brush msgsSeparatorBrush = null;

        public TalkDialog(string currentUserId, string talkId)
        {
            InitializeComponent();

            Initialize(currentUserId, talkId);
        
        }

        public void AddChat()
        {
            TabItem newChat = new TabItem();
            newChat.Header = talkId;
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/talks/get/?id=" + talkId);
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse innerWebResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (StreamReader innerReader = new StreamReader(innerWebResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        string objText = innerReader.ReadToEnd();
                        var myobj = (TalkResponseInfo)js.Deserialize(objText, typeof(TalkResponseInfo));
                        string status = myobj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
                            Talk talk = myobj.talk;
                            string talkTitle = talk.title;
                            newChat.Header = talkTitle;

                            string userIsWritingLabelContent = talkTitle + " печатает...";
                            userIsWritingLabel.Text = userIsWritingLabelContent;

                        }
                    }
                }
            }
            catch (System.Net.WebException)
            {
                MessageBox.Show("Не удается подключиться к серверу", "Ошибка");
                this.Close();
            }
            chatControl.Items.Add(newChat);
            ScrollViewer newChatScrollContent = new ScrollViewer();
            StackPanel newChatContent = new StackPanel();
            newChatScrollContent.Content = newChatContent;
            newChat.Content = newChatScrollContent;
            activeChatIndex++;
            chatControl.SelectedIndex = activeChatIndex;
        }


        public void Initialize (string currentUserId, string talkId)
        {
            InitConstants(currentUserId, talkId);
            AddChat();
        }

        public void InitConstants(string currentUserId, string talkId)
        {
            this.currentUserId = currentUserId;
            this.talkId = talkId;
            msgsSeparatorBrush = System.Windows.Media.Brushes.LightGray;
            lastInputTimeStamp = DateTime.Now;
        }

    }
}
