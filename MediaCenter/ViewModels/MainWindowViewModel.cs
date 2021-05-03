using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using MediaCenter.Managers;
using MediaCenter.Models;
using ReactiveUI;

namespace MediaCenter.ViewModels
{
    public class MainWindowViewModel : ReactiveObject
    {
        private Bitmap currentImage;
        private Window parentWindow;
        private Random rnd = new Random();
        
        private ConfigurationSettings configSettings = new ConfigurationSettings();

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
