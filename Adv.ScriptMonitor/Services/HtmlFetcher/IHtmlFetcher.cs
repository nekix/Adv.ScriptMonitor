namespace Adv.ScriptMonitor.Services.HtmlFetcher;

public interface IHtmlFetcher
{
    public Task<Stream> FetchHtmlAsync(Uri url, CancellationToken token);
}
