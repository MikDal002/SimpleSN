using SimpleSN.Core;
using System;
using System.Collections;
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

    public sealed class BitArray2D
    {
        private BitArray _array;
        private int _dimension1;
        private int _dimension2;

        public BitArray2D(System.Drawing.Size size) : this(size.Width, size.Height)
        {

        }

        public BitArray2D(int dimension1, int dimension2)
        {
            _dimension1 = dimension1 > 0 ? dimension1 : throw new ArgumentOutOfRangeException(nameof(dimension1), dimension1, string.Empty);
            _dimension2 = dimension2 > 0 ? dimension2 : throw new ArgumentOutOfRangeException(nameof(dimension2), dimension2, string.Empty);
            _array = new BitArray(dimension1 * dimension2);
        }

        public bool Get(int x, int y) { CheckBounds(x, y); return _array[y * _dimension1 + x]; }
        public bool Set(int x, int y, bool val) { CheckBounds(x, y); return _array[y * _dimension1 + x] = val; }
        public bool this[int x, int y] { get { return Get(x, y); } set { Set(x, y, value); } }
        public IEnumerable<bool> GetVector()
        {
            foreach(object foo in _array)
            {
                yield return Convert.ToBoolean(foo);
            }
        }

        private void CheckBounds(int x, int y)
        {
            if (x < 0 || x >= _dimension1)
            {
                throw new IndexOutOfRangeException();
            }
            if (y < 0 || y >= _dimension2)
            {
                throw new IndexOutOfRangeException();
            }
        }
    }

    public class ArtNetworkPageViewModel
    {
        public ArtNetworkPageViewModel()
        {
            // Określa rozmiar obrazu
            var sizeOfImage = new System.Drawing.Size(2, 2);
            var image = new BitArray2D(sizeOfImage);
            image.Set(0, 0, true);
            image.Set(1, 0, true);
            image.Set(0, 1, true);
            // Określa ilość cech które chcemy wyłuskać z obrazu (ilość neuronów na pozoiomie wyjścia)
            var requestedFeatures = 40;

            // Tworzę dwie warstwy neuronów
            var neuronyCzujności = NeuronFactory.GenerateNeurons(sizeOfImage.GetArea(), 1, 
                minValueOfWeight: 1, maxValueOfWeights: 1,
                learningImpact: 0.1).ToList();

            var wagaPoczątkowaNeuronówWyjściowych = 1.0 / (1.0 + sizeOfImage.GetArea());
            var neuronyWyjścia = NeuronFactory.GenerateNeurons(requestedFeatures, neuronyCzujności.Count, learningImpact: 0.1, 
                minValueOfWeight: wagaPoczątkowaNeuronówWyjściowych, maxValueOfWeights: wagaPoczątkowaNeuronówWyjściowych,
                fitnessFunction: (pair) => pair.Weight * pair.VectorEl).ToList();

            neuronyWyjścia.ForEach(n => n.FitnessForVector(image.GetVector().Select(d => Convert.ToDouble(d))));
            var najlepszyNeuronWyjścia = neuronyWyjścia.Max();
            var index = neuronyWyjścia.IndexOf(najlepszyNeuronWyjścia);

            var krótkotrwałaPamięć = neuronyCzujności.Select(d => d.Weights.ElementAt(index)).ToList();
            var dopasowanieKrótkotrwałejPamięci_A = krótkotrwałaPamięć.MultiplyEachElementWith(image.GetVector().Select(d => Convert.ToDouble(d)).ToList()).Sum();
            var dopasowanieKrótkotrwałejPamięci_B = image.GetVector().Select(d => Convert.ToDouble(d)).Sum();
            var dopasowanieKrótkotrwałejPamięci = dopasowanieKrótkotrwałejPamięci_A / dopasowanieKrótkotrwałejPamięci_B;
            var ro = 0.5;
            var jestDopasowane = dopasowanieKrótkotrwałejPamięci > ro;


            // To jest wektor wyjściowy dolnej warstwy
            var y_d = neuronyCzujności.Select(d => d.LastFitness).ToList().TheBiggestValueThreatAsOneOthersAsZeros();
            // to jest wektor wyjściowy górnej warstwy 
            var y_g = neuronyWyjścia.Select(d => d.LastFitness).ToList().TheBiggestValueThreatAsOneOthersAsZeros();

        }
    }
}
