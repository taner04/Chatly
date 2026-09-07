using Microsoft.AspNetCore.OpenApi;

namespace Chatly.WebApi.Common.Composition.Configs.OpenApi;

public static class OpenApiConfig
{
    public static void Config(
        OpenApiOptions options)
    {
        options.AddDocumentTransformer<BearerDocumentTransformer>();
    }
}