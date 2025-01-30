namespace CsvSorter.Tests;

public class IndexProviderTests
{
    private MemoryIndexProvider<int> _provider = new();

    [SetUp]
    public void Setup()
    {
        _provider = new MemoryIndexProvider<int>();

        _provider.Add(new CsvSorterIndex<int> { Value = 2 });
        _provider.Add(new CsvSorterIndex<int> { Value = 3 });
        _provider.Add(new CsvSorterIndex<int> { Value = 1 });
    }

    [TestCase(SortDirection.Ascending, new[] { 1, 2, 3 })]
    [TestCase(SortDirection.Descending, new[] { 3, 2, 1 })]
    public void Ensure_MemoryIndexProvider_Works(SortDirection sortDirection, IEnumerable<int> expected)
    {
        var ordered = _provider
            .GetSorted(sortDirection)
            .Select(x => x.Value);
        
        Assert.That(ordered, Is.EqualTo(expected).AsCollection);
    }

    [Test]
    public void Ensure_Clear_Method_Works()
    {
        var items = _provider.GetSorted(SortDirection.Ascending);

        Assert.That(items.Any(), Is.True);

        _provider.Clear();

        items = _provider.GetSorted(SortDirection.Ascending);

        Assert.That(items.Any(), Is.False);
    }
}