using OpenSlideNET.Interop;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;

namespace OpenSlideNET
{
    /// <summary>
    /// A user-friendly wrapper class that operates on OpenSlide image.
    /// </summary>
    public sealed class OpenSlideImage : IDisposable
    {
        /// <summary>
        /// Gets the OpenSlide library version.
        /// </summary>
        public static string LibraryVersion => OpenSlideInterop.GetVersion();

        private OpenSlideImageSafeHandle? _handle;
        private readonly bool _leaveOpen;

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenSlideImage"/> class with the specified <see cref="OpenSlideImageSafeHandle"/>.
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="leaveOpen"></param>
        public OpenSlideImage(OpenSlideImageSafeHandle handle, bool leaveOpen = false)
        {
            if (handle is null)
            {
                throw new ArgumentNullException(nameof(handle));
            }
            if (handle.IsInvalid)
            {
                throw new ArgumentException();
            }
            _handle = handle;
            _leaveOpen = leaveOpen;
        }

        /// <summary>
        /// Gets the OpenSlideImageSafeHandle for this object.
        /// </summary>
        public OpenSlideImageSafeHandle SafeHandle => _handle ?? throw new ObjectDisposedException(nameof(OpenSlideImage));

        /// <summary>
        /// Open a whole slide image.
        /// This function can be expensive; avoid calling it unnecessarily. For example, a tile server should not call Open() on every tile request. Instead, it should maintain a cache of <see cref="OpenSlideImage"/> objects and reuse them when possible.
        /// </summary>
        /// <param name="filename">The filename to open.</param>
        /// <returns>The <see cref="OpenSlideImage"/> object.</returns>
        /// <exception cref="OpenSlideUnsupportedFormatException">The file format can not be recognized.</exception>
        /// <exception cref="OpenSlideException">The file format is recognized, but an error occurred when opening the file.</exception>
        public static OpenSlideImage Open(string filename)
        {
            // Open file using OpenSlide
            var handle = OpenSlideInterop.Open(filename);
            if (handle.IsInvalid)
            {
                throw new OpenSlideUnsupportedFormatException();
            }
            if (!ThrowHelper.TryCheckError(handle, out string? errMsg))
            {
                handle.Dispose();
                throw new OpenSlideException(errMsg);
            }
            return new OpenSlideImage(handle);
        }

        /// <summary>
        /// Return a string describing the format vendor of the specified file. This string is also accessible via the PROPERTY_NAME_VENDOR property.
        /// If the file is not recognized, return null.
        /// </summary>
        /// <param name="filename">the file to examine</param>
        /// <returns>the format vendor of the specified file.</returns>
        public static string? DetectFormat(string filename)
        {
            return OpenSlideInterop.DetectVendor(filename);
        }

        /// <summary>
        /// The number of levels in the slide. Levels are numbered from 0 (highest resolution) to level_count - 1 (lowest resolution).
        /// </summary>
        public int LevelCount
        {
            get
            {
                var handle = EnsureNotDisposed();

                int result = OpenSlideInterop.GetLevelCount(handle);
                if (result == -1)
                {
                    ThrowHelper.CheckAndThrowError(handle);
                }
                return result;
            }
        }

        private ImageDimensions? _dimensionsCache = null;
        private ImageDimensions EnsureDimensionsCached()
        {
            var handle = EnsureNotDisposed();

            if (_dimensionsCache == null)
            {
                OpenSlideInterop.GetLevel0Dimensions(handle, out long w, out long h);
                if (w == -1 || h == -1)
                {
                    ThrowHelper.CheckAndThrowError(handle);
                }
                var dimensions = new ImageDimensions(w, h);
                _dimensionsCache = dimensions;
                return dimensions;
            }
            return _dimensionsCache.GetValueOrDefault(); ;
        }

        /// <summary>
        /// A (width, height) tuple for level 0 of the slide.
        /// </summary>
        public ImageDimensions Dimensions
        {
            get
            {
                return EnsureDimensionsCached();
            }
        }

        /// <summary>
        /// Width of the level 0 image of the slide.
        /// </summary>
        public long Width
        {
            get
            {
                return EnsureDimensionsCached().Width;
            }
        }

