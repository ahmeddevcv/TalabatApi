using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Talabat.Core.Entities
{
    public class Product:BaseEntity
    {
        //Name  Description  PictureUrl Price  
        public string Name { get; set; }
        public string Description { get; set; }
        public string PictureUrl { get; set; }
        public decimal Price { get; set; }

        //[ForeignKey("ProductBrand")] //// no need because I am naming fk( EntityName + EntityPK )
        public int ProductBrandId { get; set; } //// fk not allow null(cascade)
        public ProductBrand ProductBrand { get; set; } //// navigation property[one]
        public int ProductTypeId { get; set; } //// fk not allow null(cascade)
        public ProductType ProductType { get; set; } //// navigation property[one]
    }
}
