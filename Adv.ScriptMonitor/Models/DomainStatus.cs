using Adv.ScriptMonitor.Utilities;

namespace Adv.ScriptMonitor.Models;

public sealed class DomainStatus
{
    public Uri DomainUrl { get; private set; }

    public DateTime? ScriptFailureStartTime { get; private set; }

    public bool IsScriptFound => ScriptFailureStartTime == null;

    internal DomainStatus(string domainUrl)
    {
        Check.NotNullOrWhiteSpace(domainUrl, nameof(domainUrl));

        if (!Uri.TryCreate(domainUrl, UriKind.Absolute, out Uri? uri))
            throw new ArgumentException("Not valid URL format.", nameof(domainUrl));

        if (!(uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
            throw new ArgumentException("Not valid URL format.", nameof(domainUrl));

        DomainUrl = uri;
    }

    public DomainStatus WithFailure()
    {
        return new DomainStatus(DomainUrl.OriginalString)
        {
            ScriptFailureStartTime = DateTime.Now
        };
    }

    public DomainStatus WithSucces()
    {
        return new DomainStatus(DomainUrl.OriginalString);
    }
}
