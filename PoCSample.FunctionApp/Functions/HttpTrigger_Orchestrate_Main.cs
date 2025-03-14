using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Threading.Tasks;

namespace PoCSample
{
    public class HttpTrigger_Orchestrate_Main
    {
        private readonly ILogger<HttpTrigger_Orchestrate_Main> _logger;
        private readonly HttpClient _httpClient;

        public HttpTrigger_Orchestrate_Main(ILogger<HttpTrigger_Orchestrate_Main> logger)
        {
            _logger = logger;
            _httpClient = new HttpClient();
        }

        [Function("HttpTrigger_Orchestrate_Main")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
        {
            _logger.LogInformation("HttpTrigger_Orchestrate_Main: HTTP trigger function processed a request.");
            
            // Generate a random correlation ID
            var correlationId = Guid.NewGuid().ToString();
            _logger.LogInformation("Generated correlation ID: {CorrelationId}", correlationId);
            
            // Step 1: Call HttpTrigger_Utility_GetConfig
            _logger.LogInformation("Calling HttpTrigger_Utility_GetConfig...");
            var configUrl = "http://localhost:7072/api/HttpTrigger_Utility_GetConfig";
            var configResponse = await _httpClient.GetAsync(configUrl);
            
            if (configResponse.IsSuccessStatusCode)
            {
                _logger.LogInformation("HttpTrigger_Utility_GetConfig called successfully");
            }
            else
            {
                _logger.LogError("Error calling HttpTrigger_Utility_GetConfig. Status Code: {StatusCode}", configResponse.StatusCode);
            }
            
            // Step 2: Call HttpTrigger_Utility_CreateCSV with correlation ID
            _logger.LogInformation("Calling HttpTrigger_Utility_CreateCSV with correlation ID...");
            var createCsvUrl = $"http://localhost:7072/api/HttpTrigger_Utility_CreateCSV?correlationId={correlationId}";
            var createCsvResponse = await _httpClient.GetAsync(createCsvUrl);
            
            if (createCsvResponse.IsSuccessStatusCode)
            {
                _logger.LogInformation("HttpTrigger_Utility_CreateCSV called successfully");
            }
            else
            {
                _logger.LogError("Error calling HttpTrigger_Utility_CreateCSV. Status Code: {StatusCode}", createCsvResponse.StatusCode);
            }
            
            return new OkObjectResult($"Orchestration completed with correlation ID: {correlationId}");
        }
    }
}