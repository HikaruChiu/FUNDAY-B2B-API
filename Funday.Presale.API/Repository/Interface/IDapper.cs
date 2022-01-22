using Dapper;
using System.Data;
using System.Data.Common;

namespace Funday.Presale.API.Repository.Interface
{
    public interface IDapper
    {
        IDbConnection OpenConnection();

        Task<Tuple<IEnumerable<dynamic>, int>> FindWithOffsetFetch(string sql, object parameters, int pageIndex, int pageSize, List<SortDescriptor> sortings);

    }
}
