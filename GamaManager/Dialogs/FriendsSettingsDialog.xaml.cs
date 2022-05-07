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
using System.Windows.Shapes;
using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace GamaManager.Dialogs
{
    /// <summary>
    /// Логика взаимодействия для FriendsSettingsDialog.xaml
    /// </summary>
    public partial class FriendsSettingsDialog : Window
    {

        public string currentUserId = "";
        public bool isAuxVoiceSettingsOpened = false;
        public bool isStartMicroCheck = false;
        public WaveIn waveSource = null;
        public WaveFileWriter waveFile;

        public FriendsSettingsDialog(string currentUserId)
        {
            InitializeComponent();

            Initialize(currentUserId);

        }

        public void Initialize (string currentUserId)
        {
            this.currentUserId = currentUserId;
            GetVoiceChatSettings();
        }

        public void GetVoiceChatSettings ()
        {
            MMDeviceEnumerator names = new MMDeviceEnumerator();
            MMDeviceCollection inputDevices = names.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active);
            foreach (MMDevice device in inputDevices)
            {
                string deviceName = device.FriendlyName;
                ComboBoxItem soundInputBoxItem = new ComboBoxItem();
                soundInputBoxItem.Content = device;
                soundInputBox.Items.Add(deviceName);
            }
            int countDevices = inputDevices.Count;
            bool isHaveDevices = countDevices >= 1;
            if (isHaveDevices)
            {
                soundInputBox.SelectedIndex = 0;
            }
            names = new MMDeviceEnumerator();
            MMDeviceCollection outputDevices = names.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);
            foreach (MMDevice device in outputDevices)
            {
                string deviceName = device.FriendlyName;
                ComboBoxItem soundOutputBoxItem = new ComboBoxItem();
                soundOutputBoxItem.Content = device;
                soundOutputBox.Items.Add(deviceName);
            }
            countDevices = outputDevices.Count;
            isHaveDevices = countDevices >= 1;
            if (isHaveDevices)
            {
                soundOutputBox.SelectedIndex = 0;
            }
        }

        public void SelectSettingsItemHandler (object sender, RoutedEventArgs e)
        {
            TextBlock label = ((TextBlock)(sender));
            object labelData = label.DataContext;
            string rawIndex=  labelData.ToString();
            int index = Int32.Parse(rawIndex);
            SelectSettingsItem(index);
        }

        public void SelectSettingsItem (int index)
        {
            settingsControl.SelectedIndex = index;
        }

        private void SetDefaultAvatarHandler (object sender, ExceptionRoutedEventArgs e)
        {
            Image avatar = ((Image)(sender));
            SetDefaultAvatar(avatar);
        }

        public void SetDefaultAvatar(Image avatar)
        {
            avatar.BeginInit();
            avatar.Source = new BitmapImage(new Uri(@"https://cdn2.iconfinder.com/data/icons/ios-7-icons/50/user_male-128.png"));
            avatar.EndInit();
        }

        private void ToggleVoiceChatAuxSettingsHandler (object sender, RoutedEventArgs e)
        {
            ToggleVoiceChatAuxSettings();
        }

        public void ToggleVoiceChatAuxSettings ()
        {
            isAuxVoiceSettingsOpened = !isAuxVoiceSettingsOpened;
            if (isAuxVoiceSettingsOpened)
            {
                auxVoiceChatSettingsBtnLabel.Text = "Скрыть дополнительные настройки";
                auxVoiceChatSettings.Visibility = Visibility.Visible;
            }
            else
            {
                auxVoiceChatSettingsBtnLabel.Text = "Дополнительные настройки";
                auxVoiceChatSettings.Visibility = Visibility.Collapsed;
            }
        }

        private void ToggleMicroCheckHandler (object sender, RoutedEventArgs e)
        {
            ToggleMicroCheck();
        }

        public void ToggleMicroCheck ()
        {
            isStartMicroCheck = !isStartMicroCheck;
            if (isStartMicroCheck)
            {
                waveSource = new WaveIn();
                waveSource.WaveFormat = new WaveFormat(44100, 1);
                waveSource.DataAvailable += new EventHandler<WaveInEventArgs>(MicroDataAvailableHandler);
                waveSource.RecordingStopped += new EventHandler<StoppedEventArgs>(MicroRecordingStoppedHandler);
                Environment.SpecialFolder localApplicationDataFolder = Environment.SpecialFolder.LocalApplicationData;
                string localApplicationDataFolderPath = Environment.GetFolderPath(localApplicationDataFolder);
                string tempRecordFilePath = localApplicationDataFolderPath + @"\OfficeWare\GameManager\" + currentUserId + @"\record.wav";
                waveFile = new WaveFileWriter(tempRecordFilePath, waveSource.WaveFormat);
                waveSource.StartRecording();
            }
            else
            {
                waveSource.StopRecording();
            }
        }

        public void MicroRecordingStoppedHandler(object sender, StoppedEventArgs e)
        {
            MicroRecordingStopped();
        }

        public void MicroRecordingStopped()
        {
            if (waveSource != null)
            {
                waveSource.Dispose();
                waveSource = null;
            }
            if (waveFile != null)
            {
                waveFile.Dispose();
                waveFile = null;
            }
            Environment.SpecialFolder localApplicationDataFolder = Environment.SpecialFolder.LocalApplicationData;
            string localApplicationDataFolderPath = Environment.GetFolderPath(localApplicationDataFolder);
            string tempRecordFilePath = localApplicationDataFolderPath + @"\OfficeWare\GameManager\" + currentUserId + @"\record.wav";
            WaveFileReader waveFileReader = new WaveFileReader(tempRecordFilePath);
            IWavePlayer player = new WaveOut(WaveCallbackInfo.FunctionCallback());
            player.Volume = 1.0f;
            player.Init(waveFileReader);
            player.Play();
            while (true)
            {
                if (player.PlaybackState == NAudio.Wave.PlaybackState.Stopped)
                {
                    player.Dispose();
                    waveFileReader.Close();
                    waveFileReader.Dispose();
                    break;
                }
            };
        }

        public void MicroDataAvailableHandler(object sender, WaveInEventArgs e)
        {
            byte[] buffer = e.Buffer;
            int recordedBytes = e.BytesRecorded;
            MicroDataAvailable(buffer, recordedBytes);
        }

        public void MicroDataAvailable (byte[] buffer, int recordedBytes)
        {
            if (waveFile != null)
            {
                waveFile.Write(buffer, 0, recordedBytes);
                waveFile.Flush();
            }
        }

    }
}
