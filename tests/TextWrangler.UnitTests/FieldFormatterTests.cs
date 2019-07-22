using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using TextWrangler.Configuration;
using TextWrangler.Extensions;
using TextWrangler.Models;
using TextWrangler.Services.Formatters;

namespace TextWrangler.UnitTests
{
    [TestFixture]
    public class FieldFormatterTests
    {
        [Test]
        public void SourceFieldIndexReplacementReplacesSingleSourceFieldWithStaticValue()
        {
            var recordConfig = new RecordConfiguration
                               {
                                   Fields = new List<FieldConfiguration>
                                            {
                                                new FieldConfiguration
                                                {
                                                    Name = "TargetDummyField",
                                                    Sources = new List<SourceFieldConfiguration>
                                                              {
                                                                  new SourceFieldConfiguration
                                                                  {
                                                                      Name = "sourceField1"
                                                                  }
                                                              }
                                                }
                                            }
                               };

            var scalarRecord = new TargetRecord(new TargetField("TargetDummyField", ("sourceField1", "dummy field value").AsEnumerable())
                                                {
                                                    Value = "<sourceField1>"
                                                }.AsEnumerable());

            var formated = SourceFieldIndexReplacementFormatter.Instance.Format(scalarRecord.AsEnumerable(), recordConfig).Single();

            Assert.AreEqual("dummy field value", formated.Fields.Single().Value);
        }

        [Test]
        public void SourceFieldIndexReplacementReplacesFormatDelimitedFieldWithSourceFieldIndex()
        {
            var recordConfig = new RecordConfiguration
                               {
                                   Fields = new List<FieldConfiguration>
                                            {
                                                new FieldConfiguration
                                                {
                                                    Name = "TargetDummyField",
                                                    Sources = new List<SourceFieldConfiguration>
                                                              {
                                                                  new SourceFieldConfiguration
                                                                  {
                                                                      Name = "sourceField1",
                                                                      Type = "System.Double"
                                                                  }
                                                              }
                                                }
                                            }
                               };

            var scalarRecord = new TargetRecord(new TargetField("TargetDummyField", ("sourceField1", "12,345.87698").AsEnumerable())
                                                {
                                                    Value = "{<sourceField1>,0:N3}"
                                                }.AsEnumerable());

            var formated = SourceFieldIndexReplacementFormatter.Instance.Format(scalarRecord.AsEnumerable(), recordConfig).Single();

            Assert.AreEqual("{0,0:N3}", formated.Fields.Single().Value);
        }

        [Test]
        public void SourceFieldIndexReplacementReplacesMultipleFormatDelimitedFieldWithSourceFieldIndex()
        {
            var recordConfig = new RecordConfiguration
                               {
                                   Fields = new List<FieldConfiguration>
                                            {
                                                new FieldConfiguration
                                                {
                                                    Name = "TargetDummyField1",
                                                    Sources = new List<SourceFieldConfiguration>
                                                              {
                                                                  new SourceFieldConfiguration
                                                                  {
                                                                      Name = "sourceField1",
                                                                      Type = "System.Double"
                                                                  }
                                                              }
                                                },
                                                new FieldConfiguration
                                                {
                                                    Name = "TargetDummyField2",
                                                    Sources = new List<SourceFieldConfiguration>
                                                              {
                                                                  new SourceFieldConfiguration
                                                                  {
                                                                      Name = "sourceField2",
                                                                      Type = "System.Double"
                                                                  }
                                                              }
                                                },
                                                new FieldConfiguration
                                                {
                                                    Name = "TargetDummyField3",
                                                    Sources = new List<SourceFieldConfiguration>
                                                              {
                                                                  new SourceFieldConfiguration
                                                                  {
                                                                      Name = "sourceField3",
                                                                      Type = "System.Double"
                                                                  }
                                                              }
                                                }
                                            }
                               };

            var scalarRecord = new TargetRecord(new TargetField("TargetDummyField", new[]
                                                                                    {
                                                                                        ("sourceField1", "12,345.87698"),
                                                                                        ("sourceField2", "34,345.87698"),
                                                                                        ("sourceField3", "56,345.87698"),
                                                                                    })
                                                {
                                                    Value = "{<sourceField1>,0:N1} one and two {<sourceField2>,0:N2} two and three {<sourceField3>,0:N3}"
                                                }.AsEnumerable());

            var formated = SourceFieldIndexReplacementFormatter.Instance.Format(scalarRecord.AsEnumerable(), recordConfig).Single();

            Assert.AreEqual("{0,0:N1} one and two {1,0:N2} two and three {2,0:N3}", formated.Fields.Single().Value);
        }

