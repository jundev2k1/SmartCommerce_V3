namespace NovaCore.Order.Domain.Enums;

public enum PaymentProvider : short
{
    Stripe = 1,
    PayPal = 2,
    VNPay = 3,
    MoMo = 4,
    ZaloPay = 5,
    ApplePay = 6,
    GooglePay = 7,
    Manual = 8,
}
