using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using SimpleGA.Core.Chromosomes;
using SimpleGA.Core.Crossovers;
using SimpleGA.Core.Extensions;

namespace SimpleGA.Core.Tests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
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
            throw new System.NotImplementedException();
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