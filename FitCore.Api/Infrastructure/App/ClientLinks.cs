namespace FitCore.Api.Infrastructure.App;

public static class ClientLinks
{
    public static string Activate(string baseUrl, string path, string rawToken) =>
        $"{baseUrl.TrimEnd('/')}/{path.TrimStart('/')}?token={Uri.EscapeDataString(rawToken)}";
}
