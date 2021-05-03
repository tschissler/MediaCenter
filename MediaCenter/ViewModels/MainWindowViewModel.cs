using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using DynamicData.Tests;
using ManagedBass;
using MediaCenter.Managers;
using MediaCenter.Models;
using ReactiveUI;
using Timer = System.Timers.Timer;

namespace MediaCenter.ViewModels
{
    public class MainWindowViewModel : ReactiveObject
    {
        private Bitmap currentImage;
        private Window parentWindow;
        private Random rnd = new Random();
        private int musicStream;

        private ConfigurationSettings configSettings = new ConfigurationSettings();
        private bool isMusicPlaying;

        public ConfigurationSettings ConfigSettings
        {
            get => configSettings;
            set => configSettings = value;
        }

        public Bitmap CurrentImage
        {
            get => currentImage;
            set => this.RaiseAndSetIfChanged(ref currentImage, value);
        }

        public bool IsMusicPlaying
        {
            get => isMusicPlaying;
            set => this.RaiseAndSetIfChanged(ref isMusicPlaying, value);
        }

        public List<string> AllDirectories { get; set; }

        public Timer Timer { get; set; }

        public int IntervalMilliSec
        {
            get => configSettings.RefreshInterval;
            set
            {
                configSettings.RefreshInterval = value;
                Timer.Interval = configSettings.RefreshInterval;
                SaveSettings();
            }
        }

        public MainWindowViewModel(Window parentWindow)
        {
            this.parentWindow = parentWindow;
            configSettings = ConfigurationSettingsManager.ReadLastSettings();
            if (configSettings == null)
            {
                configSettings = new ConfigurationSettings();
                configSettings.RefreshInterval = 5000;
                configSettings.CommandBarOpacity = 0.8;
            }

            AllDirectories = new List<string>();
            Timer = new Timer(IntervalMilliSec);
            Timer.AutoReset = true;
            Timer.Enabled = true;
            Timer.Elapsed += Timer_Elapsed;
            Timer.Stop();

            UpdateFullScreen();
            UpdateIsRunning();

        }

        public async Task SaveSettings()
        {
            ConfigurationSettingsManager.WriteCurrentConfiguration(configSettings);
        }

        public async void ToggleIsRunning()
        {
            if (!configSettings.IsRunning)
            {
                var dialog = new OpenFolderDialog();
                dialog.Directory = configSettings.ImagesRootPath;
                var result = await dialog.ShowAsync(parentWindow);
                if (!String.IsNullOrEmpty(result))
                {
                    configSettings.ImagesRootPath = result;
                    SaveSettings();
                }
                configSettings.IsRunning = true;
            }
            else
            {
                configSettings.IsRunning = false;
            }

            UpdateIsRunning();
            SaveSettings();
        }

        public async void UpdateIsRunning()
        {
            if (configSettings.IsRunning)
            {
                AllDirectories = new List<string>();
                ReadAllDirectories(configSettings.ImagesRootPath);

                GetRandomImage();
                Timer.Start();
            }
            else
            {
                Timer.Stop();
            }
        }

        public void ToggleCommandBar()
        {
            if (configSettings.CommandBarOpacity > 0)
            {
                configSettings.CommandBarOpacity = 0;
            }
            else
            {
                configSettings.CommandBarOpacity = 0.8;
            }
            SaveSettings();
        }

        public void ToggleFullScreen()
        {
            configSettings.IsFullScreen = !configSettings.IsFullScreen;
            UpdateFullScreen();
            SaveSettings();
        }

        public void ToggleMusic()
        {
            IsMusicPlaying = !IsMusicPlaying;
            UpdateMusicPlaying();
        }

        private void UpdateMusicPlaying()
        {
            if (IsMusicPlaying)
            {
                if (Bass.Init())
                {
                    musicStream = Bass.CreateStream("test.mp3");
                    //musicStream = Bass.CreateStream("https://swr-edge-2034-dus-lg-cdn.cast.addradio.de/swr/swr1/bw/aac/96/stream.aac", 
                    //    0,
                    //    BassFlags.StreamDownloadBlocks | BassFlags.StreamStatus | BassFlags.AutoFree, (buffer, length, user) => { }
                    //);

                    if (musicStream != 0)
                        Bass.ChannelPlay(musicStream); // Play the stream

                    else throw new Exception($"Error playing music: {Bass.LastError}!");
                }
                else
                {
                    throw new Exception("Music Player BASS could not be initialized!");
                }
            }
            else
            {
                Bass.StreamFree(musicStream);
                Bass.Free();
            }
        }
        private void UpdateFullScreen()
        {
            if (!configSettings.IsFullScreen)
            {
                parentWindow.WindowState = WindowState.Normal;
            }
            else
            {
                parentWindow.WindowState = WindowState.Maximized;
                parentWindow.WindowState = WindowState.FullScreen;
            }
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            GetRandomImage();
        }

        private void ReadAllDirectories(string path)
        {
            try
            {
                if (new DirectoryInfo(path).GetFiles("*.jpg").Length > 0)
                {
                    AllDirectories.Add(path);
                }

                var directories = new DirectoryInfo(path).GetDirectories();
                foreach (var directory in directories)
                {
                    ReadAllDirectories(directory.FullName);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error while reading pictures from {path}");
            }
        }

        private void GetRandomImage()
        {
            Timer.Stop();
            var directoryIndex = rnd.Next(AllDirectories.Count-1);
            var files = new DirectoryInfo(AllDirectories[directoryIndex]).GetFiles("*.jpg");
            while (files.Length <= 1)
            {
                directoryIndex = rnd.Next(AllDirectories.Count - 1);
                files = new DirectoryInfo(AllDirectories[directoryIndex]).GetFiles("*.jpg");
            }
            var filesIndex = new Random(DateTime.Now.Millisecond).Next(files.Length-1);
            CurrentImage = new Bitmap(files[filesIndex].FullName);
            Timer.Start();
        }
    }
}
