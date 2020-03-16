//using ClassOpenCV;
using ImageProcessorLib;
//using FlyCapture2Managed;
using SpinnakerNET;
using GiaCamera;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using gPearlAnalyzer.ViewModel;

namespace gPearlAnalyzer.Model
{
    class CameraControl
    {
        public List<PtGreyCamera> cameras = new List<PtGreyCamera>();
        PtGreyCamera cameraTop = null;
        PtGreyCamera cameraSide = null;
        ManagedSystem system = null;

        public CameraControl()
        {

        }

        public bool Connect()
        {
            bool result = false;
            try
            {
                system = new ManagedSystem();
                IList<IManagedCamera> camList = system.GetCameras();

                if (camList.Count != 2)
                {
                    int count = camList.Count;
                    foreach (IManagedCamera mc in camList)
                        mc.Dispose();

                    // Clear camera list before releasing system
                    camList.Clear();

                    // Release system
                    system.Dispose();
                    throw new Exception("2 cameras needed but found " + camList.Count);
                }
                for (int i = 0; i < camList.Count; i++)
                {
                    var cam = camList[i];
                    // Initialize camera
                    cam.Init();
                    // Retrieve GenICam nodemap
                    var nodemap = cam.GetNodeMap();
                    uint SerialNumber = Convert.ToUInt32(cam.DeviceSerialNumber);
                    if (SerialNumber == App.Settings.SideCameraSerialNum)
                    {
                        cameraSide = new PtGreyCamera(cam, CameraType.Side);
                    }
                    else
                    {
                        cameraTop = new PtGreyCamera(cam, CameraType.Top);
                    }
                }

                if (cameraSide != null && cameraTop != null)
                {
                    cameras.Add(cameraTop);
                    cameras.Add(cameraSide);
                    result = true;
                } else
                {
                    result = false;
                }

            }
            catch (Exception ex)
            {
                result = false;
                if(cameraTop != null)
                {
                    cameraTop.Disconnect();
                }
                if(cameraSide != null)
                {
                    cameraSide.Disconnect();
                }
            }
            return result;
        }

        public string GetLastError(PtGreyCamera camera)
        {
            string error = "";
            if (camera != null)
                error = camera.GetLastError();

            return error;
        }

        public bool StartVideo(PtGreyCamera camera, ImageReceivedEventHandler imageReceivedHandler)
        {
            bool result = false;

            if (!camera.Start(imageReceivedHandler))
            {
                camera = null;
            }
            else
            {
                if (camera.cameraType == CameraType.Top)
                {
                    InitializeCameraSettings(camera);
                    InitializeCameraSettingsWB(camera, true);
                } else
                {
                    //InitializeBlackWhiteCameraSettings(camera);
                    // side camera changed to color camera
                    InitializeCameraSettings(camera);
                    InitializeCameraSettingsWB(camera, true);
                }
                result = true;
                camera.StartCapture();
            }

            return result;
        }


        public void Stop(PtGreyCamera camera)
        {
            if (camera != null)
            {
                camera.Disconnect();
            }
        }

        public void ChangeCameraSettings(byte settings)
        {
            Console.WriteLine("change camera settings");

            if ((settings & (byte)CameraSettings.Hue) == 1)
            {
                cameras[(int)CameraType.Top].SetHue(App.Settings.Hue);
            }

            if((settings & (byte)CameraSettings.Saturation) == 1)
            {
                cameras[(int)CameraType.Top].SetSaturation(App.Settings.Saturation);
            }

            if ((settings & (byte)CameraSettings.Gain) == 1)
            {
                cameras[(int)CameraType.Top].SetGain(App.Settings.Gain);
            }

            if ((settings & (byte)CameraSettings.Shutter) == 1)
            {
                cameras[(int)CameraType.Top].SetShutterTime(App.Settings.ShutterTime);
            }
        }

        public bool GetImages(PtGreyCamera camera, out System.Drawing.Bitmap img, out BitmapSource imgSrc)
        {
            img = null;
            imgSrc = null;

            if (!camera.GetImage(out imgSrc, out img))
                return false;

            return true;
        }

        public void AdjustShutter(PtGreyCamera camera, bool increase)
        {
            double current = camera.GetShutterTime();
            if (increase)
            {
                camera.SetShutterTime(current + 0.5 * current);
                Console.WriteLine("shutter time: {0}", current + 0.5 * current);
            } else
            {
                camera.SetShutterTime(0.5 * current);
                Console.WriteLine("shutter time: {0}", 0.5 * current);
            }
        }

