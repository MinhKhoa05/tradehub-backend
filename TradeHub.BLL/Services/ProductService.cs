using TradeHub.DAL.Repositories;
using TradeHub.DAL.Entities;
using TradeHub.BLL.DTOs.Products;
using TradeHub.BLL.Exceptions;
using TradeHub.BLL.Utils;

namespace TradeHub.BLL.Services
{
    public class ProductService
    {
        private readonly ProductRepository _productRepo;

        public ProductService(ProductRepository productRepo)
        {
            _productRepo = productRepo;
        }

        public async Task<List<Product>> GetProductsAsync()
        {
            return await _productRepo.GetAllAsync();
        }

        public async Task<Product> GetProductByIdOrThrowAsync(int productId)
        {
            var product = await _productRepo.GetByIdAsync(productId)
                                ?? throw new BusinessException("Sản phẩm không tồn tại");    

            return product;
        }

        public async Task<List<Product>> GetProductsByIdsAsync(List<int> ids)
        {
            return await _productRepo.GetByIdsAsync(ids);
        }

        public async Task<List<Product>> GetProductsBySellerAsync(int sellerId)
        {
            return await _productRepo.GetBySellerAsync(sellerId);
        }

        public async Task<Product> CreateProductAsync(int sellerId, CreateProductRequest request)
        {
            var product = new Product
            {
                Name = request.Name,
                NormalizedName = NormalizeName.Normalize(request.Name),
                Description = request.Description,
                Price = request.Price,
                Stock = 0,
                SellerId = sellerId,
            };

            product.Id = await _productRepo.CreateAsync(product);
            return product;
        }

        public async Task<Product> UpdateProductAsync(int userId, int productId, UpdateProductRequest request)
        {
            if (string.IsNullOrEmpty(request.Name) && request.Description == null)
                throw new BusinessException("Không có dữ liệu để cập nhật");
            
            var product = await GetProductByIdOrThrowAsync(productId);
            
            if (product.SellerId != userId)
                throw new BusinessException("Không có quyền để cập nhật");

            if (request.Name != null)
            {
                product.Name = request.Name;
                product.NormalizedName = NormalizeName.Normalize(request.Name);
            }

            product.Description = request.Description ?? product.Description;

            await _productRepo.UpdateAsync(productId, product);
            return product;
        }

        public async Task UpdatePriceBySellerAsync(int productId, int newPrice, int userId)
        {
            if (newPrice < 0)
                throw new BusinessException("Giá của sản phẩm không được âm");

            var product = await GetProductByIdOrThrowAsync(productId);
            if (product.SellerId != userId)
                throw new BusinessException("Không có quyền để cập nhật");

            var affected = await _productRepo.UpdatePriceAsync(productId, newPrice);

            if (affected == 0)
                throw new BusinessException("Sản phẩm không tồn tại");
        }

        public async Task IncreaseStockBySellerAsync(int productId, int quantity, int userId)
        {
            if (quantity <= 0)
                throw new BusinessException("Số lượng phải lớn hơn 0");

            var product = await GetProductByIdOrThrowAsync(productId);
            if (product.SellerId != userId)
                throw new BusinessException("Không có quyền để cập nhật");

            var affected = await _productRepo.IncreaseStockAsync(productId, quantity);
            if (affected == 0)
                throw new BusinessException("Sản phẩm không tồn tại");
        }

        public async Task DecreaseStockBySellerAsync(int productId, int quantity, int userId)
        {
            if (quantity <= 0)
                throw new BusinessException("Số lượng phải lớn hơn 0");

            var product = await GetProductByIdOrThrowAsync(productId);
            if (product.SellerId != userId)
                throw new BusinessException("Không có quyền để cập nhật");

            var affected = await _productRepo.DecreaseStockAsync(productId, quantity);
            if (affected == 0)
                throw new BusinessException("Tồn kho không đủ");
        }

        public async Task DecreaseStockForOrderAsync(int productId, int quantity)
        {
            if (quantity <= 0)
                throw new BusinessException("Số lượng phải lớn hơn 0");

            var affected = await _productRepo.DecreaseStockAsync(productId, quantity);
            if (affected == 0)
                throw new BusinessException("Tồn kho không đủ");
        }

        //public async Task<List<Product>> SearchProductByNameAsync(string productName, int page)
        //{
        //    string normalizedName = NormalizeName.Normalize(productName);

        //    if (string.IsNullOrEmpty(normalizedName))
        //    {
        //        return new List<Product>();
        //    }

        //    return await _productRepo.SearchByNameAsync(normalizedName, page, 20);
        //}
    }
}
