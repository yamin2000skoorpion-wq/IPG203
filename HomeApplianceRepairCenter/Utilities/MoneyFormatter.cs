using System.Globalization;

namespace HomeApplianceRepairCenter.Utilities;

/// <summary>
/// تنسيق المبالغ المالية بشكل موحد وبغض النظر عن لغة الجهاز.
/// </summary>
public static class MoneyFormatter
{
    private const string CurrencyCode = "USD";

    public static string Format(decimal amount)
    {
        return amount.ToString("F2", CultureInfo.InvariantCulture) + " " + CurrencyCode;
    }
}
