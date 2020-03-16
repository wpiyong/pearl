using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace gPearlAnalyzer.ViewModel
{
    public delegate void ChangeCameraSettingsHandler(byte settings);

    public enum CameraSettings
    {
        Hue = 1,
        Saturation = 2,
        Gain = 4,
        Shutter = 8
    }

    class StageSettingsViewModel : ViewModelBase
    {
        public event ChangeCameraSettingsHandler ChangeCameraSettings;

        public StageSettingsViewModel()
        {
            base.DisplayName = "StageSettingsViewModel";

            Gap = App.motorSettings.GapInPixel;
            XPixelsInMM = App.motorSettings.XPixelsInMM;
            YPixelsInMM = App.motorSettings.YPixelsInMM;

            MaxLumiValue = App.Settings.MaxLumiValue;
            DarkAreaThreshold = App.Settings.DarkAreaThreshold;
            BrightAreaThreshold = App.Settings.BrightAreaThreshold;
            MultiThreading = App.Settings.MultiThreading;

            Hue = App.Settings.Hue;
            Saturation = App.Settings.Saturation;
            Gain = App.Settings.Gain;
            Shutter = App.Settings.ShutterTime;

            CommandUpdateSettings = new RelayCommand(param => UpdateSettings(param));
            CommandCancelSettings = new RelayCommand(param => CancelSettings(param));
        }

        public RelayCommand CommandUpdateSettings { get; set; }
        public RelayCommand CommandCancelSettings { get; set; }

        double _gain;
        public double Gain
        {
            get
            {
                return _gain;
            }
            set
            {
                if (_gain == value)
                {
                    return;
                }
                _gain = value;
                OnPropertyChanged("Gain");
            }
        }

        double _shutter;
        public double Shutter
        {
            get
            {
                return _shutter;
            }
            set
            {
                if (_shutter == value)
                {
                    return;
                }
                _shutter = value;
                OnPropertyChanged("Shutter");
            }
        }

        double _hue;
        public double Hue
        {
            get
            {
                return _hue;
            }
            set
            {
                if(_hue == value)
                {
                    return;
                }
                _hue = value;
                OnPropertyChanged("Hue");
            }
        }

        double _saturation;
        public double Saturation
        {
            get
            {
                return _saturation;
            }
            set
            {
                if (_saturation == value)
                {
                    return;
                }
                _saturation = value;
                OnPropertyChanged("Saturation");
            }
        }

        int _maxLumiValue;
        public int MaxLumiValue
        {
            get
            {
                return _maxLumiValue;
            }
            set
            {
                if(_maxLumiValue == value)
                {
                    return;
                }
                _maxLumiValue = value;
                OnPropertyChanged("MaxLumiValue");
            }
        }

        int _darkAreaThreshold;
        public int DarkAreaThreshold
        {
            get
            {
                return _darkAreaThreshold;
            }
            set
            {
                if(_darkAreaThreshold == value)
                {
                    return;
                }
                _darkAreaThreshold = value;
                OnPropertyChanged("DarkAreaThreshold");
            }
        }

        int _brightAreaThreshold;
        public int BrightAreaThreshold
        {
            get
            {
                return _brightAreaThreshold;
            }
            set
            {
                if (_brightAreaThreshold == value)
                {
                    return;
                }
                _brightAreaThreshold = value;
                OnPropertyChanged("BrightAreaThreshold");
            }
        }

        bool _multiThreading;
        public bool MultiThreading
        {
            get
            {
                return _multiThreading;
            }
            set
            {
                _multiThreading = value;
                OnPropertyChanged("MultiThreading");
            }
        }

        double _gap;
        public double Gap
        {
            get
            {
                return _gap;
            }
            set
            {
                if (_gap == value)
                {
                    return;
                }
                else
                {
                    _gap = value;
                    OnPropertyChanged("Gap");
                }
            }
        }

        double _xPixelsInMM;
        public double XPixelsInMM
        {
            get
            {
                return _xPixelsInMM;
            }
            set
            {
                if (_xPixelsInMM == value)
                {
                    return;
                }
                else
                {
                    _xPixelsInMM = value;
                    OnPropertyChanged("XPixelsInMM");
                }
            }
        }

        double _yPixelsInMM;
        public double YPixelsInMM
        {
            get
            {
                return _yPixelsInMM;
            }
            set
            {
                if (_yPixelsInMM == value)
                {
                    return;
                }
                else
                {
                    _yPixelsInMM = value;
                    OnPropertyChanged("YPixelsInMM");
                }
            }
        }

        void RaiseChangeCameraSettingsEvent(byte settings)
        {
            ChangeCameraSettings?.Invoke(settings);
        }

        public void AddChangeCameraSettingsSubscriber(ChangeCameraSettingsHandler handler)
        {
            ChangeCameraSettings += handler;
        }

        void CancelSettings(object param)
        {
            ((Window)param).Close();
        }

        void UpdateSettings(object param)
        {
            bool motorSettingsChanged = false;
            bool settingsChanged = false;
            bool cameraSettingsChanged = false;

            byte settings = 0;
            if (App.motorSettings.GapInPixel != Gap)
            {
                if(Gap <=50)
                {
                    MessageBox.Show("Gap should be greater than 50 pixels, Error in Gap");
                    return;
                }
                motorSettingsChanged = true;
                App.motorSettings.GapInPixel = Gap;
            }

            if (App.motorSettings.XPixelsInMM != XPixelsInMM)
            {
                motorSettingsChanged = true;
                App.motorSettings.XPixelsInMM = XPixelsInMM;
            }

            if (App.motorSettings.YPixelsInMM != YPixelsInMM)
            {
                motorSettingsChanged = true;
                App.motorSettings.YPixelsInMM = YPixelsInMM;
            }

            if(App.Settings.MaxLumiValue != MaxLumiValue)
            {
                settingsChanged = true;
                App.Settings.MaxLumiValue = MaxLumiValue;
            }

            if(App.Settings.DarkAreaThreshold != DarkAreaThreshold)
            {
                settingsChanged = true;
                App.Settings.DarkAreaThreshold = DarkAreaThreshold;
            }

            if(App.Settings.BrightAreaThreshold != BrightAreaThreshold)
            {
                settingsChanged = true;
                App.Settings.BrightAreaThreshold = BrightAreaThreshold;
            }

            if(App.Settings.MultiThreading != MultiThreading)
            {
                settingsChanged = true;
                App.Settings.MultiThreading = MultiThreading;
            }

            if (App.Settings.Gain != Gain)
            {
                settings = (byte)(settings | (byte)CameraSettings.Gain);
                cameraSettingsChanged = true;
                App.Settings.Gain = Gain;
            }

            if (App.Settings.ShutterTime != Shutter)
            {
                settings = (byte)(settings | (byte)CameraSettings.Shutter);
                cameraSettingsChanged = true;
                App.Settings.ShutterTime = Shutter;
            }

            if (App.Settings.Hue != Hue)
            {
                settings = (byte)(settings | (byte)CameraSettings.Hue);
                cameraSettingsChanged = true;
                App.Settings.Hue = Hue;
            }

            if(App.Settings.Saturation != Saturation)
            {
                settings = (byte)(settings | (byte)CameraSettings.Saturation);
                cameraSettingsChanged = true;
                App.Settings.Saturation = Saturation;
            }

            if (motorSettingsChanged)
                App.motorSettings.Save();

            if (settingsChanged)
            {
                App.Settings.Save();
            }

            if (cameraSettingsChanged)
            {
                App.Settings.Save();
                RaiseChangeCameraSettingsEvent(settings);
            }
            ((Window)param).Close();
        }

        #region dispose
        private bool _disposed = false;

        protected override void OnDispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // dispose 
                }
                _disposed = true;
            }
        }
        #endregion
    }
}
