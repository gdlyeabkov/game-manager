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
                    addNickAfterFriendNameLabel.Text = "Пример друга (Ник)";
                }
                else
                {
                    addNickAfterFriendNameBtn.IsChecked = toggledValue;
                    addNickAfterFriendNameLabel.Text = "Ник";
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
            List<string> currentRecentChats = loadedContent.recentChats;
            Recommendations currentReccomendations = loadedContent.recommendations;
            string currentLogoutDate = loadedContent.logoutDate;
            List<string> currentSections = loadedContent.sections;
            object rawIsChecked = addNickAfterFriendNameBtn.IsChecked;
            bool isChecked = ((bool)(rawIsChecked));
            updatedSettings.isAddNickAfterFriendNames = isChecked;
            rawIsChecked = hideOfflineFriendsFromCategoriesBtn.IsChecked;
            isChecked = ((bool)(rawIsChecked));
            updatedSettings.isHideOfflineFriendsFromCategories = isChecked;
            rawIsChecked = openNewChatInNewWindowBtn.IsChecked;
            isChecked = ((bool)(rawIsChecked));
            updatedSettings.isOpenNewChatInNewWindow = isChecked;
            rawIsChecked = notIncludeImagesAndMediaFilesBtn.IsChecked;
            isChecked = ((bool)(rawIsChecked)); 
            updatedSettings.isNotIncludeImagesAndMediaFiles = isChecked;

            rawIsChecked = rememberChatsBtn.IsChecked;
            isChecked = ((bool)(rawIsChecked));
            updatedSettings.isRestoreChats = isChecked;

            rawIsChecked = showTimeIn24Btn.IsChecked;
            isChecked = ((bool)(rawIsChecked));
            updatedSettings.isShowTimeIn24 = isChecked;
            rawIsChecked = disableSpellCheckBtn.IsChecked;
            isChecked = ((bool)(rawIsChecked));
            updatedSettings.isDisableSpellCheck = isChecked;
            
            rawIsChecked = friendListAndChatsCompactViewBtn.IsChecked;
            isChecked = ((bool)(rawIsChecked));
            updatedSettings.isFriendListAndChatsCompactView = isChecked;
            rawIsChecked = favoriteCompactViewBtn.IsChecked;
            isChecked = ((bool)(rawIsChecked));
            updatedSettings.isFavoriteCompactView = isChecked;
            object chatFontSizePanelData = chatFontSizePanel.DataContext;
            string chatFontSize = ((string)(chatFontSizePanelData));
            updatedSettings.chatFontSize = chatFontSize;

            rawIsChecked = friendPlayedNotificationCheckBox.IsChecked;
            isChecked = ((bool)(rawIsChecked));
            updatedSettings.isFriendPlayedNotification = isChecked;
            rawIsChecked = friendPlayedSoundCheckBox.IsChecked;
            isChecked = ((bool)(rawIsChecked));
            updatedSettings.isFriendPlayedSound = isChecked;
            rawIsChecked = friendOnlineNotificationCheckBox.IsChecked;
            isChecked = ((bool)(rawIsChecked));
            updatedSettings.isFriendOnlineNotification = isChecked;
            rawIsChecked = friendOnlineSoundCheckBox.IsChecked;
            isChecked = ((bool)(rawIsChecked));
            updatedSettings.isFriendOnlineSound = isChecked;
            rawIsChecked = friendSendMsgNotificationCheckBox.IsChecked;
            isChecked = ((bool)(rawIsChecked));
            updatedSettings.isFriendSendMsgNotification = isChecked;
            rawIsChecked = friendSendMsgSoundCheckBox.IsChecked;
            isChecked = ((bool)(rawIsChecked));
            updatedSettings.isFriendSendMsgSound = isChecked;
            rawIsChecked = friendSendTalkMsgNotificationCheckBox.IsChecked;
            isChecked = ((bool)(rawIsChecked));
            updatedSettings.isFriendSendTalkMsgNotification = isChecked;
            rawIsChecked = friendSendTalkMsgSoundCheckBox.IsChecked;
            isChecked = ((bool)(rawIsChecked));
            updatedSettings.isFriendSendTalkMsgSound = isChecked;
            rawIsChecked = friendSendTalkEventNotificationCheckBox.IsChecked;
            isChecked = ((bool)(rawIsChecked));
            updatedSettings.isFriendSendTalkEventNotification = isChecked;
            rawIsChecked = friendSendTalkEventSoundCheckBox.IsChecked;
            isChecked = ((bool)(rawIsChecked));
            updatedSettings.isFriendSendTalkEventSound = isChecked;
            object blinkTypePanelData = blinkTypePanel.DataContext;
            string blinkType = ((string)(blinkTypePanelData));
            updatedSettings.sendMsgBlinkWindowType = blinkType;
            string savedContent = js.Serialize(new SavedContent
            {
                games = currentGames,
                friends = currentFriends,
                settings = updatedSettings,
                collections = currentCollections,
                notifications = currentNotifications,
                categories = currentCategories,
                recentChats = currentRecentChats,
                recommendations = currentReccomendations,
                logoutDate = currentLogoutDate,
                sections = currentSections,
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
            bool isAddNickAfterFriendNames = currentSettings.isAddNickAfterFriendNames;
            addNickAfterFriendNameBtn.IsChecked = isAddNickAfterFriendNames;
            notAddNickAfterFriendNameBtn.IsChecked = !isAddNickAfterFriendNames;
            if (isAddNickAfterFriendNames)
            {
                addNickAfterFriendNameLabel.Text = "Пример друга (Ник)";
            }
            else
            {
                addNickAfterFriendNameLabel.Text = "Ник";
            }
            bool isHideOfflineFriendsFromCategories = currentSettings.isHideOfflineFriendsFromCategories;
            hideOfflineFriendsFromCategoriesBtn.IsChecked = isHideOfflineFriendsFromCategories;
            showOfflineFriendsFromCategoriesBtn.IsChecked = !isHideOfflineFriendsFromCategories;
            bool isOpenNewChatInNewWindow = currentSettings.isOpenNewChatInNewWindow;
            openNewChatInNewWindowBtn.IsChecked = isOpenNewChatInNewWindow;
            openNewChatInNewTabBtn.IsChecked = !isOpenNewChatInNewWindow;
            bool isNotIncludeImagesAndMediaFiles = currentSettings.isNotIncludeImagesAndMediaFiles;
            notIncludeImagesAndMediaFilesBtn.IsChecked = isNotIncludeImagesAndMediaFiles;
            includeImagesAndMediaFilesBtn.IsChecked = !isNotIncludeImagesAndMediaFiles;

            bool isRestoreChats = currentSettings.isRestoreChats;
            rememberChatsBtn.IsChecked = isRestoreChats;
            forgetChatsBtn.IsChecked = !isRestoreChats;

            bool isShowTimeIn24 = currentSettings.isShowTimeIn24;
            showTimeIn24Btn.IsChecked = isShowTimeIn24;
            showTimeIn12Btn.IsChecked = !isShowTimeIn24;
            bool isDisableSpellCheck = currentSettings.isDisableSpellCheck;
            disableSpellCheckBtn.IsChecked = isDisableSpellCheck;
            enableSpellCheckBtn.IsChecked = !isDisableSpellCheck;

            bool isFriendListAndChatsCompactView = currentSettings.isFriendListAndChatsCompactView;
            friendListAndChatsCompactViewBtn.IsChecked = isFriendListAndChatsCompactView;
            friendListAndChatsUnCompactViewBtn.IsChecked = !isFriendListAndChatsCompactView;
            bool isFavoriteCompactView = currentSettings.isFavoriteCompactView;
            favoriteCompactViewBtn.IsChecked = isFavoriteCompactView;
            favoriteUnCompactViewBtn.IsChecked = !isFavoriteCompactView;
            string chatFontSize = currentSettings.chatFontSize;
            bool isSmallChatFontSize = chatFontSize == "small";
            bool isStandardChatFontSize = chatFontSize == "standard";
            bool isBigChatFontSize = chatFontSize == "big";
            if (isSmallChatFontSize)
            {
                ToggleChatFontSize(smallChatFontSizeBtn);
            }
            else if (isStandardChatFontSize)
            {
                ToggleChatFontSize(standardChatFontSizeBtn);
            }
            else if (isBigChatFontSize)
            {
                ToggleChatFontSize(bigChatFontSizeBtn);
            }

            bool isFriendPlayedNotification = currentSettings.isFriendPlayedNotification;
            friendPlayedNotificationCheckBox.IsChecked = isFriendPlayedNotification;
            bool isFriendPlayedSound = currentSettings.isFriendPlayedSound;
            friendPlayedSoundCheckBox.IsChecked = isFriendPlayedSound;
            bool isFriendOnlineNotification = currentSettings.isFriendOnlineNotification;
            friendOnlineNotificationCheckBox.IsChecked = isFriendOnlineNotification;
            bool isFriendOnlineSound = currentSettings.isFriendOnlineSound;
            friendOnlineSoundCheckBox.IsChecked = isFriendOnlineSound;
            bool isFriendSendMsgNotification = currentSettings.isFriendSendMsgNotification;
            friendSendMsgNotificationCheckBox.IsChecked = isFriendSendMsgNotification;
            bool isFriendSendMsgSound = currentSettings.isFriendSendMsgSound;
            friendSendMsgSoundCheckBox.IsChecked = isFriendSendMsgSound;
            bool isFriendSendTalkMsgNotification = currentSettings.isFriendSendTalkMsgNotification;
            friendSendTalkMsgNotificationCheckBox.IsChecked = isFriendSendTalkMsgNotification;
            bool isFriendSendTalkMsgSound = currentSettings.isFriendSendTalkMsgSound;
            friendSendTalkMsgSoundCheckBox.IsChecked = isFriendSendTalkMsgSound;
            bool isFriendSendTalkEventNotification = currentSettings.isFriendSendTalkEventNotification;
            friendSendTalkEventNotificationCheckBox.IsChecked = isFriendSendTalkEventNotification;
            bool isFriendSendTalkEventSound = currentSettings.isFriendSendTalkEventSound;
            friendSendTalkEventSoundCheckBox.IsChecked = isFriendSendTalkEventSound;
            string blinkType = currentSettings.sendMsgBlinkWindowType;
            bool isAlwaysBlinkType = blinkType == "always";
            bool isMinimizeBlinkType = blinkType == "minimize";
            bool isNeverBlinkType = blinkType == "never";
            if (isAlwaysBlinkType)
            {
                ToggleSendMsgBlinkWindowType(alwaysSendMsgBlinkWindowTypeBtn);
            }
            else if (isMinimizeBlinkType)
            {
                ToggleSendMsgBlinkWindowType(minimizeSendMsgBlinkWindowTypeBtn);
            }
            else if (isNeverBlinkType)
            {
                ToggleSendMsgBlinkWindowType(neverSendMsgBlinkWindowTypeBtn);
            }
        }

        private void ToggleChatFontSizeHandler (object sender, RoutedEventArgs e)
        {
            Button btn = ((Button)(sender));
            ToggleChatFontSize(btn);
        }

        public void ToggleChatFontSize (Button btn)
        {
            object btnData = btn.DataContext;
            string chatFontSize = btnData.ToString();
            smallChatFontSizeBtn.Background = System.Windows.Media.Brushes.LightGray;
            standardChatFontSizeBtn.Background = System.Windows.Media.Brushes.LightGray;
            bigChatFontSizeBtn.Background = System.Windows.Media.Brushes.LightGray;
            btn.Background = System.Windows.Media.Brushes.SkyBlue;
            chatFontSizePanel.DataContext = chatFontSize;
        }

        private void ToggleSendMsgBlinkWindowTypeHandler (object sender, RoutedEventArgs e)
        {
            Button btn = ((Button)(sender));
            ToggleSendMsgBlinkWindowType(btn);
        }

        public void ToggleSendMsgBlinkWindowType (Button btn)
        {
            object btnData = btn.DataContext;
            string blinkType = btnData.ToString();
            alwaysSendMsgBlinkWindowTypeBtn.Background = System.Windows.Media.Brushes.LightGray;
            minimizeSendMsgBlinkWindowTypeBtn.Background = System.Windows.Media.Brushes.LightGray;
            neverSendMsgBlinkWindowTypeBtn.Background = System.Windows.Media.Brushes.LightGray;
            btn.Background = System.Windows.Media.Brushes.SkyBlue;
            blinkTypePanel.DataContext = blinkType;
        }

    }
}
