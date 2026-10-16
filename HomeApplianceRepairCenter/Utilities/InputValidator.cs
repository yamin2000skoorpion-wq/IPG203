using System;
using System.Text.RegularExpressions;

namespace HomeApplianceRepairCenter.Utilities;

/// <summary>
/// فئة ثابتة (Static Class) للتحقق من صحة البيانات المدخلة.
/// الدوال Is... تعيد true/false، والدوال Require... ترمي استثناءً وتعيد القيمة المنظّفة.
/// </summary>
public static class InputValidator
{
    private const int MinimumSupportedYear = 2000;

    // الرقم التسلسلي: 2-3 أحرف كبيرة + شرطة + 4-8 أرقام (مثال: RF-100234).
    private static readonly Regex SerialNumberPattern =
        new Regex("^[A-Z]{2,3}-[0-9]{4,8}$", RegexOptions.Compiled);

    // اسم الشخص: 3-50 حرفًا (أحرف ومسافات فقط) ويبدأ بحرف.
    private static readonly Regex PersonNamePattern =
        new Regex(@"^\p{L}[\p{L} ]{2,49}$", RegexOptions.Compiled);

    // ---------- دوال التحقق (Is...) ----------

    public static string NormalizeSerialNumber(string? serialNumber)
    {
        return (serialNumber ?? string.Empty).Trim().ToUpperInvariant();
    }

    public static bool IsValidSerialNumber(string? serialNumber)
    {
        return SerialNumberPattern.IsMatch(NormalizeSerialNumber(serialNumber));
    }

    public static bool IsValidPersonName(string? name)
    {
        return name is not null && PersonNamePattern.IsMatch(name.Trim());
    }

    public static bool IsValidText(string? text, int maxLength)
    {
        return !string.IsNullOrWhiteSpace(text) && text.Trim().Length <= maxLength;
    }

    public static bool IsValidReceivedDate(DateTime receivedDate)
    {
        return receivedDate.Date >= new DateTime(MinimumSupportedYear, 1, 1)
            && receivedDate.Date <= DateTime.Today;
    }

    public static bool IsValidDeliveryDate(DateTime receivedDate, DateTime expectedDeliveryDate)
    {
        return expectedDeliveryDate.Date >= receivedDate.Date;
    }

    // ---------- دوال الفرض (Require...) ----------

    public static string RequireValidSerialNumber(string? serialNumber)
    {
        if (!IsValidSerialNumber(serialNumber))
        {
            throw new ArgumentException(
                "Serial number must look like 'RF-100234' (2-3 letters, a hyphen, then 4-8 digits).",
                nameof(serialNumber));
        }

        return NormalizeSerialNumber(serialNumber);
    }

    public static string RequireValidPersonName(string? name)
    {
        if (!IsValidPersonName(name))
        {
            throw new ArgumentException(
                "Name must contain 3-50 letters and spaces only.",
                nameof(name));
        }

        return (name ?? string.Empty).Trim();
    }

    public static string RequireText(string? text, string paramName, int maxLength)
    {
        if (!IsValidText(text, maxLength))
        {
            throw new ArgumentException(
                $"{paramName} must not be empty and must not exceed {maxLength} characters.",
                paramName);
        }

        return (text ?? string.Empty).Trim();
    }

    public static DateTime RequireReceivedDate(DateTime receivedDate)
    {
        if (!IsValidReceivedDate(receivedDate))
        {
            throw new ArgumentOutOfRangeException(
                nameof(receivedDate),
                receivedDate,
                "Received date must be between 2000-01-01 and today.");
        }

        return receivedDate.Date;
    }

    public static DateTime RequireDeliveryDate(DateTime receivedDate, DateTime expectedDeliveryDate)
    {
        if (!IsValidDeliveryDate(receivedDate, expectedDeliveryDate))
        {
            throw new ArgumentOutOfRangeException(
                nameof(expectedDeliveryDate),
                expectedDeliveryDate,
                "Expected delivery date cannot be earlier than the received date.");
        }

        return expectedDeliveryDate.Date;
    }

    public static decimal RequireNonNegative(decimal value, string paramName)
    {
        if (value < 0m)
        {
            throw new ArgumentOutOfRangeException(paramName, value, "Value cannot be negative.");
        }

        return value;
    }

    public static int RequirePositive(int value, string paramName)
    {
        if (value <= 0)
        {
            throw new ArgumentOutOfRangeException(paramName, value, "Value must be greater than zero.");
        }

        return value;
    }
}
