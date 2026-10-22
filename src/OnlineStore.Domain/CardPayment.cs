using System;

namespace OnlineStore.Domain;

public class CardPayment : IPaymentMethod
{
    public string CardNumber { get; }

    public CardPayment(string cardNumber)
    {
        CardNumber = cardNumber;
    }

    public bool Pay(decimal amount)
    {
        Console.WriteLine($"[CardPayment] Оплата карткою ({CardNumber}) на суму {amount:C} пройшла успішно.");
        return true;
    }
}