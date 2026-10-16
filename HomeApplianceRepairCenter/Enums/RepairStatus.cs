namespace HomeApplianceRepairCenter.Enums;

/// <summary>
/// مراحل دورة حياة الجهاز داخل مركز الصيانة.
/// </summary>
public enum RepairStatus
{
    /// <summary>تم استلام الجهاز وبانتظار بدء العمل عليه.</summary>
    Received = 0,

    /// <summary>الجهاز قيد الصيانة حاليًا.</summary>
    InRepair = 1,

    /// <summary>تم إصلاح الجهاز وبانتظار تسليمه للزبون.</summary>
    Repaired = 2,

    /// <summary>تم تسليم الجهاز للزبون.</summary>
    Delivered = 3
}
