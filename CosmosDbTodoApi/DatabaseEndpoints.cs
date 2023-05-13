using Microsoft.AspNetCore.Mvc;

namespace CosmosDbTodoApi;

internal static class DatabaseEndpoints
{
    internal static void MapDatabaseEndpoints(this WebApplication app)
    {
        var databaseGroup = app.MapGroup("/database");

        databaseGroup.MapPost("/", async ([FromServices] TodoContext context) => await context.Database.EnsureCreatedAsync())
            .WithName("CreateDatabase")
            .WithOpenApi();

        databaseGroup.MapDelete("/", async ([FromServices] TodoContext context) => await context.Database.EnsureDeletedAsync())
            .WithName("DropDatabase")
            .WithOpenApi();
    }
}
