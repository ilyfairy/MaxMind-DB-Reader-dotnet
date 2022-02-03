﻿#region

using System;
using System.Collections.Generic;

#endregion

namespace MaxMind.Db
{
    /// <summary>
    ///     Data about the database file itself
    /// </summary>
    public sealed class Metadata
    {
        /// <summary>
        ///     Construct a metadata object.
        /// </summary>
        /// <param name="binaryFormatMajorVersion"></param>
        /// <param name="binaryFormatMinorVersion"></param>
        /// <param name="buildEpoch"></param>
        /// <param name="databaseType"></param>
        /// <param name="description"></param>
        /// <param name="ipVersion"></param>
        /// <param name="languages"></param>
        /// <param name="nodeCount"></param>
        /// <param name="recordSize"></param>
        [Constructor]
        [CLSCompliant(false)]
        public Metadata(
            [Parameter("binary_format_major_version")] int binaryFormatMajorVersion,
            [Parameter("binary_format_minor_version")] int binaryFormatMinorVersion,
            [Parameter("build_epoch")] ulong buildEpoch,
            [Parameter("database_type")] string databaseType,
            IDictionary<string, string> description,
            [Parameter("ip_version")] int ipVersion,
            IReadOnlyList<string> languages,
            [Parameter("node_count")] long nodeCount,
            [Parameter("record_size")] int recordSize
            )
        {
            BinaryFormatMajorVersion = binaryFormatMajorVersion;
            BinaryFormatMinorVersion = binaryFormatMinorVersion;
            BuildEpoch = buildEpoch;
            DatabaseType = databaseType;
            Description = description;
            IPVersion = ipVersion;
            Languages = languages;
            NodeCount = nodeCount;
            RecordSize = recordSize;
        }

        /// <summary>
        ///     The major version number for the MaxMind DB binary format used by the database.
        /// </summary>
        public int BinaryFormatMajorVersion { get; }

        /// <summary>
        ///     The minor version number for the MaxMind DB binary format used by the database.
        /// </summary>
        public int BinaryFormatMinorVersion { get; }

        internal ulong BuildEpoch { get; }

        /// <summary>
        ///     The date-time of the database build.
        /// </summary>
        public DateTime BuildDate => new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(BuildEpoch);

        /// <summary>
        ///     The MaxMind DB database type.
        /// </summary>
        public string DatabaseType { get; }

        /// <summary>
        ///     A map from locale codes to the database description in that language.
        /// </summary>
        public IDictionary<string, string> Description { get; }

        /// <summary>
        ///     The IP version that the database supports. This will be 4 or 6.
        /// </summary>
        public int IPVersion { get; }

        /// <summary>
        ///     A list of locale codes for languages that the database supports.
        /// </summary>
        public IReadOnlyList<string> Languages { get; }

        internal long NodeCount { get; }

        internal int RecordSize { get; }

        internal long NodeByteSize => RecordSize / 4;

        internal long SearchTreeSize => NodeCount * NodeByteSize;
    }
}