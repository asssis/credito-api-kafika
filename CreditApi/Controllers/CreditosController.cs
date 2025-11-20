using System.Text.Json;
using CreditApi.Data;
using CreditApi.DTOs;
using CreditApi.Models;
using CreditApi.Services;
using CreditApi.Repositories;
using Microsoft.AspNetCore.Mvc;
using Confluent.Kafka;

namespace CreditApi.Controllers
{
    [ApiController]
    [Route("api/creditos")]
    public class CreditosController : ControllerBase
    {
        private readonly IMessagePublisher _publisher;
        private readonly ICreditoRepository _repo;
        private readonly ILogger<CreditosController> _logger;
        private const string Topic = "integrar-credito-constituido-entry";

        public CreditosController(IMessagePublisher publisher, ICreditoRepository repo, ILogger<CreditosController> logger)
        {
            _publisher = publisher;
            _repo = repo;
            _logger = logger;
        }

        [HttpPost("integrar-credito-constituido")]
        public async Task<IActionResult> Integrar([FromBody] List<CreditoDto> list)
        {
            if (list == null || !list.Any()) return BadRequest();

            foreach (var item in list)
            {
                var json = JsonSerializer.Serialize(item);
                await _publisher.PublishAsync(Topic, json);
            }

            return Accepted(new { success = true });
        }

        [HttpGet("{numeroNfse}")]
        public async Task<IActionResult> GetByNfse(string numeroNfse)
        {
            var items = await _repo.GetByNumeroNfseAsync(numeroNfse);
            var result = items.Select(c => new
            {
                c.NumeroCredito,
                c.NumeroNfse,
                c.DataConstituicao,
                c.ValorIssqn,
                c.TipoCredito,
                SimplesNacional = c.SimplesNacional ? "Sim" : "Não",
                c.Aliquota,
                c.ValorFaturado,
                c.ValorDeducao,
                c.BaseCalculo
            });
            return Ok(result);
        }

        [HttpGet("credito/{numeroCredito}")]
        public async Task<IActionResult> GetByCredito(string numeroCredito)
        {
            var c = await _repo.GetByNumeroCreditoAsync(numeroCredito);
            if (c == null) return NotFound();
            return Ok(new {
                c.NumeroCredito,
                c.NumeroNfse,
                c.DataConstituicao,
                c.ValorIssqn,
                c.TipoCredito,
                SimplesNacional = c.SimplesNacional ? "Sim" : "Não",
                c.Aliquota,
                c.ValorFaturado,
                c.ValorDeducao,
                c.BaseCalculo
            });
        }

        [HttpGet("self")]
        public IActionResult Self() => Ok(new { status = "ok" });

        [HttpGet("ready")]
        public IActionResult Ready() => Ok(new { status = "ready" });
    }
}
