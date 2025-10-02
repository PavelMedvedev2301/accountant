using Serilog;
using Motify.Server.Api;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("logs/tbx-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    Log.Information("Starting Trial Balance Classifier Server");

    var builder = WebApplication.CreateBuilder(args);
    
    // Add Serilog
    builder.Host.UseSerilog();

    // Add services
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
    });

    var app = builder.Build();

    // Configure middleware
    app.UseCors();
    app.UseSerilogRequestLogging();

    // Serve static files from wwwroot
    app.UseDefaultFiles();
    app.UseStaticFiles();

    // Map API endpoints
    ApiService.MapApiEndpoints(app);

    // Root redirect
    app.MapGet("/", () => Results.Redirect("/index.html"));

    var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
    app.Run($"http://0.0.0.0:{port}");
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

