using System;
using System.Globalization;
using HomeApplianceRepairCenter.Enums;
using HomeApplianceRepairCenter.Events;
using HomeApplianceRepairCenter.Interfaces;
using HomeApplianceRepairCenter.Utilities;

namespace HomeApplianceRepairCenter.Models;

/// <summary>
/// الفئة المجردة (Abstract Class) التي تمثل أي جهاز منزلي داخل مركز الصيانة.
/// تطبق الواجهة IRepairable وتحتوي على الخصائص المشتركة والدالة المجردة CalculateRepairCost.
/// </summary>
public abstract class Appliance : IRepairable
{
    /// <summary>أجرة ساعة العمل الموحدة لجميع الأجهزة.</summary>
    private const decimal HourlyLaborRate = 10m;

    // ---------- الحقول (جميعها خاصة - Encapsulation) ----------
    private readonly string _serialNumber;
    private readonly string _brand;
    private readonly string _model;
    private readonly string _customerName;
    private readonly DateTime _receivedDate;
    private readonly DateTime _expectedDeliveryDate;
    private string _faultDescription;
    private RepairStatus _status;
    private decimal _laborHours;
    private decimal _partsCost;
    private bool _overdueNotified;

    // ---------- الأحداث (Events) المبنية على التفويض ApplianceNotificationHandler ----------

    /// <summary>يُطلق عند تغير حالة الجهاز (بدء الإصلاح، اكتماله، التسليم).</summary>
    public event ApplianceNotificationHandler? StatusChanged;

    /// <summary>يُطلق مرة واحدة عند تأخر تسليم الجهاز عن الموعد المتوقع.</summary>
    public event ApplianceNotificationHandler? DeliveryOverdue;

    protected Appliance(
        string serialNumber,
        string brand,
        string model,
        string customerName,
        DateTime receivedDate,
        DateTime expectedDeliveryDate,
        string faultDescription)
    {
        _serialNumber = InputValidator.RequireValidSerialNumber(serialNumber);
        _brand = InputValidator.RequireText(brand, nameof(brand), 50);
        _model = InputValidator.RequireText(model, nameof(model), 50);
        _customerName = InputValidator.RequireValidPersonName(customerName);
        _receivedDate = InputValidator.RequireReceivedDate(receivedDate);
        _expectedDeliveryDate = InputValidator.RequireDeliveryDate(receivedDate, expectedDeliveryDate);
        _faultDescription = InputValidator.RequireText(faultDescription, nameof(faultDescription), 200);
        _status = RepairStatus.Received;
    }

    // ---------- خصائص للقراءة فقط بعد الإنشاء ----------

    public string SerialNumber => _serialNumber;

    public string Brand => _brand;

    public string Model => _model;

    public string CustomerName => _customerName;

    public DateTime ReceivedDate => _receivedDate;

    public DateTime ExpectedDeliveryDate => _expectedDeliveryDate;

    /// <summary>الحالة تتغير فقط عبر StartRepair / CompleteRepair / Deliver.</summary>
    public RepairStatus Status => _status;

    /// <summary>نوع الجهاز (ثلاجة، غسالة، مكيف ...) وتحدده كل فئة فرعية.</summary>
    public abstract string ApplianceType { get; }

    // ---------- خصائص قابلة للتعديل بشروط ----------

    public string FaultDescription
    {
        get => _faultDescription;
        set
        {
            EnsureEditable();
            _faultDescription = InputValidator.RequireText(value, nameof(FaultDescription), 200);
        }
    }

    public decimal LaborHours
    {
        get => _laborHours;
        set
        {
            EnsureEditable();
            _laborHours = InputValidator.RequireNonNegative(value, nameof(LaborHours));
        }
    }

    public decimal PartsCost
    {
        get => _partsCost;
        set
        {
            EnsureEditable();
            _partsCost = InputValidator.RequireNonNegative(value, nameof(PartsCost));
        }
    }

    // ---------- الدالة المجردة (Polymorphism) ----------

