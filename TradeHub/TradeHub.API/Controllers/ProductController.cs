using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TradeHub.API.Extensions;
using TradeHub.BLL.DTOs.Products;
using TradeHub.BLL.Services;

namespace TradeHub.API.Controllers
{
    [Route("api/products")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly ProductService _productService;

        public ProductController(ProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        public async Task<IActionResult> GetProducts()
        {
            var products = await _productService.GetProductsAsync();
            return Ok(products);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductById(int id)
        {
            var product = await _productService.GetProductByIdOrThrowAsync(id);
            return Ok(product);
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetMyProducts()
        {
            var userId = HttpContext.GetUserId();
            var products = await _productService.GetProductsBySellerAsync(userId);
            return Ok(products);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateProduct(CreateProductRequest request)
        {
            var userId = HttpContext.GetUserId();
            var product = await _productService.CreateProductAsync(userId, request);
            return Ok(product);
        }

        [Authorize]
        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, UpdateProductRequest request)
        {
            var userId = HttpContext.GetUserId();
            var product = await _productService.UpdateProductAsync(userId, id, request);
            return Ok(product);
        }

        [Authorize]
        [HttpPut("{id}/price")]
        public async Task<IActionResult> UpdatePrice(int id, [FromBody] int newPrice)
        {
            var userId = HttpContext.GetUserId();
            await (_productService.UpdatePriceBySellerAsync(id, newPrice, userId));
            return Ok();
        }

        [Authorize]
        [HttpPut("{id}/stock/increase")]
        public async Task<IActionResult> IncreaseStock(int id, [FromBody] int quantity)
        {
            var userId = HttpContext.GetUserId();
            await (_productService.IncreaseStockBySellerAsync(id, quantity, userId));
            return Ok();
        }

        [Authorize]
        [HttpPut("{id}/stock/descrease")]
        public async Task<IActionResult> DecreaseStock(int id, [FromBody] int quantity)
        {
            var userId = HttpContext.GetUserId();
            await (_productService.DecreaseStockBySellerAsync(id, quantity, userId));
            return Ok();
        }

    }
}
