using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Talabat.Core.Entities.Order_Aggregate
{
    public class Order:BaseEntity
    {
        //        BuyerEmail: String
        //OrderDate: DateTimeOffSet
        //Status: OrderStatus
        //ShippingAddress: ShippingAddress
        public Order()
        {
        }
        public Order(string buyerEmail, Address shippingAddress, DeliveryMethod deliveryMethod, ICollection<OrderItem> items, decimal subtotal, string paymentIntentId)
        {
            BuyerEmail = buyerEmail;
            ShippingAddress = shippingAddress;
            DeliveryMethod = deliveryMethod;
            Items = items;
            Subtotal = subtotal;
            PaymentIntentId = paymentIntentId;
        }

        public string BuyerEmail { get; set; }
        public DateTimeOffset OrderDate { get; set; } = DateTimeOffset.Now;
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public Address ShippingAddress { get; set; } //1 to 1 => not navigational property

//        DeliveryMethodId: int (Fk)
//DeliveryMethod: DeliveryMethod(navigational Property)
//Items: ICollection<OrderItems>
//Subtotal: Decimal
//PaymentIntentId: String =string.empty()

        //public int DeliveryMethodId { get; set; } //fk
        public DeliveryMethod DeliveryMethod { get; set; } // navigational property
        public ICollection<OrderItem> Items { get; set; } = new HashSet<OrderItem>(); // navigational property 1 to M
        public decimal Subtotal { get; set; }
        public decimal GetTotal() => Subtotal + DeliveryMethod.Cost; //method
        public string/*2 ?*/ PaymentIntentId { get; set; } /*= string.Empty;*/
        //migratin to set it notnull
        //Add-Migration UpdatePaymentIntentIdColumnToBeRequired -context StoreContext

    }
}
