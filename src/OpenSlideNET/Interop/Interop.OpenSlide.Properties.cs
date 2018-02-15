using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace OpenSlideNET
{
    internal static partial class Interop
    {

        [DllImport(LibOpenSlide, EntryPoint = "openslide_get_property_names", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr GetPropertyNames_Internal(IntPtr osr);

        /// <summary>
        /// Get the NULL-terminated array of property names. 
        /// Certain vendor-specific metadata properties may exist within a whole slide image. They are encoded as key-value pairs. This call provides a list of names as strings that can be used to read properties with openslide_get_property_value().
        /// </summary>
        /// <param name="osr">The OpenSlide object. </param>
        /// <returns>A NULL-terminated string array of property names, or an empty array if an error occurred. </returns>
        public static unsafe string[] GetPropertyNames(IntPtr osr)
        {
            var list = new List<string>();
            IntPtr* pCurrent = (IntPtr*)GetPropertyNames_Internal(osr);
            while (*pCurrent != IntPtr.Zero)
            {
                string name = StringFromNativeUtf8(*pCurrent);
                list.Add(name);
                pCurrent++;
            }
            return list.ToArray();
        }


        [DllImport(LibOpenSlide, EntryPoint = "openslide_get_property_value", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr GetPropertyValue_Internal(IntPtr osr, IntPtr name);

        /// <summary>
        /// Get the value of a single property. 
        /// Certain vendor-specific metadata properties may exist within a whole slide image. They are encoded as key-value pairs. This call provides the value of the property given by name.
        /// </summary>
        /// <param name="osr">The OpenSlide object. </param>
        /// <param name="name">The name of the desired property. Must be a valid name as given by openslide_get_property_names().</param>
        /// <returns>The value of the named property, or NULL if the property doesn't exist or an error occurred. </returns>
        internal static unsafe string GetPropertyValue(IntPtr osr, string name)
        {
            byte* pointer = stackalloc byte[64];
            UnsafeUtf8Encoder utf8Encoder = new UnsafeUtf8Encoder(pointer, 64);
            try
            {
                IntPtr pResult = GetPropertyValue_Internal(osr, utf8Encoder.Encode(name));
                return StringFromNativeUtf8(pResult);
            }
            finally
            {
                utf8Encoder.Dispose();
            }
        }
    }
}
