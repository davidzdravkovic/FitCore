using Microsoft.Extensions.Options;

namespace FitCore.Api.Infrastructure.App;

public sealed class ClientLinks(IOptions<AppOptions> appOptions) : IClientLinks
{
    public string Activate(string path, string rawToken)
    {
        var baseUrl = appOptions.Value.ClientBaseUrl.TrimEnd('/');
        return $"{baseUrl}/{path.TrimStart('/')}?token={Uri.EscapeDataString(rawToken)}";
    }
}
