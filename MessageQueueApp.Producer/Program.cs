using MessageQueueApp.Common.Interfaces;
using MessageQueueApp.Logging.Implementations;
using MessageQueueApp.Logging.Interfaces;
using MessageQueueApp.Messaging.Services;
using MessageQueueApp.Producer.Configurations;
using MessageQueueApp.Producer.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

class Program
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
                    services.Configure<ProducerConfig>(context.Configuration);

                    var queuePath = context.Configuration["QueuePath"] ?? string.Empty;
                    var deadLetterQueuePath = context.Configuration["DeadLetterQueuePath"] ?? string.Empty;

                    // Bind ProducerConfig instance
                    var producerConfig = new ProducerConfig();
                    context.Configuration.Bind(producerConfig);
                    services.AddSingleton(producerConfig);

                    services.AddSingleton<IMessageQueueClient>(sp =>
                        new MsmqClient(queuePath, deadLetterQueuePath, sp.GetRequiredService<ILogger>()));

                    services.AddTransient<IProducerService, ProducerService>();
                })
                .Build();

        var producer = host.Services.GetRequiredService<IProducerService>();
        await producer.RunAsync();
    }
}