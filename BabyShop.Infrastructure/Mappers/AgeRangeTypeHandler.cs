using BabyShop.Core.ValueObjects;
using Dapper;
using System.Data;

namespace BabyShop.Infrastructure.Mappers;

public class AgeRangeTypeHandler : SqlMapper.TypeHandler<AgeRange>
{
    public override void SetValue(IDbDataParameter parameter, AgeRange? value)
    {
        parameter.DbType = DbType.String;
        parameter.Value = (object?)value?.Code ?? DBNull.Value;
    }

    public override AgeRange? Parse(object value)
    {
        if (value == null || value is DBNull)
            return null;

        try
        {
            return AgeRange.FromCode(value.ToString());
        }
        catch
        {
            return AgeRange.Baby;
        }
    }
}