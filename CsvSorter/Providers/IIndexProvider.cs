using CsvSorter.Entities;

namespace CsvSorter;

public interface IIndexProvider<T> where T: IComparable
{
    void Add(CsvSorterIndex<T> record);
    IEnumerable<CsvSorterIndex<T>> GetSorted(bool descending);
    void Clear();
}