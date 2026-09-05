using FitCore.Api.Composition;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddFitCoreServices(builder.Configuration);

var app = builder.Build();

await app.UseFitCorePipelineAsync();

app.Run();
