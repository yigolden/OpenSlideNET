using System;
using System.Runtime.InteropServices;

namespace OpenSlideNET
{
    internal static partial class Interop
    {

        [DllImport(LibOpenSlide, EntryPoint = "openslide_get_error", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr GetError_Internal(IntPtr osr);

        /// <summary>
        /// Get the current error string. 
        /// For a given OpenSlide object, once this function returns a non-NULL value, the only useful operation on the object is to call openslide_close() to free its resources.
        /// </summary>
        /// <param name="osr">The OpenSlide object. </param>
        /// <returns>A string describing the original error that caused the problem, or NULL if no error has occurred. </returns>
        internal static string GetError(IntPtr osr)
        {
            IntPtr pResult = GetError_Internal(osr);
            return StringFromNativeUtf8(pResult);
        }
    }
}
