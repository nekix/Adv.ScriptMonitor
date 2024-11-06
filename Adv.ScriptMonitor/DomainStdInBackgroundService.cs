using Adv.ScriptMonitor.Models;
using Adv.ScriptMonitor.Repositories;
using Adv.ScriptMonitor.Utilities;
using Microsoft.Extensions.Hosting;

namespace Adv.ScriptMonitor;

public class DomainStdInBackgroundService : BackgroundService
{
    private IDomainStatusRepository _domainStatusRepository;

    public DomainStdInBackgroundService(IDomainStatusRepository domainStatusRepository)
    {
        _domainStatusRepository = Check.NotNull(domainStatusRepository, nameof(domainStatusRepository));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Ожидание ввода и добавление доменов в репозиторий
        await ReadDomainsFromStdin(stoppingToken);
    }

    private async Task ReadDomainsFromStdin(CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(Console.OpenStandardInput());

        while (!cancellationToken.IsCancellationRequested)
        {
            string? input = await reader.ReadLineAsync(cancellationToken);

            input = Check.NotNullOrWhiteSpace(input, nameof(input));

            var domainStatus = new DomainStatus(input);

            await _domainStatusRepository.InsertAsync(domainStatus);
        }
    }
}
