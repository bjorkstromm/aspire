// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Aspire.Hosting.ApplicationModel;
using Microsoft.SqlServer.Dac;

namespace Aspire.Hosting.DacFx;

public class DacPackageAnnotation(DatabaseProjectResource project, DacDeployOptions? options = default) : IResourceAnnotation
{
    public DatabaseProjectResource Project => project;
    public DacDeployOptions? Options => options;
}
