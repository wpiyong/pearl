using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace gPearlAnalyzer.Model
{
    class Stone : IDataErrorInfo, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;


        Dictionary<string, string> validationErrors = new Dictionary<string, string>();
        string _controlNumber;
        string _comment1, _comment2, _comment3;
        double _l, _a, _b, _c, _h;
        double _mask_L, _mask_A;
        string _lDesc, _cDesc, _hDesc;
        double _measurementTemperature;

        public Stone()
        {
            validationErrors["ControlNumber"] = String.Empty;
        }

        public string ControlNumber
        {
            get
            {
                return _controlNumber;
            }
            set
            {
                _controlNumber = value;
                if (!Validate(_controlNumber))
                {
                    validationErrors["ControlNumber"] = "Invalid Control Number";
                }
                else
                {
                    validationErrors["ControlNumber"] = String.Empty;
                }
                OnPropertyChanged("ControlNumber");
            }
        }

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this,
                    new PropertyChangedEventArgs(propertyName));
            }
        }

        public string Description
        {
            get
            {
                return CDesc + " " + LDesc + " " + HDesc;
            }
        }

        public string CDesc
        {
            get
            {
                return _cDesc;
            }
            set
            {
                _cDesc = value;
                OnPropertyChanged("CDesc");
            }
        }
        public string LDesc
        {
            get
            {
                return _lDesc;
            }
            set
            {
                _lDesc = value;
                OnPropertyChanged("LDesc");
            }
        }
        public string HDesc
        {
            get
            {
                return _hDesc;
            }
            set
            {
                _hDesc = value;
                OnPropertyChanged("HDesc");
            }
        }

        public double L
        {
            get
            {
                return _l;
            }
            set
            {
                _l = value;
                OnPropertyChanged("L");
            }
        }
        public double A
        {
            get
            {
                return _a;
            }
            set
            {
                _a = value;
                OnPropertyChanged("A");
            }
        }
        public double B
        {
            get
            {
                return _b;
            }
            set
            {
                _b = value;
                OnPropertyChanged("B");
            }
        }
        public double C
        {
            get
            {
                return _c;
            }
            set
            {
                _c = value;
                OnPropertyChanged("C");
            }
        }
        public double H
        {
            get
            {
                return _h;
            }
            set
            {
                _h = value;
                OnPropertyChanged("H");
            }
        }
        public double MeasurementTemperature
        {
            get
            {
                return _measurementTemperature;
            }
            set
            {
                _measurementTemperature = value;
                OnPropertyChanged("MeasurementTemperature");
            }
        }

        public double Mask_L
        {
            get
            {
                return _mask_L;
            }
            set
            {
                _mask_L = value;
                OnPropertyChanged("Mask_L");
            }
        }

        public double Mask_A
        {
            get
            {
                return _mask_A;
            }
            set
            {
                _mask_A = value;
                OnPropertyChanged("Mask_A");
            }
        }

        public string Comment1
        {
            get
            {
                return _comment1;
            }
            set
            {
                _comment1 = value;
                OnPropertyChanged("Comment1");
            }
        }
        public string Comment2
        {
            get
            {
                return _comment2;
            }
            set
            {
                _comment2 = value;
                OnPropertyChanged("Comment2");
            }
        }
        public string Comment3
        {
            get
            {
                return _comment3;
            }
            set
            {
                _comment3 = value;
                OnPropertyChanged("Comment3");
            }
        }

        bool _goodColorResult;
        public bool GoodColorResult
        {
            get
            {
#if DEBUG
                return true;
#else
                if (!GlobalVariables.IsAdmin)
                    return _goodColorResult;
                else
                    return true;
#endif
            }
            set
            {
                _goodColorResult = value;
                OnPropertyChanged("GoodColorResult");
            }
        }


        public string Error
        {
            get { return String.Empty; }
        }

        public string this[string columnname]
        {
            get
            {
                string result = string.Empty;
                switch (columnname)
                {
                    case "ControlNumber":
                        result = validationErrors["ControlNumber"];
                        break;
                };
                return result;
            }
        }

        bool Validate(object value)
        {
            Regex regex = new Regex(@"^[0-9]+$");

            bool result = false;
            string inputString = (value ?? string.Empty).ToString();
            if (inputString.Length == 12 && regex.IsMatch(inputString))
            {
                result = true;
            }
            return result;
        }


        public void Save(string filePath, string file)
        {
            try
            {
                if (ControlNumber != null && ControlNumber.Length > 0)
                {
                    var fileName = filePath + @"\" + file;
                    Directory.CreateDirectory(filePath);

                    var firstWord = ControlNumber;
                    var secondWord = DateTime.Now.ToUniversalTime().ToString("MM/dd/yyyy hh:mm:ss tt");
                    var thirdWord = String.Empty;
                    var csv = new StringBuilder();

                    if (Comment1 != null && Comment1.Length > 0)
                    {
                        thirdWord = Comment1;
                        var newLine = string.Format("{0},{1},{2}{3}", firstWord, secondWord, thirdWord, Environment.NewLine);
                        csv.Append(newLine);

                    }
                    if (Comment2 != null && Comment2.Length > 0)
                    {
                        thirdWord = Comment2;
                        var newLine = string.Format("{0},{1},{2}{3}", firstWord, secondWord, thirdWord, Environment.NewLine);
                        csv.Append(newLine);
                    }



                    //note using UNIX epoch of 1970-01-01
                    //.Net epoch would be DateTime.MinValue
                    //TimeSpan t = DateTime.UtcNow - new DateTime(1970, 1, 1);
                    //int secondsSinceEpoch = (int)t.TotalSeconds;
                    //var fileName = filePath + @"\Colorimeter" + DateTime.Now.ToUniversalTime().ToString("yyyyMMddhhmmss") +
                    //                "_" + secondsSinceEpoch + ".txt";



                    // This text is added only once to the file.
                    if (!File.Exists(fileName))
                    {
                        // Create a file to write to.
                        string createText = "control_number, date, device, volume, L_diamond, a_diamond," +
                                            " b_diamond, L_background, a_background, b_background, L, a, b, C," +
                                            " H, L_description, C_description, H_description, version, masklength, maskarea," +
                                            " maskheight, maskpvheight, maxmin_widthratio, min_aspectratio, diamond_proportion, comment," +
                                            " temp_measurement, temp_background, shutter, blue_gain, red_gain" + Environment.NewLine;
                        File.WriteAllText(fileName, createText);
                    }




                    File.AppendAllText(fileName, csv.ToString());

                }

            }
            catch (Exception /*e*/)
            {
                //MessageBox.Show(e.Message, "Failed to save text file");
            }
        }

        public static void SaveShort(string filePath, string file, StringBuilder csv)
        {
            try
            {
                var fileName = filePath + @"\" + file;
                Directory.CreateDirectory(filePath);

                if (!File.Exists(fileName))
                {
                    // Create a file to write to.
                    string createText = "Date, ControlNumber, L, a, b, C, H, L, a, b, C, H, Temperature, MaskArea, Comment, Mask2Area, " +
                        "L, a, b, C, H, %, " +
                        "L_2a, a_2a, b_2a, C_2a, H_2a, %_2a, " +
                        "L_2b, a_2b, b_2b, C_2b, H_2b, %_2b, " +
                        "L_3a, a_3a, b_3a, C_3a, H_3a, %_3a, " +
                        "L_3b, a_3b, b_3b, C_3b, H_3b, %_3b, " +
                        "L_3c, a_3c, b_3c, C_3c, H_3c, %_3c, " +
                        "L_4a, a_4a, b_4a, C_4a, H_4a, %_4a, " +
                        "L_4b, a_4b, b_4b, C_4b, H_4b, %_4b, " +
                        "L_4c, a_4c, b_4c, C_4c, H_4c, %_4c, " +
                        "L_4d, a_4d, b_4d, C_4d, H_4d, %_4d, " +
                        "L_5a, a_5a, b_5a, C_5a, H_5a, %_5a, " +
                        "L_5b, a_5b, b_5b, C_5b, H_5b,  %_5b, " +
                        "L_5c, a_5c, b_5c, C_5c, H_5c,  %_5c, " +
                        "L_5d, a_5d, b_5d, C_5d, H_5d,  %_5d, " +
                        "L_5e, a_5e, b_5e, C_5e, H_5e,  %_5e " + Environment.NewLine;
                    File.WriteAllText(fileName, createText);
                }

                File.AppendAllText(fileName, csv.ToString());

            }
            catch (Exception /*e*/)
            {
                //MessageBox.Show(e.Message, "Failed to save text file");
            }
        }

    }
}
