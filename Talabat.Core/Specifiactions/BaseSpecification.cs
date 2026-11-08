using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Talabat.Core.Entities;

namespace Talabat.Core.Specifiactions
{
    public class BaseSpecification<T> : ISpecification<T> where T : BaseEntity
    {
        public Expression<Func<T, bool>> Criteria { get; set; }

        public List<Expression<Func<T, object>>> Includes { get; set; }= new List<Expression<Func<T, object>>>() { };
        //sorting
        public Expression<Func<T, object>> OrderBy { get; set; }

        public Expression<Func<T, object>> OrderByDescending { get; set; }
        public int Skip { get; set; } //paging
        public int Take { get; set; }//paging
        public bool IsPaginatioEnabled { get; set; }//paging

        //GetAll
        public BaseSpecification()
        {
            //Includes = new List<Expression<Func<T, object>>>() { };
            //Includes.Add(p => p.ProductBrand);
            //Includes.Add(p => p.ProductType); // i will add in new class
        }
        //GetById
        public BaseSpecification(Expression<Func<T, bool>> criteriaExpression)
        {
            Criteria = criteriaExpression;
            //Includes = new List<Expression<Func<T, object>>>() { };
        }
        //functions Of sorting
        public void AddOrderBy(Expression<Func<T, object>> orderByExpression)
        {
            OrderBy = orderByExpression;
        }        
        public void AddOrderByDescending(Expression<Func<T, object>> orderByDescendingExpression)
        {
            OrderByDescending = orderByDescendingExpression;
        }
        //paging
        public void ApplyPagination(int skip,int take)
        {
            IsPaginatioEnabled = true;
            Skip = skip;
            Take = take;
        }

    }
}
