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
    /// Логика взаимодействия для AddReviewComplaintDialog.xaml
    /// </summary>
    public partial class AddReviewComplaintDialog : Window
    {

        string id = "";
        string currentUserId = "";

        public AddReviewComplaintDialog(string id, string currentUserId)
        {
            InitializeComponent();

            Initialize(id, currentUserId);

        }

        public void Initialize (string id, string currentUserId)
        {
            this.id = id;
            this.currentUserId = currentUserId;
        }

        private void OKHandler (object sender, RoutedEventArgs e)
        {
            OK();
        }

        public void OK ()
        {
            string desc = descBox.Text;
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:4000/api/reviews/complaints/add/?id=" + id + "&user=" + currentUserId + "&desc=" + desc);
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (StreamReader reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        string objText = reader.ReadToEnd();
                        DevicesResponseInfo myObj = (DevicesResponseInfo)js.Deserialize(objText, typeof(DevicesResponseInfo));
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
