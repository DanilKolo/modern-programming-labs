using System;

namespace OnlineStore.Domain;

public class Product
{
    public Guid Id { get; }
    public string Name { get; }
    public decimal Price { get; private set; }

    public Product(Guid id, string name, decimal price)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Назва товару не може бути порожньою.");

        if (price <= 0)
            throw new ArgumentException("Ціна товару повинна бути більше 0.");

        Id = id;
        Name = name;
        Price = price;
    }
}