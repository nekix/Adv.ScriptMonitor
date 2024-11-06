using Adv.ScriptMonitor.Models;

namespace Adv.ScriptMonitor.Repositories;

public interface IDomainStatusRepository
{
    public Task<List<DomainStatus>> GetListAsync(CancellationToken token = default);

    public Task<DomainStatus> InsertAsync(DomainStatus entity, CancellationToken token = default);

    public Task<DomainStatus> UpdateAsync(DomainStatus entity, CancellationToken token = default);
}
