using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace PoCSample.Functions.Test
{
    public class HttpTrigger_Test_NotificationReceiver
    {
        private readonly ILogger<HttpTrigger_Test_NotificationReceiver> _logger;

        public HttpTrigger_Test_NotificationReceiver(ILogger<HttpTrigger_Test_NotificationReceiver> logger)
        {
            _logger = logger;
        }

        [Function(nameof(HttpTrigger_Test_NotificationReceiver))]
        public void Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
        {
            var message = req.Query["message"].FirstOrDefault() ?? "no message received";
            
            _logger.LogInformation("NOTIFICATION RECEIVED: {Message}", message);
        }
    }
}