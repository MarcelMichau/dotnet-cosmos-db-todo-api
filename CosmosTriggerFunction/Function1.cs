using System.Collections.Generic;
using System.Text.Json;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace CosmosTriggerFunction
{
    public static class Function1
    {
        [FunctionName("Function1")]
        public static void Run([CosmosDBTrigger(
            databaseName: "TodoDB",
            containerName: "TodoContext",
            Connection = "CosmosConnectionString",
            LeaseContainerName = "leases",
            CreateLeaseContainerIfNotExists = true)]
            IReadOnlyList<Todo> input,
            ILogger log)
        {
            if (input is not {Count: > 0}) return;
            log.LogInformation("Documents modified: {modifiedCount}", input.Count);
            log.LogInformation("First document Id: {documentId}", input[0].Id);
            log.LogInformation("Document Content: {document}", JsonSerializer.Serialize(input));
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
