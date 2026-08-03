namespace SmartEcommerce.User.Domain.Enums;

public enum PaymentType : short
{
    CreditCard = 1,
    DebitCard = 2,
    BankAccount = 3,
    EWallet = 4,
    QRCode = 5,
}
