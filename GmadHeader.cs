using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace GmadFileFormat
{
    /// <summary>
    /// A GMAD addon file
    /// </summary>
    public readonly partial struct GmadHeader
    {
        /// <summary>
        /// The information of the author that created this GMAD file
        /// </summary>
        public AuthorInfo Author { get; }

        /// <summary>
        /// The description of the addon in this GMAD file
        /// </summary>
        public String Description { get; }

        /// <summary>
        /// The files contained in this GMAD file
        /// </summary>
        public ImmutableArray<File> Files { get; }

        /// <summary>
        /// The version of this file's GMAD format
        /// </summary>
        public Int16 FormatVersion { get; }

        /// <summary>
        /// The name of the addon in this GMAd file
        /// </summary>
        public String Name { get; }

        /// <summary>
        /// The timestamp that this GMAD file was generated at
        /// </summary>
        public UInt64 Timestamp { get; }

        /// <summary>
        /// The version of this addon
        /// </summary>
        public Int32 Version { get; }

        /// <summary>
        /// How far into the GMAD file the addon's files are
        /// </summary>
        public Int64 FilesOffset { get; }

        /// <summary>
        /// Initializes a new GMAD file
        /// </summary>
        /// <param name="author"></param>
        /// <param name="description"></param>
        /// <param name="files"></param>
        /// <param name="formatVersion"></param>
        /// <param name="name"></param>
        /// <param name="timestamp"></param>
        /// <param name="version"></param>
        /// <param name="filesOffset"></param>
        internal GmadHeader ( AuthorInfo author, String description, IEnumerable<File> files, Int16 formatVersion, String name, UInt64 timestamp, Int32 version, Int64 filesOffset )
        {
            if ( files is null )
                throw new ArgumentNullException ( nameof ( files ) );

            this.Author = author;
            this.Description = description ?? throw new ArgumentNullException ( nameof ( description ) );
            this.Files = files.ToImmutableArray ( );
            this.FormatVersion = formatVersion;
            this.Name = name ?? throw new ArgumentNullException ( nameof ( name ) );
            this.Timestamp = timestamp;
            this.Version = version;
            this.FilesOffset = filesOffset;
        }
    }
}