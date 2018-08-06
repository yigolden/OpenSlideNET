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
using static OpenSlideNET.DeepZoomGenerator;

namespace OpenSlideNET
{
    public static class DeepZoomGeneratorFormatExtensions
    {
        private static int EnsureMinimumSize(int size)
        {
            return size < 81920 ? 81920 : size;
        }

        private static void WriteImage(DeepZoomGenerator src, Image<Bgra32> dest, TileInfo tileInfo)
        {
            var frame = dest.Frames.RootFrame;
            src.Image.DangerousReadRegion(
                tileInfo.SlideLevel, tileInfo.X, tileInfo.Y, tileInfo.Width, tileInfo.Height,
                ref Unsafe.As<Bgra32, byte>(ref frame.DangerousGetPinnableReferenceToPixelBuffer()));

            dest.Mutate(ctx =>
            {
                if (tileInfo.ResizeNeeded)
                {
                    ctx.Resize(tileInfo.TileWidth, tileInfo.TileHeight);
                    tileInfo.ResizeNeeded = false;
                }
            });
        }

        public static byte[] GetTileAsJpeg(this DeepZoomGenerator dz, int level, int locationX, int locationY, out TileInfo info, int quality = 75)
        {
            if (dz == null)
            {
                throw new ArgumentNullException(nameof(dz));
            }

            var tileInfo = dz.GetTileInfo(level, locationX, locationY);
            info = tileInfo;
            using (var image = new Image<Bgra32>((int)tileInfo.Width, (int)tileInfo.Height))
            {
                WriteImage(dz, image, tileInfo);

                byte[] buffer = ArrayPool<byte>.Shared.Rent(EnsureMinimumSize(tileInfo.TileWidth * tileInfo.TileHeight * 4 * 2));
                try
                {
                    using (var ms = new MemoryStream(buffer))
                    {
                        image.SaveAsJpeg(ms, new JpegEncoder() { Quality = quality });
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

        public static void GetTileAsJpegToStream(this DeepZoomGenerator dz, int level, int locationX, int locationY, Stream stream, int quality = 75)
        {
            if (dz == null)
            {
                throw new ArgumentNullException(nameof(dz));
            }

            var tileInfo = dz.GetTileInfo(level, locationX, locationY);

            using (var image = new Image<Bgra32>((int)tileInfo.Width, (int)tileInfo.Height))
            {
                WriteImage(dz, image, tileInfo);

                byte[] buffer = ArrayPool<byte>.Shared.Rent(EnsureMinimumSize(tileInfo.TileWidth * tileInfo.TileHeight * 4 * 2));
                try
                {
                    using (var ms = new MemoryStream(buffer))
                    {
                        image.SaveAsJpeg(ms, new JpegEncoder() { Quality = quality });
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

        public static void GetTileAsJpegToStream(this DeepZoomGenerator dz, int level, int locationX, int locationY, MemoryStream stream, int quality = 75)
        {
            if (dz == null)
            {
                throw new ArgumentNullException(nameof(dz));
            }

            var tileInfo = dz.GetTileInfo(level, locationX, locationY);

            using (var image = new Image<Bgra32>((int)tileInfo.Width, (int)tileInfo.Height))
            {
                WriteImage(dz, image, tileInfo);

                image.SaveAsJpeg(stream, new JpegEncoder() { Quality = quality });
            }
        }

        public static async Task GetTileAsJpegToStreamAsync(this DeepZoomGenerator dz, int level, int locationX, int locationY, Stream stream, int quality = 75)
        {
            if (dz == null)
            {
                throw new ArgumentNullException(nameof(dz));
            }

            var tileInfo = dz.GetTileInfo(level, locationX, locationY);

            using (var image = new Image<Bgra32>((int)tileInfo.Width, (int)tileInfo.Height))
            {
                WriteImage(dz, image, tileInfo);

                byte[] buffer = ArrayPool<byte>.Shared.Rent(EnsureMinimumSize(tileInfo.TileWidth * tileInfo.TileHeight * 4 * 2));
                try
                {
                    using (var ms = new MemoryStream(buffer))
                    {
                        image.SaveAsJpeg(ms, new JpegEncoder() { Quality = quality });
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


        public static byte[] GetTileAsPng(this DeepZoomGenerator dz, int level, int locationX, int locationY, out TileInfo info)
        {
            if (dz == null)
            {
                throw new ArgumentNullException(nameof(dz));
            }

            var tileInfo = dz.GetTileInfo(level, locationX, locationY);
            info = tileInfo;
            using (var image = new Image<Bgra32>((int)tileInfo.Width, (int)tileInfo.Height))
            {
                WriteImage(dz, image, tileInfo);

                byte[] buffer = ArrayPool<byte>.Shared.Rent(EnsureMinimumSize(tileInfo.TileWidth * tileInfo.TileHeight * 4 * 2));
                try
                {
                    using (var ms = new MemoryStream(buffer))
                    {
                        image.SaveAsPng(ms);
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

        public static void GetTileAsPngToStream(this DeepZoomGenerator dz, int level, int locationX, int locationY, Stream stream)
        {
            if (dz == null)
            {
                throw new ArgumentNullException(nameof(dz));
            }

            var tileInfo = dz.GetTileInfo(level, locationX, locationY);

            using (var image = new Image<Bgra32>((int)tileInfo.Width, (int)tileInfo.Height))
            {
                WriteImage(dz, image, tileInfo);

                byte[] buffer = ArrayPool<byte>.Shared.Rent(EnsureMinimumSize(tileInfo.TileWidth * tileInfo.TileHeight * 4 * 2));
                try
                {
                    using (var ms = new MemoryStream(buffer))
                    {
                        image.SaveAsPng(ms);
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

        public static void GetTileAsPngToStream(this DeepZoomGenerator dz, int level, int locationX, int locationY, MemoryStream stream)
        {
            if (dz == null)
            {
                throw new ArgumentNullException(nameof(dz));
            }

            var tileInfo = dz.GetTileInfo(level, locationX, locationY);

            using (var image = new Image<Bgra32>((int)tileInfo.Width, (int)tileInfo.Height))
            {
                WriteImage(dz, image, tileInfo);

                image.SaveAsPng(stream);
            }
        }

        public static async Task GetTileAsPngToStreamAsync(this DeepZoomGenerator dz, int level, int locationX, int locationY, Stream stream)
        {
            if (dz == null)
            {
                throw new ArgumentNullException(nameof(dz));
            }

            var tileInfo = dz.GetTileInfo(level, locationX, locationY);

            using (var image = new Image<Bgra32>((int)tileInfo.Width, (int)tileInfo.Height))
            {
                WriteImage(dz, image, tileInfo);

                byte[] buffer = ArrayPool<byte>.Shared.Rent(EnsureMinimumSize(tileInfo.TileWidth * tileInfo.TileHeight * 4 * 2));
                try
                {
                    using (var ms = new MemoryStream(buffer))
                    {
                        image.SaveAsPng(ms);
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
