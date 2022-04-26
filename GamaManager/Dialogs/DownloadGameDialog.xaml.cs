using Aspose.Zip;
using Aspose.Zip.Saving;
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
        public string downloadedGameId = "";

        public DownloadGameDialog(string currentUserId)
        {
            InitializeComponent();

            this.currentUserId = currentUserId;

        }

        async public void Initialize ()
        {
            object rawDialogData = this.DataContext;
            Dictionary<String, Object> dialogData = ((Dictionary<String, Object>)(rawDialogData));
            object rawGameId = dialogData["id"];
            string gameId = ((string)(rawGameId));
            downloadedGameId = gameId;
            object rawGameName = dialogData["name"];
            string gameName = ((string)(rawGameName));
            /*object rawGameUrl = dialogData["url"];
            string gameUrl = ((string)(rawGameUrl));*/
            string gameUrl = @"http://localhost:4000/api/game/distributive/?name=" + gameName;

            Uri uri = new Uri(gameUrl);
            WebClient wc = new WebClient();
            Environment.SpecialFolder localApplicationDataFolder = Environment.SpecialFolder.LocalApplicationData;
            string localApplicationDataFolderPath = Environment.GetFolderPath(localApplicationDataFolder);
            string appFolder = localApplicationDataFolderPath + @"\OfficeWare\GameManager\" + currentUserId + @"\";
            // string cachePath = appFolder + gameName;
            string cachePath = appFolder + @"games\" + gameName;
            Directory.CreateDirectory(cachePath);
            
            string screenShotsPath = appFolder + @"screenshots\" + gameName;
            Directory.CreateDirectory(screenShotsPath);

            string filename = cachePath + @"\game.exe";
            // string filename = cachePath + @"\game.zip";

            wc.Headers.Add("User-Agent: Other");   //that is the simple line!
            // await wc.DownloadFileTaskAsync(uri, filename);
            wc.DownloadFileAsync(uri, filename);
            wc.DownloadProgressChanged += new DownloadProgressChangedEventHandler(wc_DownloadProgressChanged);
            wc.DownloadFileCompleted += new AsyncCompletedEventHandler(wc_DownloadFileCompleted);
        }

        private void wc_DownloadProgressChanged (object sender, DownloadProgressChangedEventArgs e)
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
        private void wc_DownloadFileCompleted (object sender, AsyncCompletedEventArgs e)
        {
            Exception downloadError = e.Error;
            bool isErrorsNotFound = downloadError == null;
            if (isErrorsNotFound)
            {
                Dictionary<String, Object> dialogData = new Dictionary<String, Object>();
                dialogData = new Dictionary<String, Object>();
                dialogData.Add("id", downloadedGameId);
                dialogData.Add("status", "OK");
                this.DataContext = dialogData;
                this.Close();
            }
            else
            {
                string uploadGameErrorLabelContent = Properties.Resources.uploadGameErrorLabelContent;
                string errorLabelContent = Properties.Resources.errorLabelContent;
                MessageBox.Show(uploadGameErrorLabelContent, errorLabelContent);
                Dictionary<String, Object> dialogData = new Dictionary<String, Object>();
                dialogData.Add("id", "0");
                dialogData.Add("status", "Error");
                this.DataContext = dialogData;
                this.Close();
            }
        }

        private void InitializeHandler (object sender, RoutedEventArgs e)
        {
            Initialize();
        }

    }
}