        public bool Calibrate(PtGreyCamera camera)
        {
            bool result = false;

            try
            {
                App.LogEntry.AddEntry("Initializing calibration settings...");

                // TODO: those parameters never used
                //ImageProcessing.setLabAdjustment(App.Settings.LConv, App.Settings.AConv, App.Settings.BConv,
                //App.Settings.Lshift, App.Settings.Ashift, App.Settings.Bshift);

                SetCameraCalibrationParameters(camera);
                Thread.Sleep(2000);

                int retries = 0;
                bool success = false;
                for (retries = 0; retries < 40; retries++)
                {
                    App.LogEntry.AddEntry("Checking BGR...");

                    System.Drawing.Bitmap img = null;
                    BitmapSource imgSrc = null;
                    
                    if (!camera.GetImage(out imgSrc, out img))
                        return false;

                    double B = 0, G = 0, R = 0;
                    CamCalibrationUtil.calcBGR_wholeimage(ref img, ref B, ref G, ref R);

                    if (Math.Abs(R - G) <= App.Settings.WBConvergence &&
                               Math.Abs(B - G) <= App.Settings.WBConvergence)
                    {
                        App.LogEntry.AddEntry("G = " + G.ToString());
                        App.LogEntry.AddEntry("Finalizing calibration settings...");
                        FinishCalibration(camera);
                        success = true;
                        break;
                    }

                    App.LogEntry.AddEntry("Adjusting white balance...");
                    AdjustWhiteBalance(camera, R, G, B);
                    Thread.Sleep((int)(1000/App.Settings.FrameRate));
                }

                if (success)
                {
                    result = true;
                }
                else
                {
                    result = false;
                    
                }
            }
            catch (Exception ex)
            {
                result = false;
            }

            return result;
        }

        public void EditCameraSettings(PtGreyCamera camera)
        {
            //todo: spinnaker PropertyGridControl
            camera.EditCameraSettings();
        }

        #region calibration_settings

        private void InitializeCameraSettings(PtGreyCamera camera)
        {
            //first reset to default settings
            camera.DefaultSettings();

            // hue, gamma, and saturation setting controled by pixel format RGB8 will enable those settings
            //TODO: spinnaker
            //camera.SetCameraVideoModeAndFrameRate(VideoMode.VideoMode800x600Rgb,
            //    FrameRate.FrameRate30);
            camera.SetAbsolutePropertyValue("PixelFormat", "RGB8");
            camera.SetAbsolutePropertyValue("VideoMode", "Continuous");
            int width = Convert.ToInt32(camera.GetPropertyValue("WidthMax"));
            int height = Convert.ToInt32(camera.GetPropertyValue("HeightMax"));
            camera.SetAbsolutePropertyValue("Width", width.ToString());
            camera.SetAbsolutePropertyValue("Height", height.ToString());

            camera.SetProprtyAutomaticSetting("Shutter", true);

            camera.SetAbsolutePropertyValue("Gain", App.Settings.Gain.ToString());

            camera.SetProprtyEnabledSetting("AcquisitionFrameRate", true);
            camera.SetAbsolutePropertyValue("FrameRate", App.Settings.FrameRate.ToString());

            camera.SetProprtyEnabledSetting("Saturation", true);
            camera.SetAbsolutePropertyValue("Saturation", App.Settings.Saturation.ToString());

            camera.SetProprtyEnabledSetting("Gamma", true);
            camera.SetAbsolutePropertyValue("Gamma", App.Settings.Gamma.ToString());

            camera.SetProprtyEnabledSetting("Hue", true);
            camera.SetAbsolutePropertyValue("Hue", App.Settings.Hue.ToString());

            camera.SetStreamBufferCount(1); // test only
            camera.SetAbsolutePropertyValue("StreamBufferMode", "NewestOnly");
        }

        private void InitializeBlackWhiteCameraSettings(PtGreyCamera camera)
        {
            camera.SetAbsolutePropertyValue("VideoMode", "Continuous");
            int width = Convert.ToInt32(camera.GetPropertyValue("WidthMax"));
            int height = Convert.ToInt32(camera.GetPropertyValue("HeightMax"));
            camera.SetAbsolutePropertyValue("Width", width.ToString());
            camera.SetAbsolutePropertyValue("Height", height.ToString());

            camera.SetProprtyEnabledSetting("AcquisitionFrameRate", true);
            camera.SetAbsolutePropertyValue("FrameRate", "10");
            camera.SetProprtyAutomaticSetting("Shutter", false);
            camera.SetAbsolutePropertyValue("ShutterMode", "Timed");
            camera.SetShutterTime(10 * 1000);

            camera.SetStreamBufferCount(1); // test only
            camera.SetAbsolutePropertyValue("StreamBufferMode", "NewestOnly");
        }

        private void InitializeCameraSettingsWB(PtGreyCamera camera, bool auto)
        {
            if (App.Settings.WBInitialize)
            {
                if (auto)
                {
                    camera.SetProprtyAutomaticSetting("WhiteBalance", true);
                    //camera.SetWhiteBalance(App.Settings.WBInitializeRed, false);
                    //camera.SetWhiteBalance(App.Settings.WBInitializeBlue, true);
                } else
                {
                    camera.SetProprtyAutomaticSetting("WhiteBalance", false);
                }
            }
        }

