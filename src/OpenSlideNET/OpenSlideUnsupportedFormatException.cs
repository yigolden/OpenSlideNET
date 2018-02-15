using System;
using System.Collections.Generic;
using System.Text;

namespace OpenSlideNET
{
    public class OpenSlideUnsupportedFormatException : OpenSlideException
    {
        public OpenSlideUnsupportedFormatException() : base() { }

        public OpenSlideUnsupportedFormatException(string message) : base(message) { }

        public OpenSlideUnsupportedFormatException(string message, Exception innerException) : base(message, innerException) { }
    }
}
