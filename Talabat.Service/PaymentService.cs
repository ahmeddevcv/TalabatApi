using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Stripe;
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
using Product = Talabat.Core.Entities.Product;

namespace Talabat.Service
{
    [Authorize]
    public class PaymentService : IPaymentService
    {
        private readonly IConfiguration _configuration;
        private readonly IBasketRepository _basketRepository;
        private readonly IUnitOfWork _unitOfWork;

        public PaymentService(IConfiguration configuration, IBasketRepository basketRepository, IUnitOfWork unitOfWork)
        {
            _configuration = configuration;
            _basketRepository = basketRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<CustmerBasket?> CreateOrUpdatePaymentIntent(string basketId)
        {
            StripeConfiguration.ApiKey = _configuration["StripeSettings:SecretKey"];

            var basket = await _basketRepository.GetBasketAsync(basketId);
            if (basket is null) return null;

            var shippingPrice = 0m;
            if (basket.DeliveryMethodId.HasValue)
            {
                var deliveryMethod = await _unitOfWork.Repository<DeliveryMethod>().GetByIdAsync(basket.DeliveryMethodId.Value);
                basket.ShippingCost = deliveryMethod.Cost;
                shippingPrice = deliveryMethod.Cost;
            }

            if (basket.Items.Count > 0)
            {
                foreach (var item in basket.Items)
                {
                    var product = await _unitOfWork.Repository<Product>().GetByIdAsync(item.Id);
                    if (item.Price != product.Price)
                        item.Price = product.Price;
                }
            }

            var subTotal = basket.Items.Sum(x => x.Price * x.Quantity);
            var service = new PaymentIntentService();
            PaymentIntent paymentIntent;

            if (string.IsNullOrEmpty(basket.PaymentIntentId))
            {
                var options = new PaymentIntentCreateOptions()
                {
                    Amount = (long)subTotal * 100 + (long)shippingPrice * 100,
                    Currency = "usd",
                    PaymentMethodTypes = new List<string>() { "card" }
                };
                paymentIntent = await service.CreateAsync(options);
                basket.PaymentIntentId = paymentIntent.Id;
                basket.ClientSecret = paymentIntent.ClientSecret;
            }
            else
            {
                var options = new PaymentIntentUpdateOptions()
                {
                    Amount = (long)subTotal * 100 + (long)shippingPrice * 100,
                };
                paymentIntent = await service.UpdateAsync(basket.PaymentIntentId, options);
                basket.PaymentIntentId = paymentIntent.Id;
                basket.ClientSecret = paymentIntent.ClientSecret;
            }

            await _basketRepository.UpdateBasketAsync(basket);
            return basket;
        }

        public async Task<Order?> UpdateOrderPaymentSucceededOrFailed(string paymentIntentId, bool isPaid)
        {
            var spec = new OrderWithPaymentIntentIdSpecifications(paymentIntentId);
            var order = await _unitOfWork.Repository<Order>().GetEntityWithSpecAsync(spec);
            if (order is null) return null;

            var targetStatus = isPaid ? OrderStatus.PaymentRecieved : OrderStatus.PaymentField;
            if (order.Status != targetStatus)
            {
                order.Status = targetStatus;
                await _unitOfWork.Complete();
            }

            return order;
        }

        public async Task<Order?> VerifyAndSyncPaymentIntentWithStripeAsync(string paymentIntentId)
        {
            if (string.IsNullOrWhiteSpace(paymentIntentId)) return null;

            StripeConfiguration.ApiKey = _configuration["StripeSettings:SecretKey"];
            var service = new PaymentIntentService();
            var paymentIntent = await service.GetAsync(paymentIntentId);
            if (paymentIntent is null) return null;

            var spec = new OrderWithPaymentIntentIdSpecifications(paymentIntentId);
            var order = await _unitOfWork.Repository<Order>().GetEntityWithSpecAsync(spec);
            if (order is null) return null;

            OrderStatus newStatus = order.Status;
            if (string.Equals(paymentIntent.Status, "succeeded", StringComparison.OrdinalIgnoreCase))
            {
                newStatus = OrderStatus.PaymentRecieved;
            }
            else if (string.Equals(paymentIntent.Status, "canceled", StringComparison.OrdinalIgnoreCase) ||
                     paymentIntent.LastPaymentError != null)
            {
                newStatus = OrderStatus.PaymentField;
            }

            if (order.Status != newStatus)
            {
                order.Status = newStatus;
                await _unitOfWork.Complete();
            }

            return order;
        }
    }
}
