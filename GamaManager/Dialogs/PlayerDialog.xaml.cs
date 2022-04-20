using MaterialDesignThemes.Wpf;
using NAudio.Wave;
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
using System.Windows.Threading;

namespace GamaManager.Dialogs
{
    /// <summary>
    /// Логика взаимодействия для PlayerDialog.xaml
    /// </summary>
    public partial class PlayerDialog : Window
    {

        public string currentUserId = "";
        public bool isPlaying = false;
        public Uri firstTrackUri;
        public List<string> tracks = new List<string>();
        public int tracksCursor = 0;
        public string rawCountTracks = "0";
        public PackIconKind playIcon;
        public PackIconKind pauseIcon;
        public bool isRepeatTrack = false;
        public Brush disabledBrush;
        public Brush enabledBrush;
        public DispatcherTimer timer = null;
        public string fromLabelContent = "";

        public PlayerDialog(string currentUserId)
        {
            InitializeComponent();

            Initialize(currentUserId);
        
        }

        public void InitConstants ()
        {
            playIcon = PackIconKind.Play;
            pauseIcon = PackIconKind.Pause;
            enabledBrush = System.Windows.Media.Brushes.Blue;
            disabledBrush = System.Windows.Media.Brushes.Black;
            fromLabelContent = Properties.Resources.fromLabelContent;
        }

        public void Initialize (string currentUserId)
        {

            InitConstants();

            this.currentUserId = currentUserId;
            
            Environment.SpecialFolder localApplicationDataFolder = Environment.SpecialFolder.LocalApplicationData;
            string localApplicationDataFolderPath = Environment.GetFolderPath(localApplicationDataFolder);
            string saveDataFilePath = localApplicationDataFolderPath + @"\OfficeWare\GameManager\" + currentUserId + @"\save-data.txt";
            JavaScriptSerializer js = new JavaScriptSerializer();
            string saveDataFileContent = File.ReadAllText(saveDataFilePath);
            SavedContent loadedContent = js.Deserialize<SavedContent>(saveDataFileContent);
            Settings currentSettings = loadedContent.settings;
            MusicSettings musicSettings = currentSettings.music;
            List<string> paths = musicSettings.paths;
            int localTracksCursor = -1;
            foreach (string path in paths)
            {
                string[] files = Directory.GetFileSystemEntries(path);
                foreach (string filePath in files)
                {
                    string extension = System.IO.Path.GetExtension(filePath);
                    bool isSound = extension == ".mp3";
                    if (isSound)
                    {
                        localTracksCursor++;
                        tracks.Add(filePath);
                    }
                }
            }

            GetTracks();
            
            int countTracks = localTracksCursor + 1;
            rawCountTracks = countTracks.ToString();
            ResetTrackStatsLabel();
        }

