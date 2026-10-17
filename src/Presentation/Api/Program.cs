using System.Text.Json.Serialization;
using ExpenseWise.DependencyConfiguration;
using Infrastructure.SqlDatabase;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Api.Authentication;
using System.IdentityModel.Tokens.Jwt;

var migrateOnly = args.Contains("--migrate", StringComparer.Ordinal);
var builderArgs = args.Where(argument => argument != "--migrate").ToArray();
var builder = WebApplication.CreateBuilder(builderArgs);

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
   .ConfigureDependencies(builder.Configuration)
   .RegisterUseCases();

if (!migrateOnly)
{
    var signingKey = AccessTokenIssuer.GetSigningKey(builder.Configuration);
    builder.Services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.MapInboundClaims = false;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = AccessTokenIssuer.Issuer,
                ValidateAudience = true,
                ValidAudience = AccessTokenIssuer.Audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = signingKey,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromSeconds(30),
                NameClaimType = JwtRegisteredClaimNames.Sub
            };
        });
    builder.Services.AddAuthorization();
    builder.Services.AddSingleton<AccessTokenIssuer>();
}

var app = builder.Build();

if (migrateOnly)
{
    var contextFactory = app.Services.GetRequiredService<IDbContextFactory<CoreContext>>();
    await using var context = await contextFactory.CreateDbContextAsync();
    await context.Database.MigrateAsync();
    return;
}

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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
