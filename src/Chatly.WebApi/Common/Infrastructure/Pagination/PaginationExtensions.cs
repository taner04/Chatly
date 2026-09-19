using Chatly.Contracts.Common.Pagination;
using Chatly.WebApi.Common.Infrastructure.Pagination.Exceptions;

namespace Chatly.WebApi.Common.Infrastructure.Pagination;

internal static class PaginationExtensions
{
    extension<T>(IQueryable<T> query)
    {
        public async Task<PaginationResult<T>> ToPaginationResultAsync(
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
                paginationQuery.PageIndex,
                paginationQuery.PageSize,
                totalPages,
                totalCount,
                paginationQuery.PageIndex > 1
                    ? paginationQuery.PageIndex - 1
                    : null,
                paginationQuery.PageIndex < totalPages
                    ? paginationQuery.PageIndex + 1
                    : null);

            return new PaginationResult<T>(items, metadata);
        }
    }

    extension<TSource>(PaginationResult<TSource> source)
    {
        public PaginationResult<TTarget> Map<TTarget>(Func<TSource, TTarget> map)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(map);

            return new PaginationResult<TTarget>(
                source.Items.Select(map).ToList(),
                source.Pagination);
        }
    }
}