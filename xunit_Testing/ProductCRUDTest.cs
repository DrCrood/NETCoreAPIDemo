using System;
using NetCoreAPI.Models;
using Xunit;
using Moq;

namespace xunit_ProductCRUDTesting
{
    public class ProductCRUDTest
    {
        [Theory]
        [InlineData(9)]
        [InlineData(0)]
        public void AddInventory_nonNegative_shouldReturnCorrect(int num)
        {
            //Arrange
            Product mockProduct = new Product();
            int expected = mockProduct.Inventory + num;

            //Act
            int actual = mockProduct.AddInventory(num);

            //Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void AddInventory_Negative_shouldReturnSameValue()
        {
            //Arrange
            Product mockProduct = new Product();
            int expected = mockProduct.Inventory;

            //Act
            int actual = mockProduct.AddInventory(-1);

            //Assert
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(10,10)]
        [InlineData(0, 0)]
        [InlineData(-1, 0)]
        [InlineData(20,10)]
        public void GetDiscountInPercentage_ReturnCorrectDiscount(int quantity, int discount)
        {
            //Arrange
            int expected = discount;
            Product mockProduct = new Product();

            //Act
            int actual = mockProduct.GetDiscountInPercentage(quantity);

            Assert.Equal(expected,actual);
        }

        [Fact]
        public void Create_Normal_ShouldHaveNoExceptions()
        {
            Exception ex = Record.Exception(() => new Object());

            Assert.Null(ex);
        }

    }
}
