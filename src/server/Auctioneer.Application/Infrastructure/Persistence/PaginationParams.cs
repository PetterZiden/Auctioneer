namespace Auctioneer.Application.Infrastructure.Persistence;

public class PaginationParams
{
    private const int MaxPageSize = 20;
    public int PageNumber { get; init; } = 1;
    private readonly int _pageSize = 10;

    public int PageSize
    {
        get => _pageSize;
        init => _pageSize = value > MaxPageSize ? MaxPageSize : value;
    }
}