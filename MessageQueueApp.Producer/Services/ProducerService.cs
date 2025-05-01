using System.Text.Json;
using MessageQueueApp.Common.Interfaces;
using MessageQueueApp.Common.Models;
using MessageQueueApp.Common.Utilities;
using MessageQueueApp.Logging.Enums;
using MessageQueueApp.Logging.Interfaces;
using MessageQueueApp.Producer.Configurations;
using MessageQueueApp.Producer.Interfaces;

public class ProducerService : IProducerService
{
    #region Declaration
    private readonly ILogger _logger;
    private readonly ProducerConfig _config;
    private readonly IMessageQueueClient _queueClient;

    public ProducerService(ILogger logger, ProducerConfig config, IMessageQueueClient queueClient)
    {
        _logger = logger;
        _config = config;
        _queueClient = queueClient;
    }
    #endregion

    public async Task RunAsync()
    {
        while (true)
        {
            Console.WriteLine($"Press Enter to continue, or type exit to quit");
            string? input = Console.ReadLine();
            if (string.Equals(input, "exit", StringComparison.OrdinalIgnoreCase))
            {
                _logger.Log("Producer exiting on user request.", LogLevel.Info);
                break;
            }

            Console.WriteLine($"Please enter number of messages to generate (Max {_config.MaxMessageCount}):");
            if (!int.TryParse(Console.ReadLine(), out int messageCount) || messageCount <= 0 || messageCount > _config.MaxMessageCount)
            {
                Console.WriteLine($"Invalid input. Please enter a number between 1 and {_config.MaxMessageCount}.");
                continue;
            }

            Console.WriteLine($"Enter number of messages to fail (less than or equal to {messageCount} & Max {_config.MaxFailedMessages}):");
            if (!int.TryParse(Console.ReadLine(), out int failedCount) || failedCount <  0 || failedCount > _config.MaxFailedMessages || failedCount > messageCount)
            {
                Console.WriteLine("Invalid input for failed message count.");
                continue;
            }

            // Generate messages
            var messages = GenerateMessages(messageCount, failedCount);

            foreach (var message in messages)
            {
                _queueClient.SendMessage(message);
                _logger.Log($"Produced message for Ticket: {message.Ticket}", LogLevel.Info);
                await Task.Delay(_config.MessageDelayMilliseconds);
            }

            _logger.Log($"Total messages generated: {messageCount}", LogLevel.Info);
            _logger.Log($"Total messages marked as failed: {failedCount}", LogLevel.Warning);

            Console.WriteLine("Press DQ to Clear Dead Letter Queue:");
            string? dqInput = Console.ReadLine();
            if (string.Equals(dqInput, "DQ", StringComparison.OrdinalIgnoreCase))
            {
                _queueClient.ClearDeadLetterQueue();
            }
        }
    }

    #region Private Methods
    private List<AppointmentMessage> GenerateMessages(int total, int failCount)
    {
        var messageList = new List<AppointmentMessage>(total);
        var failIndexes = new HashSet<int>();
        var rand = new Random();

        while (failIndexes.Count < failCount)
        {
            failIndexes.Add(rand.Next(0, total));
        }

        int baseTicketNumber = 10000;

        for (int i = 0; i < total; i++)
        {
            string ticket = $"{IdGenerator.GenerateRandomAlphabets()}-{DateTime.Now:MM/dd}-{(baseTicketNumber + i)}";
            string container = $"{IdGenerator.GenerateRandomAlphabets()}{IdGenerator.GenerateRandomDigits(6)}0";

            bool markedFailed = failIndexes.Contains(i);

            var message = new AppointmentMessage
            {
                Ticket = ticket,
                Container = container,
                Message = "Appointment has been created for Ticket: " + ticket + " and container: " + container
                          + (markedFailed ? " - This message is marked to fail." : "")
            };

            messageList.Add(message);
        }

        return messageList;
    }
    #endregion
}