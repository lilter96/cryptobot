using CryptoBot.API.Extensions;
using CryptoBot.Data;
using CryptoBot.Exchanges.Exchanges.Clients;
using CryptoBot.Service.Services.Implementations;
using CryptoBot.Service.Services.Interfaces;
using CryptoBot.TelegramBot;
using CryptoBot.TelegramBot.BotStates.Factory;
using CryptoBot.TelegramBot.CommandDetectors.Service;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;
using Telegram.Bot;

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
builder.Services.AddSingleton<CommandDetectorService>();

builder.Services.AddSingleton<ITelegramBotClient>(new TelegramBotClient(builder.Configuration["TelegramBotConfiguration:TelegramBotToken"]!));
builder.Services.AddTransient<TelegramBot>();

var detectorsTypes = AppDomain.CurrentDomain.GetAssemblies()
    .SelectMany(s => s.GetTypes())
    .Where(x => !x.IsInterface && x.IsAssignableTo(typeof(ICommandDetector)))
    .ToList();

foreach (var detector in detectorsTypes)
{
    builder.Services.AddTransient(detector);
};

builder.Services.AddTransient<IStateFactory, StateFactory>();
builder.Services.AddTransient<ICryptoService, CryptoService>();

// Must be LAST!!!
builder.Services.AddKeyedBotStates();

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
