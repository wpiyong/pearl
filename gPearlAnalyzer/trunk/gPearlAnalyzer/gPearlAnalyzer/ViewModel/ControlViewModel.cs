//using ClassOpenCV;
using ImageProcessorLib;
using gPearlAnalyzer.Model;
using gPearlAnalyzer.View;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using GiaCamera;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Collections.ObjectModel;

namespace gPearlAnalyzer.ViewModel
{
    public class NoSizeDecorator : System.Windows.Controls.Decorator
    {
        protected override Size MeasureOverride(Size constraint)
        {
            // Ask for no space
            Child.Measure(new Size(0, 0));
            return new Size(0, 0);
        }
    }

    public static class SizeObserver
    {
        public static readonly DependencyProperty ObserveProperty = DependencyProperty.RegisterAttached(
            "Observe",
            typeof(bool),
            typeof(SizeObserver),
            new FrameworkPropertyMetadata(OnObserveChanged));

        public static readonly DependencyProperty ObservedWidthProperty = DependencyProperty.RegisterAttached(
            "ObservedWidth",
            typeof(double),
            typeof(SizeObserver));

        public static readonly DependencyProperty ObservedHeightProperty = DependencyProperty.RegisterAttached(
            "ObservedHeight",
            typeof(double),
            typeof(SizeObserver));

        public static bool GetObserve(FrameworkElement frameworkElement)
        {
            return (bool)frameworkElement.GetValue(ObserveProperty);
        }

        public static void SetObserve(FrameworkElement frameworkElement, bool observe)
        {
            frameworkElement.SetValue(ObserveProperty, observe);
        }

        public static double GetObservedWidth(FrameworkElement frameworkElement)
        {
            return (double)frameworkElement.GetValue(ObservedWidthProperty);
        }

        public static void SetObservedWidth(FrameworkElement frameworkElement, double observedWidth)
        {
            frameworkElement.SetValue(ObservedWidthProperty, observedWidth);
        }

        public static double GetObservedHeight(FrameworkElement frameworkElement)
        {
            return (double)frameworkElement.GetValue(ObservedHeightProperty);
        }

        public static void SetObservedHeight(FrameworkElement frameworkElement, double observedHeight)
        {
            frameworkElement.SetValue(ObservedHeightProperty, observedHeight);
        }

        private static void OnObserveChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var frameworkElement = (FrameworkElement)dependencyObject;

            if ((bool)e.NewValue)
            {
                frameworkElement.SizeChanged += OnFrameworkElementSizeChanged;
                UpdateObservedSizesForFrameworkElement(frameworkElement);
            }
            else
            {
                frameworkElement.SizeChanged -= OnFrameworkElementSizeChanged;
            }
        }

