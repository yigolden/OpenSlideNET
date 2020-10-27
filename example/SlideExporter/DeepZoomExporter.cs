using OpenSlideNET;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;

namespace SlideExporter
{
    internal class DeepZoomExporter
    {
        public static void Run(FileInfo input, FileInfo output, int tileSize, int overlap)
        {
            var dziFile = new FileInfo(output.FullName + ".dzi");
            var dziDirectory = new DirectoryInfo(output.FullName + "_files");

            if (dziFile.Exists)
            {
                Console.WriteLine($"Error: File {dziFile.FullName} already exists.");
                return;
            }
            if (dziDirectory.Exists)
            {
                Console.WriteLine($"Error: Directory {dziDirectory} already exists.");
                return;
            }

            if (tileSize <= 0)
            {
                Console.WriteLine("Error: Tile size is invalid.");
                return;
            }
            if (overlap < 0)
            {
                Console.WriteLine("Error: Overlap is invalid.");
                return;
            }

            dziDirectory.Create();

            using var image = OpenSlideImage.Open(input.FullName);
            var dz = new DeepZoomGenerator(image, tileSize, overlap);

            Console.WriteLine("Writing: " + dziFile.FullName);
            File.WriteAllText(dziFile.FullName, dz.GetDzi("jpg"));

            int currentCount = 0;
            int totalCount = dz.TileCount;
            var levelDimensions = dz.LevelDimensions.ToArray();
            for (int level = dz.LevelCount - 1; level >= 0; level--)
            {
                DirectoryInfo levelDirectory = dziDirectory.CreateSubdirectory(level.ToString(CultureInfo.InvariantCulture));

                long width = levelDimensions[level].Width;
                long height = levelDimensions[level].Height;
                int colCount = (int)((width + tileSize - 1) / tileSize);
                int rowCount = (int)((height + tileSize - 1) / tileSize);

                for (int row = 0; row < rowCount; row++)
                {
                    for (int col = 0; col < colCount; col++)
                    {
                        string path = Path.Combine(levelDirectory.FullName, $"{col}_{row}.jpg");
                        Console.WriteLine("Writing: " + path + $" [{++currentCount}/{totalCount}]");
                        using var fs = new FileStream(path, FileMode.Create, FileAccess.Write);
                        dz.GetTileAsJpegToStream(level, col, row, fs);
                    }
                }
            }
        }
    }
}
