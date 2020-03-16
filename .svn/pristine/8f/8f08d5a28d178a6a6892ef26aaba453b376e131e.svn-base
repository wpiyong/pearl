using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gPearlAnalyzer.ViewModel
{
    class MainWindowViewModel : ViewModelBase
    {
        public MainWindowViewModel()
        {
            base.DisplayName = "MainWindowViewModel";

            ControlVM = new ControlViewModel();
            MenuVM = new MenuViewModel(ControlVM);

            App.LogEntry.AddEntry("Application Started");
        }

        public LogEntryViewModel LogEntryVM { get { return App.LogEntry; } }

        public ControlViewModel ControlVM { get; set; }
        public MenuViewModel MenuVM { get; set; }


        void Dispose()
        {
            ControlVM.Dispose();
            //dispose other view models here
            LogEntryVM.Dispose();
            MenuVM.Dispose();
        }

        public void OnWindowClosing(object sender, CancelEventArgs e)
        {
            Dispose();
            App.Current.Shutdown();
        }
    }
}
