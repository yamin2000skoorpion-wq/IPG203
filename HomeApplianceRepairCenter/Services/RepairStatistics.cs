using System;
using System.Threading;
using HomeApplianceRepairCenter.Enums;

namespace HomeApplianceRepairCenter.Services;

/// <summary>
/// فئة ثابتة (Static Class) تحتوي على خصائص ثابتة لعدّ الأجهزة المسجلة حسب حالتها.
/// تُحدَّث تلقائيًا من RepairCenter عند تسجيل الجهاز وعند كل تغير في حالته.
/// </summary>
public static class RepairStatistics
{
    private static readonly int[] StatusCounts = new int[Enum.GetValues(typeof(RepairStatus)).Length];
    private static int _totalRegistered;

    /// <summary>إجمالي الأجهزة التي سُجّلت في المركز.</summary>
    public static int TotalRegistered => _totalRegistered;

    /// <summary>عدد الأجهزة المستلمة وبانتظار بدء الصيانة.</summary>
    public static int ReceivedCount => CountByStatus(RepairStatus.Received);

    /// <summary>عدد الأجهزة التي ما زالت قيد الصيانة.</summary>
    public static int UnderRepairCount => CountByStatus(RepairStatus.InRepair);

    /// <summary>عدد الأجهزة التي تم إصلاحها وبانتظار التسليم.</summary>
    public static int RepairedCount => CountByStatus(RepairStatus.Repaired);

    /// <summary>عدد الأجهزة التي سُلّمت للزبائن.</summary>
    public static int DeliveredCount => CountByStatus(RepairStatus.Delivered);

    /// <summary>عدد الأجهزة في حالة معينة.</summary>
    public static int CountByStatus(RepairStatus status)
    {
        return StatusCounts[(int)status];
    }

    internal static void RecordRegistration(RepairStatus initialStatus)
    {
        Interlocked.Increment(ref _totalRegistered);
        Interlocked.Increment(ref StatusCounts[(int)initialStatus]);
    }

    internal static void RecordTransition(RepairStatus oldStatus, RepairStatus newStatus)
    {
        Interlocked.Decrement(ref StatusCounts[(int)oldStatus]);
        Interlocked.Increment(ref StatusCounts[(int)newStatus]);
    }
}
