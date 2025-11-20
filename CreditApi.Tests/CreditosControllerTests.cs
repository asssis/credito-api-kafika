using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CreditApi.Controllers;
using CreditApi.DTOs;
using CreditApi.Models;
using CreditApi.Services;
using CreditApi.Repositories;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace CreditApi.Tests
{
    // --- Fakes / Test Doubles ---
    internal class FakeMessagePublisher : IMessagePublisher
    {
        public List<(string Topic, string Message)> Published { get; } = new();

        public Task PublishAsync(string topic, string message)
        {
            Published.Add((topic, message));
            return Task.CompletedTask;
        }
    }

    internal class FakeCreditoRepository : ICreditoRepository
    {
        public Func<string, Task<Credito?>> GetByNumeroCreditoAsyncFunc { get; set; } = _ => Task.FromResult<Credito?>(null);
        public Func<string, Task<List<Credito>>> GetByNumeroNfseAsyncFunc { get; set; } = _ => Task.FromResult(new List<Credito>());
        public Func<Credito, Task> AddAsyncFunc { get; set; } = _ => Task.CompletedTask;
        public Func<Task> SaveChangesAsyncFunc { get; set; } = () => Task.CompletedTask;

        public Task AddAsync(Credito entity) => AddAsyncFunc(entity);
        public Task<Credito?> GetByNumeroCreditoAsync(string numeroCredito) => GetByNumeroCreditoAsyncFunc(numeroCredito);
        public Task<List<Credito>> GetByNumeroNfseAsync(string numeroNfse) => GetByNumeroNfseAsyncFunc(numeroNfse);
        public Task SaveChangesAsync() => SaveChangesAsyncFunc();
    }

    public class CreditosControllerTests
    {
        private const string Topic = "integrar-credito-constituido-entry";

        [Fact]
        public async Task Integrar_WithNullOrEmpty_ReturnsBadRequest()
        {
            var mockPublisher = new FakeMessagePublisher();
            var mockRepo = new FakeCreditoRepository();
            var logger = new Microsoft.Extensions.Logging.Abstractions.NullLogger<CreditosController>();

            var controller = new CreditosController(mockPublisher, mockRepo, logger);

            var nullResult = await controller.Integrar(null);
            Assert.IsType<BadRequestResult>(nullResult);

            var emptyResult = await controller.Integrar(new List<CreditoDto>());
            Assert.IsType<BadRequestResult>(emptyResult);
        }

        [Fact]
        public async Task Integrar_PublishesEachMessage_ReturnsAccepted()
        {
            var publisher = new FakeMessagePublisher();
            var mockRepo = new FakeCreditoRepository();
            var logger = new Microsoft.Extensions.Logging.Abstractions.NullLogger<CreditosController>();

            var controller = new CreditosController(publisher, mockRepo, logger);

            var list = new List<CreditoDto>
            {
                new CreditoDto { NumeroCredito = "C1", NumeroNfse = "N1", ValorIssqn = 10 },
                new CreditoDto { NumeroCredito = "C2", NumeroNfse = "N2", ValorIssqn = 20 }
            };

            var result = await controller.Integrar(list);
            var accepted = Assert.IsType<AcceptedResult>(result);
            Assert.NotNull(accepted.Value);

            Assert.Equal(list.Count, publisher.Published.Count);
            Assert.All(publisher.Published, p => Assert.Equal(Topic, p.Topic));

            var expectedJson = JsonSerializer.Serialize(list[0]);
            Assert.Contains(publisher.Published, p => p.Topic == Topic && p.Message == expectedJson);
        }

        [Fact]
        public async Task GetByCredito_NotFound_ReturnsNotFound()
        {
            var publisher = new FakeMessagePublisher();
            var mockRepo = new FakeCreditoRepository
            {
                GetByNumeroCreditoAsyncFunc = numero => Task.FromResult<Credito?>(null)
            };
            var logger = new Microsoft.Extensions.Logging.Abstractions.NullLogger<CreditosController>();

            var controller = new CreditosController(publisher, mockRepo, logger);

            var result = await controller.GetByCredito("MISSING");
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetByCredito_Found_ReturnsOkWithDto()
        {
            var publisher = new FakeMessagePublisher();
            var credito = new Credito
            {
                NumeroCredito = "TEST1",
                NumeroNfse = "NF1",
                DataConstituicao = DateTime.UtcNow,
                ValorIssqn = 50,
                TipoCredito = "ISSQN",
                SimplesNacional = false,
                Aliquota = 2,
                ValorFaturado = 1000,
                ValorDeducao = 10,
                BaseCalculo = 990
            };
            var mockRepo = new FakeCreditoRepository
            {
                GetByNumeroCreditoAsyncFunc = numero => Task.FromResult<Credito?>(credito)
            };
            var logger = new Microsoft.Extensions.Logging.Abstractions.NullLogger<CreditosController>();

            var controller = new CreditosController(publisher, mockRepo, logger);

            var actionResult = await controller.GetByCredito("TEST1");
            var ok = Assert.IsType<OkObjectResult>(actionResult);
            var payload = ok.Value;
            Assert.NotNull(payload);
        }

        [Fact]
        public async Task GetByNfse_ReturnsList()
        {
            var publisher = new FakeMessagePublisher();
            var items = new List<Credito>
            {
                new Credito { NumeroCredito = "A", NumeroNfse = "NFX", ValorIssqn = 10 },
                new Credito { NumeroCredito = "B", NumeroNfse = "NFX", ValorIssqn = 20 }
            };
            var mockRepo = new FakeCreditoRepository
            {
                GetByNumeroNfseAsyncFunc = numero => Task.FromResult(items)
            };
            var logger = new Microsoft.Extensions.Logging.Abstractions.NullLogger<CreditosController>();

            var controller = new CreditosController(publisher, mockRepo, logger);

            var actionResult = await controller.GetByNfse("NFX");
            var ok = Assert.IsType<OkObjectResult>(actionResult);
            var value = ok.Value;
            Assert.NotNull(value);

            var enumerable = ok.Value as System.Collections.IEnumerable;
            Assert.NotNull(enumerable);
            Assert.Equal(2, enumerable.Cast<object>().Count());
        }

        [Fact]
        public void Self_ReturnsOk()
        {
            var mockPublisher = new Mock<IMessagePublisher>();
            var mockRepo = new Mock<ICreditoRepository>();
            var logger = new Mock<Microsoft.Extensions.Logging.ILogger<CreditosController>>();
            var controller = new CreditosController(mockPublisher.Object, mockRepo.Object, logger.Object);
        
            var result = controller.Self();
            var ok = Assert.IsType<OkObjectResult>(result);

            var value = ok.Value;
            string status = null;
        
            if (value is string s) status = s;
            else
            {
                var prop = value?.GetType().GetProperty("status");
                if (prop != null)
                    status = prop.GetValue(value)?.ToString();
                else if (value is System.Text.Json.JsonElement je && je.ValueKind == System.Text.Json.JsonValueKind.Object)
                {
                    if (je.TryGetProperty("status", out var p))
                        status = p.GetString();
                }
            }
        
            Assert.Equal("ok", status);
        }

        [Fact]
        public void Ready_ReturnsOk()
        {
            var mockPublisher = new Mock<IMessagePublisher>();
            var mockRepo = new Mock<ICreditoRepository>();
            var logger = new Mock<Microsoft.Extensions.Logging.ILogger<CreditosController>>();
            var controller = new CreditosController(mockPublisher.Object, mockRepo.Object, logger.Object);

            var result = controller.Ready();
            var ok = Assert.IsType<OkObjectResult>(result);

            var value = ok.Value;
            string status = null;

            if (value is string s) status = s;
            else
            {
                var prop = value?.GetType().GetProperty("status");
                if (prop != null)
                    status = prop.GetValue(value)?.ToString();
                else if (value is System.Text.Json.JsonElement je && je.ValueKind == System.Text.Json.JsonValueKind.Object)
                {
                    if (je.TryGetProperty("status", out var p))
                        status = p.GetString();
                }
            }

            Assert.Equal("ready", status);
        }
    }
}
