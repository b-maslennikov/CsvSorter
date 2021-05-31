using System;
using System.IO;
using System.Reflection;
using CsvHelper.TypeConversion;
using NUnit.Framework;

namespace CsvSorter.Tests
{
    public class CsvSorterTests
    {
        private readonly string _directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        private string GetFilePath(string fileName)
        {
            return Path.Combine(_directory, "TestFiles", fileName);
        }

        [TestCase("input.csv", "a", false)]
        public void DoesNotCloseReaderAndWriter(string inputFileName, string filedName, bool descending)
        {
            var inputFilePath = GetFilePath(inputFileName);
            using var reader = new StreamReader(inputFilePath);

            using var stream = new MemoryStream();
            using var writer = new StreamWriter(stream);
 
            new CsvSorter<int>(reader, descending, filedName)
                .ToWriter(writer);

            Assert.That(reader.BaseStream.CanRead);
            Assert.That(writer.BaseStream.CanRead);
        }
        
        [TestCase("input.csv", "output_a_asc.csv", "a", false)]
        [TestCase("input.csv", "output_a_desc.csv", "a", true)]
        [TestCase("input.csv", "output_c_asc.csv", "c", false)]
        public void ResultLengthsAreEqual(string inputFileName, string outputFileName, string filedName, bool descending)
        {
            var inputFilePath = GetFilePath(inputFileName);
            using var reader = new StreamReader(inputFilePath);

            using var stream = new MemoryStream();
            using var writer = new StreamWriter(stream);

            new CsvSorter<int>(reader, descending, filedName)
                .ToWriter(writer);

            var expectedFilePath = GetFilePath(outputFileName);
            var resultLength = File.ReadAllBytes(expectedFilePath).Length;

            Assert.That(stream.Length == resultLength);
        }

        [TestCase("input.csv", "output_a_asc.csv", "a", false)]
        [TestCase("input.csv", "output_a_desc.csv", "a", true)]
        [TestCase("input.csv", "output_c_asc.csv", "c", false)]
        public void FieldNameGivesExpectedResult(string inputFileName, string outputFileName, string filedName, bool descending)
        {
            var inputFilePath = GetFilePath(inputFileName);
            using var reader = new StreamReader(inputFilePath);

            using var stream = new MemoryStream();
            using var writer = new StreamWriter(stream);

            new CsvSorter<int>(reader, descending, filedName)
                .ToWriter(writer);

            var expectedFilePath = GetFilePath(outputFileName);
            using var resultReader = new StreamReader(stream);
            resultReader.BaseStream.Seek(0, SeekOrigin.Begin);
            resultReader.DiscardBufferedData();
            
            using var expectedReader = new StreamReader(expectedFilePath);

            while (!resultReader.EndOfStream)
                Assert.That(expectedReader.ReadLine() == resultReader.ReadLine());
        }

        [TestCase("input.csv", "output_d_desc.csv", "d", true)]
        public void FieldNameGivesExpectedResultWithConverterOptions(string inputFileName, string outputFileName, string filedName, bool descending)
        {
            var inputFilePath = GetFilePath(inputFileName);
            using var reader = new StreamReader(inputFilePath);

            using var stream = new MemoryStream();
            using var writer = new StreamWriter(stream);

            var dateTimeOptions = new TypeConverterOptions {Formats = new[] {"dd_MM_yyyy"}};
            
            new CsvSorter<DateTime>(reader, descending, filedName)
                .Using(dateTimeOptions)
                .ToWriter(writer);

            var expectedFilePath = GetFilePath(outputFileName);
            using var resultReader = new StreamReader(stream);
            resultReader.BaseStream.Seek(0, SeekOrigin.Begin);
            resultReader.DiscardBufferedData();

            using var expectedReader = new StreamReader(expectedFilePath);

            while (!resultReader.EndOfStream)
                Assert.That(expectedReader.ReadLine() == resultReader.ReadLine());
        }

        [TestCase("input.csv", "output_a_asc.csv", 0, false)]
        [TestCase("input.csv", "output_a_desc.csv", 0, true)]
        [TestCase("input.csv", "output_c_asc.csv", 2, false)]
        public void FieldIndexGivesExpectedResult(string inputFileName, string outputFileName, int fieldIndex, bool descending)
        {
            var inputFilePath = GetFilePath(inputFileName);
            using var reader = new StreamReader(inputFilePath);

            using var stream = new MemoryStream();
            using var writer = new StreamWriter(stream);

            new CsvSorter<int>(reader, descending, fieldIndex)
                .ToWriter(writer);

            var expectedFilePath = GetFilePath(outputFileName);
            using var resultReader = new StreamReader(stream);
            resultReader.BaseStream.Seek(0, SeekOrigin.Begin);
            resultReader.DiscardBufferedData();

            using var expectedReader = new StreamReader(expectedFilePath);

            while (!resultReader.EndOfStream)
                Assert.That(expectedReader.ReadLine() == resultReader.ReadLine());
        }

        [Test]
        public void ThrowsIfFieldNameIsEmpty()
        {
            Assert.Throws<ArgumentException>(() => { new CsvSorter<int>(null,true, ""); });
        }

        [Test]
        public void ThrowsIfFieldIndexIsNegative()
        {
            Assert.Throws<ArgumentException>(() => { new CsvSorter<int>(null, true, -1); });
        }
    }
}