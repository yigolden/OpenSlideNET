# OpenSlideNET
.NET bindings for OpenSlide (http://openslide.org/).

[![Build Status](https://travis-ci.org/yigolden/OpenSlideNET.svg?branch=master)](https://travis-ci.org/yigolden/OpenSlideNET)

# Requirements
The following package are needed if you want to only use this library.
* [OpenSlide](http://openslide.org/download/) >= 3.4.0

The following applications are needed if you want to build and use this library.
* [.NET Core SDK](https://www.microsoft.com/net/learn/get-started/windows)
* [nuget.exe](https://www.nuget.org/downloads)

**This library currently only works on Windows even though it's a .NET Standard library.**

To use it, please make sure libopenslide-0.dll and its dependencies are on the [DLL Search Path](https://msdn.microsoft.com/en-us/library/windows/desktop/ms682586(v=vs.85).aspx) (eg. the PATH environment variable).

If you want to build this library yourself, you should install [.NET Core SDK](https://www.microsoft.com/net/learn/get-started/windows), as well as put the directory contating nuget.exe on the PATH environmental variable.

# How to Use

To use this library, install [OpenSlideNET](https://www.nuget.org/packages/OpenSlideNET) ( and optionally [OpenSlideNET.DeepZoom](https://www.nuget.org/packages/OpenSlideNET.DeepZoom) ) from nuget.org.

# How to Build

To build this library, run the following command in the project root.
```
dotnet build -c Release
```

If the build is successful, NuGet packages for OpenSlideNET and OpenSlideNET.DeepZoom will be dropped in src\OpenSlideNET\bin\Release and src\OpenSlideNET.DeepZoom\bin\Release. Use the following command to publish these packages to your local NuGet repository. (Replace C:\Data\NugetReops with your local repository path.)
```
nuget add src\OpenSlideNET\bin\Release\OpenSlideNET.1.0.0-preview1-18021601.nupkg -Source C:\Data\NugetReops
nuget add src\OpenSlideNET.DeepZoom\bin\Release\OpenSlideNET.DeepZoom.1.0.0-preview1-18021601.nupkg -Source C:\Data\NugetReops
```

To use your build in a .NET Core project, run the follow command in your project root.
```
dotnet add package OpenSlideNET --version 1.0.0-preview1-18021601 --source C:\Data\NugetReops
dotnet add package OpenSlideNET.DeepZoom --version 1.0.0-preview1-18021601 --source C:\Data\NugetReops
```

# Help Wanted
* Documentation
* Tests
* Cross-platfrom OpenSlide native library.