using Talabat.Core.Entities.Order_Aggregate;

namespace Talabat.APIs.Dtos
{
    public class OrderItemDto
    {
        //public ProductItemOrdered Product { get; set; }//mapping
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string PictureUrl { get; set; }

        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }
}