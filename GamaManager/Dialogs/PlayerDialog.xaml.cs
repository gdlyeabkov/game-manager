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

        public PlayerDialog(string currentUserId)
        {
            InitializeComponent();

            Initialize(currentUserId);
        
        }

        public void Initialize (string currentUserId)
        {
            
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
            int tracksCursor = -1;
            foreach (string path in paths)
            {
                string[] files = Directory.GetFileSystemEntries(path);
                foreach (string filePath in files)
                {
                    string extension = System.IO.Path.GetExtension(filePath);
                    bool isSound = extension == ".mp3";
                    if (isSound)
                    {

                        tracksCursor++;
                        bool isFirstSound = tracksCursor == 0;
                        if (isFirstSound)
                        {
                            firstTrackUri = new Uri(filePath);
                        }

                        StackPanel librariesItem = new StackPanel();
                        librariesItem.Orientation = Orientation.Horizontal;
                        TextBlock librariesItemNameLabel = new TextBlock();
                        FileInfo fileInfo = new FileInfo(filePath);
                        string librariesItemNameLabelContent = fileInfo.Name;
                        librariesItemNameLabel.Text = librariesItemNameLabelContent;
                        librariesItemNameLabel.Margin = new Thickness(25, 5, 25, 5);
                        librariesItem.Children.Add(librariesItemNameLabel);
                        TextBlock librariesItemDurationLabel = new TextBlock();
                        Mp3FileReader audioReader = new Mp3FileReader(filePath);
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
                        string rawSoundDurationSeconds = soundDurationMinutes.ToString();
                        rawDurationLength = rawSoundDurationSeconds.Length;
                        isAddPrefix = rawDurationLength <= 1;
                        if (isAddPrefix)
                        {
                            rawSoundDurationSeconds = "0" + rawSoundDurationSeconds;
                        }
                        string rawDuration = rawSoundDurationMinutes + ":" + rawSoundDurationSeconds;
                        librariesItemDurationLabel.Text = rawDuration;
                        librariesItemDurationLabel.Margin = new Thickness(25, 5, 25, 5);
                        librariesItemDurationLabel.HorizontalAlignment = HorizontalAlignment.Right;
                        librariesItem.Children.Add(librariesItemDurationLabel);
                        libraries.Children.Add(librariesItem);
                    
                        tracks.Add(filePath);

                    }
                }
            }
            int countTracks = tracksCursor + 1;
            rawCountTracks = countTracks.ToString();
            ResetTrackStatsLabel();
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
            RepeatTrack();
        }

        public void RepeatTrack ()
        {

        }

        private void ShuffleTracksHandler (object sender, MouseButtonEventArgs e)
        {
            ShuffleTracks();
        }

        public void ShuffleTracks ()
        {

        }

        private void PlayPreviousTrackHandler (object sender, MouseButtonEventArgs e)
        {
            PlayPreviousTrack();
        }

        public void PlayPreviousTrack ()
        {
            tracksCursor--;
            int activeTrackIndex = tracksCursor - 1;
            int newTrackIndex = activeTrackIndex + 1;
            int countTracks = tracks.Count;
            bool isTrackAvailable = newTrackIndex > -1 && newTrackIndex < countTracks;
            if (isTrackAvailable)
            {
                int currentTrackNumber = newTrackIndex - 1;
                string rawCurrentTrackNumber = currentTrackNumber.ToString();
                string tracksStatsLabelContent = rawCurrentTrackNumber + " из " + rawCountTracks;
                tracksStatsLabel.Text = tracksStatsLabelContent;
                string newTrack = tracks[newTrackIndex];
                firstTrackUri = new Uri(newTrack);
                audio.Source = firstTrackUri;
            }
            else
            {
                countTracks = tracks.Count;
                int lastTrackIndex = countTracks - 1;
                string newTrack = tracks[lastTrackIndex];
                firstTrackUri = new Uri(newTrack);
                isPlaying = false;
                tracksCursor = 0;
                ResetTrackStatsLabel();
            }
        }

        private void PlayTrackHandler (object sender, MouseButtonEventArgs e)
        {
            PlayTrackHandler();
        }

        public void PlayTrackHandler ()
        {
            if (isPlaying)
            {
                audio.LoadedBehavior = MediaState.Pause;
            }
            else
            {
                audio.LoadedBehavior = MediaState.Play;
                audio.Source = firstTrackUri;
                string tracksStatsLabelContent = "1 из " + rawCountTracks;
                tracksStatsLabel.Text = tracksStatsLabelContent;
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
            tracksCursor++;
            int activeTrackIndex = tracksCursor - 1;
            int newTrackIndex = activeTrackIndex + 1;
            int countTracks = tracks.Count;
            bool isTrackAvailable = newTrackIndex > 0 && newTrackIndex < countTracks;
            if (isTrackAvailable)
            {
                int currentTrackNumber = newTrackIndex + 1;
                string rawCurrentTrackNumber = currentTrackNumber.ToString();
                string tracksStatsLabelContent = rawCurrentTrackNumber + " из " + rawCountTracks;
                tracksStatsLabel.Text = tracksStatsLabelContent;
                string newTrack = tracks[newTrackIndex];
                firstTrackUri = new Uri(newTrack);
                audio.Source = firstTrackUri;
            }
            else
            {
                string newTrack = tracks[0];
                firstTrackUri = new Uri(newTrack);
                isPlaying = false;
                tracksCursor = 0;
                ResetTrackStatsLabel();
            }
        }

        private void  TrackEndedHandler (object sender, RoutedEventArgs e)
        {
            TrackEnded();
        }

        public void ResetTrackStatsLabel ()
        {
            string tracksStatsLabelContent = "0 из " + rawCountTracks;
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

    }
}
