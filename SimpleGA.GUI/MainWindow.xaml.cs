using Newtonsoft.Json;
using SimpleGA.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;

namespace SimpleGA.GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// Salesman dataset: https://people.sc.fsu.edu/~jburkardt/datasets/tsp/tsp.html
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // FInd the best solution for \sqrt((x1-x2)^2 + (y1 - y2)^)
            //var worker = new Thread(() =>
            {
                // var selection = new RouletteSelection();
                // var chromosomeFactory = new MyProblemChromosomeFactory();
                // //var crossover = new MyProblemChromosomeCrossover();
                // //var crossover = new UniformCrossover<MyProblemChromosome, double>(chromosomeFactory);
                // //var crossover = new SinglePointCrossover<MyProblemChromosome, double>(chromosomeFactory);
                // var crossover = new MultiPointCrossover<MyProblemChromosome, double>(2, chromosomeFactory);
                // //var mutation = new RandomResettingMutation<MyProblemChromosome, double>(chromosomeFactory);
                // var mutation = new SwapMutation<MyProblemChromosome, double>(chromosomeFactory);
                // var fitness = new MyProblemFitness();
                // var population = new Population<MyProblemChromosome>(1000, 2000, chromosomeFactory, crossover, mutation, selection);
                // 
                // var ga = new GeneticAlgorithm<MyProblemChromosome>(population, fitness);
                // ga.Termination = new GenerationNumberTermination(1000);

                var selection = new RouletteSelection();
                var chromosomeFactory = new TravelerProblemFactory();
                var crossover = new OrderedCrossover<TravelerProblemChromosome, City>(chromosomeFactory);
                var fitness = new TravelsManFitness();
                var mutation = new SwapMutation<TravelerProblemChromosome, City>(chromosomeFactory);
                var population = new Population<TravelerProblemChromosome>(1000, 2000, chromosomeFactory, crossover, mutation, selection);
                var ga = new GeneticAlgorithm<TravelerProblemChromosome>(population, fitness);
                ga.Termination = new GenerationNumberTermination(10000);

                IChromosome previousWinner = null;
                ga.GenerationHasGone += (sender, generation) =>
                {
                    if (generation.BestChromosome == previousWinner)
                    {
                        Debug.Write(".");
                        return;
                    }
                    Debug.WriteLine("");
                    previousWinner = generation.BestChromosome;
                    Debug.WriteLine(
                        $"Generację {(sender as IGeneticAlgorithm).GenerationsNumber} wygrał {generation.BestChromosome} z ścieżką {generation.BestChromosome.TotalPath}.");
                    Debug.WriteLine(JsonConvert.SerializeObject(generation.BestChromosome.Genes.Select(d => d.Name)));
                };

                Debug.WriteLine("GA running...");
                ga.Start();

                Debug.WriteLine("\r\nBest solution found has {0} path ({1}).", ga.BestChromosome.Fitness, ga.BestChromosome.TotalPath);
                Debug.WriteLine(JsonConvert.SerializeObject(ga.BestChromosome.Genes.Select(d => d.Name)));
            }
            //);
            //worker.Start();
        }
    }

    public class TravelerProblemFactory : IGenableChromosomeFactory<TravelerProblemChromosome, City>
    {
        private string FilePath { get; } = "SalesManData/ATT48.txt"; // Shortest path is 33523
        //private string FilePath { get; } = "SalesManData/P01.txt"; // Shortest path is 291
        //private string FilePath { get; } = "SalesManData/ATT48.txt";
        private List<City> CitiesFromFile { get; } = new List<City>();
        /// <inheritdoc />
        TravelerProblemChromosome IChromosomeFactory<TravelerProblemChromosome>.CreateNew()
        {
            if (CitiesFromFile.Count == 0) LoadCitiesFromFile();
            return new TravelerProblemChromosome(CitiesFromFile.Shuffle());
        }

        private void LoadCitiesFromFile()
        {
            var lines = File.ReadAllLines(FilePath);
            int i = 0;
            foreach (var cityRaw in lines.Where(d => !string.IsNullOrWhiteSpace(d)))
            {
                var split = cityRaw.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                if (split.Length != 2) throw new ArgumentException("There is abnormal amount of points for the city");
                var city = new City()
                {
                    Location = new PointF(float.Parse(split[0]), float.Parse(split[1])),
                    Name = $"City {++i}"
                };
                CitiesFromFile.Add(city);
            }
        }

        /// <inheritdoc />
        public TravelerProblemChromosome FromGenes(IList<City> genes)
        {
            return new TravelerProblemChromosome(genes);
        }

        /// <inheritdoc />
        public City GetGene(int geneNumber)
        {
            throw new System.NotImplementedException();
        }
    }

    public class TravelsManFitness : IFitness<TravelerProblemChromosome>
    {
        /// <inheritdoc />
        public double Evaluate(TravelerProblemChromosome chromosome)
        {
            var cities = chromosome.Genes;
            var sum = 0.0;
            for (int i = 0; i < cities.Count - 1; i++)
            {
                var first = cities[i].Location;
                var second = cities[i + 1].Location;
                sum += Math.Sqrt(Math.Pow(first.X - second.X, 2) + Math.Pow(first.Y - second.Y, 2));
            }

            chromosome.TotalPath = sum;

            return 1 / sum;
        }

    }
}
