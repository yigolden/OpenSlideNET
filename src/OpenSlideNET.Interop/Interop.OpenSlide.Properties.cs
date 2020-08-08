using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace OpenSlideNET.Interop
{
    public static partial class OpenSlideInterop
    {

        [DllImport(LibOpenSlide, EntryPoint = "openslide_get_property_names", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr GetPropertyNamesInternal(OpenSlideImageSafeHandle osr);

        /// <summary>
        /// Get the NULL-terminated array of property names. 
        /// Certain vendor-specific metadata properties may exist within a whole slide image. They are encoded as key-value pairs. This call provides a list of names as strings that can be used to read properties with openslide_get_property_value().
        /// </summary>
        /// <param name="osr">The OpenSlide object. </param>
        /// <returns>A NULL-terminated string array of property names, or an empty array if an error occurred. </returns>
        public static unsafe string[] GetPropertyNames(OpenSlideImageSafeHandle osr)
        {
            var list = new List<string>();
            IntPtr* pCurrent = (IntPtr*)GetPropertyNamesInternal(osr);
            while (*pCurrent != IntPtr.Zero)
            {
                string? name = StringFromNativeUtf8(*pCurrent);
                list.Add(name!);
                pCurrent++;
            }
            return list.ToArray();
        }


        [DllImport(LibOpenSlide, EntryPoint = "openslide_get_property_value", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr GetPropertyValueInternal(OpenSlideImageSafeHandle osr, IntPtr name);

        /// <summary>
        /// Get the value of a single property. 
        /// Certain vendor-specific metadata properties may exist within a whole slide image. They are encoded as key-value pairs. This call provides the value of the property given by name.
        /// </summary>
        /// <param name="osr">The OpenSlide object. </param>
        /// <param name="name">The name of the desired property. Must be a valid name as given by openslide_get_property_names().</param>
        /// <returns>The value of the named property, or NULL if the property doesn't exist or an error occurred. </returns>
        public static unsafe string? GetPropertyValue(OpenSlideImageSafeHandle osr, string name)
        {
            byte* pointer = stackalloc byte[64];
            UnsafeUtf8Encoder utf8Encoder = new UnsafeUtf8Encoder(pointer, 64);
            try
            {
                IntPtr pResult = GetPropertyValueInternal(osr, utf8Encoder.Encode(name));
                return StringFromNativeUtf8(pResult);
            }
            finally
            {
                utf8Encoder.Dispose();
            }
        }

        /// <summary>
        /// Get the value of a single property. 
        /// Certain vendor-specific metadata properties may exist within a whole slide image. They are encoded as key-value pairs. This call provides the value of the property given by name.
        /// </summary>
        /// <param name="osr">The OpenSlide object. </param>
        /// <param name="utf8Name">The name of the desired property. Must be a valid name as given by openslide_get_property_names().</param>
        /// <returns>The value of the named property, or NULL if the property doesn't exist or an error occurred. </returns>
        public static unsafe string? GetPropertyValue(OpenSlideImageSafeHandle osr, ReadOnlySpan<byte> utf8Name)
        {
            if (utf8Name.IsEmpty || utf8Name[utf8Name.Length - 1] != 0)
            {
                throw new ArgumentException();
            }
            fixed (byte* pName = utf8Name)
            {
                IntPtr pResult = GetPropertyValueInternal(osr, (IntPtr)pName);
                return StringFromNativeUtf8(pResult);
            }
        }
    }
}
