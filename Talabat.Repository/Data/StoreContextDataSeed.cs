using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
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
            // Brands
            if (!dbContext.productBrands.Any())
            {
                var brandDataPath = "../Talabat.Repository/Data/DataSeed/brands.json";
                if (!File.Exists(brandDataPath)) brandDataPath = Path.Combine(AppContext.BaseDirectory, "DataSeed", "brands.json");
                if (File.Exists(brandDataPath))
                {
                    var brandData = await File.ReadAllTextAsync(brandDataPath);
                    var brands = JsonSerializer.Deserialize<List<ProductBrand>>(brandData);
                    if (brands?.Count > 0)
                    {
                        foreach (var brand in brands)
                            await dbContext.Set<ProductBrand>().AddAsync(brand);
                        await dbContext.SaveChangesAsync();
                    }
                }
            }

            // Types
            if (!dbContext.productsType.Any())
            {
                var typeDataPath = "../Talabat.Repository/Data/DataSeed/types.json";
                if (!File.Exists(typeDataPath)) typeDataPath = Path.Combine(AppContext.BaseDirectory, "DataSeed", "types.json");
                if (File.Exists(typeDataPath))
                {
                    var typeData = await File.ReadAllTextAsync(typeDataPath);
                    var types = JsonSerializer.Deserialize<List<ProductType>>(typeData);
                    if (types?.Count > 0)
                    {
                        foreach (var typ in types)
                            await dbContext.Set<ProductType>().AddAsync(typ);
                        await dbContext.SaveChangesAsync();
                    }
                }
            }

            // Products & Auto-restore missing products
            var productDataPath = "../Talabat.Repository/Data/DataSeed/products.json";
            if (!File.Exists(productDataPath)) productDataPath = Path.Combine(AppContext.BaseDirectory, "DataSeed", "products.json");
            if (File.Exists(productDataPath))
            {
                var productData = await File.ReadAllTextAsync(productDataPath);
                var seedProducts = JsonSerializer.Deserialize<List<Product>>(productData);
                if (seedProducts?.Count > 0)
                {
                    if (!dbContext.products.Any())
                    {
                        foreach (var pro in seedProducts)
                            await dbContext.Set<Product>().AddAsync(pro);
                        await dbContext.SaveChangesAsync();
                    }
                    else
                    {
                        // Restore any accidentally deleted seed products (e.g. from previous edit bugs)
                        var existingNames = await dbContext.products.Select(p => p.Name).ToListAsync();
                        var missing = seedProducts.Where(p => !existingNames.Contains(p.Name)).ToList();
                        if (missing.Count > 0)
                        {
                            foreach (var pro in missing)
                                await dbContext.Set<Product>().AddAsync(pro);
                            await dbContext.SaveChangesAsync();
                        }
                    }
                }
            }

            // DeliveryMethods
            if (!dbContext.deliveryMethods.Any())
            {
                var deliveryMethodsDataPath = "../Talabat.Repository/Data/DataSeed/delivery.json";
                if (!File.Exists(deliveryMethodsDataPath)) deliveryMethodsDataPath = Path.Combine(AppContext.BaseDirectory, "DataSeed", "delivery.json");
                if (File.Exists(deliveryMethodsDataPath))
                {
                    var deliveryMethodsData = await File.ReadAllTextAsync(deliveryMethodsDataPath);
                    var deliveryMethods = JsonSerializer.Deserialize<List<DeliveryMethod>>(deliveryMethodsData);
                    if (deliveryMethods?.Count > 0)
                    {
                        foreach (var deliveryMethod in deliveryMethods)
                            await dbContext.Set<DeliveryMethod>().AddAsync(deliveryMethod);
                        await dbContext.SaveChangesAsync();
                    }
                }
            }
        }
    }
}
