namespace CosmosDbTodoApi;

public class Todo
{
    public Guid Id { get; set; }
    public string Text { get; set; }
    public bool IsDone { get; set; }
    public string PartitionKey { get; set; }
}