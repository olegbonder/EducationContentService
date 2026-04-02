namespace IntegrationEvents;

public static class FileEventsRouting
{
    public const string EXCHANGE = "file-events";
    
    public static class RoutingKeys
    {
        public const string ALL_LESSON_EVENTS = "*.*.lesson";
        public const string ALL_MODULE_EVENTS = "*.*.module";
        
        public static string VideoCreated(string entityType) => $"video.created.{entityType.ToLowerInvariant()}";
        public static string VideoDeleted(string entityType) => $"video.deleted.{entityType.ToLowerInvariant()}";
        public static string ImageCreated(string entityType) => $"image.created.{entityType.ToLowerInvariant()}";
        public static string ImageDeleted(string entityType) => $"image.deleted.{entityType.ToLowerInvariant()}";
    }
}