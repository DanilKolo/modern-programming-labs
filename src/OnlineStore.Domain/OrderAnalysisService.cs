using System;

namespace OnlineStore.Domain;

public static class OrderAnalysisService
{
    // 1. Switch expression: опис статусу замовлення
    public static string GetStatusDescription(OrderStatus status) => status switch
    {
        OrderStatus.Created => "Замовлення створено та очікує на оплату.",
        OrderStatus.Paid => "Замовлення успішно оплачено.",
        OrderStatus.Shipped => "Замовлення передано до служби доставки.",
        OrderStatus.Delivered => "Замовлення доставлено клієнту.",
        OrderStatus.Cancelled => "Замовлення скасовано.",
        _ => "Невідомий статус."
    };

    // 2. Relational pattern: визначення категорії замовлення за сумою
    public static string GetOrderCategory(decimal totalAmount) => totalAmount switch
    {
        < 500m => "Small",
        >= 500m and < 5000m => "Medium",
        >= 5000m => "Large"
    };

    // 3. Property pattern: перевірка умов безкоштовної доставки (Paid + Total >= 2000)
    public static bool IsEligibleForFreeDelivery(Order order) => order switch
    {
        { Status: OrderStatus.Paid, TotalAmount: >= 2000m } => true,
        _ => false
    };
}