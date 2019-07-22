using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CsvHelper;
using TextWrangler.Configuration;
using TextWrangler.Extensions;
using TextWrangler.Models;

namespace TextWrangler.Services.Writers
{
    public class CsvRecordWriter : IRecordWriter
    {
        private readonly CsvWriter _csvWriter;
        private readonly StreamWriter _streamWriter;

        private bool _disposed;

        /// <summary>
        /// Used to write a CSV file specified to a file system located csv file
        /// </summary>
        /// <param name="targetFile">The full path/name to the file to be written to - must be accessible via the <see cref="System.IO.File.OpenWrite" /> method/> </param>
        /// <param name="csvConfiguration">A <see cref="CsvHelper.Configuration" /> configuration specification for how to read the csv file</param>
        public CsvRecordWriter(string targetFile, CsvHelper.Configuration.Configuration csvConfiguration = null)
            : this(targetFile.ToWriteStream(), csvConfiguration) { }

        /// <summary>
        /// Used to write to a CSV-formated stream via a <see cref="System.IO.StreamReader" />
        /// </summary>
        /// <param name="stream">The CSV-formated stream to write to</param>
        /// <param name="csvConfiguration">A <see cref="CsvHelper.Configuration" /> configuration specification for how to read the csv file</param>
        public CsvRecordWriter(Stream stream, CsvHelper.Configuration.Configuration csvConfiguration = null)
        {
            var csvConfig = csvConfiguration ?? TextWranglerConfig.DefaultCsvConfigurationFactory();

            if (stream == null || !stream.CanWrite)
            {
                throw new ArgumentNullException(nameof(stream), "Stream must be included and writable");
            }

            // Keep a local ref to the stream reader so we can properly dispose of it on failure in creating the csv writer
            _streamWriter = new StreamWriter(stream);
            _csvWriter = new CsvWriter(_streamWriter, csvConfig);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _csvWriter?.Dispose();
            _streamWriter?.Dispose();

            _disposed = true;
        }

        public int CountProcessed { get; private set; }
        public int CountFail { get; private set; }

        /// <summary>
        /// Lazily writes the CSV from the enumerable provided.
        /// If the configuration specifies inclusion of a header record, it will be written _before_ the records to be written begin enumeration, so if
        /// there are zero records in the target provided, a file with just a header will be written.
        /// </summary>
        /// <param name="targetRecords"></param>
        /// <param name="recordConfiguration"></param>
        /// <returns>A lazily-produced enumerable of records sucessfully written</returns>
        public IEnumerable<TargetRecord> Write(IEnumerable<TargetRecord> targetRecords, RecordConfiguration recordConfiguration)
        {
            // Write the header if appropriate
            if (_csvWriter.Configuration.HasHeaderRecord)
            {
                try
                {
                    foreach (var headerField in recordConfiguration.Fields
                                                                   .Select(f => f.Name))
                    {
                        _csvWriter.WriteField(headerField);
                    }

                    _csvWriter.NextRecord();
                }
                catch(Exception x) when(!TextWranglerConfig.OnException(x, "Could not write CSV header record"))
                {
                    throw;
                }
            }

            // Now the records themselves
            foreach (var targetRecord in targetRecords)
            {
                // CountProcessed is the index of the record currently being written...
                CountProcessed++;

                var recordWritten = false;

                try
                {
                    foreach (var targetFieldValue in targetRecord.Fields
                                                                 .Select(f => f.TypedValue))
                    {
                        _csvWriter.WriteField(targetFieldValue);
                    }

                    _csvWriter.NextRecord();

                    recordWritten = true;
                }
                catch(Exception x) when(!TextWranglerConfig.OnException(x, "Could not write CSV header record"))
                {
                    throw;
                }

                if (recordWritten)
                {
                    yield return targetRecord;
                }
                else
                {
                    CountFail++;
                }
            }
        }
    }
}
