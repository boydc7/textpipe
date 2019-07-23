# TextWrangler

## [Build, Test, Run](#buildtestrun)

* Download and install dotnet core (this is the 2.2.301 direct link)
  * Mac: <https://download.visualstudio.microsoft.com/download/pr/1440e4a9-4e5f-4148-b8d2-8a2b3da4e622/d0c5cb2712e51c188200ea420d771c2f/dotnet-sdk-2.2.301-osx-x64.pkg>

  * Linux instructions: <https://dotnet.microsoft.com/download/linux-package-manager/ubuntu16-04/sdk-2.2.301>

* Clone code and build:
```bash
git clone https://github.com/boydc7/textwrangler
cd textwrangler
dotnet publish -c Release -o ../../publish src/TextWrangler.Console/TextWrangler.Console.csproj
```
* Run test suites (from textwrangler folder from above)
```bash
dotnet test tests/TextWrangler.UnitTests/TextWrangler.UnitTests.csproj
dotnet test tests/TextWrangler.IntegrationTests/TextWrangler.IntegrationTests.csproj
```
* Run (from textwrangler folder from above)
> NOTE: The sample.csv file is a copy of the sample Gist provided from the takehome doc. The recordSample recordType in the textwrangler.json config file can be used to map from any file with data similar to that to the target model record requested in the takehome doc.
```bash
# Show usage:
dotnet publish/wrangle.dll

# Wrangle sample.csv displaying output in log (console):
dotnet publish/wrangle.dll recordSample publish/sample.csv publish/textwrangler.json

# Wrangle sample.csv sending output to sample_out.csv file:
dotnet publish/wrangle.dll recordSample publish/sample.csv publish/textwrangler.json 0 publish/sample_out.csv

# Wrangle large-ish sales csv sending output to large_sales_out.csv file:
# Download and extract large sales file here:
# http://eforexcel.com/wp/wp-content/uploads/2017/07/1500000%20Sales%20Records.zip
dotnet publish/wrangle.dll salesSample "publish/1500000 Sales Records.csv" publish/textwrangler.json 0 publish/large_sales_out.csv
```

## [Short Architectural Overview](#archoverview)

This is basically a simple 5-component ETL pipeline. Each of the 5 component dependencies can be injected to the default ITextWrangler implementation to adjust the runtime behaviour. The general flow of the data through the pipeline is as follows:

IRecordReader -> IRecordBuilder -> IRecordFormatter -> IRecordFilterService -> IRecordWriter

The actual pipeline is a simple laziliy produced enumerable of entities that flow from one component to the next, which then processes each entity in turn and produces it as output as well.

#### IRecordReader

Responsible for reading source records and producing a map of labeled source values (labeled by field name or index depending on the abilities of the source). A CSV source for example could have a header which would allow for labeling the data by "field name", or it may not (or may have header values for only some fields in the CSV) in which case it may label the data by field index. Included concrete implementations are:

* [CsvRecordReader](src/TextWrangler/Readers/CsvRecordReader.cs) (reads from a source CSV file or stream)
* [ProgressLoggedRecordReader](src/TextWrangler/Readers/ProgressLoggedRecordReader.cs) (not an actual source reader, but instead decorates another reader by logging progress of a read)

#### IRecordBuilder

