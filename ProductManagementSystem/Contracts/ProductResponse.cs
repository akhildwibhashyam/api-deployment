namespace ProductManagementSystem.Contracts;

public record ProductResponse(string ProductId, string Name, string Description, decimal Price, int StockQty, bool IsActive, DateTime createdAt);
