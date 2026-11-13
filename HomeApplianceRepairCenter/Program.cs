using System;
using System.Globalization;
using System.IO;
using System.Text;
using HomeApplianceRepairCenter.Services;
using HomeApplianceRepairCenter.UI;

namespace HomeApplianceRepairCenter;

/// <summary>
/// نقطة الدخول: تهيئ الشاشة وتنشئ المركز ثم تشغّل القائمة التفاعلية
/// التي يُدخل المستخدم من خلالها بيانات الأجهزة ويتحكم بدورة الصيانة.
/// </summary>
internal static class Program
{
    private static void Main()
    {
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
        Console.OutputEncoding = Encoding.UTF8;
        Console.InputEncoding = Encoding.UTF8;

        RepairCenter center = new RepairCenter("Damascus Home Appliance Repair Center");
        RepairCenterMenu menu = new RepairCenterMenu(center);

        try
        {
            menu.Run();
        }
        catch (EndOfStreamException)
        {
            Console.WriteLine();
            Console.WriteLine("Input closed. Goodbye.");
        }
    }
}
/// <summary>
///  استخدمنا الواجهة IRepairable والفئة المجردة Appliance معاً
 /// لتحقيق التوازن بين العقد (Contract) وإعادة الاستخدام (Code Reuse)
/// الواجهة IRepairable تحدد ماذا يجب أن يقدمه أي شيء قابل للصيانة للنظام دون تقييده بنوع معين في حين جاءت الفئة المجردة Appliance لتجمع البيانات المشتركة ومنطق الأحداث في مكان واحد بدلاً من تكرارها في كل فئة 
/// ولو أردنا مستقبلاً صيانة أشياء لا تنتمي لعائلة الأجهزة المنزلية (مثل السيارات) يكفي أن نطبق IRepairable دون إجبارها 
/// على استخدام خصائص الأجهزة المنزلية
/// </summary>