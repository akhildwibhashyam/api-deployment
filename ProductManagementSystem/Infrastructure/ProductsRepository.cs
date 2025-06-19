using Amazon.DynamoDBv2.DataModel;
using ProductManagementSystem.Domain;
using ProductManagementSystem.Interfaces;
using Microsoft.Extensions.Logging;

namespace ProductManagementSystem.Infrastructure;

/// <summary>
/// Repository for products.
/// </summary>
/// <seealso cref="ProductManagementSystem.Interfaces.IProductsRepository" />
public class ProductsRepository : IProductsRepository
{
    private readonly IDynamoDBContext _dynamoDBContext;
    private readonly ILogger<ProductsRepository> _logger;

    public ProductsRepository(IDynamoDBContext dynamoDBContext, ILogger<ProductsRepository> logger)
    {
        _dynamoDBContext = dynamoDBContext;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Product> CreateAsync(Product product)
    {
        _logger.LogInformation("Creating product with name: {ProductName}", product.Name);

        if (string.IsNullOrEmpty(product.Id))
        {
            product.Id = Guid.NewGuid().ToString();
            _logger.LogDebug("Generated new product ID: {ProductId}", product.Id);
        }

        try
        {
            await _dynamoDBContext.SaveAsync(product);
            _logger.LogInformation("Successfully created product with ID: {ProductId}", product.Id);
            return product;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create product {ProductName}. Error: {ErrorMessage}", 
                product.Name, ex.Message);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task DeleteAsync(string id)
    {
        _logger.LogInformation("Deleting product with ID: {ProductId}", id);

        try
        {
            await _dynamoDBContext.DeleteAsync<Product>(id);
            _logger.LogInformation("Successfully deleted product with ID: {ProductId}", id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete product {ProductId}. Error: {ErrorMessage}", 
                id, ex.Message);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        _logger.LogInformation("Retrieving all products");

        try
        {
            var products = await _dynamoDBContext.ScanAsync<Product>(new List<ScanCondition>()).GetRemainingAsync();
            _logger.LogInformation("Retrieved {Count} products", products.Count());
            return products;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve all products. Error: {ErrorMessage}", ex.Message);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<Product> GetByIdAsync(string id)
    {
        _logger.LogInformation("Retrieving product with ID: {ProductId}", id);

        try
        {
            var product = await _dynamoDBContext.LoadAsync<Product>(id);
            if (product == null)
            {
                _logger.LogWarning("Product not found with ID: {ProductId}", id);
                throw new KeyNotFoundException($"Product with ID {id} was not found.");
            }

            _logger.LogDebug("Successfully retrieved product: {ProductId} - {ProductName}", 
                product.Id, product.Name);
            return product;
        }
        catch (Exception ex) when (ex is not KeyNotFoundException)
        {
            _logger.LogError(ex, "Failed to retrieve product {ProductId}. Error: {ErrorMessage}", 
                id, ex.Message);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<Product> UpdateAsync(Product product)
    {
        _logger.LogInformation("Updating product with ID: {ProductId}", product.Id);

        try
        {
            await _dynamoDBContext.SaveAsync(product);
            _logger.LogInformation("Successfully updated product: {ProductId} - {ProductName}", 
                product.Id, product.Name);
            return product;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update product {ProductId}. Error: {ErrorMessage}", 
                product.Id, ex.Message);
            throw;
        }
    }
}
