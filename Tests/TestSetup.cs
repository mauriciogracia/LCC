using Application.Interfaces;
using Application.Services;
using Domain.Interfaces;
using Infrastructure;
using Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Tests
{
    public class TestSetup
    {
        public ILog _log;
        public IUtilFeatures util;
        public UserService userService;
        public UserRepository userRepo;
        public ReferralDbContext dbContext;

        public TestSetup()
        {
            _log = new ConsoleLogger();
            util = new UtilService();

            dbContext = prepareDbContext();
            dbContext.Database.EnsureCreated();

            userRepo = new UserRepository(dbContext);
            userService = prepareUserSetup(dbContext);
        }
        public ReferralDbContext prepareDbContext()
        {
            var options = new DbContextOptionsBuilder<ReferralDbContext>()
            .UseInMemoryDatabase(databaseName: "TestReferralDb")
            .Options;

            return new ReferralDbContext(options);
        }

        public UserService prepareUserSetup(ReferralDbContext dbContext)
        {
            
            var conf = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
        {"Jwt:Key", "testKey1234567890"},
                    {"Jwt:Issuer", "TestIssuer"},
                    {"Jwt:Audience", "TestAudience"}
                })
                .Build();

            var ts = new TokenService(conf, _log);
            return new UserService(userRepo, _log, util, ts);
        }
    }
}
