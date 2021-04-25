using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using SimpleGA.Core.Chromosomes;
using SimpleGA.Core.Extensions;
using SimpleGA.Core.Solutions.MyProblem;

namespace SimpleGA.Core.Solutions.TravelersSalesmanProblem
{
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
            ++_counter;
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

        public long _counter = 0;

        /// <inheritdoc />
        public TravelerProblemChromosome FromGenes(IList<City> genes)
        {
            ++_counter;
            return new TravelerProblemChromosome(genes);
        }

        /// <inheritdoc />
        public City GetGene(int geneNumber)
        {
            throw new System.NotImplementedException();
        }
    }
}