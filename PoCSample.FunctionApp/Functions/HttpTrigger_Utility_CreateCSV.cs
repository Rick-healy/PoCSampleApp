using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace PoCSample
{
    public class HttpTrigger_Utility_CreateCSV
    {
        private readonly ILogger<HttpTrigger_Utility_CreateCSV> _logger;

        public HttpTrigger_Utility_CreateCSV(ILogger<HttpTrigger_Utility_CreateCSV> logger)
        {
            _logger = logger;
        }

        [Function(nameof(HttpTrigger_Utility_CreateCSV))]
        [QueueOutput("outbound-csv-queue", Connection = "StorageConnectionString")]
        public string[] Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
        {
            _logger.LogInformation("HttpTrigger_Utility_CreateCSV: HTTP trigger function processed a request.");

            // Get correlation ID from query string, generate new one if not provided
            var correlationId = req.Query["correlationId"].FirstOrDefault() ?? Guid.NewGuid().ToString();
            _logger.LogInformation("Using correlation ID: {CorrelationId}", correlationId);
            
            // Create CSV rows with correlation ID
            var csvRows = new List<string>
            {
                "correlation_id,Name,Age,City",
                $"{correlationId},John Doe,30,London",
                $"{correlationId},Jane Smith,25,New York",
                $"{correlationId},Bob Johnson,35,Sydney"
            };

            // Log the CSV content
            var csvContent = string.Join(Environment.NewLine, csvRows);
            _logger.LogInformation("Generated CSV content:\n{CsvContent}", csvContent);

            // Skip header and return rows to be written to queue
            var queueMessages = csvRows.Skip(1).ToArray();
            foreach (var row in queueMessages)
            {
                _logger.LogInformation("Adding row to queue: {Row}", row);
            }
            
            _logger.LogInformation("CSV rows generated and will be sent to queue with correlation ID: {CorrelationId}", correlationId);
            return queueMessages;
        }
    }
}