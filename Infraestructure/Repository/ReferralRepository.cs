using Domain;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repository
{
    public class ReferralRepository : IRepository<Referral>
    {
        private readonly ReferralDbContext _context;
        private readonly DbSet<Referral> _set;

        public ReferralRepository(ReferralDbContext context)
        {
            _context = context;
            _set = context.Set<Referral>();
        }

        public async Task<Referral?> GetByIdAsync(string id)
        {
            return await _set.FindAsync(id);
        }

        public async Task<IEnumerable<Referral>> GetAllAsync()
        {
            return await _set.ToListAsync();
        }

        public async Task AddAsync(Referral entity)
        {
            _set.Add(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Referral entity)
        {
            _set.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(string id)
        {
            var entity = await _set.FindAsync(id);
            if (entity != null)
            {
                _set.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        public Task<IEnumerable<Referral>> GetByFilterAsync(Func<Referral, bool> predicate)
        {
            var result = _context.Referrals
                .AsEnumerable() // needed for Func to work ;-)
                .Where(predicate);

            return Task.FromResult(result);
        }
    }
}
