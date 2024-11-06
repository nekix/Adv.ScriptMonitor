using Adv.ScriptMonitor.Services.DomainScriptReportService;

namespace Adv.ScriptMonitor.Final.Services.DomainScriptReportService;

public class StdOutDomainScriptReportService : IDomainStatusReportService
{
    public Task ReportFailureAsync(string uri, CancellationToken token = default)
    {
        Console.WriteLine($"{uri} - fail");

        return Task.CompletedTask;
    }

    public Task ReportSucessAsync(string uri, CancellationToken token = default)
    {
        Console.WriteLine($"{uri} ok");

        return Task.CompletedTask;
    }

    public Task ReportSucessAsync(string uri, TimeSpan scriptFailureTime, CancellationToken token = default)
    {
        Console.WriteLine($"{uri} - recovered after {scriptFailureTime}");

        return Task.CompletedTask;
    }
}
