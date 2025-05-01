namespace MessageQueueApp.Producer.Configurations
{
    public class ProducerConfig
    {
        public string QueuePath { get; set; } = string.Empty;
        public string DeadLetterQueuePath { get; set; } = string.Empty;
        public int MaxMessageCount { get; set; } = 100;
        public int MaxFailedMessages { get; set; } = 10;
        public int MessageDelayMilliseconds { get; set; } = 50;
    }
}