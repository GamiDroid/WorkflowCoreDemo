namespace WorkflowCore.Monitor.Services;

/// <summary>
/// Helper voor cancellation checks tijdens async operaties
/// </summary>
public static class CancellableTaskHelper
{
    /// <summary>
    /// Voert een async operatie uit met periodieke cancellation checks
    /// </summary>
    public static async Task<T> ExecuteWithCancellationCheck<T>(
        Func<Task<T>> operation,
        Func<bool> shouldCancel,
        int pollIntervalMs = 200)
    {
        var operationTask = operation();

        while (!operationTask.IsCompleted)
        {
            if (shouldCancel())
            {
                throw new OperationCanceledException("Operation cancelled by workflow");
            }

            await Task.WhenAny(operationTask, Task.Delay(pollIntervalMs));
        }

        return await operationTask;
    }
}

/// <summary>
/// CRM service voor klantvalidatie
/// </summary>
public interface ICrmService
{
    Task<CustomerValidationResult> ValidateCustomerAsync(string customerId, CancellationToken cancellationToken = default);
    Task<decimal> GetCustomerCreditLimitAsync(string customerId, CancellationToken cancellationToken = default);
}

/// <summary>
/// ERP service voor order management
/// </summary>
public interface IErpService
{
    Task<string> CreateOrderAsync(OrderRequest order, CancellationToken cancellationToken = default);
    Task<bool> ConfirmOrderAsync(string orderId, CancellationToken cancellationToken = default);
    Task CancelOrderAsync(string orderId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Inventory service voor voorraad management
/// </summary>
public interface IInventoryService
{
    Task<InventoryCheckResult> CheckAvailabilityAsync(IEnumerable<OrderLine> items, CancellationToken cancellationToken = default);
    Task<string> ReserveInventoryAsync(string orderId, IEnumerable<OrderLine> items, CancellationToken cancellationToken = default);
    Task ReleaseReservationAsync(string reservationId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Shipping service voor verzending
/// </summary>
public interface IShippingService
{
    Task<ShipmentResult> ScheduleShipmentAsync(ShipmentRequest request, CancellationToken cancellationToken = default);
    Task CancelShipmentAsync(string shipmentId, CancellationToken cancellationToken = default);
}

// DTOs
public record CustomerValidationResult(bool IsValid, string? Reason, bool IsBlacklisted);

public record OrderRequest(string CustomerId, string CustomerName, IEnumerable<OrderLine> Lines, decimal TotalAmount);

public record OrderLine(string ProductId, string ProductName, int Quantity, decimal UnitPrice);

public record InventoryCheckResult(bool AllAvailable, IEnumerable<string> UnavailableProducts);

public record ShipmentRequest(string OrderId, string CustomerId, string Address, DateTime RequestedDate);

public record ShipmentResult(string ShipmentId, DateTime ScheduledDate, string Carrier);
