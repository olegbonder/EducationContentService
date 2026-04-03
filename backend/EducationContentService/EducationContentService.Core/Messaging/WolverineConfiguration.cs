using EducationContentService.Core.Messaging;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Wolverine;
using Wolverine.Postgresql;
using Wolverine.RabbitMQ;

namespace EducationContentService.Core.Messaging;

public static class WolverineConfiguration
{
    public static void AddWolverine(this WebApplicationBuilder builder)
    {
        string rabbitMQConnectionString = builder.Configuration.GetConnectionString(ConnectionStringNames.RABBIT_MQ);
        string postgresConnectionString = builder.Configuration.GetConnectionString(ConnectionStringNames.DATABASE);

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
        opts.Policies.UseDurableOutboxOnAllSendingEndpoints();
        opts.Policies.UseDurableOutboxOnAllSendingEndpoints();
    }
    
    private static void ConfigureStandardErrorPolicies(this WebApplicationBuilder builder)
    {
        
    }
}