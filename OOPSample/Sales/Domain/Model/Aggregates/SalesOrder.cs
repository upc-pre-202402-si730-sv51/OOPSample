using OOPSample.Shared.Domain.Model.ValueObjects;

namespace OOPSample.Sales.Domain.Model.Aggregates;

public class SalesOrder(int customerId)
{
    // Public Section
    
    public Guid Id { get; } = GenerateOrderId();
    
    public int CustomerId { get; } = customerId;

    public ESalesOrderStatus Status { get; private set; } = ESalesOrderStatus.PendingPayment;

    private Address _shippingAddress;

    public string ShippingAddress => _shippingAddress.AddressAsString;
    
    public double PaidAmount { get; private set; }

    public void AddItem(int productId, int quantity, double unitPrice)
    {
        if (Status != ESalesOrderStatus.PendingPayment)
            throw new InvalidOperationException("Can't modify and order once payment is processed.");
        _items.Add(new SalesOrderItem(Id, productId, quantity, unitPrice));
    }

    public void Cancel()
    {
        Status = ESalesOrderStatus.Cancelled;
    }

    public void Dispatch(string street, string number, string city, string state, string zipCode, string country)
    {
        if (Status == ESalesOrderStatus.PendingPayment)
            throw new InvalidOperationException("Can't dispatch and order that is not paid yet.");

        if (_items.Count == 0)
            throw new InvalidOperationException("Can't dispatch an order without items.");
        
        VerifyIfReadyForShipment();
        if (Status != ESalesOrderStatus.ReadyForShipment) return;
        _shippingAddress = new Address(street, number, city, state, zipCode, country);
        Status = ESalesOrderStatus.Shipped;
    }

    public void AddPayment(double amount)
    {
        if (amount <= 0)
            throw new InvalidOperationException("Amount must be greater than zero.");
        
        if (amount > CalculateTotalPrice() - PaidAmount)
            throw new InvalidOperationException("Amount must be less than or equal to the remaining amount.");
        
        PaidAmount += amount;
        
        VerifyIfReadyForShipment();
    }
    
    // Private Section

    private void VerifyIfReadyForShipment()
    {
        if (PaidAmount == CalculateTotalPrice())
            Status = ESalesOrderStatus.ReadyForShipment;
    }
    private double CalculateTotalPrice() => _items.Sum(item => item.CalculateItemPrice());
    
    private readonly List<SalesOrderItem> _items = [];

    private static Guid GenerateOrderId() => Guid.NewGuid();
}