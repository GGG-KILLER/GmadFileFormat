# GMADFileFormat
A C# GMAD file parser made based on the [original](https://github.com/garrynewman/gmad/blob/master/include/AddonReader.h#L32-L116)

This project targets .NET Standard 1.4, which means you can use this on both .NET Core and .NET Framework.

# Dependencies
- [SevenZip](https://github.com/chrishaly/SevenZip) ([NuGet](https://www.nuget.org/packages/SevenZip))
- [Newtonsoft.Json](http://www.newtonsoft.com/json)

# How to use
1. Add a reference of it to your project
2. `using GMADFileFormat`
3. use `GMADAddon GMADParser.Parse ( Byte[] Data )`

# Documentation
There's none, there's literally just a single method there, the others are only helpers that should be `internal` (TODO).

# Gotchas
1. `GMADAddon.Author.SteamID64` will always be 0. That is not an error of this parser, the GMAD compressor(`gmad.exe`) doesn't adds any.
2. `GMADAddon.Author.Name` will always be `"Author Name"`. As before, this is `gmad.exe`s fault as it does not set any.
3. `GMADAddon.Version` on all addons I've tested this with was `1` so don't rely too much on it.

# License
This project is licensed under the MIT license, do what the license lets you do with it but just give me credits.
