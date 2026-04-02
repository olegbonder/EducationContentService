using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Wolverine;
using Wolverine.Postgresql;
using Wolverine.RabbitMQ;

namespace FileService.Core.Messaging;

public static class WolverineConfiguration
{
    public static void AddWolverine(this WebApplicationBuilder builder)
    {
        string rabbitMQConnectionString = builder.Configuration.GetConnectionString("RabbitMQConnectionString");
        string postgresConnectionString = builder.Configuration.GetConnectionString("PostgresConnectionString");

        builder.Host.UseWolverine(opts =>
        {
            opts.ApplicationAssembly = typeof(WolverineConfiguration).Assembly;
            
            opts.ConfigureDurableMessaging(postgresConnectionString);
            opts.ConfigureRabbitMq(rabbitMQConnectionString);
            opts.ConfigureStandardErrorPolicies();
        }, ExtensionDiscovery.ManualOnly);
    }

    private static void ConfigureDurableMessaging(this WolverineOptions opts, string postgresConnectionString)
    {
        opts.PersistMessagesWithPostgresql(postgresConnectionString, "public");
        opts.UseEntityFrameworkCoreTransactions();
        opts.Policies.UseDurableOutboxOnAllSendingEndpoints();
        opts.Policies.UseDurableOutboxOnAllSendingEndpoints();
    }
    
    private static void ConfigureStandardErrorPolicies(this WebApplicationBuilder builder)
    {
        
    }
}