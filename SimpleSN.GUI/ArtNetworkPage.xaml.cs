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
    /// Interaction logic for ArtNetworkPage.xaml
    /// </summary>
    public partial class ArtNetworkPage : Page
    {
        public ArtNetworkPage()
        {
            InitializeComponent();
            DataContext = new ArtNetworkPageViewModel();
        }
    }
}
