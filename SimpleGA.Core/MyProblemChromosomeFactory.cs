namespace SimpleGA.Core
{
    public class MyProblemChromosomeFactory : IChromosomeFactory<MyProblemChromosome>
    {
        /// <inheritdoc />
        public MyProblemChromosome CreateNew()
        {

            return new MyProblemChromosome(null, null, null, null);
        }
    }
}