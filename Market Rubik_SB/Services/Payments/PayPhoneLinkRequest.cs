namespace MiniMarketApp.Services.Payments;

public class PayPhoneLinkRequest
{
    public int Amount { get; set; }
    public int AmountWithoutTax { get; set; }
    public int AmountWithTax { get; set; }
    public int Tax { get; set; }
    public int? Service { get; set; } = null;
    public int? Tip { get; set; } = null;
    public string Currency { get; set; } = "USD";
    public string Reference { get; set; } = string.Empty;
    public string ClientTransactionId { get; set; } = string.Empty;
    public string? StoreId { get; set; } = null;
    public string? AdditionalData { get; set; } = null;
    public bool? OneTime { get; set; } = null;
    public int? ExpireIn { get; set; } = null;
    public bool? IsAmountEditable { get; set; } = null;
}
