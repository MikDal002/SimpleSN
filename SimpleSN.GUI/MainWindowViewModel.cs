using SimpleSN.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SimpleSN.GUI
{
    public struct NeuronDto
    {
        public List<double> Weights { get; set; }
        public string Name { get; set; }

        public PointF ToPointF()
        {
            if (Weights.Count != 2) throw new InvalidOperationException();
            return new PointF((float)Weights[0], (float)Weights[1]);
        }

        public static implicit operator NeuronDto(Neuron neuron)
        {
            var newDto =  new NeuronDto
            {
                Weights = new List<double>(),
                Name = neuron.Name
            };
            foreach (var wght in neuron.Weights) newDto.Weights.Add(wght);
            return newDto;
        }
    }
    public class Generation
    {
        public List<NeuronDto> Neurons { get; }
        public int GenerationNumber { get; }


        public Generation(IEnumerable<NeuronDto> neurons, int generationNumber)
        {
            Neurons = neurons as List<NeuronDto> ?? neurons.ToList();
            GenerationNumber = generationNumber;
        }
    }

    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private int _generetionCount = 0;
        private int _visibleGeneration;
        private int tiredness;
        private int _repeatLearningDataAmount;
        private int _neuronAmount;
        private double _learningImpact;
        RelayCommand _start;
        RelayCommand _regenarateTrainData;
        private List<Point> _dataPoints = new List<Point>();
        private readonly DataGenerator _generator;

        public List<Point> DataPoints { get => _dataPoints; set => UpdateFieldAndNotify(ref _dataPoints, value); }
        private readonly Trainer trainer;

        public ObservableCollection<PointF> VisibleNeurons { get; } = new ObservableCollection<PointF>();

        public List<Generation> Generations { get; } = new List<Generation>();
        public int GenerationCount { get => _generetionCount; set => UpdateFieldAndNotify(ref _generetionCount, value); }
        public int VisibleGeneration { get => _visibleGeneration; set => UpdateFieldAndNotify(ref _visibleGeneration, value); }
        public int Tiredness { get => tiredness; set => UpdateFieldAndNotify(ref tiredness, value); }
        public int RepeatLearningDataAmount { get => _repeatLearningDataAmount; set => UpdateFieldAndNotify(ref _repeatLearningDataAmount, value); }
        public int NeuronAmount { get => _neuronAmount; set => UpdateFieldAndNotify(ref _neuronAmount, value); }
        public double LearningImpact { get => _learningImpact; set => UpdateFieldAndNotify(ref _learningImpact, value); }

        // To use this class within your viewmodel class:
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
        public ICommand Start
        {
            get
            {
                if (_start == null)
                {
                    _start = new RelayCommand(_ => this.StartLearining(),
                        _ => true);
                }
                return _start;
            }
        }

        public MainWindowViewModel()
        {
            _generator = new DataGenerator();

            trainer = new Trainer();
            //trainer.IteractionStarting += (sender, trainer) => Debug.WriteLine($"Iteration {trainer.Iteration} started…");
            trainer.IteractionWinner += (sender, winner) =>
            {
                Generations.Add(new Generation(trainer.Neurons.Select(d => (NeuronDto)d), trainer.Iteration));

                //Debug.WriteLine($"Iteration won: {winner}");
            };
            trainer.TrainingFinished += (sender, trainer) =>
            {
                Debug.WriteLine($"Training finished in {trainer.Iteration} iterations");
                GenerationCount = trainer.Iteration;
                // Debug.WriteLine("All vectors after learning:");
                // trainer.Neurons.ForEach(d => Debug.WriteLine(d.ToString()));
            };

            this.PropertyChanged += MainWindowViewModel_PropertyChanged;
        }

        private void GenerateTrainSet()
        {
            DataPoints = _generator.Generate(10, 50, 1000, new Point(0, 0), new Point(1000, 1000)).ToList();
        }

        private void StartLearining()
        {
            if (DataPoints.Count == 0) GenerateTrainSet();
            Generations.Clear();
            GenerationCount = 0;
            VisibleGeneration = 0;
            Task.Run(() =>
            {
                var neurons = NeuronFactory.GenerateNeurons(NeuronAmount, 2, learningImpact: LearningImpact, tiredness: Tiredness, minValueOfWeight: 0, maxValueOfWeights: 1000);
                List<double[]> rawData = DataPoints.Select(d => new[] { (double)d.X, (double)d.Y }).ToList();
                var toLearn = new List<double[]>();
                foreach (var _ in Enumerable.Range(0, RepeatLearningDataAmount)) toLearn.AddRange(rawData);
                Debug.WriteLine($"Amount: {toLearn.Count}");
                trainer.Train(neurons, (IEnumerable<IEnumerable<double>>)toLearn);
            });
        }

        private void MainWindowViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(VisibleGeneration))
            {
                VisibleNeurons.Clear();
                if (Generations.Count == 0) return;
                var generationToShow = Generations.FirstOrDefault(d => d.GenerationNumber == VisibleGeneration);
                if (generationToShow == null) return;
                foreach (var neuron in generationToShow.Neurons)
                    VisibleNeurons.Add(neuron.ToPointF());
            }
        }

        public void UpdateFieldAndNotify<T>(ref T fieldToUpdate, T value, [CallerMemberName] string callerName = "")
        {
            if (Equals(fieldToUpdate, value)) return;

            fieldToUpdate = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(callerName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
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