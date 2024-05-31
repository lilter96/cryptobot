using CryptoBot.Data;
using CryptoBot.Data.Entities;
using CryptoBot.Exchanges.Exchanges.Clients;
using CryptoBot.Service.Services.Implementations;
using CryptoBot.Service.Services.Interfaces;
using CryptoBot.TelegramBot;
using CryptoBot.TelegramBot.BotStates;
using CryptoBot.TelegramBot.CommandDetectors;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;

var builder = WebApplication.CreateBuilder(args);

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
}

builder.Services.AddKeyedTransient<IBotState, WaitingForCommandState>(BotState.WaitingForCommand);
builder.Services.AddKeyedTransient<IBotState, WaitingForSymbolState>(BotState.WaitingForSymbol);
builder.Services.AddKeyedTransient<IBotState, WaitingForSelectingExchangeState>(BotState.WaitingForSelectingExchange);
builder.Services.AddKeyedTransient<IBotState, WaitingForExchangeApiKeyState>(BotState.WaitingForExchangeApiKeyState);
builder.Services.AddKeyedTransient<IBotState, WaitingForExchangeApiSecretState>(BotState.WaitingForExchangeApiSecretState);


builder.Services.AddTransient<IStateFactory, StateFactory>();
builder.Services.AddTransient<ICryptoService, CryptoService>();

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
