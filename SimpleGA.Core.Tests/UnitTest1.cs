using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using FluentAssertions;
using FluentAssertions.Common;
using Newtonsoft.Json;
using SimpleGA.Core.Chromosomes;
using SimpleGA.Core.Crossovers;
using SimpleGA.Core.Extensions;
using SimpleGA.Core.Fitnesses;
using SimpleGA.Core.Mutations;
using SimpleGA.Core.Populations;
using SimpleGA.Core.Selections;
using SimpleGA.Core.Solutions.KnapsackProblem;
using SimpleGA.Core.Solutions.MyProblem;
using SimpleGA.Core.Solutions.TravelersSalesmanProblem;
using SimpleGA.Core.Terminations;

namespace SimpleGA.Core.Tests
{
    public class Tests
    {
        public void CheckIfAreEqual<TChrom, TGen, TFactory, TFitness>()
            where TChrom : class, IGenableChromosome<TGen>
            where TFactory : IGenableChromosomeFactory<TChrom, TGen>, new()
            where TFitness : IFitness<TChrom>, new()
        {
            var selections = new Func<ISelection>[] {() => new RouletteSelection()};

            Func<double, double, TFactory, ICrossover<TChrom>> orderedCrossover = (begining, finish, factory) =>
                new OrderedCrossover<TChrom, TGen>(begining, finish, factory);
            Func<int, TFactory, ICrossover<TChrom>> cyclicCrossover =
                (step, factory) => new CyclicOrderedCrossover<TChrom, TGen>(step, factory);

            var crossOvers = new Func<TFactory, ICrossover<TChrom>>[]
            {
                factory => orderedCrossover(0.1, 0.9, factory),
                factory => orderedCrossover(0.2, 0.8, factory),
                factory => orderedCrossover(0.3, 0.7, factory),
                factory => orderedCrossover(0.4, 0.6, factory),

                factory => cyclicCrossover(2, factory),
                factory => cyclicCrossover(3, factory),
                factory => cyclicCrossover(5, factory),
                factory => cyclicCrossover(8, factory),
                factory => cyclicCrossover(13, factory),
            };

            Func<int, double, TFactory, IMutation<TChrom>> mutationFactory = (swaps, mutationThreshold, factory) =>
                new SwapMutation<TChrom, TGen>(factory) {MutationThreshold = mutationThreshold, AmountOfSwaps = swaps};

            var mutations = new List<Func<TFactory, IMutation<TChrom>>>();
            foreach (var swaps in new[] {1, 2, 3, 5, 8, 13})
            foreach (var threshold in new[] {0, 0.1, 0.2, 0.3, 0.4, 0.5, 0.6, 0.7, 0.8, 0.9})
                mutations.Add(factory => mutationFactory(swaps, threshold, factory));

            var terminationFactory = new Func<ITermination>[]
            {
                () => new GenerationNumberTermination(1000),
                () => new GenerationNumberTermination(2000),
                () => new GenerationNumberTermination(3000),
                () => new GenerationNumberTermination(5000),
                () => new GenerationNumberTermination(8000),
                () => new GenerationNumberTermination(13000),

                () => new TheSameNeuronWinner(100),
                () => new TheSameNeuronWinner(200),
                () => new TheSameNeuronWinner(300),
                () => new TheSameNeuronWinner(500),
                () => new TheSameNeuronWinner(800),
                () => new TheSameNeuronWinner(1300),
                () => new TheSameNeuronWinner(2100),
            };

            var populations = new[] {100, 200, 300, 500, 800, 1300};

            foreach (var selection in selections)
            foreach (var crossover in crossOvers)
            foreach (var mutation in mutations)
            foreach (var population in populations)
            foreach (var termination in terminationFactory)
            {
                var factory = new TFactory();
                var crossOverReal = crossover(factory);
                var selectionReal = selection();
                var mutationReal = mutation(factory);
                var terminationReal = termination();
                IChromosome previousWinner = null;

                var resA = new Result<TChrom>();
                resA.Population = 100;
                resA.Selection = selectionReal;
                resA.Crossover = crossOverReal;
                resA.Mutation = mutationReal;
                resA.Termination = terminationReal;

                var resB = new Result<TChrom>();
                resB.Population = 100;
                resB.Selection = selectionReal;
                resB.Crossover = crossOverReal;
                resB.Mutation = mutationReal;
                resB.Termination = terminationReal;
                resA.IsEqualToResult(resB).Should().BeTrue();
            }
        }

        [Test]
        public void Test3()
        {
            CheckIfAreEqual<KnapsackProblemChromosome, Insert, KnapsackProblemFactory, KnapsackFitness>();
            CheckIfAreEqual<TravelerProblemChromosome, City, TravelerProblemFactory, TravelsManFitness>();
        }

