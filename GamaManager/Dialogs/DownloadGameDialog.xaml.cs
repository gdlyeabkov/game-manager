using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
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
    /// Логика взаимодействия для DownloadGameDialog.xaml
    /// </summary>
    public partial class DownloadGameDialog : Window
    {

        public string currentUserId = "";

        public DownloadGameDialog(string currentUserId)
        {
            InitializeComponent();

            this.currentUserId = currentUserId;

        }

        public void Initialize ()
        {
            object rawDialogData = this.DataContext;
            Dictionary<String, Object> dialogData = ((Dictionary<String, Object>)(rawDialogData));
            object rawGameName = dialogData["name"];
            string gameName = ((string)(rawGameName));
            object rawGameUrl = dialogData["url"];
            string gameUrl = ((string)(rawGameUrl));

            Uri uri = new Uri(gameUrl);
            WebClient wc = new WebClient();
            Environment.SpecialFolder localApplicationDataFolder = Environment.SpecialFolder.LocalApplicationData;
            string localApplicationDataFolderPath = Environment.GetFolderPath(localApplicationDataFolder);
            string appFolder = localApplicationDataFolderPath + @"\OfficeWare\GameManager\" + currentUserId + @"\";
            string cachePath = appFolder + gameName;
            Directory.CreateDirectory(cachePath);
            string filename = cachePath + @"\game.exe";
            wc.DownloadFileAsync(uri, filename);
            wc.DownloadProgressChanged += new DownloadProgressChangedEventHandler(wc_DownloadProgressChanged);
            wc.DownloadFileCompleted += new AsyncCompletedEventHandler(wc_DownloadFileCompleted);
        }

        private void wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            gameInstalledProgress.Value = e.ProgressPercentage;
            double progress = gameInstalledProgress.Value;
            double maxProgress = gameInstalledProgress.Maximum;
            bool isDownloaded = progress == maxProgress;
            if (isDownloaded)
            {
                gameInstalledProgress.Value = 0;
            }
        }
        private void wc_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            Exception downloadError = e.Error;
            bool isErrorsNotFound = downloadError == null;
            if (isErrorsNotFound)
            {
                this.DataContext = "OK";
                this.Close();
            }
            else
            {
                MessageBox.Show("Не удалось загрузить игру", "Ошибка");
                this.DataContext = "Error";
                this.Close();
            }
        }

        private void InitializeHandler (object sender, RoutedEventArgs e)
        {
            Initialize();
        }

    }
}
