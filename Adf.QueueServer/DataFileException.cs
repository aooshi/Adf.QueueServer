using System;

namespace Adf.QueueServer
{
    class DataFileException : Exception
    {
        public DataFileException(string message)
            : base(message)
        {
        }
    }
}
