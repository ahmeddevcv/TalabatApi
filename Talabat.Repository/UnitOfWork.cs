using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talabat.Core;
using Talabat.Core.Entities;
using Talabat.Core.Entities.Order_Aggregate;
using Talabat.Core.Repositories;
using Talabat.Repository.Data;

namespace Talabat.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly StoreContext _dbContext;
        private readonly Hashtable _repositories;

        ///public IGenericRepository<Product> ProductsRepo { get; set; }
        ///public IGenericRepository<ProductBrand> BrandsRepo { get;set; }
        ///public IGenericRepository<ProductType> TypesRepo { get;set; }
        ///public IGenericRepository<DeliveryMethod> DeliveryMethodsRepo { get;set; }
        ///public IGenericRepository<OrderItem> OrderItemsRepo { get;set; }
        ///public IGenericRepository<Order> OrdersRepo { get;set; }

        public UnitOfWork(StoreContext dbContext)
        {
            _dbContext = dbContext;
            _repositories = new Hashtable();
            ///ProductsRepo = new GenericRepository<Product>(_dbContext);
            ///BrandsRepo = new GenericRepository<ProductBrand>(_dbContext);
            ///TypesRepo = new GenericRepository<ProductType>(_dbContext);
            ///DeliveryMethodsRepo = new GenericRepository<DeliveryMethod>(_dbContext);
            ///OrderItemsRepo = new GenericRepository<OrderItem>(_dbContext);
            ///OrdersRepo = new GenericRepository<Order>(_dbContext);
        }
        public IGenericRepository<TEntity> Repository<TEntity>() where TEntity : BaseEntity
        {
            var type=typeof(TEntity).Name;//Product
            if (!_repositories.ContainsKey(type))
            {
                var repository=new GenericRepository<TEntity>(_dbContext);
                _repositories.Add(type, repository);
            }
            return _repositories[type] as IGenericRepository<TEntity>;
        }

        public async Task<int> Complete()
        {
            try
            {
                return await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ SaveChangesAsync failed: " + ex.Message);
                Console.WriteLine("🔍 Inner: " + ex.InnerException?.Message);
                throw;
            }
        }

        public async ValueTask DisposeAsync()
            => await _dbContext.DisposeAsync();
    }
}
