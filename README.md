# TextWrangler

## [Build Test Run](#build-test-run)

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

## [Short Architectural Overview](#short-architectural-overview)

This is basically a simple 5-component ETL pipeline. Each of the 5 component dependencies can be injected to the default ITextWrangler implementation to adjust the runtime behaviour. To create and run a simple pipeline, initialize an instance of [TextWrangler](src/TextWrangler/TextWrangler.cs) (or any concrete implementation of ITextWrangler) and pass the appropriate args, like this:

```csharp
using(var wrangler = new TextWrangler(recordConfigName,
                                      new CsvRecordReader(fileName))
{
    wrangler.Wrangle(limit);
}
```

The general flow of the data through the pipeline is as follows:

IRecordReader -> IRecordBuilder -> IRecordFormatter -> IRecordFilterService -> IRecordWriter

The actual pipeline is a simple laziliy produced enumerable of entities that flow from one component to the next, which then processes each entity in turn and produces it as output as well.

#### IRecordReader

Responsible for reading source records and producing a map of labeled source values (labeled by field name or index depending on the abilities of the source). A CSV source for example could have a header which would allow for labeling the data by "field name", or it may not (or may have header values for only some fields in the CSV) in which case it may label the data by field index. Included concrete implementations are:

* [CsvRecordReader](src/TextWrangler/Services/Readers/CsvRecordReader.cs) (reads from a source CSV file or stream)
* [ProgressLoggedRecordReader](src/TextWrangler/Services/Readers/ProgressLoggedRecordReader.cs) (not an actual source reader, but instead decorates another reader by logging progress of a read)

#### IRecordBuilder

Responsible for turning IRecordReader source maps into initial representations of target record models including optionally filtering the source values with the injected IFieldFilterService (if the source configuration for a given field includes filters to be applied to the source value - see [config file ref](#config-file-reference)).  A single concrete implementation is included:

* [SerialRecordBuilder](src/TextWrangler/Services/Builders/SerialRecordBuilder.cs) (builds targets serially as outlined above)

#### IRecordFormatter

Responsible for manipulating target field values in some specific way to format output, prepare the value for other formatters to work, etc. Included concrete implementations are:

* [SourceFieldIndexReplacementFormatter](src/TextWrangler/Services/Formatters/SourceFieldIndexReplacementFormatter.cs) (Replaces named source fields in the target field value with either the actual value of the source field OR the indexed location (if the source field is used as part of a [format string](https://docs.microsoft.com/en-us/dotnet/api/system.string.format)))
* [StringDotFormatFormatter](src/TextWrangler/Services/Formatters/StringDotFormatFormatter.cs) (Runs a target field value through a [string.format](https://docs.microsoft.com/en-us/dotnet/api/system.string.format) operation using the field's sources list (see [config file ref](#config-file-reference)) as indexed inputs)
* [CompositeFieldFormatter](src/TextWrangler/Services/Formatters/CompositeFieldFormatter.cs) (Just a composite pattern implementation over the IFieldFormatter interface - runs multiple concrete IFieldFormatter's)

#### IRecordFilterService

Responsible for passing target values through zero or more filters (that are mapped to the target field in the config file (see [config file ref](#config-file-reference)). Some filters manipulate the data (i.e. trim, uppercase, titlecase, etc.) and some simply validate that data matches a certain restriction (and will throw an exception if not), i.e. alpha filter, alphanumeric filter, etc. The service is not a filter itself, it simply uses [IFieldFilter](src/TextWrangler/Services/Filters/IFieldFilter.cs) implementations to operate on a given field if a field requires it. To see various included [IFieldFilter](src/TextWrangler/Services/Filters/IFieldFilter.cs) implementations, see the [Filters](src/TextWrangler/Services/Filters) folder.

#### IRecordWriter

Responsible for writing final target records to something. Included concrete implementations include:

* [LogRecordWriter](src/TextWrangler/Services/Writers/LogRecordWriter.cs) (writes target records to the configured [ILogger](https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.logging.ilogger?view=aspnetcore-2.2) for the LogRecordWriter type)
* [CsvRecordWriter](src/TextWrangler/Services/Writers/CsvRecordWriter.cs) (writes target records to a given CSV file/stream)
* [NullRecordWriter](src/TextWrangler/Services/Writers/NullRecordWriter.cs) (writes target records nowhere, i.e. ignores them)

## [Documentation overview](#documentation-overview)

There's obviously some documentation here in this README, there is also a fairly large amount of [XML documentation comments](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/xmldoc/) on the various types and methods in the source code for reference. 

See the [config file ref](#config-file-reference) section for an overview of the config file format, requirements, samples, etc.

## [Assumptions](#assumptions)

* Initially I read the defined target format from the takehome doc as more or less dictating that fields in the target format MUST meet the pseudo-regex's included as a reference (hence why I implemented some filters that throw exceptions when data does not meet given requirements, i.e. alpha-only, or alphanumeric-only). After getting more into the provided data samples, I quickly instead assumed those pseudo-regex's aren't hard requirements, but instead just pointers (the ProductNumber field for example has a regex of [A-Z0-9]+, however ALL of the data includes dashes.  You could approach this at least different ways:
  * Throw an exception when things don't match
  * Replace "invalid" characters with some replacement
  * Extend the allowance regex
I took the last approach by default (i.e. allow basically any character) since the record type is string. To change this however to work in either of the other 2 approaches would be simple (either updating the config definition, or adding a new IFieldFilter implementation.

* The takehom doc didn't specify anything related to outputing the final correct target data (it did mention to log failed records).  I assumed we'd want a way to output it to one or more places naturally - by default it's just logged, but there's a CSV output writer included as well.

* Normally much of the code I wrote I would have instead opted for leveraging an external library/plugin - i.e. validation, logging, etc. I specifically did not here given the note of "without (significant) external dependencies".

## [Up next](#up-next)

* From a usability perspective I'd consider removing the current requirement of having the source fields for use on each given target field from being required in the sources list in the config. You'd still have to include it if you want filter it in some way (as you'd have to indicate the filter(s) to run it through), but you shouldn't necessarily have to have it mapped there if you just want to grab it's raw value and use it.
* Currently for any type of logging/output stream (i.e. error records, log data, etc.) you do not have context from different stages in the pipeline to include with output (for example, if a filter throws an exception, it does not currently have context of what the source values were, which record it is, etc.  Would be pretty simple to provide some thread/async local storage through the pipeline to give that context, which would allow for much "better" stream output
* From an operational perspective I'd look at adding likely significant logging/profiling options to allow for easier troubleshooting, tracking, etc. 
* I'd certainly consider extending the integration testing footprint specifically, unit testing is fairly well covered, but could also likely be extended a bit

## [Config File Reference](#config-file-reference)

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