        private static void OnFrameworkElementSizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateObservedSizesForFrameworkElement((FrameworkElement)sender);
        }

        private static void UpdateObservedSizesForFrameworkElement(FrameworkElement frameworkElement)
        {
            // WPF 4.0 onwards
            frameworkElement.SetCurrentValue(ObservedWidthProperty, frameworkElement.ActualWidth);
            frameworkElement.SetCurrentValue(ObservedHeightProperty, frameworkElement.ActualHeight);

            // WPF 3.5 and prior
            ////SetObservedWidth(frameworkElement, frameworkElement.ActualWidth);
            ////SetObservedHeight(frameworkElement, frameworkElement.ActualHeight);
        }
    }

    [ValueConversion(typeof(bool), typeof(bool))]
    public class InverseBooleanConverter : IValueConverter
    {
        //xaml
        //IsEnabled="{Binding Path=IsReadOnly, Converter={StaticResource InverseBooleanConverter}}"

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(bool))
                throw new InvalidOperationException("The target must be a boolean");

            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }

    public enum CameraModel
    {
        PointGrey = 0,
        Nikon
    }

    class ControlViewModel : ViewModelBase
    {

        #region Properties

        bool _objectDetectOn;
        public bool ObjectDetectOn
        {
            get { return _objectDetectOn; }
            set
            {
                if (value != _objectDetectOn)
                {
                    _objectDetectOn = value;
                    OnPropertyChanged("ObjectDetectOn");
                }
            }
        }

        bool _busy;
        public bool Busy
        {
            get
            {
                return _busy;
            }
            set
            {
                _busy = value;
                OnPropertyChanged("Busy");
            }
        }

        bool _connected;
        public bool Connected
        {
            get { return _connected; }
            set
            {
                _connected = value;
                OnPropertyChanged("Connected");
            }
        }

        bool _calibrated;
        public bool Calibrated
        {
            get { return _calibrated; }
            set
            {
                _calibrated = value;
                OnPropertyChanged("Calibrated");
            }
        }

        bool _calibratedMotor = true;
        public bool CalibratedMotor
        {
            get
            {
                return _calibratedMotor;
            }
            set
            {
                _calibratedMotor = value;
                OnPropertyChanged("CalibratedMotor");
            }
        }

        GridLength _logWindowWidth;
        public GridLength LogWindowWidth
        {
            get
            {
                return _logWindowWidth;
            }
            set
            {
                _logWindowWidth = value;
                OnPropertyChanged("LogWindowWidth");
            }
        }

        public GridLength GridSplitterWidth
        {
            get
            {
#if DEBUG
                double width = 1;
                return new GridLength(width);
#else
                double width = 0;
                return new GridLength(width);
#endif
            }
        }

        WriteableBitmap _cameraImage;
        public WriteableBitmap CameraImage
        {
            get
            {
                return _cameraImage;
            }
            //set
            //{
            //    _cameraImage = value;
            //    OnPropertyChanged("CameraImage");
            //}
        }

        WriteableBitmap _cameraImageSide;
        public WriteableBitmap CameraImageSide
        {
            get
            {
                return _cameraImageSide;
            }
            //set
            //{
            //    _cameraImage = value;
            //    OnPropertyChanged("CameraImageSide");
            //}
        }

        public LogEntryViewModel LogEntryVM { get { return App.LogEntry; } }

        double _imageContainerWidth;
        public double ImageContainerWidth
        {
            get
            {
                return _imageContainerWidth;
            }
            set
            {
                _imageContainerWidth = value;
                OnPropertyChanged("ImageContainerWidth");
                OnPropertyChanged("CrossHairHorizontalOffset");
                OnPropertyChanged("CrossHairBrush");
            }
        }

        double _imageContainerHeight;
        public double ImageContainerHeight
        {
            get
            {
                return _imageContainerHeight;
            }
            set
            {
                _imageContainerHeight = value;
                OnPropertyChanged("ImageContainerHeight");
                OnPropertyChanged("CrossHairVerticalOffset");
                OnPropertyChanged("CrossHairBrush");
            }
        }

        public double ImageWidth { get; set; }
        public double ImageHeight { get; set; }
        public double CropImageWidth { get; set; }
        public double CropImageHeight { get; set; }

        double widthSide;
        public double WidthSide
        {
            get
            {
                return widthSide;
            }
            set
            {
                if(widthSide == value)
                {
                    return;
                }

                widthSide = value;
                OnPropertyChanged("WidthSide");
            }
        }

        double heightSide;
        public double HeightSide
        {
            get
            {
                return heightSide;
            }
            set
            {
                if (heightSide == value)
                {
                    return;
                }

                heightSide = value;
                OnPropertyChanged("HeightSide");
            }
        }

        double widthTop;
        public double WidthTop
        {
            get
            {
                return widthTop;
            }
            set
            {
                if (widthTop == value)
                {
                    return;
                }

                widthTop = value;
                OnPropertyChanged("WidthTop");
            }
        }

        double heightTop;
        public double HeightTop
        {
            get
            {
                return heightTop;
            }
            set
            {
                if (heightTop == value)
                {
                    return;
                }

                heightTop = value;
                OnPropertyChanged("HeightTop");
            }
        }

        public Thickness CrossHairHorizontalOffset
        {
            get
            {
                double x = 0;
                if (x > 100) x = 100;
                if (x < -100) x = -100;
                x = (ImageContainerWidth * x / 100.0);
                return new Thickness(x, 0, 0, 0);
            }
        }

        public Thickness CrossHairVerticalOffset
        {
            get
            {
                double y = 0;
                if (y > 100) y = 100;
                if (y < -100) y = -100;
                y = (ImageContainerHeight * y / 100.0);
                return new Thickness(0, y, 0, 0);
            }
        }

        public double CrossHairHorizontalPixelOffset
        {
            get
            {
                if (cameraControl == null) return 0;

                double x = 0;
                if (x > 100) x = 100;
                if (x < -100) x = -100;
                x = (x + 100) / 200.0;
                double imageWidth = CropImageWidth == 0 ? ImageWidth : CropImageWidth;

                return imageWidth * x;
            }
        }

        public double CrossHairVerticalPixelOffset
        {
            get
            {
                if (cameraControl == null) return 0;

                double y = 0;
                if (y > 100) y = 100;
                if (y < -100) y = -100;
                y = (y + 100) / 200.0;
                double imageHeight = CropImageHeight == 0 ? ImageHeight : CropImageHeight;

                return imageHeight * y;
            }
        }


        public System.Windows.Media.Brush CrossHairBrush
        {
            get
            {
                int index = 0;
                switch (index)
                {
                    case 1:
                        return System.Windows.Media.Brushes.White;
                    case 2:
                        return System.Windows.Media.Brushes.Orange;
                }
                return System.Windows.Media.Brushes.Black;
            }
        }

        bool _lightOn;
        public bool LightOn
        {
            get
            {
                return _lightOn;
            }
            set
            {
                if(_lightOn == value)
                {
                    return;
                }
                _lightOn = value;
                OnPropertyChanged("LightOn");
            }
        }

        bool _calDimension = false;
        public bool CalDimension
        {
            get
            {
                return _calDimension;
            }
            set
            {
                if(_calDimension == value)
                {
                    return;
                }
                _calDimension = value;
                OnPropertyChanged("CalDimension");
            }
        }

        bool _movePlateDown = true;
        public bool MovePlateDown
        {
            get
            {
                return _movePlateDown;
            }
            set
            {
                _movePlateDown = value;
                OnPropertyChanged("MovePlateDown");
            }
        }

        bool _usingManualMask = false;
        public bool UsingManualMask
        {
            get
            {
                return _usingManualMask;
            }
            set
            {
                _usingManualMask = value;
                OnPropertyChanged("UsingManualMask");
                if (_usingManualMask)
                {
                    double x = WidthTop / 2;
                    double y = HeightTop / 2;
                    Action action = () =>
                    {
                        MainWindow mv = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
                        mv.CanvasDrawTop.Children.Remove(mask);

                        mask = DrawCanvas.Circle(x, y, radius * 2, radius * 2, true, mv.CanvasDrawTop, true, 0.25);
                    };
                    App.Current.Dispatcher.Invoke(action);
                } else
                {
                    MainWindow mv = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
                    mv.CanvasDrawTop.Children.Remove(mask);
                }
            }
        }

        public ObservableCollection<string> MaskNames { get; }

        string _maskName;
        public string MaskName
        {
            get
            {
                return _maskName;
            }
            set
            {
                if(_maskName == value)
                {
                    return;
                }
                _maskName = value;
                OnPropertyChanged("MaskName");
            }
        }

        bool _compareMeasurement = false;
        public bool CompareMeasurement
        {
            get
            {
                return _compareMeasurement;
            }
            set
            {
                _compareMeasurement = value;
                OnPropertyChanged("CompareMeasurement");
            }
        }

        #endregion

        public RelayCommand CommandRotateStage { get; set; }
        public RelayCommand CommandCalibrateMotor { get; set; }
        public RelayCommand CommandMoveUpDown { get; set; }
        public RelayCommand CommandLightOn { get; set; }
        public RelayCommand CommandConnect { get; set; }
        public RelayCommand CommandCalibrate { get; set; }
        public RelayCommand CommandCalibrateSingle { get; set; }
        public RelayCommand CommandMeasure { get; set; }
        public RelayCommand CommandMeasureMany { get; set; }
        public RelayCommand CommandMeasureSingle { get; set; }
        public RelayCommand CommandCancel { get; set; }
        public RelayCommand CommandSave { get; set; }
        public RelayCommand CommandGraph { get; set; }
        public RelayCommand CommandStageSettings { get; set; }
        public RelayCommand CommandTesting { get; set; }
        public RelayCommand CommandAdjustCropSize { get; set; }
        public RelayCommand CommandAdjustManualMask { get; set; }

        private readonly object _connectLock = new object();
        private int _connectCount = 0;

        CameraControl cameraControl = new CameraControl();
        AutoResetEvent cancelEvent = new AutoResetEvent(false);
        StringBuilder resultsBuffer;
        //GraphViewModel resultsGraphViewModel = null;
        //Graph resultsGraph = null;
        List<Tuple<System.Drawing.Bitmap, BitmapSource>> imageList;
        List<Tuple<System.Drawing.Bitmap, BitmapSource>> imageSideList;
        List<Tuple<System.Drawing.Bitmap, BitmapSource>> backgroundImageList = null;

        ImageAnalyzer imgAnalyzer = new ImageAnalyzer_Pearl();

        MotorControl motorControl = new MotorControl();
        double HomePosition;
        double StepsPerPixel;
        double HeightOffset = 0;
        double HeightInMM;
        double MaxSizeInMM;
        double MinSizeInMM;
        static AutoResetEvent autoResetEvent = new AutoResetEvent(false);

        string controlNumber = "";

        bool cancelled;
        bool connectedMotor = false;
        bool connectedCamera = false;
        //int gap = 150;
        List<Line> LineMarkers = new List<Line>();
        List<Line> LineMarkersTop = new List<Line>();
        bool isRotating = false;
        bool analyzeBusy = false;
        bool analyzeSizeBusy = false;
        string rootDirectory = "";
        private readonly object dirLock = new object();
        bool multiple = true;

        Ellipse mask = null;
        //bool usingManualMask = false;
        int radius = 200;
        System.Drawing.Bitmap bmpMask = null;

        double shiftValue;
        double maxValue;

        public ControlViewModel()
        {
            base.DisplayName = "ControlViewModel";

            MaskName = "Circle";
            MaskNames = new ObservableCollection<string>() { "Circle"};

            CommandConnect = new RelayCommand(param => this.Connect());
            CommandCalibrate = new RelayCommand(param => this.Calibrate(true));
            CommandCalibrateSingle = new RelayCommand(param => this.Calibrate(false));
            CommandMeasure = new RelayCommand(param => this.Measure(true));
            CommandMeasureSingle = new RelayCommand(param => this.Measure(false));
            //CommandMeasureMany = new RelayCommand(param => this.MeasureMany());
            CommandCancel = new RelayCommand(param => this.Cancel());
            CommandSave = new RelayCommand(param => this.SaveDialog());
            CommandGraph = new RelayCommand(param => this.ShowGraph());
            CommandLightOn = new RelayCommand(param => LightOnOff());
            CommandMoveUpDown = new RelayCommand(param => MoveUpDown(bool.Parse(param.ToString())));
            CommandCalibrateMotor = new RelayCommand(param => CalibrateMotor());
            CommandRotateStage = new RelayCommand(param => RotateStage());
            CommandStageSettings = new RelayCommand(param => StageSettings());
            CommandTesting = new RelayCommand(param => Testing());
            CommandAdjustCropSize = new RelayCommand(param => AdjustCropSize(int.Parse(param.ToString())));
            CommandAdjustManualMask = new RelayCommand(param => AdjustManualMask(int.Parse(param.ToString())));
#if DEBUG
            LogWindowWidth = new GridLength(1, GridUnitType.Star);
#else
            LogWindowWidth = new GridLength(0);
#endif
            // TODO: mask setting from mask object
            //ImageProcessingUtility.LoadMaskSettings();

            imgAnalyzer.SetMultiThreading(App.Settings.MultiThreading);

        }

        int croppedWidth = 1080;
        int croppedHeight = 1080;

        void AdjustCropSize(int f)
        {
            RemoveLineMarkersTop();

            int delta = 20 * f;
            croppedWidth += delta;
            if(croppedWidth > WidthTop)
            {
                croppedWidth = (int)WidthTop;
            }

            if(croppedWidth < 200)
            {
                croppedWidth = 200;
            }

            if(croppedWidth <= HeightTop)
            {
                croppedHeight = croppedWidth;
            } else
            {
                croppedHeight = (int) HeightTop;
            }
            double offX = (WidthTop - croppedWidth) / 2;
            double offY = (HeightTop - croppedHeight) / 2;
            Action action = () =>
            {
                MainWindow mv = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
                Line lineTop = DrawCanvas.NewLine(offX, offY, offX + croppedWidth, offY, false, mv.CanvasDrawTop);
                Line lineBottom = DrawCanvas.NewLine(offX, offY + croppedHeight, offX + croppedWidth, offY + croppedHeight, false, mv.CanvasDrawTop);
                Line lineLeft = DrawCanvas.NewLine(offX, offY, offX, offY + croppedHeight, false, mv.CanvasDrawTop);
                Line lineRight = DrawCanvas.NewLine(offX + croppedWidth, offY, offX + croppedWidth, offY + croppedHeight, false, mv.CanvasDrawTop);
                LineMarkersTop.Add(lineTop);
                LineMarkersTop.Add(lineBottom);
                LineMarkersTop.Add(lineLeft);
                LineMarkersTop.Add(lineRight);
            };
            App.Current.Dispatcher.Invoke(action);
        }

        void Testing()
        {
            if (isRotating)
            {
                RotateStage();
            }

            Busy = true;

            // clearn markers on canvas
            RemoveLineMarkers();

            // Testing: size calculation 
            BackgroundWorker bw = new BackgroundWorker();
            bw.DoWork += new DoWorkEventHandler(Testing_doWork);
            bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(Testing_completed);
            bw.RunWorkerAsync();
        }

        void Testing_doWork(object sender, DoWorkEventArgs e)
        {
            e.Result = false;
            // homing
            motorControl.Homing();
            // turn on the light
            if (!LightOn)
            {
                LightOnOff();
            }

            Thread.Sleep(3000);

            // get side image
            System.Drawing.Bitmap imgStart;
            BitmapSource imgSrc;

            if (!cameraControl.GetImages(cameraControl.cameras[(int)CameraType.Side], out imgStart, out imgSrc))
            {
                e.Result = false;
                Console.WriteLine("Failed to capture side image");
                return;
            }

            // find object edges in side and top
            double topPosition, left, right;
            ImageProcessing.CalVerticalDiamondTopPosition(ref imgStart, LineType.Horizontal, out topPosition, out left, out right, false);
            Console.WriteLine("topPosition: " + topPosition.ToString());
            //Console.WriteLine("leftPosition: " + left.ToString());
            //Console.WriteLine("rightPosition: " + right.ToString());
            if (topPosition < 50)
            {
                Console.WriteLine("No objet found on the stage");
                MessageBox.Show("No object on the stage", "Error");
                e.Result = false;
                return;
            }

            ImageProcessing.CalVerticalDiamondTopPosition(ref imgStart, LineType.Horizontal, out double top, out left, out right, true);
            Console.WriteLine("leftPosition: " + left.ToString());
            Console.WriteLine("rightPosition: " + right.ToString());

            Action action = () =>
            {
                MainWindow mv = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
                Line lineTop = DrawCanvas.NewLine(0, WidthSide - topPosition, HeightSide, WidthSide - topPosition, false, mv.CanvasDraw);
                Line lineLeft = DrawCanvas.NewLine(left, 0, left, WidthSide, false, mv.CanvasDraw);
                Line lineRight = DrawCanvas.NewLine(right, 0, right, WidthSide, false, mv.CanvasDraw);
                LineMarkers.Add(lineTop);
                LineMarkers.Add(lineLeft);
                LineMarkers.Add(lineRight);
            };
            App.Current.Dispatcher.Invoke(action);

            motorControl.Homing();
            Thread.Sleep(200);

            e.Result = true;
        }

        void Testing_completed(object sender, RunWorkerCompletedEventArgs e)
        {
            Busy = false;
            if ((bool)e.Result == true)
            {
                CalibratedMotor = true;
                App.LogEntry.AddEntry("testing completed");
            }
            else
            {
                App.LogEntry.AddEntry("testing failed!");
            }
        }

        void StageSettings()
        {
            Console.WriteLine("Start stage settings");
            StageSettings stageSettings = new StageSettings();
            StageSettingsViewModel stageSettingsVM = new StageSettingsViewModel();
            stageSettings.DataContext = stageSettingsVM;
            stageSettingsVM.AddChangeCameraSettingsSubscriber(new ChangeCameraSettingsHandler(cameraControl.ChangeCameraSettings));
            stageSettings.ShowDialog();
        }

        void MoveUpDown(bool up)
        {
            return;
            motorControl.MoveUpDown(up);
        }

        void LightOnOff()
        {
            return;
            if (LightOn)
            {
                if (motorControl.serialPorts[(int)MotorType.Linear].LightOn(false))
                {
                    LightOn = false;
                }

            } else
            {
                if (motorControl.serialPorts[(int)MotorType.Linear].LightOn(true))
                {
                    LightOn = true;
                }
            }
        }

        public void EditCameraSettings(int index)
        {
            cameraControl.EditCameraSettings(cameraControl.cameras[index]);
        }

        void Connect()
        {
            Busy = true;
            _connectCount = 0;

            BackgroundWorker bw = new BackgroundWorker();
            bw.DoWork += new DoWorkEventHandler(ConnectMotor_doWork);
            bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(ConnectMotor_completed);
            //bw.RunWorkerAsync();

            BackgroundWorker bwCam = new BackgroundWorker();
            bwCam.DoWork += new DoWorkEventHandler(ConnectCam_doWork);
            bwCam.RunWorkerCompleted += new RunWorkerCompletedEventHandler(ConnectCam_completed);
            bwCam.RunWorkerAsync();

            App.LogEntry.AddEntry("Connection in progress...");
        }

        void ConnectMotor_doWork(object sender, DoWorkEventArgs e)
        {
            // TODO: for testing comment out moto function
            if (!motorControl._driveConnected)
            {
                e.Result = motorControl.Connect();
            }
            else
            {
                e.Result = true;
            }
        }

        void ConnectMotor_completed(object sender, RunWorkerCompletedEventArgs e)
        {
            lock (_connectLock)
            {
                _connectCount++;
            }
            if ((bool)e.Result == true)
            {
                App.LogEntry.AddEntry("Camera capture started");
                connectedMotor = true;
                if (!LightOn)
                {
                    LightOnOff();
                }
                motorControl.Homing();
            }
            else
            {
                App.LogEntry.AddEntry("Failed to connect motor");
                connectedMotor = false;
            }

            lock (_connectLock)
            {
                if (_connectCount == 2)
                    Busy = false;

                Connected = connectedCamera & connectedMotor;
            }
        }

        void ConnectCam_doWork(object sender, DoWorkEventArgs e)
        {
            // TODO: connect camera
            e.Result = cameraControl.Connect();
        }

        void ConnectCam_completed(object sender, RunWorkerCompletedEventArgs e)
        {
            lock (_connectLock)
            {
                _connectCount++;
            }
            // TODO: connect camera
            if ((bool)e.Result == true)
            {
                App.LogEntry.AddEntry("Camera capture started");
                connectedCamera = true;
                for (int i = 0; i < cameraControl.cameras.Count; i++) {
                    cameraControl.StartVideo(cameraControl.cameras[i], ImageReceived);
                }
            }
            else
            {
                App.LogEntry.AddEntry("Failed to start camera, error");
                connectedCamera = false;
            }

            lock (_connectLock)
            {
                if(_connectCount == 1)
                    Busy = false;

                Connected = connectedCamera;// & connectedMotor;
            }
        }

        void ImageReceived(BitmapSource bmpSource, System.Drawing.Bitmap bmp, CameraType camType)
        {
            System.Drawing.Imaging.BitmapData data = null;
            try
            {
                if (camType == CameraType.Side)
                {
                    data = bmp.LockBits(new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height),
                            System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                }
                else
                {
                    if (!ObjectDetectOn)
                        data = bmp.LockBits(new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height),
                            System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                    else
                    {
                        // TODO: from imageanalyzer to get mask
                        //System.Drawing.Bitmap bmp_with_mask = ImageProcessingUtility.ObjectMask(bmp, -1, -1);
                        System.Drawing.Bitmap bmp_with_mask;
                        //System.Drawing.Bitmap refImg = backgroundImageList[0].Item1;
                        //if (ImageProcessing.CreateGrabCut(ref bmp, ref refImg, out bmp_with_mask))
                        if (imgAnalyzer.maskCreate(ref bmp, out bmp_with_mask, true))
                        {
                            data = bmp_with_mask.LockBits(new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height),
                                System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                        } else
                        {
                            App.LogEntry.AddEntry("No mask created.");
                            data = bmp.LockBits(new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height),
                                System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                        }
                    }

                    if(LineMarkersTop.Count == 0)
                    {
                        double offX = (bmp.Width - croppedWidth) / 2;
                        double offY = (bmp.Height - croppedHeight) / 2;
                        Action action = () =>
                        {
                            MainWindow mv = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
                            Line lineTop = DrawCanvas.NewLine(offX, offY, offX + croppedWidth, offY, false, mv.CanvasDrawTop);
                            Line lineBottom = DrawCanvas.NewLine(offX, offY + croppedHeight, offX + croppedWidth, offY + croppedHeight, false, mv.CanvasDrawTop);
                            Line lineLeft = DrawCanvas.NewLine(offX, offY, offX, offY + croppedHeight, false, mv.CanvasDrawTop);
                            Line lineRight = DrawCanvas.NewLine(offX + croppedWidth, offY, offX + croppedWidth, offY + croppedHeight, false, mv.CanvasDrawTop);
                            LineMarkersTop.Add(lineTop);
                            LineMarkersTop.Add(lineBottom);
                            LineMarkersTop.Add(lineLeft);
                            LineMarkersTop.Add(lineRight);
                        };
                        App.Current.Dispatcher.Invoke(action);
                    }

                    if(mask == null && UsingManualMask)
                    {
                        double x = bmp.Width / 2;
                        double y = bmp.Height / 2;
                        Action action = () =>
                        {
                            MainWindow mv = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
                            mask = DrawCanvas.Circle(x, y, radius * 2, radius * 2, true, mv.CanvasDrawTop, true, 0.25);
                        };
                        App.Current.Dispatcher.Invoke(action);
                    }
                }

                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    if (camType == CameraType.Top)
                    {
                        if (_cameraImage == null || _cameraImage.Width != bmp.Width || _cameraImage.Height != bmp.Height)
                        {
                            _cameraImage = new System.Windows.Media.Imaging.WriteableBitmap(bmp.Width, bmp.Height,
                                                96,
                                                96, System.Windows.Media.PixelFormats.Bgr24, null);

                            OnPropertyChanged("CameraImage");
                        }
                        _cameraImage.WritePixels(new System.Windows.Int32Rect(0, 0, bmp.Width, bmp.Height),
                                data.Scan0, bmp.Width * 3 * bmp.Height, data.Stride);
                        WidthTop = bmp.Width;
                        HeightTop = bmp.Height;
                    } else
                    {
                        if (_cameraImageSide == null || _cameraImageSide.Width != bmp.Width || _cameraImageSide.Height != bmp.Height)
                        {
                            _cameraImageSide = new System.Windows.Media.Imaging.WriteableBitmap(bmp.Width, bmp.Height,
                                                96,
                                                96, System.Windows.Media.PixelFormats.Bgr24, null);

                            OnPropertyChanged("CameraImageSide");
                        }
                        _cameraImageSide.WritePixels(new System.Windows.Int32Rect(0, 0, bmp.Width, bmp.Height),
                                data.Scan0, bmp.Width * 3 * bmp.Height, data.Stride);
                        WidthSide = bmp.Width;
                        HeightSide = bmp.Height;

                        var window = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
                        if (window != null)
                        {
                            //RotateTransform rotateTransform = new RotateTransform();
                            //rotateTransform.CenterX = window.ImageSide.ActualWidth / 2;
                            //rotateTransform.CenterY = window.ImageSide.ActualHeight / 2;
                            //rotateTransform.Angle = -90;
                            //window.ImageSide.RenderTransform = rotateTransform;
                        }
                    }
                });


                ImageWidth = bmp.Width;
                ImageHeight = bmp.Height;
                CropImageHeight = ImageHeight;
                CropImageWidth = ImageWidth;

            }
            finally
            {
                if (data != null)
                    bmp.UnlockBits(data);
            }

        }

        void AdjustManualMask(int factor, bool fill = false)
        {
            if (UsingManualMask)
            {
                if (mask != null)
                {
                    MainWindow mv = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
                    mv.CanvasDrawTop.Children.Remove(mask);
                }

                radius += factor * 3;

                double x = WidthTop / 2;
                double y = HeightTop / 2;
                Action action = () =>
                {
                    MainWindow mv = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
                    mask = DrawCanvas.Circle(x, y, radius * 2, radius * 2, true, mv.CanvasDrawTop, true, 0.25);
                };
                App.Current.Dispatcher.Invoke(action);
            }
        }

        void RemoveLineMarkers(int index = 0)
        {
            while(index < LineMarkers.Count)
            {
                MainWindow mv = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
                mv.CanvasDraw.Children.Remove(LineMarkers[LineMarkers.Count - 1]);
                LineMarkers.RemoveAt(LineMarkers.Count - 1);
            }
        }

        void RemoveLineMarkersTop(int index = 0)
        {
            while (index < LineMarkersTop.Count)
            {
                MainWindow mv = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
                mv.CanvasDrawTop.Children.Remove(LineMarkersTop[LineMarkersTop.Count - 1]);
                LineMarkersTop.RemoveAt(LineMarkersTop.Count - 1);
            }
        }

        void RotateStage()
        {
            if (!isRotating)
            {
                if (motorControl.RotateStage(true))
                {
                    isRotating = true;
                }
            } else
            {
                if (motorControl.RotateStage(false))
                {
                    isRotating = false;
                }
            }
        }

        void CalibrateMotor()
        {
            if (isRotating)
            {
                RotateStage();
            }

            Busy = true;

            // clearn markers on canvas
            RemoveLineMarkers();
            //RemoveLineMarkersTop();
            CalibratedMotor = false;

            BackgroundWorker bw = new BackgroundWorker();
            bw.DoWork += new DoWorkEventHandler(CalibrateMotor_doWork);
            bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(CalibrateMotor_completed);
            bw.RunWorkerAsync();
        }

        void CalibrateMotor_doWork(object sender, DoWorkEventArgs e)
        {
            e.Result = false;
            // homing
            motorControl.Homing();
            // turn on the light
            if (!LightOn)
            {
                LightOnOff();
            }
            //if (!motorControl.serialPorts[(int)MotorType.Linear].LightOn(true))
            //{
            //    e.Result = false;
            //    Console.WriteLine("Failed to turn on the light");
            //    return;
            //}
            Thread.Sleep(3000);

            // get side image
            System.Drawing.Bitmap imgStart;
            BitmapSource imgSrc;

            if (!cameraControl.GetImages(cameraControl.cameras[(int)CameraType.Side], out imgStart, out imgSrc))
            {
                e.Result = false;
                Console.WriteLine("Failed to capture side image");
                return;
            }

            // check if object on the stage
            double topPosition;
            ImageProcessing.CalVerticalDiamondTopPosition(ref imgStart, LineType.Horizontal, out topPosition, out double left, out double right);
            Console.WriteLine("Height offset: " + topPosition.ToString());
            if(topPosition > 40) // 40 pixels as the threshold for the offset 2 places
            {
                Console.WriteLine("Objet found on the stage");
                MessageBox.Show("Object on the stage", "Error");
                e.Result = false;
                return;
            } else
            {
                // save the crop position
                HeightOffset = topPosition;
                Action act = () =>
                {
                    MainWindow mv = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
                    Line line = DrawCanvas.NewLine(0, WidthSide - HeightOffset, HeightSide, WidthSide - HeightOffset, false, mv.CanvasDraw);
                    LineMarkers.Add(line);
                };
                App.Current.Dispatcher.Invoke(act);
            }

            // move down 2000 steps
            // CW move down, CCW up
            int steps = 2000;
            motorControl.serialPorts[(int)MotorType.Linear].Move(Direction.CW, steps);
            int count = 5;
            while(count > 0)
            {
                if (!motorControl.serialPorts[(int)MotorType.Linear].DriveBusy)
                {
                    count--;
                }
                Thread.Sleep(200);
            }

            // get side image
            System.Drawing.Bitmap imgEnd;

            if (!cameraControl.GetImages(cameraControl.cameras[(int)CameraType.Side], out imgEnd, out imgSrc))
            {
                e.Result = false;
                Console.WriteLine("Failed to capture side image");
                return;
            }

            // calculate the pixel per steps
            double positionStart, positionEnd;
            ImageProcessing.CalVerticalHorizontalLinePosition(ref imgStart, LineType.Horizontal, out positionStart);
            ImageProcessing.CalVerticalHorizontalLinePosition(ref imgEnd, LineType.Horizontal, out positionEnd);
            double stepsPerPixel = Math.Abs(steps / (positionEnd - positionStart));
            HomePosition = positionStart;
            StepsPerPixel = stepsPerPixel;

            Action action = () =>
            {
                MainWindow mv = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
                Line line = DrawCanvas.NewLine(0, WidthSide - HomePosition, HeightSide, WidthSide - HomePosition, false, mv.CanvasDraw);
                LineMarkers.Add(line);
            };
            App.Current.Dispatcher.Invoke(action);

            motorControl.Homing();
            Thread.Sleep(200);

            // todo: check the home position to confirm the initial position
            if (!cameraControl.GetImages(cameraControl.cameras[(int)CameraType.Side], out imgStart, out imgSrc))
            {
                e.Result = false;
                Console.WriteLine("Failed to capture side image");
                return;
            }
            ImageProcessing.CalVerticalHorizontalLinePosition(ref imgStart, LineType.Horizontal, out positionStart);

            if(Math.Abs(HomePosition - positionStart) < 15)
            {
                e.Result = true;
            }
            else
            {
                MessageBox.Show("Wrong stage initial position, please calibrate again.", "Error");
                e.Result = false;
            }
            
        }

        void CalibrateMotor_completed(object sender, RunWorkerCompletedEventArgs e)
        {
            if ((bool)e.Result == true)
            {
                CalibratedMotor = true;
                App.LogEntry.AddEntry("Calibration Motor completed");
            }
            else
            {
                App.LogEntry.AddEntry("Calibration motor failed!");
            }

            Busy = false;
        }

        void Calibrate(bool many)
        {
            if (isRotating)
            {
                RotateStage();
            }

            Busy = true;
            Calibrated = false;

            App.LogEntry.AddEntry("Calibration in progress...");

            backgroundImageList = new List<Tuple<System.Drawing.Bitmap, BitmapSource>>();
            cancelEvent.Reset();
            cancelled = false;

            RemoveLineMarkers(1);
            //RemoveLineMarkersTop();

            BackgroundWorker bw = new BackgroundWorker();
            bw.DoWork += new DoWorkEventHandler(Calibrate_doWork);
            bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(Calibrate_completed);
            bw.RunWorkerAsync(many);

            

        }

        void Calibrate_doWork(object sender, DoWorkEventArgs e)
        {
            bool many = (bool)e.Argument;

            // homing
            motorControl.Homing();

            // turn on the light
            if (!LightOn)
            {
                LightOnOff();
            }

            if (!CalibratedMotor)
            {
                App.LogEntry.AddEntry("Please calibrate motor first");
                e.Result = false;
                return;
            }

            // check if object on the stage
            {
                // get side image
                System.Drawing.Bitmap imgStart;
                BitmapSource imgSrc;

                if (!cameraControl.GetImages(cameraControl.cameras[(int)CameraType.Side], out imgStart, out imgSrc))
                {
                    e.Result = false;
                    Console.WriteLine("Failed to capture side image");
                    return;
                }

                // check if object on the stage
                double topPosition;
                ImageProcessing.CalVerticalDiamondTopPosition(ref imgStart, LineType.Horizontal, out topPosition, out double left, out double right);
                Console.WriteLine("topPosition: " + topPosition.ToString());
                if (topPosition > 40) // height offset threshold
                {
                    Console.WriteLine("Objet found on the stage");
                    MessageBox.Show("Object on the stage", "Error");
                    e.Result = false;
                    return;
                }
            }
            // move the plate close to the stage
            {
                int steps = (int)((HomePosition - HeightOffset - App.motorSettings.GapInPixel)* StepsPerPixel);
                if (steps > 0)
                    motorControl.serialPorts[(int)MotorType.Linear].Move(Direction.CW, steps);
            }

            Thread.Sleep(4000);

            // turn off the light
            if (LightOn)
            {
                LightOnOff();
            }

            e.Result = true;

            // side camera calibration
            {
                if (cameraControl.Calibrate(cameraControl.cameras[(int)CameraType.Side]))
                {

                } else
                {
                    Console.WriteLine("side camera calibration failed");
                }
            }
            if (cameraControl.Calibrate(cameraControl.cameras[(int)CameraType.Top]))
            {
                App.LogEntry.AddEntry("Collecting background images...");
                if (many)
                {
                    motorControl.serialPorts[(int)MotorType.Rotation].StepsPerRev = App.Settings.StepsPerRev;
                    motorControl.serialPorts[(int)MotorType.Rotation].ResetPosition();
                    motorControl.serialPorts[(int)MotorType.Rotation].RotateByAngle(360, Direction.CW);
                }

                System.Drawing.Bitmap img;
                BitmapSource imgSrc;
                do
                {
                    if (!cancelEvent.WaitOne(App.Settings.TimeBetweenImagesMs))
                    {
                        if (cameraControl.GetImages(cameraControl.cameras[(int)CameraType.Top], out img, out imgSrc))
                        {
                            backgroundImageList.Add(new Tuple<System.Drawing.Bitmap, BitmapSource>(img, imgSrc));
                        }
                        //else
                        //    App.LogEntry.AddEntry("Measurement error: " + error);
                    }
                    else
                    {
                        motorControl.serialPorts[(int)MotorType.Rotation].StopMotor();
                        cancelled = true;
                        e.Result = false;
                        break;
                    }

                } while (false); //(motorControl.serialPorts[(int)MotorType.Rotation].DriveBusy); todo

            }
            else
                e.Result = false;

            if (!LightOn)
            {
                LightOnOff();
            }
            // homing // todo
            //motorControl.Homing();
        }

        void Calibrate_completed(object sender, RunWorkerCompletedEventArgs e)
        {
            if ((bool)e.Result == true)
            {
                App.LogEntry.AddEntry("Calibration completed - " + backgroundImageList.Count + " background images captured");
                Calibrated = true;
            }
            else
            {
                App.LogEntry.AddEntry("Calibration failed!");
                Calibrated = false;
            }

            Busy = false;
        }

        void Measure(bool many)
        {
            if (isRotating)
            {
                RotateStage();
            }

            //todo: bypass calibration
            if (!Calibrated)
            {
                MessageBox.Show("Please calibrate", "Cannot measure");
                return;
            }

            var dialogControlNumberWindow = new gPearlAnalyzer.View.ControlNumber();
            var dialogViewModel = new gPearlAnalyzer.ViewModel.ControlNumberViewModel();
            dialogControlNumberWindow.DataContext = dialogViewModel;
            bool? dialogResult = dialogControlNumberWindow.ShowDialog();
            if (dialogResult == true)
                controlNumber = dialogViewModel.ControlNumber;
            else
                return;

            App.LogEntry.AddEntry("Measurement starting...");

            if (UsingManualMask)
            {
                AdjustManualMask(-1, true);
            }

            Busy = true;
            resultsBuffer = new StringBuilder();
            imageList = new List<Tuple<System.Drawing.Bitmap, BitmapSource>>();
            imageSideList = new List<Tuple<System.Drawing.Bitmap, BitmapSource>>();
            cancelEvent.Reset();
            cancelled = false;

            // clearn previous object top marker and plate position marker
            RemoveLineMarkers(1);
            //RemoveLineMarkersTop();
            HeightInMM = 0;
            MaxSizeInMM = 0;
            MinSizeInMM = 0;
            multiple = many;

            BackgroundWorker bw = new BackgroundWorker();
            bw.DoWork += new DoWorkEventHandler(Measure_doWork);
            bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(Measure_completed);
            bw.RunWorkerAsync(many);

            
        }

        void Measure_doWork(object sender, DoWorkEventArgs e)
        {
            e.Result = false;

            // homing
            motorControl.Homing();
            // turn on the light
            if (!LightOn)
            {
                LightOnOff();
            }

            Thread.Sleep(3000);

            // get side image
            System.Drawing.Bitmap imgStart;
            BitmapSource imgTmp;

            if (!cameraControl.GetImages(cameraControl.cameras[(int)CameraType.Side], out imgStart, out imgTmp))
            {
                e.Result = false;
                Console.WriteLine("Failed to capture side image");
                return;
            }

            double topPosition;
            ImageProcessing.CalVerticalDiamondTopPosition(ref imgStart, LineType.Horizontal, out topPosition, out double left, out double right);

            // check if object on the stage
            {
                if (topPosition > 50)
                {
                    double h = (topPosition - HeightOffset) / App.motorSettings.YPixelsInMM;
                    HeightInMM = h;
                    App.LogEntry.AddEntry("Diamond height: " + topPosition.ToString() + " pixels, " + h.ToString() + " mm");
                    int steps = (int)((HomePosition - topPosition - App.motorSettings.GapInPixel) * StepsPerPixel);
                    if (MovePlateDown && steps > 0)
                        motorControl.serialPorts[(int)MotorType.Linear].Move(Direction.CW, steps);
                }
                else
                {
                    //Console.WriteLine("No objet found on the stage, topPosition: " + topPosition.ToString());
                    //MessageBox.Show("No object on the stage", "Error");
                    //e.Result = false;
                    //return;
                }
            }

            Action action = () =>
            {
                double targetPosition = topPosition + App.motorSettings.GapInPixel;
                MainWindow mv = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
                Line edgeLine = DrawCanvas.NewLine(0, WidthSide - topPosition, HeightSide, WidthSide - topPosition, false, mv.CanvasDraw);
                Line targetLine = DrawCanvas.NewLine(0, WidthSide - targetPosition, HeightSide, WidthSide - targetPosition, false, mv.CanvasDraw);

                LineMarkers.Add(edgeLine);
                LineMarkers.Add(targetLine);
            };
            App.Current.Dispatcher.Invoke(action);

            Thread.Sleep(4000);
            // turn off the light
            if (LightOn)
            {
                LightOnOff();
            }

            Thread.Sleep(1000);

            // homing
            System.Drawing.Bitmap img;
            BitmapSource imgSrc;

            if (cameraControl.GetImages(cameraControl.cameras[(int)CameraType.Top], out img, out imgSrc))
            {
                string com = "";
                if (!imgAnalyzer.check_diamond_centered(ref img, (int)(img.Width / 2), (int)(img.Height / 2),
                    ref com, App.Settings.MaxCenterDistanceDiff))
                {
                    App.LogEntry.AddEntry("Error: " + com);
                    //return; // todo will remove the comment
                }
            }
            else
            {
                return;
            }

            bool adjustShutter = !CompareMeasurement;
            int refValue = 200;
            string comment = "";

            maxValue = 0;
            shiftValue = 0;
            while (adjustShutter)
            {
                System.Drawing.Bitmap tmpImg;
                BitmapSource imgSrcTmp;
                if (cameraControl.GetImages(cameraControl.cameras[(int)CameraType.Top], out tmpImg, out imgSrcTmp))
                {
                    imgAnalyzer.check_pearl_max_lumi(ref tmpImg, ref comment, out maxValue, out shiftValue);

                    if (Math.Abs(maxValue - App.Settings.MaxLumiValue) <= 2 )
                    {
                        adjustShutter = false;
                    }
                    else
                    {
                        if(maxValue > App.Settings.MaxLumiValue)
                        {
                            cameraControl.AdjustShutter(cameraControl.cameras[(int)CameraType.Top], false);
                        } else
                        {
                            cameraControl.AdjustShutter(cameraControl.cameras[(int)CameraType.Top], true);
                        }

                        Thread.Sleep(2000);
                    }
                }
                else
                {
                    App.LogEntry.AddEntry("Error in getting image");
                    return;
                }
            }

            bool many = (bool)e.Argument;

            if (many)
            {
                motorControl.serialPorts[(int)MotorType.Rotation].StepsPerRev = App.Settings.StepsPerRev;
                motorControl.serialPorts[(int)MotorType.Rotation].ResetPosition();
                motorControl.serialPorts[(int)MotorType.Rotation].RotateByAngle(360, Direction.CW);
            }


            do
            {
                if (!cancelEvent.WaitOne(App.Settings.TimeBetweenImagesMs))
                {
                    if (cameraControl.GetImages(cameraControl.cameras[(int)CameraType.Top], out img, out imgSrc))
                    {
                        imageList.Add(new Tuple<System.Drawing.Bitmap, BitmapSource>(img, imgSrc));
                    }
                    //else
                    //    App.LogEntry.AddEntry("Measurement error: " + error);

                }
                else
                {
                    motorControl.serialPorts[(int)MotorType.Rotation].StopMotor();
                    cancelled = true;
                    break;
                }

            } while (false); // (motorControl.serialPorts[(int)MotorType.Rotation].DriveBusy) ; todo

            // homing
            motorControl.Homing();
            // turn on the light
            if (!LightOn)
            {
                LightOnOff();
            }

            // collecting side images for size calculation 
            if(CalDimension)
            {
                if (many)
                {
                    motorControl.serialPorts[(int)MotorType.Rotation].StepsPerRev = App.Settings.StepsPerRev;
                    motorControl.serialPorts[(int)MotorType.Rotation].ResetPosition();
                    motorControl.serialPorts[(int)MotorType.Rotation].RotateByAngle(360, Direction.CW);
                }


                do
                {
                    if (!cancelEvent.WaitOne(App.Settings.TimeBetweenImagesMs))
                    {
                        if (cameraControl.GetImages(cameraControl.cameras[(int)CameraType.Side], out img, out imgSrc))
                        {
                            imageSideList.Add(new Tuple<System.Drawing.Bitmap, BitmapSource>(img, imgSrc));
                        }
                        //else
                        //    App.LogEntry.AddEntry("Measurement error: " + error);

                    }
                    else
                    {
                        motorControl.serialPorts[(int)MotorType.Rotation].StopMotor();
                        cancelled = true;
                        break;
                    }

                } while (motorControl.serialPorts[(int)MotorType.Rotation].DriveBusy);
            }

            e.Result = true;
        }


        //void Measure_ProgressChanged(object sender, ProgressChangedEventArgs e)
        //{
        //    Stone stone = (Stone)e.UserState;

        //    var newLine = string.Format("{0},{1},{2},{3},{4},{5},{6}",
        //        stone.L, stone.A, stone.B, stone.C, stone.H, stone.MeasurementTemperature,
        //        stone.Mask_A);

        //    resultsBuffer.Append(DateTime.Now.ToUniversalTime().ToString("MM/dd/yyyy hh:mm:ss tt") + "," + newLine
        //        + Environment.NewLine);

        //    App.LogEntry.AddEntry("Measurement result : " + newLine);

        //    //if (IsWindowOpen<Graph>())
        //    //    resultsGraphViewModel.AppendValues(stone.L, stone.A, stone.B, stone.C, stone.H,
        //    //        stone.MeasurementTemperature);
        //}

        void Measure_completed(object sender, RunWorkerCompletedEventArgs e)
        {
            RemoveLineMarkers(1);

            if ((bool)e.Result == true)
            {
                App.LogEntry.AddEntry("Measurement ended, " + imageList.Count + " images");

                if (!cancelled)
                {
                    App.LogEntry.AddEntry("Analyzing...");

                    rootDirectory = "";

                    analyzeBusy = true;
                    analyzeSizeBusy = true;

                    if (UsingManualMask)
                    {
                        RemoveLineMarkersTop();

                        // todo: generate the mask bitmap
                        MainWindow mv = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
                        //SolidColorBrush solidColorBrush = new SolidColorBrush();
                        //solidColorBrush.Color = Color.FromArgb(0, 255, 0, 0);
                        //mask.Fill = solidColorBrush;
                        //mask.StrokeThickness = 1;
                        WriteableBitmap tmp = SaveAsWriteableBitmap(mv.CanvasDrawTop);
                        bmpMask = BitmapFromWriteableBitmap(tmp);
                        ImageProcessing.AdjustMask(ref bmpMask);

                        AdjustManualMask(1);
                    }

                    BackgroundWorker bw = new BackgroundWorker();
                    bw.DoWork += new DoWorkEventHandler(Analyze_doWork);
                    bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(Analyze_completed);
                    bw.RunWorkerAsync();
                    if (CalDimension)
                    {
                        BackgroundWorker bwSize = new BackgroundWorker();
                        bwSize.DoWork += new DoWorkEventHandler(AnalyzeSize_doWork);
                        bwSize.RunWorkerCompleted += new RunWorkerCompletedEventHandler(AnalyzeSize_completed);
                        bwSize.RunWorkerAsync();
                    }
                }
                else
                {
                    Analyze_completed(null, null);
                }
            }
            else
            {
                App.LogEntry.AddEntry("Measurement error!");
                Busy = false;
                
                // turn on the light
                if (!LightOn)
                {
                    LightOnOff();
                }
                // homing
                motorControl.Homing();
            }

        }


        void Analyze_doWork(object sender, DoWorkEventArgs e)
        {
            lock (dirLock)
            {
                if (rootDirectory.Equals(""))
                {
                    rootDirectory = App.Settings.MeasurementsFolder + @"\"
                            + DateTime.Now.ToString("yyyy-MM-dd") + "_" + DateTime.Now.ToString("HHmmssfff");
                }
            }

            // todo:
            List<System.Drawing.Bitmap> imgList = new List<System.Drawing.Bitmap>();
            for (int i = 0; i < imageList.Count; i++)
            {
                imgList.Add((System.Drawing.Bitmap)(imageList[i].Item1.Clone()));
            }
            try
            {
                string ss = "";
                System.Drawing.Bitmap bm = imgList[0];
                imgAnalyzer.check_pearl_max_lumi(ref bm, ref ss, out maxValue, out shiftValue);
                App.LogEntry.AddEntry("Max Lumi: " + maxValue.ToString());
                imgAnalyzer.CalcPearlLusterWidthHeight(imgList, ref shiftValue, ref maxValue, out double w, out double h);
                App.LogEntry.AddEntry("width and height: " + w.ToString() + " : " + h.ToString());
                e.Result = true;
            }
            catch(Exception ex)
            {
                App.LogEntry.AddEntry("Error: " + ex.Message);
                e.Result = false;
            }
            return;

            Stone s = new Stone();
            string color = "", error = "";
            double L = 0, a = 0, b = 0, C = 0, H = 0, mask_L = 0, mask_A = 0, temp = 0, mask2_A = 0;
            bool result = false;
            var hsvList = new List<Tuple<double, double, double,double,double,double>>();

            bool usingCrop = true;
            int offX = 0;
            int offY = 0;
            offX = (imageList[0].Item1.Width - croppedWidth) / 2;
            offY = (imageList[0].Item1.Height - croppedHeight) / 2;

            // set the multithreading parameter
            imgAnalyzer.SetMultiThreading(App.Settings.MultiThreading);

            if (backgroundImageList.Count >= imageList.Count)
            {
                //result = cameraControl.Measure(imageList.Select(t => t.Item1).ToList(),
                //     backgroundImageList.Take(imageList.Count).ToList(), 
                //     App.Settings.BrightAreaThreshold, App.Settings.DarkAreaThreshold,
                //     out color, out L, out a, out b,
                //         out C, out H, out mask_L, out mask_A, out error, out temp, out mask2_A, out hsvList);
                System.Drawing.Rectangle r = new System.Drawing.Rectangle(offX, offY, croppedWidth, croppedHeight);

                if (usingCrop)
                {
                    List<System.Drawing.Bitmap> lSrc = new List<System.Drawing.Bitmap>();
                    List<System.Drawing.Bitmap> bSrc = new List<System.Drawing.Bitmap>();
                    for (int i = 0; i < imageList.Count; i++)
                    {
                        lSrc.Add((imageList[i].Item1).Clone(r, imageList[i].Item1.PixelFormat));
                    }
                    for (int i = 0; i < imageList.Count; i++)
                    {
                        bSrc.Add((backgroundImageList[i].Item1).Clone(r, backgroundImageList[i].Item1.PixelFormat));
                    }

                    if (UsingManualMask)
                    {
                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            bmpMask.Save(@"C:\gPearlAnalyzer\manualmask.bmp");
                        });

                        System.Drawing.Bitmap maskCrop = bmpMask.Clone(r, bmpMask.PixelFormat);
                        result = Measure(lSrc, bSrc, maskCrop,
                             App.Settings.BrightAreaThreshold, App.Settings.DarkAreaThreshold,
                             out color, out L, out a, out b,
                             out C, out H, out mask_L, out mask_A, out error, out temp, out mask2_A, out hsvList);
                    }
                    else
                    {
                        result = Measure(lSrc, bSrc,
                             App.Settings.BrightAreaThreshold, App.Settings.DarkAreaThreshold,
                             out color, out L, out a, out b,
                             out C, out H, out mask_L, out mask_A, out error, out temp, out mask2_A, out hsvList);
                    }
                }
                else
                {
                    result = Measure(imageList.Select(t => t.Item1).ToList(),
                         backgroundImageList.Select(t => t.Item1).ToList(),
                         App.Settings.BrightAreaThreshold, App.Settings.DarkAreaThreshold,
                         out color, out L, out a, out b,
                             out C, out H, out mask_L, out mask_A, out error, out temp, out mask2_A, out hsvList);
                }
            }
            else
            {
                App.LogEntry.AddEntry("Error : Not enough background images collected...");
                MessageBox.Show("Not enough background images collected", "Error");
            }

            if (result)
            {
                var tple = hsvList[0];
                var L1 = tple.Item1 / imageList.Count;
                var a1 = tple.Item2 / imageList.Count;
                var b1 = tple.Item3 / imageList.Count;
                var C1 = imgAnalyzer.calc_C(ref a1, ref b1);
                var H1 = imgAnalyzer.calc_H(ref a1, ref b1);
                hsvList[0] = new Tuple<double, double, double, double, double, double>(L1, a1, b1, C1, H1, 100);

                s.L = Math.Round(L, 3);
                s.A = Math.Round(a, 3);
                s.B = Math.Round(b, 3);
                s.C = Math.Round(C, 3);
                s.H = Math.Round(H, 3);
                s.Mask_L = Math.Round(mask_L, 3);
                s.Mask_A = Math.Round(mask_A, 3);
                s.MeasurementTemperature = Math.Round(temp, 3);
                s.Comment3 = error;

                var newLine = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},",
                                controlNumber, s.L, a1, b1, C1, H1, s.L, s.A, s.B, s.C, s.H, s.MeasurementTemperature, s.Mask_A, s.Comment3,
                                Math.Round(mask2_A, 3));

                //var newLine = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},",
                //                controlNumber, s.L, s.A, s.B, s.C, s.H, s.MeasurementTemperature, s.Mask_A, s.Comment3,
                //                Math.Round(mask2_A, 3));

                for (int i = 0; i < hsvList.Count; i++)
                {
                    var tuple = hsvList[i];
                    newLine += tuple.Item1 + ", " + tuple.Item2 + ", " + tuple.Item3
                        + ", " + tuple.Item4 + ", " + tuple.Item5 + ", " + tuple.Item6;
                    if (i != hsvList.Count - 1)
                        newLine += ", ";
                }

                //App.LogEntry.AddEntry("Measurement result : " + newLine);

                resultsBuffer.Append(DateTime.Now.ToUniversalTime().ToString("MM/dd/yyyy HH:mm:ss ") + "," + newLine
                    + Environment.NewLine);

                if (imageList.Count > 0)
                {
                    App.LogEntry.AddEntry("Measurement result : " + newLine);
                }
                else
                    App.LogEntry.AddEntry("Measurement count is 0");

                e.Result = true;
            }
            else
            {
                e.Result = false;
            }

        }

        void Analyze_completed(object sender, RunWorkerCompletedEventArgs e)
        {

            App.LogEntry.AddEntry("Analyze ended");
            //RemoveLineMarkersTop();

            if((bool)e.Result == true)
            {
                string[] stringArray = resultsBuffer.ToString().Split(',').ToArray();

                SaveData saveData = new SaveData();
                //saveData.DataContext = new SaveDataViewModel(stringArray[1], "", double.Parse(stringArray[2]), double.Parse(stringArray[5]), double.Parse(stringArray[6]));
                saveData.DataContext = new SaveDataViewModel(stringArray[0], "", 0, 0, 0);
                Nullable<Boolean> res = saveData.ShowDialog();
                if(res == true)
                {
                    // save results
                    //Stone.SaveShort(App.Settings.TextFilePath, "fancyColorTestResults.csv", resultsBuffer);

                    // save images
                    #region save_image
                    int imageCount = 0;
                    if (App.Settings.SaveMeasuments && App.Settings.MeasurementsFolder.Length > 0)
                    {
                        foreach (var imageSet in imageList)
                        {
                            Directory.CreateDirectory(rootDirectory);

                            string filePath = rootDirectory + @"\"
                                    + imageCount++ + App.Settings.MeasurementsFileExtension;

                            using (var fileStream = new FileStream(filePath, FileMode.Create))
                            {
                                BitmapEncoder encoder = new JpegBitmapEncoder();
                                if (App.Settings.MeasurementsFileExtension.ToUpper() == ".BMP")
                                    encoder = new BmpBitmapEncoder();
                                encoder.Frames.Add(BitmapFrame.Create(imageSet.Item2));
                                encoder.Save(fileStream);
                            }


                        }

                        imageCount = 0;
                        for (int i = 0; i < imageList.Count; i++)
                        {
                            //todo: images collected not same
                            var bgImg = backgroundImageList[i].Item1;
                            var bgDir = rootDirectory + @"\background";
                            Directory.CreateDirectory(bgDir);

                            string bgFilePath = bgDir + @"\background"
                                        + imageCount++ + App.Settings.MeasurementsFileExtension;

                            if (App.Settings.MeasurementsFileExtension.ToUpper() == ".BMP")
                                bgImg.Save(bgFilePath, System.Drawing.Imaging.ImageFormat.Bmp);
                            else
                                bgImg.Save(bgFilePath, System.Drawing.Imaging.ImageFormat.Jpeg);
                        }

                    }
                    #endregion
                }
                else
                {

                }
            }

            analyzeBusy = false;
            if (CalDimension)
            {
                if (!analyzeSizeBusy)
                {
                    Busy = false;
                }
            } else
            {
                Busy = false;
            }
        }

        void AnalyzeSize_doWork(object sender, DoWorkEventArgs e)
        {
            lock (dirLock)
            {
                if (rootDirectory.Equals(""))
                {
                    rootDirectory = App.Settings.MeasurementsFolder + @"\"
                            + DateTime.Now.ToString("yyyy-MM-dd") + "_" + DateTime.Now.ToString("HHmmssfff");
                }
            }

            double maxInPixel = 0;
            double minInPixel = 9999;
            for(int i = 0; i < imageSideList.Count; i++)
            {
                double top, left, right, length;
                System.Drawing.Bitmap src = imageSideList[i].Item1;

                ImageProcessing.CalVerticalDiamondTopPosition(ref src, LineType.Horizontal, out top, out left, out right, true);
                length = right - left;
                if(maxInPixel < length)
                {
                    maxInPixel = length;
                }

                if(minInPixel > length)
                {
                    minInPixel = length;
                }
            }
            double mmMax = maxInPixel / App.motorSettings.XPixelsInMM;
            double mmMin = minInPixel / App.motorSettings.XPixelsInMM;
            MaxSizeInMM = mmMax;
            MinSizeInMM = mmMin;
            App.LogEntry.AddEntry("Size in x and y: " + maxInPixel.ToString() + " pixels/" + mmMax.ToString() + " mm," + minInPixel.ToString() + " pixels/" + mmMin.ToString() + " mm");
            Console.WriteLine("size: " + maxInPixel.ToString() + " ," + minInPixel.ToString());
        }

        void AnalyzeSize_completed(object sender, RunWorkerCompletedEventArgs e)
        {

            App.LogEntry.AddEntry("Analyze size ended");
            autoResetEvent.Set();
            analyzeSizeBusy = false;
            if (!analyzeBusy)
            {
                Busy = false;
            }
        }
