using BabyShop.Core.ValueObjects;
using Dapper;
using System.Data;

namespace BabyShop.Infrastructure.Mappers;

public class GenderTypeHandler : SqlMapper.TypeHandler<Gender>
{
    public override void SetValue(IDbDataParameter parameter, Gender value)
    {
        parameter.Value = value.ToString();
        parameter.DbType = DbType.String;
    }

    public override Gender Parse(object value)
    {
        if (value == null || value is DBNull)
            return null;

        return new Gender(value.ToString());
    }
}