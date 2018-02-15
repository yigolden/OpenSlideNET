using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace OpenSlideNET
{
    public sealed class OpenSlideImage : IDisposable
    {
        public static string LibraryVersion => Interop.GetVersion();

        private IntPtr _handle;
        private int _retainedCount;
        private bool _disposed;
        // Don't make the SpinLock variable readonly because it is being mutated.
        private SpinLock _lock = new SpinLock();
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
            if (!fileInfo.Exists)
            {
                throw new FileNotFoundException();
            }
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
        public double GetLevelDownsamples(int level)
        {
            EnsureNotDisposed();

            double result = Interop.GetLevelDownsample(_handle, level);
            if (result == -1.0d)
            {
                ThrowHelper.CheckAndThrowError(_handle);
            }
            return result;
        }

        private string[] _properties;
        private void EnsurePropertyListAcquired()
        {
            if (_properties == null)
            {
                _properties = Interop.GetPropertyNames(_handle);
                ThrowHelper.CheckAndThrowError(_handle);
            }
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
                EnsureNotDisposed();

                return this[Interop.OpenSlidePropertyNameComment];
            }
        }

        public string Vendor
        {
            get
            {
                EnsureNotDisposed();

                return this[Interop.OpenSlidePropertyNameVendor];
            }
        }

        public string QuickHash1
        {
            get
            {
                EnsureNotDisposed();

                return this[Interop.OpenSlidePropertyNameQuickHash1];
            }
        }

        public string BackgroundColor
        {
            get
            {
                EnsureNotDisposed();

                return this[Interop.OpenSlidePropertyNameBackgroundColor];
            }
        }

        public string ObjectiveColor
        {
            get
            {
                EnsureNotDisposed();

                return this[Interop.OpenSlidePropertyNameObjectivePower];
            }
        }

        public string MPPX
        {
            get
            {
                EnsureNotDisposed();

                return this[Interop.OpenSlidePropertyNameMPPX];
            }
        }

        public string MPPY
        {
            get
            {
                EnsureNotDisposed();

                return this[Interop.OpenSlidePropertyNameMPPY];
            }
        }

        public long BoundsX
        {
            get
            {
                EnsureNotDisposed();

                return OpenSlideImagePropertyExtensions.GetProperty(this, Interop.OpenSlidePropertyNameBoundsX, (long)0);
            }
        }

        public long BoundsY
        {
            get
            {
                EnsureNotDisposed();

                return OpenSlideImagePropertyExtensions.GetProperty(this, Interop.OpenSlidePropertyNameBoundsY, (long)0);
            }
        }

        public long BoundsWidth
        {
            get
            {
                EnsureNotDisposed();
                EnsureDimemsionsCached();

                return OpenSlideImagePropertyExtensions.GetProperty(this, Interop.OpenSlidePropertyNameBoundsWidth, (long)0);
            }
        }

        public long BoundsHeight
        {
            get
            {
                EnsureNotDisposed();
                EnsureDimemsionsCached();

                return OpenSlideImagePropertyExtensions.GetProperty(this, Interop.OpenSlidePropertyNameBoundsHeight, (long)0);
            }
        }

        /// <summary>
        /// Get the array of property names. 
        /// </summary>
        /// <returns></returns>
        public IReadOnlyList<string> GetAllPropertyNames()
        {
            EnsureNotDisposed();

            _properties = Interop.GetPropertyNames(_handle);
            ThrowHelper.CheckAndThrowError(_handle);
            return _properties;
        }

        public bool TryGetProperty(string name, out string value)
        {
            EnsureNotDisposed();
            EnsurePropertyListAcquired();

            if (_properties.Contains(name))
            {
                value = Interop.GetPropertyValue(_handle, name);
                return value != null;
            }
            value = null;
            return false;
        }

        private string[] _associatedImages;
        private void EnsureAssociatedImageListAcquired()
        {
            if (_associatedImages == null)
            {
                _associatedImages = Interop.GetAssociatedImageNames(_handle);
                ThrowHelper.CheckAndThrowError(_handle);
            }
        }

        public IReadOnlyCollection<string> GetAllAssociatedImageNames()
        {
            EnsureNotDisposed();

            _associatedImages = Interop.GetAssociatedImageNames(_handle);
            ThrowHelper.CheckAndThrowError(_handle);
            return _associatedImages;
        }

        public bool TryGetAssociatedImageDimemsions(string name, out ImageDimemsions dims)
        {
            EnsureNotDisposed();
            EnsureAssociatedImageListAcquired();

            if (_associatedImages.Contains(name))
            {
                Interop.GetAssociatedImageDimemsions(_handle, name, out long w, out long h);
                if (w != -1 && h != -1)
                {
                    dims = new ImageDimemsions(w, h);
                    return true;
                }
            }

            dims = default;
            return false;
        }

        public byte[] ReadAssociatedImageToArray(string name)
        {
            return ReadAssociatedImageToArray(name, out var _);
        }
        public byte[] ReadAssociatedImageToArray(string name, out ImageDimemsions dimemsions)
        {
            EnsureNotDisposed();

            if (!TryGetAssociatedImageDimemsions(name, out dimemsions))
            {
                throw new KeyNotFoundException();
            }

            var data = new byte[dimemsions.Width * dimemsions.Height * 4];
            ReadAssociatedImage(ref data[0], name);
            return data;
        }
        private unsafe void ReadAssociatedImage(ref byte data, string name)
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

        #region IDisposable Support

        private void EnsureNotDisposed()
        {
            bool lockTaken = false;
            _lock.Enter(ref lockTaken);
            try
            {
                if (_disposed && _retainedCount == 0)
                {
                    throw new ObjectDisposedException(nameof(OpenSlideImage));
                }
            }
            finally
            {
                if (lockTaken)
                {
                    _lock.Exit();
                }
            }
        }

        public bool IsDisposed
        {
            get
            {
                bool lockTaken = false;
                _lock.Enter(ref lockTaken);
                try
                {
                    return _disposed && _retainedCount == 0;
                }
                finally
                {
                    if (lockTaken)
                    {
                        _lock.Exit();
                    }
                }
            }
        }

        private void Dispose(bool disposing) // instead of protected virtual
        {
            bool lockTaken = false;
            _lock.Enter(ref lockTaken);
            try
            {
                _disposed = true;
                if (_retainedCount == 0 && _handle != IntPtr.Zero)
                {
                    Interop.Close(_handle);
                    _handle = IntPtr.Zero;
                }
            }
            finally
            {
                if (lockTaken)
                {
                    _lock.Exit();
                }
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

        #region IRetainable Support

        public bool IsRetained
        {
            get
            {
                bool lockTaken = false;
                _lock.Enter(ref lockTaken);
                try
                {
                    return _retainedCount > 0;
                }
                finally
                {
                    if (lockTaken)
                    {
                        _lock.Exit();
                    }
                }
            }
        }

        public void Retain()
        {
            bool lockTaken = false;
            _lock.Enter(ref lockTaken);
            try
            {
                if (_retainedCount == 0 && _disposed)
                {
                    throw new ObjectDisposedException(nameof(OpenSlideImage));
                }
                _retainedCount++;
            }
            finally
            {
                if (lockTaken)
                {
                    _lock.Exit();
                }
            }
        }

        public bool Release()
        {
            bool lockTaken = false;
            _lock.Enter(ref lockTaken);
            try
            {
                if (_retainedCount > 0)
                {
                    _retainedCount--;
                    if (_retainedCount == 0)
                    {
                        if (_disposed && _handle != IntPtr.Zero)
                        {
                            Interop.Close(_handle);
                            _handle = IntPtr.Zero;
                        }
                        return true;
                    }
                }
            }
            finally
            {
                if (lockTaken)
                {
                    _lock.Exit();
                }
            }
            return false;
        }

        #endregion


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

        }
    }
}
