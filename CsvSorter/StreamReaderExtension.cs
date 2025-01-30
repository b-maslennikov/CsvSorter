namespace CsvSorter;

public static class StreamReaderExtension
{
    public static CsvSorter<T> GetCsvSorter<T>(this StreamReader reader, string fieldName)
        where T : struct, IComparable<T> => new(reader, fieldName);

    public static CsvSorter<T> GetCsvSorter<T>(this StreamReader reader, int fieldIndex)
        where T : IComparable<T> => new(reader, fieldIndex);
}