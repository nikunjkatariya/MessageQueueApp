using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MessageQueueApp.Common.Interfaces;
using MessageQueueApp.Common.Models;
using MessageQueueApp.Consumer.Configurations;
using MessageQueueApp.Consumer.Interfaces;
using MessageQueueApp.Logging.Enums;
using MessageQueueApp.Logging.Interfaces;

namespace MessageQueueApp.Consumer.Services
{
    public class ConsumerService : IConsumerService
    {
        private readonly ILogger _logger;
        private readonly ConsumerConfig _config;
        private readonly Random _random = new Random();
        private int _successMsgCount = 0;
        private int _deadLetterCount = 0;
        private readonly IMessageQueueClient _queueClient;

        public ConsumerService(ILogger logger, ConsumerConfig config, IMessageQueueClient queueClient)
        {
            _logger = logger;
            _config = config;
            _queueClient = queueClient;
        }

        public async Task RunAsync()
        {
            _logger.Log("Consumer started listening to MSMQ.", LogLevel.Info);

            while (true)
            {

                var message = await _queueClient.ReceiveMessageAsync();

                if (message == null)
                {
                    _logger.Log("No messages to process.", LogLevel.Info);

                    _deadLetterCount = _queueClient.GetDeadLetterMessageCount();
                    if (_deadLetterCount >= _config.DeadLetterWarningThreshold)
                    {
                        _logger.Log($"Warning: Dead Letter queue has reached {_deadLetterCount} messages.", LogLevel.Warning);
                    }
                    _logger.Log($"Info: Total Successfully processed Messages: {_successMsgCount}", LogLevel.Warning);

                    // Wait interval before checking new messages
                    await Task.Delay(_config.CheckIntervalSeconds * 1000);
                    
                    continue;
                }


                bool processed = false;
                int retryCount = 0;
                int maxRetries = _config.MaxRetryCount;

                while (retryCount < maxRetries)
                {
                    try
                    {
                        processed = await ProcessMessageAsync(message);
                        if (processed)
                        {
                            _successMsgCount++;
                            _logger.Log($"Watchlist API Success for : {message.ToString()}", LogLevel.Info);
                            break; // Exit retry loop if message is successfully processed
                        }
                        else
                        {
                            retryCount++;
                            _logger.Log($"Message processing failed, retrying... Attempt {retryCount}/{maxRetries}", LogLevel.Warning);
                            await Task.Delay(1000);
                        }
                    }
                    catch (Exception ex)
                    {
                        retryCount++;
                        _logger.Log($"Error processing message: {ex.Message}. Attempt {retryCount}/{maxRetries}", LogLevel.Error);
                        await Task.Delay(1000);
                    }
                }

                if (!processed && retryCount >= maxRetries)
                {
                    // Move to dead-letter queue after max retries
                    _queueClient.MoveToDeadLetterQueue(message);
                    _logger.Log($"Message moved to dead-letter queue after {maxRetries} failed attempts: {message}", LogLevel.Error);
                }
            }
        }

        private async Task<bool> ProcessMessageAsync(AppointmentMessage message)
        {
            // Replace with actual processing logic
            await Task.Delay(500);

            // Simulate based on message content
            if (message.ToString().Contains("fail"))
            {
                return false;
            }

            return true;
        }
    }
}
