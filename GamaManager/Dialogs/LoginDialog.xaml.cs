using SocketIOClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
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
using WebSocketSharp;

namespace GamaManager.Dialogs
{
    /// <summary>
    /// Логика взаимодействия для LoginDialog.xaml
    /// </summary>
    public partial class LoginDialog : Window
    {
        public LoginDialog()
        {
            InitializeComponent();
        }

        private void LoginHandler (object sender, RoutedEventArgs e)
        {
            Login();
        }

        public void Login()
        {
            string authLoginFieldContent = authLoginField.Text;
            string authPasswordFieldContent = authPasswordField.Password;
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("https://loud-reminiscent-jackrabbit.glitch.me/api/users/check/?login=" + authLoginFieldContent + "&password=" + authPasswordFieldContent);
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (var reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        var objText = reader.ReadToEnd();

                        LoginResponseInfo myobj = (LoginResponseInfo)js.Deserialize(objText, typeof(LoginResponseInfo));

                        string status = myobj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
                            // OpenManager(authLoginFieldContent, authPasswordFieldContent);
                            string id = myobj.id;
                            OpenManager(id);
                        }
                        else
                        {
                            MessageBox.Show("Не удалось войти в аккаунт", "Ошибка");
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

        private void RegisterHandler(object sender, RoutedEventArgs e)
        {
            Register();
        }

        public void Register ()
        {
            string registerLoginFieldContent = registerLoginField.Text;
            string registerPasswordFieldContent = registerPasswordField.Password;
            string registerConfirmPasswordFieldContent = registerConfirmPasswordField.Password;
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("https://loud-reminiscent-jackrabbit.glitch.me/api/users/create/?login=" + registerLoginFieldContent + "&password=" + registerPasswordFieldContent + "&confirmPassword=" + registerConfirmPasswordFieldContent);
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (var reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        var objText = reader.ReadToEnd();
                    
                        RegisterResponseInfo myobj = (RegisterResponseInfo)js.Deserialize(objText, typeof(RegisterResponseInfo));

                        string status = myobj.status;
                        bool isOkStatus = status == "OK";
                        if (isOkStatus)
                        {
                            // OpenManager(registerLoginFieldContent, registerPasswordFieldContent);
                            string id = myobj.id;
                            OpenManager(id);
                        }
                        else
                        {
                            MessageBox.Show("Не удалось создать аккаунт", "Ошибка");
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

        public void OpenManager (string id)
        {
            MainWindow window = new MainWindow(id);
            window.Show();
            this.Close();
        }

        private void ToggleModeHandler (object sender, MouseButtonEventArgs e)
        {
            TextBlock link = ((TextBlock)(sender));
            object rawLinkData = link.DataContext;
            string linkData = rawLinkData.ToString();
            int modeIndex = Int32.Parse(linkData);
            ToggleMode(modeIndex);
        }

        public void ToggleMode(int modeIndex)
        {
            links.SelectedIndex = modeIndex;
        }

        private void WindowLoadedHandler (object sender, RoutedEventArgs e)
        {
            // ListenSockets();
        }

        public async void ListenSockets()
        {

            var client = new SocketIO("https://loud-reminiscent-jackrabbit.glitch.me/");
            client.OnConnected += async (sender, e) =>
            {
                Debugger.Log(0, "debug", "client socket conntected");
                // await client.EmitAsync("event", "event");
            };
            client.OnDisconnected += async (sender, e) =>
            {
                Debugger.Log(0, "debug", "client socket disconntected");
                // await client.EmitAsync("event", "event");
            };
            client.OnError += async (sender, e) =>
            {
                Debugger.Log(0, "debug", "client socket error");
            };

            client.On("hello", response =>
            {
                Debugger.Log(0, "debug", response.ToString());
                var result = response.GetValue<string>();
                Debugger.Log(0, "debug", result);
            });
            await client.ConnectAsync();
        }

    }

    public class RegisterResponseInfo
    {
        public string status;
        public string id;
    }

    public class LoginResponseInfo
    {
        public string status;
        public string id;
    }

}
