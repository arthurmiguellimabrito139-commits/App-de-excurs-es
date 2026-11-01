using System.Data;
using Dapper;

namespace Excursoes.Api.Data;

// Ensina o Dapper a converter DateOnly (C#) <-> DATE (MySQL)
public class DateOnlyTypeHandler : SqlMapper.TypeHandler<DateOnly>
{
    // C# -> banco
    public override void SetValue(IDbDataParameter parametro, DateOnly valor)
    {
        parametro.DbType = DbType.Date;
        parametro.Value = valor.ToDateTime(TimeOnly.MinValue);
    }

    // banco -> C#
    public override DateOnly Parse(object valor)
    {
        if (valor is DateOnly data)
            return data;
        return DateOnly.FromDateTime((DateTime)valor);
    }
}
