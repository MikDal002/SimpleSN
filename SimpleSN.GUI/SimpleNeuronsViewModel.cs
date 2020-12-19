﻿using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Reactive.Bindings.Notifiers;
using SimpleSN.Core;
using Syncfusion.Windows.Tools.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Threading;

namespace SimpleSN.GUI
{
    public struct NeuronDto
    {
        public List<double> Weights { get; set; }
        public string Name { get; set; }
        public int NumberOfWins { get; set; }

        public PointF ToPointF()
        {
            if (Weights.Count != 2) throw new InvalidOperationException();
            return new PointF((float)Weights[0], (float)Weights[1]);
        }

        public static explicit operator NeuronDto(Neuron neuron)
        {
            var newDto = new NeuronDto
            {
                Weights = new List<double>(),
                Name = neuron.Name,
                NumberOfWins = neuron.Age,
            };
            foreach (var wght in neuron.Weights) newDto.Weights.Add(wght);
            return newDto;
        }
    }
    public class Generation
    {
        public List<NeuronDto> Neurons { get; }
        public NeuronDto? Winner { get; }
        public int GenerationNumber { get; }


        public Generation(IEnumerable<NeuronDto> neurons, int generationNumber, NeuronDto? winner)
        {
            Neurons = neurons as List<NeuronDto> ?? neurons.ToList();
            GenerationNumber = generationNumber;
            Winner = winner;
        }
    }
    public abstract class ViewModelBase : INotifyPropertyChanged, IDisposable
    {
        protected CompositeDisposable Disposables { get; } = new CompositeDisposable();

        public void Dispose() => Disposables.Dispose();


