using CreditApi.Data;
using CreditApi.Services;
using Microsoft.EntityFrameworkCore;
using Confluent.Kafka;
using CreditApi.Background;
using CreditApi.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Configuration
builder.Configuration.AddEnvironmentVariables();

// DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddSingleton<IProducer<Null, string>>(sp =>
{
    var config = new ProducerConfig
    {
        BootstrapServers = builder.Configuration["KAFKA:BootstrapServers"] ?? "localhost:9092",
        Acks = Acks.All,
        EnableIdempotence = true,
        MessageTimeoutMs = 5000
    };

    return new ProducerBuilder<Null, string>(config).Build();
});

builder.Services.AddSingleton<IMessagePublisher, KafkaMessagePublisher>();

builder.Services.AddSingleton(sp =>
    new ConsumerConfig
    {
        BootstrapServers = builder.Configuration["KAFKA:BootstrapServers"] ?? "localhost:9092",
        GroupId = builder.Configuration["KAFKA:GroupId"] ?? "creditos-consumer-group",
        AutoOffsetReset = AutoOffsetReset.Earliest
    });

builder.Services.AddScoped<ICreditoRepository, CreditoRepository>();

builder.Services.AddHostedService<KafkaCreditConsumerService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();
