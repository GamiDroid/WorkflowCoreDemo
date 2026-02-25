namespace WorkflowCore.Monitor.Services;

/// <summary>
/// Mock CRM service die random failures simuleert
/// </summary>
public class MockCrmService : ICrmService
{
    private readonly Random _random = new();
    private readonly ILogger<MockCrmService> _logger;

    private bool _simulateFailure = true;

    public MockCrmService(ILogger<MockCrmService> logger)
    {
        _logger = logger;
    }

    public async Task<CustomerValidationResult> ValidateCustomerAsync(string customerId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("CRM: Validating customer {CustomerId}", customerId);
        await Task.Delay(_random.Next(500, 1500), cancellationToken);

        if (_simulateFailure)
        {
            // Simuleer failures (10% kans)
            if (_random.Next(100) < 10)
            {
                _logger.LogError("CRM: Service temporarily unavailable");
                throw new ExternalServiceException("CRM service temporarily unavailable");
            }

            // Simuleer blacklist check
            if (customerId.Contains("BLOCKED", StringComparison.OrdinalIgnoreCase))
            {
                return new CustomerValidationResult(false, "Customer is blacklisted", true);
            }

            // Simuleer ongeldige klant
            if (customerId.Contains("INVALID", StringComparison.OrdinalIgnoreCase))
            {
                return new CustomerValidationResult(false, "Customer not found", false);
            } 
        }

        return new CustomerValidationResult(true, null, false);
    }

    public async Task<decimal> GetCustomerCreditLimitAsync(string customerId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("CRM: Getting credit limit for {CustomerId}", customerId);
        await Task.Delay(_random.Next(300, 800), cancellationToken);

        return _random.Next(5000, 50000);
    }
}

/// <summary>
/// Mock ERP service die order processing simuleert
/// </summary>
public class MockErpService : IErpService
{
    private readonly Random _random = new();
    private readonly ILogger<MockErpService> _logger;
    private readonly HashSet<string> _createdOrders = new();

    private bool _simulateFailure = true;

    public MockErpService(ILogger<MockErpService> logger)
    {
        _logger = logger;
    }

    public async Task<string> CreateOrderAsync(OrderRequest order, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("ERP: Creating order for customer {CustomerId} with {Count} lines", 
            order.CustomerId, order.Lines.Count());
        
        await Task.Delay(_random.Next(1000, 2000), cancellationToken);

        if (_simulateFailure)
        {
            // Simuleer failures (15% kans)
            if (_random.Next(100) < 15)
            {
                _logger.LogError("ERP: Failed to create order - system overload");
                throw new ExternalServiceException("ERP system overload");
            }
        }

        var orderId = $"ERP-{Guid.NewGuid():N}".Substring(0, 20);
        _createdOrders.Add(orderId);
        
        _logger.LogInformation("ERP: Order created with ID {OrderId}", orderId);
        return orderId;
    }

    public async Task<bool> ConfirmOrderAsync(string orderId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("ERP: Confirming order {OrderId}", orderId);
        await Task.Delay(_random.Next(500, 1000), cancellationToken);

        return _createdOrders.Contains(orderId);
    }

    public async Task CancelOrderAsync(string orderId, CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("ERP: Cancelling order {OrderId}", orderId);
        await Task.Delay(_random.Next(300, 700), cancellationToken);
        
        _createdOrders.Remove(orderId);
    }
}

/// <summary>
/// Mock Inventory service die voorraad checks simuleert
/// </summary>
public class MockInventoryService : IInventoryService
{
    private readonly Random _random = new();
    private readonly ILogger<MockInventoryService> _logger;
    private readonly Dictionary<string, string> _reservations = new();
    private readonly HashSet<string> _outOfStockProducts = new() { "PROD-999", "PROD-404" };

    private bool _simulateFailure = true;

    public MockInventoryService(ILogger<MockInventoryService> logger)
    {
        _logger = logger;
    }

