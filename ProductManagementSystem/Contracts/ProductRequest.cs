namespace ProductManagementSystem.Contracts;

public record ProductRequest(string Name, string Description, decimal Price, int StockQty, bool IsActive);
