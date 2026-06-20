using KnowledgeForge.Application.Configuration;
using KnowledgeForge.Application.Interfaces;
using KnowledgeForge.Infrastructure.Ai;
using KnowledgeForge.Infrastructure.Data;
using KnowledgeForge.Infrastructure.Services;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace KnowledgeForge.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RagOptions>(configuration.GetSection(RagOptions.SectionName));
        services.Configure<StorageOptions>(configuration.GetSection(StorageOptions.SectionName));
        services.Configure<RabbitMqOptions>(configuration.GetSection(RabbitMqOptions.SectionName));

        services.AddAiProviders(configuration);

        var postgresConnection = configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException("Postgres connection string is required.");

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(postgresConnection, o => o.UseVector()));

        var redisConnection = configuration.GetConnectionString("Redis") ?? "localhost:6379";
        services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisConnection));
        services.AddSingleton<ICacheService, CacheService>();

        services.AddScoped<IRetrievalService, RetrievalService>();
        services.AddScoped<IPdfProcessingService, PdfProcessingService>();
        services.AddScoped<BookProcessingService>();
        services.AddScoped<IKnowledgeGraphExtractionService, KnowledgeGraphExtractionService>();
        services.AddScoped<IBusPublisher, MassTransitBusPublisher>();

        services.AddScoped<IBookService, BookService>();
        services.AddScoped<IChatService, ChatService>();
        services.AddScoped<ISummaryService, SummaryService>();
        services.AddScoped<IQuizService, QuizService>();
        services.AddScoped<INotesService, NotesService>();
        services.AddScoped<ILearningProfileService, LearningProfileService>();
        services.AddScoped<IKnowledgeGraphService, KnowledgeGraphService>();
        services.AddScoped<IStorageService, LocalStorageService>();

        return services;
    }

    public static IServiceCollection AddMassTransitWithRabbitMq(this IServiceCollection services, IConfiguration configuration)
    {
        var rabbitOptions = configuration.GetSection(RabbitMqOptions.SectionName).Get<RabbitMqOptions>() ?? new RabbitMqOptions();

        services.AddMassTransit(x =>
        {
            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(rabbitOptions.Host, "/", h =>
                {
                    h.Username(rabbitOptions.Username);
                    h.Password(rabbitOptions.Password);
                });
                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}
