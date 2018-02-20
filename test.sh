#!/usr/bin/env sh

cd tests/OpenSlideNET.Tests
dotnet restore
dotnet xunit
