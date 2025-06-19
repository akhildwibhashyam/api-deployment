using ProductManagementSystem.Contracts;
using ProductManagementSystem.Domain;
using ProductManagementSystem.Interfaces;

namespace ProductManagementSystem.Application;

public class ProductsService(IProductsRepository productsRepository) : IProductsService
{
    public IProductsRepository ProductsRepository { get; } = productsRepository;

    /// <inheritdoc/>
    public async Task<ProductResponse> CreateProductAsync(ProductRequest productRequest)
    {
        Product newProduct = new()
        {
            Name = productRequest.Name,
            Description = productRequest.Description,
            Price = productRequest.Price,
            StockQuantity = productRequest.StockQty,
            IsActive = productRequest.IsActive,
            CreatedAt = DateTime.Now,
            LastModifiedAt = DateTime.Now,
        };

        Product product = await ProductsRepository.CreateAsync(newProduct);
        return CreateProductResponse(product);
    }

    /// <inheritdoc/>
    public async Task DeleteProductAsync(string id)
    {
        await ProductsRepository.DeleteAsync(id);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<ProductResponse>> GetAllProductsAsync()
    {
        IEnumerable<Product> products = await ProductsRepository.GetAllAsync();
        return products.Select(CreateProductResponse);
    }

    /// <inheritdoc/>
    public async Task<ProductResponse> GetProductByIdAsync(string id)
    {
        Product product = await ProductsRepository.GetByIdAsync(id);
        return CreateProductResponse(product);
    }

    /// <inheritdoc/>
    public async Task<ProductResponse> UpdateProductAsync(string productId, ProductRequest productRequest)
    {
        Product product = await ProductsRepository.GetByIdAsync(productId);

        if (product == null)
        {
            throw new KeyNotFoundException();
        }

        product.Name = productRequest.Name;
        product.Description = productRequest.Description;
        product.Price = productRequest.Price;
        product.StockQuantity = productRequest.StockQty;
        product.IsActive = productRequest.IsActive;
        product.LastModifiedAt = DateTime.Now;

        await ProductsRepository.UpdateAsync(product);
        return CreateProductResponse(product);
    }

    private static ProductResponse CreateProductResponse(Product product)
    {
        return new ProductResponse(product.Id, product.Name, product.Description, product.Price, product.StockQuantity, product.IsActive, product.CreatedAt);
    }
}
