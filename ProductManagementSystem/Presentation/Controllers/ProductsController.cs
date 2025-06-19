using Microsoft.AspNetCore.Mvc;
using ProductManagementSystem.Contracts;
using ProductManagementSystem.Interfaces;

namespace ProductManagementSystem.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController(IProductsService productsService, ILogger<ProductsController> logger) : Controller
{
    public IProductsService ProductsService { get; } = productsService;
    public ILogger<ProductsController> Logger { get; } = logger;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductResponse>>> GetAllProducts()
    {
        Logger.LogInformation("Retrieving all products");
        var products = await ProductsService.GetAllProductsAsync();
        return Ok(products);
    }

    [HttpGet("{productId}")]
    public async Task<ActionResult<ProductResponse>> GetProductById(string productId)
    {
        Logger.LogInformation($"Retrieving product with ID {productId}", productId);
        var product = await ProductsService.GetProductByIdAsync(productId);
        if (product == null) 
        {
            throw new KeyNotFoundException();
        }
        return Ok(product);
    }

    [HttpPost]
    public async Task<ActionResult<ProductResponse>> CreateProduct(ProductRequest productRequest)
    {
        Logger.LogInformation("Creating new product");
        var product = await ProductsService.CreateProductAsync(productRequest);
        return CreatedAtAction(nameof(GetProductById), new { productId = product.ProductId }, product);
    }

    [HttpPut("{productId}")]
    public async Task<ActionResult<ProductResponse>> UpdateProduct(string productId, ProductRequest productRequest)
    {
        Logger.LogInformation($"Updating product with ID {productId}", productId);
        var product = await ProductsService.UpdateProductAsync(productId, productRequest);
        return Ok(product);
    }

    [HttpDelete("{productId}")]
    public async Task<ActionResult> DeleteProduct(string productId)
    {
        Logger.LogInformation($"Deleting product with ID {productId}", productId);
        await ProductsService.DeleteProductAsync(productId);
        return NoContent();
    }
}
