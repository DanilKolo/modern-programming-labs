using System;

namespace OnlineStore.Domain;

public class OrderItem
{
    public Product Product { get; }
    public int Quantity { get; private set; }
    public decimal TotalPrice => Product.Price * Quantity;

    public OrderItem(Product product, int quantity)
    {
        Product = product ?? throw new ArgumentNullException(nameof(product));

        if (quantity <= 0)
            throw new ArgumentException("Кількість товару повинна бути більше 0.");

        Quantity = quantity;
    }
}