using SocketIOClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    /// Логика взаимодействия для CreateTextChannelDialog.xaml
    /// </summary>
    public partial class CreateTextChannelDialog : Window
    {

        public string talkId = "";
        public string currentUserId = "";
        public SocketIO client = null;

        public CreateTextChannelDialog(string talkId, string currentUserId, SocketIO client)
        {
            InitializeComponent();

            Initialize(talkId, currentUserId, client);

        }

        public void Initialize (string talkId, string currentUserId, SocketIO client)
        {
            this.talkId = talkId;
            this.currentUserId = currentUserId;
            this.client = client;
        }


        private void AcceptHandler (object sender, RoutedEventArgs e)
        {
            Accept();
        }

        async public void Accept ()
        {
            string channelNameBoxContent = channelNameBox.Text;
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("https://loud-reminiscent-jackrabbit.glitch.me/api/talks/channels/create/?id=" + talkId + @"&title=" + channelNameBoxContent);
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
                            try
                            {
                                await client.EmitAsync("user_update_talk", currentUserId + "|" + this.talkId);
                            }
                            catch (System.Net.WebSockets.WebSocketException)
                            {
                                Debugger.Log(0, "debug", "Ошибка сокетов");
                            }
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
