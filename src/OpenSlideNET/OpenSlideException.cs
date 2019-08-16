using System;

namespace OpenSlideNET
{
    /// <summary>
    /// The exception that is thrown when OpenSlide library call failed.
    /// </summary>
    public class OpenSlideException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OpenSlideException"/> class.
        /// </summary>
        public OpenSlideException() : base() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenSlideException"/> class with a specified error message.
        /// </summary>
        public OpenSlideException(string message) : base(message) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenSlideException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception. If the <paramref name="innerException"/> parameter is not a null reference, the current exception is raised in a catch block that handles the inner exception.</param>
        public OpenSlideException(string message, Exception innerException) : base(message, innerException) { }

    }
}