    /// <summary>
    /// حساب تكلفة الإصلاح؛ تعيد كل فئة فرعية تعريفها بما يناسب طبيعة الجهاز.
    /// </summary>
    public abstract decimal CalculateRepairCost();

    // ---------- دورة حياة الصيانة ----------

    public void StartRepair()
    {
        RequireStatus(RepairStatus.Received, "start the repair");
        ChangeStatus(RepairStatus.InRepair, "Repair work has started.");
    }

    public void CompleteRepair()
    {
        RequireStatus(RepairStatus.InRepair, "complete the repair");
        ChangeStatus(
            RepairStatus.Repaired,
            "Repair completed. Final cost: " + MoneyFormatter.Format(CalculateRepairCost()) + ".");
    }

    public void Deliver()
    {
        RequireStatus(RepairStatus.Repaired, "deliver the appliance");
        ChangeStatus(RepairStatus.Delivered, "Appliance delivered to the customer.");
    }

    public bool IsOverdue(DateTime currentDate)
    {
        return _status != RepairStatus.Delivered && currentDate.Date > _expectedDeliveryDate;
    }

    /// <summary>
    /// يفحص التأخير ويطلق الحدث DeliveryOverdue مرة واحدة فقط.
    /// تعيد true إذا أُطلق الحدث في هذا الاستدعاء.
    /// </summary>
    public bool CheckDeliveryDelay(DateTime currentDate)
    {
        if (_overdueNotified || !IsOverdue(currentDate))
        {
            return false;
        }

        _overdueNotified = true;
        int daysLate = (currentDate.Date - _expectedDeliveryDate).Days;
        OnDeliveryOverdue(new ApplianceNotificationEventArgs(
            _serialNumber,
            ApplianceType,
            _status,
            _status,
            "Delivery is overdue by " + daysLate.ToString(CultureInfo.InvariantCulture) + " day(s)."));
        return true;
    }

    public virtual string GetStatusSummary()
    {
        return "[" + ApplianceType + "] " + _serialNumber
            + " | " + _brand + " " + _model
            + " | Customer: " + _customerName
            + " | Status: " + _status
            + " | Due: " + _expectedDeliveryDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
            + " | Cost: " + MoneyFormatter.Format(CalculateRepairCost());
    }

    public override string ToString()
    {
        return GetStatusSummary();
    }

    // ---------- أدوات محمية تستخدمها الفئات الفرعية ----------

    /// <summary>التكلفة الأساسية المشتركة: أجور العمل + قطع الغيار.</summary>
    protected decimal CalculateBaseCost()
    {
        return (_laborHours * HourlyLaborRate) + _partsCost;
    }

    /// <summary>يمنع تعديل بيانات الإصلاح بعد اكتمال الصيانة.</summary>
    protected void EnsureEditable()
    {
        if (_status == RepairStatus.Repaired || _status == RepairStatus.Delivered)
        {
            throw new InvalidOperationException(
                "Appliance " + _serialNumber + " is already repaired; its repair details can no longer be modified.");
        }
    }

    protected static string YesNo(bool value)
    {
        return value ? "Yes" : "No";
    }

    protected virtual void OnStatusChanged(ApplianceNotificationEventArgs e)
    {
        StatusChanged?.Invoke(this, e);
    }

    protected virtual void OnDeliveryOverdue(ApplianceNotificationEventArgs e)
    {
        DeliveryOverdue?.Invoke(this, e);
    }

    // ---------- دوال خاصة ----------

    private void RequireStatus(RepairStatus expected, string action)
    {
        if (_status != expected)
        {
            throw new InvalidOperationException(
                "Cannot " + action + " for appliance " + _serialNumber
                + ": current status is " + _status + ", expected " + expected + ".");
        }
    }

    private void ChangeStatus(RepairStatus newStatus, string message)
    {
        RepairStatus oldStatus = _status;
        _status = newStatus;
        OnStatusChanged(new ApplianceNotificationEventArgs(
            _serialNumber,
            ApplianceType,
            oldStatus,
            newStatus,
            message));
    }
}
