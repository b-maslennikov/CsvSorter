# CsvSorter

![](logo.png)

[![Build](https://img.shields.io/appveyor/build/b-maslennikov/CsvSorter/master?style=flat-square)](https://ci.appveyor.com/project/b-maslennikov/CsvSorter) [![Tests](https://img.shields.io/appveyor/tests/b-maslennikov/CsvSorter?style=flat-square)](https://ci.appveyor.com/project/b-maslennikov/CsvSorter/build/tests) [![Issues](https://img.shields.io/github/issues/b-maslennikov/CsvSorter?style=flat-square)](https://github.com/b-maslennikov/CsvSorter/issues) [![License](https://img.shields.io/github/license/b-maslennikov/CsvSorter?style=flat-square)](https://github.com/b-maslennikov/CsvSorter/blob/main/LICENSE) [![Nuget](https://img.shields.io/nuget/v/CsvSorter?style=flat-square)](https://www.nuget.org/packages/CsvSorter)

It is a small package that will help you to sort your large CSV files.

## Instalation
```text
PM> Install-Package CsvSorter
```
```text
> dotnet add package CsvSorter
```
```xml
<PackageReference Include="CsvSorter" Version="2.0.0" />
```

## Dependencies
| Package name | Version |
| - | - |
| [CsvHelper](https://www.nuget.org/packages/CsvHelper) | >=30.0.1 |

## Avaliable methods

| Method name | Parameter type | Description |
| - | - | - |
| Using | `CsvConfiguration` | Sets CsvConfiguration. See [CsvHelper documentation](https://joshclose.github.io/CsvHelper/) |
|  | `TypeConverterOptions` | Sets TypeConverterOptions. See [CsvHelper documentation](https://joshclose.github.io/CsvHelper/) |
|  | `IIndexProvider<T>` or `IAsyncIndexProvider<T>` | Sets index provider. Default: `MemoryIndexProvider` |
|  | `SortDirection` | Sets sorting direction.<br>Default: `Ascending` |
| ToFileAsync | `string`, `CancellationToken` | Saves sorted data to a file |
| ToFile | `string` | Saves sorted data to a file  |
| ToWriterAsync | `TextWriter`, `CancellationToken` | Saves sorted data using provided writer |
| ToWriter | `TextWriter` | Saves sorted data using provided writer |

## Basic usage
```csharp
using CsvSorter;

await new StreamReader(@"C:\my_large_file.csv")
    .GetCsvSorter<int>("id")
    .ToFileAsync(@"C:\my_large_file_sorted_by_id.csv");
	
// or

await new CsvSorter<int>(streamReader, "id")
    .ToFileAsync(@"C:\my_large_file_sorted_by_id.csv");
```

## Index providers
Default index provider is `MemoryIndexProvider<T>`. It stores index data in the memory.<br>
You can create your own provider (AzureIndexProvider for example) by implementing `IIndexProvider<T>` or `IAsyncIndexProvider<T>` interfaces:
```csharp
public interface IIndexProvider<T> where T: IComparable<T>
{
    void Add(CsvSorterIndex<T> record);
    IEnumerable<CsvSorterIndex<T>> GetSorted(SortDirection sortDirection);
    void Clear();
}

public interface IAsyncIndexProvider<T> where T: IComparable<T>
{
    Task AddAsync(CsvSorterIndex<T> record, CancellationToken cancellationToken);
    IAsyncEnumerable<CsvSorterIndex<T>> GetSorted(SortDirection sortDirection, CancellationToken cancellationToken);
    Task ClearAsync(CancellationToken cancellationToken);
}
```

Please note that only one index provider can be used at a time.

```csharp
await new CsvSorter<int>(streamReader, "id")
    .Using(new FirebaseIndexProvider<int>()) // will be ignored
    .Using(new AzureIndexProvider<int>())    // will be used
    .ToWriterAsync(writer);
```

## Events
You can specify 4 events: `OnIndexCreationStarted`, `OnIndexCreationFinished`, `OnSortingStarted` and `OnSortingFinished`
```csharp
await new StreamReader(@"C:\my_large_file.csv")
    .GetCsvSorter<int>(0)
    .OnIndexCreationStarted(() => { logger.Info("Index creation has started"); })
    .OnIndexCreationFinished(() => { logger.Info("Index creation completed"); })
    .OnSortingStarted(() => { logger.Info("Sorting has started"); })
    .OnSortingFinished(() => { logger.Info("Sorting completed"); })
    .ToWriterAsync(writer);
```

## A few more examples
```csharp
var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
{
    HasHeaderRecord = false
};

var dateTimeConverterOptions = new TypeConverterOptions
{ 
    Formats = new[] { "dd_MM_yyyy" }
};

await new StreamReader(@"C:\my_large_file.csv")
    .GetCsvSorter<DateTime>(3)
    .Using(SortDirection.Descending)
    .Using(csvConfig)
    .Using(dateTimeConverterOptions)	
    .ToFileAsync(@"C:\my_large_file_sorted_by_date.csv", cancellationToken);
```

```csharp
var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
{
    Delimiter = "|"
};

await new StreamReader(@"C:\my_large_file.csv")
    .GetCsvSorter<string>("email")
    .Using(csvConfig)
    .Using(new AzureIndexProvider<string>())
    .ToWriterAsync(writer);
```
