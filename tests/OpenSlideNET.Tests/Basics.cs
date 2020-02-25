using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace OpenSlideNET.Tests
{
    public class Basics
    {
        public static IEnumerable<object[]> GetOpenableFiles()
        {
            string currentDir = Directory.GetCurrentDirectory();
            yield return new object[] { Path.Combine(currentDir, "Assets", "boxes.tiff") };
            yield return new object[] { Path.Combine(currentDir, "Assets", "small.svs") };
        }

        public static IEnumerable<object[]> GetUnsupportedFiles()
        {
            string currentDir = Directory.GetCurrentDirectory();
            yield return new object[] { Path.Combine(currentDir, "Assets", "boxes.png") };
        }

        [Fact]
        public void TestLibraryVersion()
        {
            string version = OpenSlideImage.LibraryVersion;
            Assert.NotNull(version);
            Assert.NotEqual(string.Empty, version);
        }

        [Theory]
        [MemberData(nameof(GetOpenableFiles))]
        public void TestOpen(string fileName)
        {
            using (var osr = OpenSlideImage.Open(fileName))
            {
                Assert.False(osr.SafeHandle.IsInvalid);
            }
        }

        [Theory]
        [MemberData(nameof(GetUnsupportedFiles))]
        public void TestUnsupportedFiles(string fileName)
        {
            Assert.Throws<OpenSlideUnsupportedFormatException>(() => OpenSlideImage.Open(fileName));
        }


        public static IEnumerable<object[]> GetDetectFormatData()
        {
            string currentDir = Directory.GetCurrentDirectory();
            yield return new object[] { Path.Combine(currentDir, "Assets", "boxes.png"), null };
            yield return new object[] { Path.Combine(currentDir, "Assets", "boxes.tiff"), "generic-tiff" };
        }
        [Theory]
        [MemberData(nameof(GetDetectFormatData))]
        public void TestDetectFormat(string fileName, string format)
        {
            Assert.Equal(format, OpenSlideImage.DetectFormat(fileName));
        }

        [Fact]
        public void TestUnopenableFile()
        {
            string currentDir = Directory.GetCurrentDirectory();
            Assert.Throws<OpenSlideException>(() => OpenSlideImage.Open(Path.Combine(currentDir, "Assets", "unopenable.tiff")));
        }

        [Fact]
        public void TestOperationsOnClosedHandle()
        {
            string currentDir = Directory.GetCurrentDirectory();
            var osr = OpenSlideImage.Open(Path.Combine(currentDir, "Assets", "boxes.tiff"));
            Assert.NotEmpty(osr.GetAllPropertyNames());
            Assert.Empty(osr.GetAllAssociatedImageNames());
            osr.Dispose();
            Assert.Throws<ObjectDisposedException>(() => osr.LevelCount);
            Assert.Throws<ObjectDisposedException>(() => osr.ReadRegion(0, 0, 0, 100, 100));
            Assert.Throws<ObjectDisposedException>(() => osr.GetProperty("openslide.vendor", string.Empty));
            Assert.Throws<ObjectDisposedException>(() => { osr.ReadAssociatedImage("label", out var _); });
        }

        [Fact]
        public void TestMeradata()
        {
            string currentDir = Directory.GetCurrentDirectory();
            using (var osr = OpenSlideImage.Open(Path.Combine(currentDir, "Assets", "boxes.tiff")))
            {
                Assert.Equal(4, osr.LevelCount);

                Assert.Equal<ValueTuple<long, long>>((300, 250), osr.GetLevelDimensions(0));
                Assert.Equal<ValueTuple<long, long>>((150, 125), osr.GetLevelDimensions(1));
                Assert.Equal<ValueTuple<long, long>>((75, 62), osr.GetLevelDimensions(2));
                Assert.Equal<ValueTuple<long, long>>((37, 31), osr.GetLevelDimensions(3));

                Assert.Equal(1, osr.GetLevelDownsample(0));
                Assert.Equal(2, osr.GetLevelDownsample(1));
                Assert.Equal(4, osr.GetLevelDownsample(2), 0);
                Assert.Equal(8, osr.GetLevelDownsample(3), 0);

                Assert.Equal(0, osr.GetBestLevelForDownsample(0.5));
                Assert.Equal(1, osr.GetBestLevelForDownsample(3));
                Assert.Equal(3, osr.GetBestLevelForDownsample(37));
            }
        }

        [Fact]
        public void TestProperties()
        {
            string currentDir = Directory.GetCurrentDirectory();
            using (var osr = OpenSlideImage.Open(Path.Combine(currentDir, "Assets", "boxes.tiff")))
            {
                var props = osr.GetAllPropertyNames();
                string value = null;
                Assert.True(osr.TryGetProperty("openslide.vendor", out value));
                Assert.Equal("generic-tiff", value);
                string value2 = null;
                Assert.False(osr.TryGetProperty("__does_not_exist", out value2));
                Assert.Null(value2);
            }
        }

        [Fact]
        public void TestReadRegion()
        {
            string currentDir = Directory.GetCurrentDirectory();
            using (var osr = OpenSlideImage.Open(Path.Combine(currentDir, "Assets", "boxes.tiff")))
            {
                byte[] arr;
                arr = osr.ReadRegion(1, -10, -10, 400, 400);
                Assert.Equal(400 * 400 * 4, arr.Length);
                arr = osr.ReadRegion(4, 0, 0, 100, 100); // Bad level
                Assert.Equal(100 * 100 * 4, arr.Length);
                Assert.Throws<ArgumentOutOfRangeException>(() => { osr.ReadRegion(1, 0, 0, 400, -5); });
            }
        }

        [Fact]
        public void TestAssociatedImages()
        {
            string currentDir = Directory.GetCurrentDirectory();
            using (var osr = OpenSlideImage.Open(Path.Combine(currentDir, "Assets", "small.svs")))
            {
                Assert.NotEmpty(osr.GetAllAssociatedImageNames());
                byte[] arr;
                arr = osr.ReadAssociatedImage("thumbnail", out var dims);
                Assert.Equal(16, dims.Width);
                Assert.Equal(16, dims.Height);
                Assert.Equal(16 * 16 * 4, arr.Length);
                Assert.Throws<KeyNotFoundException>(() => { osr.ReadAssociatedImage("__missing", out var _); });
            }
        }

        [Fact]
        public void TestUnreadableSlideBadRegion()
        {
            string currentDir = Directory.GetCurrentDirectory();
            using (var osr = OpenSlideImage.Open(Path.Combine(currentDir, "Assets", "unreadable.svs")))
            {
                Assert.Equal("aperio", osr.GetProperty("openslide.vendor", string.Empty));
                Assert.Throws<OpenSlideException>(() => { osr.ReadRegion(0, 0, 0, 16, 16); });
                // openslide object has turned into an unusable state.
                Assert.False(osr.TryGetProperty("", out string value));
            }
        }

        [Fact]
        public void TestUnreadableSlideBadAssociatedImage()
        {
            string currentDir = Directory.GetCurrentDirectory();
            using (var osr = OpenSlideImage.Open(Path.Combine(currentDir, "Assets", "unreadable.svs")))
            {
                Assert.Equal("aperio", osr.GetProperty("openslide.vendor", string.Empty));
                Assert.Throws<OpenSlideException>(() => { osr.ReadAssociatedImage("thumbnail", out var _); });
                // openslide object has turned into an unusable state.
                Assert.False(osr.TryGetProperty("", out string value));
            }
        }
    }
}
