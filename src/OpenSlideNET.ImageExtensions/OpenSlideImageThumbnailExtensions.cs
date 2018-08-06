using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Buffers;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace OpenSlideNET
{
    public static class OpenSlideImageThumbnailExtensions
    {
        private static int EnsureMinimumSize(int size)
        {
            return size < 81920 ? 81920 : size;
        }

        private static Image<Bgra32> GenerateThumbnail(OpenSlideImage image, int maxWidth, int maxHeight)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            (long width, long height) = image.Dimemsions;

            // 决定合适的level
            double downsampleWidth = width / (double)maxWidth;
            double downsampleHeight = height / (double)maxHeight;
            double downsample = Math.Max(downsampleWidth, downsampleHeight);
            int level = image.GetBestLevelForDownsample(downsample);
            (long levelWidth, long levelHeight) = image.GetLevelDimemsions(level);

            // 计算目标大小
            int targetWidth, targetHeight;
            if (downsampleWidth > downsampleHeight)
            {
                // width缩小较大，将maxWidth作为标准
                targetWidth = maxWidth;
                targetHeight = (int)(height / downsampleWidth);
            }
            else
            {
                // height缩小较大，将maxHeight作为标准
                targetWidth = (int)(width / downsampleHeight);
                targetHeight = maxHeight;
            }

            Image<Bgra32> output;
            checked
            {
                output = new Image<Bgra32>((int)levelWidth, (int)levelHeight);
            }
            output.Mutate(ctx =>
            {
                ctx.Apply(img =>
                {
                    var frame = img.Frames.RootFrame;

                    image.DangerousReadRegion(
                        level, 0, 0, levelWidth, levelHeight,
                        ref Unsafe.As<Bgra32, byte>(ref frame.DangerousGetPinnableReferenceToPixelBuffer()));
                });
                ctx.Resize(targetWidth, targetHeight);
            });
            return output;
        }

        public static byte[] GetThumbnailAsJpeg(this OpenSlideImage image, int maxSize, int quality = 75)
        {
            using (var thumbnail = GenerateThumbnail(image, maxSize, maxSize))
            {
                byte[] buffer = ArrayPool<byte>.Shared.Rent(EnsureMinimumSize(maxSize * maxSize * 4 * 2));
                try
                {
                    using (var ms = new MemoryStream(buffer))
                    {
                        thumbnail.SaveAsJpeg(ms, new JpegEncoder() { Quality = quality });
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

        public static void GetThumbnailAsJpegToStream(this OpenSlideImage image, int maxSize, Stream stream, int quality = 75)
        {
            using (var thumbnail = GenerateThumbnail(image, maxSize, maxSize))
            {
                byte[] buffer = ArrayPool<byte>.Shared.Rent(EnsureMinimumSize(maxSize * maxSize * 4 * 2));
                try
                {
                    using (var ms = new MemoryStream(buffer))
                    {
                        thumbnail.SaveAsJpeg(ms, new JpegEncoder() { Quality = quality });
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

        public static void GetThumbnailAsJpegToStream(this OpenSlideImage image, int maxSize, MemoryStream stream, int quality = 75)
        {
            using (var thumbnail = GenerateThumbnail(image, maxSize, maxSize))
            {
                byte[] buffer = ArrayPool<byte>.Shared.Rent(EnsureMinimumSize(maxSize * maxSize * 4 * 2));
                try
                {
                    thumbnail.SaveAsJpeg(stream, new JpegEncoder() { Quality = quality });
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(buffer);
                }
            }
        }

        public static async Task GetThumbnailAsJpegToStreamAsync(this OpenSlideImage image, int maxSize, Stream stream, int quality = 75)
        {
            using (var thumbnail = GenerateThumbnail(image, maxSize, maxSize))
            {
                byte[] buffer = ArrayPool<byte>.Shared.Rent(EnsureMinimumSize(maxSize * maxSize * 4 * 2));
                try
                {
                    using (var ms = new MemoryStream(buffer))
                    {
                        thumbnail.SaveAsJpeg(ms, new JpegEncoder() { Quality = quality });
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