        [Test]
        public void SourceFieldIndexReplacementReplacesMultipleFormatAndNonFormatDelimitedFieldReferencingSameSourceCorrectly()
        {
            var recordConfig = new RecordConfiguration
                               {
                                   Fields = new List<FieldConfiguration>
                                            {
                                                new FieldConfiguration
                                                {
                                                    Name = "TargetDummyField1",
                                                    Sources = new List<SourceFieldConfiguration>
                                                              {
                                                                  new SourceFieldConfiguration
                                                                  {
                                                                      Name = "sourceField1",
                                                                      Type = "System.Double"
                                                                  }
                                                              }
                                                },
                                                new FieldConfiguration
                                                {
                                                    Name = "TargetDummyField2",
                                                    Sources = new List<SourceFieldConfiguration>
                                                              {
                                                                  new SourceFieldConfiguration
                                                                  {
                                                                      Name = "sourceField2",
                                                                      Type = "System.Double"
                                                                  }
                                                              }
                                                },
                                                new FieldConfiguration
                                                {
                                                    Name = "TargetDummyField3",
                                                    Sources = new List<SourceFieldConfiguration>
                                                              {
                                                                  new SourceFieldConfiguration
                                                                  {
                                                                      Name = "sourceField3",
                                                                      Type = "System.Double"
                                                                  }
                                                              }
                                                }
                                            }
                               };

            var scalarRecord = new TargetRecord(new TargetField("TargetDummyField", new[]
                                                                                    {
                                                                                        ("sourceField1", "12,345.87698"),
                                                                                        ("sourceField2", "34,345.87698"),
                                                                                        ("sourceField3", "56,345.87698"),
                                                                                    })
                                                {
                                                    Value = "{<sourceField1>,0:N1} one <sourceField1> two {<sourceField2>,0:N2} two <sourceField3> three {<sourceField3>,0:N3}"
                                                }.AsEnumerable());

            var formated = SourceFieldIndexReplacementFormatter.Instance.Format(scalarRecord.AsEnumerable(), recordConfig).Single();

            Assert.AreEqual("{0,0:N1} one 12,345.87698 two {1,0:N2} two 56,345.87698 three {2,0:N3}", formated.Fields.Single().Value);
        }

        [Test]
        public void StringDotFormatWorksCorrectlyWithNoFormatStringIncluded()
        {
            var recordConfig = new RecordConfiguration
                               {
                                   Fields = new List<FieldConfiguration>
                                            {
                                                new FieldConfiguration
                                                {
                                                    Name = "TargetDummyField",
                                                    Sources = new List<SourceFieldConfiguration>
                                                              {
                                                                  new SourceFieldConfiguration
                                                                  {
                                                                      Name = "sourceField1"
                                                                  }
                                                              }
                                                }
                                            }
                               };

            var scalarRecord = new TargetRecord(new TargetField("TargetDummyField", ("sourceField1", "some random string").AsEnumerable())
                                                {
                                                    Value = "<sourceField1>"
                                                }.AsEnumerable());

            var formated = StringDotFormatFormatter.Instance.Format(scalarRecord.AsEnumerable(), recordConfig).Single();

            Assert.AreEqual("<sourceField1>", formated.Fields.Single().Value);
        }

        [Test]
        public void StringDotFormatWorksCorrectlyWithFormatStringButNoTypeIncluded()
        {
            var recordConfig = new RecordConfiguration
                               {
                                   Fields = new List<FieldConfiguration>
                                            {
                                                new FieldConfiguration
                                                {
                                                    Name = "TargetDummyField",
                                                    Sources = new List<SourceFieldConfiguration>
                                                              {
                                                                  new SourceFieldConfiguration
                                                                  {
                                                                      Name = "sourceField1"
                                                                  }
                                                              }
                                                }
                                            }
                               };

            var scalarRecord = new TargetRecord(new TargetField("TargetDummyField", ("sourceField1", "some random string").AsEnumerable())
                                                {
                                                    Value = "{0}"
                                                }.AsEnumerable());

            var formated = StringDotFormatFormatter.Instance.Format(scalarRecord.AsEnumerable(), recordConfig).Single();

            Assert.AreEqual("some random string", formated.Fields.Single().Value);
        }

