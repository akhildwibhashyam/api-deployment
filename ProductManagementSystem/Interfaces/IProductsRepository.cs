using ProductManagementSystem.Domain;

namespace ProductManagementSystem.Interfaces;

public interface IProductsRepository
{
    /// <summary>
    /// Gets all asynchronous.
    /// </summary>
    /// <returns></returns>
    Task<IEnumerable<Product>> GetAllAsync();

    /// <summary>
    /// Gets the by identifier asynchronous.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <returns></returns>
    Task<Product> GetByIdAsync(string id);

    /// <summary>
    /// Creates the asynchronous.
    /// </summary>
    /// <param name="product">The product.</param>
    /// <returns></returns>
    Task<Product> CreateAsync(Product product);

    /// <summary>
    /// Updates the asynchronous.
    /// </summary>
    /// <param name="product">The product.</param>
    /// <returns></returns>
    Task<Product> UpdateAsync(Product product);

    /// <summary>
    /// Deletes the asynchronous.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <returns></returns>
    Task DeleteAsync(string id);
}