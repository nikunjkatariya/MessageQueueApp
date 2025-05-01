using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using MessageQueueApp.Common.Interfaces;
using MessageQueueApp.Common.Models;
using MessageQueueApp.Logging.Enums;
using MessageQueueApp.Logging.Interfaces;
using System.ComponentModel;
using Experimental.System.Messaging;
using System.Runtime.Serialization;

namespace MessageQueueApp.Messaging.Services
{
    public class MsmqClient : IMessageQueueClient
    {
        private readonly string _queuePath;
        private readonly string? _deadLetterQueuePath;
        private readonly ILogger _logger;

        public MsmqClient(string queuePath, string? deadLetterQueuePath, ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            if (string.IsNullOrWhiteSpace(queuePath))
                throw new ArgumentException("Queue path cannot be null or empty.", nameof(queuePath));

            _queuePath = queuePath;
            _deadLetterQueuePath = deadLetterQueuePath;
            _logger = logger;

            if (!MessageQueue.Exists(_queuePath))
                MessageQueue.Create(_queuePath);
            if (_deadLetterQueuePath != null && !MessageQueue.Exists(_deadLetterQueuePath))
                MessageQueue.Create(_deadLetterQueuePath);
        }

        public void SendMessage(AppointmentMessage message)
        {
            using var queue = new MessageQueue(_queuePath);
            queue.Formatter = new XmlMessageFormatter(new Type[] { typeof(string) });
            var jsonMessage = JsonSerializer.Serialize(message);
            var msmqMessage = new Message { Body = jsonMessage, Label = message.Ticket };
            queue.Send(msmqMessage);
            _logger.Log($"Enqueued message for Ticket: {message.Ticket}", LogLevel.Info);
        }

        public async Task<AppointmentMessage?> ReceiveMessageAsync()
        {
            using var queue = new MessageQueue(_queuePath);
            queue.Formatter = new XmlMessageFormatter(new Type[] { typeof(string) });
            try
            {
                var msmqMessage = queue.Receive(new TimeSpan(0, 0, 5)); // Wait 5 sec
                if (msmqMessage == null) 
                    return null;
                var jsonString = msmqMessage?.Body as string;

                if (string.IsNullOrEmpty(jsonString))
                {
                    _logger.Log("Received empty message body.", LogLevel.Warning);
                    return null;
                }
                var message = JsonSerializer.Deserialize<AppointmentMessage>(jsonString);
                return await Task.FromResult(message);
            }
            catch (MessageQueueException mqe)
            {
                if (mqe?.Message != null && mqe.Message.IndexOf("timeout", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    _logger.Log($"Timeout while receiving message: {mqe.Message}", LogLevel.Warning);
                    return null;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.Log($"Error receiving message: {ex.Message}", LogLevel.Error);
                return null;
            }
        }

        public void MoveToDeadLetterQueue(AppointmentMessage message)
        {
            try 
            {
                if (string.IsNullOrEmpty(_deadLetterQueuePath))
                {
                    _logger.Log("Dead letter queue path not configured. Message cannot be moved to dead letter queue.", LogLevel.Warning);
                    return;
                }
                using var deadLetterQueue = new MessageQueue(_deadLetterQueuePath);
                var jsonMessage = JsonSerializer.Serialize(message);
                deadLetterQueue.Send(jsonMessage);
                _logger.Log($"Moved message to dead letter queue for Ticket: {message.Ticket}", LogLevel.Warning);
            }
            catch (MessageQueueException mqe)
            {
                _logger.Log($"Error receiving message: {mqe.Message}", LogLevel.Error);
            }
            catch (Exception ex)
            {
                _logger.Log($"Error receiving message: {ex.Message}", LogLevel.Error);
            }
        }

        public int GetDeadLetterMessageCount()
        {
            if (!MessageQueue.Exists(_deadLetterQueuePath))
                return 0;

            using var deadLetterQueue = new MessageQueue(_deadLetterQueuePath);
            return deadLetterQueue.GetAllMessages().Length;
        }

        public void ClearDeadLetterQueue()
        {
            if (!MessageQueue.Exists(_deadLetterQueuePath))
            {
                _logger.Log("Dead letter queue does not exist.", LogLevel.Warning);
                return;
            }

            using var deadLetterQueue = new MessageQueue(_deadLetterQueuePath);
            deadLetterQueue.Formatter = new XmlMessageFormatter(new Type[] { typeof(string) });

            try
            {
                while (true)
                {
                    var msg = deadLetterQueue.Receive(new TimeSpan(0, 0, 1));
                    if (msg == null)
                        break;
                }
                _logger.Log("Dead letter queue cleared successfully.", LogLevel.Info);
            }
            catch (MessageQueueException mqe)
            {
                if (mqe.Message.IndexOf("timeout", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    // No more messages in queue - expected exit condition
                    _logger.Log("Dead letter queue is now empty.", LogLevel.Info);
                }
                else
                {
                    _logger.Log($"Error clearing dead letter queue: {mqe?.Message}", LogLevel.Error);
                }
            }
            catch (Exception ex)
            {
                _logger.Log($"Unexpected error clearing dead letter queue: {ex.Message}", LogLevel.Error);
            }
        }
    }
}
