using Aspire.Hosting.DacFx;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddDacFxProvisioning();

builder.AddSqlServerContainer("sqlserver")
    .AddDatabase("database")
    .WithDacPackage<Projects.Database>();

builder.Build().Run();
