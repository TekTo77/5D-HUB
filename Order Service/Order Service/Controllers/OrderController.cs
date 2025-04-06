using Microsoft.AspNetCore.Mvc;
using Order_Service.Core;
using Order_Service.Servise;

namespace Order_Service.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {

        private readonly OrderServise _orderServise;

        public OrdersController(OrderServise orderServise)
        {
            _orderServise = orderServise;
        }


        [HttpPost("orders")]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrder request)
        {
            if (request==null || request.UserID==0|| request.createProductinorders==null)
            {
                return BadRequest("Запрос не может быть пустым");
            }

            var result = await _orderServise.CreateOrder(request);

            if (result.Flag)
            {
                return Ok(result.Id);
            }
            return BadRequest(result.Flag);


        }

        [HttpGet("orders/{id}")]
        public async Task<IActionResult> GetInfoOrders([FromRoute] int id)
        {
            if (id==0)
            {
                return BadRequest("Запрос не может быть пустым");
            }


            OrderResponse result = await _orderServise.GetOrderInfo(id);

            if (result.Products == null)
            {
                return NotFound();
            }
            return Ok(result); 

        }
        
    }
}
