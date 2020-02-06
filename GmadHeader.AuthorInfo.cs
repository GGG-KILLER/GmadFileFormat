using System;
using System.Collections.Generic;

namespace GmadFileFormat
{
    public readonly partial struct GmadHeader
    {
        /// <summary>
        /// The information of the GMAD author
        /// </summary>
        public readonly struct AuthorInfo : IEquatable<AuthorInfo>
        {
            /// <summary>
            /// The name of the author of the GMAD file
            /// </summary>
            public String Name { get; }

            /// <summary>
            /// The SteamID64 of the GMAD file author
            /// </summary>
            public UInt64 SteamID64 { get; }

            /// <summary>
            /// Initializes a new <see cref="AuthorInfo"/>
            /// </summary>
            /// <param name="name"></param>
            /// <param name="steamID64"></param>
            internal AuthorInfo ( String name, UInt64 steamID64 )
            {
                this.Name = name ?? throw new ArgumentNullException ( nameof ( name ) );
                this.SteamID64 = steamID64;
            }

            /// <inheritdoc/>
            public override Boolean Equals ( Object obj ) => obj is AuthorInfo info && this.Equals ( info );

            /// <inheritdoc/>
            public Boolean Equals ( AuthorInfo other ) => this.Name == other.Name && this.SteamID64 == other.SteamID64;

            /// <inheritdoc/>
            public override Int32 GetHashCode ( )
            {
                var hashCode = -1870212646;
                hashCode = hashCode * -1521134295 + EqualityComparer<String>.Default.GetHashCode ( this.Name );
                hashCode = hashCode * -1521134295 + this.SteamID64.GetHashCode ( );
                return hashCode;
            }

            /// <inheritdoc/>
            public static Boolean operator == ( AuthorInfo left, AuthorInfo right ) => left.Equals ( right );

            /// <inheritdoc/>
            public static Boolean operator != ( AuthorInfo left, AuthorInfo right ) => !( left == right );
        }
    }
}