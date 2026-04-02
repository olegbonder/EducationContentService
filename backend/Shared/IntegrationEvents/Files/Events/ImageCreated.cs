namespace IntegrationEvents.Files.Events;

public record ImageCreated(
    Guid VideoId,
    Guid EntityId,
    string EntityType,
    DateTime OccurredAt);