using Microsoft.Extensions.Options;
using MultiSlideServer.Cache;
using OpenSlideNET;
using System;
using System.Collections.Generic;

namespace MultiSlideServer
{
    public class ImageProvider
    {
        private ImageOptionItem[] _images;
        private DeepZoomGeneratorCache _cache;

        public ImageProvider(IOptions<ImagesOption> options, DeepZoomGeneratorCache cache)
        {
            _images = options.Value.Images;
            _cache = cache;
        }

        public IReadOnlyList<ImageOptionItem> Images => _images;

        public DeepZoomGeneratorCache Cache => _cache;

        public bool TryGetImagePath(string name, out string path)
        {
            foreach (var item in _images)
            {
                if (item.Name == name)
                {
                    path = item.Path;
                    return true;
                }
            }
            path = null;
            return false;
        }

        public IDisposable CreateDeepZoomGenerator(string name, string path, out DeepZoomGenerator dz)
        {
            if (!_cache.TryGet(name, out dz))
            {
                dz = new DeepZoomGenerator(OpenSlideImage.Open(path), disposeImage: true);
                if (!_cache.TrySet(name, dz))
                {
                    return dz;
                }
            }
            return DummyDisposable.Instance;

        }

        private sealed class DummyDisposable : IDisposable
        {
            public static readonly DummyDisposable Instance = new DummyDisposable();
            public void Dispose()
            {
                // do nothing
            }
        }
    }
}
