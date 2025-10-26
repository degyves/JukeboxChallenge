using FluentValidation.AspNetCore;
using PartyJukebox.Api.Configuration;
using PartyJukebox.Api.Infrastructure;
using PartyJukebox.Api.Repositories;
using PartyJukebox.Api.Services;
using PartyJukebox.Api.SignalR;

var builder = WebApplication.CreateBuilder(args);

var mongoOptions = builder.Configuration.GetSection(MongoOptions.SectionName).Get<MongoOptions>()
                   ?? throw new ArgumentNullException(nameof(MongoOptions), "Mongo configuration section is missing.");
ValidateRequired(mongoOptions.ConnectionString, $"{MongoOptions.SectionName}:{nameof(MongoOptions.ConnectionString)}");
ValidateRequired(mongoOptions.Database, $"{MongoOptions.SectionName}:{nameof(MongoOptions.Database)}");

var redisOptions = builder.Configuration.GetSection(RedisOptions.SectionName).Get<RedisOptions>()
                   ?? throw new ArgumentNullException(nameof(RedisOptions), "Redis configuration section is missing.");
ValidateRequired(redisOptions.ConnectionString, $"{RedisOptions.SectionName}:{nameof(RedisOptions.ConnectionString)}");

var streamOptions = builder.Configuration.GetSection(StreamClientOptions.SectionName).Get<StreamClientOptions>()
                    ?? throw new ArgumentNullException(nameof(StreamClientOptions), "Stream configuration section is missing.");
ValidateRequired(streamOptions.BaseUrl, $"{StreamClientOptions.SectionName}:{nameof(StreamClientOptions.BaseUrl)}");

builder.Services.Configure<MongoOptions>(builder.Configuration.GetSection(MongoOptions.SectionName));
builder.Services.Configure<RedisOptions>(builder.Configuration.GetSection(RedisOptions.SectionName));
builder.Services.Configure<RateLimitOptions>(builder.Configuration.GetSection(RateLimitOptions.SectionName));
builder.Services.Configure<StreamClientOptions>(builder.Configuration.GetSection(StreamClientOptions.SectionName));

builder.Services.AddSingleton<MongoDbContext>();
builder.Services.AddSingleton<IRedisConnectionFactory, RedisConnectionFactory>();
builder.Services.AddHostedService<MongoInitializer>();

builder.Services.AddScoped<IRoomRepository, RoomRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ITrackRepository, TrackRepository>();
builder.Services.AddScoped<IVoteRepository, VoteRepository>();

builder.Services.AddSingleton<IRoomCodeGenerator, RoomCodeGenerator>();
builder.Services.AddScoped<IRoomService, RoomService>();
builder.Services.AddScoped<ITrackService, TrackService>();
builder.Services.AddSingleton<IQueueOrderingService, QueueOrderingService>();
builder.Services.AddSingleton<IRateLimitService, RateLimitService>();
builder.Services.AddHttpClient<IStreamClient, StreamClient>();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();

builder.Services.AddCors(options =>
{
    options.AddPolicy("default", policy =>
    {
        var origins = builder.Configuration.GetValue<string>("CorsOrigin")?.Split(',', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
        policy.WithOrigins(origins.Length == 0 ? ["http://localhost:5173"] : origins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

app.UseCors("default");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.MapHub<RoomHub>(RoomHub.HubPath);

app.Run();

static void ValidateRequired(string? value, string key)
{
    if (string.IsNullOrWhiteSpace(value))
    {
        throw new ArgumentNullException(key, $"{key} configuration value cannot be null or empty.");
    }
}

// Make the implicit Program class public for test access.
public partial class Program;
