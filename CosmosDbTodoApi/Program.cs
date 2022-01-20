using CosmosDbTodoApi;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<TodoContext>(options =>
    options.UseCosmos(
        builder.Configuration.GetValue<string>("CosmosDb:AccountEndpoint"),
        builder.Configuration.GetValue<string>("CosmosDb:AccountKey"),
        builder.Configuration.GetValue<string>("CosmosDb:DatabaseName")));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.DisplayRequestDuration();
    });
}

app.UseHttpsRedirection();

app.MapPost("/database", async ([FromServices] TodoContext context) => await context.Database.EnsureCreatedAsync())
    .WithName("CreateDatabase");

app.MapDelete("/database", async ([FromServices] TodoContext context) => await context.Database.EnsureDeletedAsync())
    .WithName("DropDatabase");

app.MapPost("/todos", async (CreateTodoModel todo, [FromServices] TodoContext context) =>
    {
        var key = Guid.NewGuid();
        
        var newTodo = new Todo
        {
            Id = key,
            Text = todo.Text,
            IsDone = false,
            PartitionKey = key.ToString()
        };

        context.Todos.Add(newTodo);

        await context.SaveChangesAsync();

        return Results.CreatedAtRoute("GetTodo", new { id = newTodo.Id }, MapToGetTodoModel(newTodo));
    })
    .WithName("CreateTodo");

app.MapGet("/todos", async ([FromServices] TodoContext context) =>
    {
        var todos = await context.Todos.ToListAsync();
        
        return Results.Ok(todos.Select(MapToGetTodoModel));
    })
    .WithName("GetTodos");

app.MapGet("/todos/{id}", async (Guid id, [FromServices] TodoContext context) =>
    {
        var todo = await context.Todos.FindAsync(id, id.ToString());
        
        return todo is null ? Results.NotFound() : Results.Ok(MapToGetTodoModel(todo));
    })
    .WithName("GetTodo");

app.MapPut("/todos/{id}", async (Guid id, [FromServices] TodoContext context) =>
    {
        var todoToToggle = await context.Todos.FindAsync(id, id.ToString());
        
        if (todoToToggle is null)
            return Results.NotFound();
        
        todoToToggle.IsDone = !todoToToggle.IsDone;
        await context.SaveChangesAsync();

        return Results.Ok(todoToToggle);
    })
    .WithName("ToggleTodo");

app.MapDelete("/todos/{id}", async (Guid id, [FromServices] TodoContext context) =>
    {
        var todoToDelete = await context.Todos.FindAsync(id, id.ToString());
        
        if (todoToDelete is null)
            return Results.NotFound();
        
        context.Remove(todoToDelete);
        await context.SaveChangesAsync();

        return Results.Ok();
    })
    .WithName("DeleteTodo");

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TodoContext>();
    db.Database.EnsureCreated();
}

app.Run();

static GetTodoModel MapToGetTodoModel(Todo todo)
{
    return new GetTodoModel(todo.Id, todo.Text, todo.IsDone);
} 

internal record GetTodoModel(Guid Id, string Text, bool IsDone);
internal record CreateTodoModel(string Text);

