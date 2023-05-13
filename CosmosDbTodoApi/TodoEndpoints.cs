using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace CosmosDbTodoApi;

internal static class TodoEndpoints
{
    internal static void MapTodoEndpoints(this WebApplication app)
    {
        var todosGroup = app.MapGroup("/todos");

        todosGroup.MapPost("/", CreateTodo)
            .WithName(nameof(CreateTodo))
            .WithOpenApi();

        todosGroup.MapGet("/", GetTodos)
            .WithName(nameof(GetTodos))
            .WithOpenApi();

        todosGroup.MapGet("/{id:guid}", GetTodo)
            .WithName(nameof(GetTodo))
            .WithOpenApi();

        todosGroup.MapPut("/{id:guid}", ToggleTodo)
            .WithName(nameof(ToggleTodo))
            .WithOpenApi();

        todosGroup.MapDelete("/{id:guid}", DeleteTodo)
            .WithName(nameof(DeleteTodo))
            .WithOpenApi();
    }

    private static async Task<Results<Ok, NotFound>> DeleteTodo(Guid id, TodoContext context)
    {
        var todoToDelete = await context.Todos.FindAsync(id, id.ToString());

        if (todoToDelete is null)
            return TypedResults.NotFound();

        context.Remove(todoToDelete);
        await context.SaveChangesAsync();

        return TypedResults.Ok();
    }

    private static async Task<Results<Ok<TodoModel>, NotFound>> ToggleTodo(Guid id, TodoContext context)
    {
        var todoToToggle = await context.Todos.FindAsync(id, id.ToString());

        if (todoToToggle is null)
            return TypedResults.NotFound();

        todoToToggle.IsDone = !todoToToggle.IsDone;
        await context.SaveChangesAsync();

        return TypedResults.Ok(MapToTodoModel(todoToToggle));
    }

    private static async Task<Results<Ok<TodoModel>, NotFound>> GetTodo(Guid id, TodoContext context)
    {
        var todo = await context.Todos.FindAsync(id, id.ToString());

        return todo is null ? TypedResults.NotFound() : TypedResults.Ok(MapToTodoModel(todo));
    }

    private static async Task<Ok<IEnumerable<TodoModel>>> GetTodos(TodoContext context)
    {
        var todos = await context.Todos.ToListAsync();

        return TypedResults.Ok(todos.Select(MapToTodoModel));
    }

    private static async Task<IResult> CreateTodo(CreateTodoModel todo, TodoContext context)
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

        return Results.CreatedAtRoute("GetTodo", new { id = newTodo.Id }, MapToTodoModel(newTodo));
    }

    private static TodoModel MapToTodoModel(Todo todo)
    {
        return new TodoModel(todo.Id, todo.Text, todo.IsDone);
    }
}

internal record TodoModel(Guid Id, string Text, bool IsDone);
internal record CreateTodoModel(string Text);
