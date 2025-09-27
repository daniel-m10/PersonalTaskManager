using Dapper;
using System.Data;

namespace TaskManager.Data.Handlers
{
    public class GuidTypeHandler : SqlMapper.TypeHandler<Guid>
    {
        public override void SetValue(IDbDataParameter parameter, Guid value)
            => parameter.Value = value.ToString();

        public override Guid Parse(object value)
            => Guid.Parse(value.ToString()!);
    }
}
