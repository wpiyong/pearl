using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace gPearlAnalyzer.Model
{
    public class FancyColorSettings
    {
        protected XDocument _settings;
        protected string settingsFileName = "";

        public bool SaveMeasuments { get; set; }
        public string MeasurementsFolder { get; set; }
        public string MeasurementsFileExtension { get; set; }
        public double Saturation { get; set; }
        public double Gamma { get; set; }
        public double Gain { get; set; }
        public double BlackLevel { get; set; }
        public double WBConvergence { get; set; }
        public double FrameRate { get; set; }
        public double Hue { get; set; }
        public int WBIncrement { get; set; }
        public bool ExtractToTextFile { get; set; }
        public string TextFilePath { get; set; }
        public bool WBInitialize { get; set; }
        //public int WBInitializeRed { get; set; }
        //public int WBInitializeBlue { get; set; }
        public double WBInitializeRed { get; set; }
        public double WBInitializeBlue { get; set; }
        public double ShutterTime { get; set; }
        public double ShutterTimeDiff { get; set; }
        public double Temperature { get; set; }
        public double CameraTempDiff { get; set; }
        public double ADiff { get; set; }
        public double BDiff { get; set; }
        public int Time { get; set; }
        public double LConv { get; set; }
        public double AConv { get; set; }
        public double BConv { get; set; }
        public double Lshift { get; set; }
        public double Ashift { get; set; }
        public double Bshift { get; set; }
        public string BoundaryHash { get; set; }

        public double Voltage { get; set; }
        public double VoltageStep { get; set; }
        public double SlewRate { get; set; }

        public uint CropTop { get; set; }
        public uint CropLeft { get; set; }
        public uint CropHeight { get; set; }
        public uint CropWidth { get; set; }

        public double MotorContinuousVelocity { get; set; }
        public int TimeBetweenImagesMs { get; set; }
        public int MaxCenterDistanceDiff { get; set; }

        public double MaxPhotoChromicLDiff { get; set; }

        public int BrightAreaThreshold { get; set; }
        public int DarkAreaThreshold { get; set; }
        public bool FlipImage { get; set; }
        public int StepsPerRev { get; set; }
        public bool MultiThreading { get; set; }
        public int MaxLumiValue { get; set; }

        public uint SideCameraSerialNum { get; set; }
        public FancyColorSettings()
        {
            string currentDirectory = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);

            settingsFileName = currentDirectory + @"\fancyColorSettings.config";
        }

        public bool Load()
        {
            bool result = false;
            try
            {
                _settings = XDocument.Load(settingsFileName);
                foreach (var prop in this.GetType().GetProperties())
                {
                    prop.SetValue(this, Convert.ChangeType(Get(prop.Name), prop.PropertyType,
                        System.Globalization.CultureInfo.InvariantCulture));

                }

                result = true;
            }
            catch
            {
                result = false;
            }

            return result;
        }

        protected object Get(string name)
        {
            object res = null;

            var field = _settings.Descendants("setting")
                                    .Where(x => (string)x.Attribute("name") == name)
                                    .FirstOrDefault();

            if (field != null)
            {
                res = field.Element("value").Value;
            }
            else
                throw new Exception("Property not found in Settings");

            return res;
        }

        protected void Set(string name, object value)
        {
            var field = _settings.Descendants("setting")
                                    .Where(x => (string)x.Attribute("name") == name)
                                    .FirstOrDefault();

            if (field != null)
            {
                field.Element("value").Value = value.ToString();
            }
            else
                throw new Exception("Property not found in Settings");
        }

        public void Save()
        {
            foreach (var prop in this.GetType().GetProperties())
            {
                Set(prop.Name, prop.GetValue(this));
            }
            _settings.Save(settingsFileName);

        }
    }
}
