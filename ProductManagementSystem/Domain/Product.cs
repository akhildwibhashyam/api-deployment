using Amazon.DynamoDBv2.DataModel;

namespace ProductManagementSystem.Domain;

/// <summary>
/// Product Model.
/// </summary>
[DynamoDBTable("Products")]
public class Product
{
    /// <summary>
    /// Gets or sets the product identifier.
    /// </summary>
    /// <value>
    /// The product identifier.
    /// </value>
    public string Id { get; set; } = null!;

    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    /// <value>
    /// The name.
    /// </value>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    /// <value>
    /// The description.
    /// </value>
    public string Description { get; set; } = null!;

    /// <summary>
    /// Gets or sets the price.
    /// </summary>
    /// <value>
    /// The price.
    /// </value>
    public decimal Price { get; set; }

    /// <summary>
    /// Gets or sets the stock quantity.
    /// </summary>
    /// <value>
    /// The stock quantity.
    /// </value>
    public int StockQuantity { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this instance is active.
    /// </summary>
    /// <value>
    ///   <c>true</c> if this instance is active; otherwise, <c>false</c>.
    /// </value>
    public bool IsActive { get; set; }

    /// <summary>
    /// Gets or sets the created at.
    /// </summary>
    /// <value>
    /// The created at.
    /// </value>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the last modified at.
    /// </summary>
    /// <value>
    /// The last modified at.
    /// </value>
    public DateTime LastModifiedAt { get; set; }
}
