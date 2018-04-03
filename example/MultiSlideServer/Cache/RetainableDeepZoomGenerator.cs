using OpenSlideNET;
using System;
using System.Threading;

namespace MultiSlideServer.Cache
{
    public class RetainableDeepZoomGenerator : DeepZoomGenerator
    {
        private int _retainedCount;
        private bool _disposed;
        // Don't make the SpinLock variable readonly because it is being mutated. 
        private SpinLock _lock = new SpinLock();

        public RetainableDeepZoomGenerator(OpenSlideImage image, int tileSize = 254, int overlap = 1, bool limitBounds = true)
            : base(image, tileSize, overlap, limitBounds, true)
        {
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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                bool lockTaken = false;
                _lock.Enter(ref lockTaken);
                try
                {
                    _disposed = true;
                    if (_retainedCount == 0)
                    {
                        base.Dispose(true);
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
            else
            {
                base.Dispose(false);
            }
        }

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
                    throw new ObjectDisposedException(nameof(RetainableDeepZoomGenerator));
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
                        if (_disposed)
                        {
                            base.Dispose(true);
                        }
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
    }
}
