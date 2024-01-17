// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Aspire.Hosting.ApplicationModel;

namespace Aspire.Hosting.DacFx;

public static class SqlServerDatabaseResourceExtensions
{
    public static IResourceBuilder<SqlServerDatabaseResource> WithDacPackage(this IResourceBuilder<SqlServerDatabaseResource> builder, DatabaseProjectResource databaseProject, Microsoft.SqlServer.Dac.DacDeployOptions? options = default)
    {
        return builder.WithAnnotation(new DacPackageAnnotation(databaseProject));
    }

    public static IResourceBuilder<SqlServerDatabaseResource> WithDacPackage<TProject>(this IResourceBuilder<SqlServerDatabaseResource> builder, Microsoft.SqlServer.Dac.DacDeployOptions? options = default)
         where TProject : IServiceMetadata, new()
    {
        var annotation = new TProject();
        var project = new DatabaseProjectResource(annotation.ProjectPath);
        builder.ApplicationBuilder.AddResource(project).WithAnnotation(annotation);

        return builder.WithAnnotation(new DacPackageAnnotation(project, options));
    }
}
