namespace Adv.ScriptMonitor.Services.ScriptAvailabilityService;

public interface IScriptAvailabilityService
{
    public Task<bool> CheckScriptAvailabilityAsync(Stream htmlData, CancellationToken token = default);
}
