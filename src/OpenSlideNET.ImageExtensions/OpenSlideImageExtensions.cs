using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Buffers;

namespace OpenSlideNET
{
    internal static class OpenSlideImageExtensions
    {
        public static Bgra32ImageBuffer ReadImageBuffer(this OpenSlideImage image, int level, long x, long y, long w, long h)
        {
            var buffer = new Bgra32ImageBuffer(checked((int)w), checked((int)h));
            image.ReadRegion(level, x, y, w, h, ref buffer.GetPinnableReference());
            return buffer;
        }

        public static Bgra32ImageBuffer ReadAssociatedImageBuffer(this OpenSlideImage image, string name, long w, long h)
        {
            var buffer = new Bgra32ImageBuffer(checked((int)w), checked((int)h));
            image.ReadAssociatedImage(name, ref buffer.GetPinnableReference());
            return buffer;

        }
    }

    internal readonly struct Bgra32ImageBuffer : IDisposable
    {
        private readonly byte[] _buffer;
        private readonly Image<Bgra32> _image;

        public Bgra32ImageBuffer(int width, int height)
        {
            if (width <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(width));
            }
            if (height <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(height));
            }
            _buffer = ArrayPool<byte>.Shared.Rent(width * height * 4);
            _image = Image.WrapMemory(_buffer.AsMemory().Cast<byte, Bgra32>().Slice(0, width * height), width, height);
        }

        public ref byte GetPinnableReference()
        {
            return ref _buffer[0];
        }

        public Image<Bgra32> GetImage() => _image;

        public void Dispose()
        {
            ArrayPool<byte>.Shared.Return(_buffer);
            _image.Dispose();
        }
    }
}
