# OpenSlideNET
.NET bindings for OpenSlide (http://openslide.org/).

# Requirements
The following package are needed if you want to only use this library.
* [OpenSlide](http://openslide.org/download/) >= 3.4.0

The following applications are needed if you want to build and use this library.
* [.NET Core SDK](https://www.microsoft.com/net/learn/get-started/windows)
* [nuget.exe](https://www.nuget.org/downloads)

To use it on Windows, make sure that libopenslide-0.dll and its dependencies are on the [DLL Search Path](https://msdn.microsoft.com/en-us/library/windows/desktop/ms682586(v=vs.85).aspx) (eg. the PATH environment variable).

To use it on Linux, follow the instruction on the OpenSlide website to install openslide package for your Linux distribution.

# How to Use

To use this library in your project, install [OpenSlideNET](https://www.nuget.org/packages/OpenSlideNET) ( and optionally [OpenSlideNET.ImageExtensions](https://www.nuget.org/packages/OpenSlideNET.ImageExtensions) ) from nuget.org. Add the following using statement to your source file.
```csharp
using OpenSlideNET;
```

## Print OpenSlide library version
```csharp
Console.WriteLine(OpenSlideImage.LibraryVersion);
```

## Detect whether a virtual slide file can be opened
```csharp
string format = OpenSlideImage.DetectFormat(fileName);
Console.WriteLine(
        format == null ? 
        "File format not supported.":
        $"File format ({format}) supported.");
```

## Open a virtual slide file
```csharp
using (OpenSlideImage image = OpenSlideImage.Open(fileName))
{
    Console.WriteLine($"Width: {image.Width} Height: {image.Height}");
    Console.WriteLine($"Level count: {image.LevelCount}");
}
```

## Generate DeepZoom
```csharp
using (OpenSlideImage image = OpenSlideImage.Open(fileName))
{
    var dz = new DeepZoomGenerator(image, tileSize: 254, overlap: 1);
    // Generate dzi file.
    string dziFileContent = dz.GetDzi(format: "jpeg");
    // Get raw tile data.
    DeepZoomGenerator.TileInfo tileInfo;
    byte[] rawTileData = dz.GetTile(level: 0, locationX: 0, locationY: 0, out tileInfo);
    // rawTileData contains BGRA data for every pixel. rawTileData.Length == 4 * tileInfo.Width * tileInfo.Height
    if (tileInfo.ResizeNeeded)
    {
        // Code here to resize image from (tileInfo.Width, tileInfo.Height) to (tileInfo.TileWidth, tileInfo.TileHeight)
    }
    // Alternatively, use GetTileAsJpeg to directly get JPEG encoded data.
    byte[] someTile = dz.GetTileAsJpeg(level: 0, locationX: 0, locationY: 0, out tileInfo);
    // if you are using GetTileAsJpeg and friends, you do NOT need to manually resize image to (tileInfo.TileWidth, tileInfo.TileHeight) as GetTileAsJpeg does this for you.
}
```

# How to Build

If you want to build this library yourself, you should install [.NET Core SDK](https://www.microsoft.com/net/learn/get-started/windows), as well as put the directory contating nuget.exe on the PATH environmental variable.

To build this library, run the following command in the project root.
```
dotnet build -c Release
```

If the build is successful, NuGet packages for OpenSlideNET and OpenSlideNET.ImageExtensions will be dropped in src\OpenSlideNET\bin\Release and src\OpenSlideNET.ImageExtensions\bin\Release. Use the following command to publish these packages to your local NuGet repository. (Replace C:\Data\NugetReops with your local repository path.)
```
nuget add src\OpenSlideNET\bin\Release\OpenSlideNET.<version>.nupkg -Source C:\Data\NugetReops
nuget add src\OpenSlideNET.ImageExtensions\bin\Release\OpenSlideNET.ImageExtensions.<version>.nupkg -Source C:\Data\NugetReops
```

To use your build in a .NET Core project, run the follow command in your project root.
```
dotnet add package OpenSlideNET --version <version> --source C:\Data\NugetReops
dotnet add package OpenSlideNET.ImageExtensions --version <version> --source C:\Data\NugetReops
```

# Future work
* Documentation
* Tests
