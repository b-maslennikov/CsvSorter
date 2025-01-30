namespace CsvSorter.Tests;

public class StreamReaderExtensionsTests
{
    [Test]
    public void Ensure_GetCsvSorterBy_Extension_Returns_CsvSorter_With_Correct_Generic_Type()
    {
        using var sr = new StreamReader(new MemoryStream());
        
        var sorterByFieldIndex = sr
            .GetCsvSorter<DateTime>(0);

        var sorterByFieldName = sr
            .GetCsvSorter<bool>("any");
        
        Assert.Multiple(() =>
        {
            Assert.That(sorterByFieldIndex, Is.Not.Null);
            Assert.That(sorterByFieldName, Is.Not.Null);
        });
        
        Assert.Multiple(() =>
        {
            Assert.That(sorterByFieldIndex.GetType(), Is.EqualTo(typeof(CsvSorter<DateTime>)));
            Assert.That(sorterByFieldName.GetType(), Is.EqualTo(typeof(CsvSorter<bool>)));
        });
    }
}