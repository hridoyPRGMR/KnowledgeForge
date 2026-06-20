using KnowledgeForge.Infrastructure;
using KnowledgeForge.Infrastructure.Data;
using KnowledgeForge.Worker.Consumers;
using MassTransit;
using Microsoft.EntityFrameworkCore;

var hostBuilder = Host.CreateApplicationBuilder(args);

hostBuilder.Services.AddInfrastructure(hostBuilder.Configuration);

hostBuilder.Services.AddMassTransit(x =>
{
    x.AddConsumer<BookUploadedConsumer>();
    x.AddConsumer<GraphExtractionConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        var rabbitOptions = hostBuilder.Configuration.GetSection("RabbitMQ");
        cfg.Host(rabbitOptions["Host"] ?? "localhost", "/", h =>
        {
            h.Username(rabbitOptions["Username"] ?? "guest");
            h.Password(rabbitOptions["Password"] ?? "guest");
        });

        cfg.ConcurrentMessageLimit = 1;
        cfg.ConfigureEndpoints(context);
    });
});

var app = hostBuilder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}

await app.RunAsync();
