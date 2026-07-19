using BabyShop.Core.ValueObjects;
using Dapper;
using System.Data;

namespace BabyShop.Infrastructure.Mappers;

public class GenderTypeHandler : SqlMapper.TypeHandler<Gender>
{
    public override void SetValue(IDbDataParameter parameter, Gender? value)
    {
        parameter.DbType = DbType.String;
        parameter.Value = value != null ? value.Value.ToString() : DBNull.Value;
    }

    public override Gender? Parse(object value)
    {
        if (value == null || value is DBNull)
            return null;

        try
        {
            return Gender.FromAny(value.ToString());
        }
        catch
        {
            // مقدار ناشناخته در DB کل لیست را hang نکند
            return Gender.Unisex;
        }
    }
}