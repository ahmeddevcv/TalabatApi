using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talabat.Core.Entities;

namespace Talabat.Core.Repositories
{
    public interface IBasketRepository
    {
        Task<CustmerBasket?> GetBasketAsync(string basketId);
        Task<CustmerBasket?> UpdateBasketAsync(CustmerBasket basket);
        Task<bool> DeleteBasketAsync(string basketId);
    }
}
