using System.Collections.Generic;
using System.Threading.Tasks;
using CreditApi.Models;

namespace CreditApi.Repositories
{
    public interface ICreditoRepository
    {
        Task AddAsync(Credito entity);
        Task<Credito?> GetByNumeroCreditoAsync(string numeroCredito);
        Task<List<Credito>> GetByNumeroNfseAsync(string numeroNfse);
        Task SaveChangesAsync();
    }
}
