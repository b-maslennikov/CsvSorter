using CsvSorter.Entities;

namespace CsvSorter;

public class MemoryIndexProvider<T> : IIndexProvider<T> where T: IComparable
{
    private readonly List<CsvSorterIndex<T>> _indexes;

    public MemoryIndexProvider()
    {
        _indexes = new List<CsvSorterIndex<T>>();
    }

    public void Add(CsvSorterIndex<T> record)
    {
        _indexes.Add(record);
    }

    public IEnumerable<CsvSorterIndex<T>> GetSorted(bool descending)
    {
        if (descending)
            return _indexes
                .OrderByDescending(x => x.Value);

        return _indexes
            .OrderBy(x => x.Value);
    }

    public void Clear()
    {
        _indexes.Clear();
    }
}