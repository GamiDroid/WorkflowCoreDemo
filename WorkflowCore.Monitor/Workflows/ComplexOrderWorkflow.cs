using System.Collections.Concurrent;
using WorkflowCore.AspNetCore.Extensions;
using WorkflowCore.Interface;
using WorkflowCore.Models;
using WorkflowCore.Monitor.Services;

namespace WorkflowCore.Monitor.Workflows;

/// <summary>
/// Complex workflow data met order informatie en status tracking
/// </summary>
public class ComplexOrderWorkflowData
{
    public string CustomerId { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string DeliveryAddress { get; set; } = string.Empty;
    public DateTime RequestedDeliveryDate { get; set; }
    
    public List<OrderLine> OrderLines { get; set; } = new();
    
    public decimal TotalAmount => OrderLines.Sum(l => l.Quantity * l.UnitPrice);
    
    // Status tracking per stap
    public ConcurrentDictionary<string, StepStatus> StepStatuses { get; set; } = new();
    
    // IDs van externe systemen
    public string? ErpOrderId { get; set; }
    public string? InventoryReservationId { get; set; }
    public string? ShipmentId { get; set; }
    
    // Error tracking
    public bool HasError { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ErrorStep { get; set; }
    
    // Cancellation tracking
    public bool IsCancellationRequested { get; set; }
    
    public void SetError(string stepName, string message)
    {
        HasError = true;
        ErrorStep = stepName;
        ErrorMessage = message;
        StepStatuses[stepName] = StepStatus.Failed;
    }
    
    public void SetStepStatus(string stepName, StepStatus status)
    {
        StepStatuses[stepName] = status;
    }
}

public enum StepStatus
{
    Pending,
    Running,
    Completed,
    Failed,
    Cancelled,
    Compensated
}

public class ComplexOrderSteps
{
    public const string ValidateCustomer = "ValidateCustomer";
    public const string CheckInventory = "CheckInventory";
    public const string CreateErpOrder = "CreateErpOrder";
    public const string ReserveInventory = "ReserveInventory";
    public const string ScheduleShipment = "ScheduleShipment";
    public const string ConfirmErpOrder = "ConfirmErpOrder";
    public const string Rollback = "Rollback";
}

/// <summary>
/// Complex order processing workflow met externe services en error handling
/// </summary>
public class ComplexOrderWorkflow : IWorkflow<ComplexOrderWorkflowData>
{
    public string Id => nameof(ComplexOrderWorkflow);
    public int Version => 1;

    public void Build(IWorkflowBuilder<ComplexOrderWorkflowData> builder)
    {
        builder
            .Init(w => w.Description = "Complex order processing met externe services")
            
            // Saga pattern voor rollback support
            .Saga(saga => saga
                
                // Stap 1: Valideer klant via CRM
                .Then<ValidateCustomerStep>(s => s
                    .Name("Validate Customer")
                    .OnError(WorkflowErrorHandling.Retry, TimeSpan.FromSeconds(3))
                    .CancelCondition(data => data.IsCancellationRequested)
                    .Input(step => step.CustomerId, data => data.CustomerId)
                    .Output((step, data) => data.SetStepStatus(ComplexOrderSteps.ValidateCustomer, StepStatus.Completed))
                )

                // Stap 2: Check voorraad via Inventory
                .Then<CheckInventoryStep>(s => s
                    .Name("Check Inventory")
                    .OnError(WorkflowErrorHandling.Retry, TimeSpan.FromSeconds(5))
                    .CancelCondition(data => data.HasError || data.IsCancellationRequested)
                    .Input(step => step.OrderLines, data => data.OrderLines)
                    .Output((step, data) => data.SetStepStatus(ComplexOrderSteps.CheckInventory, StepStatus.Completed))
                )

                // Stap 3: Maak order in ERP
                .Then<CreateErpOrderStep>(s => s
                    .Name("Create ERP Order")
                    .OnError(WorkflowErrorHandling.Retry, TimeSpan.FromSeconds(5))
                    .CancelCondition(data => data.HasError || data.IsCancellationRequested)
                    .Output((step, data) => 
                    {
                        data.ErpOrderId = step.CreatedOrderId;
                        data.SetStepStatus(ComplexOrderSteps.CreateErpOrder, StepStatus.Completed);
                    })
                )

                // Stap 4: Reserveer voorraad
                .Then<ReserveInventoryStep>(s => s
                    .Name("Reserve Inventory")
                    .OnError(WorkflowErrorHandling.Retry, TimeSpan.FromSeconds(5))
                    .CancelCondition(data => data.HasError || data.IsCancellationRequested)
                    .Input(step => step.OrderId, data => data.ErpOrderId!)
                    .Input(step => step.OrderLines, data => data.OrderLines)
                    .Output((step, data) => 
                    {
                        data.InventoryReservationId = step.ReservationId;
                        data.SetStepStatus(ComplexOrderSteps.ReserveInventory, StepStatus.Completed);
                    })
                )

                // Stap 5: Schedule verzending
                .Then<ScheduleShipmentStep>(s => s
                    .Name("Schedule Shipment")
                    .OnError(WorkflowErrorHandling.Retry, TimeSpan.FromSeconds(5))
                    .CancelCondition(data => data.HasError || data.IsCancellationRequested)
                    .Input(step => step.OrderId, data => data.ErpOrderId!)
                    .Input(step => step.CustomerId, data => data.CustomerId)
                    .Input(step => step.Address, data => data.DeliveryAddress)
                    .Input(step => step.RequestedDate, data => data.RequestedDeliveryDate)
                    .Output((step, data) => 
                    {
                        data.ShipmentId = step.ShipmentId;
                        data.SetStepStatus(ComplexOrderSteps.ScheduleShipment, StepStatus.Completed);
                    })
                )
                // Stap 6: Bevestig order in ERP
                .Then<ConfirmErpOrderStep>(s => s
                    .Name("Confirm ERP Order")
                    .OnError(WorkflowErrorHandling.Terminate)
                    .CancelCondition(data => data.HasError || data.IsCancellationRequested)
                    .Input(step => step.OrderId, data => data.ErpOrderId!)
                    .Output((step, data) => data.SetStepStatus(ComplexOrderSteps.ConfirmErpOrder, StepStatus.Completed))
                )
            )
            // Compensatie als er iets fout gaat
            .CompensateWith<RollbackOrderStep>()
            
            .Then(ctx => 
            {
                var data = (ComplexOrderWorkflowData)ctx.Workflow.Data;
                Console.WriteLine($"Order {data.ErpOrderId} successfully processed!");
                return ExecutionResult.Next();
            })
            .Name("Complete");
    }
}

// Workflow Steps

public class ValidateCustomerStep : IStepBody
{
    private readonly ICrmService _crmService;

    public string CustomerId { get; set; } = string.Empty;

    public ValidateCustomerStep(ICrmService crmService)
    {
        _crmService = crmService;
    }

    public async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
    {
        var data = (ComplexOrderWorkflowData)context.Workflow.Data;
        data.SetStepStatus(ComplexOrderSteps.ValidateCustomer, StepStatus.Running);
        
        Console.WriteLine($"[ValidateCustomer] Validating customer {CustomerId}...");
        
        try
        {
            var result = await _crmService.ValidateCustomerAsync(CustomerId, context.CancellationToken);
            
            if (!result.IsValid)
            {
                data.SetError("ValidateCustomer", $"Customer validation failed: {result.Reason}");
                throw new WorkflowAbortException($"Customer validation failed: {result.Reason}");
            }
            
            if (result.IsBlacklisted)
            {
                data.SetError("ValidateCustomer", "Customer is blacklisted");
                throw new WorkflowAbortException("Customer is blacklisted");
            }
            
            Console.WriteLine($"[ValidateCustomer] Customer {CustomerId} validated successfully");
            return ExecutionResult.Next();
        }
        catch (ExternalServiceException ex)
        {
            Console.WriteLine($"[ValidateCustomer] Service error: {ex.Message}");
            throw;
        }
    }
}

public class CheckInventoryStep : IStepBody
{
    private readonly IInventoryService _inventoryService;

    public IEnumerable<OrderLine> OrderLines { get; set; } = Enumerable.Empty<OrderLine>();

    public CheckInventoryStep(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    public async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
    {
        var data = (ComplexOrderWorkflowData)context.Workflow.Data;
        data.SetStepStatus(ComplexOrderSteps.CheckInventory, StepStatus.Running);
        
        Console.WriteLine($"[CheckInventory] Checking availability for {OrderLines.Count()} products...");
        
        try
        {
            var result = await _inventoryService.CheckAvailabilityAsync(OrderLines, context.CancellationToken);
            
            if (!result.AllAvailable)
            {
                var unavailable = string.Join(", ", result.UnavailableProducts);
                data.SetError("CheckInventory", $"Products not available: {unavailable}");
                throw new WorkflowAbortException($"Products not available: {unavailable}");
            }
            
            Console.WriteLine($"[CheckInventory] All products available");
            return ExecutionResult.Next();
        }
        catch (ExternalServiceException ex)
        {
            Console.WriteLine($"[CheckInventory] Service error: {ex.Message}");
            throw;
        }
    }
}

public class CreateErpOrderStep : IStepBody
{
    private readonly IErpService _erpService;

    public string? CreatedOrderId { get; set; }

    public CreateErpOrderStep(IErpService erpService)
    {
        _erpService = erpService;
    }

    public async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
    {
        var data = (ComplexOrderWorkflowData)context.Workflow.Data;
        data.SetStepStatus(ComplexOrderSteps.CreateErpOrder, StepStatus.Running);
        
        Console.WriteLine($"[CreateErpOrder] Creating order in ERP...");
        
        try
        {
            var orderRequest = new OrderRequest(
                data.CustomerId,
                data.CustomerName,
                data.OrderLines,
                data.TotalAmount
            );
            
            CreatedOrderId = await _erpService.CreateOrderAsync(orderRequest, context.CancellationToken);
            
            Console.WriteLine($"[CreateErpOrder] Order created with ID {CreatedOrderId}");
            return ExecutionResult.Next();
        }
        catch (ExternalServiceException ex)
        {
            Console.WriteLine($"[CreateErpOrder] Service error: {ex.Message}");
            data.SetError("CreateErpOrder", ex.Message);
            throw;
        }
    }
}

public class ReserveInventoryStep : IStepBody
{
    private readonly IInventoryService _inventoryService;

    public string OrderId { get; set; } = string.Empty;
    public IEnumerable<OrderLine> OrderLines { get; set; } = Enumerable.Empty<OrderLine>();
    public string? ReservationId { get; set; }

    public ReserveInventoryStep(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    public async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
    {
        var data = (ComplexOrderWorkflowData)context.Workflow.Data;
        data.SetStepStatus(ComplexOrderSteps.ReserveInventory, StepStatus.Running);
        
        Console.WriteLine($"[ReserveInventory] Reserving inventory for order {OrderId}...");
        
        try
        {
            ReservationId = await _inventoryService.ReserveInventoryAsync(OrderId, OrderLines, context.CancellationToken);
            
            Console.WriteLine($"[ReserveInventory] Inventory reserved with ID {ReservationId}");
            return ExecutionResult.Next();
        }
        catch (ExternalServiceException ex)
        {
            Console.WriteLine($"[ReserveInventory] Service error: {ex.Message}");
            data.SetError("ReserveInventory", ex.Message);
            throw;
        }
    }
}

public class ScheduleShipmentStep : IStepBody
{
    private readonly IShippingService _shippingService;

    public string OrderId { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public DateTime RequestedDate { get; set; }
    public string? ShipmentId { get; set; }

    public ScheduleShipmentStep(IShippingService shippingService)
    {
        _shippingService = shippingService;
    }

    public async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
    {
        var data = (ComplexOrderWorkflowData)context.Workflow.Data;
        data.SetStepStatus(ComplexOrderSteps.ScheduleShipment, StepStatus.Running);
        
        Console.WriteLine($"[ScheduleShipment] Scheduling shipment for order {OrderId}...");
        
        try
        {
            var request = new ShipmentRequest(OrderId, CustomerId, Address, RequestedDate);
            var result = await _shippingService.ScheduleShipmentAsync(request, context.CancellationToken);
            
            ShipmentId = result.ShipmentId;
            
            Console.WriteLine($"[ScheduleShipment] Shipment scheduled with ID {ShipmentId}, carrier: {result.Carrier}, date: {result.ScheduledDate:yyyy-MM-dd}");
            return ExecutionResult.Next();
        }
        catch (ExternalServiceException ex)
        {
            Console.WriteLine($"[ScheduleShipment] Service error: {ex.Message}");
            data.SetError("ScheduleShipment", ex.Message);
            throw;
        }
    }
}

public class ConfirmErpOrderStep : IStepBody
{
    private readonly IErpService _erpService;

    public string OrderId { get; set; } = string.Empty;

    public ConfirmErpOrderStep(IErpService erpService)
    {
        _erpService = erpService;
    }

    public async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
    {
        var data = (ComplexOrderWorkflowData)context.Workflow.Data;
        data.SetStepStatus(ComplexOrderSteps.ConfirmErpOrder, StepStatus.Running);
        
        Console.WriteLine($"[ConfirmErpOrder] Confirming order {OrderId} in ERP...");
        
        try
        {
            var confirmed = await _erpService.ConfirmOrderAsync(OrderId, context.CancellationToken);
            
            if (!confirmed)
            {
                data.SetError("ConfirmErpOrder", "Order confirmation failed");
                throw new Exception("Order confirmation failed");
            }
            
            Console.WriteLine($"[ConfirmErpOrder] Order {OrderId} confirmed");
            return ExecutionResult.Next();
        }
        catch (ExternalServiceException ex)
        {
            Console.WriteLine($"[ConfirmErpOrder] Service error: {ex.Message}");
            data.SetError("ConfirmErpOrder", ex.Message);
            throw;
        }
    }
}

/// <summary>
/// Rollback/compensatie step die alle wijzigingen ongedaan maakt
/// </summary>
public class RollbackOrderStep : IStepBody
{
    private readonly IErpService _erpService;
    private readonly IInventoryService _inventoryService;
    private readonly IShippingService _shippingService;

    public RollbackOrderStep(
        IErpService erpService,
        IInventoryService inventoryService,
        IShippingService shippingService)
    {
        _erpService = erpService;
        _inventoryService = inventoryService;
        _shippingService = shippingService;
    }

    public async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
    {
        var data = (ComplexOrderWorkflowData)context.Workflow.Data;
        
        Console.WriteLine("[Rollback] Starting rollback process...");
        
        // Cancel shipment if created
        if (!string.IsNullOrEmpty(data.ShipmentId))
        {
            try
            {
                Console.WriteLine($"[Rollback] Cancelling shipment {data.ShipmentId}...");
                await _shippingService.CancelShipmentAsync(data.ShipmentId, context.CancellationToken);
                data.SetStepStatus(ComplexOrderSteps.ScheduleShipment, StepStatus.Compensated);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Rollback] Failed to cancel shipment: {ex.Message}");
            }
        }
        
        // Release inventory reservation if created
        if (!string.IsNullOrEmpty(data.InventoryReservationId))
        {
            try
            {
                Console.WriteLine($"[Rollback] Releasing inventory reservation {data.InventoryReservationId}...");
                await _inventoryService.ReleaseReservationAsync(data.InventoryReservationId, context.CancellationToken);
                data.SetStepStatus(ComplexOrderSteps.ReserveInventory, StepStatus.Compensated);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Rollback] Failed to release reservation: {ex.Message}");
            }
        }
        
        // Cancel ERP order if created
        if (!string.IsNullOrEmpty(data.ErpOrderId))
        {
            try
            {
                Console.WriteLine($"[Rollback] Cancelling ERP order {data.ErpOrderId}...");
                await _erpService.CancelOrderAsync(data.ErpOrderId, context.CancellationToken);
                data.SetStepStatus(ComplexOrderSteps.CreateErpOrder, StepStatus.Compensated);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Rollback] Failed to cancel order: {ex.Message}");
            }
        }
        
        Console.WriteLine("[Rollback] Rollback completed");
        return ExecutionResult.Next();
    }
}
