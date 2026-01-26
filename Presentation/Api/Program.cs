using System.Text.Json.Serialization;
using ExpenseWise.DependencyConfiguration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi(options =>
{
    options.AddSchemaTransformer((schema, context, cancellationToken) =>
    {
        schema.AdditionalPropertiesAllowed = false;
        return Task.CompletedTask;
    });
});
// Configure Dependencies
builder.Services
   .ConfigureDependencies()
   .RegisterUseCases();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "v1");
    });
}

// TODO: Only enable HTTPS redirection when not in local environment
// app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
