using EbayClone.API.DTOs.Dashboard;
using EbayClone.API.Repositories;

namespace EbayClone.API.Services;

public class AdminDashboardService(IDashboardRepository dashboardRepository) : IAdminDashboardService
{
    public async Task<DashboardDto> GetAsync(CancellationToken cancellationToken = default)
    {
        var totalUsers = await dashboardRepository.CountUsersAsync(cancellationToken);
        var totalProducts = await dashboardRepository.CountProductsAsync(cancellationToken);
        var totalOrders = await dashboardRepository.CountOrdersAsync(cancellationToken);
        var revenue = await dashboardRepository.SumRevenueAsync(cancellationToken);
        var activeUsers = await dashboardRepository.CountActiveUsersAsync(cancellationToken);
        var bannedUsers = await dashboardRepository.CountBannedUsersAsync(cancellationToken);
        var hiddenProducts = await dashboardRepository.CountHiddenProductsAsync(cancellationToken);
        var pendingDisputes = await dashboardRepository.CountPendingDisputesAsync(cancellationToken);

        var alerts = new List<DashboardAlertDto>();
        if (pendingDisputes > 0)
            alerts.Add(new("danger", "Khiếu nại cần xử lý", $"Có {pendingDisputes} khiếu nại đang chờ xử lý.", "Disputes", "Index"));
        if (hiddenProducts > 0)
            alerts.Add(new("warning", "Sản phẩm đang bị ẩn", $"Có {hiddenProducts} sản phẩm cần được kiểm duyệt.", "Products", "Index"));
        if (bannedUsers > 0)
            alerts.Add(new("warning", "Tài khoản bị khóa", $"Hiện có {bannedUsers} tài khoản đang bị khóa.", "Users", "Index"));

        return new DashboardDto(
            totalUsers,
            totalProducts,
            totalOrders,
            revenue,
            activeUsers,
            bannedUsers,
            hiddenProducts,
            pendingDisputes,
            alerts);
    }
}
