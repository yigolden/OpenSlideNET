using System;
using System.Runtime.InteropServices;

namespace OpenSlideNET
{
    internal static partial class Interop
    {
        internal const string LibOpenSlide = "libopenslide-0.dll";

        /// <summary>
        /// The name of the property containing a slide's comment, if any. 
        /// </summary>
        internal const string OpenSlidePropertyNameComment = "openslide.comment";

        /// <summary>
        /// The name of the property containing an identification of the vendor. 
        /// </summary>
        internal const string OpenSlidePropertyNameVendor = "openslide.vendor";

        /// <summary>
        /// The name of the property containing the "quickhash-1" sum. 
        /// </summary>
        internal const string OpenSlidePropertyNameQuickHash1 = "openslide.quickhash-1";

        /// <summary>
        /// The name of the property containing a slide's background color, if any.
        /// </summary>
        internal const string OpenSlidePropertyNameBackgroundColor = "openslide.background-color";

        /// <summary>
        /// The name of the property containing a slide's objective power, if known. 
        /// </summary>
        internal const string OpenSlidePropertyNameObjectivePower = "openslide.objective-power";

        /// <summary>
        /// The name of the property containing the number of microns per pixel in the X dimension of level 0, if known.
        /// </summary>
        internal const string OpenSlidePropertyNameMPPX = "openslide.mpp-x";

        /// <summary>
        /// The name of the property containing the number of microns per pixel in the Y dimension of level 0, if known.
        /// </summary>
        internal const string OpenSlidePropertyNameMPPY = "openslide.mpp-y";

        /// <summary>
        /// The name of the property containing the X coordinate of the rectangle bounding the non-empty region of the slide, if available. 
        /// </summary>
        internal const string OpenSlidePropertyNameBoundsX = "openslide.bounds-x";

        /// <summary>
        /// The name of the property containing the Y coordinate of the rectangle bounding the non-empty region of the slide, if available. 
        /// </summary>
        internal const string OpenSlidePropertyNameBoundsY = "openslide.bounds-y";

        /// <summary>
        /// The name of the property containing the width of the rectangle bounding the non-empty region of the slide, if available. 
        /// </summary>
        internal const string OpenSlidePropertyNameBoundsWidth = "openslide.bounds-width";

        /// <summary>
        /// The name of the property containing the height of the rectangle bounding the non-empty region of the slide, if available.
        /// </summary>
        internal const string OpenSlidePropertyNameBoundsHeight = "openslide.bounds-height";

        [DllImport(LibOpenSlide, EntryPoint = "openslide_get_version", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr GetVersion_Internal();

        internal static string GetVersion()
        {
            IntPtr pResult = GetVersion_Internal();
            return StringFromNativeUtf8(pResult);
        }
    }
}
