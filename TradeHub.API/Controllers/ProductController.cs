using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TradeHub.BLL.DTOs.Products;
using TradeHub.BLL.Services;

namespace TradeHub.API.Controllers
{
    [Authorize]
    [Route("api/products")]
    [ApiController]
    public class ProductController : BaseController
    {
        private readonly ProductService _product;

        public ProductController(ProductService product)
        {
            _product = product;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetProducts()
        {
            var products = await _product.GetProductsAsync();
            return ApiOk(products);
        }

        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductById(int id)
        {
            var product = await _product.GetProductByIdOrThrowAsync(id);
            return ApiOk(product);
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetMyProducts()
        {
            var products = await _product.GetProductsBySellerAsync(CurrentUserId);
            return ApiOk(products);
        }

        [HttpPost]
        public async Task<IActionResult> CreateProduct(CreateProductRequest request)
        {
            var product = await _product.CreateProductAsync(CurrentUserId, request);
            return ApiCreated(product);
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, UpdateProductRequest request)
        {
            var product = await _product.UpdateProductAsync(CurrentUserId, id, request);
            return ApiOk(product);
        }

        [HttpPut("{id}/price")]
        public async Task<IActionResult> UpdatePrice(int id, [FromBody] int newPrice)
        {
            await _product.UpdatePriceBySellerAsync(id, newPrice, CurrentUserId);
            return ApiNoContent();
        }

        [HttpPut("{id}/stock/increase")]
        public async Task<IActionResult> IncreaseStock(int id, [FromBody] int quantity)
        {
            await _product.IncreaseStockBySellerAsync(id, quantity, CurrentUserId);
            return ApiNoContent();
        }

        [HttpPut("{id}/stock/decrease")]
        public async Task<IActionResult> DecreaseStock(int id, [FromBody] int quantity)
        {
            await _product.DecreaseStockBySellerAsync(id, quantity, CurrentUserId);
            return ApiNoContent();
        }
    }
}
