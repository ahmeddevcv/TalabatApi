using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Stripe;
using System.IO;
using System.Threading.Tasks;
using Talabat.APIs.Dtos;
using Talabat.APIs.Errors;
using Talabat.Core.Entities;
using Talabat.Core.Entities.Order_Aggregate;
using Talabat.Core.Services;

namespace Talabat.APIs.Controllers
{
    public class PaymentsController : ApiBaseController
    {
        private readonly IPaymentService _paymentService;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly ILogger<PaymentsController> _logger;

        public PaymentsController(
            IPaymentService paymentService,
            IMapper mapper,
            IConfiguration configuration,
            ILogger<PaymentsController> logger)
        {
            _paymentService = paymentService;
            _mapper = mapper;
            _configuration = configuration;
            _logger = logger;
        }

        [Authorize]
        [ProducesResponseType(typeof(CustmerBasketDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [HttpPost("{basketId}")] // POST : /api/Payments/{basketId}
        public async Task<ActionResult<CustmerBasketDto>> CreateOrUpdatePaymentIntent(string basketId)
        {
            var basket = await _paymentService.CreateOrUpdatePaymentIntent(basketId);
            if (basket is null) return BadRequest(new ApiResponse(400, "A Problem With Your Basket"));
            var mappedBasket = _mapper.Map<CustmerBasket, CustmerBasketDto>(basket);
            return Ok(mappedBasket);
        }

        /// <summary>
        /// POST /api/Payments/confirm/{paymentIntentId}
        /// Verifies the PaymentIntent directly with Stripe's API after client confirmation
        /// and updates the Order status to PaymentRecieved or PaymentField.
        /// </summary>
        [Authorize]
        [HttpPost("confirm/{paymentIntentId}")]
        public async Task<ActionResult<OrderToReturnDto>> ConfirmPaymentIntentStatus(string paymentIntentId)
        {
            var order = await _paymentService.VerifyAndSyncPaymentIntentWithStripeAsync(paymentIntentId);
            if (order is null) return NotFound(new ApiResponse(404, "Order with this PaymentIntent was not found"));
            return Ok(_mapper.Map<Order, OrderToReturnDto>(order));
        }

        /// <summary>
        /// POST /api/Payments/webhook
        /// Standard Stripe Webhook endpoint for asynchronous payment_intent.succeeded / payment_intent.payment_failed events.
        /// </summary>
        [HttpPost("webhook")]
        public async Task<IActionResult> StripeWebhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            try
            {
                Event stripeEvent;
                var webhookSecret = _configuration["StripeSettings:WebhookSecret"];
                if (!string.IsNullOrWhiteSpace(webhookSecret) && Request.Headers.ContainsKey("Stripe-Signature"))
                {
                    stripeEvent = EventUtility.ConstructEvent(json, Request.Headers["Stripe-Signature"], webhookSecret);
                }
                else
                {
                    stripeEvent = EventUtility.ParseEvent(json, throwOnApiVersionMismatch: false);
                }

                var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                if (paymentIntent is not null)
                {
                    if (stripeEvent.Type == Events.PaymentIntentSucceeded)
                    {
                        await _paymentService.UpdateOrderPaymentSucceededOrFailed(paymentIntent.Id, true);
                        _logger.LogInformation("Payment succeeded for PaymentIntent {PaymentIntentId}", paymentIntent.Id);
                    }
                    else if (stripeEvent.Type == Events.PaymentIntentPaymentFailed)
                    {
                        await _paymentService.UpdateOrderPaymentSucceededOrFailed(paymentIntent.Id, false);
                        _logger.LogWarning("Payment failed for PaymentIntent {PaymentIntentId}", paymentIntent.Id);
                    }
                }

                return new EmptyResult();
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe webhook error");
                return BadRequest();
            }
        }
    }
}
