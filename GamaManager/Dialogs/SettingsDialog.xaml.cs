using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
    /// Логика взаимодействия для SettingsDialog.xaml
    /// </summary>
    public partial class SettingsDialog : Window
    {

        public string currentUserId = "";
        public Brush transparentBrush;
        public Brush selectedBrush;

        public SettingsDialog (string currentUserId)
        {
            InitializeComponent();

            Initialize(currentUserId);
        
        }

        public void Initialize (string currentUserId)
        {
            InitConstants(currentUserId);
            LoadSettings();
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
            string currentLang = currentSettings.language;
            int currentStartWindow = currentSettings.startWindow;
            string currentOverlayHotKey = currentSettings.overlayHotKey;
            string currentScreenShotsHotKey = currentSettings.screenShotsHotKey;
            string currentFrames = currentSettings.frames;
            bool currentShowScreenShotsNotification = currentSettings.showScreenShotsNotification;
            bool currentPlayScreenShotsNotification = currentSettings.playScreenShotsNotification;
            bool currentSaveScreenShotsCopy = currentSettings.saveScreenShotsCopy;
            MusicSettings currentMusicSettings = currentSettings.music;
            List<string> currentMusicSettingsPaths = currentMusicSettings.paths;
            double currentMusicSettingsVolume = currentMusicSettings.volume;
            foreach (var currentMusicSettingsPath in currentMusicSettingsPaths)
            {
                string newLine = Environment.NewLine;
                string path = currentMusicSettingsPath + newLine;
                musicLibraryListBox.Text += path;
            }
            musicSettingsVolumeSlider.Value = currentMusicSettingsVolume;
            ItemCollection langSelectorItems = langSelector.Items;
            foreach (ComboBoxItem langSelectorItem in langSelectorItems)
            {
                object rawSelectedLangData = langSelectorItem.DataContext;
                string selectedLangData = ((string)(rawSelectedLangData));
                bool isLangFound = selectedLangData == currentLang;
                if (isLangFound)
                {
                    int currentLangIndex = langSelectorItems.IndexOf(langSelectorItem);
                    langSelector.SelectedIndex = currentLangIndex;
                    break;
                }
            }
            startWindowSelector.SelectedIndex = currentStartWindow;
            overlayHotKeyBox.Text = currentOverlayHotKey;
            screenShotsHotKeyBox.Text = currentScreenShotsHotKey;
            ItemCollection framesBoxItems = framesBox.Items;
            foreach (ComboBoxItem framesBoxItem in framesBoxItems)
            {
                object rawSelectedframesData = framesBoxItem.DataContext;
                string selectedFramesData = ((string)(rawSelectedframesData));
                bool isFramesFound = selectedFramesData == currentFrames;
                if (isFramesFound)
                {
                    int currentFramesIndex = framesBoxItems.IndexOf(framesBoxItem);
                    framesBox.SelectedIndex = currentFramesIndex;
                    break;
                }
            }
            showScreenShotsNotificationCheckBox.IsChecked = currentShowScreenShotsNotification;
            playScreenShotsNotificationCheckBox.IsChecked = currentPlayScreenShotsNotification;
            saveScreenShotsCopyCheckBox.IsChecked = currentSaveScreenShotsCopy;
        }

        public void InitConstants (string currentUserId)
        {
            this.currentUserId = currentUserId;
            transparentBrush = System.Windows.Media.Brushes.Transparent;
            selectedBrush = System.Windows.Media.Brushes.LightGray;
        }


        public void SelectSettingsTabHandler (object sender, MouseEventArgs e)
        {
            StackPanel tab = ((StackPanel)(sender));
            object tabData = tab.DataContext;
            string rawTabIndex = ((string)(tabData));
            int tabIndex = Int32.Parse(rawTabIndex);
            SelectSettingsTab(tabIndex, tab);
        }

        public void SelectSettingsTab (int tabIndex, StackPanel selectedTab)
        {
            settingsControl.SelectedIndex = tabIndex;
            foreach (StackPanel tab in tabs.Children)
            {
                tab.Background = transparentBrush;
            }
            selectedTab.Background = selectedBrush;
        }

        private void SaveSettingsHandler(object sender, RoutedEventArgs e)
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
            int selectedLangIndex = langSelector.SelectedIndex;
            ItemCollection langSelectorItems = langSelector.Items;
            object rawSelectedLang = langSelectorItems[selectedLangIndex];
            ComboBoxItem selectedLang = ((ComboBoxItem)(rawSelectedLang));
            object rawSelectedLangData = selectedLang.DataContext;
            string selectedLangData = ((string)(rawSelectedLangData));
            updatedSettings.language = selectedLangData;
            int selectedStartWindowIndex = startWindowSelector.SelectedIndex;
            updatedSettings.startWindow = selectedStartWindowIndex;
            string overlayHotKey = overlayHotKeyBox.Text;
            updatedSettings.overlayHotKey = overlayHotKey;
            MusicSettings musicSettings = new MusicSettings();
            musicSettings.paths = new List<string>();
            int lineIndex = -1;
            while (true)
            {
                lineIndex++;
                try
                {
                    string lineContent = musicLibraryListBox.GetLineText(lineIndex);
                    bool isLineContentExists = lineContent != null;
                    if (isLineContentExists)
                    {
                        lineContent = lineContent.Trim();
                        int lineContentLength = lineContent.Length;
                        bool isNotFirstCaretBreak = lineContent != @"\r\n";
                        bool isNotSecondCaretBreak = lineContent != @"\n";
                        bool isNotThirdCaretBreak = lineContent != @"\r";
                        bool isNotFourthCaretBreak = lineContent != @"";
                        bool isPath = isNotFirstCaretBreak && isNotSecondCaretBreak && isNotThirdCaretBreak && isNotFourthCaretBreak;
                        if (isPath)
                        {
                            musicSettings.paths.Add(lineContent);
                        }
                        else
                        {
                            continue;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                catch (ArgumentOutOfRangeException)
                {
                    break;
                }
            }
            double musicSettingsVolume = musicSettingsVolumeSlider.Value;
            musicSettings.volume = musicSettingsVolume;
            updatedSettings.music = musicSettings;
            string screenShotsHotKey = screenShotsHotKeyBox.Text;
            updatedSettings.screenShotsHotKey = screenShotsHotKey;
            int selectedFramesBoxItemIndex = framesBox.SelectedIndex;
            ItemCollection framesBoxItems = framesBox.Items;
            object rawSelectedFramesBoxItem = framesBoxItems[selectedFramesBoxItemIndex];
            ComboBoxItem selectedFramesBoxItem = ((ComboBoxItem)(rawSelectedFramesBoxItem));
            object rawFrames = selectedFramesBoxItem.DataContext;
            string frames = ((string)(rawFrames));
            updatedSettings.frames = frames;
            object rawIsShowScreenShotsNotification = showScreenShotsNotificationCheckBox.IsChecked;
            bool isShowScreenShotsNotification = ((bool)(rawIsShowScreenShotsNotification));
            updatedSettings.showScreenShotsNotification = isShowScreenShotsNotification;
            object rawIsPlayScreenShotsNotification = playScreenShotsNotificationCheckBox.IsChecked;
            bool isPlayScreenShotsNotification = ((bool)(rawIsPlayScreenShotsNotification));
            updatedSettings.playScreenShotsNotification = isPlayScreenShotsNotification;
            object rawIsSaveScreenShotsCopy = saveScreenShotsCopyCheckBox.IsChecked;
            bool isSaveScreenShotsCopy = ((bool)(rawIsSaveScreenShotsCopy));
            updatedSettings.saveScreenShotsCopy = isSaveScreenShotsCopy;

            string savedContent = js.Serialize(new SavedContent
            {
                games = currentGames,
                friends = currentFriends,
                settings = updatedSettings
            });
            File.WriteAllText(saveDataFilePath, savedContent);
            this.Close();
        }

        private void CancelHandler (object sender, RoutedEventArgs e)
        {
            Cancel();
        }

        public void Cancel ()
        {
            this.Close();
        }

        private void OverlayHotKeyHandler (object sender, KeyEventArgs e)
        {
            TextBox input = ((TextBox)(sender));
            Key currentKey = e.Key;
            OverlayHotKey(input, currentKey);
        }

        public void OverlayHotKey(TextBox input, Key key)
        {
            Key leftShiftKey = Key.LeftShift;
            Key rightShiftKey = Key.RightShift;
            Key leftCtrlKey = Key.LeftCtrl;
            Key rightCtrlKey = Key.RightCtrl;
            bool isNotLeftShiftKey = key != leftShiftKey;
            bool isNotRightShiftKey = key != rightShiftKey;
            bool isNotShiftKey = isNotLeftShiftKey && isNotRightShiftKey;
            bool isNotLeftCtrlKey = key != leftCtrlKey;
            bool isNotRightCtrlKey = key != rightCtrlKey;
            bool isNotCtrlKey = isNotLeftCtrlKey && isNotRightCtrlKey;
            bool isNotKeyModifier = isNotShiftKey && isNotCtrlKey;
            if (isNotKeyModifier)
            {
                bool isCtrlEnabled = (Keyboard.Modifiers & ModifierKeys.Control) > 0;
                bool isShiftEnabled = (Keyboard.Modifiers & ModifierKeys.Shift) > 0;
                string rawHotKey = key.ToString();
                if (isShiftEnabled)
                {
                    rawHotKey = "Shift + " + rawHotKey;
                }
            
                if (isCtrlEnabled)
                {
                    rawHotKey = "Ctrl + " + rawHotKey;
                }
                input.Text = rawHotKey;
            }
        }

        private void AddPathToMusicSettingsHandler (object sender, RoutedEventArgs e)
        {
            AddPathToMusicSettings();
        }

        public void AddPathToMusicSettings ()
        {
            System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();
            fbd.Description = "Выберите музыкальную библиотеку";
            System.Windows.Forms.DialogResult res = fbd.ShowDialog();
            System.Windows.Forms.DialogResult okResult = System.Windows.Forms.DialogResult.OK;
            bool isOpened = res == okResult;
            if (isOpened)
            {
                string path = fbd.SelectedPath;
                string newLine = Environment.NewLine;
                string newPath = newLine + path;
                musicLibraryListBox.Text += newPath;
            }
        }

        
        private void SelectMusicLibraryHandler (object sender, RoutedEventArgs e)
        {
            SelectMusicLibrary();
        }

        public void SelectMusicLibrary ()
        {
            int caretIndex = musicLibraryListBox.CaretIndex;
            int lineIndex = musicLibraryListBox.GetLineIndexFromCharacterIndex(caretIndex);
            int startSelectionIndex = musicLibraryListBox.GetCharacterIndexFromLineIndex(lineIndex);
            int endSelectionIndex = musicLibraryListBox.GetLineLength(lineIndex);
            musicLibraryListBox.Select(startSelectionIndex, endSelectionIndex);
        }

        private void RemoveMusicLibraryHandler (object sender, RoutedEventArgs e)
        {
            RemoveMusicLibrary();
        }

        public void RemoveMusicLibrary ()
        {
            int selectionLength = musicLibraryListBox.SelectionLength;
            bool isLibrarySelected = selectionLength >= 1;
            if (isLibrarySelected)
            {
                string selectedText = musicLibraryListBox.SelectedText;
                musicLibraryListBox.Text = musicLibraryListBox.Text.Replace(selectedText, "");
            }
        }

        private void ScreenShotsHotKeyHandler (object sender, KeyEventArgs e)
        {
            TextBox input = ((TextBox)(sender));
            Key currentKey = e.Key;
            ScreenShotsHotKey(input, currentKey);
        }

        public void ScreenShotsHotKey (TextBox input, Key key)
        {
            Key leftShiftKey = Key.LeftShift;
            Key rightShiftKey = Key.RightShift;
            Key leftCtrlKey = Key.LeftCtrl;
            Key rightCtrlKey = Key.RightCtrl;
            bool isNotLeftShiftKey = key != leftShiftKey;
            bool isNotRightShiftKey = key != rightShiftKey;
            bool isNotShiftKey = isNotLeftShiftKey && isNotRightShiftKey;
            bool isNotLeftCtrlKey = key != leftCtrlKey;
            bool isNotRightCtrlKey = key != rightCtrlKey;
            bool isNotCtrlKey = isNotLeftCtrlKey && isNotRightCtrlKey;
            bool isNotKeyModifier = isNotShiftKey && isNotCtrlKey;
            if (isNotKeyModifier)
            {
                bool isCtrlEnabled = (Keyboard.Modifiers & ModifierKeys.Control) > 0;
                bool isShiftEnabled = (Keyboard.Modifiers & ModifierKeys.Shift) > 0;
                string rawHotKey = key.ToString();
                if (isShiftEnabled)
                {
                    rawHotKey = "Shift + " + rawHotKey;
                }

                if (isCtrlEnabled)
                {
                    rawHotKey = "Ctrl + " + rawHotKey;
                }
                input.Text = rawHotKey;
            }
        }

        private void OpenScreenShotsFolderHandler (object sender, RoutedEventArgs e)
        {
            OpenScreenShotsFolder();
        }

        public void OpenScreenShotsFolder ()
        {
            Environment.SpecialFolder localApplicationDataFolder = Environment.SpecialFolder.LocalApplicationData;
            string localApplicationDataFolderPath = Environment.GetFolderPath(localApplicationDataFolder);
            // string screenShotsPath = localApplicationDataFolderPath + @"\OfficeWare\GameManager\" + currentUserId;
            string screenShotsPath = localApplicationDataFolderPath + @"\OfficeWare\GameManager\" + currentUserId + @"\screenshots";
            System.Diagnostics.Process.Start(new ProcessStartInfo
            {
                FileName = "explorer",
                Arguments = screenShotsPath,
                UseShellExecute = true
            });
        }

    }
}
