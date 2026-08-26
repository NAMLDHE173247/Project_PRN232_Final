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
        var pendingDisputes = await dashboardRepository.CountPendingDisputesAsync(cancellationToken);
        var pendingUsers = await dashboardRepository.CountPendingUsersAsync(cancellationToken);
        var hiddenProducts = await dashboardRepository.CountHiddenProductsAsync(cancellationToken);
        var hiddenReviews = await dashboardRepository.CountHiddenReviewsAsync(cancellationToken);

        var alerts = new List<DashboardAlertDto>();
        if (pendingDisputes > 0)
            alerts.Add(new("danger", "Khiếu nại cần xử lý", $"Có {pendingDisputes} khiếu nại đang chờ xử lý.", "Disputes", "Index"));

        return new DashboardDto(
            totalUsers,
            totalProducts,
            totalOrders,
            revenue,
            pendingDisputes,
            pendingUsers,
            hiddenProducts,
            hiddenReviews,
            alerts);
    }
}
