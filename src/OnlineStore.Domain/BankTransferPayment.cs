using System;

namespace OnlineStore.Domain;

public class BankTransferPayment : IPaymentMethod
{
    public string Iban { get; }

    public BankTransferPayment(string iban)
    {
        Iban = iban;
    }

    public bool Pay(decimal amount)
    {
        Console.WriteLine($"[BankTransferPayment] Банківський переказ на IBAN {Iban} на суму {amount:C} підтверджено.");
        return true;
    }
}