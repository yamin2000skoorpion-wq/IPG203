using System;
using System.Globalization;
using HomeApplianceRepairCenter.Utilities;

namespace HomeApplianceRepairCenter.UI;

/// <summary>
/// فئة ثابتة (Static Class) لقراءة مدخلات المستخدم من الشاشة مع إعادة السؤال حتى تصبح القيمة صحيحة.
/// تعتمد على InputValidator في التحقق، وتكتب "cancel" في أي سؤال لإلغاء العملية الحالية.
/// </summary>
public static class ConsoleInput
{
    private const string CancelWord = "cancel";
    private const string DateFormat = "yyyy-MM-dd";

    // ---------- القراءة الأساسية ----------

    /// <summary>يعرض السؤال ويقرأ سطرًا؛ يرمي استثناءً عند الإلغاء أو عند انتهاء المدخلات.</summary>
    public static string ReadLine(string prompt)
    {
        Console.Write(prompt);
        string? line = Console.ReadLine();

        if (line is null)
        {
            throw new System.IO.EndOfStreamException("Input stream closed.");
        }

        line = line.Trim();

        if (line.Equals(CancelWord, StringComparison.OrdinalIgnoreCase))
        {
            throw new OperationCanceledException("Operation cancelled by the user.");
        }

        return line;
    }

    public static void PrintError(string message)
    {
        Console.WriteLine("  ! " + message);
    }

    // ---------- قراءة نصوص ----------

    public static string ReadSerialNumber(Func<string, bool>? isAlreadyUsed = null)
    {
        while (true)
        {
            string input = ReadLine("  Serial number (e.g. RF-100234): ");

            if (!InputValidator.IsValidSerialNumber(input))
            {
                PrintError("Serial number must be 2-3 letters, a hyphen, then 4-8 digits (e.g. RF-100234).");
                continue;
            }

            string normalized = InputValidator.NormalizeSerialNumber(input);

            if (isAlreadyUsed is not null && isAlreadyUsed(normalized))
            {
                PrintError("An appliance with serial number " + normalized + " is already registered.");
                continue;
            }

            return normalized;
        }
    }

    public static string ReadPersonName(string prompt)
    {
        while (true)
        {
            string input = ReadLine(prompt);

            if (InputValidator.IsValidPersonName(input))
            {
                return input;
            }

            PrintError("Name must contain 3-50 letters and spaces only.");
        }
    }

    public static string ReadText(string prompt, int maxLength)
    {
        while (true)
        {
            string input = ReadLine(prompt);

            if (InputValidator.IsValidText(input, maxLength))
            {
                return input;
            }

            PrintError("Value must not be empty and must not exceed " + maxLength + " characters.");
        }
    }

    /// <summary>نص اختياري: الضغط على Enter يُبقي القيمة الحالية.</summary>
    public static string ReadOptionalText(string prompt, string currentValue, int maxLength)
    {
        while (true)
        {
            string input = ReadLine(prompt);

            if (input.Length == 0)
            {
                return currentValue;
            }

            if (InputValidator.IsValidText(input, maxLength))
            {
                return input;
            }

            PrintError("Value must not exceed " + maxLength + " characters.");
        }
    }

    // ---------- قراءة تواريخ ----------

    /// <summary>
    /// يقرأ تاريخًا بصيغة yyyy-MM-dd؛ الضغط على Enter يختار القيمة الافتراضية.
    /// </summary>
    public static DateTime ReadDate(
        string label,
        DateTime defaultValue,
        Func<DateTime, bool> isAcceptable,
        string rejectionMessage)
    {
        string defaultText = defaultValue.ToString(DateFormat, CultureInfo.InvariantCulture);

        while (true)
        {
            string input = ReadLine(label + " (" + DateFormat + ", Enter = " + defaultText + "): ");
            DateTime value = defaultValue;

            if (input.Length > 0
                && !DateTime.TryParseExact(input, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out value))
            {
                PrintError("Invalid date. Use the format " + DateFormat + " (e.g. " + defaultText + ").");
                continue;
            }

            if (!isAcceptable(value))
            {
                PrintError(rejectionMessage);
                continue;
            }

            return value.Date;
        }
    }

    // ---------- قراءة أرقام ----------

    public static int ReadPositiveInt(string prompt)
    {
        while (true)
        {
            string input = ReadLine(prompt);

            if (int.TryParse(input, NumberStyles.None, CultureInfo.InvariantCulture, out int value) && value > 0)
            {
                return value;
            }

            PrintError("Enter a whole number greater than zero.");
        }
    }

    public static int ReadMenuChoice(string prompt, int min, int max)
    {
        while (true)
        {
            string input = ReadLine(prompt);

            if (int.TryParse(input, NumberStyles.None, CultureInfo.InvariantCulture, out int value)
                && value >= min && value <= max)
            {
                return value;
            }

            PrintError("Enter a number between " + min + " and " + max + ".");
        }
    }

    /// <summary>رقم عشري غير سالب اختياري: الضغط على Enter يُبقي القيمة الحالية.</summary>
    public static decimal ReadOptionalDecimal(string prompt, decimal currentValue)
    {
        while (true)
        {
            string input = ReadLine(prompt);

            if (input.Length == 0)
            {
                return currentValue;
            }

            if (TryParseDecimal(input, out decimal value) && value >= 0m)
            {
                return value;
            }

            PrintError("Enter a number that is zero or greater (e.g. 2.5).");
        }
    }

    // ---------- قراءة نعم / لا ----------

    public static bool ReadYesNo(string prompt)
    {
        while (true)
        {
            string input = ReadLine(prompt);

            if (TryParseYesNo(input, out bool value))
            {
                return value;
            }

            PrintError("Answer with y (yes) or n (no).");
        }
    }

    /// <summary>سؤال نعم/لا اختياري: الضغط على Enter يُبقي القيمة الحالية.</summary>
    public static bool ReadOptionalYesNo(string prompt, bool currentValue)
    {
        while (true)
        {
            string input = ReadLine(prompt);

            if (input.Length == 0)
            {
                return currentValue;
            }

            if (TryParseYesNo(input, out bool value))
            {
                return value;
            }

            PrintError("Answer with y (yes) or n (no), or press Enter to keep the current value.");
        }
    }

    // ---------- دوال مساعدة خاصة ----------

    private static bool TryParseDecimal(string input, out decimal value)
    {
        return decimal.TryParse(
            input.Replace(',', '.'),
            NumberStyles.AllowDecimalPoint,
            CultureInfo.InvariantCulture,
            out value);
    }

    private static bool TryParseYesNo(string input, out bool value)
    {
        string text = input.Trim().ToLowerInvariant();

        if (text == "y" || text == "yes")
        {
            value = true;
            return true;
        }

        if (text == "n" || text == "no")
        {
            value = false;
            return true;
        }

        value = false;
        return false;
    }
}
