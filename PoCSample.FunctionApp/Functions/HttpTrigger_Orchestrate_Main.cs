using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace PoCSample
{
    public class HttpTrigger_Orchestrate_Main
    {
        private readonly ILogger<HttpTrigger_Orchestrate_Main> _logger;

        public HttpTrigger_Orchestrate_Main(ILogger<HttpTrigger_Orchestrate_Main> logger)
        {
            _logger = logger;
        }

        [Function("HttpTrigger_Orchestrate_Main")]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
        {
            _logger.LogInformation("HttpTrigger_Orchestrate_Main: HTTP trigger function processed a request.");
            return new OkObjectResult("HttpTrigger_Orchestrate_Main Processed");
        }
    }
}