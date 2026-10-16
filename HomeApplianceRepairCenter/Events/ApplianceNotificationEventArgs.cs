using System;
using HomeApplianceRepairCenter.Enums;

namespace HomeApplianceRepairCenter.Events;

/// <summary>
/// بيانات الإشعار المرسلة مع أحداث الجهاز (للقراءة فقط بعد الإنشاء).
/// </summary>
public class ApplianceNotificationEventArgs : EventArgs
{
    public ApplianceNotificationEventArgs(
        string serialNumber,
        string applianceType,
        RepairStatus oldStatus,
        RepairStatus newStatus,
        string message)
    {
        SerialNumber = serialNumber;
        ApplianceType = applianceType;
        OldStatus = oldStatus;
        NewStatus = newStatus;
        Message = message;
        OccurredAt = DateTime.Now;
    }

    public string SerialNumber { get; }

    public string ApplianceType { get; }

    public RepairStatus OldStatus { get; }

    public RepairStatus NewStatus { get; }

    public string Message { get; }

    public DateTime OccurredAt { get; }
}
