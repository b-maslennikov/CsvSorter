namespace CsvSorter;

public class CsvSorterIndex<T> where T: IComparable<T>
{
    public T? Value { get; set; }
    public long Offset { get; set; }
    public int Length { get; set; }
}