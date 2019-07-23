using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using NUnit.Framework;
using TextWrangler.Configuration;
using TextWrangler.Models;
using TextWrangler.Services.Readers;
using TextWrangler.Services.Writers;

namespace TextWrangler.IntegrationTests
{
    [TestFixture]
    public class SampleCsvTests
    {
        [Test]
        public void SampleCsvCanBeReadBuiltFormattedFilteredAndWrittenCorrectlyInDefaultConfig()
        {
            var config = RecordConfigurationBuilder.Build("recordSample");

            var record1 = new TargetRecord(new[]
                                           {
                                               new TargetField("OrderID", new[]
                                                                          {
                                                                              ("Order Number", "1000")
                                                                          })
                                               {
                                                   Type = "System.Int64",
                                                   Value = "1000",
                                                   TypedValue = 1000L
                                               },
                                               new TargetField("OrderDate", new[]
                                                                            {
                                                                                ("Year", "2018"),
                                                                                ("Month", "1"),
                                                                                ("Day", "1")
                                                                            })
                                               {
                                                   Type = "System.DateTime",
                                                   Value = "2018-1-1",
                                                   TypedValue = new DateTime(2018, 1, 1)
                                               },
                                               new TargetField("ProductId", new[]
                                                                            {
                                                                                ("Product Number", "P-10001")
                                                                            })
                                               {
                                                   Value = "P-10001",
                                                   TypedValue = "P-10001"
                                               },
                                               new TargetField("ProductName", new[]
                                                                              {
                                                                                  ("Product Name", "Arugola")
                                                                              })
                                               {
                                                   Value = "Arugola",
                                                   TypedValue = "Arugola"
                                               },
                                               new TargetField("Quantity", new[]
                                                                           {
                                                                               ("Count", "5,250.50")
                                                                           })
                                               {
                                                   Type = "System.Double",
                                                   Value = "5,250.50",
                                                   TypedValue = 5250.5d
                                               },
                                               new TargetField("Unit", null)
                                               {
                                                   Value = "kg",
                                                   TypedValue = "kg"
                                               }
                                           });

            var record2 = new TargetRecord(new[]
                                           {
                                               new TargetField("OrderID", new[]
                                                                          {
                                                                              ("Order Number", "1001")
                                                                          })
                                               {
                                                   Type = "System.Int64",
                                                   Value = "1001",
                                                   TypedValue = 1001L
                                               },
                                               new TargetField("OrderDate", new[]
                                                                            {
                                                                                ("Year", "2017"),
                                                                                ("Month", "12"),
                                                                                ("Day", "12")
                                                                            })
                                               {
                                                   Type = "System.DateTime",
                                                   Value = "2017-12-12",
                                                   TypedValue = new DateTime(2017, 12, 12)
                                               },
                                               new TargetField("ProductId", new[]
                                                                            {
                                                                                ("Product Number", "P-10002")
                                                                            })
                                               {
                                                   Value = "P-10002",
                                                   TypedValue = "P-10002"
                                               },
                                               new TargetField("ProductName", new[]
                                                                              {
                                                                                  ("Product Name", "Iceberg Lettuce")
                                                                              })
                                               {
                                                   Value = "Iceberg Lettuce",
                                                   TypedValue = "Iceberg Lettuce"
                                               },
                                               new TargetField("Quantity", new[]
                                                                           {
                                                                               ("Count", "500.00")
                                                                           })
                                               {
                                                   Type = "System.Double",
                                                   Value = "500.00",
                                                   TypedValue = 500d
                                               },
                                               new TargetField("Unit", null)
                                               {
                                                   Value = "kg",
                                                   TypedValue = "kg"
                                               }
                                           });

            using(var reader = new CsvRecordReader("sample.csv"))
            using(var writer = new IntegrationTestRecordWriter(2))
            using(var wrangler = new TextWrangler(config, reader, recordWriter: writer))
            {
                wrangler.Wrangle();

                Assert.AreEqual(2, reader.CountRead, "Reader CountRead should be 2");
                Assert.AreEqual(2, writer.TargetRecords.Count, "Writer TargetRecords.Count should be 2");
                Assert.AreEqual(0, reader.CountFail, "Reader CountRead should be 0");

                var expectedJson1 = JsonConvert.SerializeObject(record1, Formatting.None);
                var actualJson1 = JsonConvert.SerializeObject(writer.TargetRecords[0], Formatting.None);

                Assert.That(actualJson1.Equals(expectedJson1, StringComparison.Ordinal));

                var expectedJson2 = JsonConvert.SerializeObject(record2, Formatting.None);
                var actualJson2 = JsonConvert.SerializeObject(writer.TargetRecords[1], Formatting.None);

                Assert.That(actualJson2.Equals(expectedJson2, StringComparison.Ordinal));
            }
        }
    }

    internal class IntegrationTestRecordWriter : IRecordWriter
    {
        public IntegrationTestRecordWriter(int expectedRecordCount)
        {
            TargetRecords = new List<TargetRecord>(expectedRecordCount);
        }

        internal List<TargetRecord> TargetRecords { get; }

        public void Dispose() { }

        public IEnumerable<TargetRecord> Write(IEnumerable<TargetRecord> targetRecords, RecordConfiguration recordConfiguration)
        {
            TargetRecords.AddRange(targetRecords);

            return TargetRecords;
        }
    }
}
