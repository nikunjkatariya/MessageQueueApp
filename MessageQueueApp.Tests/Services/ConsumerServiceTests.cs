using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using MessageQueueApp.Common.Interfaces;
using MessageQueueApp.Common.Models;
using MessageQueueApp.Consumer.Configurations;
using MessageQueueApp.Consumer.Services;
using MessageQueueApp.Logging.Enums;
using MessageQueueApp.Logging.Interfaces;
using Moq;

namespace MessageQueueApp.Tests.Services
{
    public class ConsumerServiceTests
    {
        private readonly Mock<ILogger> _loggerMock;
        private readonly Mock<IMessageQueueClient> _queueClientMock;
        private readonly ConsumerConfig _config;
        private readonly ConsumerService _consumerService;

        public ConsumerServiceTests()
        {
            _loggerMock = new Mock<ILogger>();
            _queueClientMock = new Mock<IMessageQueueClient>();
            _config = new ConsumerConfig
            {
                MaxRetryCount = 2,
                DeadLetterWarningThreshold = 5,
                CheckIntervalSeconds = 1,
                QueuePath = @".\Private$\AppointmentQueue",
                DeadLetterQueuePath = @".\Private$\DeadLetterQueue"
            };
            _consumerService = new ConsumerService(_loggerMock.Object, _config, _queueClientMock.Object);
        }

        [Fact]
        public async Task RunAsync_Processes_Successful_Message_And_Logs_Properly()
        {
            // Arrange
            var message = new AppointmentMessage { Ticket = "TICK-0205-10101", Container = "CONT1859650", Message = "Appointment has been created for Ticket: TICK-0205-10101 and container: CONT1859650." };
            _queueClientMock.Setup(q => q.ReceiveMessageAsync()).ReturnsAsync(message);
            _queueClientMock.SetupSequence(q => q.ReceiveMessageAsync())
                .ReturnsAsync(message)
                .ReturnsAsync((AppointmentMessage)null); // End after one message

            // Override ProcessMessageAsync to simulate success
            var consumerService = new TestableConsumerService(_loggerMock.Object, _config, _queueClientMock.Object, true);

            // Act
            var runTask = consumerService.RunAsync();

            // Wait briefly to allow processing loop iteration
            await Task.Delay(500);

            consumerService.Stop();

            // Assert
            _loggerMock.Verify(l => l.Log(It.Is<string>(s => s.Contains("Watchlist API Success")), LogLevel.Info), Times.AtLeastOnce);
            _loggerMock.Verify(l => l.Log(It.IsAny<string>(), LogLevel.Error), Times.Never);
            _queueClientMock.Verify(q => q.MoveToDeadLetterQueue(It.IsAny<AppointmentMessage>()), Times.Never);
        }

        // Test subclass to control ProcessMessageAsync success/failure
        private class TestableConsumerService : ConsumerService
        {
            private bool _simulateSuccess;
            private bool _stopRequested;

            public TestableConsumerService(ILogger logger, ConsumerConfig config, IMessageQueueClient queueClient, bool simulateSuccess)
                : base(logger, config, queueClient)
            {
                _simulateSuccess = simulateSuccess;
                _stopRequested = false;
            }

            public new async Task RunAsync()
            {
                // Overridden to add stop condition to exit loop for testing
                while (!_stopRequested)
                {
                    await base.RunSingleIterationAsync();
                    await Task.Delay(100); // small delay to prevent tight loop in test
                }
            }

            public void Stop()
            {
                _stopRequested = true;
            }

            protected override Task<bool> ProcessTestMessageAsync(AppointmentMessage message)
            {
                return Task.FromResult(_simulateSuccess);
            }
        }

        [Fact]
        public async Task RunAsync_Processes_Failed_Message_Moves_To_DeadLetter_And_Logs()
        {
            // Arrange
            var message = new AppointmentMessage { Ticket = "TICK-0205-10102", Container = "CONT8585954", Message = "Appointment has been created for Ticket: TICK-0205-10102 and container: CONT8585954 - This message is marked to fail." };
            _queueClientMock.SetupSequence(q => q.ReceiveMessageAsync())
                .ReturnsAsync(message)
                .ReturnsAsync((AppointmentMessage)null); // End after one message

            // Override ProcessMessageAsync to simulate failure
            var consumerService = new TestableConsumerService(_loggerMock.Object, _config, _queueClientMock.Object, false);

            // Act
            var runTask = consumerService.RunAsync();

            // Wait briefly to allow processing loop iteration
            await Task.Delay(1500);

            consumerService.Stop();

            // Assert
            _loggerMock.Verify(l => l.Log(It.Is<string>(s => s.Contains("Message processing failed")), LogLevel.Warning), Times.Exactly(2)); // Two attempts
            _loggerMock.Verify(l => l.Log(It.Is<string>(s => s.Contains("Message moved to dead-letter queue")), LogLevel.Error), Times.Once);
        }
    }
}
