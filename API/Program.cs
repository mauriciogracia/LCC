
using Application.Interfaces;
using Application.Services;
using Application.Validators;
using Domain.Entities;
using Domain.Interfaces;
using FluentValidation;
using FluentValidation.AspNetCore;
using Infrastructure;
using Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace API
{
    public partial class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddScoped<ILog, ConsoleLogger>();
            builder.Services.AddScoped<IUtilFeatures, UtilService>();
            builder.Services.AddScoped<IUserFeatures, UserService>();
            builder.Services.AddScoped<IReferralFeatures, ReferralService>();
            builder.Services.AddScoped<IRepository<User>, UserRepository>();
            builder.Services.AddScoped<IRepository<Referral>, ReferralRepository>();

            builder.Services.AddDbContext<ReferralDbContext>(options =>
            options.UseInMemoryDatabase("ReferralDb"));

            builder.Services.AddControllers();


            // Add Swagger services
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Add all validators
            builder.Services.AddValidatorsFromAssemblyContaining<GetReferralRequestValidator>();
            builder.Services.AddValidatorsFromAssemblyContaining<PrepareMessageRequestValidator>();
            builder.Services.AddValidatorsFromAssemblyContaining<ReferralAddRequestValidator>();
            builder.Services.AddValidatorsFromAssemblyContaining<ReferralAttributionRequestValidator>();
            builder.Services.AddFluentValidationAutoValidation();

            configureJWT(builder);

            //Inject the precise implementation of Middleware
            builder.Services.AddScoped<ExceptionHandlingMiddleware>();

            var app = builder.Build();

            app.UseMiddleware<ExceptionHandlingMiddleware>();
            // Enable Swagger in development
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            else
            {
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }

        static void configureJWT(WebApplicationBuilder wab)
        {
            wab.Services.AddAuthentication("Bearer")
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = "yourIssuer",
                        ValidAudience = "yourAudience",
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes("yourSecretKey"))
                    };
                });
        }
    }
}
