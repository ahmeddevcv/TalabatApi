using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talabat.Core.Entities.Order_Aggregate;

namespace Talabat.Core.Specifiactions.Order_Spec
{
    public class OrderSpecifications:BaseSpecification<Order>
    {
        public OrderSpecifications(string email)
            :base(o=>o.BuyerEmail==email)
        {
            Includes.Add(o => o.DeliveryMethod);
            Includes.Add(o => o.Items);

            AddOrderByDescending(o => o.OrderDate);
        }
        public OrderSpecifications(int id, string email)
    : base(o => o.BuyerEmail == email && o.Id==id)
        {
            Includes.Add(o => o.DeliveryMethod);
            Includes.Add(o => o.Items);

        }
    }
}
