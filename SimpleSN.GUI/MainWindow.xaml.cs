using System;
using System.Collections.Generic;
using System.Linq;
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
            viewModel.PropertyChanged += ViewModel_PropertyChanged;
            InitializeComponent();

        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MainWindowViewModel.GenerationCount))
            {
                Dispatcher.Invoke(() =>
                {
                    Slider.TickFrequency = (DataContext as MainWindowViewModel).GenerationCount / 10;
                });
                
            }
            //throw new NotImplementedException();
        }
    }
}
