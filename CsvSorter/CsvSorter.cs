using System.Globalization;
using System.Text.RegularExpressions;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;

namespace CsvSorter;

public class CsvSorter<T> where T : IComparable<T>
{
    private readonly bool _isFieldNameSet;
    private readonly string? _fieldName;
    private readonly int _fieldIndex;
    private readonly StreamReader _reader;
    private readonly IndexService<T> _indexService = new(new MemoryIndexProvider<T>());
    
    private string? _lineEnding;
    private CsvConfiguration _csvConfig = new(CultureInfo.InvariantCulture) { CountBytes = true };
    private TypeConverterOptions? _typeConverterOptions;
    private SortDirection _sortDirection = SortDirection.Ascending;
    
    private Action? _onIndexCreationStarted;
    private Action? _onIndexCreationFinished;
    private Action? _onSortingStarted;
    private Action? _onSortingFinished;

    private CsvSorter(StreamReader reader)
    {
        _reader = reader ?? throw new ArgumentNullException(nameof(reader));
    }

    internal CsvSorter(StreamReader reader, string fieldName) : this(reader)
    {
        if (string.IsNullOrWhiteSpace(fieldName))
            throw new ArgumentNullException(nameof(fieldName));

        _isFieldNameSet = true;
        _fieldName = fieldName;
    }

    internal CsvSorter(StreamReader reader, int fieldIndex) : this(reader)
    {
        if (fieldIndex < 0)
            throw new ArgumentNullException(nameof(fieldIndex));

        _isFieldNameSet = false;
        _fieldIndex = fieldIndex;
    }

    #region Using
    
    public CsvSorter<T> Using(IIndexProvider<T> provider)
    {
        _indexService.SetIndexProvider(provider);
        return this;
    }

    public CsvSorter<T> Using(IAsyncIndexProvider<T> provider)
    {
        _indexService.SetIndexProvider(provider);
        return this;
    }

    public CsvSorter<T> Using(CsvConfiguration config)
    {
        _csvConfig = config;
        _csvConfig.CountBytes = true;
        return this;
    }

    public CsvSorter<T> Using(TypeConverterOptions options)
    {
        _typeConverterOptions = options;
        return this;
    }

    public CsvSorter<T> Using(SortDirection sortDirection)
    {
        _sortDirection = sortDirection;
        return this;
    }
    
    #endregion

    #region Events
    
    public CsvSorter<T> OnIndexCreationStarted(Action action)
    {
        _onIndexCreationStarted = action;
        return this;
    }

    public CsvSorter<T> OnIndexCreationFinished(Action action)
    {
        _onIndexCreationFinished = action;
        return this;
    }

    public CsvSorter<T> OnSortingStarted(Action action)
    {
        _onSortingStarted = action;
        return this;
    }

    public CsvSorter<T> OnSortingFinished(Action action)
    {
        _onSortingFinished = action;
        return this;
    }
    
    #endregion

    public async Task ToWriterAsync(TextWriter writer, CancellationToken cancellationToken = default)
    {
        if (_reader == null)
            throw new NullReferenceException("StreamReader was not set");

        if (writer == null)
            throw new NullReferenceException("StreamWriter was not set");

        var bom = await GetBomAsync();

        _onIndexCreationStarted?.Invoke();

        await CreateIndexAsync(bom, cancellationToken);

        _onIndexCreationFinished?.Invoke();

        _onSortingStarted?.Invoke();

        var sortedIndexes = _indexService.GetSorted(_sortDirection, cancellationToken);

        await WriteSortedAsync(writer, sortedIndexes, cancellationToken);

        await _indexService.ClearAsync(cancellationToken);

        await writer.FlushAsync();
        SeekReader(0);

        _onSortingFinished?.Invoke();
    }

    public void ToWriter(TextWriter writer)
    {
        ToWriterAsync(writer).GetAwaiter().GetResult();
    }
    
    public async Task ToFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        await using var writer = new StreamWriter(filePath);
        await ToWriterAsync(writer, cancellationToken);
    }

    public void ToFile(string filePath)
    {
        ToFileAsync(filePath).GetAwaiter().GetResult();
    }

    private async Task WriteSortedAsync(TextWriter writer, IAsyncEnumerable<CsvSorterIndex<T>> sortedIndexes, CancellationToken cancellationToken)
    {
        await foreach (var index in sortedIndexes.WithCancellation(cancellationToken))
        {
            SeekReader(index.Offset);

            var c = new char[index.Length];
            await _reader.ReadAsync(c, 0, index.Length);

            await writer.WriteAsync(c);

            var currentLineEnding = string.Concat(c.Reverse().Take(_lineEnding!.Length).Reverse());
            if (_lineEnding != currentLineEnding)
                await writer.WriteAsync(_lineEnding);
        }
    }

    private async Task CreateIndexAsync(IReadOnlyCollection<byte>? bom, CancellationToken cancellationToken)
    {
        var bomSize = bom?.Count ?? 0;
        SeekReader(bomSize);

        using var csv = new CsvReader(_reader, _csvConfig, leaveOpen: true);

        if (_typeConverterOptions != null)
            csv.Context.TypeConverterOptionsCache.AddOptions<T>(_typeConverterOptions);

        long previousByteCount = bomSize;

        var getCsvField = _isFieldNameSet
            ? new Func<IReaderRow, T?>(r => r.GetField<T>(_fieldName))
            : new Func<IReaderRow, T?>(r => r.GetField<T>(_fieldIndex));

        if (await csv.ReadAsync())
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            _lineEnding ??= Regex.Match(csv.Parser.RawRecord, "(\r\n|\r|\n)$").Groups[0].Value;

            if (csv.Configuration.HasHeaderRecord)
            {
                csv.ReadHeader();
                _indexService.SetHeaderIndex(new CsvSorterIndex<T>
                {
                    Offset = previousByteCount,
                    Length = csv.Parser.RawRecord.Length
                });
            }
            else
            {
                await _indexService.AddIndexAsync(GetIndexFromRecord(), cancellationToken);
            }

            previousByteCount = csv.Parser.ByteCount + bomSize;
        }

        while (await csv.ReadAsync())
        {
            cancellationToken.ThrowIfCancellationRequested();
            await _indexService.AddIndexAsync(GetIndexFromRecord(), cancellationToken);
            previousByteCount = csv.Parser.ByteCount + bomSize;
        }

        return;

        CsvSorterIndex<T> GetIndexFromRecord() => new()
        {
            Value = getCsvField(csv),
            Offset = previousByteCount,
            Length = csv.Parser.RawRecord.Length
        };
    }

    private async Task<IReadOnlyCollection<byte>?> GetBomAsync()
    {
        _reader.Peek();
        SeekReader(0);

        var preamble = _reader.CurrentEncoding.GetPreamble();
        var c = new char[1];
        await _reader.ReadAsync(c, 0, 1);
        var possibleBomChar = _reader.CurrentEncoding.GetBytes(c);
        var hasBom = preamble.SequenceEqual(possibleBomChar);

        return hasBom
            ? preamble
            : null;
    }

    private void SeekReader(long offset)
    {
        _reader.BaseStream.Seek(offset, SeekOrigin.Begin);
        _reader.DiscardBufferedData();
    }
}