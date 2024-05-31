using CryptoBot.API.Settings;
using CryptoBot.Data;
using CryptoBot.Data.Entities;
using CryptoBot.Exchanges.Exchanges.Clients;
using CryptoBot.TelegramBot;
using CryptoBot.TelegramBot.BotStates;
using CryptoBot.TelegramBot.CommandDetectors;
using CryptoExchange.Net.Authentication;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<CryptoBotDbContext>(opt => opt.UseSqlServer(connectionString));

var bybitApiSettings = builder.Configuration.GetSection("BybitApiCredentials").Get<BybitApiSettings>();

if (bybitApiSettings == null)
{
    throw new InvalidOperationException("Can't get BybitApiCredentials from config");
}

builder.Services.AddBybit(options =>
{
    options.ApiCredentials = new ApiCredentials(bybitApiSettings.Key, bybitApiSettings.Secret);
});

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
}

// var states = AppDomain.CurrentDomain.GetAssemblies()
//     .SelectMany(s => s.GetTypes())
//     .Where(x => !x.IsInterface && x.IsAssignableTo(typeof(IBotState)))
//     .ToList();
//
// foreach (var botState in states)
// {
//     builder.Services.AddSingleton(botState);
// }

builder.Services.AddKeyedTransient<IBotState, WaitingForCommandState>(BotState.WaitingForCommand);
builder.Services.AddKeyedTransient<IBotState, WaitingForSymbolState>(BotState.WaitingForSymbol);

builder.Services.AddTransient<IStateFactory, StateFactory>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
