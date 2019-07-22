using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using TextWrangler.Configuration;
using TextWrangler.Extensions;
using TextWrangler.Models;
using TextWrangler.Services.Formatters;

namespace TextWrangler.IntegrationTests
{
    [TestFixture]
    public class CompositeFormatterTests
    {
        [Test]
        public void CompositeFormatterReplacesFormatDelimittedFieldsWithIndexAndStringFormatsCorrectly()
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

            var formatter = new CompositeFieldFormatter(new IFieldFormatter[]
                                                        {
                                                            SourceFieldIndexReplacementFormatter.Instance,
                                                            StringDotFormatFormatter.Instance
                                                        });

            var formated = formatter.Format(scalarRecord.AsEnumerable(), recordConfig).Single();

            Assert.AreEqual("12,345.877", formated.Fields.Single().Value);
        }
    }
}
