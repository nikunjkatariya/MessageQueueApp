using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageQueueApp.Consumer.Configurations
{
    public class ConsumerConfig
    {
        public string QueuePath { get; set; } = string.Empty;
        public string DeadLetterQueuePath { get; set; } = string.Empty;
        public int MaxRetryCount { get; set; } = 2;
        public int CheckIntervalSeconds { get; set; } = 10;
        public int DeadLetterWarningThreshold { get; set; } = 5;
    }
}
