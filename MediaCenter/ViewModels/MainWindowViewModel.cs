using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Timers;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using ReactiveUI;

namespace MediaCenter.ViewModels
{
    public class MainWindowViewModel : ReactiveObject
    {
        private Bitmap currentImage;
        private int intervalMilliSec;
        private bool isRunning;
        private bool isFullScreen;
        private Window parentWindow;
        private double commandBarOpacity;
        private Random rnd = new Random();

        public Bitmap CurrentImage
        {
            get => currentImage;
            set => this.RaiseAndSetIfChanged(ref currentImage, value);
        }

        public double CommandBarOpacity
        {
            get => commandBarOpacity;
            set => this.RaiseAndSetIfChanged(ref commandBarOpacity, value);
        }

        public bool IsRunning
        {
            get => isRunning;
            set => this.RaiseAndSetIfChanged(ref isRunning, value);
        }

        public bool IsFullScreen
        {
            get => isFullScreen;
            set => this.RaiseAndSetIfChanged(ref isFullScreen, value);
        }

        public string ImagesRootPath
        {
            get;
            set;
        }

        public List<string> AllDirectories { get; set; }

        public Timer Timer { get; set; }

        public int IntervalMilliSec
        {
            get => intervalMilliSec;
            set
            {
                intervalMilliSec = value;
                Timer.Interval = intervalMilliSec;
                this.RaiseAndSetIfChanged(ref intervalMilliSec, value);
            }
        }

        public MainWindowViewModel(Window parentWindow)
        {
            this.parentWindow = parentWindow;
            commandBarOpacity = 0.8;
            intervalMilliSec = 2000;
            AllDirectories = new List<string>();
            Timer = new Timer(IntervalMilliSec);
            Timer.AutoReset = true;
            Timer.Enabled = true;
            Timer.Elapsed += Timer_Elapsed;
            Timer.Stop();
        }

        public async void StartSlideShow()
        {
            if (!isRunning)
            {
                var dialog = new OpenFolderDialog();
                dialog.Directory = ImagesRootPath;
                var result = await dialog.ShowAsync(parentWindow);
                if (!String.IsNullOrEmpty(result))
                {
                    ImagesRootPath = result;
                }

                AllDirectories = new List<string>();
                ReadAllDirectories(ImagesRootPath);

                GetRandomImage();
                Timer.Start();
                IsRunning = true;
            }
            else
            {
                Timer.Stop();
                IsRunning = false;
            }
        }

        public void ToggleCommandBar()
        {
            if (CommandBarOpacity > 0)
            {
                CommandBarOpacity = 0;
            }
            else
            {
                CommandBarOpacity = 0.8;
            }
        }

        public void ToggleFullScreen()
        {
            if (IsFullScreen)
            {
                parentWindow.WindowState = WindowState.Normal;
                IsFullScreen = false;
            }
            else
            {
                parentWindow.WindowState = WindowState.Maximized;
                parentWindow.WindowState = WindowState.FullScreen;
                IsFullScreen = true;
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
