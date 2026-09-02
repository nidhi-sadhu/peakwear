using PeakWear.Core.Models.Order;

public class CheckoutResponse
{
    public OrderResponse Order { get; set; } = null!;
    public string ClientSecret { get; set; } = "";
}