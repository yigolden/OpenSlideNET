using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenSlideNET
{
    /// <summary>
    /// Deep zoom generator for OpenSlide image.
    /// </summary>
    public class DeepZoomGenerator
    {
        private OpenSlideImage? _image;
        private readonly bool _disposeImage;
        private readonly long _boundX;
        private readonly long _boundY;
        private readonly long _width;
        private readonly long _height;
        private readonly int _tileSize;
        private readonly int _overlap;
        private readonly DeepZoomLayerInformation[] _layers;

        /// <summary>
        /// Get the underlying OpenSlide image object.
        /// </summary>
        public OpenSlideImage Image => EnsureNotDisposed();

        /// <summary>
        /// Initialize a <see cref="DeepZoomGenerator"/> instance with the specified parameters.
        /// </summary>
        /// <param name="image">The OpenSlide image.</param>
        /// <param name="tileSize">The tile size paramter.</param>
        /// <param name="overlap">The overlap paramter.</param>
        /// <param name="limitBounds">Whether image bounds should be respected.</param>
        /// <param name="disposeImage">Whether the OpenSlide image instance should be disposed when this <see cref="DeepZoomGenerator"/> instance is disposed.</param>
        public DeepZoomGenerator(OpenSlideImage image, int tileSize = 254, int overlap = 1, bool limitBounds = true, bool disposeImage = false)
        {
            _image = image ?? throw new ArgumentNullException(nameof(image));
            _disposeImage = disposeImage;

            long width, height;
            if (limitBounds)
            {
                _boundX = image.BoundsX ?? 0;
                _boundY = image.BoundsY ?? 0;
                _width = width = image.BoundsWidth ?? image.Width;
                _height = height = image.BoundsHeight ?? image.Height;
            }
            else
            {
                _boundX = 0;
                _boundY = 0;
                _width = width = image.Width;
                _height = height = image.Height;
            }

            DeepZoomLayer[] dzLayers = DeepZoomHelper.CalculateDeepZoomLayers(width, height);
            DeepZoomLayerInformation[] layers = new DeepZoomLayerInformation[dzLayers.Length];

            for (int i = 0; i < layers.Length; i++)
            {
                DeepZoomLayer dzLayer = dzLayers[i];
                int layerDownsample = 1 << (dzLayers.Length - i - 1);

                int level = image.GetBestLevelForDownsample(layerDownsample);
                (long levelWidth, long levelHeight) = image.GetLevelDimensions(level);
                layers[i] = new DeepZoomLayerInformation
                {
                    Level = level,
                    LevelDownsample = image.GetLevelDownsample(level),
                    LevelWidth = levelWidth,
                    LevelHeight = levelHeight,
                    LayerDownsample = layerDownsample,
                    LayerWidth = dzLayer.Width,
                    LayerHeight = dzLayer.Height
                };
            }

            _layers = layers;
            _tileSize = tileSize;
            _overlap = overlap;
        }

        /// <summary>
        /// The Count of the deep zoom level.
        /// </summary>
        public int LevelCount => _layers.Length;

        private IEnumerable<(int, int)>? _levelTilesCache;

        /// <summary>
        /// The number of tiles in each level.
        /// </summary>
        public IEnumerable<(int HorizontalTileCount, int VerticalTileCount)> LevelTiles
            => _levelTilesCache is null ? _levelTilesCache = _layers.Select(l => ((int)((l.LayerWidth + _tileSize - 1) / _tileSize), (int)((l.LayerHeight + _tileSize - 1) / _tileSize))) : _levelTilesCache;

        private IEnumerable<(long, long)>? _levelDimensionsCache;

        /// <summary>
        /// The dimensions of each level.
        /// </summary>
        public IEnumerable<(long Width, long Height)> LevelDimensions
            => _levelDimensionsCache is null ? _levelDimensionsCache = _layers.Select(l => (l.LayerWidth, l.LayerHeight)) : _levelDimensionsCache;

        private int? _tileCountCache;

        /// <summary>
        /// The total number of tiles.
        /// </summary>
        public int TileCount
            => _tileCountCache.HasValue ? _tileCountCache.GetValueOrDefault() : (_tileCountCache = (int)_layers.Sum(l => ((l.LayerWidth + _tileSize - 1) / _tileSize) * ((l.LayerHeight + _tileSize - 1) / _tileSize))).GetValueOrDefault();

        /// <summary>
        /// Get the pre-multiplied BGRA data for the specified tile.
        /// </summary>
        /// <param name="level">The deep zoom level.</param>
        /// <param name="locationX">Horizontal tile index.</param>
        /// <param name="locationY">Vertical tile index.</param>
        /// <param name="info">Information of the specified tile.</param>
        /// <returns>Pre-multiplied BGRA image data.</returns>
        public byte[] GetTile(int level, int locationX, int locationY, out TileInfo info)
        {
            var image = EnsureNotDisposed();

            info = GetTileInfo(level, locationX, locationY);
            return image.ReadRegion(info.SlideLevel, info.X, info.Y, info.Width, info.Height);
        }

        /// <summary>
        /// Get information of the specified tile.
        /// </summary>
        /// <param name="level">The deep zoom level.</param>
        /// <param name="locationX">Horizontal tile index.</param>
        /// <param name="locationY">Vertical tile index.</param>
        /// <returns>Information of the specified tile.</returns>
        public TileInfo GetTileInfo(int level, int locationX, int locationY)
        {
            EnsureNotDisposed();

            var layers = _layers;
            if ((uint)level >= (uint)layers.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(level));
            }
            var layer = layers[level];
            int tileSize = _tileSize;
            int overlap = _overlap;
            int horizontalTileCount = (int)((layer.LayerWidth + tileSize - 1) / tileSize);
            int verticalTileCount = (int)((layer.LayerHeight + tileSize - 1) / tileSize);
            if ((uint)locationX >= (uint)horizontalTileCount)
            {
                throw new ArgumentOutOfRangeException(nameof(locationX));
            }
            if ((uint)locationY >= (uint)verticalTileCount)
            {
                throw new ArgumentOutOfRangeException(nameof(locationY));
            }

            long offsetX = (long)locationX * tileSize;
            long offsetY = (long)locationY * tileSize;
            int width = (int)Math.Min(tileSize, layer.LayerWidth - offsetX);
            int height = (int)Math.Min(tileSize, layer.LayerHeight - offsetY); ;
            if (locationX != 0)
            {
                offsetX -= overlap;
                width += overlap;
            }
            if (locationX != horizontalTileCount - 1)
            {
                width += overlap;
            }
            if (locationY != 0)
            {
                offsetY -= overlap;
                height += overlap;
            }
            if (locationY != verticalTileCount - 1)
            {
                height += overlap;
            }

            return new TileInfo
            {
                X = _boundX + offsetX * layer.LayerDownsample,
                Y = _boundY + offsetY * layer.LayerDownsample,
                SlideLevel = layer.Level,
                Width = (long)(width * layer.LayerDownsample / layer.LevelDownsample),
                Height = (long)(height * layer.LayerDownsample / layer.LevelDownsample),
                TileWidth = width,
                TileHeight = height
            };
        }

        /// <summary>
        /// Get the dzi file content.
        /// </summary>
        /// <param name="format">The iamge format.</param>
        /// <returns>Dzi file content.</returns>
        public string GetDzi(string format = "jpeg")
        {
            EnsureNotDisposed();

            return "<?xml version=\"1.0\" encoding=\"UTF-8\"?><Image xmlns=\"http://schemas.microsoft.com/deepzoom/2008\" Format=\"" + format + "\" Overlap=\"" + _overlap + "\" TileSize=\"" + _tileSize + "\"><Size Height=\"" + _height + "\" Width=\"" + _width + "\" /></Image>";
        }

        private OpenSlideImage EnsureNotDisposed()
        {
            var image = _image;
            if (image is null)
            {
                throw new ObjectDisposedException(nameof(DeepZoomGenerator));
            }
            return image;
        }

        /// <summary>
        /// Dispose this instance.
        /// </summary>
        public void Dispose()
        {
            if (_disposeImage && !(_image is null))
            {
                _image.Dispose();
                _image = null;
            }
        }

        /// <summary>
        /// Information of a tile.
        /// </summary>
        public class TileInfo
        {
            /// <summary>
            /// The X coordinate in the base level.
            /// </summary>
            public long X { get; set; }


            /// <summary>
            /// The Y coordinate in the base level.
            /// </summary>
            public long Y { get; set; }

            /// <summary>
            /// The corresponding level in the OpenSlide image.
            /// </summary>
            public int SlideLevel { get; set; }

            /// <summary>
            /// Width of the image to read from OpenSlide image.
            /// </summary>
            public long Width { get; set; }

            /// <summary>
            /// Height of the image to read from OpenSlide image.
            /// </summary>
            public long Height { get; set; }

            /// <summary>
            /// The width of the deep zoom tile.
            /// </summary>
            public int TileWidth { get; set; }


            /// <summary>
            /// The height of the deep zoom tile.
            /// </summary>
            public int TileHeight { get; set; }
        }

        class DeepZoomLayerInformation
        {
            public int Level { get; set; }
            public long LevelWidth { get; set; }
            public long LevelHeight { get; set; }
            public double LevelDownsample { get; set; }
            public int LayerDownsample { get; set; }
            public long LayerWidth { get; set; }
            public long LayerHeight { get; set; }
        }
    }
}
