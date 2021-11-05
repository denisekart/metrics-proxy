using System.Threading;
using System.Threading.Tasks;

namespace MetricsProxy.Application.Contracts
{
    public interface IMetricsManagementService
    {
        Task QueryAndReport(CancellationToken cancellationToken);
    }
}