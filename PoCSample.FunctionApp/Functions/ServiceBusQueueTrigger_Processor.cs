using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace PoCSample
{
    public class ServiceBusQueueTrigger_Processor
    {
        private readonly ILogger<ServiceBusQueueTrigger_Processor> _logger;

        public ServiceBusQueueTrigger_Processor(ILogger<ServiceBusQueueTrigger_Processor> logger)
        {
            _logger = logger;
        }

        [Function("ServiceBusQueueTrigger_Processor")]
        public void Run([ServiceBusTrigger("inbound_queue", Connection = "PocSampleServiceBus")] string message)
        {
            _logger.LogInformation("ServiceBusQueueTrigger_Processor function processed message: {message}", message);
        }
    }
}