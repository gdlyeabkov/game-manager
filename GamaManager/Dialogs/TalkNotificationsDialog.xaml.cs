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
    /// Логика взаимодействия для TalkNotificationsDialog.xaml
    /// </summary>
    public partial class TalkNotificationsDialog : Window
    {

        public string currentUserId = "";
        public string talkId = "";

        public TalkNotificationsDialog(string currentUserId, string talkId)
        {
            InitializeComponent();

            Initialize(currentUserId, talkId);

        }

        public void Initialize(string currentUserId, string talkId)
        {
            this.currentUserId = currentUserId;
            this.talkId = talkId;
            SetForTalkLabel();
            GetChannels();
        }

        public void GetChannels ()
        {
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("https://loud-reminiscent-jackrabbit.glitch.me/api/talks/channels/all");
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
                                string channelTitle = channel.title;
                                RowDefinition row = new RowDefinition();
                                row.Height = new GridLength(50);
                                channels.RowDefinitions.Add(row);
                                RowDefinitionCollection rows = channels.RowDefinitions;
                                int countRows = rows.Count;
                                int lastRowIndex = countRows - 1;
                                TextBlock channelNameLabel = new TextBlock();
                                channelNameLabel.VerticalAlignment = VerticalAlignment.Center;
                                channelNameLabel.Text = channelTitle;
                                channels.Children.Add(channelNameLabel);
                                Grid.SetRow(channelNameLabel, lastRowIndex);
                                Grid.SetColumn(channelNameLabel, 0);
                                ComboBox channelSettingsBox = new ComboBox();
                                channelSettingsBox.VerticalAlignment = VerticalAlignment.Center;
                                channelSettingsBox.VerticalContentAlignment = VerticalAlignment.Center;
                                channelSettingsBox.HorizontalAlignment = HorizontalAlignment.Left;
                                channelSettingsBox.Width = 150;
                                channelSettingsBox.Height = 30;
                                ComboBoxItem channelSettingsBoxItem = new ComboBoxItem();
                                channelSettingsBoxItem.Content = "Не заменять настройки";
                                channelSettingsBox.Items.Add(channelSettingsBoxItem);
                                channelSettingsBoxItem = new ComboBoxItem();
                                channelSettingsBoxItem.Content = "Обо всех сообщениях";
                                channelSettingsBox.Items.Add(channelSettingsBoxItem);
                                channelSettingsBoxItem = new ComboBoxItem();
                                channelSettingsBoxItem.Content = "Только об @all, @online или упоминаниях";
                                channelSettingsBox.Items.Add(channelSettingsBoxItem);
                                channelSettingsBoxItem = new ComboBoxItem();
                                channelSettingsBoxItem.Content = "Только об упоминаниях";
                                channelSettingsBox.Items.Add(channelSettingsBoxItem);
                                channelSettingsBoxItem = new ComboBoxItem();
                                channelSettingsBoxItem.Content = "Никогда";
                                channelSettingsBox.Items.Add(channelSettingsBoxItem);
                                channelSettingsBox.SelectedIndex = 0;
                                channels.Children.Add(channelSettingsBox);
                                Grid.SetRow(channelSettingsBox, lastRowIndex);
                                Grid.SetColumn(channelSettingsBox, 1);
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

        public void SetForTalkLabel()
        {
            Talk talk = GetTalkInfo();
            string talkTitle = talk.title;
            string forTalkLabelContent = "Для чата " + talkTitle;
            forTalkLabel.Text = forTalkLabelContent;
        }

        private Talk GetTalkInfo()
        {
            Talk talk = null;
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("https://loud-reminiscent-jackrabbit.glitch.me/api/talks/get/?id=" + talkId);
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

    }
}
