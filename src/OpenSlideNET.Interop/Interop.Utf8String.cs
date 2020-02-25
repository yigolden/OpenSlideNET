using System;
using System.Runtime.InteropServices;
using System.Text;

namespace OpenSlideNET.Interop
{
    public static partial class OpenSlideInterop
    {
        internal static unsafe string StringFromNativeUtf8(IntPtr nativeUtf8)
        {
            if (nativeUtf8 == IntPtr.Zero)
                return null;
            int len = 0;
            while (*(byte*)(nativeUtf8 + len) != 0)
                ++len;
            return Encoding.UTF8.GetString((byte*)nativeUtf8, len);
        }

        internal ref struct UnsafeUtf8Encoder
        {
            private readonly IntPtr _stackPointer;
            private readonly int _stackSize;
            private GCHandle _handle;

            public unsafe UnsafeUtf8Encoder(byte* stackPointer, int stackSize)
            {
                _stackPointer = (IntPtr)stackPointer;
                _stackSize = stackSize;
                _handle = default;
            }

            public unsafe IntPtr Encode(string value)
            {
                if (value is null)
                {
                    return IntPtr.Zero;
                }

                if (_handle != default)
                {
                    _handle.Free();
                    _handle = default;
                }

                IntPtr pointer = _stackPointer;
                int count = Encoding.UTF8.GetByteCount(value);
                if (count + 1 > _stackSize)
                {
                    var buffer = new byte[count + 1];
                    _handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                    pointer = _handle.AddrOfPinnedObject();
                }
                fixed (char* pValue = value)
                {
                    Encoding.UTF8.GetBytes(pValue, value.Length, (byte*)pointer, count);
                }
                ((byte*)pointer)[count] = 0;
                return pointer;
            }

            public void Dispose()
            {
                if (_handle.IsAllocated)
                {
                    _handle.Free();
                    _handle = default;
                }
            }

        }
    }
}
