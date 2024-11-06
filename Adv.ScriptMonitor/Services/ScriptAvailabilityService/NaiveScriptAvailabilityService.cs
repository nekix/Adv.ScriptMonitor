using HtmlAgilityPack;

namespace Adv.ScriptMonitor.Services.ScriptAvailabilityService;

public class NaiveScriptAvailabilityService : IScriptAvailabilityService
{
    // Алгоритм наивный, т.к. заставляет нас загрузить весь htmlData
    // и после построить HtmlDocument объект разобрав его.
    // Из оптимизаций возможно работать с потоком в "ручном" режиме,
    // загружать данные в некий буфер (а лучше два, для исключения разрыва разметки)
    // итеративно и проверять буфер(ы) на наличие скрипта. Это потенциально
    // позволит не загружать весь html для страниц, на которых скрипт определен в начале.
    public Task<bool> CheckScriptAvailabilityAsync(Stream htmlData, CancellationToken token = default)
    {
        var htmlDoc = new HtmlDocument();

        htmlDoc.Load(htmlData);

        var node = htmlDoc.DocumentNode
            .SelectSingleNode("//script[contains(@src, 'advmusic.com')]");

        return Task.FromResult(node != null);
    }
}
