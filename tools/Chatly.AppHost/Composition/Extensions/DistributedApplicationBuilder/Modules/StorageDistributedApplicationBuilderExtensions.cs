using Aspire.Hosting.Azure;
using Chatly.ServiceDefaults;

namespace Chatly.AppHost.Composition.Extensions.DistributedApplicationBuilder.Modules;

internal static class StorageDistributedApplicationBuilderExtensions
{
    extension(IDistributedApplicationBuilder builder)
    {
        internal IResourceBuilder<AzureBlobStorageResource> AddChatlyBlobStorage() =>
            builder.AddAzureStorage(AppHostConstants.BlobStorageResourceName)
                .RunAsEmulator()
                .AddBlobs(AppHostConstants.BlobServiceConnectionName);
    }
}
