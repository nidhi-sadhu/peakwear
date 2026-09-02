namespace PeakWear.Core.Services;

public static class Money
{
    // 45.99m -> 4599. MidpointRounding.ToEven ("banker's rounding") is .NET's
    // default and rounds .5 toward the nearest even number, which skews totals
    // over many orders. AwayFromZero is what people expect money to do.
    public static long ToMinorUnits(decimal amount) =>
        (long)Math.Round(amount * 100m, MidpointRounding.AwayFromZero);
}