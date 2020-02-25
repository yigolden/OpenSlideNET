using System;
using System.Runtime.InteropServices;

namespace OpenSlideNET.Interop
{
    /// <summary>
    /// Represents a wrapper class for OpenSlide image handle.
    /// </summary>
    public class OpenSlideImageSafeHandle : SafeHandle
    {

        /// <summary>
        /// Initializes a new instance of the OpenSlideImageSafeHandle class with the specified handle value.
        /// </summary>
        /// <param name="handle">The OpenSlide handle.</param>
        public OpenSlideImageSafeHandle(IntPtr handle) : base(IntPtr.Zero, true)
        {
            SetHandle(handle);
        }

        /// <summary>
        /// Gets a value indicating whether the handle value is invalid
        /// </summary>
        public override bool IsInvalid => handle == IntPtr.Zero;

        /// <summary>
        /// Free the OpenSlide image handle.
        /// </summary>
        /// <returns>true if the handle is released successfully; otherwise, in the event of a catastrophic failure, false.</returns>
        protected override bool ReleaseHandle()
        {
            var h = handle;
            if (h != IntPtr.Zero)
            {
                OpenSlideInterop.Close(h);
            }
            return true;
        }
    }
}
