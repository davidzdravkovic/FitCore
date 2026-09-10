namespace FitCore.Api.Features.Members.Login;

public record MemberSessionResponse(
    string Message,
    string AccessToken,
    string OrganizationName,
    string FirstName);
