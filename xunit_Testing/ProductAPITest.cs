using System;
using System.Collections.Generic;
using System.Linq;
using NetCoreAPI.Models;
using NetCoreAPI.Controllers.v1;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NetCoreAPI.Interfaces;
using RockLib.Logging;
using Moq;
using Xunit;

namespace xunit_ProductAPITesting
{
    public class ProductAPITest
    {
        //Test method naming: MethedBeingTested_ScenarioUnderTest_ExpectedResults
        //All tests shall use simple inputs and avoid logic code in tests
        [Fact]
        public void GetProduct_GetRequest_ReturnListofProduct()
        {
            //Arrange
            var stubProducts = GetQueryableMockDbSet<Product>(GetListofTestProducts());
            var stubDbContext = new Mock<ProductDBContext>(new DbContextOptions<ProductDBContext>());
            var stubLogger = new Mock<ILogger>();

            stubDbContext.Setup(db => db.Products).Returns(stubProducts.Object);
            var mockController = new ProductsController(stubDbContext.Object, stubLogger.Object);

            //Act
            var result = mockController.GetProducts();

            //Assert
            Assert.Equal(3, result.Count());
        }

        [Fact]
        public void GetProductAsync_GetRequest_ReturnListofProduct()
        {
            //EF core async method is hard to test, need to build async query provider for DbSet.
            //Using SQLite is easier to implement

            //Arrange
            DbSqliteInMemory DbContext = new DbSqliteInMemory(new DbContextOptionsBuilder<ProductDBContext>().UseSqlite("Data Source=APITest.db").Options);
            using var stubContext = new ProductDBContext(DbContext.ContextOptions);
            var mockProduct = stubContext.Products.ToList();
            var stubLogger = new Mock<ILogger>();            
            var mockController = new ProductsController(stubContext, stubLogger.Object);

            //Act
            var result = mockController.GetProductsAsync().Result;

            //Assert
            Assert.Equal(3, result.Count());
        }

        [Fact]
        public void GetProductAsync_GetRequestUsingAsyncProvider_ReturnListofProduct()
        {
            //using async query provider to mock EF core async method.

            //Arrange
            var stubProducts = GetAsyncQueryableMockDbSet<Product>(GetListofTestProducts());
            var stubDbContext = new Mock<ProductDBContext>(new DbContextOptions<ProductDBContext>());
            var stubLogger = new Mock<ILogger>();

            stubDbContext.Setup(db => db.Products).Returns(stubProducts.Object);
            var mockController = new ProductsController(stubDbContext.Object, stubLogger.Object);

            //var t = stubDbContext.Object.Products.ToList();

            //Act
            var result = mockController.GetProductsAsync().Result;

            //Assert
            Assert.Equal(3, result.Count());
        }

        private List<Product> GetListofTestProducts()
        {
            return new List<Product>()
            {
                new Product(){ Id = 1, Name = "Hammer", Price = 1, Inventory = 4},
                new Product(){ Id = 2, Name = "Wrench", Price = 2, Inventory = 5},
                new Product(){ Id = 3, Name = "Saw", Price = 3, Inventory = 6}
            };
        }

        private Mock<DbSet<T>> GetQueryableMockDbSet<T>(List<T> sourceList) where T : class
        {
            var queryable = sourceList.AsQueryable();

            var dbSet = new Mock<DbSet<T>>();
            //dbSet is not a real dbSet, need to set the methods for it to work
            //add IQueryable to dbSet
            dbSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
            dbSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
            dbSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            dbSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => queryable.GetEnumerator());

            return dbSet;
        }

        private Mock<DbSet<T>> GetAsyncQueryableMockDbSet<T>(List<T> sourceList) where T : class
        {
            var queryable = sourceList.AsQueryable();

            var dbSet = new Mock<DbSet<T>>();
            //dbSet is not a real dbSet, need to set the methods for it to work
            //add IQueryable to dbSet

            dbSet.As<IAsyncEnumerable<T>>().Setup(m => m.GetAsyncEnumerator(default)).Returns(new DbAsyncEnumerator<T>(queryable.GetEnumerator()));
            dbSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(new DbAsyncQueryProvider<T>(queryable.Provider));
            dbSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
            dbSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            dbSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => queryable.GetEnumerator());

            return dbSet;
        }
    }
}
