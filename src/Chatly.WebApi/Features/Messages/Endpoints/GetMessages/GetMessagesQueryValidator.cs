using FluentValidation;

namespace Chatly.WebApi.Features.Messages.Endpoints.GetMessages;

internal sealed class GetMessagesQueryValidator : AbstractValidator<GetMessagesQuery>
{
    public GetMessagesQueryValidator()
    {
        RuleFor(query => query.PageSize)
            .InclusiveBetween(1, 100);

        RuleFor(query => query)
            .Must(query => query.BeforeSentAt.HasValue == query.BeforeMessageId.HasValue)
            .WithMessage("Both cursor values must be provided together.");
    }
}
