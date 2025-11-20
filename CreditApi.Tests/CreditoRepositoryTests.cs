using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CreditApi.Data;
using CreditApi.Models;
using CreditApi.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CreditApi.Tests
{
    public class CreditoRepositoryExtraTests
    {
        private AppDbContext GetContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        [Fact]
        public async Task GetByNumeroNfse_ReturnsAllMatching()
        {
            using var ctx = GetContext();
            var repo = new CreditoRepository(ctx);
        
            var c1 = new Credito { NumeroCredito = "1", NumeroNfse = "NF001", ValorIssqn = 10, TipoCredito = "ISSQN" };
            var c2 = new Credito { NumeroCredito = "2", NumeroNfse = "NF001", ValorIssqn = 20, TipoCredito = "ISSQN" };
            var c3 = new Credito { NumeroCredito = "3", NumeroNfse = "NF002", ValorIssqn = 30, TipoCredito = "ISSQN" };
        
            await repo.AddAsync(c1);
            await repo.AddAsync(c2);
            await repo.AddAsync(c3);
            await repo.SaveChangesAsync();
        
            var result = await repo.GetByNumeroNfseAsync("NF001");
            Assert.NotNull(result);
            Assert.Equal(2, ((ICollection<Credito>)result).Count);
        }
    }
}
