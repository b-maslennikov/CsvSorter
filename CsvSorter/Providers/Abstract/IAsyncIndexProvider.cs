namespace CsvSorter;

public interface IAsyncIndexProvider<T> where T: IComparable<T>
{
    Task AddAsync(CsvSorterIndex<T> record, CancellationToken cancellationToken);
    IAsyncEnumerable<CsvSorterIndex<T>> GetSorted(SortDirection sortDirection, CancellationToken cancellationToken);
    Task ClearAsync(CancellationToken cancellationToken);
}