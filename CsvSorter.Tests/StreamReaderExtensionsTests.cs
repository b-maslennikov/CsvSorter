using System;
using System.IO;
using NUnit.Framework;

namespace CsvSorter.Tests
{
    public class StreamReaderExtensionsTests
    {

        [Test]
        public void OrderByExtensionReturnsCsvSorterWithCorrectGenericType()
        {
            var sorter = new StreamReader(new MemoryStream())
                .OrderBy<DateTime>(0);

            Assert.NotNull(sorter);
            Assert.That(sorter.GetType() == typeof(CsvSorter<DateTime>));
        }

        [Test]
        public void OrderByDescendingExtensionReturnsCsvSorterWithCorrectGenericType()
        {
            var sorter = new StreamReader(new MemoryStream())
                .OrderByDescending<string>(0);

            Assert.NotNull(sorter);
            Assert.That(sorter.GetType() == typeof(CsvSorter<string>));
        }
    }
}
