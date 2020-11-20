using SimpleSN.Core;
using System;
using System.Collections.Generic;
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

    public class ArtNetworkPageViewModel
    {
        public ArtNetworkPageViewModel()
        {
            // Określa rozmiar obrazu, będzie rzutowana do wartości całkowitych
            var sizeOfImage = new Size(20, 20);
            // Określa ilość cech które chcemy wyłuskać z obrazu (ilość neuronów na pozoiomie wyjścia
            var requestedFeatures = 40;
            // Ilośc neuronów na poziomie wejścia
            var numberOfInputNeurons = 80;
            var inputNeurons = new List<Neuron>();
            for(int no = 0; no < numberOfInputNeurons; ++no)
            {
                int size = (int)(sizeOfImage.Height * sizeOfImage.Width);
                var weightsOfNeuron = new double[size];
                var rnd = new Random();
                for(int i = 0; i < size; i++)
                {
                    weightsOfNeuron[i] = rnd.NextDouble();
                }
                inputNeurons.Add(new Neuron(weightsOfNeuron, 0.1));
            }
        }
    }
}
