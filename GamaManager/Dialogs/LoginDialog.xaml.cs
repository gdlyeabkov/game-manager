using SocketIOClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
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
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/users/check/?login=" + authLoginFieldContent + "&password=" + authPasswordFieldContent);
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
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/users/create/?login=" + registerLoginFieldContent + "&password=" + registerPasswordFieldContent + "&confirmPassword=" + registerConfirmPasswordFieldContent);
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
                            string id = myobj.id;
                            try
                            {
                                MailMessage message = new MailMessage();
                                SmtpClient smtp = new SmtpClient();
                                message.From = new System.Net.Mail.MailAddress("glebdyakov2000@gmail.com");
                                message.To.Add(new System.Net.Mail.MailAddress(registerLoginFieldContent));
                                string subjectBoxContent = @"Подтверждение аккаунта Office ware game manager";
                                message.Subject = subjectBoxContent;
                                message.IsBodyHtml = true; //to make message body as html  
                                string messageBodyBoxContent = "<h3>Здравствуйте, " + registerLoginFieldContent + "!</h3><p>Подтвердите E-mail вашего аккаунта Office ware game manager</p><a href=\"http://localhost:4000/api/users/email/confirm/?id=" + id + "\">Подтвердить</a>";
                                message.Body = messageBodyBoxContent;
                                smtp.Port = 587;
                                smtp.Host = "smtp.gmail.com"; //for gmail host  
                                smtp.EnableSsl = true;
                                smtp.UseDefaultCredentials = false;
                                smtp.Credentials = new NetworkCredential("glebdyakov2000@gmail.com", "ttolpqpdzbigrkhz");
                                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                                smtp.Send(message);
                            }
                            catch (Exception)
                            {
                                MessageBox.Show("Произошла ошибка при отправке письма", "Ошибка");
                            }
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

            // var client = new SocketIO("http://localhost:4000/");
            var client = new SocketIO("https://digitaldistributtionservice.herokuapp.com/");

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

    public class PasswordBoxMonitor : DependencyObject
    {
        public static bool GetIsMonitoring(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsMonitoringProperty);
        }

        public static void SetIsMonitoring(DependencyObject obj, bool value)
        {
            obj.SetValue(IsMonitoringProperty, value);
        }

        public static readonly DependencyProperty IsMonitoringProperty =
            DependencyProperty.RegisterAttached("IsMonitoring", typeof(bool), typeof(PasswordBoxMonitor), new UIPropertyMetadata(false, OnIsMonitoringChanged));



        public static int GetPasswordLength(DependencyObject obj)
        {
            return (int)obj.GetValue(PasswordLengthProperty);
        }

        public static void SetPasswordLength(DependencyObject obj, int value)
        {
            obj.SetValue(PasswordLengthProperty, value);
        }

        public static readonly DependencyProperty PasswordLengthProperty =
            DependencyProperty.RegisterAttached("PasswordLength", typeof(int), typeof(PasswordBoxMonitor), new UIPropertyMetadata(0));

        private static void OnIsMonitoringChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var pb = d as PasswordBox;
            if (pb == null)
            {
                return;
            }
            if ((bool)e.NewValue)
            {
                pb.PasswordChanged += PasswordChanged;
            }
            else
            {
                pb.PasswordChanged -= PasswordChanged;
            }
        }

        static void PasswordChanged(object sender, RoutedEventArgs e)
        {
            var pb = sender as PasswordBox;
            if (pb == null)
            {
                return;
            }
            SetPasswordLength(pb, pb.Password.Length);
        }
    }

}
