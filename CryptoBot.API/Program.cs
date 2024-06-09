using CryptoBot.API.Extensions;
using CryptoBot.Data;
using CryptoBot.Exchanges.Exchanges.Clients;
using CryptoBot.Service.Services.Cryptography;
using CryptoBot.TelegramBot;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("Logs/app.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<CryptoBotDbContext>(opt => opt.UseSqlServer(connectionString));

builder.Services.AddTransient<BybitApiClient>();

builder.Services.AddTransient<ICryptographyService, CryptographyService>();

builder.Services.AddTelegramBot(builder.Configuration);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        var dbContext = services.GetRequiredService<CryptoBotDbContext>();
        dbContext.Database.Migrate();
        logger.LogInformation("Database successfully have been migrated.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while migrating the database.");
    }

    var telegramBot = services.GetRequiredService<TelegramBot>();

    await telegramBot.StartReceivingMessagesAsync();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
Log.CloseAndFlush();
