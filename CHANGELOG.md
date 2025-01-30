## 2.0.0
### Features
- Target framework is `.NET Standard 2.1` now
- Added async index provider
- Added `CancellationToken` to async methods
- Added `SortDirection` options
- Enabled nullable annotation context

### Breaking changes
- `OrderBy` and `OrderByDescending` methods are removed (Use `SortDirection` instead)
- `StreamReader` helper name was changes to `GetCsvSorter`

## 1.1.0
### Features
- Updated `CsvHelper` package
- Target framework is `.NET Standard 2.1` now

## 1.0.2
## Features
- Added `ToWriterAsync` and `ToFileAsync` methods
- Added `OnIndexCreationStarted`, `OnIndexCreationFinished`, `OnSortingStarted` and `OnSortingFinished` methods

## 1.0.1
### Bug Fixes
- Fixed package information

## 1.0.0
### Features
- Added `CsvSorter`
- Added `MemoryIndexProvider`
- Added `StreamReader` extensions