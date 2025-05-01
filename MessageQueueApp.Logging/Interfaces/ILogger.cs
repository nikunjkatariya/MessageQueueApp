using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MessageQueueApp.Logging.Enums;

namespace MessageQueueApp.Logging.Interfaces
{
    public interface ILogger
    {
        void Log(string message, LogLevel level);
    }
}
