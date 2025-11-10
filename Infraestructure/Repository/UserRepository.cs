using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Linq.Expressions;

namespace Infrastructure.Repository
{
    public class UserRepository : IRepository<User>
    {
        private readonly ReferralDbContext _context;
        public UserRepository(ReferralDbContext context) => _context = context;

        public async Task<User?> GetByIdAsync(string id) => await _context.Users.FindAsync(id);
        public async Task<IEnumerable<User>> GetAllAsync() => await _context.Users.ToListAsync();
        public async Task AddAsync(User entity) { _context.Users.Add(entity); await _context.SaveChangesAsync(); }
        public async Task UpdateAsync(User entity) { _context.Users.Update(entity); await _context.SaveChangesAsync(); }
        public async Task DeleteAsync(string id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null) { _context.Users.Remove(user); await _context.SaveChangesAsync(); }
        }

        public IQueryable<User> GetByFilter(Expression<Func<User, bool>> predicate)
        {
            return _context.Users.Where(predicate);
        }
    }
}
