using CsvSorter.Entities;
using NUnit.Framework;

namespace CsvSorter.Tests;

public class IndexProviderTests
{
    private MemoryIndexProvider<int> _provider = new MemoryIndexProvider<int>();

    [SetUp]
    public void Setup()
    {
        _provider = new MemoryIndexProvider<int>();

        _provider.Add(new CsvSorterIndex<int> { Value = 2 });
        _provider.Add(new CsvSorterIndex<int> { Value = 3 });
        _provider.Add(new CsvSorterIndex<int> { Value = 1 });
    }

    [TestCase(false, new[] { 1,2,3 })]
    [TestCase(true, new[] { 3,2,1 })]
    public void MemoryIndexProviderWorksAsExpected(bool descending, IEnumerable<int> expected)
    {
        var ordered = _provider.GetSorted(descending)
            .Select(x => x.Value);
            
        Assert.That(ordered.SequenceEqual(expected));
    }

    [Test]
    public void ClearMethodWorks()
    {
        var items = _provider.GetSorted(false);

        Assert.That(items.Any());

        _provider.Clear();

        items = _provider.GetSorted(false);

        Assert.That(!items.Any());
    }
}