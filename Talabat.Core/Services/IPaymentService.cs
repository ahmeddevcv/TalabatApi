using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talabat.Core.Entities;

namespace Talabat.Core.Services
{
    public interface IPaymentService
    {
        /// <summary>
        /// to create paymentIntent, you need amount,options
        /// basketId to catch items,amount in basket
        /// </summary>
        Task<CustmerBasket?> CreateOrUpdatePaymentIntent(string basketId);
    }
}
