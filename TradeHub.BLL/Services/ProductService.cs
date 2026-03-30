using TradeHub.BLL.Common;
using TradeHub.BLL.DTOs.Products;
using TradeHub.BLL.Exceptions;
using TradeHub.BLL.Utils;
using TradeHub.DAL.DTOs;
using TradeHub.DAL.Entities;
using TradeHub.DAL.Repositories;

namespace TradeHub.BLL.Services
{
    public class ProductService : BaseService
    {
        private readonly ProductRepository _productRepo;

        public ProductService(ProductRepository productRepo, IIdentityService identityService)
            : base(identityService)
        {
            _productRepo = productRepo;
        }

        // ===== PUBLIC (ai cũng dùng được) =====

        public async Task<List<Product>> GetProductsAsync()
        {
            return await _productRepo.GetAllAsync();
        }

        public async Task<Product> GetProductByIdOrThrowAsync(int productId)
        {
            return await _productRepo.GetByIdAsync(productId)
                ?? throw new BusinessException("Sản phẩm không tồn tại");
        }

        // ===== MY (seller hiện tại) =====

        public async Task<List<Product>> GetMyProductsAsync()
        {
            return await _productRepo.GetBySellerAsync(CurrentUserId);
        }

        public async Task<Product> CreateProductAsync(CreateProductRequest request)
        {
            var product = new Product
            {
                Name = request.Name,
                NormalizedName = NormalizeName.Normalize(request.Name),
                Description = request.Description,
                Price = request.Price,
                Stock = 0,
                SellerId = CurrentUserId,
            };

            product.Id = await _productRepo.CreateAsync(product);
            return product;
        }

        public async Task<Product> UpdateProductAsync(int productId, UpdateProductRequest request)
        {
            if (string.IsNullOrEmpty(request.Name) && request.Description == null)
                throw new BusinessException("Không có dữ liệu để cập nhật");

            var product = await GetProductForUpdateAsync(productId);

            if (request.Name != null)
            {
                product.Name = request.Name;
                product.NormalizedName = NormalizeName.Normalize(request.Name);
            }

            if (request.Description != null)
            {
                product.Description = request.Description;
            }

            await _productRepo.UpdateAsync(productId, product);
            return product;
        }

        public async Task UpdateProductPriceAsync(int productId, int newPrice)
        {
            if (newPrice < 0)
                throw new BusinessException("Giá của sản phẩm không được âm");

            await GetProductForUpdateAsync(productId);

            var affected = await _productRepo.UpdatePriceAsync(productId, newPrice);

            if (affected == 0)
                throw new BusinessException("Cập nhật thất bại, vui lòng kiểm tra lại.");
        }

        public async Task IncreaseStockAsync(int productId, int quantity)
        {
            if (quantity <= 0)
                throw new BusinessException("Số lượng phải lớn hơn 0");

            await GetProductForUpdateAsync(productId);

            var affected = await _productRepo.IncreaseStockAsync(productId, quantity);

            if (affected == 0)
                throw new BusinessException("Cập nhật thất bại, vui lòng kiểm tra lại.");
        }

        public async Task DecreaseStockAsync(int productId, int quantity)
        {
            if (quantity <= 0)
                throw new BusinessException("Số lượng phải lớn hơn 0");

            var product = await GetProductForUpdateAsync(productId);

            if (product.Stock < quantity)
                throw new BusinessException($"Tồn kho không đủ. Hiện có: {product.Stock}, yêu cầu: {quantity}");

            var affected = await _productRepo.DecreaseStockAsync(productId, quantity);

            if (affected == 0)
                throw new BusinessException("Cập nhật thất bại, vui lòng kiểm tra lại.");
        }

        // ===== SYSTEM (không phụ thuộc user) =====

        public async Task DecreaseStockRangeAsync(List<ProductStockUpdate> updates)
        {
            if (updates.Count == 0)
                throw new BusinessException("Không có sản phẩm để cập nhật");

            var affected = await _productRepo.DecreaseStockRangeAsync(updates);

            if (affected != updates.Count)
                throw new BusinessException("Một vài sản phẩm không đủ tồn kho");
        }

        // ===== PRIVATE =====

        private async Task<Product> GetProductForUpdateAsync(int productId)
        {
            var product = await GetProductByIdOrThrowAsync(productId);

            if (product.SellerId != CurrentUserId)
                throw new BusinessException("Không có quyền để cập nhật");

            return product;
        }
    }
}