    public async Task<InventoryCheckResult> CheckAvailabilityAsync(IEnumerable<OrderLine> items, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Inventory: Checking availability for {Count} items", items.Count());
        await Task.Delay(_random.Next(800, 1500), cancellationToken);

        if (_simulateFailure)
        {
            // Simuleer timeout (5% kans)
            if (_random.Next(100) < 5)
            {
                _logger.LogError("Inventory: Service timeout");
                throw new ExternalServiceException("Inventory service timeout");
            } 
        }

        var unavailable = items
            .Where(i => _outOfStockProducts.Contains(i.ProductId))
            .Select(i => i.ProductId)
            .ToList();

        var allAvailable = !unavailable.Any();
        
        _logger.LogInformation("Inventory: {Status} - {Count} unavailable products", 
            allAvailable ? "All available" : "Some unavailable", unavailable.Count);
        
        return new InventoryCheckResult(allAvailable, unavailable);
    }

    public async Task<string> ReserveInventoryAsync(string orderId, IEnumerable<OrderLine> items, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Inventory: Reserving inventory for order {OrderId}", orderId);
        await Task.Delay(_random.Next(1000, 2000), cancellationToken);

        if (_simulateFailure)
        {
            // Simuleer failure (10% kans)
            if (_random.Next(100) < 10)
            {
                _logger.LogError("Inventory: Failed to reserve - locking issue");
                throw new ExternalServiceException("Inventory reservation failed - locking issue");
            } 
        }

        var reservationId = $"RES-{Guid.NewGuid():N}".Substring(0, 20);
        _reservations[reservationId] = orderId;
        
        _logger.LogInformation("Inventory: Reserved with ID {ReservationId}", reservationId);
        return reservationId;
    }

    public async Task ReleaseReservationAsync(string reservationId, CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("Inventory: Releasing reservation {ReservationId}", reservationId);
        await Task.Delay(_random.Next(300, 700), cancellationToken);
        
        _reservations.Remove(reservationId);
    }
}

/// <summary>
/// Mock Shipping service die verzending scheduling simuleert
/// </summary>
public class MockShippingService : IShippingService
{
    private readonly Random _random = new();
    private readonly ILogger<MockShippingService> _logger;
    private readonly Dictionary<string, ShipmentResult> _shipments = new();
    private readonly string[] _carriers = { "DHL", "PostNL", "DPD", "UPS" };

    private bool _simulateFailure = true;

    public MockShippingService(ILogger<MockShippingService> logger)
    {
        _logger = logger;
    }

    public async Task<ShipmentResult> ScheduleShipmentAsync(ShipmentRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Shipping: Scheduling shipment for order {OrderId}", request.OrderId);
        await Task.Delay(_random.Next(1500, 2500), cancellationToken);

        if (_simulateFailure)
        {
            // Simuleer failure (8% kans)
            if (_random.Next(100) < 8)
            {
                _logger.LogError("Shipping: No carriers available for requested date");
                throw new ExternalServiceException("No carriers available for requested date");
            } 
        }

        var carrier = _carriers[_random.Next(_carriers.Length)];
        var scheduledDate = request.RequestedDate.AddDays(_random.Next(1, 3));
        
        var result = new ShipmentResult(
            $"SHIP-{Guid.NewGuid():N}".Substring(0, 20),
            scheduledDate,
            carrier
        );
        
        _shipments[result.ShipmentId] = result;
        
        _logger.LogInformation("Shipping: Scheduled with {Carrier} on {Date}", carrier, scheduledDate);
        return result;
    }

    public async Task CancelShipmentAsync(string shipmentId, CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("Shipping: Cancelling shipment {ShipmentId}", shipmentId);
        await Task.Delay(_random.Next(500, 1000), cancellationToken);
        
        _shipments.Remove(shipmentId);
    }
}

/// <summary>
/// Custom exception voor externe service failures
/// </summary>
public class ExternalServiceException : Exception
{
    public ExternalServiceException(string message) : base(message)
    {
    }

    public ExternalServiceException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
