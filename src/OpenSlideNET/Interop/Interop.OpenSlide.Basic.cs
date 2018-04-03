using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace OpenSlideNET
{
    internal static partial class Interop
    {

        [DllImport(LibOpenSlide, EntryPoint = "openslide_detect_vendor", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr DetectVendor_Internal(IntPtr filename);

        /// <summary>
        /// Quickly determine whether a whole slide image is recognized. 
        /// If OpenSlide recognizes the file referenced by filename, return a string identifying the slide format vendor. This is equivalent to the value of the OPENSLIDE_PROPERTY_NAME_VENDOR property. Calling openslide_open() on this file will return a valid OpenSlide object or an OpenSlide object in error state.
        /// Otherwise, return NULL. Calling openslide_open() on this file will also return NULL.
        /// </summary>
        /// <param name="filename">The filename to check. </param>
        /// <returns>An identification of the format vendor for this file, or NULL. </returns>
        internal static unsafe string DetectVendor(string filename)
        {
            Debug.Assert(filename != null);

            byte* pointer = stackalloc byte[128];
            UnsafeUtf8Encoder utf8Encoder = new UnsafeUtf8Encoder(pointer, 128);
            try
            {
                IntPtr pResult = DetectVendor_Internal(utf8Encoder.Encode(filename));
                return StringFromNativeUtf8(pResult);
            }
            finally
            {
                utf8Encoder.Dispose();
            }
        }

        [DllImport(LibOpenSlide, EntryPoint = "openslide_open", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr Open_Internal(IntPtr filename);

        /// <summary>
        /// Open a whole slide image. 
        /// This function can be expensive; avoid calling it unnecessarily. For example, a tile server should not call openslide_open() on every tile request. Instead, it should maintain a cache of OpenSlide objects and reuse them when possible.
        /// </summary>
        /// <param name="filename">The filename to open. </param>
        /// <returns>On success, a new OpenSlide object. If the file is not recognized by OpenSlide, NULL. If the file is recognized but an error occurred, an OpenSlide object in error state. </returns>
        internal static unsafe IntPtr Open(string filename)
        {
            Debug.Assert(filename != null);
            byte* pointer = stackalloc byte[128];
            UnsafeUtf8Encoder utf8Encoder = new UnsafeUtf8Encoder(pointer, 128);
            try
            {
                return Open_Internal(utf8Encoder.Encode(filename));
            }
            finally
            {
                utf8Encoder.Dispose();
            }
        }

        /// <summary>
        /// Get the number of levels in the whole slide image. 
        /// </summary>
        /// <param name="osr">The OpenSlide object. </param>
        /// <returns>The number of levels, or -1 if an error occurred. </returns>
        [DllImport(LibOpenSlide, EntryPoint = "openslide_get_level_count", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int GetLevelCount(IntPtr osr);

        /// <summary>
        /// Get the dimensions of level 0 (the largest level). 
        /// Exactly equivalent to calling openslide_get_level_dimensions(osr, 0, w, h).
        /// </summary>
        /// <param name="osr">The OpenSlide object. </param>
        /// <param name="w">The width of the image, or -1 if an error occurred. </param>
        /// <param name="h">The height of the image, or -1 if an error occurred. </param>
        [DllImport(LibOpenSlide, EntryPoint = "openslide_get_level0_dimensions", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void GetLevel0Dimensions(IntPtr osr, out long w, out long h);

        /// <summary>
        /// Get the dimensions of a level. 
        /// </summary>
        /// <param name="osr">The OpenSlide object. </param>
        /// <param name="level">The desired level. </param>
        /// <param name="w">The width of the image, or -1 if an error occurred or the level was out of range. </param>
        /// <param name="h">The height of the image, or -1 if an error occurred or the level was out of range. </param>
        [DllImport(LibOpenSlide, EntryPoint = "openslide_get_level_dimensions", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void GetLevelDimensions(IntPtr osr, int level, out long w, out long h);

        /// <summary>
        /// Get the downsampling factor of a given level. 
        /// </summary>
        /// <param name="osr">The OpenSlide object.</param>
        /// <param name="level">The desired level. </param>
        /// <returns>The downsampling factor for this level, or -1.0 if an error occurred or the level was out of range. </returns>
        [DllImport(LibOpenSlide, EntryPoint = "openslide_get_level_downsample", CallingConvention = CallingConvention.Cdecl)]
        internal static extern double GetLevelDownsample(IntPtr osr, int level);

        /// <summary>
        /// Get the best level to use for displaying the given downsample. 
        /// </summary>
        /// <param name="osr">The OpenSlide object.</param>
        /// <param name="downsample">The downsample factor.</param>
        /// <returns>The level identifier, or -1 if an error occurred.</returns>
        [DllImport(LibOpenSlide, EntryPoint = "openslide_get_best_level_for_downsample", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int GetBestLevelForDownsample(IntPtr osr, double downsample);

        /// <summary>
        /// Copy pre-multiplied ARGB data from a whole slide image. 
        /// This function reads and decompresses a region of a whole slide image into the specified memory location. dest must be a valid pointer to enough memory to hold the region, at least (w * h * 4) bytes in length. If an error occurs or has occurred, then the memory pointed to by dest will be cleared.
        /// </summary>
        /// <param name="osr">The OpenSlide object. </param>
        /// <param name="dest">The destination buffer for the ARGB data. </param>
        /// <param name="x">The top left x-coordinate, in the level 0 reference frame. </param>
        /// <param name="y">The top left y-coordinate, in the level 0 reference frame. </param>
        /// <param name="level">The desired level. </param>
        /// <param name="w">The width of the region. Must be non-negative. </param>
        /// <param name="h">The height of the region. Must be non-negative. </param>
        [DllImport(LibOpenSlide, EntryPoint = "openslide_read_region", CallingConvention = CallingConvention.Cdecl)]
        internal static unsafe extern void ReadRegion(IntPtr osr, void* dest, long x, long y, int level, long w, long h);

        [DllImport(LibOpenSlide, EntryPoint = "openslide_close", CallingConvention = CallingConvention.Cdecl)]
        private static extern void Close_Internal(IntPtr osr);
        private static readonly object s_closeLock = new object();

        /// <summary>
        /// Close an OpenSlide object. 
        /// No other threads may be using the object. After this call returns, the object cannot be used anymore.
        /// </summary>
        /// <param name="osr">The OpenSlide object. </param>
        internal static void Close(IntPtr osr)
        {
            lock (s_closeLock)
            {
                Close_Internal(osr);
            }
        }
    }
}
