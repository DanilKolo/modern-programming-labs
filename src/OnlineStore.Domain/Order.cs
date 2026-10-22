using System;
using System.Collections.Generic;
using System.Linq;

namespace OnlineStore.Domain;

public class Order
{
    private readonly List<OrderItem> _items = [];

    public Guid Id {; }
    public Customer Customer {; }
    public OrderStatus Status {; private set; } = OrderStatus.Pending;

    // Інкапсуляція колекції
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    public decimal TotalAmount => _items.Sum(item => item.TotalPrice);

    public Order(Guid id, Customer customer)
    {
        Id = id;
        Customer = customer ?? throw new ArgumentNullException(nameof(customer));
    }

    public void AddItem(Product product, int quantity)
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("Неможливо змінювати товари у замовленні, яке не знаходиться у стані Pending.");

        var existingItem = _items.FirstOrDefault(i => i.Product.Id == product.Id);
        if (existingItem != null)
        {
            _items.Remove(existingItem);
            _items.Add(new OrderItem(product, existingItem.Quantity + quantity));
        }
        else
        {
            _items.Add(new OrderItem(product, quantity));
        }
    }

    public void RemoveItem(Guid productId)
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("Неможливо видаляти товари з замовлення у даному стані.");

        var item = _items.FirstOrDefault(i => i.Product.Id == productId);
        if (item != null)
        {
            _items.Remove(item);
        }
    }

    public void Pay(IPaymentMethod paymentMethod)
    {
        if (Status == OrderStatus.Paid)
            throw new InvalidOperationException("Оплачене замовлення не можна оплачувати повторно.");

        if (Status == OrderStatus.Cancelled)
            throw new InvalidOperationException("Скасоване замовлення не можна оплатити.");

        if (!_items.Any())
            throw new InvalidOperationException("Неможливо оплатити порожнє замовлення.");

        bool success = paymentMethod.Pay(TotalAmount);
        if (success)
        {
            Status = OrderStatus.Paid;
        }
    }

    public void Cancel()
    {
        if (Status == OrderStatus.Paid)
            throw new InvalidOperationException("Оплачене замовлення не можна скасувати.");

        Status = OrderStatus.Cancelled;
    }
}