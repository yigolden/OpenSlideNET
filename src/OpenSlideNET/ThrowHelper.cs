using System;
using System.Runtime.CompilerServices;

namespace OpenSlideNET
{
    internal static class ThrowHelper
    {
        internal static void CheckAndThrowError(IntPtr osr)
        {
            string message = Interop.GetError(osr);
            if (message != null)
            {
                throw new OpenSlideException(message);
            }
        }
        
        internal static bool TryCheckError(IntPtr osr, out string message)
        {
            message = Interop.GetError(osr);
            return message == null;
        }
    }
}
