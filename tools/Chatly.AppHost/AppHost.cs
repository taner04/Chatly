using System.Net.Sockets;
using Chatly.AppHost.Options;
using Chatly.Contracts.Common;
using Chatly.ServiceDefaults;
using Chatly.Shared.Extensions;
using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var papercutOption = builder.Configuration.GetOption<PapercutOption>();
var liveKitOption = builder.Configuration.GetOption<LiveKitOption>();
var emailOption = builder.Configuration.GetOption<EmailOption>();

var db = builder.AddPostgres("database").WithPgAdmin();
var chatlyDb = db.AddDatabase(AppHostConstants.DatabaseConnectionName);

var migration = builder.AddProject<Chatly_MigrationService>(AppHostConstants.MigrationServiceResourceName)
    .WithReference(chatlyDb)
    .WaitFor(chatlyDb);

var blobStorage = builder.AddAzureStorage(AppHostConstants.BlobStorageResourceName)
    .RunAsEmulator()
    .AddBlobs(AppHostConstants.BlobServiceConnectionName);

var papercut = builder.AddPapercutSmtp(AppHostConstants.Papercut, papercutOption.HttpPort, papercutOption.SmtpPort)
    .WithImage(papercutOption.Image, papercutOption.Tag);

var api = builder.AddProject<Chatly_WebApi>(AppHostConstants.WebApiResourceName);

var liveKit = builder.AddContainer(AppHostConstants.LiveKitResourceName, liveKitOption.Image, liveKitOption.Tag)
    .WithArgs("--dev", "--bind", liveKitOption.BindAddress, "--node-ip", liveKitOption.NodeIp)
    .WithEnvironment("LIVEKIT_KEYS", $"{liveKitOption.ApiKey}: {liveKitOption.ApiSecret}")
    .WithEnvironment(
        "LIVEKIT_CONFIG",
        ReferenceExpression.Create(
            $"webhook:\n  api_key: {liveKitOption.ApiKey}\n  urls:\n    - {api.GetEndpoint("http")}{ApiRoutes.LiveKit.Webhook}\n"))
    .WithHttpEndpoint(liveKitOption.HttpPort, liveKitOption.HttpPort, "http", isProxied: false)
    .WithEndpoint(liveKitOption.RtcTcpPort, liveKitOption.RtcTcpPort, "tcp", "rtc-tcp", isProxied: false)
    .WithEndpoint(
        liveKitOption.RtcUdpPort,
        liveKitOption.RtcUdpPort,
        "udp",
        "rtc-udp",
        isProxied: false,
        protocol: ProtocolType.Udp);

api
    .WithReference(chatlyDb)
    .WaitFor(chatlyDb)
    .WithReference(blobStorage)
    .WaitFor(blobStorage)
    .WaitForCompletion(migration)
    .WithReference(papercut)
    .WaitFor(papercut)
    .WithEnvironment("EmailOption__Host", papercut.GetEndpoint("smtp").Property(EndpointProperty.Host))
    .WithEnvironment("EmailOption__Port", papercut.GetEndpoint("smtp").Property(EndpointProperty.Port))
    .WithEnvironment("EmailOption__SenderName", emailOption.SenderName)
    .WithEnvironment("EmailOption__SenderEmail", emailOption.SenderEmail)
    .WithEnvironment("EmailOption__Username", emailOption.Username ?? string.Empty)
    .WithEnvironment("EmailOption__Password", emailOption.Password ?? string.Empty)
    .WithEnvironment("EmailOption__UseSsl", emailOption.UseSsl.ToString())
    .WithEnvironment("LiveKitOption__ServerUrl", liveKitOption.ServerUrl)
    .WithEnvironment("LiveKitOption__ApiKey", liveKitOption.ApiKey)
    .WithEnvironment("LiveKitOption__ApiSecret", liveKitOption.ApiSecret)
    .WaitFor(liveKit);

builder.Build().Run();