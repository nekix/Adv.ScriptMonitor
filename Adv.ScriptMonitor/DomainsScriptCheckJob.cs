using Adv.ScriptMonitor.Models;
using Adv.ScriptMonitor.Repositories;
using Adv.ScriptMonitor.Services.DomainScriptReportService;
using Adv.ScriptMonitor.Services.HtmlFetcher;
using Adv.ScriptMonitor.Services.ScriptAvailabilityService;
using Quartz;

namespace Adv.ScriptMonitor;

[DisallowConcurrentExecution]
public class DomainsScriptCheckJob : IJob
{
    private readonly IDomainStatusRepository _domainStatusRepository;
    private readonly IScriptAvailabilityService _scriptAvailabilityService;
    private readonly IHtmlFetcher _htmlFetcher;
    private readonly IDomainStatusReportService _reportService;

    // MaxDegreeOfParallelism потенциально можно получать через свой класс
    // настроек и внедрять через DI.
    // Или производить настройку через фабрику и конструктор.
    private const int MaxDegreeOfParallelism = 10;

    public DomainsScriptCheckJob(IDomainStatusRepository domainStatusRepository,
                                 IScriptAvailabilityService scriptAvailabilityService,
                                 IHtmlFetcher htmlFetcher,
                                 IDomainStatusReportService reportService)
    {
        _domainStatusRepository = domainStatusRepository;
        _scriptAvailabilityService = scriptAvailabilityService;
        _htmlFetcher = htmlFetcher;
        _reportService = reportService;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var domainsToCheck = await _domainStatusRepository.GetListAsync();

        var parrallelOptions = new ParallelOptions()
        {
            MaxDegreeOfParallelism = MaxDegreeOfParallelism,
            CancellationToken = context.CancellationToken
        };

        // Параллельная обработка задачи проверки доменов
        await Parallel.ForEachAsync(domainsToCheck,
            parrallelOptions,
            async (domainStatus, token) => await CheckDomainScriptsAsync(domainStatus, token));
    }

    // Возможно стоит вынести метод и зависимые методы в отдельный сервис,
    // зависит от планируемого развития и использования.
    private async Task CheckDomainScriptsAsync(DomainStatus domainStatus, CancellationToken token = default)
    {
        Stream htmlStream;

        try
        {
            htmlStream = await _htmlFetcher.FetchHtmlAsync(domainStatus.DomainUrl, token);
        }
        catch (HttpRequestException)
        {
            await _reportService.ReportFailureAsync(domainStatus.DomainUrl.OriginalString, token);
            return;
        }

        var isAvailable = await _scriptAvailabilityService.CheckScriptAvailabilityAsync(htmlStream, token);

        if (!isAvailable)
        {
            await HandleFailureAsync(domainStatus, token);
        }
        else
        {
            await HandleSucessAsync(domainStatus, token);
        }
    }

    private async Task HandleFailureAsync(DomainStatus domainStatus, CancellationToken token)
    {
        await _domainStatusRepository.UpdateAsync(domainStatus.WithFailure(), token);

        await _reportService.ReportFailureAsync(domainStatus.DomainUrl.OriginalString, token);

        return;
    }

    private async Task HandleSucessAsync(DomainStatus domainStatus, CancellationToken token)
    {
        await _domainStatusRepository.UpdateAsync(domainStatus.WithSucces(), token);

        if (domainStatus.ScriptFailureStartTime.HasValue)
        {
            var failureTime = DateTime.Now.Subtract(domainStatus.ScriptFailureStartTime.Value);

            await _reportService.ReportSucessAsync(domainStatus.DomainUrl.OriginalString, failureTime, token);
        }
        else
        {
            await _reportService.ReportSucessAsync(domainStatus.DomainUrl.OriginalString, token);
        }
    }
}
