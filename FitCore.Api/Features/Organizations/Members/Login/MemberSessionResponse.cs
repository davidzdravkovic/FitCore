namespace FitCore.Api.Features.Organizations.Members.Login;

public record MemberSessionResponse(
    string Message,
    string AccessToken,
    string OrganizationName,
    string FirstName);
