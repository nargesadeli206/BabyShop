using BabyShop.Core.Exceptions;

namespace BabyShop.Core.ValueObjects;

public class Gender
{
    public int Value { get; }
    public string Name { get; }

    private Gender(int value, string name)
    {
        Value = value;
        Name = name;
    }

    // مقادیر ثابت (استاتیک) - مثل Enum
    public static Gender پسرانه => new(1, "پسرانه");
    public static Gender دخترانه => new(2, "دخترانه");
    public static Gender مشترک => new(3, "مشترک");

    // تبدیل از int به Gender
    public static Gender FromInt(int value)
    {
        return value switch
        {
            1 => پسرانه,
            2 => دخترانه,
            3 => مشترک,
            _ => throw new DomainException("مقدار جنسیت نامعتبر است")
        };
    }

    // تبدیل از string به Gender (برای مواقعی که از string استفاده می‌کنی)
    public static Gender FromString(string genderName)
    {
        return genderName?.Trim() switch
        {
            "پسرانه" => پسرانه,
            "دخترانه" => دخترانه,
            "مشترک" => مشترک,
            _ => throw new DomainException("نام جنسیت نامعتبر است")
        };
    }

    // لیست همه جنسیت‌ها (برای Dropdown)
    public static IEnumerable<Gender> GetAll()
    {
        yield return پسرانه;
        yield return دخترانه;
        yield return مشترک;
    }

    // برای مقایسه دو جنسیت
    public override bool Equals(object? obj)
    {
        if (obj is not Gender other) return false;
        return Value == other.Value;
    }

    public override int GetHashCode() => Value.GetHashCode();

    // برای نمایش درست در خروجی
    public override string ToString() => Name;
}