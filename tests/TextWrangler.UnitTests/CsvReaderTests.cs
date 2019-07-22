using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using TextWrangler.Configuration;
using TextWrangler.Services;
using TextWrangler.Services.Readers;

namespace TextWrangler.UnitTests
{
    [TestFixture]
    public class CsvReaderTests
    {
        [Test]
        public void DefaultCsvReaderCanParseSimpleSampleFile()
        {
            var sampleCsv = new List<string>
                            {
                                "Order Number,Year,Month,Day,Product Number,Product Name,Count,Extra Col1,Extra Col2,Empty Column",
                                "1000,2018,1,1,P-10001,Arugola,\"5,250.50\",Lorem,Ipsum,",
                                "1001,2017,12,12,P-10002,Iceberg lettuce,500.00,Lorem,Ipsum,"
                            };

            List<IReadOnlyDictionary<string, string>> records;

            using(var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(string.Join(Environment.NewLine, sampleCsv))))
            using(var reader = new CsvRecordReader(memoryStream))
            {
                records = reader.GetRecords().ToList();
            }

            Assert.That(records != null && records.Count == 2, $"Null or miscounted - [{records?.Count ?? -1}]");

            var record0Fields = string.Join(',', records[0].Keys);
            Assert.That(record0Fields.Equals(sampleCsv[0], StringComparison.Ordinal), $"Record[0] field names do not match header - [{record0Fields}]");

            var record1Fields = string.Join(',', records[1].Keys);
            Assert.That(record1Fields.Equals(sampleCsv[0], StringComparison.Ordinal), $"Record[1] field names do not match header - [{record1Fields}]");

            // Validate all non-header records
            for (var i = 1; i < sampleCsv.Count; i++)
            {
                Assert.AreEqual(sampleCsv[i].Replace("\"", string.Empty), string.Join(',', records[i-1].Values), $"Record [{i}] values mismatch");
            }
        }

        [Test]
        public void CanReadCsvWithouHeaderCorrectly()
        {
            var sampleCsv = new List<string>
                            {
                                "1000,2018,1,1,P-10001,Arugola,\"5,250.50\",Lorem,Ipsum,",
                                "1001,2017,12,12,P-10002,Iceberg lettuce,500.00,Lorem,Ipsum,"
                            };

            List<IReadOnlyDictionary<string, string>> records;

            var csvConfig = TextWranglerConfig.DefaultCsvConfigurationFactory();
            csvConfig.HasHeaderRecord = false;

            using(var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(string.Join(Environment.NewLine, sampleCsv))))
            using(var reader = new CsvRecordReader(memoryStream, csvConfig))
            {
                records = reader.GetRecords().ToList();
            }

            Assert.That(records != null && records.Count == 2, $"Null or miscounted - [{records?.Count ?? -1}]");

            var fieldIndexNames = string.Join(',', Enumerable.Range(0, 10).Select(i => i.ToString(CultureInfo.InvariantCulture)));

            var record0Fields = string.Join(',', records[0].Keys);
            Assert.That(record0Fields.Equals(fieldIndexNames, StringComparison.Ordinal), $"Record[0] field names do not match field indexes - [{record0Fields}]");

            var record1Fields = string.Join(',', records[1].Keys);
            Assert.That(record1Fields.Equals(fieldIndexNames), $"Record[1] field names do not match field indexes - [{record1Fields}]");

            // Validate records
            for (var i = 0; i < sampleCsv.Count; i++)
            {
                Assert.AreEqual(sampleCsv[i].Replace("\"", string.Empty), string.Join(',', records[i].Values), $"Record [{i}] values mismatch");
            }
        }

        [Test]
        public void CanReadCsvWithVaryingRecordLengthsCorrectly()
        {
            var sampleCsv = new List<string>
                            {
                                "Order Number,Year,Month,Day,Product Number,Product Name,Count,Extra Col1,Extra Col2,Empty Column",
                                "1000,2018,1,1,P-10001,Arugola,\"5,250.50\",Lorem,Ipsum,,newone,newtwo",
                                "1001,2017,12,12,P-10002,Iceberg lettuce,500.00,Lorem,Ipsum,"
                            };

            List<IReadOnlyDictionary<string, string>> records;

            using(var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(string.Join(Environment.NewLine, sampleCsv))))
            using(var reader = new CsvRecordReader(memoryStream))
            {
                records = reader.GetRecords().ToList();
            }

            Assert.That(records != null && records.Count == 2, $"Null or miscounted - [{records?.Count ?? -1}]");

            Assert.That(records[0].Count == 12, "Record[0] does not have 12 fields");
            Assert.That(records[1].Count == 10, "Record[1] does not have 10 fields");

            var record0Fields = string.Join(',', records[0].Keys);
            var record0FieldsShouldBe = string.Concat(sampleCsv[0], ",10,11");

            Assert.That(record0Fields.Equals(record0FieldsShouldBe, StringComparison.Ordinal), $"Record[0] field names do not match field indexes - [{record0Fields}]");

            var record1Fields = string.Join(',', records[1].Keys);
            Assert.That(record1Fields.Equals(sampleCsv[0]), $"Record[1] field names do not match field indexes - [{record1Fields}]");

            // Validate all non-header records
            for (var i = 1; i < sampleCsv.Count; i++)
            {
                Assert.AreEqual(sampleCsv[i].Replace("\"", string.Empty), string.Join(',', records[i-1].Values), $"Record [{i}] values mismatch");
            }
        }

        [Test]
        public void DisposingCsvRecordReaderRepeatedlyResultsInNoException()
        {
            var sampleCsv = new List<string>
                            {
                                "Order Number,Year,Month,Day,Product Number,Product Name,Count,Extra Col1,Extra Col2,Empty Column",
                                "1000,2018,1,1,P-10001,Arugola,5,250.50,Lorem,Ipsum,10,20,30,,40",
                                "1001,2017,12,12,P-10002,Iceberg lettuce,500.00,Lorem,Ipsum,10,20"
                            };

            using(var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(string.Join(Environment.NewLine, sampleCsv))))
            using(var reader = new CsvRecordReader(memoryStream))
            {
                var records = reader.GetRecords().ToList();

                reader.Dispose();
                reader.Dispose();
                reader.Dispose();
            }
        }
    }
}
