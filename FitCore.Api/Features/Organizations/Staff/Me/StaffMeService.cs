using FitCore.Api.Data.Stores.OrganizationOwner.StaffStore;
using FitCore.Api.Errors.Business;
using FitCore.Api.Features.Organizations.Admin.Staff;
using FitCore.Api.Features.Organizations.Admin.Staff.Create;

namespace FitCore.Api.Features.Organizations.Staff.Me;

public class StaffMeService(IStaffStore staffStore)
{
    public async Task<Result<StaffResponse>> GetAsync(
        Guid tenantId,
        Guid staffId,
        CancellationToken cancellationToken = default)
    {
        var staff = await staffStore.FindActiveByIdAsync(
            tenantId,
            staffId,
            cancellationToken);

        if (staff is null)
            return Result<StaffResponse>.Fail(ErrorCodes.StaffNotFound);

        return Result<StaffResponse>.Success(StaffService.ToResponse(staff));
    }
}
