#nullable enable

using System;
using System.Collections.Generic;

namespace OpenSlideNET
{
    internal readonly struct DeepZoomLayer
    {
        public long Width { get; }
        public long Height { get; }

        public DeepZoomLayer(long width, long height)
        {
            Width = width;
            Height = height;
        }
    }

    internal static class DeepZoomHelper
    {
        internal static DeepZoomLayer[] CalculateDeepZoomLayers(long width, long height)
        {
            if (width <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(width));
            }
            if (height <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(height));
            }
            var layers = new List<DeepZoomLayer>();
            DeepZoomLayer currentLayer = new DeepZoomLayer(width, height);
            layers.Add(currentLayer);
            while (currentLayer.Width != 1 || currentLayer.Height != 1)
            {
                currentLayer = new DeepZoomLayer((currentLayer.Width + 1) / 2, (currentLayer.Height + 1) / 2);
                layers.Add(currentLayer);
            }
            DeepZoomLayer[] layersArray = layers.ToArray();
            Array.Reverse(layersArray);
            return layersArray;
        }
    }
}
