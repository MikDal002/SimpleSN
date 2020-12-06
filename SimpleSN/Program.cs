using SimpleSN.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace SimpleSN
{

    class Program
    {
        static void Main(string[] _)
        {
            var generator = new DataGenerator();
            var data = generator.Generate(10, 50, 1000, new Point(0, 0), new Point(1000, 1000));

            StringBuilder vectors = new StringBuilder();
            data.Aggregate(vectors, (left, point) => left.AppendLine(point.X + "; " + point.Y));
            File.WriteAllText("sourcedata.csv", vectors.ToString());

            var trainer = new Trainer();
            trainer.IterationStarted += (sender, trainer) => Console.WriteLine($"Iteration {trainer.Iteration} started…");
            trainer.IterationFinished += (sender, winner) => Console.WriteLine($"Iteration won: {winner}");
            trainer.TrainingFinished += (sender, trainer) =>
            {
                Console.WriteLine($"Training finished in {trainer.Iteration} iterations");
                Console.WriteLine("All vectors after learning:");
                trainer.Neurons.ForEach(d => Console.WriteLine(d.ToString()));
            };
            //trainer.Train(NeuronFactory.GetFromLab1(), VectorsFactory.FromFile("Lab1InputVectors.csv"));
            var neurons = NeuronFactory.GenerateNeurons(10, 2, 0, 1000).ToList();
            trainer.Train(neurons, data.Select(d=> new[] {(double) d.X, (double)d.Y }));

            StringBuilder neur = new StringBuilder();
            neurons.Select(d => d.Weights).Aggregate(neur, (_, r) => neur.AppendLine(r.ToCsvLine()));
            File.WriteAllText("neurons.csv", neur.ToString());

            StringBuilder vect = new StringBuilder();
            data.Aggregate(vect, (left, point) => left.AppendLine(point.X + "; " + point.Y));
            File.WriteAllText("sourcedata.csv", vect.ToString());
        }
    }
}
