namespace Auctioneer.Application.Infrastructure.Persistence;

public static class PaginationParams
{
    private const int MaxPageSize = 50;
    public static int PageNumber { get; set; } = 1;
    private static int _pageSize = 10;

    public static int PageSize
    {
        get => _pageSize;
        set => _pageSize = value > MaxPageSize ? MaxPageSize : value;
    }
}