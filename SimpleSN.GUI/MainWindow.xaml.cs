using Reactive.Bindings;
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
    public class MainWindowViewModel : ViewModelBase
    {
        public MainWindowViewModel(IEnumerable<Page> pages)
        {
            foreach (var pg in pages)
            {
                System.Diagnostics.Debug.WriteLine(pg.Title);
                AvailableContents.Add(pg);
            }
            Content = new ReactiveProperty<Page>().AddTo(Disposables);
            Open = new ReactiveCommand<object>(Content.Select(d => d != null)).WithSubscribe(d =>
            {
                var wnd = new Window();
                wnd.Title = Content.Value.Title;
                wnd.Content = Content.Value;
                wnd.Show();
            }).AddTo(Disposables);
        }
        public ReactiveProperty<Page> Content { get; set; }
        public List<Page> AvailableContents { get; } = new List<Page>();
        public ReactiveCommand<object> Open { get; }
    }
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow(MainWindowViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
