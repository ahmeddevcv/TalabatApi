using System.ComponentModel.DataAnnotations;
using Talabat.Core.Entities;

namespace Talabat.APIs.Dtos
{
    public class CustmerBasketDto//for validation
    {
        [Required]
        public string Id { get; set; }
        //PaymentIntent
        public string? PaymentIntentId { get; set; }
        public string? ClientSecret { get; set; }
        public int? DeliveryMethodId { get; set; }

        public decimal ShippingCost { get; set; }
        public List<BasketItemDto> Items { get; set; }
    }
}
