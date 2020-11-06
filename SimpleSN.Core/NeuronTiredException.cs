using System;
using System.Runtime.Serialization;

namespace SimpleSN
{
    [Serializable]
    internal class NeuronTiredException : Exception
    {
        public NeuronTiredException()
        {
        }

        public NeuronTiredException(string message) : base(message)
        {
        }

        public NeuronTiredException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected NeuronTiredException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}