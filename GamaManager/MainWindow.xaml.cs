using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Windows.Interop;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Net;
using System.ComponentModel;
using System.Management.Automation;
using System.Collections.ObjectModel;

namespace GamaManager
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public string filename = @"C:\Gleb\game-manager\game.exe";

        public MainWindow()
        {
            InitializeComponent();

            Initialize();

        }

        public void Initialize()
        {
            someGame.DataContext = @"C:\GOG Games\Guns, Gore & Cannoli 2\ggc2.exe";
            ShowOffers();
        }

        public void ShowOffers()
        {
            Dialogs.OffersDialog dialog = new Dialogs.OffersDialog();
            dialog.Show();
        }

        private void RunGameHandler(object sender, MouseButtonEventArgs e)
        {
            StackPanel game = ((StackPanel)(sender));
            object gameData = game.DataContext;
            string gamePath = ((string)(gameData));
            RunGame(gamePath);
        }

        public void RunGame(string path)
        {
            GameWindow window = new GameWindow();
            window.DataContext = path;
            window.Show();
        }

        private void InstallGameHandler(object sender, RoutedEventArgs e)
        {
            InstallGame();
        }

        public void InstallGame()
        {
            Uri uri = new Uri(@"https://download.kde.org/stable/krita/4.4.8/krita-x64-4.4.8-setup.exe");
            WebClient wc = new WebClient();
            wc.DownloadFileAsync(uri, filename);
            wc.DownloadProgressChanged += new DownloadProgressChangedEventHandler(wc_DownloadProgressChanged);
            wc.DownloadFileCompleted += new AsyncCompletedEventHandler(wc_DownloadFileCompleted);
        }

        private void wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            gameInstalledProgress.Value = e.ProgressPercentage;
            if (gameInstalledProgress.Value == gameInstalledProgress.Maximum)
            {
                gameInstalledProgress.Value = 0;
            }
        }
        private void wc_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                MessageBox.Show("Download complete!, running exe", "Completed!");
                // Process.Start(filename);

                /*Process process = new Process();
                process.StartInfo.FileName = filename;
                process.StartInfo.Arguments = "/quiet";
                process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                process.Start();
                process.WaitForExit();*/

                PowerShell powerShell = null;
                Console.WriteLine(" ");
                Console.WriteLine("Deploying application...");
                /*try
                {
                    using (powerShell = PowerShell.Create())
                    {
                        powerShell.AddScript("$setup=Start-Process 'C:\\Gleb\\game-manager\\game.exe ' -ArgumentList ' / S ' -Wait -PassThru");
                        Collection <PSObject> PSOutput = powerShell.Invoke();
                        foreach (PSObject outputItem in PSOutput)
                        {
                            if (outputItem != null)
                            {
                                Console.WriteLine(outputItem.BaseObject.GetType().FullName);
                                Console.WriteLine(outputItem.BaseObject.ToString() + "\n");
                            }
                        }
                        if (powerShell.Streams.Error.Count > 0)
                        {
                            string temp = powerShell.Streams.Error.First().ToString();
                            Console.WriteLine("Error: {0}", temp);
                        }
                        else
                            Console.WriteLine("Installation has completed successfully.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error occured: {0}", ex.InnerException);
                    //throw;  
                }
                finally
                {
                    if (powerShell != null)
                        powerShell.Dispose();
                }*/
            }
            else
            {
                MessageBox.Show("Unable to download exe, please check your connection", "Download failed!");
            }
        }

    }
}
