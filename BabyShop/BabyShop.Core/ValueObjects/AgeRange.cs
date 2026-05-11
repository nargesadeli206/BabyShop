using BabyShop.Core.Exceptions;

namespace BabyShop.Core.ValueObjects;

public class AgeRange
{
    public string Code { get; }
    public string DisplayName { get; }
    public int MinMonth { get; }
    public int? MaxMonth { get; } // ? یعنی می‌تونه null باشه (برای 24+ که بیشینه نداره)

    private AgeRange(string code, string displayName, int minMonth, int? maxMonth = null)
    {
        Code = code;
        DisplayName = displayName;
        MinMonth = minMonth;
        MaxMonth = maxMonth;
    }

    // مقادیر ثابت - همه رده‌های سنی
    public static AgeRange ZeroToThree => new("0-3", "۰ تا ۳ ماه", 0, 3);
    public static AgeRange ThreeToSix => new("3-6", "۳ تا ۶ ماه", 3, 6);
    public static AgeRange SixToTwelve => new("6-12", "۶ تا ۱۲ ماه", 6, 12);
    public static AgeRange TwelveToTwentyFour => new("12-24", "۱۲ تا ۲۴ ماه", 12, 24);
    public static AgeRange TwentyFourPlus => new("24+", "بالای ۲ سال", 24, null);

    
    public static AgeRange FromCode(string code)
    {
        return code?.Trim() switch
        {
            "0-3" => ZeroToThree,
            "3-6" => ThreeToSix,
            "6-12" => SixToTwelve,
            "12-24" => TwelveToTwentyFour,
            "24+" => TwentyFourPlus,
            _ => throw new DomainException("کد رده سنی نامعتبر است")
        };
    }

    // لیست همه رده‌های سنی (برای Dropdown)
    public static IEnumerable<AgeRange> GetAll()
    {
        yield return ZeroToThree;
        yield return ThreeToSix;
        yield return SixToTwelve;
        yield return TwelveToTwentyFour;
        yield return TwentyFourPlus;
    }

    // بررسی اینکه یک سن (بر حسب ماه) در این رده قرار می‌گیره یا نه
    public bool IsInRange(int ageInMonths)
    {
        if (ageInMonths < MinMonth) return false;
        if (MaxMonth.HasValue && ageInMonths > MaxMonth.Value) return false;
        return true;
    }

    // برای مقایسه دو رده سنی
    public override bool Equals(object? obj)
    {
        if (obj is not AgeRange other) return false;
        return Code == other.Code;
    }

    public override int GetHashCode() => Code.GetHashCode();

    // برای نمایش درست در خروجی
    public override string ToString() => DisplayName;
}