using CsvHelper.TypeConversion;

namespace CsvSorter.Tests;

public class CsvSorterTests
{
    [TestCase("input.csv", "a", SortDirection.Ascending)]
    public async Task Ensure_Does_Not_Close_Reader_And_Writer(
        string inputFileName,
        string fieldName,
        SortDirection sortDirection
    )
    {
        var inputFilePath = GetFilePath(inputFileName);
        using var reader = new StreamReader(inputFilePath);

        using var stream = new MemoryStream();
        await using var writer = new StreamWriter(stream);

        await new CsvSorter<int>(reader, fieldName)
            .Using(sortDirection)
            .ToWriterAsync(writer);

        Assert.Multiple(() =>
        {
            Assert.That(reader.BaseStream.CanRead, Is.True);
            Assert.That(writer.BaseStream.CanRead, Is.True);
        });
    }

    [TestCase("input.csv", "reference_a_asc.csv", "a", SortDirection.Ascending)]
    [TestCase("input.csv", "reference_a_desc.csv", "a", SortDirection.Descending)]
    [TestCase("input.csv", "reference_c_asc.csv", "c", SortDirection.Ascending)]
    public async Task Ensure_Sort_Result_Is_Equal_To_Reference(
        string inputFileName,
        string referenceFileName,
        string fieldName,
        SortDirection sortDirection
    )
    {
        var inputFilePath = GetFilePath(inputFileName);
        using var reader = new StreamReader(inputFilePath);

        using var stream = new MemoryStream();
        await using var writer = new StreamWriter(stream);

        await new CsvSorter<int>(reader, fieldName)
            .Using(sortDirection)
            .ToWriterAsync(writer);

        var referenceFilePath = GetFilePath(referenceFileName);
        var resultContent = await File.ReadAllBytesAsync(referenceFilePath);

        Assert.That(stream.Length, Is.EqualTo(resultContent.Length));
    }

    [TestCase("input.csv", "reference_a_asc.csv", "a", SortDirection.Ascending)]
    [TestCase("input.csv", "reference_a_desc.csv", "a", SortDirection.Descending)]
    [TestCase("input.csv", "reference_c_asc.csv", "c", SortDirection.Ascending)]
    public async Task Ensure_Works_Using_Field_Name(
        string inputFileName,
        string referenceFileName,
        string fieldName,
        SortDirection sortDirection)
    {
        var inputFilePath = GetFilePath(inputFileName);
        using var reader = new StreamReader(inputFilePath);

        using var stream = new MemoryStream();
        await using var writer = new StreamWriter(stream);

        await new CsvSorter<int>(reader, fieldName)
            .Using(sortDirection)
            .ToWriterAsync(writer);

        using var resultReader = new StreamReader(stream);
        resultReader.BaseStream.Seek(0, SeekOrigin.Begin);
        resultReader.DiscardBufferedData();

        var referenceFilePath = GetFilePath(referenceFileName);
        using var referenceReader = new StreamReader(referenceFilePath);

        Assert.Multiple(() =>
        {
            while (!resultReader.EndOfStream)
                Assert.That(referenceReader.ReadLine(), Is.EqualTo(resultReader.ReadLine()));
        });
    }

    [TestCase("input.csv", "reference_d_desc.csv", "d", SortDirection.Descending)]
    public async Task Ensure_Type_Converter_Works(
        string inputFileName,
        string referenceFileName,
        string fieldName,
        SortDirection sortDirection
    )
    {
        var inputFilePath = GetFilePath(inputFileName);
        using var reader = new StreamReader(inputFilePath);

        using var stream = new MemoryStream();
        await using var writer = new StreamWriter(stream);

        var dateTimeOptions = new TypeConverterOptions { Formats = ["dd_MM_yyyy"] };

        await new CsvSorter<DateTime>(reader, fieldName)
            .Using(sortDirection)
            .Using(dateTimeOptions)
            .ToWriterAsync(writer);

        using var resultReader = new StreamReader(stream);
        resultReader.BaseStream.Seek(0, SeekOrigin.Begin);
        resultReader.DiscardBufferedData();

        var referenceFilePath = GetFilePath(referenceFileName);
        using var expectedReader = new StreamReader(referenceFilePath);

        await Assert.MultipleAsync(async () =>
        {
            while (!resultReader.EndOfStream)
                Assert.That(await expectedReader.ReadLineAsync(), Is.EqualTo(await resultReader.ReadLineAsync()));
        });
    }

    [TestCase("input.csv", "reference_a_asc.csv", 0, SortDirection.Ascending)]
    [TestCase("input.csv", "reference_a_desc.csv", 0, SortDirection.Descending)]
    [TestCase("input.csv", "reference_c_asc.csv", 2, SortDirection.Ascending)]
    public async Task Ensure_Works_Using_Field_Index(
        string inputFileName,
        string referenceFileName,
        int fieldIndex,
        SortDirection sortDirection
    )
    {
        var inputFilePath = GetFilePath(inputFileName);
        using var reader = new StreamReader(inputFilePath);

        using var stream = new MemoryStream();
        await using var writer = new StreamWriter(stream);

        await new CsvSorter<int>(reader, fieldIndex)
            .Using(sortDirection)
            .ToWriterAsync(writer);

        using var resultReader = new StreamReader(stream);
        resultReader.BaseStream.Seek(0, SeekOrigin.Begin);
        resultReader.DiscardBufferedData();

        var referenceFilePath = GetFilePath(referenceFileName);
        using var referenceReader = new StreamReader(referenceFilePath);

        await Assert.MultipleAsync(async () =>
        {
            while (!resultReader.EndOfStream)
                Assert.That(await referenceReader.ReadLineAsync(), Is.EqualTo(await resultReader.ReadLineAsync()));
        });
    }

    [Test]
    public async Task Ensure_Cancellation_Token_Works()
    {
        var inputFilePath = GetFilePath("input.csv");

        var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        Assert.That(async () =>
            {
                using var reader = new StreamReader(inputFilePath);

                using var stream = new MemoryStream();
                await using var writer = new StreamWriter(stream);

                await new CsvSorter<int>(reader, 0)
                    .ToWriterAsync(writer, cts.Token);
            },
            Throws.TypeOf<OperationCanceledException>());
    }

    [Test]
    public void Ensure_Throws_If_Reader_Is_Null()
    {
        Assert.That(() => new CsvSorter<int>(null!, "name"), Throws.TypeOf<ArgumentNullException>());
    }

    [Test]
    public void Ensure_Throws_If_Field_Name_Is_Null_Or_Empty()
    {
        var reader = new StreamReader(new MemoryStream());

        Assert.That(() => new CsvSorter<int>(reader, ""), Throws.TypeOf<ArgumentNullException>());
        Assert.That(() => new CsvSorter<int>(reader, null!), Throws.TypeOf<ArgumentNullException>());
    }

    [Test]
    public void Ensure_Throws_If_Field_Index_Is_Negative()
    {
        var reader = new StreamReader(new MemoryStream());

        Assert.That(() => new CsvSorter<int>(reader, -1), Throws.TypeOf<ArgumentNullException>());
    }

    private string GetFilePath(string fileName)
    {
        return Path.Combine(AppContext.BaseDirectory, "TestFiles", fileName);
    }
}