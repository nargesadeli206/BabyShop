namespace BabyShop.Core.ValueObjects;

public class AgeRange : IEquatable<AgeRange>
{
    public string Code { get; }
    public string DisplayName { get; }
    public int MinMonths { get; }
    public int MaxMonths { get; }

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

    public AgeRange(string code)
    {
        var ageRange = FromCode(code);
        Code = ageRange.Code;
        DisplayName = ageRange.DisplayName;
        MinMonths = ageRange.MinMonths;
        MaxMonths = ageRange.MaxMonths;
    }

    public static AgeRange FromCode(string? code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("AgeRange is empty");

        var c = code.Trim()
            .Replace(" ", "")
            .Replace("تا", "-")
            .Replace("ماه", "")
            .Replace("–", "-")
            .Replace("—", "-");

        return c switch
        {
            "0-3" => Newborn,
            "3-6" => Infant,
            "6-12" => Baby,
            "12-24" => Toddler,
            "24+" or "24" => Child,
            _ => throw new ArgumentException(
                $"Invalid age range code: {code}. Use: 0-3, 3-6, 6-12, 12-24, 24+")
        };
    }

    public static IEnumerable<AgeRange> GetAll() =>
        new[] { Newborn, Infant, Baby, Toddler, Child };

    public bool Equals(AgeRange? other) => other is not null && Code == other.Code;
    public override bool Equals(object? obj) => obj is AgeRange a && Equals(a);
    public override int GetHashCode() => Code.GetHashCode();
    public static bool operator ==(AgeRange? left, AgeRange? right) => Equals(left, right);
    public static bool operator !=(AgeRange? left, AgeRange? right) => !Equals(left, right);
    public static implicit operator string(AgeRange ageRange) => ageRange.Code;
    public override string ToString() => Code;
}