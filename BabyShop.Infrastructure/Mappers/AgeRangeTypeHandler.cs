using BabyShop.Core.ValueObjects;
using Dapper;
using System.Data;

namespace BabyShop.Infrastructure.Mappers;

public class AgeRangeTypeHandler : SqlMapper.TypeHandler<AgeRange>
{
    public override void SetValue(IDbDataParameter parameter, AgeRange value)
    {
        parameter.Value = value?.Code;
        parameter.DbType = DbType.String;
    }

    public override AgeRange Parse(object value)
    {
        if (value == null || value is DBNull)
            return null;

        return new AgeRange(value.ToString());
    }
}