using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Shell;
using Microsoft.WindowsAPICodePack.Shell.PropertySystem;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;

namespace ChatApplication
{
    public class SongsViewModel : BaseViewModel
    {
        private TimeSpan TotalTime { get; set; }

        public int Width { get; set; } = 770;

        public int Height { get; set; } = 450;

        public int ColumnWidth { get { return Width / 3; } }

        public ObservableCollection<Song> Songs { get; set; }

        public Song CurrentSong { get; set; } = new Song("", "", "", "D:\\sample.mp3");

        public string SongPath { get { return CurrentSong.FullPath; } }

        public string SongDurationString { get; set; }

        public string CurrentSongTimeString { get; set; }

        public double CurrentSongTime { get; set; }

        public double Volume { get; set; } = 0.5;

        public double SongDuration { get; set; } = 0;

        public MediaElement MyMediaElement { get; set; }

        public ICommand PlayCommand { get; set; }

        public ICommand PauseCommand { get; set; }

        public ICommand StopCommand { get; set; }

        public ICommand ChooseFilesCommand { get; set; }

        public SongsViewModel(MediaElement mediaElement, Slider slider)
        {
            Songs = new ObservableCollection<Song>();
            MyMediaElement = mediaElement;
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Multiselect = true,
                Filter = "Audio files (*.mp3)|*.mp3|All files (*.*)|*.*",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic)
            };
            if (openFileDialog.ShowDialog() == true)
            {
                foreach (string filename in openFileDialog.FileNames)
                {
                    EncodeSongFile(filename);
                }
                CurrentSong = Songs.ElementAt(0) ?? new Song("", "", "", "D:\\sample.mp3");
            }
            PlayCommand = new RelayCommand(() => MyMediaElement.Play());
            PauseCommand = new RelayCommand(() => MyMediaElement.Pause());
            StopCommand = new RelayCommand(() => MyMediaElement.Stop());
            ChooseFilesCommand = new RelayCommand(() => ChooseAudioFiles());

            MyMediaElement.MediaOpened += SetSongDuration;
            MyMediaElement.MediaEnded += Stop;

            slider.ValueChanged += SeekTo;
        }
        private void SetSongDuration(object sender, EventArgs e)
        {
            SongDuration = MyMediaElement.NaturalDuration.TimeSpan.TotalSeconds;
            Console.WriteLine(SongDuration.ToString());
            TotalTime = MyMediaElement.NaturalDuration.TimeSpan;
            SongDurationString = string.Format("{0:D2}:{1:D2}",
                        TotalTime.Minutes,
                        TotalTime.Seconds);
            // Create a timer that will update the counters and the time slider
            var timerVideoTime = new DispatcherTimer();
            timerVideoTime.Interval = TimeSpan.FromSeconds(1);
            timerVideoTime.Tick += new EventHandler(timerTick);
            timerVideoTime.Start();
        }

        private void Stop(object sender, EventArgs e)
        {
            MyMediaElement.Stop();
        }

        void timerTick(object sender, EventArgs e)
        {
            // Check if the movie finished calculate it's total time
            if (MyMediaElement.NaturalDuration.HasTimeSpan)
            {
                // Updating time slider
                CurrentSongTime = MyMediaElement.Position.TotalSeconds;
                var position = MyMediaElement.Position;
                CurrentSongTimeString = string.Format("{0:D2}:{1:D2}",
                            position.Minutes,
                            position.Seconds);
                
            }
        }

        private void SeekTo(object sender, EventArgs e)
        {
            MyMediaElement.Position = TimeSpan.FromSeconds(CurrentSongTime);
        }

        private void ChooseAudioFiles()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Multiselect = true,
                Filter = "Audio files (*.mp3)|*.mp3|All files (*.*)|*.*",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };
            if (openFileDialog.ShowDialog() == true)
            {
                foreach (string filename in openFileDialog.FileNames)
                {
                    EncodeSongFile(filename);
                }
            }
        }

        private void EncodeSongFile(string filename)
        {
            byte[] b = new byte[128];
            string sTitle = filename;
            string sSinger = "Undefined";
            string sAlbum = "Undefined";

            foreach(var mySong in Songs)
            {
                if (mySong.FullPath.Equals(filename, StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show("This song is already in your playlist", "Miracle (Pre-Alpha)");
                    return;
                }
            }

            FileStream fs = new FileStream(filename, FileMode.Open);
            fs.Seek(-128, SeekOrigin.End);
            fs.Read(b, 0, 128);
            bool isSet = false;
            string sFlag = Encoding.Default.GetString(b, 0, 3);
            if (sFlag.CompareTo("TAG") == 0)
            {
                isSet = true;
            }

            if (isSet)
            {
                //get   title   of   song; 
                sTitle = Encoding.Default.GetString(b, 3, 30);
                //get   singer; 
                sSinger = Encoding.Default.GetString(b, 33, 30);
                //get   album; 
                sAlbum = Encoding.Default.GetString(b, 63, 30);
            }

            Song song = new Song(sTitle, sSinger, sAlbum, filename);
            Songs.Add(song);
        }
    }
}
