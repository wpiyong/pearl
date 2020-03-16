using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gPearlAnalyzer.ViewModel
{
    class MenuViewModel : ViewModelBase
    {
        bool testMode = false;

        ControlViewModel controlVM;

        public MenuViewModel(ControlViewModel cvm)
        {
            base.DisplayName = "MenuViewModel";
            controlVM = cvm;

#if DEBUG
            testMode = true;
#endif

            CommandAdmin = new RelayCommand(param => this.LogIn());
            CommandSettings = new RelayCommand(param => this.EditSettings());
            CommandCameraSettings = new RelayCommand(param => this.EditCameraSettings(int.Parse(param.ToString())),
                cc => { return controlVM.Connected; });

            CommandAbout = new RelayCommand(param => this.HelpAbout());

            CommandLogOut = new RelayCommand(param => this.LogOut());

            CommandTest = new RelayCommand(param => this.Test(), cc => { return testMode; });
        }

        public RelayCommand CommandAdmin { get; set; }
        public RelayCommand CommandSettings { get; set; }
        public RelayCommand CommandCameraSettings { get; set; }
        public RelayCommand CommandAbout { get; set; }
        public RelayCommand CommandLogOut { get; set; }
        public RelayCommand CommandTest { get; set; }

        public System.Windows.Visibility IsAdmin
        {
            get
            {
                return App.IsAdmin ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
            }
        }

        public System.Windows.Visibility IsNotAdmin
        {
            get
            {
                return App.IsAdmin ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;
            }
        }

        void LogIn()
        {
            
        }

        void EditSettings()
        {
            
        }

        void EditCameraSettings(int index)
        {
            controlVM.EditCameraSettings(index);
        }

        private void HelpAbout()
        {
            
        }


        void LogOut()
        {
            
        }

        void Test()
        {
        }

    }
}
