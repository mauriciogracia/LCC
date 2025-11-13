using Microsoft.EntityFrameworkCore;
using Domain;
using Infrastructure.Repository;
using Infrastructure;
using Domain.Entities;

namespace UserTests
{
    public class UserRepositoryTests
    {
        private DbContextOptions<ReferralDbContext> CreateOptions(string dbName) =>
            new DbContextOptionsBuilder<ReferralDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

        [Fact]
        public async Task AddAsync_ShouldAddUserAndSave()
        {
            var options = CreateOptions(nameof(AddAsync_ShouldAddUserAndSave));
            using var context = new ReferralDbContext(options);
            var repository = new UserRepository(context);

            var user = new User { Uid = "3", Name = "Charlie" };
            await repository.AddAsync(user);

            var result = await context.Users.FindAsync("3");
            Assert.NotNull(result);
            Assert.Equal("Charlie", result.Name);
        }

        [Fact]
        public async Task DeleteAsync_ShouldRemoveUserIfFound()
        {
            var options = CreateOptions(nameof(DeleteAsync_ShouldRemoveUserIfFound));
            using var context = new ReferralDbContext(options);
            context.Users.Add(new User { Uid = "1", Name = "Alice" });
            await context.SaveChangesAsync();

            var repository = new UserRepository(context);
            await repository.DeleteAsync("1");

            var result = await context.Users.FindAsync("1");
            Assert.Null(result);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllUsers()
        {
            var options = CreateOptions(nameof(GetAllAsync_ShouldReturnAllUsers));
            using var context = new ReferralDbContext(options);
            context.Users.AddRange(
                new User { Uid = "1", Name = "Alice" },
                new User { Uid = "2", Name = "Bob" }
            );
            await context.SaveChangesAsync();

            var repository = new UserRepository(context);
            var result = await repository.GetAllAsync();

            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetByFilterAsync_ShouldReturnFilteredUsers()
        {
            var options = CreateOptions(nameof(GetByFilterAsync_ShouldReturnFilteredUsers));
            using var context = new ReferralDbContext(options);
            context.Users.AddRange(
                new User { Uid = "1", Name = "Alice" },
                new User { Uid = "2", Name = "Bob" }
            );
            await context.SaveChangesAsync();

            var repository = new UserRepository(context);
            var result = repository.GetByFilter(u => u.Name.StartsWith("A"));

            Assert.Single(result);
            Assert.Equal("Alice", result.First().Name);
        }
    }
}