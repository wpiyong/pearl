using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace gPearlAnalyzer.ViewModel
{
    class ControlNumberViewModel : ViewModelBase
    {
        public ControlNumberViewModel()
        {
            base.DisplayName = "ControlNumberViewModel";

            CommandCancel = new RelayCommand(param => this.Close(param));
            CommandOK = new RelayCommand(param => this.Continue(param));
        }
          

        
        public RelayCommand CommandCancel { get; set; }
        public RelayCommand CommandOK { get; set; }

        public string ControlNumber { get; set; }

        private bool IsValid(DependencyObject obj)
        {
            // The dependency object is valid if it has no errors, 
            //and all of its children (that are dependency objects) are error-free.
            return !Validation.GetHasError(obj) &&
                LogicalTreeHelper.GetChildren(obj)
                .OfType<DependencyObject>()
                .All(IsValid);
        }

        void Continue(object param)
        {
            if (ControlNumber != null &&  ControlNumber.Length > 0)
            {
                ((Window)param).DialogResult = true;
            }
            else
                Close(param);
        }

        void Close(object param)
        {
            ((Window)param).Close();
        }
    }
}