        /// <summary>
        /// Height of the level 0 image of the slide.
        /// </summary>
        public long Height
        {
            get
            {
                return EnsureDimensionsCached().Height;
            }
        }

        /// <summary>
        /// Get a (width, height) tuple for level k of the slide.
        /// </summary>
        /// <param name="level">the k level</param>
        /// <returns>A (width, height) tuple for level k of the slide.</returns>
        /// <exception cref="OpenSlideException">An error occurred when calling reading the slide or the <see cref="OpenSlideImage"/> was already in the error state.</exception>
        public ImageDimensions GetLevelDimensions(int level)
        {
            var handle =EnsureNotDisposed();

            OpenSlideInterop.GetLevelDimensions(handle, level, out long w, out long h);
            if (w == -1 || h == -1)
            {
                ThrowHelper.CheckAndThrowError(handle);
            }
            return new ImageDimensions(w, h);
        }

        /// <summary>
        /// Get the downsample factor for level k of the slide.
        /// </summary>
        /// <param name="level">the k level</param>
        /// <returns>The downsample factor for level k of the slide.</returns>
        /// <exception cref="OpenSlideException">An error occurred when calling reading the slide or the <see cref="OpenSlideImage"/> was already in the error state.</exception>
        public double GetLevelDownsample(int level)
        {
            var handle = EnsureNotDisposed();

            double result = OpenSlideInterop.GetLevelDownsample(handle, level);
            if (result == -1.0d)
            {
                ThrowHelper.CheckAndThrowError(handle);
            }
            return result;
        }

        /// <summary>
        /// Gets the metadata about the slide.
        /// </summary>
        /// <param name="name">The metadata key name.</param>
        /// <returns>A string containing the metadata value or NULL if there is no such metadata.</returns>
        /// <exception cref="OpenSlideException">An error occurred when calling reading the slide or the <see cref="OpenSlideImage"/> was already in the error state.</exception>
        [IndexerName("Property")]
        public string? this[string name]
        {
            get
            {
                var handle =EnsureNotDisposed();

                string? value = OpenSlideInterop.GetPropertyValue(handle, name);
                ThrowHelper.CheckAndThrowError(handle);
                return value;
            }
        }

        /// <summary>
        /// Gets the comment of the slide.
        /// </summary>
        public string? Comment
        {
            get
            {
                return this[OpenSlideInterop.OpenSlidePropertyNameComment];
            }
        }

        /// <summary>
        /// Gets the vendor of the slide.
        /// </summary>
        public string? Vendor
        {
            get
            {
                return this[OpenSlideInterop.OpenSlidePropertyNameVendor];
            }
        }

        /// <summary>
        /// Gets the quick hash of the slide.
        /// </summary>
        public string? QuickHash1
        {
            get
            {
                return this[OpenSlideInterop.OpenSlidePropertyNameQuickHash1];
            }
        }

        /// <summary>
        /// Gets the background color of the slide.
        /// </summary>
        public string? BackgroundColor
        {
            get
            {
                return this[OpenSlideInterop.OpenSlidePropertyNameBackgroundColor];
            }
        }

        /// <summary>
        /// Gets the objective color of the slide.
        /// </summary>
        public string? ObjectiveColor
        {
            get
            {
                return this[OpenSlideInterop.OpenSlidePropertyNameObjectivePower];
            }
        }

        /// <summary>
        /// Get microns per pixel in the left to right direction.
        /// </summary>
        public double? MicronsPerPixelX
        {
            get
            {
                if (TryGetProperty(OpenSlideInterop.OpenSlidePropertyNameMPPX, out string? value) && double.TryParse(value, out double result))
                {
                    return result;
                }
                return null;
            }
        }

        /// <summary>
        /// Get microns per pixel in the top to bottom direction.
        /// </summary>
        public double? MicronsPerPixelY
        {
            get
            {
                if (TryGetProperty(OpenSlideInterop.OpenSlidePropertyNameMPPY, out string? value) && double.TryParse(value, out double result))
                {
                    return result;
                }
                return null;
            }
        }

