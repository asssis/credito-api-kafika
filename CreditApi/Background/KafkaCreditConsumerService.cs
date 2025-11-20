using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using CreditApi.Data;
using CreditApi.Models;
using CreditApi.Converters;
using CreditApi.Repositories;

namespace CreditApi.Background
{
    public class KafkaCreditConsumerService : BackgroundService
    {
        private readonly ILogger<KafkaCreditConsumerService> _logger;
        private readonly ConsumerConfig _consumerConfig;
        private readonly IServiceScopeFactory _scopeFactory;
        private const string Topic = "integrar-credito-constituido-entry";

        public KafkaCreditConsumerService(
            ILogger<KafkaCreditConsumerService> logger,
            ConsumerConfig consumerConfig,
            IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _consumerConfig = consumerConfig;
            _scopeFactory = scopeFactory;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.Run(() => RunConsumerLoop(stoppingToken), CancellationToken.None);
        }

        private void RunConsumerLoop(CancellationToken stoppingToken)
        {
            using var consumer = new ConsumerBuilder<Ignore, string>(_consumerConfig).Build();

            try
            {
                consumer.Subscribe(Topic);
                _logger.LogInformation("Kafka consumer subscribed to {topic}", Topic);

                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        var cr = consumer.Consume(stoppingToken);
                        if (cr?.Message?.Value is null) continue;

                        _logger.LogInformation("Mensagem recebida: {offset} {value}", cr.Offset, cr.Message.Value);

                        try
                        {
                            ProcessMessageAsync(cr.Message.Value, stoppingToken).GetAwaiter().GetResult();

                            consumer.Commit(cr);
                        }
                        catch (Exception exProcess)
                        {
                            _logger.LogError(exProcess, "Erro processando mensagem. Mensagem NÃO confirmada (commit).");
                        }
                    }
                    catch (OperationCanceledException) { break; }
                    catch (ConsumeException cex)
                    {
                        _logger.LogError(cex, "Erro ao consumir mensagem Kafka: {reason}", cex.Error.Reason);
                        Task.Delay(1000, stoppingToken).GetAwaiter().GetResult();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Erro inesperado no loop de consumo.");
                        Task.Delay(1000, stoppingToken).GetAwaiter().GetResult();
                    }
                }
            }
            finally
            {
                try { consumer.Close(); } catch (Exception ex) { _logger.LogWarning(ex, "Erro ao fechar consumer"); }
                _logger.LogInformation("Kafka consumer finalizado.");
            }
        }

        private async Task ProcessMessageAsync(string messageValue, CancellationToken cancellationToken)
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            options.Converters.Add(new BooleanStringConverter());

            Credito? credito = null;
            try
            {
                credito = JsonSerializer.Deserialize<Credito>(messageValue, options);
                if (credito is null)
                {
                    _logger.LogWarning("Mensagem desserializada como nula: {message}", messageValue);
                    return;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha ao desserializar mensagem: {message}", messageValue);
                return;
            }

            if (credito.DataConstituicao != default(DateTime))
            {
                credito.DataConstituicao = DateTime.SpecifyKind(credito.DataConstituicao, DateTimeKind.Utc);
            }

            using var scope = _scopeFactory.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<ICreditoRepository>();

            try
            {
                var existing = await repo.GetByNumeroCreditoAsync(credito.NumeroCredito);
                if (existing != null)
                {
                    _logger.LogInformation("Crédito já existente: {numero}. Ignorando.", credito.NumeroCredito);
                    return;
                }

                await repo.AddAsync(credito);
                await repo.SaveChangesAsync();

                _logger.LogInformation("Crédito {numero} persistido com sucesso.", credito.NumeroCredito);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao salvar crédito no banco.");
                throw;
            }
        }
    }
}
