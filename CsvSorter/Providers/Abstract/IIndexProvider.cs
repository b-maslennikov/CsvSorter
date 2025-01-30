namespace CsvSorter;

public interface IIndexProvider<T> where T: IComparable<T>
{
    void Add(CsvSorterIndex<T> record);
    IEnumerable<CsvSorterIndex<T>> GetSorted(SortDirection sortDirection);
    void Clear();
}