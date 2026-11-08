using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Talabat.APIs.Dtos;
using Talabat.APIs.Errors;
using Talabat.Core.Entities;
using Talabat.Core.Repositories;

namespace Talabat.APIs.Controllers
{
    public class BasketController : ApiBaseController
    {
        //GetCustomerBasket UpdateBasket DeleteBasket 
        private readonly IBasketRepository _basketRepository;
        private readonly IMapper _mapper;

        public BasketController(IBasketRepository basketRepository,IMapper mapper) 
        {
            _basketRepository = basketRepository;
            _mapper = mapper;
        }
        //[HttpGet("{id}")]//Get : api/Baskets/id
        [HttpGet]
        public async Task<ActionResult<CustmerBasket>> GetCustomerBasket(string id)
        {
            var basket = await _basketRepository.GetBasketAsync(id);
            return basket is null? new CustmerBasket(id) : basket; // مش فاهم ليه عملت اوبجكت من الكستمر هنا
        }
        [HttpPost]// Post : api/Baskets
        public async Task<ActionResult<CustmerBasket>> UpdateBasket(CustmerBasketDto basket)
        {
            var mappedBasket = _mapper.Map<CustmerBasketDto,CustmerBasket>(basket);
            var createdOrUpdated=await _basketRepository.UpdateBasketAsync(mappedBasket);
            if (createdOrUpdated is null) return BadRequest(new ApiResponse(400));
            return Ok(createdOrUpdated);
        }
        [HttpDelete]// Delete : api/Baskets
        public async Task<ActionResult<bool>> DeleteBasket(string id)
        {
            return await _basketRepository.DeleteBasketAsync(id);
        }
    }
}
