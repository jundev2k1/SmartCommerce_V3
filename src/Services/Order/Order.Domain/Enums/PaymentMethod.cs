namespace NovaCore.Order.Domain.Enums;

public enum PaymentMethod : short
{
    CreditCard = 1,
    DebitCard = 2,
    BankAccount = 3,
    EWallet = 4,
    QRCode = 5,
    CashOnDelivery = 6,
}
