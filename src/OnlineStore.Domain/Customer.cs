using System;

namespace OnlineStore.Domain;

public class Customer
{
    public Guid Id {; }
    public string FullName {; }
    public string Email {; }

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