using SimpleSN.Core;
using System;
using System.Collections.Generic;
using System.Linq;
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
            
            // Tworzę dwie warstwy neuronów
            
            var inputNeurons = NeuronFactory.GenerateNeurons(numberOfInputNeurons, (int)(sizeOfImage.Height * sizeOfImage.Width), 
                minValueOfWeight: 1, maxValueOfWeights: 1,
                learningImpact: 0.1).ToList();
            var outNeurons = NeuronFactory.GenerateNeurons(requestedFeatures, inputNeurons.Count, learningImpact: 0.1, 
                fitnessFunction: (pair) => pair.Weight * pair.VectorEl);


        }
    }
}