Responsible for turning IRecordReader source maps into initial representations of target record models including optionally filtering the source values with the injected IFieldFilterService (if the source configuration for a given field includes filters to be applied to the source value - see [config file ref](#config-ref) ).  A single concrete implementation is included:

* [SerialRecordBuilder](src/TextWrangler/Builders/SerialRecordBuilder.cs) (builds targets serially as outlined above)

#### IRecordFormatter

Responsible for manipulating target field values in some specific way to format output, prepare the value for other formatters to work, etc. Included concrete implementations are:

* [SourceFieldIndexReplacementFormatter](src/TextWrangler/Formatters/SourceFieldIndexReplacementFormatter.cs) (Replaces named source fields in the target field value with either the actual value of the source field OR the indexed location (if the source field is used as part of a [format string](https://docs.microsoft.com/en-us/dotnet/api/system.string.format)))
* [StringDotFormatFormatter](src/TextWrangler/Formatters/StringDotFormatFormatter.cs) (Runs a target field value through a [string.format](https://docs.microsoft.com/en-us/dotnet/api/system.string.format) operation using the field's sources list (see [config file ref](#config-ref)) as indexed inputs)


## [Documentation overview](#docoverview)

## [Assumptions](#assumptions)

## [Up next](#upnext)

## [Config File Reference](#config-ref)

The TextWrangler config file is a JSON formatted file responsible for defining one or more target record types that map one or more source fields to one or more target fields within the record.  

A simple sample config file that represents most (all?) of the functionality is as follows:

```javascript
{
    "orderRecord": {                    // A mapping of an OrderRecord"
      "fields": [                       // One or more fields that make up the order
        {
            "name": "OrderId",          // Target field named OrderId (Target fields are outputs)  
            "format": "<Order Num>",    // The source field(s) and format specifier used to create and
                                        // format the target field
            "type": "System.Int64",     // An optional type that the final target field value MUST 
                                        // convert into successfully
            "sources": [                // Zero or more source field(s) that are used to build this 
                                        // target field
              {         
                "name": "Order Num",    // The name of the source field. To reference a source field 
                                        // by name, it must come from a file format that supports 
                                        // named fields, i.e. a CSV file with valid headers, JSON,...
                "filters": [            // One or more filters to apply to the source field before using
                  "trim",               // These modify the source value in some way, and do not 
                  "upper",              // fail/throw exceptions based on the contents of the field
                                        // a value is simply trimmed, or upper-cased (if upper-
                  "titlecase"           // caseable) or title-cased...
                ]
              }
            ]
          },
          {
            "name": "OrderAmount",      // 2nd target field named OrderAmount
            "format": "{<Amt>,0:N4}",   // Formats can include any viable .NET format string (this
                                        // one will format the source field Amt value from a double 
                                        // into a number with culture-specific thousands separators
                                        // and 4 decimal places (rounded)
            "sources": [
              {
                "name": "Amt",          // The source field 
                "type": "System.Double"  // If the source field is used in a format string, the 
              }                         // type is required to make it work contextually correctly
            ]
          },
          {
            "name": "OrderDate",        // The 3rd target field named OrderDate
            "format": "{<Year>:2000}-{<Month>:00}-{<Day>:00}T00:00:00Z",  
                      // Above is an ISO 8601 format date with zulu tz build from 3 source fields
                      // The Year will be a 4-digit year (if 2 digits it will be 20xx), month and
                      // day will be 2 digit days padded with a 0 on teh left
            "sources": [
              {                          // Single target made of 3 different source fields
                "name": "Year",
                "type": "System.Int32"   // Each with a type specified to work contextually in 
              },                         // the format string
              {
                "name": "Month",
                "type": "System.Int32"
              },
              {
                "name": "Day",
                "type": "System.Int32"
              }
            ]
          },
          {
            "name": "Unit",               // 4th target field
            "format": "miles"             // Just a static value
          },
          {
            "name": "ItemName",           // 5th target field
            "format": "<Item ID>",        // Made from a single source field
            "sources": [
              {
                "name": "Item ID",
                "filters": [
                  "alphanumeric",         // Some filters are validators that do not change the field but verify
                  "alpha"                 // they contain valid data and throw an excpeption otherwise
                ]
              }
            ]
          }
      ]
    }
}
```

There are also multiple sample config files included in the project for reference use.

You can also refer to the XML documentation on the [RecordConfiguration.cs](src/TextWrangler/Configuration/RecordConfiguration.cs) and [FieldConfiguration.cs](src/TextWrangler/Models/FieldConfiguration.cs) classes for more detailed information.
 
### 

