using Chatly.Contracts.Users.Results;
using Mediator;

namespace Chatly.WebApi.Features.Users.Endpoints.GetProfilePicture;

public class GetCurrentUserProfilePictureQuery : IQuery<GetPictureResponse>;