using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageQueueApp.Common.Models
{
    public class AppointmentMessage
    {
        public string Ticket { get; set; }
        public string Container { get; set; }
        public string Message { get; set; }

        public override string ToString()
        {
            return Message ?? $"Appointment has been created for Ticket: {Ticket} and container: {Container}";
        }
    }
}
