using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Talabat.Core.Entities;
using Talabat.Core.Entities.Order_Aggregate;

namespace Talabat.Repository.Data
{
    public static class StoreContextDataSeed
    {
        public static async Task SeedAsync(StoreContext dbContext)
        {
            //check if brand have data or not to avoid double seeding
            if (!dbContext.productBrands.Any()) //not if have date it will  excute else not
            {//read files
                var brandData = File.ReadAllText("../Talabat.Repository/Data/DataSeed/brands.json");//read file
                var brands = JsonSerializer.Deserialize<List<ProductBrand>>(brandData);//list<ProductBrand>
                ////if (brands is not null && brands.Count > 0) ////brands may be null or may contain empty list
                if (brands?.Count > 0) ////brands may be null or may contain empty list
                {
                    foreach (var brand in brands)
                        await dbContext.Set<ProductBrand>().AddAsync(brand); //put in db
                                                                             //await dbContext.productBrands.AddAsync(brand); //put in db
                                                                             //table in db==> Set<ProductBrand>() == productBrands
                }
                await dbContext.SaveChangesAsync();
            }
            //check if brand have data or not to avoid double seeding
            if (!dbContext.productsType.Any()) //not if have date it will  excute else not
            {//read files
                var typeData = File.ReadAllText("../Talabat.Repository/Data/DataSeed/types.json");//read file
                var types = JsonSerializer.Deserialize<List<ProductType>>(typeData);//list<ProductBrand>
                if (types?.Count > 0) ////brands may be null or may contain empty list
                {
                    foreach (var typ in types)
                        await dbContext.Set<ProductType>().AddAsync(typ); //put in db
                }
                await dbContext.SaveChangesAsync();
            }
            //check if brand have data or not to avoid double seeding
            if (!dbContext.products.Any()) //not if have date it will  excute else not
            {//read files
                var productData = File.ReadAllText("../Talabat.Repository/Data/DataSeed/products.json");//read file
                var products = JsonSerializer.Deserialize<List<Product>>(productData);//list<ProductBrand>
                if (products?.Count > 0) ////brands may be null or may contain empty list
                {
                    foreach (var pro in products)
                        await dbContext.Set<Product>().AddAsync(pro); //put in db
                }
                await dbContext.SaveChangesAsync();
            }

            // DeliveryMethods
            if (!dbContext.deliveryMethods.Any()) //not if have date it will  excute else not
            {//read files
                var deliveryMethodsData = File.ReadAllText("../Talabat.Repository/Data/DataSeed/delivery.json");//read file
                var deliveryMethods = JsonSerializer.Deserialize<List<DeliveryMethod>>(deliveryMethodsData);//list<ProductBrand>
                if (deliveryMethods?.Count > 0) ////brands may be null or may contain empty list
                {
                    foreach (var deliveryMethod in deliveryMethods)
                        await dbContext.Set<DeliveryMethod>().AddAsync(deliveryMethod); //put in db
                }
                await dbContext.SaveChangesAsync();
            }

        }
    }
}