        private void SetCameraCalibrationParameters(PtGreyCamera camera)
        {
            InitializeCameraSettings(camera);
            Thread.Sleep(1000);

            camera.StartCapture();
            //Default settings
            camera.SetProprtyAutomaticSetting("Sharpness", false);
            camera.SetProprtyAutomaticSetting("ExposureCompensationAuto", false);
            camera.SetProprtyAutomaticSetting("Saturation", false);
            camera.SetProprtyAutomaticSetting("Hue", false);
            camera.SetProprtyAutomaticSetting("FrameRate", false);
            //camera.SetProprtyAutomaticSetting("WhiteBalance", false);

            camera.SetAbsolutePropertyValue("BlackLevel", App.Settings.BlackLevel.ToString());
            camera.SetAbsolutePropertyValue("Saturation", App.Settings.Saturation.ToString());
            camera.SetAbsolutePropertyValue("Hue", App.Settings.Hue.ToString());
            camera.SetAbsolutePropertyValue("FrameRate", App.Settings.FrameRate.ToString());

            Thread.Sleep(200);

            InitializeCameraSettingsWB(camera, false);
            Thread.Sleep(1200);
            // TODO: start capture in calibration
            
        }

        private void AdjustWhiteBalance(PtGreyCamera camera, double R, double G, double B)
        {
            double oldValue = 0.0;

            //if (R - G > App.Settings.WBConvergence)
            //{
            //    camera.AdjustWhiteBalance(-1, false, ref oldValue);
            //}
            //else if (G - R > App.Settings.WBConvergence)
            //{
            //    camera.AdjustWhiteBalance(1, false, ref oldValue);
            //}

            //if (B - G > App.Settings.WBConvergence)
            //{
            //    camera.AdjustWhiteBalance(-1, true, ref oldValue);
            //}
            //else if (G - B > App.Settings.WBConvergence)
            //{
            //    camera.AdjustWhiteBalance(1, true, ref oldValue);
            //}


            if (R - G > App.Settings.WBConvergence)
            {
                camera.AdjustWhiteBalance(-0.04, false, ref oldValue);
                //Application.Current.Dispatcher.Invoke((Action)(() =>
                //    App.LogEntry.AddEntry("Calibration Mode : Decremented WB Red from " + oldValue)));
            }
            else if (G - R > App.Settings.WBConvergence)
            {
                camera.AdjustWhiteBalance(0.04, false, ref oldValue);
                //Application.Current.Dispatcher.Invoke((Action)(() =>
                //    App.LogEntry.AddEntry("Calibration Mode : Incremented WB Red from " + oldValue)));
            }

            if (B - G > App.Settings.WBConvergence)
            {
                camera.AdjustWhiteBalance(-0.04, true, ref oldValue);
                //Application.Current.Dispatcher.Invoke((Action)(() =>
                //    App.LogEntry.AddEntry("Calibration Mode : Decremented WB Blue from " + oldValue)));
            }
            else if (G - B > App.Settings.WBConvergence)
            {
                camera.AdjustWhiteBalance(0.04, true, ref oldValue);
                //Application.Current.Dispatcher.Invoke((Action)(() =>
                //    App.LogEntry.AddEntry("Calibration Mode : Incremented WB Blue from " + oldValue)));
            }

            Thread.Sleep(500);
        }

        private void FinishCalibration(PtGreyCamera camera)
        {
            camera.SetProprtyAutomaticSetting("Shutter", false);
            Thread.Sleep(500);
        }

        #endregion

/*
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

                ImageProcessing.setLabAdjustment(Lconv, Aconv, Bconv,
                        Lshift, Ashift, Bshift);
              
                
                //string unused = "";
                //int x = (int)(img.Width / 2.0);
                //int y = (int)(img.Height / 2.0);
                //if (!ImageProcessing.check_diamond_position(ref img, x, y, ref unused))
                //{
                //    error = "Check diamond position";
                //    return false;
                //}

               
                //var imageList = new List<System.Drawing.Bitmap>();

                //for (int i = 0; i < img.Count; i++)
                //{
                //    BitmapSource mainImage, bgImage;

                //    if (!GetMeasurementImages(imgSrc[i], out mainImage, out bgImage))
                //    {
                //        error = "Could not divide image";
                //        return false;
                //    }

                //    imageList.Add(camera.GetBitmap(mainImage));
                //}                


                string L_description = "", C_description = "", H_description = "", comment1 = "",
                    comment2 = "", comment3 = "";

                bool colorResult = ImageProcessing.GetColor_description(ref img, ref bgImg,
                    ref L, ref a, ref b, ref C, ref H, ref L_description, ref C_description,
                    ref H_description, ref mask_L, ref mask_A, ref comment1, ref comment2, ref comment3, 
                    ref mask2_A, ref hsvList, false,
                    App.Settings.MaxPhotoChromicLDiff, brightAreaThreshold, darkAreaThreshold);

                temp = CameraTemperature();

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

        

    */    

        public double CameraTemperature(PtGreyCamera camera)
        {
            double temp = 0;
            if (camera != null)
            {
                // TODO: need to verify the value
                temp = Convert.ToDouble(camera.GetPropertyValue("DeviceTemperature"));
            }

            return temp;
        }
    }
}
