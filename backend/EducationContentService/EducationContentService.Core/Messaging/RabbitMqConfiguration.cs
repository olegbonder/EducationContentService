using IntegrationEvents;
using IntegrationEvents.Education;
using IntegrationEvents.Education.Events;
using Wolverine;
using Wolverine.RabbitMQ;

namespace EducationContentService.Core.Messaging;

public static class RabbitMqConfiguration
{
    private const string EDUCATION_LESSON_FILE_EVENTS_QUEUE = "education.lesson-file-events";

    private const string EDUCATION_MODULE_FILE_EVENTS_QUEUE = "education.module-file-events";
    
    public static void ConfigureRabbitMq(this WolverineOptions options, string connectionString)
    {
        options.UseRabbitMq(new Uri(connectionString))
            .AutoProvision()
            .EnableWolverineControlQueues()
            .UseQuorumQueues()
            .DeclareExchange(EducationEventsRouting.EXCHANGE, exchange =>
            {
                exchange.ExchangeType = ExchangeType.Fanout;
                exchange.IsDurable = true;
            })
            .DeclareExchange(FileEventsRouting.EXCHANGE, exchange =>
            {
                exchange.ExchangeType = ExchangeType.Topic;
                exchange.IsDurable = true;
            });

        options.ConfigureEducationEventsPublishing();
        options.ConfigureEducationEventsListeners();
    }
    
    private static void ConfigureEducationEventsPublishing(this WolverineOptions opts)
    {
        opts.ListenToRabbitQueue(EDUCATION_LESSON_FILE_EVENTS_QUEUE, queue =>
        {
            queue.BindExchange(FileEventsRouting.EXCHANGE, FileEventsRouting.RoutingKeys.ALL_LESSON_EVENTS);
        });

        opts.ListenToRabbitQueue(EDUCATION_MODULE_FILE_EVENTS_QUEUE, queue =>
        {
            queue.BindExchange(FileEventsRouting.EXCHANGE, FileEventsRouting.RoutingKeys.ALL_MODULE_EVENTS);
        });
    }

    private static void ConfigureEducationEventsListeners(this WolverineOptions opts)
    {
        string exchange = EducationEventsRouting.EXCHANGE;

        opts.PublishMessage<LessonCreated>().ToRabbitQueue(exchange).UseDurableOutbox();
        opts.PublishMessage<LessonSoftDeleted>().ToRabbitQueue(exchange).UseDurableOutbox();
        opts.PublishMessage<IssueCreated>().ToRabbitQueue(exchange).UseDurableOutbox();
        opts.PublishMessage<IssueSoftDeleted>().ToRabbitQueue(exchange).UseDurableOutbox();
    }
}