        /// <summary>
        /// The X coordinate of the rectangle bounding the non-empty region of the slide, if available.
        /// </summary>
        public long? BoundsX
        {
            get
            {
                if (TryGetProperty(OpenSlideInterop.OpenSlidePropertyNameBoundsX, out string? value) && long.TryParse(value, out long result))
                {
                    return result;
                }
                return null;
            }
        }

        /// <summary>
        /// The Y coordinate of the rectangle bounding the non-empty region of the slide, if available.
        /// </summary>
        public long? BoundsY
        {
            get
            {
                if (TryGetProperty(OpenSlideInterop.OpenSlidePropertyNameBoundsY, out string? value) && long.TryParse(value, out long result))
                {
                    return result;
                }
                return null;
            }
        }

        /// <summary>
        /// The width of the rectangle bounding the non-empty region of the slide, if available.
        /// </summary>
        public long? BoundsWidth
        {
            get
            {
                if (TryGetProperty(OpenSlideInterop.OpenSlidePropertyNameBoundsWidth, out string? value) && long.TryParse(value, out long result))
                {
                    return result;
                }
                return null;
            }
        }

        /// <summary>
        /// The height of the rectangle bounding the non-empty region of the slide, if available.
        /// </summary>
        public long? BoundsHeight
        {
            get
            {
                if (TryGetProperty(OpenSlideInterop.OpenSlidePropertyNameBoundsHeight, out string? value) && long.TryParse(value, out long result))
                {
                    return result;
                }
                return null;
            }
        }

        /// <summary>
        /// Get the array of property names. 
        /// </summary>
        /// <returns>The array of property names</returns>
        public IReadOnlyList<string> GetAllPropertyNames()
        {
            var handle = EnsureNotDisposed();

            string[] properties = OpenSlideInterop.GetPropertyNames(handle);
            ThrowHelper.CheckAndThrowError(handle);
            return properties;
        }

        /// <summary>
        /// Gets the property value.
        /// </summary>
        /// <param name="name">The name of the property.</param>
        /// <param name="value">The value of the property.</param>
        /// <returns>True if the property of the specified name exists. Otherwise, false.</returns>
        public bool TryGetProperty(string name, [NotNullWhen(true)] out string? value)
        {
            var handle = EnsureNotDisposed();

            value = OpenSlideInterop.GetPropertyValue(handle, name);
            return !(value is null);
        }

        /// <summary>
        /// Get the array of names of associated images. 
        /// </summary>
        /// <returns>The array of names of associated images.</returns>
        public IReadOnlyCollection<string> GetAllAssociatedImageNames()
        {
            var handle = EnsureNotDisposed();

            var associatedImages = OpenSlideInterop.GetAssociatedImageNames(handle);
            ThrowHelper.CheckAndThrowError(handle);
            return associatedImages;
        }

        /// <summary>
        /// Gets the dimensions of the associated image.
        /// </summary>
        /// <param name="name">The name of the associated image.</param>
        /// <param name="dimensions">The dimensions of the associated image.</param>
        /// <returns>True if the associated image of the specified name exists. Otherwise, false.</returns>
        public bool TryGetAssociatedImageDimensions(string name, out ImageDimensions dimensions)
        {
            var handle = EnsureNotDisposed();

            OpenSlideInterop.GetAssociatedImageDimensions(handle, name, out long w, out long h);
            if (w != -1 && h != -1)
            {
                dimensions = new ImageDimensions(w, h);
                return true;
            }

            dimensions = default;
            return false;
        }

        /// <summary>
        /// Copy pre-multiplied BGRA data from an associated image.
        /// </summary>
        /// <param name="name">The name of the associated image.</param>
        /// <param name="dimensions">The dimensions of the associated image.</param>
        /// <returns>The pixel data of the associated image.</returns>
        public unsafe byte[] ReadAssociatedImage(string name, out ImageDimensions dimensions)
        {
            if (!TryGetAssociatedImageDimensions(name, out dimensions))
            {
                throw new KeyNotFoundException();
            }

            var data = new byte[dimensions.Width * dimensions.Height * 4];
            if (data.Length > 0)
            {
                fixed (void* pdata = &data[0])
                {
                    ReadAssociatedImageInternal(name, pdata);
                }
            }
            return data;
        }