#if false

        void MeasureMany()
        {
            App.LogEntry.AddEntry("Measurement starting...");
            Busy = true;
            resultsBuffer = new StringBuilder();
            cancelEvent.Reset();
            BackgroundWorker bw = new BackgroundWorker();
            bw.WorkerReportsProgress = true;
            bw.DoWork += new DoWorkEventHandler(Measure_doWork);
            bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(Measure_completed);
            bw.ProgressChanged += new ProgressChangedEventHandler(Measure_ProgressChanged);
            bw.RunWorkerAsync();
        }


        void Measure_doWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = (BackgroundWorker)sender;

            while (true)
            {
                if (!cancelEvent.WaitOne(500))
                {
                    Stone s = new Stone();
                    string color = "", error = "";
                    double L = 0, a = 0, b = 0, C = 0, H = 0, mask_L = 0, mask_A = 0, temp =0;
                    bool result = cameraControl.Measure(out color, out L, out a, out b,
                            out C, out H, out mask_L, out mask_A, out error, out temp);

                    if (result)
                    {
                        s.L = Math.Round(L,3);
                        s.A = Math.Round(a,3);
                        s.B = Math.Round(b,3);
                        s.C = Math.Round(C,3);
                        s.H = Math.Round(H,3);
                        s.Mask_L = Math.Round(mask_L,3);
                        s.Mask_A = Math.Round(mask_A,3);
                        s.MeasurementTemperature = Math.Round(temp, 3);

                        worker.ReportProgress(0, s);
                    }
                    else
                        App.LogEntry.AddEntry("Measurement error: " + error);
                }
                else
                    break;
            }

        }


        void Measure_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            Stone stone = (Stone)e.UserState;

            var newLine = string.Format("{0},{1},{2},{3},{4},{5},{6}",
                stone.L, stone.A, stone.B, stone.C, stone.H, stone.MeasurementTemperature,
                stone.Mask_A);
            
            resultsBuffer.Append(DateTime.Now.ToUniversalTime().ToString("MM/dd/yyyy hh:mm:ss tt") + "," + newLine
                + Environment.NewLine);

            App.LogEntry.AddEntry("Measurement result : " + newLine);

            if (IsWindowOpen<Graph>())
                resultsGraphViewModel.AppendValues(stone.L, stone.A, stone.B, stone.C, stone.H, 
                    stone.MeasurementTemperature);
        }

        void Measure_completed(object sender, RunWorkerCompletedEventArgs e)
        {
            Stone.SaveShort(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), 
                "fancyColorTestResults.csv", resultsBuffer);

            App.LogEntry.AddEntry("Measurement stopped");
            Busy = false;
        }
