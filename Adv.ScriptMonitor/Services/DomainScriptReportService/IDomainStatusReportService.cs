namespace Adv.ScriptMonitor.Services.DomainScriptReportService;

public interface IDomainStatusReportService
{
    public Task ReportSucessAsync(string uri, CancellationToken token = default);

    public Task ReportSucessAsync(string uri, TimeSpan scriptFailureTime, CancellationToken token = default);

    public Task ReportFailureAsync(string uri, CancellationToken token = default);
}
