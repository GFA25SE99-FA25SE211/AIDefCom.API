using AIDefCom.Repository.Repositories;
using AIDefCom.Repository.Repositories.AppUserRepository;
using AIDefCom.Repository.UnitOfWork;
using AIDefCom.Service.Services.AuthService;
using AIDefCom.Service.Services.EmailService;
using Microsoft.Extensions.DependencyInjection;

namespace AIDefCom.API
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddProjectServices(this IServiceCollection services)
        {
            // Repositories
            services.AddScoped<IAppUserRepository, AppUserRepository>();

            // Unit of Work
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Services
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IEmailService, EmailService>();

            return services;
        }
    }
}
