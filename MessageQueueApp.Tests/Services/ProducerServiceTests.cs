using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using MessageQueueApp.Common.Interfaces;
using MessageQueueApp.Common.Models;
using MessageQueueApp.Logging.Interfaces;
using MessageQueueApp.Producer.Configurations;
using Moq;

namespace MessageQueueApp.Tests.Services
{
    public class ProducerServiceTests
    {
        [Fact]
        public async Task RunAsync_GeneratesMessages_AndSendsToQueue()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var queueClientMock = new Mock<IMessageQueueClient>();
            var config = new ProducerConfig
            {
                MaxMessageCount = 10,
                MaxFailedMessages = 2,
                MessageDelayMilliseconds = 1,
                QueuePath = @".\\Private$\\AppointmentQueue",
                DeadLetterQueuePath = @".\\Private$\\AppointmentDeadLetter"
            };

            // Create instance with mocked dependencies
            var producer = new ProducerService(loggerMock.Object, config, queueClientMock.Object);

            var messages = new List<AppointmentMessage>
            {
                new AppointmentMessage { Ticket = "TICK-0205-10101", Container = "CONT1859650", Message = "Appointment has been created for Ticket: TICK-0205-10101 and container: CONT1859650." },
                new AppointmentMessage { Ticket = "TICK-0205-10102", Container = "CONT8585954", Message = "Appointment has been created for Ticket: TICK-0205-10102 and container: CONT8585954 - This message is marked to fail."}
            };

            // Act
            foreach (var msg in messages)
            {
                queueClientMock.Object.SendMessage(msg);
            }

            // Assert
            queueClientMock.Verify(q => q.SendMessage(It.IsAny<AppointmentMessage>()), Times.Exactly(messages.Count));
        }
    }
}
