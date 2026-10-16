using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using HomeApplianceRepairCenter.Enums;
using HomeApplianceRepairCenter.Events;
using HomeApplianceRepairCenter.Models;
using HomeApplianceRepairCenter.Utilities;

namespace HomeApplianceRepairCenter.Services;

/// <summary>
/// الفئة الرئيسية: تحتفظ بقائمة من الكائنات المشتقة (List&lt;Appliance&gt;)
/// وتعالجها بشكل متعدد الأشكال دون معرفة النوع الفعلي لكل جهاز.
/// كما تشترك في أحداث الأجهزة وتسجل الإشعارات الواردة منها.
/// </summary>
public class RepairCenter
{
    private readonly string _name;
    private readonly List<Appliance> _appliances = new List<Appliance>();
    private readonly List<string> _notificationLog = new List<string>();

    public RepairCenter(string name)
    {
        _name = InputValidator.RequireText(name, nameof(name), 80);
    }

    public string Name => _name;

    /// <summary>قائمة الأجهزة للقراءة فقط (لا يمكن إضافة أجهزة إلا عبر Register).</summary>
    public IReadOnlyList<Appliance> Appliances => _appliances.AsReadOnly();

    /// <summary>سجل الإشعارات الواردة من الأجهزة.</summary>
    public IReadOnlyList<string> NotificationLog => _notificationLog.AsReadOnly();

    /// <summary>تسجيل جهاز جديد والاشتراك في أحداثه.</summary>
    public void Register(Appliance appliance)
    {
        if (appliance is null)
        {
            throw new ArgumentNullException(nameof(appliance));
        }

        if (FindBySerial(appliance.SerialNumber) is not null)
        {
            throw new InvalidOperationException(
                "An appliance with serial number " + appliance.SerialNumber + " is already registered.");
        }

        appliance.StatusChanged += HandleStatusChanged;
        appliance.DeliveryOverdue += HandleDeliveryOverdue;
        _appliances.Add(appliance);
        RepairStatistics.RecordRegistration(appliance.Status);
    }

    public Appliance? FindBySerial(string serialNumber)
    {
        string normalized = InputValidator.NormalizeSerialNumber(serialNumber);
        return _appliances.FirstOrDefault(a => a.SerialNumber == normalized);
    }

    public IReadOnlyList<Appliance> GetByStatus(RepairStatus status)
    {
        return _appliances.Where(a => a.Status == status).ToList().AsReadOnly();
    }

    /// <summary>
    /// مجموع تكاليف جميع الأجهزة؛ كل جهاز يحسب تكلفته بدالته الخاصة (تعدد الأشكال).
    /// </summary>
    public decimal CalculateTotalCost()
    {
        return _appliances.Sum(a => a.CalculateRepairCost());
    }

    public decimal CalculateTotalCostByStatus(RepairStatus status)
    {
        return GetByStatus(status).Sum(a => a.CalculateRepairCost());
    }

    /// <summary>ملخص حالة كل جهاز؛ كل نوع يعرض تفاصيله الخاصة (تعدد الأشكال).</summary>
    public IReadOnlyList<string> GetStatusReport()
    {
        return _appliances.Select(a => a.GetStatusSummary()).ToList().AsReadOnly();
    }

    /// <summary>
    /// يفحص جميع الأجهزة بحثًا عن تأخر التسليم، ويعيد عدد الإشعارات التي أُطلقت.
    /// </summary>
    public int CheckDeliveryDelays(DateTime currentDate)
    {
        int notifications = 0;

        foreach (Appliance appliance in _appliances)
        {
            if (appliance.CheckDeliveryDelay(currentDate))
            {
                notifications++;
            }
        }

        return notifications;
    }

    // ---------- معالجات الأحداث (Event Handlers) ----------

    private void HandleStatusChanged(object? sender, ApplianceNotificationEventArgs e)
    {
        RepairStatistics.RecordTransition(e.OldStatus, e.NewStatus);
        PublishNotification("STATUS", e);
    }

    private void HandleDeliveryOverdue(object? sender, ApplianceNotificationEventArgs e)
    {
        PublishNotification("OVERDUE", e);
    }

    private void PublishNotification(string category, ApplianceNotificationEventArgs e)
    {
        string timestamp = e.OccurredAt.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
        string transition = e.OldStatus == e.NewStatus
            ? e.NewStatus.ToString()
            : e.OldStatus + " -> " + e.NewStatus;

        string entry = "[" + category + "] " + timestamp
            + " | " + e.ApplianceType + " " + e.SerialNumber
            + " | " + transition
            + " | " + e.Message;

        _notificationLog.Add(entry);
        Console.WriteLine("  >> " + entry);
    }
}
