using System;
using System.Runtime.InteropServices;

namespace OpenSlideNET.Interop
{
    /// <summary>
    /// The interop helpler for OpenSlide.
    /// </summary>
    public static partial class OpenSlideInterop
    {
#if LINUX
        internal const string LibOpenSlide = "libopenslide.so.0";
#elif OSX
        internal const string LibOpenSlide = "libopenslide.0.dylib";
#else
        internal const string LibOpenSlide = "libopenslide-0.dll";
#endif

        /// <summary>
        /// The name of the property containing a slide's comment, if any. 
        /// </summary>
        public const string OpenSlidePropertyNameComment = "openslide.comment";

        /// <summary>
        /// The name of the property containing a slide's comment, if any. 
        /// </summary>
        public static ReadOnlySpan<byte> Utf8OpenSlidePropertyNameComment => new byte[]
        {
            (byte)'o', (byte)'p', (byte)'e', (byte)'n', (byte)'s', (byte)'l', (byte)'i', (byte)'d', (byte)'e',
            (byte)'.',
            (byte)'c', (byte)'o', (byte)'m', (byte)'m', (byte)'e', (byte)'n', (byte)'t',
            0
        };

        /// <summary>
        /// The name of the property containing an identification of the vendor. 
        /// </summary>
        public const string OpenSlidePropertyNameVendor = "openslide.vendor";

        /// <summary>
        /// The name of the property containing an identification of the vendor. 
        /// </summary>
        public static ReadOnlySpan<byte> Utf8OpenSlidePropertyNameVendor => new byte[]
        {
            (byte)'o', (byte)'p', (byte)'e', (byte)'n', (byte)'s', (byte)'l', (byte)'i', (byte)'d', (byte)'e',
            (byte)'.',
            (byte)'v', (byte)'e', (byte)'n', (byte)'d', (byte)'o', (byte)'r',
            0
        };

        /// <summary>
        /// The name of the property containing the "quickhash-1" sum. 
        /// </summary>
        public const string OpenSlidePropertyNameQuickHash1 = "openslide.quickhash-1";

        /// <summary>
        /// The name of the property containing the "quickhash-1" sum. 
        /// </summary>
        public static ReadOnlySpan<byte> Utf8OpenSlidePropertyNameQuickHash1 => new byte[]
        {
            (byte)'o', (byte)'p', (byte)'e', (byte)'n', (byte)'s', (byte)'l', (byte)'i', (byte)'d', (byte)'e',
            (byte)'.',
            (byte)'q', (byte)'u', (byte)'i', (byte)'c', (byte)'k', (byte)'h', (byte)'a', (byte)'s', (byte)'h',
            (byte)'-', (byte)'1',
            0
        };

        /// <summary>
        /// The name of the property containing a slide's background color, if any.
        /// </summary>
        public const string OpenSlidePropertyNameBackgroundColor = "openslide.background-color";

        /// <summary>
        /// The name of the property containing a slide's background color, if any.
        /// </summary>
        public static ReadOnlySpan<byte> Utf8OpenSlidePropertyNameBackgroundColor => new byte[]
        {
            (byte)'o', (byte)'p', (byte)'e', (byte)'n', (byte)'s', (byte)'l', (byte)'i', (byte)'d', (byte)'e',
            (byte)'.',
            (byte)'b', (byte)'a', (byte)'c', (byte)'k', (byte)'g', (byte)'r', (byte)'o', (byte)'u', (byte)'n',
            (byte)'d', (byte)'-', (byte)'c', (byte)'o', (byte)'l', (byte)'o', (byte)'r',
            0
        };

        /// <summary>
        /// The name of the property containing a slide's objective power, if known. 
        /// </summary>
        public const string OpenSlidePropertyNameObjectivePower = "openslide.objective-power";

        /// <summary>
        /// The name of the property containing a slide's objective power, if known. 
        /// </summary>
        public static ReadOnlySpan<byte> Utf8OpenSlidePropertyNameObjectivePower => new byte[]
        {
            (byte)'o', (byte)'p', (byte)'e', (byte)'n', (byte)'s', (byte)'l', (byte)'i', (byte)'d', (byte)'e',
            (byte)'.',
            (byte)'o', (byte)'b', (byte)'j', (byte)'e', (byte)'c', (byte)'t', (byte)'i', (byte)'v', (byte)'e',
            (byte)'-', (byte)'p', (byte)'o', (byte)'w', (byte)'e', (byte)'r',
            0
        };

        /// <summary>
        /// The name of the property containing the number of microns per pixel in the X dimension of level 0, if known.
        /// </summary>
        public const string OpenSlidePropertyNameMPPX = "openslide.mpp-x";