        public void UpdateFieldAndNotify<T>(ref T fieldToUpdate, T value, [CallerMemberName] string callerName = "")
        {
            if (Equals(fieldToUpdate, value)) return;

            fieldToUpdate = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(callerName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    public class SimpleNeuronsViewModel : ViewModelBase
    {
        private int _visibleGeneration;
        private int tiredness;
        private int _repeatLearningDataAmount;
        private int _neuronAmount;
        private bool _generateNewNeurons = true;
        private double _learningImpact;
        private double _agingFactor;
        RelayCommand _regenarateTrainData;
        private List<Point> _dataPoints = new List<Point>();
        private readonly DataGenerator _generator;

        public List<Point> DataPoints { get => _dataPoints; set => UpdateFieldAndNotify(ref _dataPoints, value); }
        private readonly Trainer trainer;

        public ObservableCollection<PointF> VisibleNeurons { get; } = new ObservableCollection<PointF>();
        public ReactivePropertySlim<Generation> VisibleGenerationGeneration { get; }
        public List<Generation> Generations { get; } = new List<Generation>();
        public ReactivePropertySlim<int> GenerationCount { get; }
        public ReactivePropertySlim<int> DataGroupsCount { get; }
        public ReactivePropertySlim<int> MaxXPointValue { get; }
        public ReactivePropertySlim<int> MaxYPointValue { get; }
        public ReactivePropertySlim<int> GroupRadius { get; }
        public ReactivePropertySlim<int> AllPointsCount { get; }
        public BusyNotifier IsStillWorking { get; }
        public BooleanNotifier DontIntializeNeurons { get; }

        public int VisibleGeneration { get => _visibleGeneration; set => UpdateFieldAndNotify(ref _visibleGeneration, value); }
        public int Tiredness { get => tiredness; set => UpdateFieldAndNotify(ref tiredness, value); }
        public int RepeatLearningDataAmount { get => _repeatLearningDataAmount; set => UpdateFieldAndNotify(ref _repeatLearningDataAmount, value); }
        public int NeuronAmount { get => _neuronAmount; set => UpdateFieldAndNotify(ref _neuronAmount, value); }
        public bool GenerateNewNeurons { get => _generateNewNeurons; set => UpdateFieldAndNotify(ref _generateNewNeurons, value); }
        public double LearningImpact { get => _learningImpact; set => UpdateFieldAndNotify(ref _learningImpact, value); }
        public double AgingFactor { get => _agingFactor; set => UpdateFieldAndNotify(ref _agingFactor, value); }

        // To use this class within your viewmodel class:
        public ReactiveCommand Start { get; }
        public ReactiveCommand SaveGeneratedData { get; }
        public ICommand RegenarateTrainData
        {
            get
            {
                if (_regenarateTrainData == null)
                {
                    _regenarateTrainData = new RelayCommand(_ => this.GenerateTrainSet(),
                        _ => true);
                }
                return _regenarateTrainData;
            }
        }

        public SimpleNeuronsViewModel()
        {
            _generator = new DataGenerator();

            VisibleGenerationGeneration = new ReactivePropertySlim<Generation>().AddTo(Disposables);
            DataGroupsCount = new ReactivePropertySlim<int>(10).AddTo(Disposables);
            GenerationCount = new ReactivePropertySlim<int>(0).AddTo(Disposables);
            MaxXPointValue = new ReactivePropertySlim<int>(1000).AddTo(Disposables);
            MaxYPointValue = new ReactivePropertySlim<int>(1000).AddTo(Disposables);
            GroupRadius = new ReactivePropertySlim<int>(50).AddTo(Disposables);
            AllPointsCount = new ReactivePropertySlim<int>(1000).AddTo(Disposables);
            IsStillWorking = new BusyNotifier();
            DontIntializeNeurons = new BooleanNotifier(false);
            SaveGeneratedData = new ReactiveCommand().WithSubscribe(() => {
                if (DataPoints.Count == 0) GenerateTrainSet();
                StringBuilder bldr = new StringBuilder();
                DataPoints.Aggregate(bldr, (l, r) => l.AppendLine($"{r.X};{r.Y}"));
                using System.Windows.Forms.SaveFileDialog dlg = new SaveFileDialog();
                dlg.FileName = "dataset.csv";
                var result = dlg.ShowDialog();
                if (result == DialogResult.OK)
                {
                    System.IO.File.WriteAllText(dlg.FileName, bldr.ToString());
                }
            }).AddTo(Disposables);
            Start = new ReactiveCommand(IsStillWorking.Select(d => !d))
                .WithSubscribe(async () =>
            {
                await StartLearining();
            }).AddTo(Disposables);

            trainer = new Trainer();
            //trainer.IteractionStarting += (sender, trainer) => Debug.WriteLine($"Iteration {trainer.Iteration} started…");
            trainer.IterationFinished += (sender, winner) =>
            {
                Generations.Add(new Generation(trainer.Neurons.Select(d => (NeuronDto)d), trainer.Iteration, winner != null ? (NeuronDto?)winner : null));
                GenerationCount.Value = trainer.Iteration;

                
                if (Generations.Count - 1 != trainer.Iteration)
                {
                    Debug.WriteLine($"Count won: {Generations.Count}");
                    Debug.WriteLine($"Iteration won: {trainer.Iteration}");
                }
            };
            trainer.TrainingFinished += (sender, trainer) =>
            {
                Debug.WriteLine($"Training finished in {trainer.Iteration} iterations");
                // Debug.WriteLine("All vectors after learning:");
                // trainer.Neurons.ForEach(d => Debug.WriteLine(d.ToString()));
            };

            this.PropertyChanged += MainWindowViewModel_PropertyChanged;
        }

        private void GenerateTrainSet()
        {
            DataPoints = _generator.Generate(DataGroupsCount.Value, GroupRadius.Value, AllPointsCount.Value, new Point(0, 0), new Point(MaxXPointValue.Value, MaxYPointValue.Value)).ToList();
        }

        private async Task StartLearining()
        {
            if (DataPoints.Count == 0) GenerateTrainSet();
            GenerationCount.Value = 0;
            VisibleGeneration = 0;

            using var _ = IsStillWorking.ProcessStart();
            await Task.Run(() =>
            {
                try
                {
                    List<Neuron> neurons = null;
                    if (GenerateNewNeurons || Generations.Count == 0)
                    {
                        neurons = NeuronFactory.GenerateNeurons(NeuronAmount, 2, learningImpact: LearningImpact, tiredness: Tiredness, minValueOfWeight: 0, maxValueOfWeights: DontIntializeNeurons.Value ? 0 : Math.Min(MaxXPointValue.Value, MaxYPointValue.Value), agingFactor: AgingFactor).ToList();
                    }
                    else
                    {
                        neurons = NeuronFactory.FromVectors(Generations.ElementAt(0).Neurons.Select(d => d.Weights), learningImpact: LearningImpact, tiredness: Tiredness, agingFactor: AgingFactor).ToList();
                    }

                    Generations.Clear();
                    Generations.Add(new Generation(neurons.Select(d => (NeuronDto)d), 0, null));

                    List<double[]> rawData = DataPoints.Select(d => new[] { (double)d.X, (double)d.Y }).ToList();
                    var toLearn = new List<double[]>();
                    foreach (var _ in Enumerable.Range(0, RepeatLearningDataAmount)) toLearn.AddRange(rawData);
                    Debug.WriteLine($"Amount: {toLearn.Count}");
                    trainer.Train(neurons, toLearn);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }
            });

        }

        private void MainWindowViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(VisibleGeneration))
            {
                VisibleNeurons.Clear();
                if (Generations.Count == 0) return;
                var generationToShow = Generations[VisibleGeneration];//.FirstOrDefault(d => d.GenerationNumber == VisibleGeneration);
                // if (generationToShow == null) return;
                foreach (var neuron in generationToShow.Neurons)
                    VisibleNeurons.Add(neuron.ToPointF());
                VisibleGenerationGeneration.Value = Generations[VisibleGeneration];
            }
        }
    }

    public class RelayCommand : ICommand
    {
        #region Fields

        readonly Action<object> _execute;
        readonly Predicate<object> _canExecute;

        #endregion // Fields

        #region Constructors

        public RelayCommand(Action<object> execute)
        : this(execute, null)
        {
        }

        public RelayCommand(Action<object> execute, Predicate<object> canExecute)
        {
            if (execute == null)
                throw new ArgumentNullException("execute");

            _execute = execute;
            _canExecute = canExecute;
        }
        #endregion // Constructors

        #region ICommand Members

        [DebuggerStepThrough]
        public bool CanExecute(object parameter)
        {
            return _canExecute == null ? true : _canExecute(parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter)
        {
            _execute(parameter);
        }

        #endregion // ICommand Members
    }
}