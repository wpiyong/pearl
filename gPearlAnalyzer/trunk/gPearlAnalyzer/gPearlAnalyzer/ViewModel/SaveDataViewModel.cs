using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace gPearlAnalyzer.ViewModel
{
    class SaveDataViewModel : ViewModelBase
    {
        public SaveDataViewModel(string controlNum, string description, double l, double c, double h)
        {
            base.DisplayName = "ControlViewModel";
            ControlNum = controlNum;
            Description = description;
            LValue = l;
            CValue = c;
            HValue = h;

            CommandSave = new RelayCommand(param => Save(param));
            CommandCancel = new RelayCommand(param => Cancel(param));
        }

        public RelayCommand CommandSave { get; set; }
        public RelayCommand CommandCancel { get; set; }

        string _controlNum;
        public string ControlNum
        {
            get
            {
                return _controlNum;
            }
            set
            {
                _controlNum = value;
                OnPropertyChanged("ControlNum");
            }
        }

        string _description;
        public string Description
        {
            get
            {
                return _description;
            }
            set
            {
                _description = value;
                OnPropertyChanged("Description");
            }
        }

        double _cValue;
        public double CValue
        {
            get
            {
                return _cValue;
            }
            set
            {
                _cValue = value;
                OnPropertyChanged("CValue");
            }
        }

        double _hValue;
        public double HValue
        {
            get
            {
                return _hValue;
            }
            set
            {
                _hValue = value;
                OnPropertyChanged("HValue");
            }
        }

        double _lValue;
        public double LValue
        {
            get
            {
                return _lValue;
            }
            set
            {
                _lValue = value;
                OnPropertyChanged("LValue");
            }
        }

        void Save(object o)
        {
            ((Window)o).DialogResult = true;
            ((Window)o).Close();
        }

        void Cancel(object o)
        {
            ((Window)o).DialogResult = false;
            ((Window)o).Close();
        }


    }
}
