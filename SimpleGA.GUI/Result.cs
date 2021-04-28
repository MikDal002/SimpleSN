using System.Collections.Generic;
using SimpleGA.Core.Chromosomes;
using SimpleGA.Core.Crossovers;
using SimpleGA.Core.Mutations;
using SimpleGA.Core.Selections;
using SimpleGA.Core.Terminations;

namespace SimpleGA.GUI
{
    public class StepDef
    {
        public int generation { get; set; }
        public long Elapse { get; set; }
        public double fitness { get; set; }
    }

    public class Result<TChrom> where TChrom : class, IChromosome
    {
        public string SelectionName => Selection.GetType().Name;
        public ISelection Selection { get; set; }
        public string CrossoverName => Crossover.GetType().Name;
        public ICrossover<TChrom> Crossover { get; set; }
        public string MutationName => Mutation.GetType().Name;
        public IMutation<TChrom> Mutation { get; set; }
        public string TerminationName => Termination.GetType().Name;
        public ITermination Termination { get; set; }
        public int Population { get; set; }

        public TChrom WinnerChromosome { get; set; }
        public int AmountOfGenerations { get; set; }


        public double RealTheBestValue { get; set; }
        public double TheBestFoundFitness { get; set; }
        public long TotalTimeMs { get; set; }


        public List<StepDef> Steps { get; set; } = new();
    }
}