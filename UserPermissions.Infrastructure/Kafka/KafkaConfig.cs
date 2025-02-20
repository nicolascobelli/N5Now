namespace UserPermissions.Infrastructure.Kafka
{
    public class KafkaConfig
    {
        public string BootstrapServers { get; set; } = string.Empty;
        public string TopicName { get; set; } = string.Empty;
    }
}