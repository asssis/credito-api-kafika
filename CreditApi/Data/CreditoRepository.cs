using CreditApi.Models;
using CreditApi.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CreditApi.Data
{
    public class CreditoRepository : ICreditoRepository
    {
        private readonly AppDbContext _db;
        public CreditoRepository(AppDbContext db) => _db = db;

        public async Task AddAsync(Credito entity)
        {
            await _db.Creditos.AddAsync(entity);
        }

        public async Task<Credito?> GetByNumeroCreditoAsync(string numeroCredito)
        {
            return await _db.Creditos.AsNoTracking().FirstOrDefaultAsync(c => c.NumeroCredito == numeroCredito);
        }

        public async Task<List<Credito>> GetByNumeroNfseAsync(string numeroNfse)
        {
            return await _db.Creditos.AsNoTracking().Where(c => c.NumeroNfse == numeroNfse).ToListAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _db.SaveChangesAsync();
        }
    }
}
