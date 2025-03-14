using System.Net.Http.Json;
using System.Text;
using Azure.Storage.Queues;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace PoCSample
{
    public class HttpTrigger_Outbound_Email
    {
        private readonly ILogger<HttpTrigger_Outbound_Email> _logger;
        private readonly HttpClient _httpClient;
        private readonly QueueServiceClient _queueServiceClient;
        private readonly BlobServiceClient _blobServiceClient;

        public HttpTrigger_Outbound_Email(ILogger<HttpTrigger_Outbound_Email> logger)
        {
            _logger = logger;
            _httpClient = new HttpClient();
            
            // Initialize Azure Storage clients
            var storageConnectionString = Environment.GetEnvironmentVariable("StorageConnectionString");
            _queueServiceClient = new QueueServiceClient(storageConnectionString);
            _blobServiceClient = new BlobServiceClient(storageConnectionString);
        }

        [Function(nameof(HttpTrigger_Outbound_Email))]
        public async Task Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
        {
            var correlationId = req.Query["correlationId"].FirstOrDefault();
            if (string.IsNullOrEmpty(correlationId))
            {
                _logger.LogError("No correlationId provided in request");
                return;
            }

            _logger.LogInformation("Processing request for correlationId: {CorrelationId}", correlationId);

            // Get queue reference and messages
            var queueClient = _queueServiceClient.GetQueueClient("outbound-csv-queue");
            await queueClient.CreateIfNotExistsAsync();

            var matchingMessages = new List<string>();
            var messages = await queueClient.ReceiveMessagesAsync(maxMessages: 32);
            var messageList = messages.Value;
            
            if (!messageList.Any())
            {
                _logger.LogInformation("No messages in queue");
                return;
            }

            // Process each message once
            foreach (var message in messageList)
            {
                // Decode the base64 message
                var decodedMessage = string.Empty;
                try
                {
                    var bytes = Convert.FromBase64String(message.MessageText);
                    decodedMessage = Encoding.UTF8.GetString(bytes);
                    _logger.LogInformation("Decoded message: {DecodedMessage}", decodedMessage);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("Failed to decode message as base64: {Message}. Error: {Error}", 
                        message.MessageText, ex.Message);
                    // If decoding fails, try using the raw message
                    decodedMessage = message.MessageText;
                }

                // Split the decoded message and check if the first part matches the correlation ID exactly
                var parts = decodedMessage.Split(',');
                if (parts.Length > 0 && parts[0] == correlationId)
                {
                    matchingMessages.Add(decodedMessage);
                    await queueClient.DeleteMessageAsync(message.MessageId, message.PopReceipt);
                    _logger.LogInformation("Found and deleted matching message: {Message}", decodedMessage);
                }
                else
                {
                    // For non-matching messages, set visibility timeout to 30 seconds
                    await queueClient.UpdateMessageAsync(message.MessageId, message.PopReceipt, 
                        visibilityTimeout: TimeSpan.FromSeconds(30));
                    _logger.LogInformation("Message did not match correlation ID, making visible again in 30s: {Message}", decodedMessage);
                }
            }

            if (!matchingMessages.Any())
            {
                _logger.LogInformation("No messages found for correlationId: {CorrelationId}", correlationId);
                return;
            }

            // Create blob with found messages
            var blobContainerClient = _blobServiceClient.GetBlobContainerClient("outbound");
            await blobContainerClient.CreateIfNotExistsAsync();

            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            var blobName = $"{correlationId}_{timestamp}.txt";
            var blobClient = blobContainerClient.GetBlobClient(blobName);

            var content = string.Join(Environment.NewLine, matchingMessages);
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(content)))
            {
                await blobClient.UploadAsync(stream, overwrite: true);
            }

            _logger.LogInformation("Created blob: {BlobName}", blobName);

            // Send notification
            var notificationUrl = "http://localhost:7072/api/HttpTrigger_Test_NotificationReceiver";
            var notificationMessage = $"Dear Customer TEST, you have a new file waiting for your download at location ABD.COM/storage/{blobName}";
            
            try
            {
                var response = await _httpClient.GetAsync($"{notificationUrl}?message={Uri.EscapeDataString(notificationMessage)}");
                response.EnsureSuccessStatusCode();
                _logger.LogInformation("Notification sent successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending notification");
            }
        }
    }
}