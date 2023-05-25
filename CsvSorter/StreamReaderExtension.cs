namespace CsvSorter;

public static class StreamReaderExtension
{
    public static CsvSorter<T> OrderBy<T>(this StreamReader reader, string fieldName) where T : IComparable
    {
        return new(reader, false, fieldName);
    }

    public static CsvSorter<T> OrderBy<T>(this StreamReader reader, int fieldIndex) where T : IComparable
    {
        return new(reader, false, fieldIndex);
    }

    public static CsvSorter<T> OrderByDescending<T>(this StreamReader reader, string fieldName) where T : IComparable
    {
        return new(reader, true, fieldName);
    }

    public static CsvSorter<T> OrderByDescending<T>(this StreamReader reader, int fieldIndex) where T : IComparable
    {
        return new(reader, true, fieldIndex);
    }
}