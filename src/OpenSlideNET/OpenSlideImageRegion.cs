namespace OpenSlideNET
{
    public struct OpenSlideImageRegion
    {
        private readonly long _x;
        private readonly long _y;
        private readonly long _width;
        private readonly long _height;

        public OpenSlideImageRegion(long x, long y, long width, long height)
        {
            _x = x;
            _y = y;
            _width = width;
            _height = height;
        }

        public long X => _x;
        public long Y => _y;
        public long Width => _width;
        public long Height => _height;

        public void Deconstruct(out long x, out long y, out long width, out long height)
        {
            x = _x;
            y = _y;
            width = _width;
            height = _height;
        }
    }
}
