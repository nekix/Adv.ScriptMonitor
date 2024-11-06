using Adv.ScriptMonitor.Models;

namespace Adv.ScriptMonitor.Repositories;

// Простая реализация репозитория на базе списка,
// обычно используется какая-либо готовая инфраструктура.
public class DomainStatusRepository : IDomainStatusRepository
{
    // Сравнительно (с http запросами) небольшое снижение
    // производительности за счёт потокобезопасности
    private readonly object _syncRoot = new object();

    private readonly List<DomainStatus> _domainStatucesData;

    public DomainStatusRepository()
    {
        _domainStatucesData = new List<DomainStatus>();
    }

    public Task<List<DomainStatus>> GetListAsync(CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();

        lock (_syncRoot)
        {
            return Task.FromResult(_domainStatucesData.ToList());
        }
    }

    public Task<DomainStatus> InsertAsync(DomainStatus entity, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();

        lock (_syncRoot)
        {
            if (_domainStatucesData.Any(e => e.DomainUrl == entity.DomainUrl))
                throw new ArgumentException($"Entity with '{entity.DomainUrl.OriginalString}' URL already exist exception.", nameof(entity));

            _domainStatucesData.Add(entity);
        }

        return Task.FromResult(entity);
    }

    public Task<DomainStatus> UpdateAsync(DomainStatus updatedEntity, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();

        lock (_syncRoot)
        {
            var index = _domainStatucesData.FindIndex(f => f.DomainUrl == updatedEntity.DomainUrl);

            if (index < 0)
                throw new ArgumentException($"Entity with '{updatedEntity.DomainUrl.OriginalString}' URL not exist exception.");

            _domainStatucesData[index] = updatedEntity;
        }

        return Task.FromResult(updatedEntity);
    }
}
