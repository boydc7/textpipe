using System.Collections.Generic;
using NUnit.Framework;
using TextWrangler.Configuration;
using TextWrangler.Models;

namespace TextWrangler.UnitTests
{
    [SetUpFixture]
    public class UnitTestSetup
    {
        public static readonly RecordConfiguration SampleTestRecordConfiguration = new RecordConfiguration
                                                                                   {
                                                                                       RecordTypeName = "recordSample",
                                                                                       Fields = new List<FieldConfiguration>
                                                                                                {
                                                                                                    new FieldConfiguration
                                                                                                    {
                                                                                                        Name = "OrderID",
                                                                                                        Format = "<Order Number>",
                                                                                                        Type = "System.Int64",
                                                                                                        Sources = new List<SourceFieldConfiguration>
                                                                                                                  {
                                                                                                                      new SourceFieldConfiguration
                                                                                                                      {
                                                                                                                          Name = "Order Number",
                                                                                                                          Filters = new List<string>
                                                                                                                                    {
                                                                                                                                        "trim"
                                                                                                                                    }
                                                                                                                      }
                                                                                                                  }
                                                                                                    },
                                                                                                    new FieldConfiguration
                                                                                                    {
                                                                                                        Name = "OrderDate",
                                                                                                        Format = "<Year>-<Month>-<Day>",
                                                                                                        Type = "System.DateTime",
                                                                                                        Sources = new List<SourceFieldConfiguration>
                                                                                                                  {
                                                                                                                      new SourceFieldConfiguration
                                                                                                                      {
                                                                                                                          Name = "Year",
                                                                                                                          Filters = new List<string>
                                                                                                                                    {
                                                                                                                                        "trim"
                                                                                                                                    }
                                                                                                                      },
                                                                                                                      new SourceFieldConfiguration
                                                                                                                      {
                                                                                                                          Name = "Month",
                                                                                                                          Filters = new List<string>
                                                                                                                                    {
                                                                                                                                        "trim"
                                                                                                                                    }
                                                                                                                      },
                                                                                                                      new SourceFieldConfiguration
                                                                                                                      {
                                                                                                                          Name = "Day",
                                                                                                                          Filters = new List<string>
                                                                                                                                    {
                                                                                                                                        "trim"
                                                                                                                                    }
                                                                                                                      }
                                                                                                                  }
                                                                                                    },
                                                                                                    new FieldConfiguration
                                                                                                    {
                                                                                                        Name = "ProductId",
                                                                                                        Format = "<Product Number>",
                                                                                                        Sources = new List<SourceFieldConfiguration>
                                                                                                                  {
                                                                                                                      new SourceFieldConfiguration
                                                                                                                      {
                                                                                                                          Name = "Product Number",
                                                                                                                          Filters = new List<string>
                                                                                                                                    {
                                                                                                                                        "trim",
                                                                                                                                        "upper"
                                                                                                                                    }
                                                                                                                      }
                                                                                                                  }
                                                                                                    },
                                                                                                    new FieldConfiguration
                                                                                                    {
                                                                                                        Name = "ProductName",
                                                                                                        Format = "<Product Name>",
                                                                                                        Sources = new List<SourceFieldConfiguration>
                                                                                                                  {
                                                                                                                      new SourceFieldConfiguration
                                                                                                                      {
                                                                                                                          Name = "Product Name",
                                                                                                                          Filters = new List<string>
                                                                                                                                    {
                                                                                                                                        "trim",
                                                                                                                                        "titlecase"
                                                                                                                                    }
                                                                                                                      }
                                                                                                                  }
                                                                                                    },
                                                                                                    new FieldConfiguration
                                                                                                    {
                                                                                                        Name = "Quantity",
                                                                                                        Format = "<Count>",
                                                                                                        Type = "System.Double",
                                                                                                        Sources = new List<SourceFieldConfiguration>
                                                                                                                  {
                                                                                                                      new SourceFieldConfiguration
                                                                                                                      {
                                                                                                                          Name = "Count",
                                                                                                                          Filters = new List<string>
                                                                                                                                    {
                                                                                                                                        "trim"
                                                                                                                                    }
                                                                                                                      }
                                                                                                                  }
                                                                                                    },
                                                                                                    new FieldConfiguration
                                                                                                    {
                                                                                                        Name = "Unit",
                                                                                                        Format = "kg"
                                                                                                    }
                                                                                                }
                                                                                   };
    }
}
