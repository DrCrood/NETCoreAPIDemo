using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetCoreAPI.Models
{
    public class ProductDBContext : DbContext
    {
        public ProductDBContext(DbContextOptions<ProductDBContext> options) : base(options)
        {

        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Map table names
            modelBuilder.Entity<Product>().ToTable("Products", "Product");
            base.OnModelCreating(modelBuilder);
        }

        //need to be virtual for working with Moq
        public virtual DbSet<Product> Products { get; set; }
    }
}
