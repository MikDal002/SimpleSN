using SimpleSN.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

        public Size Size => new Size(_dimension1, _dimension2);

        public BitArray2D(System.Drawing.Size size) : this(size.Width, size.Height)
        {

        }

        public BitArray2D(int dimension1, int dimension2)
        {
            _dimension1 = dimension1 > 0 ? dimension1 : throw new ArgumentOutOfRangeException(nameof(dimension1), dimension1, string.Empty);
            _dimension2 = dimension2 > 0 ? dimension2 : throw new ArgumentOutOfRangeException(nameof(dimension2), dimension2, string.Empty);
            _array = new BitArray(dimension1 * dimension2);
        }

        public BitArray2D(IEnumerable<double> vector, Size size) : this(size.Width, size.Height)
        {
            int pixel = 0;
            foreach (var el in vector)
            {
                Set(pixel % size.Width, pixel / size.Height, el > 0);
                ++pixel;
            }
        }

        public BitArray2D(string pbmFilename)
        {
            Regex findWhiteSpaces = new Regex(@"\s");
            var rawLines = System.IO.File.ReadAllLines(pbmFilename);
            var wasTypeReaded = false;
            var size = System.Drawing.Size.Empty;
            int currentPoint = 0;
            foreach (var line in rawLines.Select(d => d.Trim()))
            {
                if (line.StartsWith('#')) continue;
                if (line.StartsWith("P", StringComparison.OrdinalIgnoreCase))
                {
                    var number = Convert.ToInt32(line[1].ToString());
                    if (number != 1) throw new InvalidOperationException($"The format of bitmap should be P1 but is P{number}!");
                    wasTypeReaded = true;
                    continue;
                }
                if (Char.IsDigit(line[0]) && size == Size.Empty)
                {
                    string[] sizeAsStrings = findWhiteSpaces.Split(line);
                    if (sizeAsStrings.Length != 2) throw new InvalidOperationException($"Something else than two nubmers is in size line! The line: {line}.");
                    size = new Size(Convert.ToInt32(sizeAsStrings[0]), Convert.ToInt32(sizeAsStrings[1]));
                    _dimension1 = size.Width;
                    _dimension2 = size.Height;
                    _array = new BitArray(size.GetArea());
                }
                else if (Char.IsDigit(line[0]))
                {
                    var bits = findWhiteSpaces.Split(line);
                    foreach (var bit in bits)
                    {
                        if (currentPoint > size.GetArea()) throw new ArgumentOutOfRangeException($"Size specified in file is smaller than data in file!");
                        _array.Set(currentPoint++, Convert.ToInt32(bit) == 1 ? true : false);
                    }
                }
                else if (Char.IsLetter(line[0]))
                {
                    throw new NotImplementedException($"The data line started with letter. I don't know what to do! Line: {line}.");
                }
            }
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            for (int x = 0; x < _dimension1; x++)
            {
                for (int y = 0; y < _dimension2; y++)
                {
                    builder.Append(_array.Get((x * _dimension1) + y) ? 'X' : ' ');
                }
                builder.AppendLine();
            }
            return builder.ToString();
        }

        public bool Get(int w) => _array.Get(w);
        public bool Get(int x, int y) { CheckBounds(x, y); return _array[y * _dimension1 + x]; }
        public bool Set(int x, int y, bool val) { CheckBounds(x, y); return _array[y * _dimension1 + x] = val; }
        public bool this[int x, int y] { get { return Get(x, y); } set { Set(x, y, value); } }
        public IEnumerable<bool> GetVector()
        {
            foreach (object foo in _array)
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
            var requestedFeatures = 3;

                var sizeOfImage = new Size(5, 5);
            // Tworzę dwie warstwy neuronów
            var neuronyCzujności = NeuronFactory.GenerateNeurons(sizeOfImage.GetArea(), requestedFeatures,
                minValueOfWeight: 1, maxValueOfWeights: 1,
                learningImpact: 0.1).ToList();

            var wagaPoczątkowaNeuronówWyjściowych = 1.0 / (1.0 + sizeOfImage.GetArea());
            var neuronyWyjścia = NeuronFactory.GenerateNeurons(requestedFeatures, neuronyCzujności.Count, learningImpact: 0.1,
                minValueOfWeight: wagaPoczątkowaNeuronówWyjściowych, maxValueOfWeights: wagaPoczątkowaNeuronówWyjściowych,
                fitnessFunction: (pair) => pair.Weight * pair.VectorEl).ToList();
            var fileNames = new[] { "pierwszy.pbm", "drugi.pbm", "trzeci.pbm", "czwarty.pbm" };
            foreach (var imageName in fileNames)
            {
                Debug.WriteLine($"#### New image: {imageName,12} ####");
                // Określa rozmiar obrazu
                var image = new BitArray2D($"Portable Binary Bitmaps/{imageName}");
                if (image.Size != sizeOfImage) throw new ArgumentOutOfRangeException($"Image {imageName} has different size than requested ({image.Size} vs {sizeOfImage})!");
                System.Diagnostics.Debug.WriteLine(image.ToString());

                // Określa ilość cech które chcemy wyłuskać z obrazu (ilość neuronów na pozoiomie wyjścia)


                neuronyWyjścia.ForEach(n => n.FitnessForVector(image.GetVector().Select(d => Convert.ToDouble(d))));
                Neuron tenWłaściwy = null;
                int indexTegoWłaściwego = -1;
                List<double> krótkotrwałaPamięć = null;
                for (int i = 0; i < neuronyWyjścia.Count; i++)
                {
                    var najlepszyNeuronWyjścia = neuronyWyjścia.Max();
                    var index = neuronyWyjścia.IndexOf(najlepszyNeuronWyjścia);

                    krótkotrwałaPamięć = neuronyCzujności.Select(d => d.Weights[index]).ToList();
                    var dopasowanieKrótkotrwałejPamięci_A = krótkotrwałaPamięć.MultiplyEachElementWith(image.GetVector().Select(d => Convert.ToDouble(d)).ToList()).Sum();
                    var dopasowanieKrótkotrwałejPamięci_B = image.GetVector().Select(d => Convert.ToDouble(d)).Sum();
                    var dopasowanieKrótkotrwałejPamięci = dopasowanieKrótkotrwałejPamięci_A / dopasowanieKrótkotrwałejPamięci_B;
                    var ro = 0.7;
                    var jestDopasowane = dopasowanieKrótkotrwałejPamięci > ro;
                    if (jestDopasowane)
                    {
                        // pomyślnie
                        tenWłaściwy = najlepszyNeuronWyjścia;
                        indexTegoWłaściwego = index;
                        break;
                    }
                    else
                    {
                        // nie pomyślnie – wyzeruj 
                        najlepszyNeuronWyjścia.LastFitness = 0;
                    }
                }

                if (tenWłaściwy == null) throw new InvalidOperationException("None neuron could be fitted!");

                // dopasowanie krótkotrwałej pamięci
                int nr = 0;
                neuronyCzujności.ForEach(d =>
                {
                    var nowaWaga = Convert.ToDouble(image.Get(nr++)) * d.Weights[indexTegoWłaściwego];
                    //if (nowaWaga < 0.5) Debug.WriteLine("Foo");
                    d.Weights[indexTegoWłaściwego] = nowaWaga;
                });
                krótkotrwałaPamięć = neuronyCzujności.Select(d => d.Weights[indexTegoWłaściwego]).ToList();
                for (int i = 0; i < tenWłaściwy.Weights.Count; ++i)
                {
                    var value = tenWłaściwy.Weights[i];
                    tenWłaściwy.Weights[i] = krótkotrwałaPamięć[i] / (0.5 + krótkotrwałaPamięć.MultiplyEachElementWith(image.GetVector().Select(d => Convert.ToDouble(d)).ToList()).Sum());
                }
                var mapy = new List<string>();
                for (int cecha = 0; cecha < requestedFeatures; ++cecha)
                {
                    var mapaCzujności = new BitArray2D(neuronyCzujności.Select(d => d.Weights[cecha]).ToList(), image.Size);
                    mapy.Add(mapaCzujności.ToString());
                }

                for (int linia = 0; linia < image.Size.Height; ++linia)
                {
                    foreach (var ln in mapy)
                    {
                        Debug.Write(ln.Split("\r\n")[linia] + " \t");
                    }
                    Debug.WriteLine("");
                }



            }
        }
    }
}
