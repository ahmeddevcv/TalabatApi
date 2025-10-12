using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talabat.Core.Entities;
using Talabat.Core.Repositories;
using Talabat.Core.Specifiactions;
using Talabat.Repository.Data;

namespace Talabat.Repository
{
    public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
    {
        private readonly StoreContext dbContext;

        public GenericRepository(StoreContext _dbContext)
        {
            dbContext = _dbContext;
        }
        public async Task<IReadOnlyList<T>> GetAllAsync()
        {
            if(typeof(T)==typeof(Product) )
                return (IReadOnlyList<T>) await dbContext.products.Include(p=>p.ProductBrand).Include(t=>t.ProductType).ToListAsync();
            return await dbContext.Set<T>().ToListAsync();
        }



        public async Task<T> GetByIdAsync(int id)
        {
            //return await dbContext.Set<T>().FirstOrDefaultAsync(x => x.Id == id);
            //return await dbContext.Set<T>().Where(x=> x.Id == id).FirstOrDefaultAsync();
            return await dbContext.Set<T>().FindAsync(id);
        }
        public async Task<IReadOnlyList<T>> GetAllWithSpecAsync(ISpecification<T> spec)
        {
            return await ApplySpecification(spec).ToListAsync();
        }
        public async Task<T> GetEntityWithSpecAsync(ISpecification<T> spec)
        {
            return await ApplySpecification(spec).FirstOrDefaultAsync();
        }
        //count
        public async Task<int> GetCountWithSpecAsync(ISpecification<T> spec)
        {
            return await ApplySpecification(spec).CountAsync();
        }

        ///////////////////////////////////////////////////////////////////////
        private IQueryable<T> ApplySpecification(ISpecification<T> spec)
        {
            return SpecificationEvalutor<T>.GetQuery(dbContext.Set<T>(), spec);
        }

        public async Task Add(T entity)
            => await dbContext.Set<T>().AddAsync(entity);

        public void Update(T entity)
            => dbContext.Set<T>().Update(entity);

        public void Delete(T entity)
            => dbContext.Set<T>().Remove(entity);
    }
}
