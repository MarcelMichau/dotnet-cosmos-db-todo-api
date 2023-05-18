using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace CosmosTriggerFunctionIsolated
{
    public class Function1
    {
        private readonly ILogger _logger;

        public Function1(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<Function1>();
        }

        [Function("Function1")]
        public void Run([CosmosDBTrigger(
            databaseName: "TodoDB",
            containerName: "TodoContext",
            Connection = "CosmosConnection",
            LeaseContainerName = "leases")] IReadOnlyList<Todo> input)
        {
            if (input is not {Count: > 0}) return;
            _logger.LogInformation("Documents modified: {modifiedCount}", input.Count);
            _logger.LogInformation("First document Id: {documentId}", input[0].Id);
            _logger.LogInformation("Document Content: {document}", JsonSerializer.Serialize(input));
        }
    }

    public class Todo
    {
        public string Id { get; set; }
        public string Text { get; set; }
        public bool IsDone { get; set; }
        public string PartitionKey { get; set; }
    }
}
