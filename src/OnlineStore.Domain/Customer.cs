using System;

namespace OnlineStore.Domain;

public class Customer
{
    public Guid Id { get; }
    public string FullName { get; }
    public string Email { get; }

    public Customer(Guid id, string fullName, string email)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new ArgumentException("ПІБ покупця не може бути порожнім.");

        if (string.IsNullOrWhiteSpace(email) || !email.Contains("@"))
            throw new ArgumentException("Вказано некоректний Email.");

        Id = id;
        FullName = fullName;
        Email = email;
    }
}