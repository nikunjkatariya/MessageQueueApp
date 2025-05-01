using MessageQueueApp.Common.Interfaces;
using MessageQueueApp.Consumer.Configurations;
using MessageQueueApp.Consumer.Interfaces;
using MessageQueueApp.Consumer.Services;
using MessageQueueApp.Logging.Implementations;
using MessageQueueApp.Logging.Interfaces;
using MessageQueueApp.Messaging.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

class program
{
    static async Task Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, config) =>
            {
                config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            })
            .ConfigureServices((context, services) =>
            {
                services.AddSingleton<ILogger>(ConsoleLogger.Instance);

                services.Configure<ConsumerConfig>(context.Configuration);

                var queuePath = context.Configuration["QueuePath"] ?? string.Empty;
                var deadLetterQueuePath = context.Configuration["DeadLetterQueuePath"] ?? string.Empty;
            
                // Bind ProducerConfig instance
                var consumerConfig = new ConsumerConfig();
                context.Configuration.Bind(consumerConfig);
                services.AddSingleton(consumerConfig);

                services.AddSingleton<IMessageQueueClient>(sp =>
                    new MsmqClient(queuePath, deadLetterQueuePath, sp.GetRequiredService<ILogger>()));

                services.AddTransient<IConsumerService, ConsumerService>();
            })
            .Build();

        var consumer = host.Services.GetRequiredService<IConsumerService>();
        await consumer.RunAsync();
    }
}