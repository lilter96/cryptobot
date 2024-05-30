using CryptoBot.API.Settings;
using CryptoBot.Exchanges.Exchanges.Clients;
using CryptoExchange.Net.Authentication;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var configuration = builder.Configuration;

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
