using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talabat.Core.Entities;

namespace Talabat.Core.Specifiactions
{
    public class ProductWithBrandAndTypeSpecifications:BaseSpecification<Product>
    {
        //GetAllProducts
        public ProductWithBrandAndTypeSpecifications(ProductSpecParams specParams)
            : base(p =>//filter
                    (string.IsNullOrEmpty(specParams.Search)||p.Name.ToLower().Contains(specParams.Search)) &&
                    (!specParams.BrandId.HasValue || p.ProductBrandId == specParams.BrandId.Value) &&
                    (!specParams.TypeId.HasValue || p.ProductTypeId == specParams.TypeId.Value)
                 )
        {
            Includes.Add(p => p.ProductBrand);
            Includes.Add(p => p.ProductType);
            //sorting
            AddOrderBy(p => p.Name);
            if (!string.IsNullOrEmpty(specParams.Sort))
            {
                switch (specParams.Sort)
                {
                    case "priceAsc":
                        AddOrderBy(p => p.Price);break;
                    case "priceDesc":
                        AddOrderByDescending(p => p.Price);break;
                    default:
                        AddOrderBy(p=>p.Name); break;
                }
            }
            //paging
            //total pages =100
            //Pagesize        = 10
            //Pageindex       = 3       10 * (3-1)
            ApplyPagination(specParams.PageSize * (specParams.PageIndex - 1), specParams.PageSize);
        }
        public ProductWithBrandAndTypeSpecifications(int id):base(p=>p.Id==id)
        {
            Includes.Add(p => p.ProductBrand);
            Includes.Add(p => p.ProductType);
        }
    }
}
