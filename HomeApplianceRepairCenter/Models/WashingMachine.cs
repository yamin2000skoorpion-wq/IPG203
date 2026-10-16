using System;
using HomeApplianceRepairCenter.Utilities;

namespace HomeApplianceRepairCenter.Models;

/// <summary>
/// غسالة: تتأثر التكلفة بنوع التحميل (أمامي/علوي) واستبدال رولمان البلي (Bearings) الخاصة بالأسطوانة.
/// </summary>
public class WashingMachine : Appliance
{
    private const decimal FrontLoadingServiceFee = 20m;
    private const decimal DrumBearingReplacementFee = 45m;

    private readonly int _loadCapacityKg;
    private readonly bool _isFrontLoading;
    private bool _drumBearingReplaced;

    public WashingMachine(
        string serialNumber,
        string brand,
        string model,
        string customerName,
        DateTime receivedDate,
        DateTime expectedDeliveryDate,
        string faultDescription,
        int loadCapacityKg,
        bool isFrontLoading)
        : base(serialNumber, brand, model, customerName, receivedDate, expectedDeliveryDate, faultDescription)
    {
        _loadCapacityKg = InputValidator.RequirePositive(loadCapacityKg, nameof(loadCapacityKg));
        _isFrontLoading = isFrontLoading;
    }

    public override string ApplianceType => "Washing Machine";

    public int LoadCapacityKg => _loadCapacityKg;

    public bool IsFrontLoading => _isFrontLoading;

    public bool DrumBearingReplaced
    {
        get => _drumBearingReplaced;
        set
        {
            EnsureEditable();
            _drumBearingReplaced = value;
        }
    }

    public override decimal CalculateRepairCost()
    {
        decimal cost = CalculateBaseCost();

        if (_isFrontLoading)
        {
            cost += FrontLoadingServiceFee;
        }

        if (_drumBearingReplaced)
        {
            cost += DrumBearingReplacementFee;
        }

        return cost;
    }

    public override string GetStatusSummary()
    {
        return base.GetStatusSummary()
            + " | Load: " + _loadCapacityKg + " kg"
            + " | Front loading: " + YesNo(_isFrontLoading)
            + " | Drum bearing replaced: " + YesNo(_drumBearingReplaced);
    }
}
