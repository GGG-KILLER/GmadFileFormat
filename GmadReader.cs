using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GmadFileFormat
{
    /// <summary>
    /// The class that contains the methods for parsing GMAD files
    /// </summary>
    public static class GmadReader
    {
        /// <summary>
        /// Represents a file that does not have a known offset yet
        /// </summary>
        /// <remarks>
        /// This had to be added to keep .NET Standard &lt; 2.0 support instead of value tuples.
        /// </remarks>
        private readonly struct PartialFile
        {
            /// <summary>
            /// The path of the file
            /// </summary>
            public readonly String Path;

            /// <summary>
            /// The size (in bytes) of the file
            /// </summary>
            public readonly Int64 Size;

            /// <summary>
            /// The CRC32 checksum of the file (unused by GMod)
            /// </summary>
            public readonly UInt32 Crc;

            public PartialFile ( String path, Int64 size, UInt32 crc )
            {
                this.Path = path;
                this.Size = size;
                this.Crc = crc;
            }

            /// <inheritdoc/>
            public override Boolean Equals ( Object? obj ) =>
                obj is PartialFile other && this.Path == other.Path && this.Size == other.Size && this.Crc == other.Crc;

            /// <inheritdoc/>
            public override Int32 GetHashCode ( )
            {
                var hashCode = -777822709;
                hashCode = hashCode * -1521134295 + EqualityComparer<String>.Default.GetHashCode ( this.Path );
                hashCode = hashCode * -1521134295 + this.Size.GetHashCode ( );
                hashCode = hashCode * -1521134295 + this.Crc.GetHashCode ( );
                return hashCode;
            }

            public void Deconstruct ( out String path, out Int64 size, out UInt32 crc )
            {
                path = this.Path;
                size = this.Size;
                crc = this.Crc;
            }

            /// <summary>
            /// Converts this <see cref="PartialFile"/> into a <see cref="GmadHeader.File"/> with
            /// the provided <paramref name="offset"/>
            /// </summary>
            /// <param name="offset"></param>
            /// <returns></returns>
            public GmadHeader.File WithOffset ( Int64 offset ) =>
                new GmadHeader.File ( this.Path, this.Crc, offset, this.Size );
        }

        /// <summary>
        /// The the bytes of the header
        /// </summary>
        private static ReadOnlySpan<Byte> Header => new Byte[] { 0x47 /* 'G' */, 0x4D /* 'M' */, 0x41 /* 'A' */, 0x44 /* 'D' */ };

        /// <summary>
        /// Returns wether a byte array has a valid GMAD header
        /// </summary>
        /// <param name="data">the file content as an array of bytes</param>
        /// <returns></returns>
        public static Boolean HasValidHeader ( ReadOnlySpan<Byte> data ) =>
            Header.SequenceEqual ( data );

        /// <summary>
        /// Reads 4 bytes from the stream and returns whether they form a valid header.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static Boolean HasValidHeader ( Stream stream )
        {
            var head = ArrayPool<Byte>.Shared.Rent ( 4 );
            try
            {
                stream.Read ( head, 0, 4 );
                return HasValidHeader ( head );
            }
            finally
            {
                ArrayPool<Byte>.Shared.Return ( head, clearArray: true );
            }
        }

        /// <summary>
        /// Parses a GMAD file taking into account possible JSON encoded descriptions
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static GmadHeader ReadHeader ( Stream stream )
        {
            if ( !HasValidHeader ( stream ) )
            {
                throw new Exception ( "Invalid GMAD file." );
            }

            using var reader = new BinaryReader ( stream, Encoding.UTF8, true );

            // We only support up to v3
            var formatVersion = ( Int16 ) reader.ReadChar ( );
            if ( formatVersion > 3 )
                throw new Exception ( "Unsupported GMAD file version." );

            // These stuff are almost always wrong (aka SID64 = 0)
            var authorSteamId64 = reader.ReadUInt64 ( );
            var timestamp = reader.ReadUInt64 ( );

            // required content ( not used )
            if ( formatVersion > 1 )
            {
                var content = ReadNullTerminatedString ( reader );
                while ( content != "" )
                {
                    content = ReadNullTerminatedString ( reader );
                }
            }

            var name = ReadNullTerminatedString ( reader );
            var description = ReadNullTerminatedString ( reader );
            var authorName = ReadNullTerminatedString ( reader );
            var addonVersion = reader.ReadInt32 ( );

            var fileHeaders = new List<PartialFile> ( );

            // Retrieve file metadata
            while ( reader.ReadUInt32 ( ) != 0 )
            {
                var path = ReadNullTerminatedString ( reader );
                var size = reader.ReadInt64 ( );
                var crc = reader.ReadUInt32 ( );
                fileHeaders.Add ( new PartialFile ( path, size, crc ) );
            }

            var filesOffset = stream.Position;

            // Files' data is stored after the metadata
            var files = new List<GmadHeader.File> ( );
            var accumulatedOffset = 0L;
            foreach ( PartialFile file in fileHeaders )
            {
                files.Add ( file.WithOffset ( filesOffset + accumulatedOffset ) );
                accumulatedOffset += file.Size;
            }

            return new GmadHeader ( new GmadHeader.AuthorInfo ( authorName, authorSteamId64 ), description, files, formatVersion, name, timestamp, addonVersion, filesOffset );

            static String ReadNullTerminatedString ( BinaryReader reader )
            {
                var build = new StringBuilder ( );
                for ( var ch = reader.ReadChar ( ); ch != 0x00; ch = reader.ReadChar ( ) )
                    build.Append ( ch );
                return build.ToString ( );
            }
        }

        /// <summary>
        /// Reads a <see cref="GmadHeader.File"/> from the stream by seeking to its start.
        /// </summary>
        /// <param name="file"></param>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static Byte[] ReadFile ( GmadHeader.File file, Stream stream )
        {
            if ( !stream.CanSeek )
                throw new NotSupportedException ( "Non-seekable streams are not supported." );

            stream.Seek ( file.Offset, SeekOrigin.Begin );
            return ReadFileAsNext ( file, stream );
        }

        /// <summary>
        /// Reads a <see cref="GmadHeader.File"/> from the stream WITHOUT SEEKING. If used
        /// inappropiately can read different files.
        /// </summary>
        /// <param name="file"></param>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static Byte[] ReadFileAsNext ( GmadHeader.File file, Stream stream )
        {
            var buffer = new Byte[file.Size];
            var read = stream.Read ( buffer, 0, ( Int32 ) file.Size );

            if ( read != file.Size )
                throw new InvalidDataException ( "The full file wasn't available in the stream." );

            return buffer;
        }
    }
}