        /// <summary>
        /// The name of the property containing the number of microns per pixel in the X dimension of level 0, if known.
        /// </summary>
        public static ReadOnlySpan<byte> Utf8OpenSlidePropertyNameMPPX => new byte[]
        {
            (byte)'o', (byte)'p', (byte)'e', (byte)'n', (byte)'s', (byte)'l', (byte)'i', (byte)'d', (byte)'e',
            (byte)'.',
            (byte)'m', (byte)'p', (byte)'p', (byte)'-', (byte)'x',
            0
        };

        /// <summary>
        /// The name of the property containing the number of microns per pixel in the Y dimension of level 0, if known.
        /// </summary>
        public const string OpenSlidePropertyNameMPPY = "openslide.mpp-y";

        /// <summary>
        /// The name of the property containing the number of microns per pixel in the Y dimension of level 0, if known.
        /// </summary>
        public static ReadOnlySpan<byte> Utf8OpenSlidePropertyNameMPPY => new byte[]
        {
            (byte)'o', (byte)'p', (byte)'e', (byte)'n', (byte)'s', (byte)'l', (byte)'i', (byte)'d', (byte)'e',
            (byte)'.',
            (byte)'m', (byte)'p', (byte)'p', (byte)'-', (byte)'y',
            0
        };

        /// <summary>
        /// The name of the property containing the X coordinate of the rectangle bounding the non-empty region of the slide, if available. 
        /// </summary>
        public const string OpenSlidePropertyNameBoundsX = "openslide.bounds-x";

        /// <summary>
        /// The name of the property containing the X coordinate of the rectangle bounding the non-empty region of the slide, if available. 
        /// </summary>
        public static ReadOnlySpan<byte> Utf8OpenSlidePropertyNameBoundsX => new byte[]
        {
            (byte)'o', (byte)'p', (byte)'e', (byte)'n', (byte)'s', (byte)'l', (byte)'i', (byte)'d', (byte)'e',
            (byte)'.',
            (byte)'b', (byte)'o', (byte)'u', (byte)'n', (byte)'d', (byte)'s', (byte)'-', (byte)'x',
            0
        };

        /// <summary>
        /// The name of the property containing the Y coordinate of the rectangle bounding the non-empty region of the slide, if available. 
        /// </summary>
        public const string OpenSlidePropertyNameBoundsY = "openslide.bounds-y";

        /// <summary>
        /// The name of the property containing the Y coordinate of the rectangle bounding the non-empty region of the slide, if available. 
        /// </summary>
        public static ReadOnlySpan<byte> Utf8OpenSlidePropertyNameBoundsY => new byte[]
        {
            (byte)'o', (byte)'p', (byte)'e', (byte)'n', (byte)'s', (byte)'l', (byte)'i', (byte)'d', (byte)'e',
            (byte)'.',
            (byte)'b', (byte)'o', (byte)'u', (byte)'n', (byte)'d', (byte)'s', (byte)'-', (byte)'y',
            0
        };

        /// <summary>
        /// The name of the property containing the width of the rectangle bounding the non-empty region of the slide, if available. 
        /// </summary>
        public const string OpenSlidePropertyNameBoundsWidth = "openslide.bounds-width";

        /// <summary>
        /// The name of the property containing the width of the rectangle bounding the non-empty region of the slide, if available. 
        /// </summary>
        public static ReadOnlySpan<byte> Utf8OpenSlidePropertyNameBoundsWidth => new byte[]
        {
            (byte)'o', (byte)'p', (byte)'e', (byte)'n', (byte)'s', (byte)'l', (byte)'i', (byte)'d', (byte)'e',
            (byte)'.',
            (byte)'b', (byte)'o', (byte)'u', (byte)'n', (byte)'d', (byte)'s', (byte)'-', (byte)'w', (byte)'i',
            (byte)'d', (byte)'t', (byte)'h',
            0
        };

        /// <summary>
        /// The name of the property containing the height of the rectangle bounding the non-empty region of the slide, if available.
        /// </summary>
        public const string OpenSlidePropertyNameBoundsHeight = "openslide.bounds-height";

        /// <summary>
        /// The name of the property containing the height of the rectangle bounding the non-empty region of the slide, if available.
        /// </summary>
        public static ReadOnlySpan<byte> Utf8OpenSlidePropertyNameBoundsHeight => new byte[]
        {
            (byte)'o', (byte)'p', (byte)'e', (byte)'n', (byte)'s', (byte)'l', (byte)'i', (byte)'d', (byte)'e',
            (byte)'.',
            (byte)'b', (byte)'o', (byte)'u', (byte)'n', (byte)'d', (byte)'s', (byte)'-', (byte)'h', (byte)'e',
            (byte)'i', (byte)'g', (byte)'h', (byte)'t',
            0
        };

        [DllImport(LibOpenSlide, EntryPoint = "openslide_get_version", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr GetVersionInternal();

        /// <summary>
        /// Get the version of the OpenSlide library.
        /// </summary>
        public static string GetVersion()
        {
            IntPtr pResult = GetVersionInternal();
            return StringFromNativeUtf8(pResult)!;
        }
    }
}
