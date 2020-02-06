# GmadFileFormat
A C# GMAD (`.gma` files) file parser made based on the [original](https://github.com/garrynewman/gmad/blob/master/include/AddonReader.h#L32-L116)

This project targets .NET Standard 1.3, which means you can use this on both .NET Core and .NET Framework.

# Sample
Here's an example of a **simple** (read: no validations at all) `.gma` extractor:
```cs
using GmadFileFormat;
using System;
using System.IO;

public class Program
{
    public static void Main ( String[] args )
    {
        var file = args[0];
        var output = args[1];

        using FileStream stream = File.OpenRead ( file );
        GmadHeader header = GmadReader.ReadHeader ( stream );

        Console.WriteLine ( $@"Addon information:
    Author: {header.Author.Name}
    Name:   {header.Name}" );

        foreach ( GmadHeader.File addonFile in header.Files )
        {
            var addonFilePath = Path.Combine ( output, addonFile.Path );
            var addonFileDirectory = Path.GetDirectoryName ( addonFilePath );
            Directory.CreateDirectory ( addonFileDirectory );

            // In this case AND ONLY IN THIS CASE, you could use GmadReader.ReadFileAsNext to avoid
            // seeking the stream.
            File.WriteAllBytes ( addonFilePath, GmadReader.ReadFile ( addonFile, stream ) );
        }
    }
}
```

# Documentation
There isn't much to document. The main two methods you need to know are:
1. `GmadHeader GmadReader.ReadHeader ( Stream stream )`: Reads a `GmadHeader` from a `Stream`;
2. `Byte[] GmadReader.ReadFile ( GmadHeader.File file, Stream stream )`: Reads a file inside the GMAD file from the stream.

There's also a `Byte[] GmadReader.ReadFileAsNext ( GmadHeader.File file, Stream stream )` method that does the same as the `ReadFile` one but without seeking (since it assumes that the stream you pass to it is at the position the file is located in).

It should only be used on **both** of these conditions:
1. You're reading the files **right after reading the header** without doing any operations to the Stream in the middle;
2. **And** you're reading the files in **the same order as they came in the header**.

This option was added for those when you have un-seekable streams or don't want the overhead of seeking.

# Dependencies
- [System.Collections.Immutable](https://www.nuget.org/packages/System.Collections.Immutable/) (Why? Because I wanted to make the header 100% immutable.)

# GMA Gotchas
1. The file *might* be zlib compressed (but without the zlib header).
2. `GmadHeader.Author.SteamID64` will always be 0. (`gmad.exe` creates them like this).
3. `GmadHeader.Author.Name` will always be `"Author Name"`. (`gmad.exe` creates them like this).
4. `GmadHeader.Version` on all addons I've tested this with was `1` so I don't recommend relying on it. (`gmad.exe` creates them like this).
5. `GmadHeader.Description` *might* be in JSON (depends on how old the file you're reading is).

There *was* support for automatically extracting the description part of the JSON and automatically de-compressing the whole file previously but it was removed because it wasn't in the scope of this library. These may be added later as extensions.

A sample of a JSON description would be:
```json
{
    "description": "Description",
    "type": "servercontent",
    "tags": [
        "build",
        "roleplay"
    ]
}
```

# License
This project is licensed under the MIT license.
