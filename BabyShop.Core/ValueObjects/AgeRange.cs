namespace BabyShop.Core.ValueObjects;

public class AgeRange : IEquatable<AgeRange>
{
    public string Code { get; }
    public string DisplayName { get; }
    public int MinMonths { get; }
    public int MaxMonths { get; }

    // مقادیر ثابت
    public static readonly AgeRange Newborn = new AgeRange("0-3", "0 تا 3 ماه", 0, 3);
    public static readonly AgeRange Infant = new AgeRange("3-6", "3 تا 6 ماه", 3, 6);
    public static readonly AgeRange Baby = new AgeRange("6-12", "6 تا 12 ماه", 6, 12);
    public static readonly AgeRange Toddler = new AgeRange("12-24", "12 تا 24 ماه", 12, 24);
    public static readonly AgeRange Child = new AgeRange("24+", "بالای 24 ماه", 24, int.MaxValue);

    private AgeRange(string code, string displayName, int minMonths, int maxMonths)
    {
        Code = code;
        DisplayName = displayName;
        MinMonths = minMonths;
        MaxMonths = maxMonths;
    }

    // سازنده از روی کد (برای EF Core)
    public AgeRange(string code)
    {
        var ageRange = FromCode(code);
        Code = ageRange.Code;
        DisplayName = ageRange.DisplayName;
        MinMonths = ageRange.MinMonths;
        MaxMonths = ageRange.MaxMonths;
    }

    // سازنده از روی نام (برای تبدیل از string)
    public AgeRange FromDisplayName(string displayName)
    {
        return displayName switch
        {
            "0 تا 3 ماه" => Newborn,
            "3 تا 6 ماه" => Infant,
            "6 تا 12 ماه" => Baby,
            "12 تا 24 ماه" => Toddler,
            "بالای 24 ماه" => Child,
            _ => throw new ArgumentException($"Invalid age range name: {displayName}")
        };
    }

    // تبدیل کد به آبجکت AgeRange
    public static AgeRange FromCode(string code)
    {
        return code switch
        {
            "0-3" => Newborn,
            "3-6" => Infant,
            "6-12" => Baby,
            "12-24" => Toddler,
            "24+" => Child,
            _ => throw new ArgumentException($"Invalid age range code: {code}")
        };
    }

    // گرفتن لیست همه رده‌های سنی
    public static IEnumerable<AgeRange> GetAll() => new[] { Newborn, Infant, Baby, Toddler, Child };

    public bool Equals(AgeRange? other)
    {
        if (other is null) return false;
        return Code == other.Code;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((AgeRange)obj);
    }

    public override int GetHashCode() => Code.GetHashCode();

    public static bool operator ==(AgeRange left, AgeRange right) => Equals(left, right);
    public static bool operator !=(AgeRange left, AgeRange right) => !Equals(left, right);

    // تبدیل ضمنی به string
    public static implicit operator string(AgeRange ageRange) => ageRange.Code;
}