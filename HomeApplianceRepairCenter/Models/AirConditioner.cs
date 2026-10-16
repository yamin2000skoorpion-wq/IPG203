using System;
using HomeApplianceRepairCenter.Utilities;

namespace HomeApplianceRepairCenter.Models;

/// <summary>
/// مكيف: تتأثر التكلفة بكمية غاز التبريد المضافة ونوع المحرك (Inverter) وسعة الوحدة.
/// </summary>
public class AirConditioner : Appliance
{
    private const decimal GasPricePerKg = 30m;
    private const decimal InverterBoardServiceFee = 40m;
    private const decimal LargeUnitSurcharge = 25m;
    private const int LargeUnitThresholdBtu = 24000;

    private readonly int _coolingCapacityBtu;
    private readonly bool _isInverter;
    private decimal _gasRefillKg;

    public AirConditioner(
        string serialNumber,
        string brand,
        string model,
        string customerName,
        DateTime receivedDate,
        DateTime expectedDeliveryDate,
        string faultDescription,
        int coolingCapacityBtu,
        bool isInverter)
        : base(serialNumber, brand, model, customerName, receivedDate, expectedDeliveryDate, faultDescription)
    {
        _coolingCapacityBtu = InputValidator.RequirePositive(coolingCapacityBtu, nameof(coolingCapacityBtu));
        _isInverter = isInverter;
    }

    public override string ApplianceType => "Air Conditioner";

    public int CoolingCapacityBtu => _coolingCapacityBtu;

    public bool IsInverter => _isInverter;

    /// <summary>كمية غاز التبريد المضافة بالكيلوغرام.</summary>
    public decimal GasRefillKg
    {
        get => _gasRefillKg;
        set
        {
            EnsureEditable();
            _gasRefillKg = InputValidator.RequireNonNegative(value, nameof(GasRefillKg));
        }
    }

    public override decimal CalculateRepairCost()
    {
        decimal cost = CalculateBaseCost() + (_gasRefillKg * GasPricePerKg);

        if (_isInverter)
        {
            cost += InverterBoardServiceFee;
        }

        if (_coolingCapacityBtu >= LargeUnitThresholdBtu)
        {
            cost += LargeUnitSurcharge;
        }

        return cost;
    }

    public override string GetStatusSummary()
    {
        return base.GetStatusSummary()
            + " | Capacity: " + _coolingCapacityBtu + " BTU"
            + " | Inverter: " + YesNo(_isInverter)
            + " | Gas refill: " + _gasRefillKg + " kg";
    }
}
