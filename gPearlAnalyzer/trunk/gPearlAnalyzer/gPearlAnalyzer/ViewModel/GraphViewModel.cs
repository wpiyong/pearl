using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.Charts;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace gColorFancy.ViewModel
{
    class GraphViewModel
    {
        public ObservableDataSource<Point> SourceL {get;set;}
        public ObservableDataSource<Point> SourceA { get; set; }
        public ObservableDataSource<Point> SourceB { get; set; }
        public ObservableDataSource<Point> SourceC { get; set; }
        public ObservableDataSource<Point> SourceH { get; set; }
        public ObservableDataSource<Point> SourceT { get; set; }

        public RelayCommand PreviewDropCommand { get; set; }

        public GraphViewModel()
        {
            SourceL = new ObservableDataSource<Point>();
            SourceL.SetXYMapping(p => p);

            SourceA = new ObservableDataSource<Point>();
            SourceA.SetXYMapping(p => p);

            SourceB = new ObservableDataSource<Point>();
            SourceB.SetXYMapping(p => p);

            SourceC = new ObservableDataSource<Point>();
            SourceC.SetXYMapping(p => p);

            SourceH = new ObservableDataSource<Point>();
            SourceH.SetXYMapping(p => p);

            SourceT = new ObservableDataSource<Point>();
            SourceT.SetXYMapping(p => p);

            PreviewDropCommand = new RelayCommand(param => this.HandlePreviewDrop(param));

        }

        public void AppendValues(double L, double a, double b, double C, double H, double T)
        {
            DateTimeAxis axis = new DateTimeAxis();
            double time = axis.ConvertToDouble(DateTime.Now);
            Point p1 = new Point(time, L);
            Point p2 = new Point(time, a);
            Point p3 = new Point(time, b);
            Point p4 = new Point(time, C);
            Point p5 = new Point(time, H);
            Point p6 = new Point(time, T);

            SourceL.AppendAsync(App.Current.Dispatcher, p1);
            SourceA.AppendAsync(App.Current.Dispatcher, p2);
            SourceB.AppendAsync(App.Current.Dispatcher, p3);
            SourceC.AppendAsync(App.Current.Dispatcher, p4);
            SourceH.AppendAsync(App.Current.Dispatcher, p5);
            SourceT.AppendAsync(App.Current.Dispatcher, p6);

            Thread.Sleep(10);
        }


        private void HandlePreviewDrop(object inObject)
        {
            IDataObject ido = inObject as IDataObject;
            if (null == ido) return;

            // Get all the possible format
            //string[] formats = ido.GetFormats();

            // Do what you need here based on the format passed in.
            // You will probably have a few options and you need to
            // decide an order of preference.

            try
            {
                string[] droppedFilenames = ido.GetData(DataFormats.FileDrop) as string[];
                
            }
            catch
            {
            }
        }
    }

    public class LineGraphHelper
    {
        public static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.RegisterAttached("Description",
            typeof(string), typeof(LineGraphHelper),
            new FrameworkPropertyMetadata("", new PropertyChangedCallback(DescriptionPropertyChanged)));

        private static void DescriptionPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            LineGraph lineGraphControl = obj as LineGraph;

            if (lineGraphControl != null)
            {
                lineGraphControl.Description = e.NewValue != null ? new PenDescription(e.NewValue.ToString()) :
                                                                new PenDescription("Sample");
            }
        }

        public static void SetDescription(UIElement element, string value)
        {
            element.SetValue(DescriptionProperty, value);
        }

        public static string GetDescription(UIElement element)
        {
            return (string)element.GetValue(DescriptionProperty);
        }
    }

    /// <summary>
    /// This is an Attached Behavior and is intended for use with
    /// XAML objects to enable binding a drag and drop event to
    /// an ICommand.
    /// </summary>
    public static class DropBehavior
    {
        #region The dependecy Property
        /// <summary>
        /// The Dependency property. To allow for Binding, a dependency
        /// property must be used.
        /// </summary>
        private static readonly DependencyProperty PreviewDropCommandProperty =
                    DependencyProperty.RegisterAttached
                    (
                        "PreviewDropCommand",
                        typeof(ICommand),
                        typeof(DropBehavior),
                        new PropertyMetadata(PreviewDropCommandPropertyChangedCallBack)
                    );
        #endregion

        #region The getter and setter
        /// <summary>
        /// The setter. This sets the value of the PreviewDropCommandProperty
        /// Dependency Property. It is expected that you use this only in XAML
        ///
        /// This appears in XAML with the "Set" stripped off.
        /// XAML usage:
        ///
        /// <Grid mvvm:DropBehavior.PreviewDropCommand="{Binding DropCommand}" />
        ///
        /// </summary>
        /// <param name="inUIElement">A UIElement object. In XAML this is automatically passed
        /// in, so you don't have to enter anything in XAML.</param>
        /// <param name="inCommand">An object that implements ICommand.</param>
        public static void SetPreviewDropCommand(this UIElement inUIElement, ICommand inCommand)
        {
            inUIElement.SetValue(PreviewDropCommandProperty, inCommand);
        }

        /// <summary>
        /// Gets the PreviewDropCommand assigned to the PreviewDropCommandProperty
        /// DependencyProperty. As this is only needed by this class, it is private.
        /// </summary>
        /// <param name="inUIElement">A UIElement object.</param>
        /// <returns>An object that implements ICommand.</returns>
        private static ICommand GetPreviewDropCommand(UIElement inUIElement)
        {
            return (ICommand)inUIElement.GetValue(PreviewDropCommandProperty);
        }
        #endregion

        #region The PropertyChangedCallBack method
        /// <summary>
        /// The OnCommandChanged method. This event handles the initial binding and future
        /// binding changes to the bound ICommand
        /// </summary>
        /// <param name="inDependencyObject">A DependencyObject</param>
        /// <param name="inEventArgs">A DependencyPropertyChangedEventArgs object.</param>
        private static void PreviewDropCommandPropertyChangedCallBack(
            DependencyObject inDependencyObject, DependencyPropertyChangedEventArgs inEventArgs)
        {
            UIElement uiElement = inDependencyObject as UIElement;
            if (null == uiElement) return;

            uiElement.Drop += (sender, args) =>
            {
                GetPreviewDropCommand(uiElement).Execute(args.Data);
                args.Handled = true;
            };
        }
        #endregion
    }
}
