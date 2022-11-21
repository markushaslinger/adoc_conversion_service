using AdocConversionService.Core;
using AdocConversionService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc();
builder.Services.AddGrpcReflection();
builder.Services.PerformServiceSetup();

var app = builder.Build();

// TODO remove for final deployment
app.MapGrpcReflectionService();

// Configure the HTTP request pipeline.
app.MapGrpcService<ConvertService>();
app.MapGet("/",
    () =>
        "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();