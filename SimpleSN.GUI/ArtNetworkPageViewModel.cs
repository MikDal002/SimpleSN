using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using SimpleSN.Core;
using Syncfusion.Windows.PropertyGrid;
using Syncfusion.Windows.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using System.Windows.Data;
using System.Windows.Forms;

namespace SimpleSN.GUI
{
    public class ArtNetworkSettings : ViewModelBase
    {
        [DisplayName("Katalog z danymi uczącymi")]
        [Description("Katalog w którym znajdują się obrazy testowe")]
        public ReactivePropertySlim<string> DirectoryWithLearningFiles { get; }

        [DisplayName("Liczba cech do wyróżnienia")]
        [Description("Ilość neuronów wyjściowych – mówi o tym ile różnych kategorii chcemy rozróżniać")]
        public ReactivePropertySlim<int> RequestedFeatures { get; }

        public ReactivePropertySlim<double> SimilarityThreshold { get; }

        public ReactiveCommand<object> SelectDirForLearningData { get; }

        public ReactivePropertySlim<bool> TryDetermineSizeFromFile { get; }
        public ReactivePropertySlim<int> ImageWidth { get; }
        public ReactivePropertySlim<int> ImageHeight { get; }

        public ReadOnlyReactiveProperty<Size> ImageSize { get; }

        public ArtNetworkSettings()
        {
            ImageWidth = new ReactivePropertySlim<int>().AddTo(Disposables);
            ImageHeight = new ReactivePropertySlim<int>().AddTo(Disposables);
            
            ImageSize = Observable.Concat(ImageHeight, ImageWidth).Select(_ => new Size(ImageWidth.Value, ImageHeight.Value)).ToReadOnlyReactiveProperty(new Size(1,1));
            DirectoryWithLearningFiles = new ReactivePropertySlim<string>("Portable Binary Bitmaps/").AddTo(Disposables);
            RequestedFeatures = new ReactivePropertySlim<int>(3).AddTo(Disposables);
            SimilarityThreshold = new ReactivePropertySlim<double>(0.7).AddTo(Disposables);
            TryDetermineSizeFromFile = new ReactivePropertySlim<bool>(true).AddTo(Disposables);
            SelectDirForLearningData = new ReactiveCommand<object>().WithSubscribe(d =>
            {
                using System.Windows.Forms.FolderBrowserDialog dlg = new FolderBrowserDialog();
                var result = dlg.ShowDialog();
                if (result == DialogResult.OK)
                    DirectoryWithLearningFiles.Value = dlg.SelectedPath;
            });
        }
    }

    public class ArtNetworkPageViewModel : ViewModelBase
    {
        public ArtNetworkSettings Settings { get; } = new ArtNetworkSettings();

        public ReactiveCollection<BitArray2D> InputFiles { get; } 
        public ReactiveCollection<BitArray2D> VisibleMemoryMaps { get; }
        public ReactiveCollection<List<BitArray2D>> AllMemoryMaps { get; }
        public ReactivePropertySlim<int> VisibleGeneration { get; }
       
        public ReactiveCommand<object> Recalculate { get; }

        public ArtNetworkPageViewModel()
        {
            Recalculate = new ReactiveCommand<object>().AddTo(Disposables).WithSubscribe(_ =>
            {
                MakeCalculations();
            });

            InputFiles = new ReactiveCollection<BitArray2D>().AddTo(Disposables);
            VisibleMemoryMaps = new ReactiveCollection<BitArray2D>().AddTo(Disposables);
            AllMemoryMaps = new ReactiveCollection<List<BitArray2D>>().AddTo(Disposables);
            AllMemoryMaps.ObserveAddChanged().Subscribe(d =>
            {
                VisibleGeneration.Value = AllMemoryMaps.Count;
            });
            VisibleGeneration = new ReactivePropertySlim<int>().AddTo(Disposables);
            
            VisibleGeneration.Where(d => d >= 0 && d < AllMemoryMaps.Count).Subscribe(d =>
            {
                VisibleMemoryMaps.Clear();
                VisibleMemoryMaps.AddRangeOnScheduler(AllMemoryMaps[d]);
            }).AddTo(Disposables);
            Settings.DirectoryWithLearningFiles.Subscribe(d =>
            {
                LoadAllFiles(d);
            }).AddTo(Disposables);
        }

        private void LoadAllFiles(string dirPath)
        {
            var directory = new DirectoryInfo(dirPath);
            if (!directory.Exists) throw new ArgumentException(nameof(dirPath), $"Directory {dirPath} doesn't exist!");

            InputFiles.Clear();
            bool isSizeToSet = Settings.TryDetermineSizeFromFile.Value;
            foreach(var fileName in directory.GetFiles("*.pbm"))
            {
                var image = new BitArray2D(fileName.FullName);
                if (isSizeToSet)
                {
                    Settings.ImageWidth.Value = image.Size.Width;
                    Settings.ImageHeight.Value = image.Size.Height;
                    isSizeToSet = false;
                }
                InputFiles.Add(image);
            }
        }

