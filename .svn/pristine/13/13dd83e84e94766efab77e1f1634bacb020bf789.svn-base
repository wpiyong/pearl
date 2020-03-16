using gPearlAnalyzer.Model;
using gPearlAnalyzer.ViewModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace gPearlAnalyzer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static bool IsAdmin = false;
        public static LogEntryViewModel LogEntry;

        public static FancyColorSettings Settings = new FancyColorSettings();
        public static MotorSettings motorSettings = new MotorSettings();

        MainWindowViewModel viewModel = null;

        private void Application_Startup(object sender, StartupEventArgs e)
        {

#if DEBUG
            IsAdmin = true;
#endif

            if (!Settings.Load() || !motorSettings.Load())
            {
                MessageBox.Show("Could not load settinsg file", "Cannot start");
                App.Current.Shutdown();
                return;
            }

            LogEntry = new LogEntryViewModel();

            var window = new MainWindow();
            viewModel = new MainWindowViewModel();
            window.DataContext = viewModel;
            window.Closing += viewModel.OnWindowClosing;

            window.Show();
        }

        private void Application_DispatcherUnhandledException(object sender,
            System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            System.Windows.MessageBox.Show(e.Exception.ToString(), "Unhandled exception, shutting down");
            Application.Current.Shutdown();
        }
    }

    
}
