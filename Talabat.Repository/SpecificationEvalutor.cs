using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talabat.Core.Entities;
using Talabat.Core.Specifiactions;

namespace Talabat.Repository
{
    /// this class to implement functions in ISpecifiaction
    /// dbContext.products.Where(x=> x.Id == id).Include(p => p.ProductBrand).Include(t => t.ProductType)
    /// dbContext.products.Where(x=> x.Id == id).Skip(skip).Take(take).Include(p => p.ProductBrand).Include(t => t.ProductType)
    public static class SpecificationEvalutor<TEntity> where TEntity : BaseEntity
    {
        public static IQueryable<TEntity> GetQuery(IQueryable<TEntity> inputQuery,ISpecification<TEntity> spec)
        {
            var query = inputQuery; //dbContect.Products
            if (spec.Criteria is not null)  //x=> x.Id == id
                query =query.Where(spec.Criteria);// dbContext.products.Where(x=> x.Id == id)

            //sorting
            if (spec.OrderBy is not null) //p=>p.price
                query = query.OrderBy(spec.OrderBy);//dbContext.products.Where(x=> x.Id == id).OrderBy(p=>p.price)
            if(spec.OrderByDescending is not null) 
                query = query.OrderByDescending(spec.OrderByDescending);

            //paging
            if (spec.IsPaginatioEnabled==true)
                query = query.Skip(spec.Skip).Take(spec.Take);

            //add 2 includes
            query = spec.Includes.Aggregate(query, (current, includeExpression) => current.Include(includeExpression));
            //Include(p => p.ProductBrand) 1st
            //Include(t => t.ProductType) 2nd
            return query;
        }
    }
}
