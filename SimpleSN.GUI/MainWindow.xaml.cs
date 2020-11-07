using Reactive.Bindings.Extensions;
using Reactive.Bindings.ObjectExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace SimpleSN.GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow(MainWindowViewModel viewModel)
        {
            DataContext = viewModel;
            InitializeComponent();
            this.Loaded += MainWindow_Initialized;

        }

        private void MainWindow_Initialized(object sender, EventArgs e)
        {
            (DataContext as MainWindowViewModel).GenerationCount
                //.SubscribeOnUIDispatcher()
                .Buffer(TimeSpan.FromMilliseconds(10))
                .Where(d=> d.Count > 0)
                .Select(d => d.Last())
                .Subscribe((value) =>
            {
                Dispatcher.Invoke(() =>
                Slider.TickFrequency = value / 10);
            });
        }
    }
}