#endif
        public bool Measure(List<System.Drawing.Bitmap> imgs, List<System.Drawing.Bitmap> bgImgs, out List<List<double>> colors)
        {
            colors = new List<List<double>>();
            List<string> comments = new List<string>();
            List<System.Drawing.Rectangle> mList = new List<System.Drawing.Rectangle>();
            bool res = imgAnalyzer.Get_Description(ref imgs, ref bgImgs, ref mList, out colors, out comments);

            return res;
        }

        public bool Measure(List<System.Drawing.Bitmap> img, List<System.Drawing.Bitmap> bgImg,
            int brightAreaThreshold, int darkAreaThreshold,
            out string color, out double L, out double a, out double b,
            out double C, out double H, out double mask_L, out double mask_A, out string error, out double temp,
            out double mask2_A, out List<Tuple<double,double,double,double,double,double>> hsvList)
        {
            return MeasureEx(img, bgImg, brightAreaThreshold, darkAreaThreshold, 
                out color, out L, out a, out b, out C, out H, out mask_L, 
                out mask_A, out error, out temp, out mask2_A, out hsvList,
                App.Settings.LConv, App.Settings.AConv, App.Settings.BConv,
                App.Settings.Lshift, App.Settings.Ashift, App.Settings.Bshift);
        }

        public bool MeasureEx(List<System.Drawing.Bitmap> img, List<System.Drawing.Bitmap> bgImg,
            int brightAreaThreshold, int darkAreaThreshold, out string color, out double L, out double a, out double b,
            out double C, out double H, out double mask_L, out double mask_A, out string error,out double temp, 
            out double mask2_A, out List<Tuple<double,double,double,double,double,double>> hsvList,
            double Lconv = 1.0, double Aconv = 1.0, double Bconv = 1.0,
            double Lshift = 0, double Ashift = 0, double Bshift = 0)
        {
            bool result = false;
            hsvList = new List<Tuple<double, double, double,double,double,double>>();

            color = "";
            error = "";
            L = 0; a = 0; b = 0; C = 0; H = 0;
            mask_L = 0; mask_A = 0; mask2_A = 0;
            temp = 0;

            try
            {
                if (img.Count != bgImg.Count)
                {
                    error = "background image count not equal to image count";
                    return false;
                }

                imgAnalyzer.setLabAdjustment(Lconv, Aconv, Bconv,
                        Lshift, Ashift, Bshift);

                string L_description = "", C_description = "", H_description = "", comment1 = "",
                    comment2 = "", comment3 = "";

                bool colorResult = imgAnalyzer.Get_Description(ref img, ref bgImg,
                    ref L, ref a, ref b, ref C, ref H, ref L_description, ref C_description,
                    ref H_description, ref mask_L, ref mask_A, ref comment1, ref comment2, ref comment3, 
                    ref mask2_A, ref hsvList, false,
                    App.Settings.MaxPhotoChromicLDiff, brightAreaThreshold, darkAreaThreshold);

                temp = cameraControl.CameraTemperature(cameraControl.cameras[(int)CameraType.Top]);

                if (colorResult)
                {
                    color = C_description;
                    result = true;
                    error = comment3;
                }
                else
                {
                    error = "Bad color result: " + comment3;
                    result = false;
                }
            }
            catch (Exception ex)
            {
                error = ex.ToString() ;
            }

            return result;
        }

        public bool Measure(List<System.Drawing.Bitmap> img, List<System.Drawing.Bitmap> bgImg, System.Drawing.Bitmap maskBmp,
            int brightAreaThreshold, int darkAreaThreshold,
            out string color, out double L, out double a, out double b,
            out double C, out double H, out double mask_L, out double mask_A, out string error, out double temp,
            out double mask2_A, out List<Tuple<double, double, double, double, double, double>> hsvList)
        {
            return MeasureEx(img, bgImg, maskBmp, brightAreaThreshold, darkAreaThreshold,
                out color, out L, out a, out b, out C, out H, out mask_L,
                out mask_A, out error, out temp, out mask2_A, out hsvList,
                App.Settings.LConv, App.Settings.AConv, App.Settings.BConv,
                App.Settings.Lshift, App.Settings.Ashift, App.Settings.Bshift);
        }

        public bool MeasureEx(List<System.Drawing.Bitmap> img, List<System.Drawing.Bitmap> bgImg, System.Drawing.Bitmap maskBmp,
            int brightAreaThreshold, int darkAreaThreshold, out string color, out double L, out double a, out double b,
            out double C, out double H, out double mask_L, out double mask_A, out string error, out double temp,
            out double mask2_A, out List<Tuple<double, double, double, double, double, double>> hsvList,
            double Lconv = 1.0, double Aconv = 1.0, double Bconv = 1.0,
            double Lshift = 0, double Ashift = 0, double Bshift = 0)
        {
            bool result = false;
            hsvList = new List<Tuple<double, double, double, double, double, double>>();

            color = "";
            error = "";
            L = 0; a = 0; b = 0; C = 0; H = 0;
            mask_L = 0; mask_A = 0; mask2_A = 0;
            temp = 0;

            try
            {
                if (img.Count != bgImg.Count)
                {
                    error = "background image count not equal to image count";
                    return false;
                }

                imgAnalyzer.setLabAdjustment(Lconv, Aconv, Bconv,
                        Lshift, Ashift, Bshift);

                string L_description = "", C_description = "", H_description = "", comment1 = "",
                    comment2 = "", comment3 = "";

                bool colorResult = imgAnalyzer.Get_Description(ref img, ref bgImg, ref maskBmp,
                    ref L, ref a, ref b, ref C, ref H, ref L_description, ref C_description,
                    ref H_description, ref mask_L, ref mask_A, ref comment1, ref comment2, ref comment3,
                    ref mask2_A, ref hsvList, false,
                    App.Settings.MaxPhotoChromicLDiff, brightAreaThreshold, darkAreaThreshold);

                temp = cameraControl.CameraTemperature(cameraControl.cameras[(int)CameraType.Top]);

                if (colorResult)
                {
                    color = C_description;
                    result = true;
                    error = comment3;
                }
                else
                {
                    error = "Bad color result: " + comment3;
                    result = false;
                }
            }
            catch (Exception ex)
            {
                error = ex.ToString();
            }

            return result;
        }

        void Cancel()
        {
            cancelEvent.Set();
        }


        void SaveDialog()
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Title = "Save current image";
            dlg.DefaultExt = "jpg";
            dlg.Filter = "Jpeg files|*.jpg|Bitmap files|*.bmp";
            if (dlg.ShowDialog() == true)
            {
                Busy = true;
                App.LogEntry.AddEntry("Saving " + dlg.FileName);

                BackgroundWorker bw = new BackgroundWorker();
                bw.DoWork += new DoWorkEventHandler(SaveImage);
                bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(SaveImageCompleted);
                bw.RunWorkerAsync(dlg.FileName);

            }
        }

        void SaveImage(object sender, DoWorkEventArgs e)
        {
            string filePath = (string)e.Argument;

            try
            {
                string extension = System.IO.Path.GetExtension(filePath);
                BitmapSource[] images = new BitmapSource[3];
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                    //images[0] = CameraImage;
                    //images[0].Freeze();
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        BitmapEncoder encoder = new JpegBitmapEncoder();
                        if (extension.ToUpper() == ".BMP")
                            encoder = new BmpBitmapEncoder();
                        encoder.Frames.Add(BitmapFrame.Create(CameraImage));
                        encoder.Save(fileStream);
                    }
                });
                //cameraControl.GetMeasurementImages(images[0], out images[1], out images[2]);
                //for (int i = 0; i < images.Length; i++)
                //{
                //    var fileName = Path.GetFileNameWithoutExtension(filePath);
                //    switch (i)
                //    {
                //        case 0:
                //            fileName += "Complete";
                //            break;
                //        case 1:
                //            fileName += "Diamond";
                //            break;
                //        case 2:
                //            fileName += "Background";
                //            break;
                //        default:
                //            throw new Exception("Bad");
                //    }
                //    fileName = Path.GetDirectoryName(filePath) + @"\" + fileName + extension;

                //    using (var fileStream = new FileStream(fileName, FileMode.Create))
                //    {
                //        BitmapEncoder encoder = new JpegBitmapEncoder();
                //        if (extension.ToUpper() == ".BMP")
                //            encoder = new BmpBitmapEncoder();
                //        encoder.Frames.Add(BitmapFrame.Create(images[i]));
                //        encoder.Save(fileStream);
                //    }
                //}

                e.Result = filePath + " saved!";
            }
            catch (Exception ex)
            {
                e.Result = "Save Failed: " + ex.Message;
            }
            finally
            {

            }
        }

        void SaveImageCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            App.LogEntry.AddEntry((string)e.Result);
            Busy = false;
        }


        bool IsWindowOpen<T>(string name = "") where T : Window
        {
            return string.IsNullOrEmpty(name)
               ? Application.Current.Windows.OfType<T>().Any()
               : Application.Current.Windows.OfType<T>().Any(w => w.Name.Equals(name));
        }

        void ShowGraph()
        {
            //if (!IsWindowOpen<Graph>())
            //{
            //    resultsGraph = new Graph();
            //    resultsGraphViewModel = new GraphViewModel();
            //    resultsGraph.DataContext = resultsGraphViewModel;
            //    resultsGraph.Show();
            //}
        }

        public WriteableBitmap SaveAsWriteableBitmap(Canvas surface)
        {
            if (surface == null) return null;

            // Save current canvas transform
            Transform transform = surface.LayoutTransform;
            // reset current transform (in case it is scaled or rotated)
            surface.LayoutTransform = null;

            // Get the size of canvas
            Size size = new Size(surface.ActualWidth, surface.ActualHeight);
            // Measure and arrange the surface
            // VERY IMPORTANT
            surface.Measure(size);
            surface.Arrange(new Rect(size));

            // Create a render bitmap and push the surface to it
            RenderTargetBitmap renderBitmap = new RenderTargetBitmap(
              (int)size.Width,
              (int)size.Height,
              96d,
              96d,
              PixelFormats.Pbgra32);
            renderBitmap.Render(surface);


            //Restore previously saved layout
            surface.LayoutTransform = transform;

            //create and return a new WriteableBitmap using the RenderTargetBitmap
            return new WriteableBitmap(renderBitmap);
        }

        private System.Drawing.Bitmap BitmapFromWriteableBitmap(WriteableBitmap writeBmp)
        {
            System.Drawing.Bitmap bmp;
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create((BitmapSource)writeBmp));
                enc.Save(outStream);
                bmp = new System.Drawing.Bitmap(outStream);
            }
            return bmp;
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
                    for (int i = 0; i < cameraControl.cameras.Count; i++)
                    {
                        cameraControl.Stop(cameraControl.cameras[i]);
                        //if (IsWindowOpen<Graph>())
                        //{
                        //    resultsGraph.Close();
                        //}
                    }
                    if (isRotating)
                    {
                        RotateStage();
                    }
                    if(motorControl.serialPorts.Count > 0)
                        motorControl.serialPorts[(int)MotorType.Linear].LightOn(false);
                    motorControl.DisConnect();
                }
                _disposed = true;
            }
        }
        #endregion
    }

    public static class DrawCanvas
    {
        public static Line NewLine(double x1, double y1, double x2, double y2, bool refPoint, Canvas cv)
        {

            Line line = new Line()
            {
                X1 = x1,
                Y1 = y1,
                X2 = x2,
                Y2 = y2,
                Stroke = refPoint ? Brushes.Green : Brushes.Red,
                StrokeThickness = refPoint ? 10 : 5
            };

            line.SnapsToDevicePixels = true;
            line.SetValue(RenderOptions.EdgeModeProperty, EdgeMode.Aliased);

            cv.Children.Add(line);

            return line;
        }

        public static Ellipse Circle(double x, double y, int width, int height, bool refPoint, Canvas cv, bool fill = false, double opacity = 1.0)
        {

            Ellipse circle = new Ellipse()
            {
                Width = width,
                Height = height,
                Stroke = refPoint ? Brushes.Green : Brushes.Red,
                Fill = fill ? Brushes.Green : null,
                Opacity = opacity,
                StrokeThickness = refPoint ? 10 : 5
            };

            cv.Children.Add(circle);

            circle.SetValue(Canvas.LeftProperty, x - width / 2.0);
            circle.SetValue(Canvas.TopProperty, y - height / 2.0);

            return circle;
        }

        public static TextBlock Text(double x, double y, string text, bool refPoint, SolidColorBrush color, Canvas cv, bool shift = true)
        {

            TextBlock textBlock = new TextBlock();

            textBlock.Text = text;

            textBlock.Foreground = color == null ? refPoint ? Brushes.Green : Brushes.Red : color;
            textBlock.FontSize = refPoint ? 36 : 24;
            textBlock.FontWeight = refPoint ? FontWeights.Bold : FontWeights.Normal;

            Canvas.SetLeft(textBlock, x - (shift ? (refPoint ? 10 : 7.5) : 0));
            Canvas.SetTop(textBlock, y - (shift ? (refPoint ? 58 : 40) : 10));

            cv.Children.Add(textBlock);

            return textBlock;
        }

        public static Rectangle Rect(double x, double y, int width, int height, SolidColorBrush color, Canvas cv)
        {
            Rectangle rect = new Rectangle()
            {
                Width = width,
                Height = height,
                Fill = color,
                Stroke = color
            };

            cv.Children.Add(rect);
            Canvas.SetTop(rect, y);
            Canvas.SetLeft(rect, x);

            return rect;
        }
    }
}