        public void GetTracks ()
        {
            libraries.Children.Clear();
            int localTracksCursor = -1;
            foreach (string track in tracks)
            {
                localTracksCursor++;
                bool isFirstSound = localTracksCursor == 0;
                if (isFirstSound)
                {
                    firstTrackUri = new Uri(track);
                }
                /*StackPanel librariesItem = new StackPanel();
                librariesItem.Orientation = Orientation.Horizontal;
                TextBlock librariesItemNameLabel = new TextBlock();
                FileInfo fileInfo = new FileInfo(track);
                string librariesItemNameLabelContent = fileInfo.Name;
                librariesItemNameLabel.Text = librariesItemNameLabelContent;
                librariesItemNameLabel.Margin = new Thickness(25, 5, 25, 5);
                librariesItem.Children.Add(librariesItemNameLabel);
                TextBlock librariesItemDurationLabel = new TextBlock();
                Mp3FileReader audioReader = new Mp3FileReader(track);
                TimeSpan soundDuration = audioReader.TotalTime;
                int soundDurationMinutes = soundDuration.Minutes;
                string rawSoundDurationMinutes = soundDurationMinutes.ToString();
                int rawDurationLength = rawSoundDurationMinutes.Length;
                bool isAddPrefix = rawDurationLength <= 1;
                if (isAddPrefix)
                {
                    rawSoundDurationMinutes = "0" + rawSoundDurationMinutes;
                }
                int soundDurationSeconds = soundDuration.Seconds;
                string rawSoundDurationSeconds = soundDurationSeconds.ToString();
                rawDurationLength = rawSoundDurationSeconds.Length;
                isAddPrefix = rawDurationLength <= 1;
                if (isAddPrefix)
                {
                    rawSoundDurationSeconds = "0" + rawSoundDurationSeconds;
                }
                string rawDuration = rawSoundDurationMinutes + ":" + rawSoundDurationSeconds;
                librariesItemDurationLabel.Text = rawDuration;
                librariesItemDurationLabel.HorizontalAlignment = HorizontalAlignment.Right;
                librariesItemDurationLabel.TextAlignment = TextAlignment.Right;
                librariesItemDurationLabel.Margin = new Thickness(25, 5, 25, 5);
                librariesItemDurationLabel.HorizontalAlignment = HorizontalAlignment.Right;
                librariesItem.Children.Add(librariesItemDurationLabel);
                libraries.Children.Add(librariesItem);*/
                RowDefinition librariesItem = new RowDefinition();
                libraries.RowDefinitions.Add(librariesItem);
                RowDefinitionCollection rows = libraries.RowDefinitions;
                int countRows = rows.Count;
                int lastRowIndex = countRows - 1;
                TextBlock librariesItemNameLabel = new TextBlock();
                FileInfo fileInfo = new FileInfo(track);
                string librariesItemNameLabelContent = fileInfo.Name;
                librariesItemNameLabel.Text = librariesItemNameLabelContent;
                librariesItemNameLabel.Margin = new Thickness(25, 5, 25, 5);
                libraries.Children.Add(librariesItemNameLabel);
                Grid.SetRow(librariesItemNameLabel, lastRowIndex);
                Grid.SetColumn(librariesItemNameLabel, 0);
                TextBlock librariesItemDurationLabel = new TextBlock();
                Mp3FileReader audioReader = new Mp3FileReader(track);
                TimeSpan soundDuration = audioReader.TotalTime;
                int soundDurationMinutes = soundDuration.Minutes;
                string rawSoundDurationMinutes = soundDurationMinutes.ToString();
                int rawDurationLength = rawSoundDurationMinutes.Length;
                bool isAddPrefix = rawDurationLength <= 1;
                if (isAddPrefix)
                {
                    rawSoundDurationMinutes = "0" + rawSoundDurationMinutes;
                }
                int soundDurationSeconds = soundDuration.Seconds;
                string rawSoundDurationSeconds = soundDurationSeconds.ToString();
                rawDurationLength = rawSoundDurationSeconds.Length;
                isAddPrefix = rawDurationLength <= 1;
                if (isAddPrefix)
                {
                    rawSoundDurationSeconds = "0" + rawSoundDurationSeconds;
                }
                string rawDuration = rawSoundDurationMinutes + ":" + rawSoundDurationSeconds;
                librariesItemDurationLabel.Text = rawDuration;
                librariesItemDurationLabel.HorizontalAlignment = HorizontalAlignment.Right;
                librariesItemDurationLabel.TextAlignment = TextAlignment.Right;
                librariesItemDurationLabel.Margin = new Thickness(25, 5, 25, 5);
                librariesItemDurationLabel.HorizontalAlignment = HorizontalAlignment.Right;
                libraries.Children.Add(librariesItemDurationLabel);
                Grid.SetRow(librariesItemDurationLabel, lastRowIndex);
                Grid.SetColumn(librariesItemDurationLabel, 1);

            }
        }

        private void OpenVolumeDialogHandler(object sender, MouseButtonEventArgs e)
        {
            OpenVolumeDialog();
        }

