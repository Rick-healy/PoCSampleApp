using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace PoCSample
{
    public class HttpTrigger_Utility_GetConfig
    {
        private readonly ILogger<HttpTrigger_Utility_GetConfig> _logger;
        private readonly IConfiguration _configuration;

        public HttpTrigger_Utility_GetConfig(ILogger<HttpTrigger_Utility_GetConfig> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        [Function("HttpTrigger_Utility_GetConfig")]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
        {
            _logger.LogInformation("HttpTrigger_Utility_GetConfig: HTTP trigger function processed a request.");
            
            var keyEntry1 = _configuration["keyentry1"];
            var keyEntry2 = _configuration["keyentry2"];
            
            _logger.LogInformation("Config Values - KeyEntry1: {KeyEntry1}, KeyEntry2: {KeyEntry2}", keyEntry1, keyEntry2);
            
            return new OkObjectResult("HttpTrigger_Utility_GetConfig completed successfully");
        }
    }
}