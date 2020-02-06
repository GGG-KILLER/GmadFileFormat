using System;
using System.Collections.Generic;

namespace GmadFileFormat
{
    public readonly partial struct GmadHeader
    {
        /// <summary>
        /// The information of a GMAD file
        /// </summary>
        public readonly struct File : IEquatable<File>
        {
            /// <summary>
            /// The (relative) path of the file
            /// </summary>
            public String Path { get; }

            /// <summary>
            /// The CRC32 of the file's contents
            /// </summary>
            public UInt32 Crc { get; } // GMOD doesn't checks this

            /// <summary>
            /// The offset the file is located at from the start of the GMAD addon file
            /// </summary>
            public Int64 Offset { get; }

            /// <summary>
            /// The size of the file
            /// </summary>
            public Int64 Size { get; }

            /// <summary>
            /// Initializes this <see cref="File"/>
            /// </summary>
            /// <param name="path"></param>
            /// <param name="crc"></param>
            /// <param name="offset"></param>
            /// <param name="size"></param>
            internal File ( String path, UInt32 crc, Int64 offset, Int64 size )
            {
                this.Path = path ?? throw new ArgumentNullException ( nameof ( path ) );
                this.Crc = crc;
                this.Offset = offset;
                this.Size = size;
            }

            /// <inheritdoc/>
            public override Boolean Equals ( Object obj ) => obj is File file && this.Equals ( file );

            /// <inheritdoc/>
            public Boolean Equals ( File other ) => this.Path == other.Path && this.Crc == other.Crc && this.Offset == other.Offset && this.Size == other.Size;

            /// <inheritdoc/>
            public override Int32 GetHashCode ( )
            {
                var hashCode = -2125415227;
                hashCode = hashCode * -1521134295 + EqualityComparer<String>.Default.GetHashCode ( this.Path );
                hashCode = hashCode * -1521134295 + this.Crc.GetHashCode ( );
                hashCode = hashCode * -1521134295 + this.Offset.GetHashCode ( );
                hashCode = hashCode * -1521134295 + this.Size.GetHashCode ( );
                return hashCode;
            }

            /// <inheritdoc/>
            public static Boolean operator == ( File left, File right ) => left.Equals ( right );

            /// <inheritdoc/>
            public static Boolean operator != ( File left, File right ) => !( left == right );
        }
    }
}