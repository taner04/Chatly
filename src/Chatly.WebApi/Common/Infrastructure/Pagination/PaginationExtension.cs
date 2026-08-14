using Chatly.Contracts.Pagination;
using Chatly.WebApi.Common.Infrastructure.Pagination.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Chatly.WebApi.Common.Infrastructure.Pagination;

public static class PaginationExtensions
{
    public const int MaxPageSize = 100;

    public static async Task<PaginationResult<T>> ToPaginationResultAsync<T>(
        this IQueryable<T> query,
        PaginationQuery paginationQuery,
        CancellationToken cancellationToken = default)
    {
        PaginationQueryException.ThrowIfInvalidPaginationQuery(paginationQuery);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((paginationQuery.PageIndex - 1) * paginationQuery.PageSize)
            .Take(paginationQuery.PageSize)
            .ToListAsync(cancellationToken);

        var totalPages = (int)Math.Ceiling(
            totalCount / (double)paginationQuery.PageSize);

        var metadata = new PaginationMetadata(
            PageIndex: paginationQuery.PageIndex,
            PageSize: paginationQuery.PageSize,
            TotalPages: totalPages,
            TotalCount: totalCount,
            PreviousPageIndex: paginationQuery.PageIndex > 1
                ? paginationQuery.PageIndex - 1
                : null,
            NextPageIndex: paginationQuery.PageIndex < totalPages
                ? paginationQuery.PageIndex + 1
                : null);

        return new PaginationResult<T>(items, metadata);
    }

    public static PaginationResult<TTarget> Map<TSource, TTarget>(
        this PaginationResult<TSource> source,
        Func<TSource, TTarget> map)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(map);

        return new PaginationResult<TTarget>(
            source.Items.Select(map).ToList(),
            source.Pagination);
    }
}
