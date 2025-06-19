using Moq;
using ProductManagementSystem.Domain;
using ProductManagementSystem.Interfaces;
using ProductManagementSystem.Application;
using ProductManagementSystem.Contracts;

namespace ProductManagementSystem.Tests;

public class ProductsServiceTests
{
    private readonly Mock<IProductsRepository> _mockRepository;
    private readonly IProductsService _productsService;

    public ProductsServiceTests()
    {
        _mockRepository = new Mock<IProductsRepository>();
        _productsService = new ProductsService(_mockRepository.Object);
    }

    [Fact]
    public async Task GetAllProductsAsync_ShouldReturnAllProducts()
    {
        // Arrange
        var products = new List<Product>
        {
            new Product
            {
                Id = "1",
                Name = "Test Product 1",
                Description = "Test Description 1",
                Price = 10.99m,
                StockQuantity = 100,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                LastModifiedAt = DateTime.UtcNow
            },
            new Product
            {
                Id = "2",
                Name = "Test Product 2",
                Description = "Test Description 2",
                Price = 20.99m,
                StockQuantity = 50,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                LastModifiedAt = DateTime.UtcNow
            }
        };

        _mockRepository.Setup(x => x.GetAllAsync()).ReturnsAsync(products);

        // Act
        var result = await _productsService.GetAllProductsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetProductByIdAsync_WithValidId_ShouldReturnProduct()
    {
        // Arrange
        var productId = "1";
        var product = new Product
        {
            Id = productId,
            Name = "Test Product",
            Description = "Test Description",
            Price = 10.99m,
            StockQuantity = 100,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            LastModifiedAt = DateTime.UtcNow
        };

        _mockRepository.Setup(x => x.GetByIdAsync(productId)).ReturnsAsync(product);

        // Act
        var result = await _productsService.GetProductByIdAsync(productId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(productId, result.ProductId);
    }

    [Fact]
    public async Task CreateProductAsync_ShouldCreateAndReturnProduct()
    {
        // Arrange
        var productRequest = new ProductRequest("Test Product", "Test Description", 10.99m, 100, true);
        var createdProduct = new Product
        {
            Id = "1",
            Name = productRequest.Name,
            Description = productRequest.Description,
            Price = productRequest.Price,
            StockQuantity = productRequest.StockQty,
            IsActive = productRequest.IsActive,
            CreatedAt = DateTime.UtcNow,
            LastModifiedAt = DateTime.UtcNow
        };

        _mockRepository.Setup(x => x.CreateAsync(It.IsAny<Product>())).ReturnsAsync(createdProduct);

        // Act
        var result = await _productsService.CreateProductAsync(productRequest);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(productRequest.Name, result.Name);
        Assert.Equal(productRequest.Description, result.Description);
        Assert.Equal(productRequest.Price, result.Price);
    }

    [Fact]
    public async Task UpdateProductAsync_WithValidId_ShouldUpdateAndReturnProduct()
    {
        // Arrange
        var productId = "1";
        var existingProduct = new Product
        {
            Id = productId,
            Name = "Original Name",
            Description = "Original Description",
            Price = 10.99m,
            StockQuantity = 100,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            LastModifiedAt = DateTime.UtcNow
        };

        var updateRequest = new ProductRequest("Updated Name", "Updated Description", 20.99m, 50, true);

        _mockRepository.Setup(x => x.GetByIdAsync(productId)).ReturnsAsync(existingProduct);
        _mockRepository.Setup(x => x.UpdateAsync(It.IsAny<Product>()))
            .ReturnsAsync((Product p) => p);

        // Act
        var result = await _productsService.UpdateProductAsync(productId, updateRequest);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(updateRequest.Name, result.Name);
        Assert.Equal(updateRequest.Description, result.Description);
        Assert.Equal(updateRequest.Price, result.Price);
    }

    [Fact]
    public async Task DeleteProductAsync_WithValidId_ShouldCallRepository()
    {
        // Arrange
        var productId = "1";
        _mockRepository.Setup(x => x.DeleteAsync(productId)).Returns(Task.CompletedTask);

        // Act
        await _productsService.DeleteProductAsync(productId);

        // Assert
        _mockRepository.Verify(x => x.DeleteAsync(productId), Times.Once);
    }
}
