namespace IntegrationEvents.Files.Events;

public record ImageDeleted(
    Guid VideoId,
    Guid EntityId,
    string EntityType,
    DateTime OccurredAt);