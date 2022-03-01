using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Contoso.TradingAggregator.Domain.extensions;
using FluentAssertions;

namespace Contoso.TradingAggregator.UnitTests.extensiontests
{
    [TestClass]
    public class FileNameExtensionsTests
    {
        [TestMethod]
        public void When_ParseDateFromFileName_And_InValid_FileName_Should_Return_Null()
        {
            //Arrange
            var fileName = "PowerPosition_20221-invalid-filename.csv";

            //Act
            var actualDate = fileName.ToDateTime();

            //Assert
            actualDate.Should().BeNull();
        }

        [TestMethod]
        public void When_ParseDateFromFileName_And_Valid_FileName_Should_Return_DateTime()
        {
            //Arrange
            var fileName = "PowerPosition_20221225_0833.csv";
            var expectedDate = new DateTime(2022, 12, 25, 08, 33, 0);

            //Act
            var actualDate = fileName.ToDateTime();

            //Assert
            actualDate.Value.Should().Be(expectedDate);
        }
    }
}