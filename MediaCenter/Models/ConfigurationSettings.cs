using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace MediaCenter.Models
{
    public class ConfigurationSettings : INotifyPropertyChanged
    {
        private bool _isRunning;
        private bool _isFullScreen;
        private double _commandBarOpacity;
        private int _refreshInterval;
        private string _imagesRootPath;

        public string ImagesRootPath
        {
            get => _imagesRootPath;
            set
            {
                if (value == _imagesRootPath) return;
                _imagesRootPath = value;
                OnPropertyChanged();
            }
        }

        public int RefreshInterval
        {
            get => _refreshInterval;
            set
            {
                if (value == _refreshInterval) return;
                _refreshInterval = value;
                OnPropertyChanged();
            }
        }

        public bool IsRunning
        {
            get => _isRunning;
            set
            {
                if (value == _isRunning) return;
                _isRunning = value;
                OnPropertyChanged();
            }
        }

        public bool IsFullScreen
        {
            get => _isFullScreen;
            set
            {
                if (value == _isFullScreen) return;
                _isFullScreen = value;
                OnPropertyChanged();
            }
        }

        public double CommandBarOpacity
        {
            get => _commandBarOpacity;
            set
            {
                if (value.Equals(_commandBarOpacity)) return;
                _commandBarOpacity = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