        [Test]
        public void Test4()
        {
            var factory = new KnapsackProblemFactory();
            var crossOver = new OrderedCrossover<KnapsackProblemChromosome, Insert>(factory);
            var mutation = new SwapMutation<KnapsackProblemChromosome, Insert>(factory);
            var selection = new RouletteSelection();
            var resA = new Result<KnapsackProblemChromosome>();
            resA.Population = 100;
            resA.Selection = selection;
            resA.Crossover = crossOver;
            resA.Mutation = mutation;
            resA.Termination = new GenerationNumberTermination(1000);

            var resB = new Result<KnapsackProblemChromosome>();
            resB.Population = 100;
            resB.Selection = selection;
            resB.Crossover = crossOver;
            resB.Mutation = mutation;
            resB.Termination = new GenerationNumberTermination(2000);

            resA.IsEqualToResult(resB).Should().BeFalse();
        }

        [Test]
        public void Test5()
        {
            var factory = new KnapsackProblemFactory();
            var crossOver = new OrderedCrossover<KnapsackProblemChromosome, Insert>(factory);
            var mutation = new SwapMutation<KnapsackProblemChromosome, Insert>(factory);
            var selection = new RouletteSelection();
            var termination = new GenerationNumberTermination(1000);
            var resA = new Result<KnapsackProblemChromosome>();
            resA.Population = 100;
            resA.Selection = selection;
            resA.Crossover = crossOver;
            resA.Mutation = mutation;
            resA.Termination = termination;
            ;

            var resB = new Result<KnapsackProblemChromosome>();
            resB.Population = 200;
            resB.Selection = selection;
            resB.Crossover = crossOver;
            resB.Mutation = mutation;
            resB.Termination = termination;
            ;
            resA.IsEqualToResult(resB).Should().BeFalse();
        }

        [Test]
        public void Test1()
        {
            var factory = new IntArrayOrderedChromosomeFactory();
            var orderedCrossover = new OrderedCrossover<IntArrayChromosome, int>(factory);

            var parents = Enumerable.Range(0, orderedCrossover.RequiredNumberOfParents)
                .Select(_ => factory.CreateNew());

            var kids = orderedCrossover.MakeChildren(parents);

            var firstParentCount = parents.First().Genes.Count;
            foreach (var parent in parents)
            {
                parent.Genes.Count.Should().Be(firstParentCount,
                    "All chromosomes after Ordered cross over MUST be the same length");
                foreach(var kid in kids)
                {
                    kid.Genes.Should().HaveCount(parent.Genes.Count);
                    kid.Genes.Should().IntersectWith(parent.Genes);
                }
            }
        }

        [Test]
        public void Test2()
        {
            var factory = new IntArrayOrderedChromosomeFactory();
            var orderedCrossover = new CyclicOrderedCrossover<IntArrayChromosome, int>(factory);

            var parents = Enumerable.Range(0, orderedCrossover.RequiredNumberOfParents)
                                    .Select(_ => factory.CreateNew());

            var kids = orderedCrossover.MakeChildren(parents);

            var firstParentCount = parents.First().Genes.Count;
            foreach (var parent in parents)
            {
                parent.Genes.Count.Should().Be(firstParentCount,
                    "All chromosomes after Ordered cross over MUST be the same length");
                foreach (var kid in kids)
                {
                    kid.Genes.Should().HaveCount(parent.Genes.Count);
                    kid.Genes.Should().IntersectWith(parent.Genes);
                }
            }
        }
    }

    public class IntArrayOrderedChromosomeFactory : IGenableChromosomeFactory<IntArrayChromosome, int>
    {
        public int AmountOfElements { get; set; } = 10;
        public List<int> AllElements { get; } = new List<int>();
        private Random Random = new Random();
        /// <inheritdoc />
        public IntArrayChromosome CreateNew()
        {
            if (AllElements.Count == 0)
            {
                AllElements.AddRange(Enumerable.Range(0, AmountOfElements).Select(d => Random.Next()));
            }
            return new IntArrayChromosome(AllElements.Shuffle().ToList());
        }

        /// <inheritdoc />
        public IntArrayChromosome FromGenes(IList<int> genes)
        {
            return new IntArrayChromosome(genes.ToList());
        }

        /// <inheritdoc />
        public int GetGene(int geneNumber)
        {
            throw new NotImplementedException();
        }
    }

    public class IntArrayChromosome : FitnessComparableChromosome, IGenableChromosome<int>
    {
        private IReadOnlyList<int> _genes;

        public IntArrayChromosome(IReadOnlyList<int> genes)
        {
            _genes = genes;
        }

        /// <inheritdoc />
        public double? Fitness { get; set; }

        /// <inheritdoc />
        public IReadOnlyList<int> Genes => _genes;
    }
}