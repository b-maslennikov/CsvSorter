using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using CsvSorter.Entities;

namespace CsvSorter
{
    public class CsvSorter<T> where T : IComparable
    {
        private readonly bool _isFieldNameSet;
        private readonly string _fieldName;
        private readonly int _fieldIndex;
        private readonly bool _descending;
        private readonly IList<CsvSorterIndex<T>> _headerIndex;
        private readonly StreamReader _reader;
        private string _lineEnding;
        private IIndexProvider<T> _indexProvider;
        private CsvConfiguration _csvConfig;
        private TypeConverterOptions _typeConverterOptions;

        private CsvSorter(StreamReader reader, bool descending)
        {
            _reader = reader;
            _descending = descending;

            _csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture);
            _headerIndex = new List<CsvSorterIndex<T>>();
            _indexProvider = new MemoryIndexProvider<T>();

            FixCsvConfig();
        }

        internal CsvSorter(StreamReader reader, bool descending, string fieldName) : this(reader, descending)
        {
            if (string.IsNullOrWhiteSpace(fieldName))
                throw new ArgumentException("CSV field name can not be empty.", nameof(fieldName));

            _isFieldNameSet = true;
            _fieldName = fieldName;
        }

        internal CsvSorter(StreamReader reader, bool descending, int fieldIndex) : this(reader, descending)
        {
            if (fieldIndex < 0)
                throw new ArgumentException("CSV field index can not be negative.", nameof(fieldIndex));

            _isFieldNameSet = false;
            _fieldIndex = fieldIndex;
        }

        public CsvSorter<T> Using(IIndexProvider<T> provider)
        {
            _indexProvider = provider;
            return this;
        }

        public CsvSorter<T> Using(CsvConfiguration config)
        {
            _csvConfig = config;
            FixCsvConfig();
            return this;
        }

        public CsvSorter<T> Using(TypeConverterOptions options)
        {
            _typeConverterOptions = options;
            return this;
        }

        public void ToFile(string filePath)
        {
            using var writer = new StreamWriter(filePath);
            ToWriter(writer);
        }

        public void ToWriter(TextWriter writer)
        {
            if (_reader == null)
                throw new NullReferenceException("StreamReader was not set");

            if (writer == null)
                throw new NullReferenceException("StreamWriter was not set");

            var bom = GetBom();

            CreateIndex(bom);

            var sortedIndexes = _headerIndex
                .Concat(_indexProvider.GetSorted(_descending));

            WriteSorted(writer, sortedIndexes);

            _indexProvider.Clear();

            writer.Flush();
            SeekReader(0);
        }

        private void WriteSorted(TextWriter writer, IEnumerable<CsvSorterIndex<T>> sortedIndexes)
        {
            foreach (var index in sortedIndexes)
            {
                SeekReader(index.Offset);

                var c = new char[index.Length];
                _reader.Read(c, 0, index.Length);

                writer.Write(c);
                
                var currentLineEnding = string.Concat(c.Reverse().Take(_lineEnding.Length).Reverse());
                if (_lineEnding != currentLineEnding)
                    writer.Write(_lineEnding);
            }
        }

        private void CreateIndex(IReadOnlyCollection<byte> bom)
        {
            var bomSize = bom?.Count ?? 0;
            SeekReader(bomSize);

            using var csv = new CsvReader(_reader, _csvConfig);

            if (_typeConverterOptions != null)
                csv.Context.TypeConverterOptionsCache.AddOptions<T>(_typeConverterOptions);

            long previousByteCount = bomSize;

            var getCsvField = _isFieldNameSet
                ? new Func<IReaderRow, T>(r => r.GetField<T>(_fieldName))
                : new Func<IReaderRow, T>(r => r.GetField<T>(_fieldIndex));

            CsvSorterIndex<T> GetRecord()
            {
                return new()
                {
                    Value = getCsvField(csv),
                    Offset = previousByteCount,
                    Length = csv.Parser.RawRecord.Length
                };
            }

            if (csv.Read())
            {
                _lineEnding ??= Regex.Match(csv.Parser.RawRecord, "(\r\n|\r|\n)$").Groups[0].Value;

                if (csv.Configuration.HasHeaderRecord)
                {
                    csv.ReadHeader();
                    _headerIndex.Add(new CsvSorterIndex<T>
                    {
                        Offset = previousByteCount,
                        Length = csv.Parser.RawRecord.Length
                    });
                }
                else
                {
                    _indexProvider.Add(GetRecord());
                }

                previousByteCount = csv.Parser.ByteCount + bomSize;
            }

            while (csv.Read())
            {
                _indexProvider.Add(GetRecord());
                previousByteCount = csv.Parser.ByteCount + bomSize;
            }
        }

        private IReadOnlyCollection<byte> GetBom()
        {
            _reader.Peek();
            SeekReader(0);

            var preamble = _reader.CurrentEncoding.GetPreamble();
            var c = new char[1];
            _reader.Read(c, 0, 1);
            var possibleBomChar = _reader.CurrentEncoding.GetBytes(c);
            var hasBom = preamble.SequenceEqual(possibleBomChar);

            return hasBom
                ? preamble
                : null;
        }

        private void FixCsvConfig()
        {
            _csvConfig.CountBytes = true;
            _csvConfig.LeaveOpen = true;
        }

        private void SeekReader(long offset)
        {
            _reader.BaseStream.Seek(offset, SeekOrigin.Begin);
            _reader.DiscardBufferedData();
        }
    }
}