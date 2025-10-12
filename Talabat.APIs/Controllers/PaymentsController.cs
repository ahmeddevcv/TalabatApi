using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Talabat.APIs.Dtos;
using Talabat.APIs.Errors;
using Talabat.Core.Entities;
using Talabat.Core.Services;

namespace Talabat.APIs.Controllers
{
    public class PaymentsController : ApiBaseController
    {
        private readonly IPaymentService _paymentService;
        private readonly IMapper _mapper;

        public PaymentsController(IPaymentService paymentService,IMapper mapper)
        {
            _paymentService = paymentService;
            _mapper = mapper;
        }
        [ProducesResponseType(typeof(CustmerBasketDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        //[HttpPost] //post : /api/Payments?id (query string
        [HttpPost("{basketId}")] //post : /api/Payments/basketId
        public async Task<ActionResult<CustmerBasketDto>> CreateOrUpdatePaymentIntent(string basketId)
        {
            var basket = await _paymentService.CreateOrUpdatePaymentIntent(basketId);
            if (basket is null) return BadRequest(new ApiResponse(400, "A Problem With Your Basket"));
            var mappedBasket=_mapper.Map<CustmerBasket,CustmerBasketDto>(basket);
            return Ok(mappedBasket);
        }
        //[HttpPost] //post : /api/Payments
        //public async Task<ActionResult<CustmerBasket>> UpdateBasket(CustmerBasketDto custmerBasket)
        //{
        //    var basket = await _paymentService.CreateOrUpdatePaymentIntent(custmerBasket);
        //    if (basket is null) return BadRequest(new ApiResponse(400, "A Problem With Your Basket"));
        //    return Ok(basket);
        //}

        //[HttpPost({"webhook"})] // Post : /api/Payments/webhook
        


    }
}
