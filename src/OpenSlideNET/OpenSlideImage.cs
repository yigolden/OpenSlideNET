using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;

namespace OpenSlideNET
{
    public sealed class OpenSlideImage : IDisposable
    {
        public static string LibraryVersion => Interop.GetVersion();

        private IntPtr _handle;
        private FileInfo _fileInfo;

        internal OpenSlideImage(IntPtr handle, FileInfo fileInfo)
        {
            Debug.Assert(handle != IntPtr.Zero);
            _handle = handle;
            _fileInfo = fileInfo;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public IntPtr Handle => _handle;

        public DateTime LastWriteTimeUtc => _fileInfo.LastWriteTimeUtc;

        public static OpenSlideImage Open(string filename)
        {
            FileInfo fileInfo = new FileInfo(filename);
            // Open file using OpenSlide
            IntPtr handle = Interop.Open(filename);
            if (handle == IntPtr.Zero)
            {
                throw new OpenSlideUnsupportedFormatException();
            }
            if (!ThrowHelper.TryCheckError(handle, out string errMsg))
            {
                Interop.Close(handle);
                throw new OpenSlideException(errMsg);
            }
            return new OpenSlideImage(handle, fileInfo);
        }

        /// <summary>
        /// Return a string describing the format vendor of the specified file. This string is also accessible via the PROPERTY_NAME_VENDOR property.
        /// If the file is not recognized, return null.
        /// </summary>
        /// <param name="filename">the file to examine</param>
        /// <returns>the format vendor of the specified file.</returns>
        public static string DetectFormat(string filename)
        {
            return Interop.DetectVendor(filename);
        }

        /// <summary>
        /// The number of levels in the slide. Levels are numbered from 0 (highest resolution) to level_count - 1 (lowest resolution).
        /// </summary>
        public int LevelCount
        {
            get
            {
                EnsureNotDisposed();

                int result = Interop.GetLevelCount(_handle);
                if (result == -1)
                {
                    ThrowHelper.CheckAndThrowError(_handle);
                }
                return result;
            }
        }

        private ImageDimemsions? _dimemsionsCache = null;
        private void EnsureDimemsionsCached()
        {
            if (_dimemsionsCache == null)
            {
                Interop.GetLevel0Dimensions(_handle, out long w, out long h);
                if (w == -1 || h == -1)
                {
                    ThrowHelper.CheckAndThrowError(_handle);
                }
                _dimemsionsCache = new ImageDimemsions(w, h);
            }
        }
        /// <summary>
        /// A (width, height) tuple for level 0 of the slide.
        /// </summary>
        public ImageDimemsions Dimemsions
        {
            get
            {
                EnsureNotDisposed();
                EnsureDimemsionsCached();

                return _dimemsionsCache.Value;
            }
        }
        public long Width
        {
            get
            {
                EnsureNotDisposed();
                EnsureDimemsionsCached();

                return _dimemsionsCache.Value.Width;
            }
        }
        public long Height
        {
            get
            {
                EnsureNotDisposed();
                EnsureDimemsionsCached();

                return _dimemsionsCache.Value.Height;
            }
        }

        /// <summary>
        /// Get a (width, height) tuple for level k of the slide.
        /// </summary>
        /// <param name="level">the k level</param>
        /// <returns>A (width, height) tuple for level k of the slide.</returns>
        public ImageDimemsions GetLevelDimemsions(int level)
        {
            EnsureNotDisposed();

            Interop.GetLevelDimensions(_handle, level, out long w, out long h);
            if (w == -1 || h == -1)
            {
                ThrowHelper.CheckAndThrowError(_handle);
            }
            return new ImageDimemsions(w, h);
        }

        /// <summary>
        /// Get the downsample factor for level k of the slide.
        /// </summary>
        /// <param name="level">the k level</param>
        /// <returns>The downsample factor for level k of the slide.</returns>
        public double GetLevelDownsample(int level)
        {
            EnsureNotDisposed();

            double result = Interop.GetLevelDownsample(_handle, level);
            if (result == -1.0d)
            {
                ThrowHelper.CheckAndThrowError(_handle);
            }
            return result;
        }

        /// <summary>
        /// Metadata about the slide.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        [IndexerName("Property")]
        public string this[string name]
        {
            get
            {
                EnsureNotDisposed();

                string value = Interop.GetPropertyValue(_handle, name);
                ThrowHelper.CheckAndThrowError(_handle);
                return value;
            }
        }
        public string Comment
        {
            get
            {
                return this[Interop.OpenSlidePropertyNameComment];
            }
        }

        public string Vendor
        {
            get
            {
                return this[Interop.OpenSlidePropertyNameVendor];
            }
        }

        public string QuickHash1
        {
            get
            {
                return this[Interop.OpenSlidePropertyNameQuickHash1];
            }
        }

        public string BackgroundColor
        {
            get
            {
                return this[Interop.OpenSlidePropertyNameBackgroundColor];
            }
        }

        public string ObjectiveColor
        {
            get
            {
                return this[Interop.OpenSlidePropertyNameObjectivePower];
            }
        }

        /// <summary>
        /// Get microns per pixel in the left to right direction.
        /// </summary>
        public double? MicronsPerPixelX
        {
            get
            {
                if (TryGetProperty(Interop.OpenSlidePropertyNameMPPX, out string value) && double.TryParse(value, out double result))
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
                if (TryGetProperty(Interop.OpenSlidePropertyNameMPPY, out string value) && double.TryParse(value, out double result))
                {
                    return result;
                }
                return null;
            }
        }

        public long? BoundsX
        {
            get
            {
                if (TryGetProperty(Interop.OpenSlidePropertyNameBoundsX, out string value) && long.TryParse(value, out long result))
                {
                    return result;
                }
                return null;
            }
        }

        public long? BoundsY
        {
            get
            {
                if (TryGetProperty(Interop.OpenSlidePropertyNameBoundsY, out string value) && long.TryParse(value, out long result))
                {
                    return result;
                }
                return null;
            }
        }

        public long? BoundsWidth
        {
            get
            {
                if (TryGetProperty(Interop.OpenSlidePropertyNameBoundsWidth, out string value) && long.TryParse(value, out long result))
                {
                    return result;
                }
                return null;
            }
        }

        public long? BoundsHeight
        {
            get
            {
                if (TryGetProperty(Interop.OpenSlidePropertyNameBoundsHeight, out string value) && long.TryParse(value, out long result))
                {
                    return result;
                }
                return null;
            }
        }

        /// <summary>
        /// Get the array of property names. 
        /// </summary>
        /// <returns></returns>
        public IReadOnlyList<string> GetAllPropertyNames()
        {
            EnsureNotDisposed();

            string[] properties = Interop.GetPropertyNames(_handle);
            ThrowHelper.CheckAndThrowError(_handle);
            return properties;
        }

        public bool TryGetProperty(string name, out string value)
        {
            EnsureNotDisposed();

            value = Interop.GetPropertyValue(_handle, name);
            return value != null;
        }

        public IReadOnlyCollection<string> GetAllAssociatedImageNames()
        {
            EnsureNotDisposed();

            var associatedImages = Interop.GetAssociatedImageNames(_handle);
            ThrowHelper.CheckAndThrowError(_handle);
            return associatedImages;
        }

        public bool TryGetAssociatedImageDimemsions(string name, out ImageDimemsions dims)
        {
            EnsureNotDisposed();

            Interop.GetAssociatedImageDimemsions(_handle, name, out long w, out long h);
            if (w != -1 && h != -1)
            {
                dims = new ImageDimemsions(w, h);
                return true;
            }

            dims = default;
            return false;
        }

        public byte[] ReadAssociatedImageToArray(string name, out ImageDimemsions dimemsions)
        {
            EnsureNotDisposed();

            if (!TryGetAssociatedImageDimemsions(name, out dimemsions))
            {
                throw new KeyNotFoundException();
            }

            var data = new byte[dimemsions.Width * dimemsions.Height * 4];
            if (data.Length > 0)
            {
                ReadAssociatedImage(name, ref data[0]);
            }
            return data;
        }
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void DangerousReadAssociatedImageToBuffer(string name, ref byte buffer)
        {
            EnsureNotDisposed();

            ReadAssociatedImage(name, ref buffer);
        }
        private unsafe void ReadAssociatedImage(string name, ref byte data)
        {
            fixed (void* pdata = &data)
            {
                Interop.ReadAssociatedImage(_handle, name, (IntPtr)pdata);
            }
            ThrowHelper.CheckAndThrowError(_handle);
        }

        public byte[] ReadRegionToArray(int level, long x, long y, long width, long height)
        {
            EnsureNotDisposed();

            if (width < 0)
                throw new ArgumentOutOfRangeException(nameof(width));
            if (height < 0)
                throw new ArgumentOutOfRangeException(nameof(height));

            var data = new byte[width * height * 4];
            ReadRegion(level, x, y, width, height, ref data[0]);
            return data;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void DangerousReadRegionToBuffer(int level, long x, long y, long width, long height, ref byte buffer)
        {
            EnsureNotDisposed();

            ReadRegion(level, x, y, width, height, ref buffer);
        }

        private unsafe void ReadRegion(int level, long x, long y, long width, long height, ref byte data)
        {
            fixed (void* pdata = &data)
            {
                Interop.ReadRegion(_handle, (IntPtr)pdata, x, y, level, width, height);
            }
            ThrowHelper.CheckAndThrowError(_handle);
        }

        public int GetBestLevelForDownsample(double downsample)
        {
            EnsureNotDisposed();

            return Interop.GetBestLevelForDownsample(_handle, downsample);
        }


        public readonly struct ImageDimemsions
        {
            private readonly long _width;
            private readonly long _height;

            public long Width => _width;
            public long Height => _height;

            public ImageDimemsions(long width, long height)
            {
                _width = width;
                _height = height;
            }

            public void Deconstruct(out long width, out long height)
            {
                width = _width;
                height = _height;
            }

            public static implicit operator (long Width, long Height)(ImageDimemsions dimemsions)
            {
                return (Width: dimemsions._width, Height: dimemsions._height);
            }

            public static explicit operator ImageDimemsions(ValueTuple<long, long> dimemsions)
            {
                return new ImageDimemsions(dimemsions.Item1, dimemsions.Item2);
            }
        }

        #region IDisposable Support

        public bool IsDisposed => _handle == IntPtr.Zero;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EnsureNotDisposed()
        {
            if (_handle == IntPtr.Zero)
            {
                throw new ObjectDisposedException(nameof(OpenSlideImage));
            }
        }

        void Dispose(bool disposing)
        {
            var handle = Interlocked.Exchange(ref _handle, IntPtr.Zero);
            if (handle != IntPtr.Zero)
            {
                Interop.Close(handle);
            }
        }

        // 仅当以上 Dispose(bool disposing) 拥有用于释放未托管资源的代码时才替代终结器。
        ~OpenSlideImage()
        {
            // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
            Dispose(false);
        }

        // 添加此代码以正确实现可处置模式。
        public void Dispose()
        {
            // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
            Dispose(true);
            // 如果在以上内容中替代了终结器，则取消注释以下行。
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
