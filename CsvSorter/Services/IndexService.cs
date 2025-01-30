using System.Runtime.CompilerServices;

namespace CsvSorter;

internal class IndexService<T> where T : IComparable<T>
{
    public IndexService(IIndexProvider<T> indexProvider)
    {
        _indexProvider = indexProvider;
    }

    public IndexService(IAsyncIndexProvider<T> indexProvider)
    {
        _asyncIndexProvider = indexProvider;
    }

    private IIndexProvider<T>? _indexProvider;
    private IAsyncIndexProvider<T>? _asyncIndexProvider;

    private CsvSorterIndex<T>? _headerIndex;

    public void SetIndexProvider(IAsyncIndexProvider<T> provider)
    {
        _asyncIndexProvider = provider;
        _indexProvider = null;
    }

    public void SetIndexProvider(IIndexProvider<T> provider)
    {
        _indexProvider = provider;
        _asyncIndexProvider = null;
    }

    public async Task AddIndexAsync(CsvSorterIndex<T> index, CancellationToken cancellationToken)
    {
        _indexProvider?.Add(index);
        if(_asyncIndexProvider != null)
            await _asyncIndexProvider.AddAsync(index, cancellationToken);
    }

    public void SetHeaderIndex(CsvSorterIndex<T> headerIndex)
    {
        _headerIndex = headerIndex;
    }

    public async IAsyncEnumerable<CsvSorterIndex<T>> GetSorted(
        SortDirection direction,
        [EnumeratorCancellation] CancellationToken cancellationToken
    )
    {
        if (_headerIndex != null)
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return _headerIndex;
        }

        if (_asyncIndexProvider != null)
        {
            await foreach (var index in _asyncIndexProvider.GetSorted(direction, cancellationToken))
                yield return index;
        }
        else if (_indexProvider != null)
        {
            foreach (var index in _indexProvider.GetSorted(direction))
            {
                cancellationToken.ThrowIfCancellationRequested();
                yield return index;
            }
        }
    }

    public async Task ClearAsync(CancellationToken cancellationToken)
    {
        _indexProvider?.Clear();
        if(_asyncIndexProvider != null)
            await _asyncIndexProvider.ClearAsync(cancellationToken);
    }
}