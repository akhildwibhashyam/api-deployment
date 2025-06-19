using ProductManagementSystem.Contracts;

namespace ProductManagementSystem.Interfaces;

public interface IProductsService
{
    /// <summary>
    /// Gets all products asynchronous.
    /// </summary>
    /// <returns></returns>
    Task<IEnumerable<ProductResponse>> GetAllProductsAsync();

    /// <summary>
    /// Gets the product by identifier asynchronous.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <returns></returns>
    Task<ProductResponse> GetProductByIdAsync(string id);

    /// <summary>
    /// Creates the product asynchronous.
    /// </summary>
    /// <param name="product">The product.</param>
    /// <returns></returns>
    Task<ProductResponse> CreateProductAsync(ProductRequest product);

    /// <summary>
    /// Updates the product asynchronous.
    /// </summary>
    /// <param name="productId">The product identifier.</param>
    /// <param name="product">The product.</param>
    /// <returns></returns>
    Task<ProductResponse> UpdateProductAsync(string productId, ProductRequest product);

    /// <summary>
    /// Deletes the product asynchronous.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <returns></returns>
    Task DeleteProductAsync(string id);
}
