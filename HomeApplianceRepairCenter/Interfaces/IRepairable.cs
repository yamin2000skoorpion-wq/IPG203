using System;
using HomeApplianceRepairCenter.Enums;

namespace HomeApplianceRepairCenter.Interfaces;

/// <summary>
/// العمليات الأساسية المشتركة بين جميع الأجهزة المنزلية في مركز الصيانة (Abstraction).
/// </summary>
public interface IRepairable
{
    /// <summary>الرقم التسلسلي للجهاز.</summary>
    string SerialNumber { get; }

    /// <summary>الحالة الحالية للجهاز.</summary>
    RepairStatus Status { get; }

    /// <summary>حساب تكلفة الإصلاح الحالية.</summary>
    decimal CalculateRepairCost();

    /// <summary>بدء أعمال الصيانة.</summary>
    void StartRepair();

    /// <summary>إنهاء أعمال الصيانة.</summary>
    void CompleteRepair();

    /// <summary>تسليم الجهاز للزبون.</summary>
    void Deliver();

    /// <summary>هل تأخر تسليم الجهاز عن الموعد المتوقع في التاريخ المعطى؟</summary>
    bool IsOverdue(DateTime currentDate);

    /// <summary>ملخص نصي لحالة الجهاز.</summary>
    string GetStatusSummary();
}
