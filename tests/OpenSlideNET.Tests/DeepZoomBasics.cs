using System;
using System.IO;
using Xunit;

namespace OpenSlideNET.Tests
{
    public class DeepZoomBasics
    {
        [Fact]
        public void TestMetadata()
        {
            string currentDir = Directory.GetCurrentDirectory();
            using (var osr = OpenSlideImage.Open(Path.Combine(currentDir, "Assets", "boxes.tiff")))
            {
                var dz = new DeepZoomGenerator(osr, 254, 1);
                Assert.Equal(10, dz.LevelCount);
                Assert.Equal(11, dz.TileCount);
                Assert.Equal(new(int, int)[] { (1, 1), (1, 1), (1, 1), (1, 1), (1, 1), (1, 1), (1, 1), (1, 1), (1, 1), (2, 1) }, dz.LevelTiles);
                Assert.Equal(new(long, long)[] { (1, 1), (2, 1), (3, 2), (5, 4), (10, 8), (19, 16), (38, 32), (75, 63), (150, 125), (300, 250) }, dz.LevelDimensions);
            }
        }

        [Fact]
        public void TestGetTile()
        {
            string currentDir = Directory.GetCurrentDirectory();
            using (var osr = OpenSlideImage.Open(Path.Combine(currentDir, "Assets", "boxes.tiff")))
            {
                var dz = new DeepZoomGenerator(osr, 254, 1);
                byte[] arr = dz.GetTile(9, 1, 0, out var info);
                Assert.Equal(47 * 250 * 4, arr.Length);
                Assert.Equal(47, info.TileWidth);
                Assert.Equal(250, info.TileHeight);
            }
        }

        [Fact]
        public void TestGetTileBad()
        {
            string currentDir = Directory.GetCurrentDirectory();
            using (var osr = OpenSlideImage.Open(Path.Combine(currentDir, "Assets", "boxes.tiff")))
            {
                var dz = new DeepZoomGenerator(osr, 254, 1);
                Assert.Throws<ArgumentOutOfRangeException>(() => { dz.GetTile(-1, 0, 0, out var _); });
                Assert.Throws<ArgumentOutOfRangeException>(() => { dz.GetTile(10, 0, 0, out var _); });
                Assert.Throws<ArgumentOutOfRangeException>(() => { dz.GetTile(0, -1, 0, out var _); });
                Assert.Throws<ArgumentOutOfRangeException>(() => { dz.GetTile(0, 1, 0, out var _); });
            }
        }

        [Fact]
        public void TestGetTileCoordinates()
        {

            string currentDir = Directory.GetCurrentDirectory();
            using (var osr = OpenSlideImage.Open(Path.Combine(currentDir, "Assets", "boxes.tiff")))
            {
                var dz = new DeepZoomGenerator(osr, 254, 1);
                var info = dz.GetTileInfo(9, 1, 0);
                Assert.Equal(0, info.SlideLevel);
                Assert.Equal(253, info.X);
                Assert.Equal(0, info.Y);
                Assert.Equal(47, info.Width);
                Assert.Equal(250, info.Height);
                Assert.Equal(47, info.TileWidth);
                Assert.Equal(250, info.TileHeight);
            }
        }

        [Fact]
        public void TestGetDzi()
        {

            string currentDir = Directory.GetCurrentDirectory();
            using (var osr = OpenSlideImage.Open(Path.Combine(currentDir, "Assets", "boxes.tiff")))
            {
                var dz = new DeepZoomGenerator(osr, 254, 1);
                Assert.Contains("http://schemas.microsoft.com/deepzoom/2008", dz.GetDzi("jpeg"));
            }
        }

    }
}
