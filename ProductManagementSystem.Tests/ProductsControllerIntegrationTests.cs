using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Text.Json;
using ProductManagementSystem.Contracts;
using System.Text;

namespace ProductManagementSystem.Tests;

public class ProductsControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public ProductsControllerIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetAllProducts_ReturnsSuccessStatusCode()
    {
        // Act
        var response = await _client.GetAsync("/api/products");

        // Assert
        response.EnsureSuccessStatusCode();
        var responseString = await response.Content.ReadAsStringAsync();
        var products = JsonSerializer.Deserialize<List<ProductResponse>>(responseString, 
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        Assert.NotNull(products);
    }

    [Fact]
    public async Task CreateProduct_WithValidData_ReturnsCreatedProduct()
    {
        // Arrange
        var product = new ProductRequest("Test Product", "Test Description", 10.99m, 100, true);
        var content = new StringContent(
            JsonSerializer.Serialize(product),
            Encoding.UTF8,
            "application/json");

        // Act
        var response = await _client.PostAsync("/api/products", content);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var responseString = await response.Content.ReadAsStringAsync();
        var createdProduct = JsonSerializer.Deserialize<ProductResponse>(responseString,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        Assert.NotNull(createdProduct);
        Assert.Equal(product.Name, createdProduct.Name);
    }

    [Fact]
    public async Task GetProductById_WithValidId_ReturnsProduct()
    {
        // Arrange - First create a product
        var product = new ProductRequest("Test Product", "Test Description", 10.99m, 100, true);
        var content = new StringContent(
            JsonSerializer.Serialize(product),
            Encoding.UTF8,
            "application/json");
        var createResponse = await _client.PostAsync("/api/products", content);
        var createResponseString = await createResponse.Content.ReadAsStringAsync();
        var createdProduct = JsonSerializer.Deserialize<ProductResponse>(createResponseString,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        // Act
        var response = await _client.GetAsync($"/api/products/{createdProduct!.ProductId}");

        // Assert
        response.EnsureSuccessStatusCode();
        var responseString = await response.Content.ReadAsStringAsync();
        var returnedProduct = JsonSerializer.Deserialize<ProductResponse>(responseString,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        Assert.NotNull(returnedProduct);
        Assert.Equal(createdProduct.ProductId, returnedProduct.ProductId);
    }

    [Fact]
    public async Task UpdateProduct_WithValidData_ReturnsUpdatedProduct()
    {
        // Arrange - First create a product
        var product = new ProductRequest("Test Product", "Test Description", 10.99m, 100, true);
        var content = new StringContent(
            JsonSerializer.Serialize(product),
            Encoding.UTF8,
            "application/json");
        var createResponse = await _client.PostAsync("/api/products", content);
        var createResponseString = await createResponse.Content.ReadAsStringAsync();
        var createdProduct = JsonSerializer.Deserialize<ProductResponse>(createResponseString,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        // Update the product
        var updateProduct = new ProductRequest("Updated Product", "Updated Description", 20.99m, 50, true);
        var updateContent = new StringContent(
            JsonSerializer.Serialize(updateProduct),
            Encoding.UTF8,
            "application/json");

        // Act
        var response = await _client.PutAsync($"/api/products/{createdProduct!.ProductId}", updateContent);

        // Assert
        response.EnsureSuccessStatusCode();
        var responseString = await response.Content.ReadAsStringAsync();
        var updatedProduct = JsonSerializer.Deserialize<ProductResponse>(responseString,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        Assert.NotNull(updatedProduct);
        Assert.Equal(updateProduct.Name, updatedProduct.Name);
    }

    [Fact]
    public async Task DeleteProduct_WithValidId_ReturnsNoContent()
    {
        // Arrange - First create a product
        var product = new ProductRequest("Test Product", "Test Description", 10.99m, 100, true);
        var content = new StringContent(
            JsonSerializer.Serialize(product),
            Encoding.UTF8,
            "application/json");
        var createResponse = await _client.PostAsync("/api/products", content);
        var createResponseString = await createResponse.Content.ReadAsStringAsync();
        var createdProduct = JsonSerializer.Deserialize<ProductResponse>(createResponseString,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        // Act
        var response = await _client.DeleteAsync($"/api/products/{createdProduct!.ProductId}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
}
