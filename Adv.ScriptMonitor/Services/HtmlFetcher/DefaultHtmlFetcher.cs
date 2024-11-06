using Adv.ScriptMonitor.Utilities;

namespace Adv.ScriptMonitor.Services.HtmlFetcher;

public class DefaultHtmlFetcher : IHtmlFetcher
{
    private readonly IHttpClientFactory _httpClientFactory;

    public DefaultHtmlFetcher(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = Check.NotNull(httpClientFactory, nameof(httpClientFactory));
    }

    public async Task<Stream> FetchHtmlAsync(Uri url, CancellationToken token = default)
    {
        var httpClient = _httpClientFactory.CreateClient();

        return await httpClient.GetStreamAsync(url, token);
    }
}
