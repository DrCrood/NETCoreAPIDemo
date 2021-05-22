using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using NetCoreAPI.Models;

namespace xunit_ProductAPITesting
{
    public class DbSqliteInMemory
    {
        public DbContextOptions<ProductDBContext> ContextOptions { get; }

        public DbSqliteInMemory(DbContextOptions<ProductDBContext> options)
        {
            ContextOptions = options;
            Seed();
        }
        
        private void Seed()
        {
            using (var context = new ProductDBContext(ContextOptions))
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                context.Add(new Product() { Id = 1, Name = "Wranch", Inventory = 2, Price = 5 });
                context.Add(new Product() { Id = 2, Name = "Saw", Inventory = 3, Price = 6 });
                context.Add(new Product() { Id = 3, Name = "Hammer", Inventory = 4, Price = 7 });

                context.SaveChanges();
            }
        }
    }

}
