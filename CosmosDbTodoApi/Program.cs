using Azure.Identity;
using CosmosDbTodoApi;
using Microsoft.EntityFrameworkCore;

var cosmosDbCredential = new DefaultAzureCredential();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<TodoContext>(options =>
    options.UseCosmos(
        builder.Configuration.GetValue<string>("CosmosDb:AccountEndpoint") ?? throw new InvalidOperationException(),
        cosmosDbCredential,
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

app.MapTodoEndpoints();
app.MapDatabaseEndpoints();

//using (var scope = app.Services.CreateScope())
//{
//    var db = scope.ServiceProvider.GetRequiredService<TodoContext>();
//    db.Database.EnsureCreated();
//}

app.Run();