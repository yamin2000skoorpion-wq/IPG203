namespace HomeApplianceRepairCenter.Events;

/// <summary>
/// التفويض (Delegate) المستخدم للإشعار بتغير حالة الجهاز أو بتأخر تسليمه.
/// </summary>
/// <param name="sender">الجهاز الذي أطلق الحدث.</param>
/// <param name="e">تفاصيل الإشعار.</param>
public delegate void ApplianceNotificationHandler(object? sender, ApplianceNotificationEventArgs e);
