using System;
using System.Collections.Generic;
using System.Linq;

namespace OnlineStore.Domain;

public class Order
{
    private readonly List<OrderItem> _items = [];

    public Guid Id { get; init; } = Guid.NewGuid();
    public Customer Customer { get; }
    public OrderStatus Status {; private set; } = OrderStatus.Created;

    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();
    public decimal TotalAmount => _items.Sum(item => item.TotalPrice);

    public Order(Customer customer)
    {
        Customer = customer ?? throw new ArgumentNullException(nameof(customer));
    }

    public void AddItem(Product product, int quantity)
    {
        if (Status != OrderStatus.Created)
            throw new InvalidOperationException("Неможливо змінювати замовлення після його обробки.");

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

    public void Pay(IPaymentMethod paymentMethod)
    {
        if (Status == OrderStatus.Paid)
            throw new InvalidOperationException("Оплачене замовлення не можна оплачувати повторно.");

        if (Status == OrderStatus.Cancelled)
            throw new InvalidOperationException("Скасоване замовлення не можна оплатити.");

        if (!_items.Any())
            throw new InvalidOperationException("Неможливо оплатити порожнє замовлення.");

        if (paymentMethod.Pay(TotalAmount))
        {
            Status = OrderStatus.Paid;
        }
    }

    public void Ship()
    {
        if (Status != OrderStatus.Paid)
            throw new InvalidOperationException("Доставити можна тільки оплачене замовлення.");

        Status = OrderStatus.Shipped;
    }

    public void Cancel()
    {
        if (Status == OrderStatus.Paid || Status == OrderStatus.Shipped || Status == OrderStatus.Delivered)
            throw new InvalidOperationException("Завершене або оплачене замовлення скасувати не можна.");

        Status = OrderStatus.Cancelled;
    }
}