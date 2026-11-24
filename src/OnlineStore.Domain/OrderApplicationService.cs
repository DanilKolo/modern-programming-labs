using Microsoft.Extensions.Logging;

namespace OnlineStore.Domain;

public sealed class OrderApplicationService(
    INotificationService notificationService,
    ILogger<OrderApplicationService> logger)
{
    private readonly INotificationService _notificationService =
        notificationService ?? throw new ArgumentNullException(nameof(notificationService));

    private readonly ILogger<OrderApplicationService> _logger =
        logger ?? throw new ArgumentNullException(nameof(logger));

    public void PayOrder(Order order, IPaymentMethod paymentMethod)
    {
        ArgumentNullException.ThrowIfNull(order);
        ArgumentNullException.ThrowIfNull(paymentMethod);

        _logger.LogInformation(
            "Starting payment for order {OrderId} (Customer: {CustomerName}, Amount: {Amount})",
            order.Id,
            order.Customer.Name,
            order.TotalAmount);

        try
        {
            order.Pay(paymentMethod);

            if (order.Status == OrderStatus.Paid)
            {
                _notificationService.Notify(
                    $"Замовлення {order.Id} на суму {order.TotalAmount} грн успішно оплачено.");

                _logger.LogInformation(
                    "Order {OrderId} successfully paid. Notification sent.",
                    order.Id);
            }
            else
            {
                _logger.LogWarning(
                    "Payment provider declined payment for order {OrderId}. Status remains {Status}.",
                    order.Id,
                    order.Status);
            }
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(
                ex,
                "Cannot pay order {OrderId} in status {Status}: {Reason}",
                order.Id,
                order.Status,
                ex.Message);
            throw;
        }
    }
}
