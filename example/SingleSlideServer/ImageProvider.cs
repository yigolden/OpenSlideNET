using Microsoft.Extensions.Options;
using OpenSlideNET;
using System;

namespace SingleSlideServer
{
    public class ImageProvider : IDisposable
    {
        private OpenSlideImage _image;
        private DeepZoomGenerator _generator;

        public ImageProvider(IOptions<ImageOption> options)
        {
            var path = options.Value.Path;
            _image = OpenSlideImage.Open(path);
            _generator = new DeepZoomGenerator(_image, tileSize: 254, overlap: 1);
        }

        public DeepZoomGenerator DeepZoomGenerator => _generator;

        public void Dispose()
        {
            _generator.Dispose();
            _image.Dispose();
        }
    }
}
