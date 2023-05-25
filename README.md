# CsvSorter

![](logo.png)

[![Build](https://img.shields.io/appveyor/build/b-maslennikov/CsvSorter/master?style=flat-square)](https://ci.appveyor.com/project/b-maslennikov/CsvSorter) [![Tests](https://img.shields.io/appveyor/tests/b-maslennikov/CsvSorter?style=flat-square)](https://ci.appveyor.com/project/b-maslennikov/CsvSorter/build/tests) [![Issues](https://img.shields.io/github/issues/b-maslennikov/CsvSorter?style=flat-square)](https://github.com/b-maslennikov/CsvSorter/issues) [![License](https://img.shields.io/github/license/b-maslennikov/CsvSorter?style=flat-square)](https://github.com/b-maslennikov/CsvSorter/blob/main/LICENSE) [![Nuget](https://img.shields.io/nuget/v/CsvSorter?style=flat-square)](https://www.nuget.org/packages/CsvSorter)

It is a small package that will help you to sort your large CSV files.

## Instalation
```
PM> Install-Package CsvSorter
```

## Dependencies
| Package name | Version |
| - | - |
| [CsvHelper](https://www.nuget.org/packages/CsvHelper) | >=30.0.1 |

## Avaliable methods

| Method name | Parameter type | Description |
| - | - | - |
| OrderBy&lt;T&gt; | `string`<br>`int` | Describes CSV field type and name (or index) that are going to be used during the sorting.<br>Sort direction: ascending. |
| OrderByDescending&lt;T&gt; | `string`<br>`int` | Describes CSV field type and name (or index) that are going to be used during the sorting.<br>Sort direction: descending. |
| Using | `CsvConfiguration` | Sets CsvConfiguration. See [CsvHelper documentation](https://joshclose.github.io/CsvHelper/) |
|  | `TypeConverterOptions` | Sets TypeConverterOptions. See [CsvHelper documentation](https://joshclose.github.io/CsvHelper/) |
|  | `IIndexProvider<T>` | Alows to set index provider. Default: `MemoryIndexProvider` |
| ToFile | `string` | Saves sorted data to a file |
| ToWriter | `TextWriter` | Saves sorted data using provided writer |

## Usage example
```csharp
using CsvSorter;
```
```csharp
new StreamReader(@"C:\my_large_file.csv")
    .OrderBy<int>("id")
    .ToFile(@"C:\my_large_file_sorted_by_id.csv");
```

## Index provider
Default index provider is `MemoryIndexProvider<T>`. It stores index data in the memory.
You can create your own provider (DatabaseIndexProvider for example) by implementing `IIndexProvider<T>` interface:
```csharp
public interface IIndexProvider<T> where T: IComparable
{
    void Add(CsvSorterIndex<T> record);
    IEnumerable<CsvSorterIndex<T>> GetSorted(bool descending);
    void Clear();
}
```

## Events
You can specify 4 events: `OnIndexCreationStarted`, `OnIndexCreationFinished`, `OnSortingStarted` and `OnSortingFinished`
```csharp
new StreamReader(@"C:\my_large_file.csv")
    .OrderBy<int>(0)
    .OnIndexCreationStarted(() => { logger.Info("Index creation has started"); })
    .OnIndexCreationFinished(() => { logger.Info("Index creation completed"); })
    .OnSortingStarted(() => { logger.Info("Sorting has started"); })
    .OnSortingFinished(() => { logger.Info("Sorting completed"); })
    .ToWriter(writer);
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

new StreamReader(@"C:\my_large_file.csv")
    .OrderByDescending<DateTime>(3)
    .Using(csvConfig)
    .Using(dateTimeConverterOptions)
    .ToFile(@"C:\my_large_file_sorted_by_date.csv");
```

```csharp
var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
{
    Delimiter = "|"
};

new StreamReader(@"C:\my_large_file.csv")
    .OrderBy<string>("email")
    .Using(csvConfig)
    .Using(new AzureIndexProvider<string>())
    .ToWriter(writer);
```
