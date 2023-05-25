namespace CsvSorter.Entities;

public class CsvSorterIndex<T> where T: IComparable
{
    public T Value { get; set; }
    public long Offset { get; set; }
    public int Length { get; set; }
}