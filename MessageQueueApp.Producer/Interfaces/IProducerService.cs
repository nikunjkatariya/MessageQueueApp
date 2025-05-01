using System.Threading.Tasks;

namespace MessageQueueApp.Producer.Interfaces
{
    public interface IProducerService
    {
        Task RunAsync();
    }
}