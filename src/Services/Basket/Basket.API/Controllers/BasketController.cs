using Basket.API.Entities;
using Basket.API.GrpcServices;
using Basket.API.Repositories;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Basket.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class BasketController: ControllerBase
    {
        private readonly IBasketRepository _repo;
        private readonly DiscountGrpcService _discountService;
        public BasketController(IBasketRepository basketRepository, DiscountGrpcService discountService)
        {
            _repo = basketRepository ?? throw new ArgumentNullException(nameof(basketRepository));
            _discountService = discountService ?? throw new ArgumentNullException(nameof(discountService));
        }
        [HttpGet("{userName}", Name = "GetBasket")]
        [ProducesResponseType(typeof(ShoppingCart), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<ShoppingCart>> GetBasket(string userName)
        {
            var basket = await _repo.GetBasket(userName);
            return Ok(basket ?? new ShoppingCart(userName));
        }
        [HttpPost]
        [ProducesResponseType(typeof(ShoppingCart), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<ShoppingCart>> UpdateCart([FromBody] ShoppingCart cart)
        {
            foreach (var item in cart.Items)
            {
                var coupon = await _discountService.GetDiscount(item.ProductName);
                item.Price -= coupon.Amount;
            }
            return Ok(await _repo.UpdateBasket(cart));
        }
        [HttpDelete("userName", Name = "DeleteCart")]
        [ProducesResponseType(typeof(void), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> DeleteBasket(string userName)
        {
            await _repo.DeleteBasket(userName);
            return Ok();
        }
    }
}
