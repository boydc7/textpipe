# TextWrangler

Download and install dotnet core (this is the 2.2.301 direct link)
Mac: <https://download.visualstudio.microsoft.com/download/pr/1440e4a9-4e5f-4148-b8d2-8a2b3da4e622/d0c5cb2712e51c188200ea420d771c2f/dotnet-sdk-2.2.301-osx-x64.pkg>

Linux instructions: <https://dotnet.microsoft.com/download/linux-package-manager/ubuntu16-04/sdk-2.2.301>


Clone code and build:

git clone <https://github.com/boydc7/textwrangler>
cd textwrangler
dotnet publish -c Release -o ../../publish src/TextWrangler.Console/TextWrangler.Console.csproj

Run (from textwrangler folder from above)

Show usage:
dotnet publish/wrangle.dll

Wrangle sample.csv displaying output in log (console):
dotnet publish/wrangle.dll recordSample publish/sample.csv publish/textwrangler.json

Wrangle sample.csv sending output to sample_out.csv file:
dotnet publish/wrangle.dll recordSample publish/sample.csv publish/textwrangler.json 0 publish/sample_out.csv

Wrangle large-ish sales csv sending output to large_sales_out.csv file:
dotnet publish/wrangle.dll salesSample "publish/1500000 Sales Records.csv" publish/textwrangler.json 0 publish/large_sales_out.csv
