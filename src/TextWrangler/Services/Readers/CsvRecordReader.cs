using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using CsvHelper;
using TextWrangler.Configuration;
using TextWrangler.Extensions;
using TextWrangler.Models;

namespace TextWrangler.Services.Readers
{
    public class CsvRecordReader : IRecordReader
    {
        private readonly string[] _header;
        private readonly CsvReader _csvReader;
        private readonly StreamReader _streamReader;

        private bool _disposed;

        /// <summary>
        /// Used to read a CSV file specified from a file system located csv file
        /// </summary>
        /// <param name="sourceFile">The full path/name to the file to be read - must be accessible via the <see cref="System.IO.File.OpenRead" /> method/> </param>
        /// <param name="csvConfiguration">A <see cref="CsvHelper.Configuration" /> configuration specification for how to read the csv file</param>
        public CsvRecordReader(string sourceFile, CsvHelper.Configuration.Configuration csvConfiguration = null)
            : this(sourceFile.ToStream(), csvConfiguration) { }

        /// <summary>
        /// Used to read a CSV-formated stream via a <see cref="System.IO.StreamReader" />
        /// </summary>
        /// <param name="stream">The CSV-formated stream to read from</param>
        /// <param name="csvConfiguration">A <see cref="CsvHelper.Configuration" /> configuration specification for how to read the csv file</param>
        public CsvRecordReader(Stream stream, CsvHelper.Configuration.Configuration csvConfiguration = null)
        {
            var csvConfig = csvConfiguration ?? GetDefaultConfiguration();

            if (stream == null || !stream.CanRead)
            {
                throw new ArgumentNullException(nameof(stream), "Stream must be included and readable");
            }

            // Keep a local ref to the stream reader so we can properly dispose of it on failure in creating the csv reader
            _streamReader = new StreamReader(stream);
            _csvReader = new CsvReader(new StreamReader(stream), csvConfig);

            if (csvConfig.HasHeaderRecord)
            {
                _csvReader.Read();
                _csvReader.ReadHeader();

                _header = _csvReader.Context.HeaderRecord;
            }
        }

        public static CsvHelper.Configuration.Configuration GetDefaultConfiguration() => new CsvHelper.Configuration.Configuration
                                                                                         {
                                                                                             HasHeaderRecord = true,
                                                                                             CultureInfo = CultureInfo.InvariantCulture,
                                                                                             DetectColumnCountChanges = false,
                                                                                             Delimiter = ",",
                                                                                             Escape = '"',
                                                                                             Quote = '"',
                                                                                             AllowComments = false,
                                                                                             Encoding = Encoding.UTF8,
                                                                                             IgnoreBlankLines = true,
                                                                                             BufferSize = 1024 * 10,
                                                                                             BadDataFound = rctx =>
                                                                                                            {
                                                                                                                var msg = $"Could not read CSV record [{rctx.RawRow}], BadDataFound. RawRecord [{rctx.RawRecord}]";

                                                                                                                if (TextWranglerConfig.OnException(null, msg))
                                                                                                                {
                                                                                                                    throw new TextWranglerReaderException(msg);
                                                                                                                }
                                                                                                            },
                                                                                             MissingFieldFound = (headers, recordIndex, rctx) =>
                                                                                                                 {
                                                                                                                     var msg = $"Source filed not found in CSV reader, row number  [{recordIndex}], MissingFieldFound. RawRecord [{rctx.RawRecord}]";

                                                                                                                     if (TextWranglerConfig.OnException(null, msg))
                                                                                                                     {
                                                                                                                         throw new TextWranglerReaderException(msg);
                                                                                                                     }
                                                                                                                 },
                                                                                             ReadingExceptionOccurred = csvx => TextWranglerConfig.OnException(csvx, null),
                                                                                         };

        /// <inheritdoc/>
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _csvReader?.Dispose();
            _streamReader?.Dispose();

            _disposed = true;
        }

        public int CountRead { get; private set; }
        public int CountFail { get; private set; }

        /// <summary>
        /// Lazily reads the CSV, mapping header names to field values in the output map. If no header is included, field names will simply be the field index from the
        /// CSV file.
        /// Note that this method is not thread-safe - reading records in parallel should be avoided
        /// </summary>
        /// <returns>A lazily-read enumerable map raw records</returns>
        public IEnumerable<IReadOnlyDictionary<string, string>> GetRecords(int limit = int.MaxValue)
        {
            while (true)
            {   // CountRead is the index of the record being read on this iteration...
                CountRead++;

                var recordMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                var recordMapComplete = false;

                try
                {
                    if (!_csvReader.Read())
                    {
                        break;
                    }

                    var columnCount = _csvReader.Context.Record?.Length ?? _header?.Length ?? int.MaxValue;

                    for (var fieldIndex = 0; fieldIndex < columnCount; fieldIndex++)
                    {
                        if (!_csvReader.TryGetField<string>(fieldIndex, out var fieldValue))
                        {
                            break;
                        }

                        recordMap.Add(_header == null || _header.Length <= fieldIndex || string.IsNullOrEmpty(_header[fieldIndex])
                                          ? fieldIndex.ToString()
                                          : _header[fieldIndex],
                                      fieldValue);
                    }

                    recordMapComplete = true;
                }
                catch(Exception x) when(!TextWranglerConfig.OnException(x, $"Could not read CSV record [{CountRead}]"))
                {
                    throw;
                }

                if (recordMapComplete)
                {
                    yield return recordMap;
                }
                else
                {
                    CountFail++;
                }

                if (CountRead >= limit)
                {
                    break;
                }
            }
        }
    }
}
