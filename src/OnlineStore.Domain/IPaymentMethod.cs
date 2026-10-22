namespace OnlineStore.Domain;

public interface IPaymentMethod
{
    bool Pay(decimal amount);
}