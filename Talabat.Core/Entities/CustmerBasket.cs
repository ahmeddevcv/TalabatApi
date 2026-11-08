using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Talabat.Core.Entities
{
    public class CustmerBasket
    {//string Id - Items
        public string Id { get; set; }
        public List<BasketItem> Items { get; set; }

        //PaymentIntent
        public string PaymentIntentId { get; set; }
        public string ClientSecret { get; set; }
        public int? DeliveryMethodId { get; set; }
        public decimal ShippingCost { get; set; }



        public CustmerBasket(string id)
        {
            Id = id;
        }
    }
}
