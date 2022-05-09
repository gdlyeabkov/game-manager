using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
            LoadSettings();
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

        private void ToggleAttachChatToFriendListHandler (object sender, RoutedEventArgs e)
        {
            ToggleButton btn = ((ToggleButton)(sender));
            ToggleAttachChatToFriendList(btn);
        }

        public void ToggleAttachChatToFriendList (ToggleButton btn)
        {
            object rawIsChecked = btn.IsChecked;
            bool isChecked = ((bool)(rawIsChecked));
            if (isChecked)
            {
                bool toggledValue = !isChecked;
                bool isEnabledBtn = btn == attachChatToFriendListBtn;
                if (isEnabledBtn)
                {
                    notAttachChatToFriendListBtn.IsChecked = toggledValue;
                }
                else
                {
                    attachChatToFriendListBtn.IsChecked = toggledValue;
                }
            }
        }

        private void ToggleOpenNewChatInNewWindowHandler (object sender, RoutedEventArgs e)
        {
            ToggleButton btn = ((ToggleButton)(sender));
            ToggleOpenNewChatInNewWindow(btn);
        }

        public void ToggleOpenNewChatInNewWindow (ToggleButton btn)
        {
            object rawIsChecked = btn.IsChecked;
            bool isChecked = ((bool)(rawIsChecked));
            if (isChecked)
            {
                bool toggledValue = !isChecked;
                bool isEnabledBtn = btn == openNewChatInNewWindowBtn;
                if (isEnabledBtn)
                {
                    openNewChatInNewTabBtn.IsChecked = toggledValue;
                }
                else
                {
                    openNewChatInNewWindowBtn.IsChecked = toggledValue;
                }
            }
        }

        private void ToggleNotIncludeImagesAndMediaFilesHandler (object sender, RoutedEventArgs e)
        {
            ToggleButton btn = ((ToggleButton)(sender));
            ToggleNotIncludeImagesAndMediaFiles(btn);
        }

        public void ToggleNotIncludeImagesAndMediaFiles (ToggleButton btn)
        {
            object rawIsChecked = btn.IsChecked;
            bool isChecked = ((bool)(rawIsChecked));
            if (isChecked)
            {
                bool toggledValue = !isChecked;
                bool isEnabledBtn = btn == notIncludeImagesAndMediaFilesBtn;
                if (isEnabledBtn)
                {
                    includeImagesAndMediaFilesBtn.IsChecked = toggledValue;
                }
                else
                {
                    notIncludeImagesAndMediaFilesBtn.IsChecked = toggledValue;
                }
            }
        }

        private void ToggleRememberChatsHandler (object sender, RoutedEventArgs e)
        {
            ToggleButton btn = ((ToggleButton)(sender));
            ToggleRememberChats(btn);
        }
        public void ToggleRememberChats (ToggleButton btn)
        {
            object rawIsChecked = btn.IsChecked;
            bool isChecked = ((bool)(rawIsChecked));
            if (isChecked)
            {
                bool toggledValue = !isChecked;
                bool isEnabledBtn = btn == rememberChatsBtn;
                if (isEnabledBtn)
                {
                    forgetChatsBtn.IsChecked = toggledValue;
                }
                else
                {
                    rememberChatsBtn.IsChecked = toggledValue;
                }
            }
        }

        private void ToggleShowTimeIn24Handler (object sender, RoutedEventArgs e)
        {
            ToggleButton btn = ((ToggleButton)(sender));
            ToggleShowTimeIn24(btn);
        }
        public void ToggleShowTimeIn24 (ToggleButton btn)
        {
            object rawIsChecked = btn.IsChecked;
            bool isChecked = ((bool)(rawIsChecked));
            if (isChecked)
            {
                bool toggledValue = !isChecked;
                bool isEnabledBtn = btn == showTimeIn24Btn;
                if (isEnabledBtn)
                {
                    showTimeIn12Btn.IsChecked = toggledValue;
                }
                else
                {
                    showTimeIn24Btn.IsChecked = toggledValue;
                }
            }
        }

        private void ToggleDisableSpellCheckHandler (object sender, RoutedEventArgs e)
        {
            ToggleButton btn = ((ToggleButton)(sender));
            ToggleDisableSpellCheck(btn);
        }
        public void ToggleDisableSpellCheck (ToggleButton btn)
        {
            object rawIsChecked = btn.IsChecked;
            bool isChecked = ((bool)(rawIsChecked));
            if (isChecked)
            {
                bool toggledValue = !isChecked;
                bool isEnabledBtn = btn == disableSpellCheckBtn;
                if (isEnabledBtn)
                {
                    enableSpellCheckBtn.IsChecked = toggledValue;
                }
                else
                {
                    disableSpellCheckBtn.IsChecked = toggledValue;
                }
            }
        }

        private void ToggleDisableAnimationHandler (object sender, RoutedEventArgs e)
        {
            ToggleButton btn = ((ToggleButton)(sender));
            ToggleDisableAnimation(btn);
        }
        public void ToggleDisableAnimation (ToggleButton btn)
        {
            object rawIsChecked = btn.IsChecked;
            bool isChecked = ((bool)(rawIsChecked));
            if (isChecked)
            {
                bool toggledValue = !isChecked;
                bool isEnabledBtn = btn == disableAnimationBtn;
                if (isEnabledBtn)
                {
                    enableAnimationBtn.IsChecked = toggledValue;
                }
                else
                {
                    disableAnimationBtn.IsChecked = toggledValue;
                }
            }
        }

        private void ToggleFriendListAndChatsCompactViewHandler (object sender, RoutedEventArgs e)
        {
            ToggleButton btn = ((ToggleButton)(sender));
            ToggleFriendListAndChatsCompactView(btn);
        }
        public void ToggleFriendListAndChatsCompactView (ToggleButton btn)
        {
            object rawIsChecked = btn.IsChecked;
            bool isChecked = ((bool)(rawIsChecked));
            if (isChecked)
            {
                bool toggledValue = !isChecked;
                bool isEnabledBtn = btn == friendListAndChatsCompactViewBtn;
                if (isEnabledBtn)
                {
                    friendListAndChatsUnCompactViewBtn.IsChecked = toggledValue;
                }
                else
                {
                    friendListAndChatsCompactViewBtn.IsChecked = toggledValue;
                }
            }
        }

        private void ToggleFavoriteCompactViewHandler (object sender, RoutedEventArgs e)
        {
            ToggleButton btn = ((ToggleButton)(sender));
            ToggleFavoriteCompactView(btn);
        }
        public void ToggleFavoriteCompactView (ToggleButton btn)
        {
            object rawIsChecked = btn.IsChecked;
            bool isChecked = ((bool)(rawIsChecked));
            if (isChecked)
            {
                bool toggledValue = !isChecked;
                bool isEnabledBtn = btn == favoriteCompactViewBtn;
                if (isEnabledBtn)
                {
                    favoriteUnCompactViewBtn.IsChecked = toggledValue;
                }
                else
                {
                    favoriteCompactViewBtn.IsChecked = toggledValue;
                }
            }
        }

        private void ToggleAddNickAfterFriendNameHandler (object sender, RoutedEventArgs e)
        {
            ToggleButton btn = ((ToggleButton)(sender));
            ToggleAddNickAfterFriendName(btn);
        }
        public void ToggleAddNickAfterFriendName (ToggleButton btn)
        {
            object rawIsChecked = btn.IsChecked;
            bool isChecked = ((bool)(rawIsChecked));
            if (isChecked)
            {
                bool toggledValue = !isChecked;
                bool isEnabledBtn = btn == addNickAfterFriendNameBtn;
                if (isEnabledBtn)
                {
                    notAddNickAfterFriendNameBtn.IsChecked = toggledValue;
                }
                else
                {
                    addNickAfterFriendNameBtn.IsChecked = toggledValue;
                }
            }
        }

        private void ToggleGroupFriendsByGameHandler (object sender, RoutedEventArgs e)
        {
            ToggleButton btn = ((ToggleButton)(sender));
            ToggleGroupFriendsByGame(btn);
        }
        public void ToggleGroupFriendsByGame (ToggleButton btn)
        {
            object rawIsChecked = btn.IsChecked;
            bool isChecked = ((bool)(rawIsChecked));
            if (isChecked)
            {
                bool toggledValue = !isChecked;
                bool isEnabledBtn = btn == groupFriendsByGameBtn;
                if (isEnabledBtn)
                {
                    notGroupFriendsByGameBtn.IsChecked = toggledValue;
                }
                else
                {
                    groupFriendsByGameBtn.IsChecked = toggledValue;
                }
            }
        }

        private void ToggleHideOfflineFriendsFromCategoriesHandler (object sender, RoutedEventArgs e)
        {
            ToggleButton btn = ((ToggleButton)(sender));
            ToggleHideOfflineFriendsFromCategories(btn);
        }
        public void ToggleHideOfflineFriendsFromCategories (ToggleButton btn)
        {
            object rawIsChecked = btn.IsChecked;
            bool isChecked = ((bool)(rawIsChecked));
            if (isChecked)
            {
                bool toggledValue = !isChecked;
                bool isEnabledBtn = btn == hideOfflineFriendsFromCategoriesBtn;
                if (isEnabledBtn)
                {
                    showOfflineFriendsFromCategoriesBtn.IsChecked = toggledValue;
                }
                else
                {
                    hideOfflineFriendsFromCategoriesBtn.IsChecked = toggledValue;
                }
            }
        }

        private void ToggleHideFriendsFromCategoriesHandler (object sender, RoutedEventArgs e)
        {
            ToggleButton btn = ((ToggleButton)(sender));
            ToggleHideFriendsFromCategories(btn);
        }
        public void ToggleHideFriendsFromCategories (ToggleButton btn)
        {
            object rawIsChecked = btn.IsChecked;
            bool isChecked = ((bool)(rawIsChecked));
            if (isChecked)
            {
                bool toggledValue = !isChecked;
                bool isEnabledBtn = btn == hideFriendsFromCategoriesBtn;
                if (isEnabledBtn)
                {
                    showFriendsFromCategoriesBtn.IsChecked = toggledValue;
                }
                else
                {
                    hideFriendsFromCategoriesBtn.IsChecked = toggledValue;
                }
            }
        }

        private void ToggleIgnoreNotHereStatusWithSortHandler (object sender, RoutedEventArgs e)
        {
            ToggleButton btn = ((ToggleButton)(sender));
            ToggleIgnoreNotHereStatusWithSort(btn);
        }
        public void ToggleIgnoreNotHereStatusWithSort(ToggleButton btn)
        {
            object rawIsChecked = btn.IsChecked;
            bool isChecked = ((bool)(rawIsChecked));
            if (isChecked)
            {
                bool toggledValue = !isChecked;
                bool isEnabledBtn = btn == ignoreNotHereStatusWithSortBtn;
                if (isEnabledBtn)
                {
                    notIgnoreNotHereStatusWithSortBtn.IsChecked = toggledValue;
                }
                else
                {
                    ignoreNotHereStatusWithSortBtn.IsChecked = toggledValue;
                }
            }
        }

        private void ToggleLoginInFriendSystemAfterClientLoadedHandler (object sender, RoutedEventArgs e)
        {
            ToggleButton btn = ((ToggleButton)(sender));
            ToggleLoginInFriendSystemAfterClientLoaded(btn);
        }
        public void ToggleLoginInFriendSystemAfterClientLoaded (ToggleButton btn)
        {
            object rawIsChecked = btn.IsChecked;
            bool isChecked = ((bool)(rawIsChecked));
            if (isChecked)
            {
                bool toggledValue = !isChecked;
                bool isEnabledBtn = btn == loginInFriendSystemAfterClientLoadedBtn;
                if (isEnabledBtn)
                {
                    notLoginInFriendSystemAfterClientLoadedBtn.IsChecked = toggledValue;
                }
                else
                {
                    loginInFriendSystemAfterClientLoadedBtn.IsChecked = toggledValue;
                }
            }
        }

        private void ToggleEnableAnimationAvatarsHandler (object sender, RoutedEventArgs e)
        {
            ToggleButton btn = ((ToggleButton)(sender));
            ToggleEnableAnimationAvatars(btn);
        }
        public void ToggleEnableAnimationAvatars (ToggleButton btn)
        {
            object rawIsChecked = btn.IsChecked;
            bool isChecked = ((bool)(rawIsChecked));
            if (isChecked)
            {
                bool toggledValue = !isChecked;
                bool isEnabledBtn = btn == enableAnimationAvatarsBtn;
                if (isEnabledBtn)
                {
                    disableAnimationAvatarsBtn.IsChecked = toggledValue;
                }
                else
                {
                    enableAnimationAvatarsBtn.IsChecked = toggledValue;
                }
            }
        }

        private void SaveSettingsHandler (object sender, EventArgs e)
        {
            SaveSettings();
        }

        public void SaveSettings ()
        {
            Environment.SpecialFolder localApplicationDataFolder = Environment.SpecialFolder.LocalApplicationData;
            string localApplicationDataFolderPath = Environment.GetFolderPath(localApplicationDataFolder);
            string saveDataFilePath = localApplicationDataFolderPath + @"\OfficeWare\GameManager\" + currentUserId + @"\save-data.txt";
            JavaScriptSerializer js = new JavaScriptSerializer();
            string saveDataFileContent = File.ReadAllText(saveDataFilePath);
            SavedContent loadedContent = js.Deserialize<SavedContent>(saveDataFileContent);
            List<Game> currentGames = loadedContent.games;
            List<FriendSettings> currentFriends = loadedContent.friends;
            Settings updatedSettings = loadedContent.settings;
            List<string> currentCollections = loadedContent.collections;
            Notifications currentNotifications = loadedContent.notifications;
            List<string> currentCategories = loadedContent.categories;
            object rawIsChecked = hideOfflineFriendsFromCategoriesBtn.IsChecked;
            bool isChecked = ((bool)(rawIsChecked));
            updatedSettings.isHideOfflineFriendsFromCategories = isChecked;
            rawIsChecked = openNewChatInNewWindowBtn.IsChecked;
            isChecked = ((bool)(rawIsChecked));
            updatedSettings.isOpenNewChatInNewWindow = isChecked;
            rawIsChecked = notIncludeImagesAndMediaFilesBtn.IsChecked;
            isChecked = ((bool)(rawIsChecked)); 
            updatedSettings.isNotIncludeImagesAndMediaFiles = isChecked;
            rawIsChecked = showTimeIn24Btn.IsChecked;
            isChecked = ((bool)(rawIsChecked));
            updatedSettings.isShowTimeIn24 = isChecked;
            rawIsChecked = disableSpellCheckBtn.IsChecked;
            isChecked = ((bool)(rawIsChecked));
            updatedSettings.isDisableSpellCheck = isChecked;
            string savedContent = js.Serialize(new SavedContent
            {
                games = currentGames,
                friends = currentFriends,
                settings = updatedSettings,
                collections = currentCollections,
                notifications = currentNotifications,
                categories = currentCategories
            });
            File.WriteAllText(saveDataFilePath, savedContent);
        }

        public void LoadSettings ()
        {
            Environment.SpecialFolder localApplicationDataFolder = Environment.SpecialFolder.LocalApplicationData;
            string localApplicationDataFolderPath = Environment.GetFolderPath(localApplicationDataFolder);
            string saveDataFilePath = localApplicationDataFolderPath + @"\OfficeWare\GameManager\" + currentUserId + @"\save-data.txt";
            JavaScriptSerializer js = new JavaScriptSerializer();
            string saveDataFileContent = File.ReadAllText(saveDataFilePath);
            SavedContent loadedContent = js.Deserialize<SavedContent>(saveDataFileContent);
            Settings currentSettings = loadedContent.settings;
            bool isHideOfflineFriendsFromCategories = currentSettings.isHideOfflineFriendsFromCategories;
            hideOfflineFriendsFromCategoriesBtn.IsChecked = isHideOfflineFriendsFromCategories;
            showOfflineFriendsFromCategoriesBtn.IsChecked = !isHideOfflineFriendsFromCategories;
            bool isOpenNewChatInNewWindow = currentSettings.isOpenNewChatInNewWindow;
            openNewChatInNewWindowBtn.IsChecked = isOpenNewChatInNewWindow;
            openNewChatInNewTabBtn.IsChecked = !isOpenNewChatInNewWindow;
            bool isNotIncludeImagesAndMediaFiles = currentSettings.isNotIncludeImagesAndMediaFiles;
            notIncludeImagesAndMediaFilesBtn.IsChecked = isNotIncludeImagesAndMediaFiles;
            includeImagesAndMediaFilesBtn.IsChecked = !isNotIncludeImagesAndMediaFiles;
            bool isShowTimeIn24 = currentSettings.isDisableSpellCheck;
            showTimeIn24Btn.IsChecked = isShowTimeIn24;
            showTimeIn12Btn.IsChecked = !isShowTimeIn24;
            bool isDisableSpellCheck = currentSettings.isDisableSpellCheck;
            disableSpellCheckBtn.IsChecked = isDisableSpellCheck;
            enableSpellCheckBtn.IsChecked = !isDisableSpellCheck;
        }

    }
}
