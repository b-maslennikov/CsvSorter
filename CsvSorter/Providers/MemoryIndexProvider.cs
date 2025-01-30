namespace CsvSorter;

public class MemoryIndexProvider<T> : IIndexProvider<T> where T : IComparable<T>
{
    private readonly List<CsvSorterIndex<T>> _indexes = [];

    public void Add(CsvSorterIndex<T> record)
    {
        _indexes.Add(record);
    }

    public IEnumerable<CsvSorterIndex<T>> GetSorted(SortDirection sortDirection)
    {
        var comparer = sortDirection == SortDirection.Ascending
            ? Comparer<T?>.Create((x, y) =>
            {
                if (x == null) return y == null ? 0 : -1;
                return y == null ? 1 : x.CompareTo(y);
            })
            : Comparer<T?>.Create((x, y) =>
            {
                if (x == null) return y == null ? 0 : 1;
                return y == null ? -1 : y.CompareTo(x);
            });

        foreach (var index in _indexes.OrderBy(x => x.Value, comparer))
            yield return index;
    }

    public void Clear()
    {
        _indexes.Clear();
    }
}