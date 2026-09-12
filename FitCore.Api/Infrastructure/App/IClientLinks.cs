namespace FitCore.Api.Infrastructure.App;

public interface IClientLinks
{
    string Activate(string path, string rawToken);
}
