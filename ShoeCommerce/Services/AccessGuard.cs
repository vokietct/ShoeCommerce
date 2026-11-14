namespace ShoeCommerce.Services;

public sealed class AccessGuard(IAuthService auth)
{
    public async Task AdminAsync(int? actorId = null)
    {
        await ActiveAsync();
        if (!auth.IsAdmin || (actorId.HasValue && actorId != auth.CurrentUser!.UserId)) Denied();
    }

    public async Task StaffAsync(int? actorId = null)
    {
        await ActiveAsync();
        if (!auth.IsAdminOrEmployee || (actorId.HasValue && actorId != auth.CurrentUser!.UserId)) Denied();
    }

    public async Task CustomerAsync(int customerId, bool allowAdmin = false, bool allowStaff = false)
    {
        await ActiveAsync();
        if ((allowAdmin && auth.IsAdmin) || (allowStaff && auth.IsAdminOrEmployee)) return;
        if (!auth.IsCustomer || auth.CurrentUser!.UserId != customerId) Denied();
    }

    private async Task ActiveAsync()
    {
        if (!await auth.ValidateSessionAsync()) Denied();
    }

    private static void Denied() => throw new UnauthorizedAccessException("Bạn không có quyền thực hiện thao tác này hoặc phiên đăng nhập đã hết hạn.");
}
