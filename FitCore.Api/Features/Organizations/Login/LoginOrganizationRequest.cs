namespace FitCore.Api.Features.Organizations.Login;

public record LoginOrganizationRequest(
    string Email,
    string Password);
