using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System.Buffers
{
    internal sealed class UnmanagedTypeCastMemoryManager<TFrom, TTo> : MemoryManager<TTo> where TFrom : unmanaged where TTo : unmanaged
    {
        private readonly Memory<TFrom> _memory;

        public UnmanagedTypeCastMemoryManager(Memory<TFrom> memory)
        {
            _memory = memory;
        }

        public override Span<TTo> GetSpan()
        {
            return MemoryMarshal.Cast<byte, TTo>(MemoryMarshal.AsBytes(_memory.Span));
        }

        public override MemoryHandle Pin(int elementIndex = 0)
        {
            return _memory.Slice(elementIndex * Unsafe.SizeOf<TTo>() / Unsafe.SizeOf<TFrom>()).Pin();
        }

        public override void Unpin() { }

        protected override void Dispose(bool disposing) { }
    }

    internal static class ValueTypeCastMemoryManagerExtensions
    {
        public static Memory<TTo> Cast<TFrom, TTo>(this MemoryManager<TFrom> memoryManager) where TFrom : unmanaged where TTo : unmanaged
        {
            if (typeof(TFrom) == typeof(TTo))
            {
                return Unsafe.As<MemoryManager<TTo>>(memoryManager).Memory;
            }
            return new UnmanagedTypeCastMemoryManager<TFrom, TTo>(memoryManager.Memory).Memory;
        }

        public static Memory<TTo> Cast<TFrom, TTo>(this Memory<TFrom> memory) where TFrom : unmanaged where TTo : unmanaged
        {
            if (typeof(TFrom) == typeof(TTo))
            {
                return Unsafe.As<Memory<TFrom>, Memory<TTo>>(ref memory);
            }
            return new UnmanagedTypeCastMemoryManager<TFrom, TTo>(memory).Memory;
        }
    }
}
