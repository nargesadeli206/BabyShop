namespace BabyShop.Core.Exceptions;

/// <summary>
/// استثنای عدم وجود - وقتی موجودیتی پیدا نمی‌شود
/// </summary>
public sealed class NotFoundException : Exception
{
    public string EntityName { get; }
    public object EntityId { get; }

    public NotFoundException(string entityName, object entityId)
        : base($"{entityName} with id '{entityId}' was not found.")
    {
        EntityName = entityName;
        EntityId = entityId;
    }

    public NotFoundException(string entityName, object entityId, string message)
        : base(message)
    {
        EntityName = entityName;
        EntityId = entityId;
    }
}