using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.Specification.Data
{
    //https://gunnarpeipman.com/ef-core-dbcontext-repository/
    //UOW = Repository Container
    public interface IDbContext
    {
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
