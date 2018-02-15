using System;

namespace OpenSlideNET
{
    public class OpenSlideException : Exception
    {

        public OpenSlideException() : base() { }

        public OpenSlideException(string message) : base(message) { }

        public OpenSlideException(string message, Exception innerException) : base(message, innerException) { }

    }
}
