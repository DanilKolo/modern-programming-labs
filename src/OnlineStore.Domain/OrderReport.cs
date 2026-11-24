namespace OnlineStore.Domain;

public sealed record OrderReport(
    Guid OrderId,
    string CustomerName,
    decimal Total,
    string Status,
    int ItemsCount)
{
    public static OrderReport FromOrder(Order order)
    {
        ArgumentNullException.ThrowIfNull(order);

        return new OrderReport(
            OrderId: order.Id,
            CustomerName: order.Customer.Name,
            Total: order.TotalAmount,
            Status: order.Status.ToString(),
            ItemsCount: order.Items.Sum(i => i.Quantity));
    }
}