        [Test]
        public void StringDotFormatWorksCorrectlyWithLongDouble()
        {
            var recordConfig = new RecordConfiguration
                               {
                                   Fields = new List<FieldConfiguration>
                                            {
                                                new FieldConfiguration
                                                {
                                                    Name = "TargetDoubleField",
                                                    Sources = new List<SourceFieldConfiguration>
                                                              {
                                                                  new SourceFieldConfiguration
                                                                  {
                                                                      Name = "sourceField1",
                                                                      Type = "System.Double"
                                                                  }
                                                              }
                                                }
                                            }
                               };

            var scalarRecord = new TargetRecord(new TargetField("TargetDoubleField", ("sourceField1", "12,345.67890987").AsEnumerable())
                                                {
                                                    Value = "test {0,0:N2}"
                                                }.AsEnumerable());

            var formated = StringDotFormatFormatter.Instance.Format(scalarRecord.AsEnumerable(), recordConfig).Single();

            Assert.AreEqual("test 12,345.68", formated.Fields.Single().Value);
        }

        [Test]
        public void StringDotFormatWorksCorrectlyWithTzEncodedDateTime()
        {
            var recordConfig = new RecordConfiguration
                               {
                                   Fields = new List<FieldConfiguration>
                                            {
                                                new FieldConfiguration
                                                {
                                                    Name = "TargetDateTimeField",
                                                    Sources = new List<SourceFieldConfiguration>
                                                              {
                                                                  new SourceFieldConfiguration
                                                                  {
                                                                      Name = "sourceField1",
                                                                      Type = "System.DateTime"
                                                                  }
                                                              }
                                                }
                                            }
                               };

            var now = DateTime.Now;
            var currentTzOffset = TimeZoneInfo.Local.GetUtcOffset(now);
            var nowAsString = $"{now,0:yyyy-MM-ddThh:mm:ss}";

            var sourceValue = $"{nowAsString}{(currentTzOffset.TotalMinutes > 0 ? "+" : "-")}{Math.Abs(currentTzOffset.Hours).ToString().PadLeft(2, '0')}:{Math.Abs(currentTzOffset.Minutes).ToString().PadLeft(2, '0')}";

            var scalarRecord = new TargetRecord(new TargetField("TargetDateTimeField", ("sourceField1", sourceValue).AsEnumerable())
                                                {
                                                    Value = "{0,0:yyyy-MM-dd}"
                                                }.AsEnumerable());

            var formated = StringDotFormatFormatter.Instance.Format(scalarRecord.AsEnumerable(), recordConfig).Single();

            Assert.AreEqual($"{now,0:yyyy-MM-dd}", formated.Fields.Single().Value);
        }

        [Test]
        public void StringDotFormatWorksCorrectlyWithSimpleDate()
        {
            var recordConfig = new RecordConfiguration
                               {
                                   Fields = new List<FieldConfiguration>
                                            {
                                                new FieldConfiguration
                                                {
                                                    Name = "TargetDateTimeField",
                                                    Sources = new List<SourceFieldConfiguration>
                                                              {
                                                                  new SourceFieldConfiguration
                                                                  {
                                                                      Name = "sourceField1",
                                                                      Type = "System.DateTime"
                                                                  }
                                                              }
                                                }
                                            }
                               };

            var scalarRecord = new TargetRecord(new TargetField("TargetDateTimeField", ("sourceField1", "2019-02-03").AsEnumerable())
                                                {
                                                    Value = "{0,0:yyyy-MM-dd}"
                                                }.AsEnumerable());

            var formated = StringDotFormatFormatter.Instance.Format(scalarRecord.AsEnumerable(), recordConfig).Single();

            Assert.AreEqual("2019-02-03", formated.Fields.Single().Value);
        }

    }
}
