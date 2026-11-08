using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Talabat.Core.Entities;
using Talabat.Core.Entities.Order_Aggregate;
using Talabat.Repository.Data.Config;

namespace Talabat.Repository.Data
{
    public class StoreContext : DbContext
    {
        public StoreContext(DbContextOptions<StoreContext> options) : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            ////fluent API
            //modelBuilder.ApplyConfiguration(new ProductConfiguration());
            //modelBuilder.ApplyConfiguration(new ProductBrandConfiguration());
            //modelBuilder.ApplyConfiguration(new ProductTypeConfiguration());
            //modelBuilder.ApplyConfiguration(new OrderConfiguration());
            //modelBuilder.ApplyConfiguration(new OrderItemConfiguartion());
            //modelBuilder.ApplyConfiguration(new DeliveryMethodConfiguartion());
            ////another way to call configured Classes
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }

        public DbSet<Product> products { get; set; }
        public DbSet<ProductType> productsType { get; set; }
        public DbSet<ProductBrand> productBrands { get; set; }
        public DbSet<Order> orders { get; set; }
        public DbSet<OrderItem> orderItems { get; set; }
        public DbSet<DeliveryMethod> deliveryMethods { get; set; }

    }
}
