// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.DacFx;
using Aspire.Hosting.Lifecycle;
using Aspire.Hosting.Publishing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SqlServer.Dac;

namespace Aspire.Hosting;

internal sealed class DacPacProvisioner(
    IOptions<PublishingOptions> publishingOptions,
    ILogger<DacPacProvisioner> logger) : IDistributedApplicationLifecycleHook
{
    public Task AfterEndpointsAllocatedAsync(DistributedApplicationModel appModel, CancellationToken cancellationToken = default)
    {
        // TODO: Make this more general purpose
        if (publishingOptions.Value.Publisher == "manifest")
        {
            return Task.CompletedTask;
        }

        var databaseResources = appModel.Resources
            .OfType<IResourceWithParent<ISqlServerParentResource>>()
            .Where(r => r.Annotations.OfType<DacPackageAnnotation>().Any())
            .ToArray();

        if (databaseResources.Length == 0)
        {
            return Task.CompletedTask;
        }

        var t = new Thread(() => ProvisionDacFxResources(logger, databaseResources, cancellationToken))
        {
            IsBackground = true
        };

        t.Start();

        return Task.CompletedTask;
    }

    private static void ProvisionDacFxResources(ILogger<DacPacProvisioner> logger, IResourceWithParent<ISqlServerParentResource>[] databaseResources, CancellationToken cancellationToken)
    {
        foreach (var databaseResource in databaseResources)
        {
            var annotation = databaseResource.Annotations.OfType<DacPackageAnnotation>().Single();
            var dacPackage = annotation.Project;
            var options = annotation.Options;
            var connectionString = databaseResource.Parent.GetConnectionString();

            logger.LogInformation("Provisioning database {DatabaseName} from project {ProjectName}", databaseResource.Name, dacPackage.Name);
            logger.LogInformation("Connection string: {ConnectionString}", connectionString);

            var dacServices = new DacServices(connectionString);
            dacServices.Message += (sender, args) =>
            {
                if (args.Message.MessageType == DacMessageType.Error)
                {
                    logger.LogError(message: args.Message.Message);
                    return;
                }
                if (args.Message.MessageType == DacMessageType.Warning)
                {
                    logger.LogWarning(message: args.Message.Message);
                    return;
                }
                logger.LogInformation(message: args.Message.Message);
            };

            // var projectPath = dacPackage.Annotations.OfType<IServiceMetadata>().Single().ProjectPath;
            using var package = DacPackage.Load("C:\\Users\\martin\\src\\gh\\aspire\\aspire\\artifacts\\bin\\Database\\Debug\\Database.dacpac");
            dacServices.Deploy(package, databaseResource.Name, true, options, cancellationToken);
        }
    }
}
