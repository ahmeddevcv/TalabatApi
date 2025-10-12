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
using Product = Talabat.Core.Entities.Product;

namespace Talabat.Service
{
    [Authorize]
    public class PaymentService : IPaymentService
    {
        private readonly IConfiguration _configuration;
        private readonly IBasketRepository _basketRepository;
        private readonly IUnitOfWork _unitOfWork;

        public PaymentService(IConfiguration configuration,IBasketRepository basketRepository,IUnitOfWork unitOfWork)
        {
            _configuration = configuration;
            _basketRepository = basketRepository;
            _unitOfWork = unitOfWork;
        }
        public async Task<CustmerBasket?> CreateOrUpdatePaymentIntent(string basketId)
        {
            
            
            ///apikey
            ///get basket to get cost of
            ///deliverymethod.cost, subtotal
            ///paymentintent,create,update
            

            //apikey
            StripeConfiguration.ApiKey = _configuration["StripeSettings:SecretKey"];
            //get basket
            var basket=await _basketRepository.GetBasketAsync(basketId);
            if (basket is null) return null;

            /// Total = DM.cost + subtotal
            var shippingPrice = 0m;
            if (basket.DeliveryMethodId.HasValue)
            {
                //integration
                var deliveryMethod = await _unitOfWork.Repository<DeliveryMethod>().GetByIdAsync(basket.DeliveryMethodId.Value);
                basket.ShippingCost = deliveryMethod.Cost;
                shippingPrice = deliveryMethod.Cost;

            }
            /// basket.Items
            if (basket.Items.Count>0)
            {
                foreach (var item in basket.Items)
                {
                    var product = await _unitOfWork.Repository<Product>().GetByIdAsync(item.Id);
                    if(item.Price != product.Price)
                        item.Price = product.Price;
                }
            }

            var subTotal = basket.Items.Sum(x => x.Price * x.Quantity);
            //shippingPrice, subTotal => create PaymentIntent
            //create Payment Intent
            var service = new PaymentIntentService();
            PaymentIntent paymentIntent;

            if (string.IsNullOrEmpty(basket.PaymentIntentId)) //create
            {
                var options = new PaymentIntentCreateOptions()
                {
                    Amount = (long)subTotal*100 + (long)shippingPrice * 100,
                    Currency = "usd",
                    PaymentMethodTypes = new List<string>() { "card"}
                };
                paymentIntent =await service.CreateAsync(options);
                basket.PaymentIntentId=paymentIntent.Id;
                basket.ClientSecret = paymentIntent.ClientSecret;
            }
            //update
            else 
            {
                var options = new PaymentIntentUpdateOptions()
                {
                    Amount = (long)subTotal*100 + (long)shippingPrice * 100,
                };
                paymentIntent = await service.UpdateAsync(basket.PaymentIntentId,options);
                basket.PaymentIntentId = paymentIntent.Id;
                basket.ClientSecret = paymentIntent.ClientSecret;
            }
            await _basketRepository.UpdateBasketAsync(basket);
            return basket;
        }//CreateOrUpdatePaymentIntent
    }//PaymentService
}
