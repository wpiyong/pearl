//using FlyCapture2Managed;
using SpinnakerNET;
using SpinnakerNET.GenApi;
using gPearlAnalyzer;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using gPearlAnalyzer.View;
using System.Windows.Threading;

namespace GiaCamera
{
    public delegate void ImageReceivedEventHandler(BitmapSource bmpSource, System.Drawing.Bitmap bmp, CameraType camType);

    public enum CameraType{
        Top = 0,
        Side
    }

    public class PtGreyCamera
    {
        string lastError;
        IManagedCamera camera = null;
        ImageReceivedEventHandler ImageReceivedEventNotification;
        ImageEventListener imageEventListener = null;
        INodeMap nodeMap = null;

        BitmapSource latestBitmapSrc = null;
        System.Drawing.Bitmap latestBitmap = null;

        object imageLock = new object();

        public string GetLastError() { return lastError; }
        bool isCapturing = false;

        public CameraType cameraType;

        class ImageEventListener : ManagedImageEvent
        {
            private PtGreyCamera _camera;
            public ImageEventListener(PtGreyCamera camera)
            {
                _camera = camera;
            }


            override protected void OnImageEvent(ManagedImage image)
            {
                Dispatcher dispatcher = Dispatcher.CurrentDispatcher;
                dispatcher.BeginInvokeShutdown(DispatcherPriority.Normal);
                Dispatcher.Run();
                _camera.OnImageGrabbed(image);
                //Console.WriteLine("capture image: {0}, exposure time: {1}", image.FrameID, image.ChunkData.ExposureTime);
                image.Release();
            }
        }

        void OnImageGrabbed(ManagedImage rawImage)
        {
            ManagedImage convertedBitmapSource = new ManagedImage();
            rawImage.ConvertToBitmapSource(PixelFormatEnums.BGR8, convertedBitmapSource);
            try
            {
                if (Monitor.TryEnter(imageLock))
                {
                    try
                    {
                        BitmapSource bmpSource = convertedBitmapSource.bitmapsource;
                        if(bmpSource == null)
                        {
                            return;
                        }
                        bmpSource.Freeze();

                        BitmapSource source = null;
                        if (cameraType == CameraType.Top)
                        {
                            if ((App.Settings.CropHeight == 0 && App.Settings.CropWidth == 0) ||
                                 (App.Settings.CropHeight + App.Settings.CropTop > bmpSource.Height) ||
                                 (App.Settings.CropWidth + App.Settings.CropLeft > bmpSource.Width)
                                )
                            {
                                source = bmpSource;
                            }
                            else
                            {
                                source = new CroppedBitmap(bmpSource,
                                    new System.Windows.Int32Rect((int)App.Settings.CropLeft,
                                                                    (int)App.Settings.CropTop,
                                                                    (int)(App.Settings.CropWidth == 0 ?
                                                                            bmpSource.Width : App.Settings.CropWidth),
                                                                    (int)(App.Settings.CropHeight == 0 ?
                                                                            bmpSource.Height : App.Settings.CropHeight)));
                            }
                        } else
                        {
                            source = bmpSource;
                        }
                        source.Freeze();

                        latestBitmap = GetBitmap(source);
                        //flip image vertically
                        if (App.Settings.FlipImage)
                            latestBitmap.RotateFlip(System.Drawing.RotateFlipType.RotateNoneFlipY);

                        latestBitmapSrc = gPearlAnalyzer.Model.Utility.GetBitmapSource(latestBitmap);
                        latestBitmapSrc.Freeze();
                        ImageReceivedEventNotification(latestBitmapSrc, latestBitmap, cameraType);
                    }
                    catch(Exception ex)
                    {
                        Debug.WriteLine("Error: exception: " + ex.Message);
                    }
                    finally
                    {
                        Monitor.Exit(imageLock);
                    }
                }
                

            }
            catch (Exception ex)
            {
                lastError = ex.Message;
            }
            finally
            {
                convertedBitmapSource.Dispose();
                GC.Collect();
            }
        }

