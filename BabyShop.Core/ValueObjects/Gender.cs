namespace BabyShop.Core.ValueObjects;

public class Gender : IEquatable<Gender>
{
    public int Value { get; }
    public string DisplayName { get; }

    private Gender(int value, string displayName)
    {
        Value = value;
        DisplayName = displayName;
    }

    public Gender(string? v)
    {
        this.v = v;
    }

    public static readonly Gender Boy = new Gender(1, "پسرانه");
    public static readonly Gender Girl = new Gender(2, "دخترانه");
    public static readonly Gender Unisex = new Gender(3, "یونیسکس");
    private string? v;

    public static Gender FromValue(int value)
    {
        return value switch
        {
            1 => Boy,
            2 => Girl,
            3 => Unisex,
            _ => throw new ArgumentException($"Invalid gender value: {value}")
        };
    }

    public static Gender FromName(string displayName)
    {
        return displayName switch
        {
            "پسرانه" => Boy,
            "دخترانه" => Girl,
            "یونیسکس" => Unisex,
            _ => throw new ArgumentException($"Invalid gender name: {displayName}")
        };
    }

    public static IEnumerable<Gender> GetAll() => new[] { Boy, Girl, Unisex };

    public bool Equals(Gender? other)
    {
        if (other is null) return false;
        return Value == other.Value;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as Gender);
    }

    public override int GetHashCode() => Value.GetHashCode();

    public override string ToString() => DisplayName;

    public static bool operator ==(Gender? left, Gender? right) => Equals(left, right);
    public static bool operator !=(Gender? left, Gender? right) => !Equals(left, right);
}