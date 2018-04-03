// See openslide-python (https://github.com/openslide/openslide-python)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace OpenSlideNET
{
    public class DeepZoomGenerator : IDisposable
    {
        private OpenSlideImage _image;
        private readonly bool _disposeImage;
        private readonly int _tileSize;
        private readonly int _overlap;

        private (long x, long y) _l0_offset;
        private (long width, long height)[] _l_dimemsions;
        private List<(long width, long height)> _z_dimemsions;
        private (int cols, int rows)[] _t_dimensions;
        private int _dz_levels;
        private int[] _slide_from_dz_level;
        private double[] _l0_l_downsamples;
        private double[] _l_z_downsamples;

        public OpenSlideImage Image
        {
            get
            {
                EnsureNotDisposed();

                return _image;
            }
        }

        public DeepZoomGenerator(OpenSlideImage image, int tileSize = 254, int overlap = 1, bool limitBounds = true, bool disposeImage = false)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            _image = image;
            _tileSize = tileSize;
            _overlap = overlap;
            _disposeImage = disposeImage;

            // We have four coordinate planes:
            // - Row and column of the tile within the Deep Zoom level (t_)
            // - Pixel coordinates within the Deep Zoom level (z_)
            // - Pixel coordinates within the slide level (l_)
            // - Pixel coordinates within slide level 0 (l0_)

            // Precompute dimensions
            // Slide level and offset
            if (limitBounds)
            {
                // Level 0 coordinate offset
                _l0_offset = (image.BoundsX ?? 0, image.BoundsY ?? 0);
                // Slide level dimensions scale factor in each axis

                long boundsWidth = image.BoundsWidth ?? 0;
                boundsWidth = boundsWidth == 0 ? image.Width : boundsWidth;
                long boundsHeight = image.BoundsHeight ?? 0;
                boundsHeight = boundsHeight == 0 ? image.Height : boundsHeight;
                (double width, double height) _size_scale = (boundsWidth / (double)image.Width, boundsHeight / (double)image.Height);
                // Dimensions of active area
                _l_dimemsions = Enumerable.Range(0, image.LevelCount)
                    .Select(i => image.GetLevelDimemsions(i))
                    .Select(l_size => ((long)Math.Ceiling(l_size.Width * _size_scale.width), (long)Math.Ceiling(l_size.Height * _size_scale.height)))
                    .ToArray();
            }
            else
            {
                _l0_offset = (0, 0);
                _l_dimemsions = Enumerable.Range(0, image.LevelCount)
                    .Select(i => image.GetLevelDimemsions(i))
                    .Select(l_size => (l_size.Width, l_size.Height))
                    .ToArray();
            }
            var _l0_dimemsions = _l_dimemsions[0];
            // Deep Zoom level
            var z_size = _l0_dimemsions;
            var z_dimemsions = new List<(long width, long height)>();
            z_dimemsions.Add(z_size);
            while (z_size.width > 1 || z_size.height > 1)
            {
                z_size = (width: Math.Max(1, (long)Math.Ceiling(z_size.width / 2d)), height: Math.Max(1, (long)Math.Ceiling(z_size.height / 2d)));
                z_dimemsions.Add(z_size);
            }
            z_dimemsions.Reverse();
            _z_dimemsions = z_dimemsions;
            // Tile
            int tiles(long z_lim)
            {
                return (int)Math.Ceiling(z_lim / (double)_tileSize);
            }
            _t_dimensions = _z_dimemsions.Select(z => (tiles(z.width), tiles(z.height))).ToArray();

            // Deep Zoom level count
            _dz_levels = _z_dimemsions.Count;

            // Total downsamples for each Deep Zoom level
            var l0_z_downsamples = Enumerable.Range(0, _dz_levels).Select(dz_level => Math.Pow(2, _dz_levels - dz_level - 1)).ToArray();

            // Preferred slide levels for each Deep Zoom level
            _slide_from_dz_level = l0_z_downsamples.Select(d => image.GetBestLevelForDownsample(d)).ToArray();

            // Piecewise downsamples
            _l0_l_downsamples = Enumerable.Range(0, image.LevelCount).Select(l => image.GetLevelDownsample(l)).ToArray();
            _l_z_downsamples = Enumerable.Range(0, _dz_levels).Select(dz_level => l0_z_downsamples[dz_level] / _l0_l_downsamples[_slide_from_dz_level[dz_level]]).ToArray();
        }

        /// <summary>
        /// The number of Deep Zoom levels in the image.
        /// </summary>
        public int LevelCount
        {
            get
            {
                EnsureNotDisposed();

                return _dz_levels;
            }
        }

        public IReadOnlyList<(int tiles_x, int tiles_y)> LevelTiles
        {
            get
            {
                EnsureNotDisposed();

                return _t_dimensions;
            }
        }

        public IReadOnlyList<(long pixels_x, long pixels_y)> LevelDimemsions
        {
            get
            {
                EnsureNotDisposed();

                return _z_dimemsions;
            }
        }

        public int TileCount
        {
            get
            {
                EnsureNotDisposed();

                return _t_dimensions.Sum(t => t.cols * t.rows);
            }
        }

        public byte[] GetTile(int level, int locationX, int locationY, out TileInfo info)
        {
            EnsureNotDisposed();

            info = GetTileInfo(level, locationX, locationY);
            return _image.ReadRegion(info.SlideLevel, info.X, info.Y, info.Width, info.Height);
        }

        public TileInfo GetTileInfo(int level, int locationX, int locationY)
        {
            EnsureNotDisposed();

            if (level < 0 || level >= _dz_levels)
            {
                throw new ArgumentOutOfRangeException(nameof(level));
            }
            var t_dim = _t_dimensions[level];
            if (locationX < 0 || locationX >= t_dim.cols)
            {
                throw new ArgumentOutOfRangeException(nameof(locationX));
            }
            if (locationY < 0 || locationY >= t_dim.rows)
            {
                throw new ArgumentOutOfRangeException(nameof(locationY));
            }

            // Get preferred slide level
            var slide_level = _slide_from_dz_level[level];

            // Calculate top/left and bottom/right overlap
            int z_overlap_l = locationX != 0 ? 1 : 0;
            int z_overlap_t = locationY != 0 ? 1 : 0;
            int z_overlap_r = locationX != t_dim.cols - 1 ? 1 : 0;
            int z_overlap_b = locationY != t_dim.rows - 1 ? 1 : 0;

            // Get final size of the tile
            var z_dim = _z_dimemsions[level];
            int z_size_x = Math.Min(_tileSize, (int)(z_dim.width - _tileSize * locationX)) + z_overlap_l + z_overlap_r;
            int z_size_y = Math.Min(_tileSize, (int)(z_dim.height - _tileSize * locationY)) + z_overlap_t + z_overlap_b;

            // Obtain the region coordinates
            var z_location_x = _z_from_t(locationX);
            var z_location_y = _z_from_t(locationY);
            var l_location_x = _l_from_z(level, z_location_x - z_overlap_l);
            var l_location_y = _l_from_z(level, z_location_y - z_overlap_t);

            // Round location down and size up, and add offset of active area
            var l0_location_x = (long)(_l0_from_l(slide_level, l_location_x) + _l0_offset.x);
            var l0_location_y = (long)(_l0_from_l(slide_level, l_location_y) + _l0_offset.y);
            var l_dim = _l_dimemsions[slide_level];
            var l_size_x = (long)Math.Min(Math.Ceiling(_l_from_z(level, z_size_x)), l_dim.width - Math.Ceiling(l_location_x));
            var l_size_y = (long)Math.Min(Math.Ceiling(_l_from_z(level, z_size_y)), l_dim.height - Math.Ceiling(l_location_y));

            return new TileInfo
            {
                X = l0_location_x,
                Y = l0_location_y,
                SlideLevel = slide_level,
                Width = l_size_x,
                Height = l_size_y,
                TileWidth = z_size_x,
                TileHeight = z_size_y,
                ResizeNeeded = l_size_x != z_size_x || l_size_y != z_size_y
            };
        }

        [Obsolete("Use GetTileInfo instead.")]
        public TileInfo GetTileCoordinates(int level, int locationX, int locationY)
            => GetTileInfo(level, locationX, locationY);

        private const string DziTemplete = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><Image xmlns=\"http://schemas.microsoft.com/deepzoom/2008\" Format=\"{FORMAT}\" Overlap=\"{OVERLAP}\" TileSize=\"{TILESIZE}\"><Size Height=\"{HEIGHT}\" Width=\"{WIDTH}\" /></Image>";
        public string GetDzi(string format = "jpeg")
        {
            EnsureNotDisposed();

            var (width, height) = _l_dimemsions[0];
            var sb = new StringBuilder(DziTemplete);
            sb.Replace("{FORMAT}", format);
            sb.Replace("{OVERLAP}", _overlap.ToString());
            sb.Replace("{TILESIZE}", _tileSize.ToString());
            sb.Replace("{HEIGHT}", height.ToString());
            sb.Replace("{WIDTH}", width.ToString());
            return sb.ToString();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private double _l0_from_l(int slide_level, double l)
        {
            return _l0_l_downsamples[slide_level] * l;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private double _l_from_z(int dz_level, int z)
        {
            return _l_z_downsamples[dz_level] * z;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int _z_from_t(int t)
        {
            return _tileSize * t;
        }

        #region IDisposable Support
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void EnsureNotDisposed()
        {
            if (_image == null)
            {
                throw new ObjectDisposedException(nameof(DeepZoomGenerator));
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_image != null && disposing && _disposeImage)
            {
                var obj = Interlocked.Exchange(ref _image, null);
                if (obj != null)
                {
                    obj.Dispose();
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion

        public class TileInfo
        {
            public long X { get; set; }
            public long Y { get; set; }
            public int SlideLevel { get; set; }
            public long Width { get; set; }
            public long Height { get; set; }
            public int TileWidth { get; set; }
            public int TileHeight { get; set; }
            public bool ResizeNeeded { get; set; }
        }
    }
}
