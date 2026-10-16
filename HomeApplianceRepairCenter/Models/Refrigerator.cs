using System;
using HomeApplianceRepairCenter.Utilities;

namespace HomeApplianceRepairCenter.Models;

/// <summary>
/// ثلاجة: تتأثر التكلفة بوجود الفريزر واستبدال الضاغط (الكمبروسور).
/// </summary>
public class Refrigerator : Appliance
{
    private const decimal FreezerServiceFee = 15m;
    private const decimal CompressorReplacementFee = 120m;

    private readonly int _capacityLiters;
    private readonly bool _hasFreezer;
    private bool _compressorReplaced;

    public Refrigerator(
        string serialNumber,
        string brand,
        string model,
        string customerName,
        DateTime receivedDate,
        DateTime expectedDeliveryDate,
        string faultDescription,
        int capacityLiters,
        bool hasFreezer)
        : base(serialNumber, brand, model, customerName, receivedDate, expectedDeliveryDate, faultDescription)
    {
        _capacityLiters = InputValidator.RequirePositive(capacityLiters, nameof(capacityLiters));
        _hasFreezer = hasFreezer;
    }

    public override string ApplianceType => "Refrigerator";

    public int CapacityLiters => _capacityLiters;

    public bool HasFreezer => _hasFreezer;

    public bool CompressorReplaced
    {
        get => _compressorReplaced;
        set
        {
            EnsureEditable();
            _compressorReplaced = value;
        }
    }

    public override decimal CalculateRepairCost()
    {
        decimal cost = CalculateBaseCost();

        if (_hasFreezer)
        {
            cost += FreezerServiceFee;
        }

        if (_compressorReplaced)
        {
            cost += CompressorReplacementFee;
        }

        return cost;
    }

    public override string GetStatusSummary()
    {
        return base.GetStatusSummary()
            + " | Capacity: " + _capacityLiters + " L"
            + " | Freezer: " + YesNo(_hasFreezer)
            + " | Compressor replaced: " + YesNo(_compressorReplaced);
    }
}
