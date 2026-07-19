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

    /// <summary>
    /// از مقدار DB: 1/2/3 یا Male/Female یا پسرانه/دخترانه
    /// </summary>
    public Gender(string? raw)
    {
        var g = FromAny(raw);
        Value = g.Value;
        DisplayName = g.DisplayName;
    }

    public static readonly Gender Boy = new Gender(1, "پسرانه");
    public static readonly Gender Girl = new Gender(2, "دخترانه");
    public static readonly Gender Unisex = new Gender(3, "یونیسکس");

    public static Gender FromValue(int value) => value switch
    {
        1 => Boy,
        2 => Girl,
        3 => Unisex,
        _ => throw new ArgumentException($"Invalid gender value: {value}")
    };

    public static Gender FromName(string displayName) => FromAny(displayName);

    public static Gender FromAny(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            throw new ArgumentException("Gender value is empty");

        var s = raw.Trim();

        if (int.TryParse(s, out var n))
            return FromValue(n);

        return s.ToLowerInvariant() switch
        {
            "male" or "boy" or "m" or "پسرانه" or "پسر" => Boy,
            "female" or "girl" or "f" or "دخترانه" or "دختر" => Girl,
            "unisex" or "u" or "یونیسکس" or "هر دو" => Unisex,
            _ => throw new ArgumentException($"Invalid gender: {raw}")
        };
    }

    /// <summary>مقادیری که ممکن است در ستون Gender دیتابیس باشد.</summary>
    public IReadOnlyList<string> DbValues => Value switch
    {
        1 => new[] { "1", "Male", "male", "Boy", "پسرانه" },
        2 => new[] { "2", "Female", "female", "Girl", "دخترانه" },
        3 => new[] { "3", "Unisex", "unisex", "یونیسکس" },
        _ => new[] { DisplayName }
    };

    public static IEnumerable<Gender> GetAll() => new[] { Boy, Girl, Unisex };

    public bool Equals(Gender? other) => other is not null && Value == other.Value;
    public override bool Equals(object? obj) => Equals(obj as Gender);
    public override int GetHashCode() => Value.GetHashCode();
    public override string ToString() => DisplayName;

    public static bool operator ==(Gender? left, Gender? right) => Equals(left, right);
    public static bool operator !=(Gender? left, Gender? right) => !Equals(left, right);
}