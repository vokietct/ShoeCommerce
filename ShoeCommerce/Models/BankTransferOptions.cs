namespace ShoeCommerce.Models;

public sealed class BankTransferOptions
{
    public string BankName { get; set; } = "";
    public string AccountNumber { get; set; } = "";
    public string AccountName { get; set; } = "";
    public bool IsConfigured => !string.IsNullOrWhiteSpace(BankName) &&
        !string.IsNullOrWhiteSpace(AccountNumber) && !string.IsNullOrWhiteSpace(AccountName);
}