        public void OpenVolumeDialog ()
        {
            volumeDialog.IsOpen = true;
        }

        private void RepeatTrackHandler (object sender, MouseButtonEventArgs e)
        {
            PackIcon icon = ((PackIcon)(sender));
            RepeatTrack(icon);
        }

        public void RepeatTrack (PackIcon icon)
        {
            isRepeatTrack = !isRepeatTrack;
            if (isRepeatTrack)
            {
                icon.Foreground = enabledBrush;
            }
            else
            {
                icon.Foreground = disabledBrush;
            }
        }

        private void ShuffleTracksHandler (object sender, MouseButtonEventArgs e)
        {
            ShuffleTracks();
        }

        public void ShuffleTracks ()
        {
            var r = new Random();
            tracks = tracks.OrderBy(x => r.Next()).ToList<string>();
            GetTracks();
        }

        private void PlayPreviousTrackHandler (object sender, MouseButtonEventArgs e)
        {
            PlayPreviousTrack();
        }

        public void PlayPreviousTrack ()
        {
            bool isNotFirstSound = tracksCursor > 0;
            if (isNotFirstSound)
            {
                tracksCursor--;
                int activeTrackIndex = tracksCursor;
                int newTrackIndex = activeTrackIndex;
                int countTracks = tracks.Count;
                bool isTrackAvailable = newTrackIndex > -1 && newTrackIndex < countTracks;
                if (isTrackAvailable)
                {
                    int currentTrackNumber = newTrackIndex + 1;
                    string rawCurrentTrackNumber = currentTrackNumber.ToString();
                    string fromLabelContent = Properties.Resources.fromLabelContent;
                    string tracksStatsLabelContent = rawCurrentTrackNumber + fromLabelContent + rawCountTracks;
                    tracksStatsLabel.Text = tracksStatsLabelContent;
                    string newTrack = tracks[newTrackIndex];
                    firstTrackUri = new Uri(newTrack);
                    audio.Source = firstTrackUri;
                }
                else
                {
                    /*countTracks = tracks.Count;
                    int lastTrackIndex = countTracks - 1;
                    string newTrack = tracks[lastTrackIndex];
                    firstTrackUri = new Uri(newTrack);
                    isPlaying = false;
                    tracksCursor = 0;
                    ResetTrackStatsLabel();
                    */
                }

                SetActiveTrackName();

            }
            Debugger.Log(0, "debug", Environment.NewLine + "tracksCursor: " + tracksCursor + Environment.NewLine);

            UpdateTimeLine();

        }

        public void SetActiveTrackName ()
        {
            string activeTrack = tracks[tracksCursor];
            string activeTrackName = System.IO.Path.GetFileName(activeTrack);
            activeTrackNameLabel.Text = activeTrackName;
        }

        public void UpdateTimeLine()
        {
            timeline.Value = 0;
            bool isTimerExists = timer != null;
            if (isTimerExists)
            {
                timer.Stop();
            }
            timer = new DispatcherTimer();
            string currentTrack = tracks[tracksCursor];
            Mp3FileReader reader = new Mp3FileReader(currentTrack);
            TimeSpan totalTime = reader.TotalTime;
            double totalSeconds = totalTime.TotalSeconds;
            double delta = 100 / totalSeconds;
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += delegate
            {
                timeline.Value += delta;
            };
            timer.Start();
        }

        private void PlayTrackHandler (object sender, MouseButtonEventArgs e)
        {
            PackIcon icon = ((PackIcon)(sender));
            PlayTrackHandler(icon);
        }

