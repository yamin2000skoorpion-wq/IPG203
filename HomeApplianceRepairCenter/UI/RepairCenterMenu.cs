using System;
using System.Collections.Generic;
using System.Linq;
using HomeApplianceRepairCenter.Enums;
using HomeApplianceRepairCenter.Interfaces;
using HomeApplianceRepairCenter.Models;
using HomeApplianceRepairCenter.Services;
using HomeApplianceRepairCenter.Utilities;

namespace HomeApplianceRepairCenter.UI;

/// <summary>
/// القائمة التفاعلية: تقرأ بيانات المستخدم وتنفذ العمليات على RepairCenter.
/// لا تحتوي على منطق العمل؛ كل القواعد (التحقق، دورة الحياة، التكلفة) داخل الفئات الأخرى.
/// </summary>
public class RepairCenterMenu
{
    private readonly RepairCenter _center;

    public RepairCenterMenu(RepairCenter center)
    {
        _center = center ?? throw new ArgumentNullException(nameof(center));
    }

    /// <summary>حلقة القائمة الرئيسية: تستمر حتى يختار المستخدم الخروج.</summary>
    public void Run()
    {
        bool running = true;

        while (running)
        {
            PrintMenu();

            try
            {
                int choice = ConsoleInput.ReadMenuChoice("Choose an option: ", 0, 12);
                running = Execute(choice);
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("  (cancelled)");
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine("  [REJECTED] " + ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine("  [REJECTED] " + ex.Message);
            }
        }

        Console.WriteLine();
        Console.WriteLine("Goodbye.");
    }

    // ---------- القائمة ----------

    private void PrintMenu()
    {
        Console.WriteLine();
        Console.WriteLine("==================== " + _center.Name + " ====================");
        Console.WriteLine("  1) Register a new appliance");
        Console.WriteLine("  2) Start repair");
        Console.WriteLine("  3) Update repair details (labor, parts, ...)");
        Console.WriteLine("  4) Complete repair");
        Console.WriteLine("  5) Deliver appliance to the customer");
        Console.WriteLine("  6) List all appliances");
        Console.WriteLine("  7) Show appliances by status (under repair / repaired / ...)");
        Console.WriteLine("  8) Check delayed deliveries");
        Console.WriteLine("  9) Costs and statistics");
        Console.WriteLine(" 10) Notification log");
        Console.WriteLine(" 11) Load sample appliances");
        Console.WriteLine(" 12) Run validation and encapsulation self-check");
        Console.WriteLine("  0) Exit");
        Console.WriteLine("  (type 'cancel' at any prompt to abort the current operation)");
    }

    private bool Execute(int choice)
    {
        switch (choice)
        {
            case 1:
                RegisterAppliance();
                break;
            case 2:
                StartRepair();
                break;
            case 3:
                UpdateRepairDetails();
                break;
            case 4:
                CompleteRepair();
                break;
            case 5:
                DeliverAppliance();
                break;
            case 6:
                ShowAllAppliances();
                break;
            case 7:
                ShowAppliancesByStatus();
                break;
            case 8:
                CheckDelays();
                break;
            case 9:
                ShowCostsAndStatistics();
                break;
            case 10:
                ShowNotificationLog();
                break;
            case 11:
                LoadSampleAppliances();
                break;
            case 12:
                RunSelfCheck();
                break;
            default:
                return false;
        }

        return true;
    }

    // ---------- 1) تسجيل جهاز جديد ----------

    private void RegisterAppliance()
    {
        PrintSection("Register a new appliance");
        Console.WriteLine("  1) Refrigerator");
        Console.WriteLine("  2) Washing Machine");
        Console.WriteLine("  3) Air Conditioner");
        int type = ConsoleInput.ReadMenuChoice("  Appliance type: ", 1, 3);

        string serial = ConsoleInput.ReadSerialNumber(s => _center.FindBySerial(s) is not null);
        string brand = ConsoleInput.ReadText("  Brand: ", 50);
        string model = ConsoleInput.ReadText("  Model: ", 50);
        string customer = ConsoleInput.ReadPersonName("  Customer name: ");

        DateTime received = ConsoleInput.ReadDate(
            "  Received date",
            DateTime.Today,
            d => InputValidator.IsValidReceivedDate(d),
            "Received date must be between 2000-01-01 and today.");

        DateTime expected = ConsoleInput.ReadDate(
            "  Expected delivery date",
            received.AddDays(7),
            d => InputValidator.IsValidDeliveryDate(received, d),
            "Expected delivery date cannot be earlier than the received date.");

        string fault = ConsoleInput.ReadText("  Fault description: ", 200);

        Appliance appliance;

        if (type == 1)
        {
            int liters = ConsoleInput.ReadPositiveInt("  Capacity (liters): ");
            bool hasFreezer = ConsoleInput.ReadYesNo("  Has freezer? (y/n): ");
            appliance = new Refrigerator(serial, brand, model, customer, received, expected, fault, liters, hasFreezer);
        }
        else if (type == 2)
        {
            int loadKg = ConsoleInput.ReadPositiveInt("  Load capacity (kg): ");
            bool isFront = ConsoleInput.ReadYesNo("  Front loading? (y/n): ");
            appliance = new WashingMachine(serial, brand, model, customer, received, expected, fault, loadKg, isFront);
        }
        else
        {
            int btu = ConsoleInput.ReadPositiveInt("  Cooling capacity (BTU): ");
            bool isInverter = ConsoleInput.ReadYesNo("  Inverter motor? (y/n): ");
            appliance = new AirConditioner(serial, brand, model, customer, received, expected, fault, btu, isInverter);
        }

        _center.Register(appliance);
        Console.WriteLine("  Registered: " + appliance.GetStatusSummary());
    }

    // ---------- 2) بدء الإصلاح ----------

    private void StartRepair()
    {
        PrintSection("Start repair");
        Appliance? appliance = SelectAppliance(
            "Appliances waiting for repair:",
            a => a.Status == RepairStatus.Received,
            "No appliance is waiting for repair.");

        appliance?.StartRepair();
    }

    // ---------- 3) تعديل بيانات الإصلاح ----------

    private void UpdateRepairDetails()
    {
        PrintSection("Update repair details");
        Appliance? appliance = SelectAppliance(
            "Appliances whose repair details can still be edited (repaired appliances are locked):",
            a => a.Status == RepairStatus.Received || a.Status == RepairStatus.InRepair,
            "No editable appliance. Repaired and delivered appliances can no longer be modified.");

        if (appliance is null)
        {
            return;
        }

        Console.WriteLine("  Press Enter at a prompt to keep the current value.");

        decimal labor = ConsoleInput.ReadOptionalDecimal(
            "  Labor hours (current " + appliance.LaborHours + "): ", appliance.LaborHours);
        decimal parts = ConsoleInput.ReadOptionalDecimal(
            "  Parts cost in USD (current " + appliance.PartsCost + "): ", appliance.PartsCost);
        string fault = ConsoleInput.ReadOptionalText(
            "  Fault description (Enter = keep): ", appliance.FaultDescription, 200);

        // الأسئلة الخاصة بنوع الجهاز
        bool compressorReplaced = false;
        bool drumBearingReplaced = false;
        decimal gasRefillKg = 0m;

        if (appliance is Refrigerator fridge)
        {
            compressorReplaced = ConsoleInput.ReadOptionalYesNo(
                "  Compressor replaced? (y/n, current " + YesNo(fridge.CompressorReplaced) + "): ",
                fridge.CompressorReplaced);
        }
        else if (appliance is WashingMachine washer)
        {
            drumBearingReplaced = ConsoleInput.ReadOptionalYesNo(
                "  Drum bearing replaced? (y/n, current " + YesNo(washer.DrumBearingReplaced) + "): ",
                washer.DrumBearingReplaced);
        }
        else if (appliance is AirConditioner conditioner)
        {
            gasRefillKg = ConsoleInput.ReadOptionalDecimal(
                "  Gas refill in kg (current " + conditioner.GasRefillKg + "): ",
                conditioner.GasRefillKg);
        }

        // تطبيق القيم بعد جمعها كلها؛ المعيّنات (setters) تتحقق من الصحة وتمنع التعديل بعد الإصلاح
        appliance.LaborHours = labor;
        appliance.PartsCost = parts;
        appliance.FaultDescription = fault;

        if (appliance is Refrigerator fridgeToUpdate)
        {
            fridgeToUpdate.CompressorReplaced = compressorReplaced;
        }
        else if (appliance is WashingMachine washerToUpdate)
        {
            washerToUpdate.DrumBearingReplaced = drumBearingReplaced;
        }
        else if (appliance is AirConditioner conditionerToUpdate)
        {
            conditionerToUpdate.GasRefillKg = gasRefillKg;
        }

        Console.WriteLine("  Updated. Current cost: " + MoneyFormatter.Format(appliance.CalculateRepairCost()));
    }

    // ---------- 4) إكمال الإصلاح ----------

    private void CompleteRepair()
    {
        PrintSection("Complete repair");
        Appliance? appliance = SelectAppliance(
            "Appliances currently under repair:",
            a => a.Status == RepairStatus.InRepair,
            "No appliance is under repair.");

        appliance?.CompleteRepair();
    }

    // ---------- 5) التسليم ----------

    private void DeliverAppliance()
    {
        PrintSection("Deliver appliance");
        Appliance? appliance = SelectAppliance(
            "Repaired appliances awaiting delivery:",
            a => a.Status == RepairStatus.Repaired,
            "No repaired appliance is waiting for delivery.");

        appliance?.Deliver();
    }

    // ---------- 6) و 7) التقارير ----------

    private void ShowAllAppliances()
    {
        PrintSection("All appliances");

        if (_center.Appliances.Count == 0)
        {
            Console.WriteLine("  No appliances registered yet.");
            return;
        }

        foreach (string line in _center.GetStatusReport())
        {
            Console.WriteLine("  " + line);
        }
    }

    private void ShowAppliancesByStatus()
    {
        PrintSection("Appliances by status");
        Console.WriteLine("  1) Received (waiting)");
        Console.WriteLine("  2) Under repair");
        Console.WriteLine("  3) Repaired (awaiting delivery)");
        Console.WriteLine("  4) Delivered");
        int choice = ConsoleInput.ReadMenuChoice("  Status: ", 1, 4);
        RepairStatus status = (RepairStatus)(choice - 1);

        IReadOnlyList<Appliance> matches = _center.GetByStatus(status);

        if (matches.Count == 0)
        {
            Console.WriteLine("  No appliances with status " + status + ".");
            return;
        }

        foreach (Appliance appliance in matches)
        {
            Console.WriteLine("  " + appliance.GetStatusSummary());
        }

        Console.WriteLine("  Count: " + matches.Count
            + " | Total cost: " + MoneyFormatter.Format(_center.CalculateTotalCostByStatus(status)));
    }

    // ---------- 8) فحص التأخير ----------

    private void CheckDelays()
    {
        PrintSection("Check delayed deliveries");
        DateTime date = ConsoleInput.ReadDate(
            "  Check date",
            DateTime.Today,
            d => d.Date >= new DateTime(2000, 1, 1),
            "Date must be 2000-01-01 or later.");

        int raised = _center.CheckDeliveryDelays(date);
        Console.WriteLine("  Overdue notifications raised: " + raised);
    }

    // ---------- 9) التكاليف والعدّادات الثابتة ----------

    private void ShowCostsAndStatistics()
    {
        PrintSection("Cost per appliance (through the IRepairable interface)");

        if (_center.Appliances.Count == 0)
        {
            Console.WriteLine("  No appliances registered yet.");
        }

        foreach (IRepairable item in _center.Appliances)
        {
            Console.WriteLine("  " + item.SerialNumber + " -> " + MoneyFormatter.Format(item.CalculateRepairCost()));
        }

        PrintSection("Totals and static statistics");
        Console.WriteLine("  Total cost (all appliances): " + MoneyFormatter.Format(_center.CalculateTotalCost()));
        Console.WriteLine("  Total cost (repaired)      : " + MoneyFormatter.Format(_center.CalculateTotalCostByStatus(RepairStatus.Repaired)));
        Console.WriteLine("  Total cost (delivered)     : " + MoneyFormatter.Format(_center.CalculateTotalCostByStatus(RepairStatus.Delivered)));
        Console.WriteLine("  Registered appliances      : " + RepairStatistics.TotalRegistered);
        Console.WriteLine("  Received count             : " + RepairStatistics.ReceivedCount);
        Console.WriteLine("  Under repair count         : " + RepairStatistics.UnderRepairCount);
        Console.WriteLine("  Repaired (awaiting pickup) : " + RepairStatistics.RepairedCount);
        Console.WriteLine("  Delivered count            : " + RepairStatistics.DeliveredCount);
    }

    // ---------- 10) سجل الإشعارات ----------

    private void ShowNotificationLog()
    {
        PrintSection("Notification log");

        if (_center.NotificationLog.Count == 0)
        {
            Console.WriteLine("  No notifications yet.");
            return;
        }

        foreach (string entry in _center.NotificationLog)
        {
            Console.WriteLine("  " + entry);
        }
    }

    // ---------- 11) بيانات تجريبية ----------

    private void LoadSampleAppliances()
    {
        PrintSection("Load sample appliances");
        DateTime today = DateTime.Today;

        List<Appliance> samples = new List<Appliance>
        {
            new Refrigerator("RF-100234", "Samsung", "RT38", "Ahmad Khaled",
                today.AddDays(-5), today.AddDays(2), "Not cooling", 380, true),
            new Refrigerator("RF-100235", "LG", "GN-B422", "Lina Haddad",
                today.AddDays(-12), today.AddDays(-4), "Loud compressor noise", 420, true),
            new WashingMachine("WM-200101", "Bosch", "WAN2820", "Omar Nasser",
                today.AddDays(-3), today.AddDays(4), "Drum does not spin", 8, true),
            new WashingMachine("WM-200102", "Ariston", "AQ80", "Rana Youssef",
                today.AddDays(-8), today.AddDays(-1), "Water leakage", 7, false),
            new AirConditioner("AC-300501", "Gree", "GWH18", "Sami Darwish",
                today.AddDays(-2), today.AddDays(5), "Refrigerant leak", 18000, true),
            new AirConditioner("AC-300502", "Carrier", "42QH24", "Maya Saleh",
                today.AddDays(-6), today.AddDays(1), "Weak cooling", 24000, false)
        };

        foreach (Appliance sample in samples)
        {
            try
            {
                _center.Register(sample);
                Console.WriteLine("  Registered " + sample.SerialNumber);
            }
            catch (InvalidOperationException)
            {
                Console.WriteLine("  Skipped " + sample.SerialNumber + " (already registered).");
            }
        }
    }

    // ---------- 12) فحص ذاتي للتحقق والتغليف ----------

    private void RunSelfCheck()
    {
        PrintSection("Validation and encapsulation self-check");
        DateTime today = DateTime.Today;

        TryAction(
            "Invalid serial number '123'",
            () => new Refrigerator("123", "LG", "X1", "Test Customer", today, today, "Fault", 300, true));

        TryAction(
            "Delivery date earlier than received date",
            () => new AirConditioner("AC-400001", "LG", "X2", "Test Customer", today, today.AddDays(-1), "Fault", 12000, false));

        // أجهزة تجريبية غير مسجلة في المركز، فلا تتأثر العدّادات الثابتة
        Refrigerator probe = new Refrigerator("TS-000001", "LG", "X3", "Test Customer", today, today, "Fault", 300, true);
        TryAction(
            "Negative parts cost",
            () => probe.PartsCost = -5m);

        probe.StartRepair();
        probe.CompleteRepair();
        TryAction(
            "Modifying parts cost after the repair is completed",
            () => probe.PartsCost = 999m);

        WashingMachine freshProbe = new WashingMachine("TS-000002", "LG", "X4", "Test Customer", today, today, "Fault", 6, false);
        TryAction(
            "Delivering an appliance that is not repaired yet",
            () => freshProbe.Deliver());

        if (_center.Appliances.Count > 0)
        {
            string existingSerial = _center.Appliances[0].SerialNumber;
            TryAction(
                "Duplicate serial number " + existingSerial,
                () => _center.Register(new WashingMachine(existingSerial, "LG", "X5", "Test Customer", today, today, "Fault", 6, false)));
        }

        // محاولة تعديل الرقم التسلسلي لا تُترجم أصلًا لأنه للقراءة فقط:
        // probe.SerialNumber = "RF-999999";   // خطأ ترجمة (Compile-time error)
        Console.WriteLine("  [INFO] SerialNumber has no setter: changing it is a compile-time error.");
    }

    // ---------- دوال مساعدة خاصة ----------

    /// <summary>
    /// يعرض الأجهزة المطابقة للشرط ويطلب رقمًا تسلسليًا منها؛ يعيد null إذا لم يوجد أي جهاز مناسب.
    /// </summary>
    private Appliance? SelectAppliance(string title, Func<Appliance, bool> filter, string emptyMessage)
    {
        List<Appliance> candidates = _center.Appliances.Where(filter).ToList();

        if (candidates.Count == 0)
        {
            Console.WriteLine("  " + emptyMessage);
            return null;
        }

        Console.WriteLine("  " + title);

        foreach (Appliance candidate in candidates)
        {
            Console.WriteLine("    " + candidate.SerialNumber
                + " | " + candidate.ApplianceType
                + " | " + candidate.CustomerName
                + " | " + candidate.Status);
        }

        while (true)
        {
            string serial = ConsoleInput.ReadLine("  Serial number: ");
            Appliance? found = _center.FindBySerial(serial);

            if (found is not null && filter(found))
            {
                return found;
            }

            ConsoleInput.PrintError("No appliance in the list above has this serial number.");
        }
    }

    private static string YesNo(bool value)
    {
        return value ? "Yes" : "No";
    }

    private static void PrintSection(string title)
    {
        Console.WriteLine();
        Console.WriteLine("=== " + title + " ===");
    }

    private static void TryAction(string description, Action action)
    {
        try
        {
            action();
            Console.WriteLine("  [UNEXPECTED] " + description + " was accepted.");
        }
        catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
        {
            Console.WriteLine("  [REJECTED] " + description + " -> " + ex.GetType().Name);
        }
    }
}
