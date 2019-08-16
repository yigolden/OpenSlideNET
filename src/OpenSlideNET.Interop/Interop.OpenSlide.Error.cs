using System;
using System.Runtime.InteropServices;

namespace OpenSlideNET.Interop
{
    public static partial class OpenSlideInterop
    {

        [DllImport(LibOpenSlide, EntryPoint = "openslide_get_error", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr GetErrorInternal(OpenSlideImageSafeHandle osr);

        /// <summary>
        /// Get the current error string. 
        /// For a given OpenSlide object, once this function returns a non-NULL value, the only useful operation on the object is to call openslide_close() to free its resources.
        /// </summary>
        /// <param name="osr">The OpenSlide object. </param>
        /// <returns>A string describing the original error that caused the problem, or NULL if no error has occurred. </returns>
        public static string GetError(OpenSlideImageSafeHandle osr)
        {
            IntPtr pResult = GetErrorInternal(osr);
            return StringFromNativeUtf8(pResult);
        }
    }
}
