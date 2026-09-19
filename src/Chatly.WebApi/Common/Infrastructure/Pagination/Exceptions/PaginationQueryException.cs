using System.Net;
using Chatly.Contracts.Common.Pagination;

namespace Chatly.WebApi.Common.Infrastructure.Pagination.Exceptions;

internal sealed class PaginationQueryException : ChatlyException
{
    private PaginationQueryException(
        string title,
        string message,
        string errorCode)
        : base(title, message, errorCode, HttpStatusCode.BadRequest)
    {
    }

    public static void ThrowIfInvalidPaginationQuery(PaginationQuery paginationQuery)
    {
        if (paginationQuery.PageIndex < 1 ||
            paginationQuery.PageSize is < 1 or > PaginationPolicy.MaxPageSize)
        {
            throw new PaginationQueryException(
                "Invalid pagination query",
                $"Page index must be at least 1, and page size must be between 1 and {PaginationPolicy.MaxPageSize}.",
                "Pagination.InvalidQuery");
        }
    }
}