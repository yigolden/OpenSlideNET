using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace OpenSlideNET
{
    public static class OpenSlideImageAssociatedImageExtensions
    {
        private static int EnsureMinimumSize(int size)
        {
            return size < 81920 ? 81920 : size;
        }

        private static Image<Bgra32> WriteImage(OpenSlideImage image, string name)
        {
            if (!image.TryGetAssociatedImageDimemsions(name, out var dims))
            {
                throw new KeyNotFoundException();
            }
            var dest = new Image<Bgra32>((int)dims.Width, (int)dims.Height);
            var frame = dest.Frames.RootFrame;
            image.DangerousReadAssociatedImage(name, ref Unsafe.As<Bgra32, byte>(ref frame.DangerousGetPinnableReferenceToPixelBuffer()));
            return dest;
        }

        public static byte[] GetAssociatedImageAsJpeg(this OpenSlideImage image, string name, int quality = 75)
        {
            using (var img = WriteImage(image, name))
            {
                byte[] buffer = ArrayPool<byte>.Shared.Rent(EnsureMinimumSize(img.Width * img.Height * 4 * 2));
                try
                {
                    using (var ms = new MemoryStream(buffer))
                    {
                        img.SaveAsJpeg(ms, new JpegEncoder() { Quality = quality });
                        ms.SetLength(ms.Position);
                        return ms.ToArray();
                    }
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(buffer);
                }
            }
        }

        public static void GetAssociatedImageAsJpegToStream(this OpenSlideImage image, string name, Stream stream, int quality = 75)
        {
            using (var img = WriteImage(image, name))
            {
                byte[] buffer = ArrayPool<byte>.Shared.Rent(EnsureMinimumSize(img.Width * img.Height * 4 * 2));
                try
                {
                    using (var ms = new MemoryStream(buffer))
                    {
                        img.SaveAsJpeg(ms, new JpegEncoder() { Quality = quality });
                        ms.SetLength(ms.Position);
                        ms.Position = 0;
                        ms.CopyTo(stream);
                    }
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(buffer);
                }
            }
        }

        public static void GetAssociatedImageAsJpegToStream(this OpenSlideImage image,string name, MemoryStream stream, int quality = 75)
        {
            using (var img = WriteImage(image, name))
            {
                img.SaveAsJpeg(stream, new JpegEncoder() { Quality = quality });
            }
        }

        public static async Task GetAssociatedImageAsJpegToStreamAsync(this OpenSlideImage image, string name, Stream stream, int quality = 75)
        {
            using (var img = WriteImage(image, name))
            {
                byte[] buffer = ArrayPool<byte>.Shared.Rent(EnsureMinimumSize(img.Width * img.Height * 4 * 2));
                try
                {
                    using (var ms = new MemoryStream(buffer))
                    {
                        img.SaveAsJpeg(ms, new JpegEncoder() { Quality = quality });
                        ms.SetLength(ms.Position);
                        ms.Position = 0;
                        await ms.CopyToAsync(stream).ConfigureAwait(false);
                    }
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(buffer);
                }
            }
        }
    }
}
