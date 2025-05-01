using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageQueueApp.Consumer.Interfaces
{
    public interface IConsumerService
    {
        Task RunAsync();
    }
}
