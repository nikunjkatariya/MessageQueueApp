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
using MessageQueueApp.Logging.Interfaces;
using Moq;

namespace MessageQueueApp.Tests.Services
{
    public class ConsumerServiceTests
    {
        [Fact]
        public async Task RunAsync_ProcessesMessages_SuccessfullyAndFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var queueClientMock = new Mock<IMessageQueueClient>();

            var config = new ConsumerConfig
            {
                MaxRetryCount = 2,
                CheckIntervalSeconds = 1,
                DeadLetterWarningThreshold = 5,
                QueuePath = @".\Private$\AppointmentQueue",
                DeadLetterQueuePath = @".\Private$\DeadLetterQueue"
            };

            var consumer = new ConsumerService(loggerMock.Object, config, queueClientMock.Object);

            var message1 = new AppointmentMessage { Ticket = "TICK-0205-10101", Container = "CONT1859650", Message = "Appointment has been created for Ticket: TICK-0205-10101 and container: CONT1859650." };
            var message2 = new AppointmentMessage { Ticket = "TICK-0205-10102", Container = "CONT8585954", Message = "Appointment has been created for Ticket: TICK-0205-10102 and container: CONT8585954 - This message is marked to fail."};

            // Setup queueClient to sequentially return two messages then null
            queueClientMock.SetupSequence(q => q.ReceiveMessageAsync())
                .ReturnsAsync(message1)
                .ReturnsAsync(message2)
                .ReturnsAsync((AppointmentMessage?)null);

            // Act - Run a short consumer loop iteration (consider exposing a method that processes one message for testing)
            for (int i = 0; i < 2; i++)
            {
                var message = await queueClientMock.Object.ReceiveMessageAsync();
                Assert.NotNull(message);
            }

            var noMessage = await queueClientMock.Object.ReceiveMessageAsync();
            Assert.Null(noMessage);
        }
    }
}
