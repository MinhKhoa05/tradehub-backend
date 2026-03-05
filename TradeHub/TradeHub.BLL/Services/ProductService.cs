using TradeHub.DAL.Repositories;
using TradeHub.DAL.Entities;
using TradeHub.BLL.DTOs.Products;
using TradeHub.BLL.Exceptions;

namespace TradeHub.BLL.Services
{
    public class ProductService
    {
        private readonly ProductRepository _productRepository;

        public ProductService(ProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<List<Product>> GetProductsAsync()
        {
            return await _productRepository.GetAllAsync();
        }

        public async Task<Product> GetProductByIdOrThrowAsync(int productId)
        {
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
            {
                throw new NotFoundException("sản phẩm", "id", productId);
            }

            return product;
        }

        public async Task<List<Product>> GetProductsBySellerAsync(int sellerId)
        {
            return await _productRepository.GetBySellerAsync(sellerId);
        }

        public async Task<Product> CreateProductAsync(int sellerId, CreateProductRequest request)
        {
            var product = new Product
            {
                Name = request.Name,
                Description = request.Description,
                Price = request.Price,
                Stock = 0,
                SellerId = sellerId,
            };

            product.Id = await _productRepository.CreateAsync(product);
            return product;
        }

        public async Task<Product> UpdateProductAsync(int userId, int productId, UpdateProductRequest request)
        {
            if (request.Name == null && request.Description == null)
            {
                throw new BusinessException("Không có dữ liệu để cập nhật");
            }
            
            var product = await GetProductByIdOrThrowAsync(productId);
            if (product.SellerId != userId)
            {
                throw new BusinessException("Không có quyền để cập nhật");
            }

            product.Name = request.Name ?? product.Name;
            product.Description = request.Description ?? product.Description;

            await _productRepository.UpdateAsync(productId, product);
            return product;
        }

        public async Task UpdatePriceBySellerAsync(int productId, double newPrice, int userId)
        {
            if (newPrice < 0)
            {
                throw new BusinessException("Giá của sản phẩm không được âm");
            }

            var affected = await _productRepository.UpdatePriceBySellerAsync(productId, newPrice, userId);
            if (affected == 0)
            {
                throw new NotFoundException("sản phẩm", "id", productId);
            }
        }

        public async Task IncreaseStockBySellerAsync(int productId, int quantity, int userId)
        {
            if (quantity <= 0)
            {
                throw new BusinessException("Số lượng phải lớn hơn 0");
            }

            var affected = await _productRepository.IncreaseStockBySellerAsync(productId, quantity, userId);
            if (affected == 0)
            {
                throw new BusinessException("Sản phẩm không tồn tại hoặc không có quyền cập nhật");
            }
        }

        public async Task DecreaseStockBySellerAsync(int productId, int quantity, int userId)
        {
            if (quantity <= 0)
            {
                throw new BusinessException("Số lượng phải lớn hơn 0");
            }

            var affected = await _productRepository.DecreaseStockBySellerAsync(productId, quantity, userId);
            if (affected == 0)
            {
                throw new BusinessException("Tồn kho không đủ hoặc không có quyền cập nhật");
            }
        }
    }
}
