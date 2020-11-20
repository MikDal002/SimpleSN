using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
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
    /// Logika interakcji dla klasy SimpleNeurons.xaml
    /// </summary>
    public partial class SimpleNeurons : Page
    {
        public SimpleNeurons(SimpleNeuronsViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            this.Initialized += SimpleNeurons_Initialized;
        }

        private void SimpleNeurons_Initialized(object sender, EventArgs e)
        {
            (DataContext as SimpleNeuronsViewModel).GenerationCount
                //.SubscribeOnUIDispatcher()
                .Buffer(TimeSpan.FromMilliseconds(10))
                .Where(d => d.Count > 0)
                .Select(d => d.Last())
                .Subscribe((value) =>
                {
                    Dispatcher.Invoke(() =>
                    Slider.TickFrequency = value / 10);
                });
        }
    }
}
