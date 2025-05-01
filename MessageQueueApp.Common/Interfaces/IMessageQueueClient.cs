using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Experimental.System.Messaging;
using MessageQueueApp.Common.Models;

namespace MessageQueueApp.Common.Interfaces
{
    public interface IMessageQueueClient
    {
        void SendMessage(AppointmentMessage message);
        Task<AppointmentMessage?> ReceiveMessageAsync();
        void MoveToDeadLetterQueue(AppointmentMessage message);
        int GetDeadLetterMessageCount();
        void ClearDeadLetterQueue();
    }
}