        /// <summary>
        /// Copy pre-multiplied BGRA data from an associated image.
        /// </summary>
        /// <param name="name">The name of the associated image.</param>
        /// <param name="buffer">The destination buffer to hold the pixel data. Should be at least (width * height * 4) bytes in length</param>
        public unsafe void ReadAssociatedImage(string name, Span<byte> buffer)
        {
            if (!TryGetAssociatedImageDimensions(name, out var dimensions))
            {
                throw new KeyNotFoundException();
            }

            if (buffer.Length < 4 * dimensions.Width * dimensions.Height)
            {
                throw new ArgumentException("Destination is too small.");
            }

            fixed (void* pdata = buffer)
            {
                ReadAssociatedImageInternal(name, pdata);
            }
        }

        /// <summary>
        /// Copy pre-multiplied BGRA data from an associated image.
        /// </summary>
        /// <param name="name">The name of the associated image.</param>
        /// <param name="buffer">The destination buffer to hold the pixel data.</param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public unsafe void ReadAssociatedImage(string name, ref byte buffer)
        {
            fixed (void* pdata = &buffer)
            {
                ReadAssociatedImageInternal(name, pdata);
            }
        }

        /// <summary>
        /// Copy pre-multiplied BGRA data from an associated image.
        /// </summary>
        /// <param name="name">The name of the associated image.</param>
        /// <param name="buffer">The destination buffer to hold the pixel data.</param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public unsafe void ReadAssociatedImage(string name, IntPtr buffer)
        {
            ReadAssociatedImageInternal(name, (void*)buffer);
        }

        private unsafe void ReadAssociatedImageInternal(string name, void* pointer)
        {
            var handle = EnsureNotDisposed();
            OpenSlideInterop.ReadAssociatedImage(handle, name, pointer);
            ThrowHelper.CheckAndThrowError(handle);
        }

        /// <summary>
        /// Copy pre-multiplied BGRA data from a whole slide image.
        /// </summary>
        /// <param name="level">The desired level.</param>
        /// <param name="x">The top left x-coordinate, in the level 0 reference frame.</param>
        /// <param name="y">The top left y-coordinate, in the level 0 reference frame.</param>
        /// <param name="width">The width of the region. Must be non-negative.</param>
        /// <param name="height">The height of the region. Must be non-negative.</param>
        /// <returns>The pixel data of this region.</returns>
        public unsafe byte[] ReadRegion(int level, long x, long y, long width, long height)
        {
            if (width < 0)
                throw new ArgumentOutOfRangeException(nameof(width));
            if (height < 0)
                throw new ArgumentOutOfRangeException(nameof(height));

            var data = new byte[width * height * 4];
            if (data.Length > 0)
            {
                fixed (void* pdata = &data[0])
                {
                    ReadRegionInternal(level, x, y, width, height, pdata);
                }
            }
            return data;
        }

        /// <summary>
        /// Copy pre-multiplied BGRA data from a whole slide image.
        /// </summary>
        /// <param name="level">The desired level.</param>
        /// <param name="x">The top left x-coordinate, in the level 0 reference frame.</param>
        /// <param name="y">The top left y-coordinate, in the level 0 reference frame.</param>
        /// <param name="width">The width of the region. Must be non-negative.</param>
        /// <param name="height">The height of the region. Must be non-negative.</param>
        /// <param name="buffer">The destination buffer for the BGRA data.</param>
        public unsafe void ReadRegion(int level, long x, long y, long width, long height, Span<byte> buffer)
        {
            EnsureNotDisposed();

            if (buffer.Length < 4 * width * height)
            {
                throw new ArgumentException("Destination is too small.");
            }

            fixed (void* pdata = buffer)
            {
                ReadRegionInternal(level, x, y, width, height, pdata);
            }
        }

        /// <summary>
        /// Copy pre-multiplied BGRA data from a whole slide image.
        /// </summary>
        /// <param name="level">The desired level.</param>
        /// <param name="x">The top left x-coordinate, in the level 0 reference frame.</param>
        /// <param name="y">The top left y-coordinate, in the level 0 reference frame.</param>
        /// <param name="width">The width of the region. Must be non-negative.</param>
        /// <param name="height">The height of the region. Must be non-negative.</param>
        /// <param name="buffer">The destination buffer for the BGRA data.</param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public unsafe void ReadRegion(int level, long x, long y, long width, long height, ref byte buffer)
        {
            EnsureNotDisposed();

            fixed (void* pdata = &buffer)
            {
                ReadRegionInternal(level, x, y, width, height, pdata);
            }
        }

