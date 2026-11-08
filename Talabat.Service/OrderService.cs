using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talabat.Core;
using Talabat.Core.Entities;
using Talabat.Core.Entities.Order_Aggregate;
using Talabat.Core.Repositories;
using Talabat.Core.Services;
using Talabat.Core.Specifiactions.Order_Spec;

namespace Talabat.Service
{
    public class OrderService : IOrderService
    {
        private readonly IBasketRepository _basketRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPaymentService _paymentService;

        //private readonly IGenericRepository<Product> _productRepo;
        //private readonly IGenericRepository<DeliveryMethod> _deliveryMethodRepo;
        //private readonly IGenericRepository<Order> _orderRepo;

        public OrderService(IBasketRepository basketRepository,IUnitOfWork unitOfWork,IPaymentService paymentService /*IGenericRepository<Product> productRepo, IGenericRepository<DeliveryMethod> deliveryMethodRepo, IGenericRepository<Order> orderRepo*/)
        {
            _basketRepository = basketRepository;
            _unitOfWork = unitOfWork;
            _paymentService = paymentService;
            //_productRepo = productRepo;
            //_deliveryMethodRepo = deliveryMethodRepo;
            //_orderRepo = orderRepo;
        }
        public async Task<Order?> CreateOrderAsync(string buyerEmail, string basketId, int deliveryMethodId, Address shippingAddress)
        {
            // 1. Get Basket From Baskets Repo
            var basket = await _basketRepository.GetBasketAsync(basketId);
            // 2. Get Selected Items at Basket From Products Repo
            var orderItems = new List<OrderItem>();
            if (basket?.Items?.Count>0)
            {
                foreach (var item in basket.Items)
                {
                    var product = await _unitOfWork.Repository<Product>().GetByIdAsync(item.Id);
                    var productItemOrdered = new ProductItemOrdered(product.Id, product.Name, product.PictureUrl);
                    var orderItem = new OrderItem(productItemOrdered, product.Price, item.Quantity);
                    orderItems.Add(orderItem);
                }

            }
            // 3. Calculate SubTotal
            var subTotal = orderItems.Sum(x=>x.Price*x.Quantity);
            // 4. Get Delivery Method From DeliveryMethods Repo
            var deliveryMethod =await _unitOfWork.Repository<DeliveryMethod>().GetByIdAsync(deliveryMethodId);
            // 5. Create Order
            //lec6
            //#session6 eng Aliaa
            var spec = new OrderWithPaymentIntentIdSpecifications(basket.PaymentIntentId);
            var existingOrder = await _unitOfWork.Repository<Order>().GetEntityWithSpecAsync(spec);
            if (existingOrder is not null)
            {
                _unitOfWork.Repository<Order>().Delete(existingOrder);
                //await _paymentService.CreateOrUpdatePaymentIntent(basket.Id); //eng ahmed nasr
                await _paymentService.CreateOrUpdatePaymentIntent(basketId);   // eng aliaa
            }
            //#session6 eng Aliaa

            var order =new Order(buyerEmail, shippingAddress, deliveryMethod,orderItems,subTotal, basket.PaymentIntentId);
            await _unitOfWork.Repository<Order>().Add(order);
            // 6. Save To Database [TODO]
            var result = await _unitOfWork.Complete();
            if (result <= 0) return null;
            return order;
        }

        public Task<Order> GetOrderByIdForUserAsync(int OrderId, string buyerEmail)
        {
            var spec = new OrderSpecifications(OrderId,buyerEmail);
            var order=_unitOfWork.Repository<Order>().GetEntityWithSpecAsync(spec);
            return order;
        }

        public Task<IReadOnlyList<Order>> GetOrdersForUserAsync(string buyerEmail)
        {
            var spec = new OrderSpecifications(buyerEmail);
            var orders = _unitOfWork.Repository<Order>().GetAllWithSpecAsync(spec);
            return orders;
        }

        public async Task<IReadOnlyList<DeliveryMethod>> GetDeliveryMethodsAsync()
        {
            var deliveryMethods = await _unitOfWork.Repository<DeliveryMethod>().GetAllAsync();
            return deliveryMethods;
        }
    }
}
