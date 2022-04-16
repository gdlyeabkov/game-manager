using SocketIOClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    /// Логика взаимодействия для ChatDialog.xaml
    /// </summary>
    public partial class ChatDialog : Window
    {

        public int activeChatIndex = -1;
        public string currentUserId;
        public SocketIO client;
        public string friendId;

        public ChatDialog(string currentUserId, SocketIO client, string friendId)
        {
            InitializeComponent();
            this.currentUserId = currentUserId;
            // this.client = client;
            this.friendId = friendId;
            // ReceiveMessages();
        }

        async public void ReceiveMessages ()
        {
            try
            {
                
                client = new SocketIO("https://digitaldistributtionservice.herokuapp.com/");
                await client.ConnectAsync();

                client.On("friend_send_msg", async response =>
                {
                    var rawResult = response.GetValue<string>();
                    string[] result = rawResult.Split(new char[] { '|' });
                    string userId = result[0];
                    string msg = result[1];
                    Debugger.Log(0, "debug", Environment.NewLine + "user " + userId + " send msg: " + msg + Environment.NewLine);

                    // SendMsg(msg);
                    try
                    {
                        ItemCollection chatControlItems = chatControl.Items;
                        object rawActiveChat = chatControlItems[activeChatIndex];
                        TabItem activeChat = ((TabItem)(rawActiveChat));
                        object rawActiveChatScrollContent = activeChat.Content;
                        ScrollViewer activeChatScrollContent = ((ScrollViewer)(rawActiveChatScrollContent));
                        object rawActiveChatContent = activeChatScrollContent.Content;
                        StackPanel activeChatContent = ((StackPanel)(rawActiveChatContent));
                        TextBlock newMsg = new TextBlock();
                        string newMsgContent = msg;
                        newMsg.Text = newMsgContent;
                        inputChatMsgBox.Text = "";
                        activeChatContent.Children.Add(newMsg);
                    }
                    catch (Exception)
                    {
                        Debugger.Log(0, "debug", "поток занят");
                        await client.ConnectAsync();
                    }

                });
            }
            catch (System.Net.WebSockets.WebSocketException)
            {
                Debugger.Log(0, "debug", "Ошибка сокетов");
                await client.ConnectAsync();
            }
        }

        public void InitializeHandler (object sender, RoutedEventArgs e)
        {
            Initialize();
        }

        public void Initialize()
        {
            AddChat();
            ReceiveMessages();
        }

        public void AddChat()
        {
            TabItem newChat = new TabItem();
            newChat.Header = friendId;
            chatControl.Items.Add(newChat);
            ScrollViewer newChatScrollContent = new ScrollViewer();
            StackPanel newChatContent = new StackPanel();
            newChatScrollContent.Content = newChatContent;
            newChat.Content = newChatScrollContent;
            activeChatIndex++;
            chatControl.SelectedIndex = activeChatIndex;
        }

        private void SendMsgHandler (object sender, RoutedEventArgs e)
        {
            string newMsgContent = inputChatMsgBox.Text;
            SendMsg(newMsgContent);
        }

        async public void SendMsg(string newMsgContent)
        {
            try
            {
                ItemCollection chatControlItems = chatControl.Items;
                object rawActiveChat = chatControlItems[activeChatIndex];
                TabItem activeChat = ((TabItem)(rawActiveChat));
                object rawActiveChatScrollContent = activeChat.Content;
                ScrollViewer activeChatScrollContent = ((ScrollViewer)(rawActiveChatScrollContent));
                object rawActiveChatContent = activeChatScrollContent.Content;
                StackPanel activeChatContent = ((StackPanel)(rawActiveChatContent));
                TextBlock newMsg = new TextBlock();
                newMsg.Text = newMsgContent;
                inputChatMsgBox.Text = "";
                activeChatContent.Children.Add(newMsg);
            }
            catch (Exception)
            {
                Debugger.Log(0, "debug", "поток занят");
            }
            try
            {
                await client.EmitAsync("user_send_msg", currentUserId + "|" + newMsgContent);

            }
            catch (System.Net.WebSockets.WebSocketException)
            {
                Debugger.Log(0, "debug", "Ошибка сокетов");
            }
            catch (InvalidOperationException)
            {
                Debugger.Log(0, "debug", "Нельзя отправить повторно");
            }
        }

    }
}
