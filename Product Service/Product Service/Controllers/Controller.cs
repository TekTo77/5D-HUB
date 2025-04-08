using Microsoft.AspNetCore.Mvc;
using Product_Service.Core;
using Product_Service.Servise;
using Servise.Tools;

namespace Product_Service.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {

        private readonly ProductServise  _productServise;

        public ProductsController(ProductServise productServise)
        {
            _productServise = productServise;
        }

        [HttpGet("products/{id}")]
        public async Task<IActionResult> GetInfoProduct([FromRoute] int id)
        {
            if (id==0)
            {
                return BadRequest("Запрос не может быть пустым");
            }
            ProductDB product = await _productServise.GetProductDBAsync(id);
            return Ok( product);
        }

        [HttpPost("productsCreate")]
        public async Task<IActionResult> CreateProduct([FromBody] Product request)
        {
            if (request == null || request.Name.IsNullOrEmpty())
            {
                return BadRequest("Запрос не может быть пустым");
            }


            bool result = await _productServise.CreateProductAsync(request);
            if (result)
            {
                return Ok();
            }
            return BadRequest();
        }

        [HttpPut("products/{id}/stock")]
        public async Task<IActionResult> changeQuantity([FromRoute] int id, int quantity)
        {
            if (id == 0 || quantity == 0)
            {
                return BadRequest("Запрос не может быть пустым");
            }

            bool reault = await _productServise.changeQuntityProductAsync(id, quantity);

            if (reault)
            {
                return Ok();
            }
            return BadRequest();
        }

    }
}