        private void MakeCalculations()
        {
            var requestedFeatures = Settings.RequestedFeatures.Value;
            var ro = Settings.SimilarityThreshold.Value;

            var sizeOfImage = Settings.ImageSize.Value;
            // Tworzę dwie warstwy neuronów
            var neuronyCzujności = NeuronFactory.GenerateNeurons(sizeOfImage.GetArea(), requestedFeatures,
                minValueOfWeight: 1, maxValueOfWeights: 1,
                learningImpact: 0.1).ToList();


            var wagaPoczątkowaNeuronówWyjściowych = 1.0 / (1.0 + sizeOfImage.GetArea());
            var neuronyWyjścia = NeuronFactory.GenerateNeurons(requestedFeatures, neuronyCzujności.Count, learningImpact: 0.1,
                minValueOfWeight: wagaPoczątkowaNeuronówWyjściowych, maxValueOfWeights: wagaPoczątkowaNeuronówWyjściowych,
                fitnessFunction: (pair) => pair.Weight * pair.VectorEl).ToList();
            // Dodaję mapy przed rozpoczęciem prac


            AllMemoryMaps.Clear();
            AllMemoryMaps.AddRangeOnScheduler(
                Enumerable.Range(0, requestedFeatures)
                          .Select(cecha => new BitArray2D(neuronyCzujności.Select(d => d.Weights[cecha]).ToList(), sizeOfImage)).ToList());

            //var artTrainer = new TrainArtNetwork();
            //artTrainer.TrainingStarted += (s, e) =>
            //{
            //
            //};
            //artTrainer.Train(neuronyWyjścia, InputFiles.Select(d => d.GetVector().Select(d => Convert.ToDouble(d)).ToList()));

            foreach (var image in InputFiles)
            {
                // Określa rozmiar obrazu
                if (image.Size != sizeOfImage) throw new ArgumentOutOfRangeException($"Image {image.Name} has different size than requested ({image.Size} vs {sizeOfImage})!");
                System.Diagnostics.Debug.WriteLine(image.ToString());

                // Określa ilość cech które chcemy wyłuskać z obrazu (ilość neuronów na pozoiomie wyjścia)
                neuronyWyjścia.ForEach(n => n.FitnessForVector(image.GetVector().Select(d => Convert.ToDouble(d))));
                Neuron tenWłaściwy = null;
                int indexTegoWłaściwego = -1;
                List<double> krótkotrwałaPamięć = null;

                // Tutaj odbywa się wybór najlepsze
                for (int i = 0; i < neuronyWyjścia.Count; i++)
                {
                    var najlepszyNeuronWyjścia = neuronyWyjścia.Max();
                    var index = neuronyWyjścia.IndexOf(najlepszyNeuronWyjścia);

                    krótkotrwałaPamięć = neuronyCzujności.Select(d => d.Weights[index]).ToList();
                    var dopasowanieKrótkotrwałejPamięci_A = krótkotrwałaPamięć.MultiplyEachElementWith(image.GetVector().Select(d => Convert.ToDouble(d)).ToList()).Sum();
                    var dopasowanieKrótkotrwałejPamięci_B = image.GetVector().Select(d => Convert.ToDouble(d)).Sum();
                    var dopasowanieKrótkotrwałejPamięci = dopasowanieKrótkotrwałejPamięci_A / dopasowanieKrótkotrwałejPamięci_B;
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

                if (tenWłaściwy == null)
                {
                    Debug.WriteLine("None neuron could be fitted!");
                    continue;

                    //   throw new InvalidOperationException("None neuron could be fitted!");
                }

                // dopasowanie krótkotrwałej pamięci
                int nr = 0;
                neuronyCzujności.ForEach(d =>
                {
                    var nowaWaga = Convert.ToDouble(image.Get(nr++)) * d.Weights[indexTegoWłaściwego];
                    //if (nowaWaga < 0.5) Debug.WriteLine("Foo");
                    d.Weights[indexTegoWłaściwego] = nowaWaga;
                });
                krótkotrwałaPamięć = neuronyCzujności.Select(d => d.Weights[indexTegoWłaściwego]).ToList();

                // !! Tutaj odbywa się właściwy trening nowych wag!
                for (int i = 0; i < tenWłaściwy.Weights.Count; ++i)
                {
                    var value = tenWłaściwy.Weights[i];
                    tenWłaściwy.Weights[i] =
                        krótkotrwałaPamięć[i] /
                        (0.5 + krótkotrwałaPamięć.MultiplyEachElementWith(
                                                    image.GetVector()
                                                    .Select(d => Convert.ToDouble(d))
                                                    .ToList())
                                                .Sum());
                }
                var maps = new List<BitArray2D>();
                for (int cecha = 0; cecha < requestedFeatures; ++cecha)
                {
                    var mapaCzujności = new BitArray2D(neuronyCzujności.Select(d => d.Weights[cecha]).ToList(), image.Size);
                    maps.Add(mapaCzujności);
                }
                AllMemoryMaps.AddRangeOnScheduler(maps);
            }
        }
    }
}
