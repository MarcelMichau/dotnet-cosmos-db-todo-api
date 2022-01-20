using Microsoft.EntityFrameworkCore;

namespace CosmosDbTodoApi;

public class TodoContext : DbContext
{
    public DbSet<Todo> Todos { get; set; }

    public TodoContext(DbContextOptions<TodoContext> options) : base(options)
    {

    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Todo>()
            .HasPartitionKey(t => t.PartitionKey);
    }
}