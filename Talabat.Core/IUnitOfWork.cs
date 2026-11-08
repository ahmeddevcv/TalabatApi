using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talabat.Core.Entities;
using Talabat.Core.Entities.Order_Aggregate;
using Talabat.Core.Repositories;

namespace Talabat.Core
{
    public interface IUnitOfWork:IAsyncDisposable
    {
        //signature
        IGenericRepository<TEntity> Repository<TEntity>() where TEntity : BaseEntity;

        ///public IGenericRepository<Product> ProductsRepo { get; set; }
        ///public IGenericRepository<ProductBrand> BrandsRepo { get; set; }
        ///public IGenericRepository<ProductType> TypesRepo { get; set; }
        ///public IGenericRepository<DeliveryMethod> DeliveryMethodsRepo { get; set; }
        ///public IGenericRepository<OrderItem> OrderItemsRepo { get; set; }
        ///public IGenericRepository<Order> OrdersRepo { get; set; }

        public Task<int> Complete();
    }
}
