using System.Collections;
using System.Collections.Generic;

namespace SimpleGA.Core
{
    public class Generation<T> : IReadOnlyCollection<T> where T : IChromosome
    {
        private readonly IReadOnlyCollection<T> _readOnlyCollectionImplementation;

        public T BestChromosome { get; set; }
        public Generation(IReadOnlyCollection<T> list)
        {
            _readOnlyCollectionImplementation = list;
        }

        /// <inheritdoc />
        public IEnumerator<T> GetEnumerator()
        {
            return _readOnlyCollectionImplementation.GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_readOnlyCollectionImplementation).GetEnumerator();
        }

        /// <inheritdoc />
        public int Count => _readOnlyCollectionImplementation.Count;

    }
}