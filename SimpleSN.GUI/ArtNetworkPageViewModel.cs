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
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Threading;
using SimpleSN.Core.OtherStuff;

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
        public ReactivePropertySlim<bool> LoadFromSubdirs { get; }
        public ReactivePropertySlim<int> ImageWidth { get; }
        public ReactivePropertySlim<int> ImageHeight { get; }

        public ReadOnlyReactiveProperty<Size> ImageSize { get; }

        public ArtNetworkSettings()
        {
            ImageWidth = new ReactivePropertySlim<int>().AddTo(Disposables);
            ImageHeight = new ReactivePropertySlim<int>().AddTo(Disposables);

            ImageSize = Observable.Concat(ImageHeight, ImageWidth).Select(_ => new Size(ImageWidth.Value, ImageHeight.Value)).ToReadOnlyReactiveProperty(new Size(1, 1));
            DirectoryWithLearningFiles = new ReactivePropertySlim<string>("Portable Binary Bitmaps/").AddTo(Disposables);
            RequestedFeatures = new ReactivePropertySlim<int>(3).AddTo(Disposables);
            SimilarityThreshold = new ReactivePropertySlim<double>(0.7).AddTo(Disposables);
            TryDetermineSizeFromFile = new ReactivePropertySlim<bool>(true).AddTo(Disposables);
            LoadFromSubdirs = new ReactivePropertySlim<bool>(false).AddTo(Disposables);
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
                VisibleGeneration.Value = 0;
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
                if (AllMemoryMaps.Count == 0) return;
                VisibleMemoryMaps.Clear();
                VisibleMemoryMaps.AddRangeOnScheduler(AllMemoryMaps[d]);
            }).AddTo(Disposables);
            Settings.DirectoryWithLearningFiles.Subscribe(d =>
            {
                InputFiles.ClearOnScheduler();
                var disp = Dispatcher.CurrentDispatcher;
                Task.Run(() => LoadAllFiles(d, (array) => disp.Invoke(() => InputFiles.AddOnScheduler(array))).ToList());
               

            }).AddTo(Disposables);
        }

        private IEnumerable<BitArray2D> LoadAllFiles(string dirPath, Action<BitArray2D> adder)
        {
            if (string.IsNullOrEmpty(dirPath)) throw new ArgumentException($"'{nameof(dirPath)}' cannot be null or empty", nameof(dirPath));
            if (adder is null) throw new ArgumentNullException(nameof(adder));

            var directory = new DirectoryInfo(dirPath);
            if (!directory.Exists) throw new ArgumentException(nameof(dirPath), $"Directory {dirPath} doesn't exist!");

            bool isSizeToSet = Settings.TryDetermineSizeFromFile.Value;
            var options = new EnumerationOptions()
            {
                RecurseSubdirectories = Settings.LoadFromSubdirs.Value
                };
            foreach (var fileName in directory.GetFiles("*.*", options))
            {
                var image = new BitArray2D(fileName.FullName);
                if (isSizeToSet)
                {
                    Settings.ImageWidth.Value = image.Size.Width;
                    Settings.ImageHeight.Value = image.Size.Height;
                    isSizeToSet = false;
                }
                if (image.Size != Settings.ImageSize.Value)
                {
                    var result = MessageBox.Show($"Obraz {fileName} ma inny wymiar ({image.Size}) niż żadany ({Settings.ImageSize.Value}). Wybierz \"Ponów\" aby spróbować załadować ponownie (to nie działa)" +
                        $"\"Ignoruj\" aby pominąć lub \"Anuluj\" aby przerwać ładowanie.", "Niepoprawny rozmiar obrazu!",MessageBoxButtons.AbortRetryIgnore);
                    if (result == DialogResult.Abort)
                    {
                        InputFiles.Clear();
                        yield break;
                    } else if (result == DialogResult.Ignore)
                    {
                        continue;
                    } else if (result == DialogResult.Retry)
                    {
                        continue;   
                    }
                }
                adder(image);
                yield return image;

            }
        }

        private void MakeCalculations()
        {
            var sizeOfImage = Settings.ImageSize.Value;

            var requestedFeatures = Settings.RequestedFeatures.Value;
            
            var wagaPoczątkowaNeuronówWyjściowych = 1.0 / (1.0 + sizeOfImage.GetArea());
            var neuronyWyjścia = NeuronFactory.GenerateNeurons(requestedFeatures, sizeOfImage.GetArea(), learningImpact: 0.1,
                minValueOfWeight: wagaPoczątkowaNeuronówWyjściowych, maxValueOfWeights: wagaPoczątkowaNeuronówWyjściowych,
                fitnessFunction: (pair) => pair.Weight * pair.VectorEl).ToList();

            var artTrainer = new TrainArtNetwork();
            artTrainer.RequiredSimilarity = Settings.SimilarityThreshold.Value;
            artTrainer.TrainingStarted += (s, e) =>
            {
                AllMemoryMaps.Clear();
                AllMemoryMaps.AddRangeOnScheduler(
                Enumerable.Range(0, requestedFeatures)
                          .Select(cecha => new BitArray2D((e as TrainArtNetwork).MemoryMaps[cecha].Map, sizeOfImage)).ToList());

            };
            artTrainer.IterationFinished += (s, e) =>
            {
                var maps = new List<BitArray2D>();
                foreach(var map in (s as TrainArtNetwork).MemoryMaps)
                {
                    maps.Add(new BitArray2D(map.Map, sizeOfImage));
                }
                AllMemoryMaps.AddRangeOnScheduler(maps);
            };
            var task = Task.Run(() => 
            artTrainer.Train(neuronyWyjścia, InputFiles.Select(d => d.GetVector().Select(d => Convert.ToDouble(d)).ToList())));
            //task.Wait();
        }
    }
}
