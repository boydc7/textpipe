using System;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace TextWrangler.Extensions
{
    internal static class FileExtensions
    {
        private static readonly string[] _gzipDecompressableExtensions = new[]
                                                                         {
                                                                             "gz",
                                                                             "gzip",
                                                                             "zip"
                                                                         };

        public static Stream ToStream(this string localFile)
            => _gzipDecompressableExtensions.Any(x => localFile.EndsWith(x, StringComparison.OrdinalIgnoreCase))
                   ? new GZipStream(File.OpenRead(localFile), CompressionMode.Decompress)
                   : (Stream)File.OpenRead(localFile);
    }
}
