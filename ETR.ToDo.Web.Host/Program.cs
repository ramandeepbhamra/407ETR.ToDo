using ETR.ToDo.Services.Seeding;
using ETR.ToDo.Services.Core.Seeding;
using ETR.ToDo.Core.EF.Context;
using ETR.ToDo.Core.Repositories;
using ETR.ToDo.Core.EF.Repositories;
using ETR.ToDo.Services.Authentication;
using ETR.ToDo.Services.Core.Authentication;
using ETR.ToDo.Services.Core.Logging;
using ETR.ToDo.Services.Core.Tasks;
using ETR.ToDo.Services.Core.Users;
using ETR.ToDo.Services.Logging;
using ETR.ToDo.Services.Mapping;
using ETR.ToDo.Services.Tasks;
using ETR.ToDo.Services.Users;
using ETR.ToDo.Core.Shared.Configuration;
using ETR.ToDo.Web.Core.Extensions;
using Microsoft.EntityFrameworkCore;
using NSwag;
using Scalar.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Configuration
builder.Services.Configure<ApplicationSettings>(builder.Configuration.GetSection("Application"));
builder.Services.Configure<SessionSettings>(builder.Configuration.GetSection("Session"));

// Add services to the container
builder.Services.AddControllers();

// Database - SQLite (file-based, zero-setup)
builder.Services.AddDbContext<ToDoDbContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));

    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

// Repositories
builder.Services.AddScoped(typeof(IRepository<,>), typeof(Repository<,>));

// Services
builder.Services.AddScoped<IUserService, UserServices>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();
builder.Services.AddScoped<ITaskListService, TaskListService>();
builder.Services.AddScoped<ITodoTaskService, TodoTaskService>();
builder.Services.AddScoped<IApiLogService, ApiLogService>();
builder.Services.AddScoped<IDataSeeder, DataSeeder>();

// Mappers
builder.Services.AddScoped<UserMapper>();
builder.Services.AddScoped<TaskMapper>();
builder.Services.AddSingleton<ApiLogMapper>();

// Web.Core Infrastructure
builder.Services.AddWebCoreServices(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        var origins = builder.Configuration["Cors:Origins"]?.Split(',') ?? Array.Empty<string>();
        policy.WithOrigins(origins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// OpenAPI / Scalar (Modern .NET 10 API Documentation)
builder.Services.AddOpenApi();

// Health checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ToDoDbContext>("database");

// NSwag / Swagger (Alternative API Documentation)
builder.Services.AddOpenApiDocument(options =>
{
    options.Title = "ETR.ToDo API";
    options.Version = "v1";
    options.Description = "ETR.ToDo RESTful API with JWT Authentication";

    // Add JWT authentication to Swagger
    options.AddSecurity("Bearer", new NSwag.OpenApiSecurityScheme
    {
        Type = NSwag.OpenApiSecuritySchemeType.ApiKey,
        Name = "Authorization",
        In = NSwag.OpenApiSecurityApiKeyLocation.Header,
        Description = "Enter 'Bearer' [space] and then your JWT token",
        Scheme = "Bearer"
    });

    options.OperationProcessors.Add(new NSwag.Generation.Processors.Security.AspNetCoreOperationSecurityScopeProcessor("Bearer"));
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    // Scalar UI (Modern)
    app.MapOpenApi();
    app.MapScalarApiReference();  // Available at: /scalar/v1

    // NSwag / Swagger UI (Traditional)
    app.UseOpenApi();             // OpenAPI spec at: /swagger/v1/swagger.json
    app.UseSwaggerUi();            // Swagger UI at: /swagger
}

// Global exception handling
app.UseApiLogging();
app.UseGlobalExceptionHandler();

app.UseCors("AllowAll");

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

// Auto-apply migrations and seed data on startup
// Auto-apply migrations and seed data on startup (skip in test environment)
if (!app.Environment.IsEnvironment("Testing"))
{
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<ToDoDbContext>();

        // Apply pending migrations (creates database if it doesn't exist)
        await context.Database.MigrateAsync();

        // Seed initial data — idempotent, safe to run on every startup
        await scope.ServiceProvider
            .GetRequiredService<IDataSeeder>()
            .SeedAsync();
    }
}

app.Run();

// Make the Program class accessible to WebApplicationFactory for integration testing
public partial class Program { }
