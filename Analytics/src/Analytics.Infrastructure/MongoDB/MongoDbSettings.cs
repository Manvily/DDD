namespace Analytics.Infrastructure.MongoDB;

public class MongoDbSettings
{
    public const string SectionName = "MongoDB";
    
    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
    public string AnalyticsEventsCollectionName { get; set; } = "analytics_events";
    public int ConnectionTimeoutMs { get; set; } = 30000;
    public int ServerSelectionTimeoutMs { get; set; } = 30000;
    public int SocketTimeoutMs { get; set; } = 30000;
    public int MaxPoolSize { get; set; } = 100;
    public int MinPoolSize { get; set; } = 0;
    public bool RetryWrites { get; set; } = true;
    public bool RetryReads { get; set; } = true;
    public int MaxIdleTimeMs { get; set; } = 300000; // 5 minutes
    public int HeartbeatIntervalMs { get; set; } = 10000; // 10 seconds
}
