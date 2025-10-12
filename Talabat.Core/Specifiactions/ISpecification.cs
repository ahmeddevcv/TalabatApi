using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Talabat.Core.Entities;



///dbContext.products.Where(x=> x.Id == id).Skip(skip).Take(take).Include(p => p.ProductBrand).Include(t => t.ProductType)
///this query,I am gonna specificate it
namespace Talabat.Core.Specifiactions
{
    public interface ISpecification<T>where T : BaseEntity
    {
        public Expression<Func<T, bool>> Criteria { get; set; }
        public List<Expression<Func<T, object>>> Includes { get; set;}
        public Expression<Func<T, object>> OrderBy { get; set;}
        public Expression<Func<T, object>> OrderByDescending { get; set; }
        //public Expression<Func<T, object>> OrderByDescending
        public int Skip { get; set; }
        public int Take { get; set; }
        public bool IsPaginatioEnabled { get; set; }

    }
}
