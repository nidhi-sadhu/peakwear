namespace PeakWear.Core.DbModels;

public static class OrderStatus
{
    public const string Pending = "Pending";     // created, awaiting payment
    public const string Paid = "Paid";           // payment confirmed by webhook
    public const string Failed = "Failed";       // payment declined
    public const string Expired = "Expired";     // abandoned, stock released
    public const string Cancelled = "Cancelled";
    public const string Shipped = "Shipped";
}