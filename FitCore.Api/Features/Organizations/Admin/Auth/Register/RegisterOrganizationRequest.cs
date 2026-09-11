using System.ComponentModel.DataAnnotations;

namespace FitCore.Api.Features.Organizations.Admin.Auth.Register;

public record RegisterOrganizationRequest(
    [Required] string InvitationToken,
    [Required] string OrganizationName,
    [Required, EmailAddress] string BusinessEmail,
    [Required] string Country,
    [Required] string City,
    [Required] string TimeZone,
    [Required] string OwnerFirstName,
    [Required] string OwnerLastName,
    [Required, EmailAddress] string OwnerEmail,
    [Required, MinLength(8)] string OwnerPassword);