        /// <summary>
        /// Copy pre-multiplied BGRA data from a whole slide image.
        /// </summary>
        /// <param name="level">The desired level.</param>
        /// <param name="x">The top left x-coordinate, in the level 0 reference frame.</param>
        /// <param name="y">The top left y-coordinate, in the level 0 reference frame.</param>
        /// <param name="width">The width of the region. Must be non-negative.</param>
        /// <param name="height">The height of the region. Must be non-negative.</param>
        /// <param name="buffer">The destination buffer for the BGRA data.</param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public unsafe void ReadRegion(int level, long x, long y, long width, long height, IntPtr buffer)
        {
            ReadRegionInternal(level, x, y, width, height, (void*)buffer);
        }
        private unsafe void ReadRegionInternal(int level, long x, long y, long width, long height, void* pointer)
        {
            var handle = EnsureNotDisposed();
            OpenSlideInterop.ReadRegion(handle, pointer, x, y, level, width, height);
            ThrowHelper.CheckAndThrowError(handle);
        }

        /// <summary>
        /// Get the best level to use for displaying the given downsample.
        /// </summary>
        /// <param name="downsample">The downsample factor.</param>
        /// <returns>The level identifier, or -1 if an error occurred.</returns>
        public int GetBestLevelForDownsample(double downsample)
        {
            var handle = EnsureNotDisposed();
            return OpenSlideInterop.GetBestLevelForDownsample(handle, downsample);
        }

        /// <summary>
        /// Represents the image dimensions
        /// </summary>
        public readonly struct ImageDimensions
        {
            private readonly long _width;
            private readonly long _height;

            /// <summary>
            /// The width of the image.
            /// </summary>
            public long Width => _width;

            /// <summary>
            /// The height of the image.
            /// </summary>
            public long Height => _height;

            /// <summary>
            /// Initialize a new <see cref="ImageDimensions"/> struct.
            /// </summary>
            /// <param name="width">The width of the image.</param>
            /// <param name="height">The height of the image.</param>
            public ImageDimensions(long width, long height)
            {
                _width = width;
                _height = height;
            }

            /// <summary>
            /// Deconstruction the struct.
            /// </summary>
            /// <param name="width">The width of the image.</param>
            /// <param name="height">The height of the image.</param>
            public void Deconstruct(out long width, out long height)
            {
                width = _width;
                height = _height;
            }

            /// <summary>
            /// Converts the <see cref="ImageDimensions"/> struct into a tuple of (Width, Height).
            /// </summary>
            /// <param name="dimensions">the <see cref="ImageDimensions"/> struct.</param>
            public static implicit operator (long Width, long Height)(ImageDimensions dimensions)
            {
                return (Width: dimensions._width, Height: dimensions._height);
            }

            /// <summary>
            /// Converts a tuple of (Width, Height) into the <see cref="ImageDimensions"/> struct.
            /// </summary>
            /// <param name="dimensions">A tuple of (Width, Height).</param>
            public static explicit operator ImageDimensions(ValueTuple<long, long> dimensions)
            {
                return new ImageDimensions(dimensions.Item1, dimensions.Item2);
            }
        }

        #region IDisposable Support

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private OpenSlideImageSafeHandle EnsureNotDisposed()
        {
            var handle = _handle;
            if (handle is null)
            {
                ThrowObjectDisposedException();
            }
            return handle;
        }

        [DoesNotReturn]
        private static void ThrowObjectDisposedException()
        {
            throw new ObjectDisposedException(nameof(OpenSlideImage));
        }

        /// <summary>
        /// Dispose the <see cref="OpenSlideImage"/> object.
        /// </summary>
        public void Dispose()
        {
            var handle = Interlocked.Exchange(ref _handle, null);
            if (!(handle is null) && !_leaveOpen)
            {
                handle.Dispose();
            }
        }
        #endregion
    }
}
