using System;

namespace OnlineStore.Domain;

public class Customer
{
    public Guid Id { get; init; } = Guid.NewGuid();
    
    // required + init гарантує обов'язковість при ініціалізації
    public required string Name { get; init; }
    public required string Email { get; init; }
    
    public Address? ShippingAddress { get; set; }
    public string? PhoneNumber { get; set; } // Nullable Reference Type
}