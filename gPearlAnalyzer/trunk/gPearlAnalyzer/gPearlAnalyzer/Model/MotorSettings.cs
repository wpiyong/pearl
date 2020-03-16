using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SettingsLib;

namespace gPearlAnalyzer.Model
{
    public class MotorSettings : Settings
    {
        public MotorSettings() : base("motorSettings.config")
        {

        }

        public double GapInPixel { get; set; }

        public double XPixelsInMM { get; set; }
        public double YPixelsInMM { get; set; }
        public double TopPixelsInMM { get; set; }
    }
}