        public System.Drawing.Bitmap GetBitmap(BitmapSource bitmapsource)
        {
            System.Drawing.Bitmap tempBitmap, bitmap;
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapsource));
                enc.Save(outStream);
                tempBitmap = new System.Drawing.Bitmap(outStream);
            }
            // TODO: fix those comment out which cause out of memory exception
            bitmap = tempBitmap.Clone(new System.Drawing.Rectangle(0, 0, tempBitmap.Width, tempBitmap.Height),
                System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            tempBitmap.Dispose();
            return bitmap;
        }

        public PtGreyCamera(IManagedCamera cam, CameraType camType)
        {
            camera = cam;
            cameraType = camType;
            nodeMap = camera.GetNodeMap();
        }

        // TODO: need to seperate connect camera and start capture
        public bool Start(ImageReceivedEventHandler OnImageReceived)
        {
            bool result = false;
            try
            {
                ImageReceivedEventNotification = OnImageReceived;

                // Configure image events
                imageEventListener = new ImageEventListener(this);
                camera.RegisterEvent(imageEventListener);

                result = true;

            }
            catch (Exception ex)
            {
                lastError = ex.Message;
                result = false;
                Disconnect();
            }

            return result;
        }

        public void StartCapture()
        {
            if(camera != null && !isCapturing)
            {
                camera.BeginAcquisition();
                isCapturing = true;
            }
        }

        public void StopCapture()
        {
            if(camera != null && isCapturing)
            {
                try
                {
                    camera.EndAcquisition();
                    isCapturing = false;
                }catch (Exception ex)
                {
                    Console.WriteLine("failed to EndAcquisition: " + ex.Message);
                }
            }
        }

        public void DefaultSettings()
        {
            //camera.RestoreFromMemoryChannel(0);

            //FC2Config config = new FC2Config();
            //config = camera.GetConfiguration();
            //config.grabTimeout = 200;
            //camera.SetConfiguration(config);
            try
            {
                StopCapture();
                if (RestoreDefaultSettings())
                {
                    if (!SetStreamBufferCount(1))
                        return;

                    //shutter, gain, wb off
                    SetProprtyAutomaticSetting("Shutter", false);
                    //camera.ExposureAuto.Value = ExposureAutoEnums.Off.ToString();
                    SetAbsolutePropertyValue("ShutterMode", "Timed");
                    //if(CameraType.Top == cameraType)
                        //SetAbsolutePropertyValue("Binning", "2");
                    //camera.ExposureMode.Value = ExposureModeEnums.Timed.ToString();
                    SetShutterTime(Convert.ToDouble(App.Settings.ShutterTime) * 1000);

                    //camera.GainAuto.Value = GainAutoEnums.Off.ToString();
                    SetProprtyAutomaticSetting("Gain", false);

                    // whitebalance auto setting enabled with uisng pixel format rgb 8
                    SetAbsolutePropertyValue("PixelFormat", "RGB8");
                    //SetProprtyAutomaticSetting("WhiteBalance", false);
                    //camera.BalanceWhiteAuto.Value = BalanceWhiteAutoEnums.Off.ToString();

                    EnableChunkData();
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error: DefaultSettings exception: " + ex.Message);
            }
        }

        bool RestoreDefaultSettings()
        {
            bool result = false;
            try
            {
                for (int i = 0; i < 3; i++)
                {
                    try
                    {
                        camera.UserSetSelector.Value = UserSetSelectorEnums.Default.ToString();
                        camera.UserSetLoad.Execute();
                        result = true;
                        break;
                    }
                    catch (SpinnakerException s)
                    {
                        camera.AcquisitionMode.Value = AcquisitionModeEnums.Continuous.ToString();
                        camera.BeginAcquisition();
                        System.Threading.Thread.Sleep(500);
                        camera.EndAcquisition();
                    }
                }

                //TODO: stream buffer default count mode to manual
                // Set stream buffer Count Mode to manual
                // Retrieve Stream Parameters device nodemap
                INodeMap sNodeMap = camera.GetTLStreamNodeMap();
                IEnum streamBufferCountMode = sNodeMap.GetNode<IEnum>("StreamBufferCountMode");
                if (streamBufferCountMode == null || !streamBufferCountMode.IsWritable)
                {
                    return false;
                }

                IEnumEntry streamBufferCountModeManual = streamBufferCountMode.GetEntryByName("Manual");
                if (streamBufferCountModeManual == null || !streamBufferCountModeManual.IsReadable)
                {
                    return false;
                }

                streamBufferCountMode.Value = streamBufferCountModeManual.Value;

            }
            catch (Exception ex)
            {
                result = false;
            }

            return result;
        }

        public bool SetStreamBufferCount(long count)
        {
            try
            {
                //StreamDefaultBufferCount is the number of images to buffer on PC
                //default is 10
                INodeMap sNodeMap = camera.GetTLStreamNodeMap();
                IInteger streamNode = sNodeMap.GetNode<IInteger>("StreamDefaultBufferCount");
                if (streamNode == null || !streamNode.IsWritable)
                {
                    return false;
                }

                streamNode.Value = count;
            }
            catch
            {
                return false;
            }

            return true;
        }

        bool EnableChunkData()
        {
            bool result = true;

            try
            {
                IBool iChunkModeActive = nodeMap.GetNode<IBool>("ChunkModeActive");
                if (iChunkModeActive == null || !iChunkModeActive.IsWritable)
                {
                    Console.WriteLine("Cannot active chunk mode. Aborting...");
                    return false;
                }

                iChunkModeActive.Value = true;

                IEnum iChunkSelector = nodeMap.GetNode<IEnum>("ChunkSelector");
                if (iChunkSelector == null || !iChunkSelector.IsReadable)
                {
                    Console.WriteLine("Chunk selector not available. Aborting...");
                    return false;
                }

                IEnumEntry frameIDEntry = iChunkSelector.GetEntryByName("FrameCounter");//iChunkSelector.Entries[2];
                if (!frameIDEntry.IsAvailable || !frameIDEntry.IsReadable)
                {
                    
                }else
                {
                    iChunkSelector.Value = frameIDEntry.Value;
                    IBool iChunkEnable = nodeMap.GetNode<IBool>("ChunkEnable");
                    if(iChunkEnable?.IsWritable == true)
                    {
                        iChunkEnable.Value = true;
                    }
                }

                IEnumEntry timeStampEntry = iChunkSelector.GetEntryByName("Timestamp");
                if (!timeStampEntry.IsAvailable || !timeStampEntry.IsReadable)
                {
                    
                }
                else
                {
                    iChunkSelector.Value = timeStampEntry.Value;
                    IBool iChunkEnable = nodeMap.GetNode<IBool>("ChunkEnable");
                    if (iChunkEnable?.IsWritable == true)
                    {
                        iChunkEnable.Value = true;
                    }
                }

                IEnumEntry exposureTimeEntry = iChunkSelector.GetEntryByName("ExposureTime");
                if (!exposureTimeEntry.IsAvailable || !exposureTimeEntry.IsReadable)
                {

                }
                else
                {
                    iChunkSelector.Value = exposureTimeEntry.Value;
                    IBool iChunkEnable = nodeMap.GetNode<IBool>("ChunkEnable");
                    if (iChunkEnable?.IsWritable == true)
                    {
                        iChunkEnable.Value = true;
                    }
                }

            }
            catch (SpinnakerException ex)
            {
                Debug.WriteLine("Error: " + ex.Message);
                result = false;
            }

            return result;
        }

        public bool GetImage(out BitmapSource bmpSource, out System.Drawing.Bitmap bmp)
        {
            bool result = false;

            bmp = null;
            bmpSource = null;

            try
            {
                if (Monitor.TryEnter(imageLock, 5000))
                {
                    try
                    {
                        bmp = new System.Drawing.Bitmap(latestBitmap.Width, latestBitmap.Height,
                            latestBitmap.PixelFormat);
                        using (System.Drawing.Graphics gr = System.Drawing.Graphics.FromImage(bmp))
                        {
                            gr.DrawImage(latestBitmap, 
                                new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height));
                        }


                        bmpSource = latestBitmapSrc.Clone();
                        bmpSource.Freeze();

                        result = true;
                    }
                    finally
                    {
                        Monitor.Exit(imageLock);
                    }
                }
                else
                    throw new Exception("timed out waiting for imageLock");

            }
            catch (Exception ex)
            {
                lastError = ex.Message;
                result = false;
            }
            return result;
        }

        public void Disconnect()
        {
            try
            {
                if (camera != null)
                {
                    camera.Dispose();
                    //camera.EndAcquisition();
                    //camera.UnregisterEvent(imageEventListener);
                    //camera.DeInit();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("PtGrey Disconnect : " + ex.Message);
            }
            finally
            {

            }
        }

        public void SetCameraVideoModeAndFrameRate(double newFrameRate)
        {
            bool restartCapture = true;
            try
            {
                StopCapture();
            }
            catch (SpinnakerException)
            {
                throw;
            }

            try
            {
                //camera.SetVideoModeAndFrameRate(newVideoMode, newFrameRate);
                // Set acquisition mode to continuous
                IEnum iAcquisitionMode = nodeMap.GetNode<IEnum>("AcquisitionMode");
                if (iAcquisitionMode == null || !iAcquisitionMode.IsWritable)
                {
                    Console.WriteLine("Unable to set acquisition mode to continuous (node retrieval). Aborting...\n");
                    restartCapture = false;
                }

                IEnumEntry iAcquisitionModeContinuous = iAcquisitionMode.GetEntryByName("Continuous");
                if (iAcquisitionModeContinuous == null || !iAcquisitionMode.IsReadable)
                {
                    Console.WriteLine("Unable to set acquisition mode to continuous (enum entry retrieval). Aborting...\n");
                    restartCapture = false;
                }

                iAcquisitionMode.Value = iAcquisitionModeContinuous.Symbolic;
            }
            catch (SpinnakerException /*ex*/)
            {
                throw;
            }

            if (!SetFrameRate(newFrameRate))
            {
                restartCapture = false;
            }

            if (restartCapture)
            {
                StartCapture();
            }
        }

        public void SetHue(double value)
        {
            SetAbsolutePropertyValue("Hue", value.ToString());
        }

        public void SetSaturation(double value)
        {
            SetAbsolutePropertyValue("Saturation", value.ToString());
        }

        public void SetAbsolutePropertyValue(string property, string newValue)
        {
            try
            {
                if (property == "Hue")
                {
                    IFloat hue = nodeMap.GetNode<IFloat>("Hue");
                    if (hue != null)
                    {
                        hue.Value = Convert.ToDouble(newValue);
                    } else
                    {
                        Debug.WriteLine("Error: SetAbsolutePropertyValue for " + property);
                    }
                }
                else if (property == "Gamma")
                {
                    IFloat gamma = nodeMap.GetNode<IFloat>("Gamma");
                    gamma.Value = Convert.ToDouble(newValue);
                }
                else if (property == "Width")
                {
                    IInteger width = nodeMap.GetNode<IInteger>("Width");
                    width.Value = Convert.ToInt32(newValue);
                }
                else if (property == "Height")
                {
                    IInteger height = nodeMap.GetNode<IInteger>("Height");
                    height.Value = Convert.ToInt32(newValue);
                }
                else if (property == "Gain")
                {
                    IEnum gainAuto = nodeMap.GetNode<IEnum>("GainAuto");
                    gainAuto.Value = "Off";

                    IFloat gainValue = nodeMap.GetNode<IFloat>("Gain");
                    gainValue.Value = Convert.ToDouble(newValue);
                }
                else if (property == "Saturation")
                {
                    IEnum saturationAuto = nodeMap.GetNode<IEnum>("SaturationAuto");
                    saturationAuto.Value = "Off";

                    IFloat saturationValue = nodeMap.GetNode<IFloat>("Saturation");
                    saturationValue.Value = Convert.ToDouble(newValue);
                }
                else if (property == "BlackLevel")
                {
                    IFloat blackLevelValue = nodeMap.GetNode<IFloat>("BlackLevel");
                    blackLevelValue.Value = Convert.ToDouble(newValue);
                }
                else if (property == "FrameRate")
                {
                    //IBool frameRateEnable = nodeMap.GetNode<IBool>("AcquisitionFrameRateEnable");
                    //frameRateEnable.Value = true;
                    SetProprtyEnabledSetting("AcquisitionFrameRate", true);

                    if(cameraType == CameraType.Top)
                    {
                        SetProprtyAutomaticSetting(property, false);
                    }
                    IFloat frameRateValue = nodeMap.GetNode<IFloat>("AcquisitionFrameRate");
                    frameRateValue.Value = Convert.ToDouble(newValue);
                }
                else if (property == "PixelFormat")
                {
                    IEnum pixelFormat = nodeMap.GetNode<IEnum>("PixelFormat");
                    IEnumEntry pixelFormatItem = pixelFormat.GetEntryByName(newValue);

                    if (pixelFormatItem?.IsReadable == true)
                    {
                        pixelFormat.Value = pixelFormatItem.Symbolic;
                    }
                }
                else if (property == "VideoMode")
                {
                    IEnum acquisitionMode = nodeMap.GetNode<IEnum>("AcquisitionMode");
                    if (acquisitionMode?.IsWritable == true)
                    {
                        IEnumEntry acquisitionModeItem = acquisitionMode.GetEntryByName(newValue);
                        if (acquisitionModeItem?.IsReadable == true)
                        {
                            acquisitionMode.Value = acquisitionModeItem.Symbolic;
                        } else
                        {
                            Debug.WriteLine("Error: SetAbsolutePropertyValue for " + property);
                        }
                    } else
                    {
                        Debug.WriteLine("Error: SetAbsolutePropertyValue for " + property);
                    }
                }
                else if (property == "Binning")
                {
                    IInteger binningValue = nodeMap.GetNode<IInteger>("BinningVertical");
                    binningValue.Value = Convert.ToInt32(newValue);
                }
                else if (property == "ShutterMode")
                {
                    IEnum exposureMode = nodeMap.GetNode<IEnum>("ExposureMode");
                    if (exposureMode?.IsWritable == true)
                    {
                        IEnumEntry exposureModeItem = exposureMode.GetEntryByName(newValue);
                        if (exposureModeItem?.IsReadable == true)
                        {
                            exposureMode.Value = exposureModeItem.Symbolic;
                        }
                        else
                        {
                            Debug.WriteLine("Error: SetAbsolutePropertyValue for " + property);
                        }
                    }
                    else
                    {
                        Debug.WriteLine("Error: SetAbsolutePropertyValue for " + property);
                    }
                }
                else if (property == "StreamBufferMode")
                {
                    INodeMap nodeMapStream = camera.GetTLStreamNodeMap();
                    IEnum bufferMode = nodeMapStream.GetNode<IEnum>("StreamBufferHandlingMode");
                    if (bufferMode?.IsWritable == true)
                    {
                        IEnumEntry bufferModeItem = bufferMode.GetEntryByName(newValue);
                        if (bufferModeItem?.IsReadable == true)
                        {
                            bufferMode.Value = bufferModeItem.Symbolic;
                        }
                        else
                        {
                            Debug.WriteLine("Error: SetAbsolutePropertyValue for " + property);
                        }
                    }
                    else
                    {
                        Debug.WriteLine("Error: SetAbsolutePropertyValue for " + property);
                    }
                }
                else
                {
                    Debug.WriteLine("Error: SetAbsolutePropertyValue for " + property + " not implemented.");
                }
            } catch (SpinnakerException e)
            {
                Debug.WriteLine("Error: SetAbsolutePropertyValue for " + property + " exceptoin: " + e.Message);
            }
            
        }

        public string GetPropertyValue(string property, bool valueB = false)
        {
            if (property == "Shutter")
            {
                IFloat node = nodeMap.GetNode<IFloat>("ExposureTime");
                return node.Value.ToString();
            }
            else if (property == "DeviceTemperature")
            {
                IFloat node = nodeMap.GetNode<IFloat>("DeviceTemperature");
                return node.Value.ToString();
            }
            else if (property == "WidthMax")
            {
                IInteger node = nodeMap.GetNode<IInteger>("WidthMax");
                return node.Value.ToString();
            }
            else if (property == "HeightMax")
            {
                IInteger node = nodeMap.GetNode<IInteger>("HeightMax");
                return node.Value.ToString();
            }
            else if (property == "FrameRate")
            {
                IFloat node = nodeMap.GetNode<IFloat>("AcquisitionFrameRate");
                return node.Value.ToString();
            }
            else
            {
                IEnum node = nodeMap.GetNode<IEnum>(property);
                return node.Value.ToString();
            }
        }


        public void AdjustWhiteBalance(double increment, bool valueB, ref double oldValue)
        {
            // TODO: increment value need to check for spinnaker, before is 1 unit step for increase/decrease
            //oldValue = valueB ? GetWhiteBalanceBlue() : GetWhiteBalanceRed();

            //if(valueB)
            //{
            //    SetWhiteBalanceBlue(oldValue + increment);
            //} else
            //{
            //    SetWhiteBalanceRed(oldValue + increment);
            //}

            oldValue = valueB ? GetWhiteBalanceBlue() : GetWhiteBalanceRed();

            if (valueB)
            {
                SetWhiteBalanceBlue(oldValue + increment);
                if (GetWhiteBalanceBlue() == oldValue)
                    SetWhiteBalanceBlue(oldValue + increment * 1.5);
            }
            else
            {
                SetWhiteBalanceRed(oldValue + increment);
                if (GetWhiteBalanceRed() == oldValue)
                    SetWhiteBalanceRed(oldValue + increment * 1.5);
            }
        }

        public double SetWhiteBalance(double newValue, bool valueB)
        {
            double oldValue;

            IEnum balanceWhiteAuto = nodeMap.GetNode<IEnum>("BalanceWhiteAuto");
            balanceWhiteAuto.Value = "Off";
            IEnum balanceRatioSelector = nodeMap.GetNode<IEnum>("BalanceRatioSelector");
            if (valueB)
            {
                oldValue = GetWhiteBalanceBlue();
                SetWhiteBalanceBlue(newValue);
            } else
            {
                oldValue = GetWhiteBalanceRed();
                SetWhiteBalanceRed(newValue);
            }

            return oldValue;
        }

        public void SetProprtyAutomaticSetting(string property, bool automatic)
        {
            try
            {
                if (property == "Gain")
                {
                    IEnum gainAuto = nodeMap.GetNode<IEnum>("GainAuto");
                    if (automatic)
                    {
                        // TODO: may have other selection such as "Once"
                        gainAuto.Value = "Continuous";
                    }
                    else
                    {
                        gainAuto.Value = "Off";
                    }
                }
                else if (property == "Shutter")
                {
                    IEnum exposureAuto = nodeMap.GetNode<IEnum>("ExposureAuto");
                    if (automatic)
                    {
                        // TODO: may have other selection such as "Once"
                        exposureAuto.Value = "Continuous";
                    }
                    else
                    {
                        exposureAuto.Value = "Off";
                    }
                }
                else if (property == "Sharpness")
                {
                    IEnum sharpnessAuto = nodeMap.GetNode<IEnum>("SharpnessAuto");
                    if (automatic)
                    {
                        // TODO: may have other selection such as "Once"
                        sharpnessAuto.Value = "Continuous";
                    }
                    else
                    {
                        sharpnessAuto.Value = "Off";
                    }
                }
                else if (property == "FrameRate")
                {
                    IEnum framerateAuto = nodeMap.GetNode<IEnum>("AcquisitionFrameRateAuto");
                    if (automatic)
                    {
                        // TODO: may have other selection such as "Once"
                        framerateAuto.Value = "Continuous";
                    }
                    else
                    {
                        framerateAuto.Value = "Off";
                    }
                }
                else if (property == "WhiteBalance")
                {
                    IEnum whiteBalanceAuto = nodeMap.GetNode<IEnum>("BalanceWhiteAuto");
                    if (automatic)
                    {
                        // TODO: may have other selection such as "Once"
                        IEnumEntry iBalanceWhiteAutoModeContinuous = whiteBalanceAuto.GetEntryByName("Continuous");
                        if (iBalanceWhiteAutoModeContinuous?.IsReadable == true)
                        {
                            whiteBalanceAuto.Value = iBalanceWhiteAutoModeContinuous.Symbolic;
                        }
                    }
                    else
                    {
                        whiteBalanceAuto.Value = "Off";
                    }
                }
                else if (property == "ExposureCompensationAuto")
                {
                    IEnum expoCompAuto = nodeMap.GetNode<IEnum>("pgrExposureCompensationAuto");
                    if (automatic)
                    {
                        // TODO: may have other selection such as "Once"
                        IEnumEntry iExpoCompAutoModeContinuous = expoCompAuto.GetEntryByName("Continuous");
                        if (iExpoCompAutoModeContinuous?.IsReadable == true)
                        {
                            expoCompAuto.Value = iExpoCompAutoModeContinuous.Symbolic;
                        }
                    }
                    else
                    {
                        expoCompAuto.Value = "Off";
                    }
                }
                else
                {
                    Debug.WriteLine("Error: SetPropertyAutomaticSetting for " + property + " not implemented.");
                }
            } catch(SpinnakerException e)
            {
                Debug.WriteLine("Error: SetPropertyAutomaticSetting for " + property + " exceptoin: " + e.Message);
            }
        }

        public void SetProprtyEnabledSetting(string property, bool enabled)
        {
            try
            {
                BoolNode boolNode;
                if (property == "Gamma")
                {
                    boolNode = nodeMap.GetNode<BoolNode>("GammaEnabled");
                    if (boolNode?.IsReadable == true)
                    {
                        boolNode.Value = enabled;
                    }
                    else
                    {
                        Debug.WriteLine("Error: SetProprtyEnabledSetting {0} not readable", property);
                    }
                }
                else if (property == "Sharpness")
                {
                    boolNode = nodeMap.GetNode<BoolNode>("SharpnessEnabled");
                    if (boolNode?.IsReadable == true)
                    {
                        boolNode.Value = enabled;
                    }
                    else
                    {
                        Debug.WriteLine("Error: SetProprtyEnabledSetting {0} not readable", property);
                    }
                }
                else if (property == "Hue")
                {
                    boolNode = nodeMap.GetNode<BoolNode>("HueEnabled");
                    if (boolNode?.IsReadable == true)
                    {
                        boolNode.Value = enabled;
                    }
                    else
                    {
                        Console.WriteLine("Error: SetProprtyEnabledSetting {0} not readable", property);
                    }
                }
                else if (property == "Saturation")
                {
                    boolNode = nodeMap.GetNode<BoolNode>("SaturationEnabled");
                    if (boolNode?.IsReadable == true)
                    {
                        boolNode.Value = enabled;
                    }
                    else
                    {
                        Console.WriteLine("Error: SetProprtyEnabledSetting {0} not readable", property);
                    }
                }
                else if(property == "AcquisitionFrameRate")
                {
                    boolNode = nodeMap.GetNode<BoolNode>("AcquisitionFrameRateEnabled");
                    if (boolNode?.IsReadable == true)
                    {
                        boolNode.Value = enabled;
                    }
                    else
                    {
                        Console.WriteLine("Error: SetProprtyEnabledSetting {0} not readable", property);
                    }
                }
                else
                {
                    Console.WriteLine("Error: SetProprtyEnabledSetting {0} not implemented", property);
                }
            } catch (SpinnakerException e)
            {
                Debug.WriteLine("Error: SetProprtyEnabledSetting " + property + " exceptoin: " + e.Message);
            }
        }

        public bool SetShutterTime(double value)
        {
            bool result = false;

            try
            {
                camera.ExposureTime.Value = value;//microseconds
                result = true;
            }
            catch (SpinnakerException ex)
            {
                Debug.WriteLine("Error: SetShutterTime exception: {0}", ex.Message);
                result = false;
            }


            return result;
        }

        public double GetShutterTime()
        {
            return camera.ExposureTime.Value;//microseconds
        }

        public bool SetGain(double value)
        {
            bool result = false;

            try
            {
                camera.Gain.Value = value;
                result = true;
            }
            catch (SpinnakerException ex)
            {
                Debug.WriteLine("Error: {0}", ex.Message);
                result = false;
            }


            return result;
        }

        public double GetGain()
        {
            return camera.Gain.Value;
        }

        public bool SetFrameRate(double value)
        {
            bool result = false;

            try
            {
                camera.AcquisitionFrameRate.Value = value;
                result = true;
            }
            catch (SpinnakerException ex)
            {
                Debug.WriteLine("Error: {0}", ex.Message);
                result = false;
            }


            return result;
        }

        public double GetFrameRate()
        {
            return camera.AcquisitionFrameRate.Value;
        }

        public bool SetWhiteBalanceRed(double wbRed)
        {
            bool result = false;

            try
            {
                camera.BalanceRatioSelector.Value = BalanceRatioSelectorEnums.Red.ToString();
                camera.BalanceRatio.Value = wbRed;
                result = true;
            }
            catch (SpinnakerException ex)
            {
                Debug.WriteLine("Error: {0}", ex.Message);
                result = false;
            }


            return result;
        }

        public double GetWhiteBalanceRed()
        {
            camera.BalanceRatioSelector.Value = BalanceRatioSelectorEnums.Red.ToString();
            return (double)camera.BalanceRatio.Value;
        }

        public bool SetWhiteBalanceBlue(double wbBlue)
        {
            bool result = false;

            try
            {
                camera.BalanceRatioSelector.Value = BalanceRatioSelectorEnums.Blue.ToString();
                camera.BalanceRatio.Value = wbBlue;
                result = true;
            }
            catch (SpinnakerException ex)
            {
                Debug.WriteLine("Error: {0}", ex.Message);
                result = false;
            }


            return result;
        }

        public double GetWhiteBalanceBlue()
        {
            camera.BalanceRatioSelector.Value = BalanceRatioSelectorEnums.Blue.ToString();
            return (double)camera.BalanceRatio.Value;
        }

        public void EditCameraSettings()
        {
            //todo: spinnaker PropertyGridControl
            CameraSettings camDlg = new CameraSettings(camera);
            camDlg.ShowDialog();
        }
    }
}
