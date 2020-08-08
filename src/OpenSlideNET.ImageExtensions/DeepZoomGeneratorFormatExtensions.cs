using PooledGrowableBufferHelper;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using static OpenSlideNET.DeepZoomGenerator;

namespace OpenSlideNET
{
    public static class DeepZoomGeneratorFormatExtensions
    {
        private static int EnsureMinimumSize(int size)
        {
            return size < 81920 ? 81920 : size;
        }

        private static Image<Bgra32> ReadImage(DeepZoomGenerator src, TileInfo tileInfo)
        {
            using var buffer = src.Image.ReadImageBuffer(tileInfo.SlideLevel, tileInfo.X, tileInfo.Y, tileInfo.Width, tileInfo.Height);
            if (tileInfo.Width != tileInfo.TileWidth || tileInfo.Height != tileInfo.TileHeight)
            {
                return buffer.GetImage().Clone(ctx =>
                {
                    ctx.Resize(tileInfo.TileWidth, tileInfo.TileHeight);
                });
            }
            else
            {
                return buffer.GetImage().Clone();
            }
        }

        public static byte[] GetTileAsJpeg(this DeepZoomGenerator dz, int level, int locationX, int locationY, out TileInfo info, int quality = 75)
        {
            if (dz == null)
            {
                throw new ArgumentNullException(nameof(dz));
            }

            info = dz.GetTileInfo(level, locationX, locationY);
            using var image = ReadImage(dz, info);
            using var ms = PooledMemoryStreamManager.Shared.GetStream();
            image.SaveAsJpeg(ms, new JpegEncoder { Quality = quality });
            return ms.ToArray();
        }

        public static void GetTileAsJpegToStream(this DeepZoomGenerator dz, int level, int locationX, int locationY, Stream stream, int quality = 75)
        {
            if (dz == null)
            {
                throw new ArgumentNullException(nameof(dz));
            }

            var info = dz.GetTileInfo(level, locationX, locationY);
            using var image = ReadImage(dz, info);
            using var ms = PooledMemoryStreamManager.Shared.GetStream();
            image.SaveAsJpeg(ms, new JpegEncoder { Quality = quality });
            ms.Seek(0, SeekOrigin.Begin);
            ms.CopyTo(stream);
        }

        public static void GetTileAsJpegToStream(this DeepZoomGenerator dz, int level, int locationX, int locationY, MemoryStream stream, int quality = 75)
        {
            if (dz == null)
            {
                throw new ArgumentNullException(nameof(dz));
            }

            var info = dz.GetTileInfo(level, locationX, locationY);
            using var image = ReadImage(dz, info);
            image.SaveAsJpeg(stream, new JpegEncoder { Quality = quality });
        }

        public static async Task GetTileAsJpegToStreamAsync(this DeepZoomGenerator dz, int level, int locationX, int locationY, Stream stream, int quality = 75, CancellationToken cancellationToken = default)
        {
            if (dz == null)
            {
                throw new ArgumentNullException(nameof(dz));
            }

            var info = dz.GetTileInfo(level, locationX, locationY);
            using var image = ReadImage(dz, info);
            using var ms = PooledMemoryStreamManager.Shared.GetStream();
            image.SaveAsJpeg(ms, new JpegEncoder { Quality = quality });
            ms.Seek(0, SeekOrigin.Begin);
            await ms.CopyToAsync(stream, 4096, cancellationToken).ConfigureAwait(false);
        }


        public static byte[] GetTileAsPng(this DeepZoomGenerator dz, int level, int locationX, int locationY, out TileInfo info)
        {
            if (dz == null)
            {
                throw new ArgumentNullException(nameof(dz));
            }

            info = dz.GetTileInfo(level, locationX, locationY);
            using var image = ReadImage(dz, info);
            using var ms = PooledMemoryStreamManager.Shared.GetStream();
            image.SaveAsPng(ms);
            return ms.ToArray();
        }

        public static void GetTileAsPngToStream(this DeepZoomGenerator dz, int level, int locationX, int locationY, Stream stream)
        {
            if (dz == null)
            {
                throw new ArgumentNullException(nameof(dz));
            }

            var info = dz.GetTileInfo(level, locationX, locationY);
            using var image = ReadImage(dz, info);
            using var ms = PooledMemoryStreamManager.Shared.GetStream();
            image.SaveAsPng(ms);
            ms.Seek(0, SeekOrigin.Begin);
            ms.CopyTo(stream);
        }

        public static void GetTileAsPngToStream(this DeepZoomGenerator dz, int level, int locationX, int locationY, MemoryStream stream)
        {
            if (dz == null)
            {
                throw new ArgumentNullException(nameof(dz));
            }

            var info = dz.GetTileInfo(level, locationX, locationY);
            using var image = ReadImage(dz, info);
            image.SaveAsPng(stream);
        }

        public static async Task GetTileAsPngToStreamAsync(this DeepZoomGenerator dz, int level, int locationX, int locationY, Stream stream, CancellationToken cancellationToken = default)
        {
            if (dz == null)
            {
                throw new ArgumentNullException(nameof(dz));
            }

            var info = dz.GetTileInfo(level, locationX, locationY);
            using var image = ReadImage(dz, info);
            using var ms = PooledMemoryStreamManager.Shared.GetStream();
            image.SaveAsPng(ms);
            ms.Seek(0, SeekOrigin.Begin);
            await ms.CopyToAsync(stream, 4096, cancellationToken).ConfigureAwait(false);
        }
    }
}