        public void PlayTrackHandler (PackIcon icon)
        {
            if (isPlaying)
            {
                audio.LoadedBehavior = MediaState.Pause;
                icon.Kind = playIcon;

                string audioStatePausedLabelContent = Properties.Resources.audioStatePausedLabelContent;
                audioStateLabel.Text = audioStatePausedLabelContent;

            }
            else
            {
                audio.LoadedBehavior = MediaState.Play;
                audio.Source = firstTrackUri;

                int currentTrackNumber = tracksCursor + 1;
                string rawCurrentTrackNumber = currentTrackNumber.ToString();
                string fromLabelContent = Properties.Resources.fromLabelContent;
                string tracksStatsLabelContent = rawCurrentTrackNumber + fromLabelContent + rawCountTracks;
                tracksStatsLabel.Text = tracksStatsLabelContent;
                
                icon.Kind = pauseIcon;

                SetActiveTrackName();

                string audioStatePlayingLabelContent = Properties.Resources.audioStatePlayingLabelContent;
                audioStateLabel.Text = audioStatePlayingLabelContent;

            }
            isPlaying = !isPlaying;
        }
        

        private void PlayNextTrackHandler (object sender, MouseButtonEventArgs e)
        {
            PlayNextTrack();
        }

        public void PlayNextTrack()
        {
            TrackEnded();
        }

        public void TrackEnded ()
        {
            bool isNotRepeatTrack = !isRepeatTrack;
            if (isNotRepeatTrack)
            {
                tracksCursor++;
            }
            int activeTrackIndex = tracksCursor - 1;
            int newTrackIndex = activeTrackIndex + 1;
            int countTracks = tracks.Count;
            bool isTrackAvailable = newTrackIndex > 0 && newTrackIndex < countTracks;
            if (isTrackAvailable)
            {
                int currentTrackNumber = newTrackIndex + 1;
                string rawCurrentTrackNumber = currentTrackNumber.ToString();
                string fromLabelContent = Properties.Resources.fromLabelContent;
                string tracksStatsLabelContent = rawCurrentTrackNumber + fromLabelContent + rawCountTracks;
                tracksStatsLabel.Text = tracksStatsLabelContent;
                string newTrack = tracks[newTrackIndex];
                firstTrackUri = new Uri(newTrack);
                audio.Source = firstTrackUri;
            }
            else
            {
                int currentTrackNumber = 1;
                string rawCurrentTrackNumber = currentTrackNumber.ToString();
                string fromLabelContent = Properties.Resources.fromLabelContent;
                string tracksStatsLabelContent = rawCurrentTrackNumber + fromLabelContent + rawCountTracks;
                tracksStatsLabel.Text = tracksStatsLabelContent;
                string newTrack = tracks[0];
                firstTrackUri = new Uri(newTrack);
                isPlaying = false;
                tracksCursor = 0;
            }
            Debugger.Log(0, "debug", Environment.NewLine + "tracksCursor: " + tracksCursor + Environment.NewLine);

            UpdateTimeLine();

            SetActiveTrackName();

        }

        private void  TrackEndedHandler (object sender, RoutedEventArgs e)
        {
            TrackEnded();
        }

        public void ResetTrackStatsLabel ()
        {
            string fromLabelContent = Properties.Resources.fromLabelContent;
            string tracksStatsLabelContent = "0" + fromLabelContent + rawCountTracks;
            tracksStatsLabel.Text = tracksStatsLabelContent;
        }

        private void SetVolumeHandler (object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Slider slider = ((Slider)(sender));
            double volume = slider.Value;
            SetVolume(volume);
        }

        public void SetVolume (double volume)
        {
            double updatedVolume = volume;
            audio.Volume = updatedVolume;
        }

        private void SetTimeLinePositionHandler (object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            SetTimeLinePosition();
        }

        private void SetTimeLinePosition ()
        {
            string currentTrack = tracks[tracksCursor];
            Mp3FileReader reader = new Mp3FileReader(currentTrack);
            TimeSpan totalTime = reader.TotalTime;
            double totalSeconds = totalTime.TotalSeconds;
            double delta = totalSeconds / 100;
            double position = delta * timeline.Value;
            audio.Position = TimeSpan.FromSeconds(position);
        }

    }
}
