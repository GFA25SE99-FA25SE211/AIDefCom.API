using AIDefCom.API.Mapper;
using AIDefCom.API.Middlewares;
using AIDefCom.Repository;
using AIDefCom.Repository.Entities;
using AIDefCom.Repository.UnitOfWork;
using AIDefCom.Service.Dto.Account;
using AIDefCom.Service.Services.EmailService;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Collections.Concurrent;
using System.Text;

namespace AIDefCom.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // 1?? Controller + Swagger
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();

            // ?? Swagger + JWT Config
            builder.Services.AddSwaggerGen(option =>
            {
                option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Enter token: Bearer {your token}",
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    BearerFormat = "JWT",
                    Scheme = "Bearer"
                });

                option.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] {}
                    }
                });
            });

            // 2?? Database
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // 3?? Identity
            builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
            {
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireDigit = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            // 4?? JWT Authentication
            var secretKey = builder.Configuration["AppSettings:Token"];
            if (string.IsNullOrEmpty(secretKey))
                throw new InvalidOperationException("JWT Secret Key is missing in appsettings.json.");

            var key = Encoding.UTF8.GetBytes(secretKey);

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = builder.Configuration["AppSettings:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = builder.Configuration["AppSettings:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuerSigningKey = true
                };
            })
            .AddGoogle(options =>
            {
                options.ClientId = builder.Configuration["Google:ClientId"];
                options.ClientSecret = builder.Configuration["Google:ClientSecret"];
                options.Scope.Add("https://www.googleapis.com/auth/calendar.events");
            });

            // 5?? Authorization
            builder.Services.AddAuthorization();

            // 6?? AutoMapper
            builder.Services.AddAutoMapper(typeof(AutoMapperProfile));

            // 7?? Cache & Session
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            // 8?? CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });

            // 9?? Email Service Config
            var emailConfig = builder.Configuration
                .GetSection("EmailConfiguration")
                .Get<EmailConfiguration>()
                ?? throw new InvalidOperationException("Missing Email Configuration in appsettings.json.");

            builder.Services.Configure<EmailConfiguration>(
                builder.Configuration.GetSection("EmailConfiguration"));

            builder.Services.AddSingleton(emailConfig);
            builder.Services.AddSingleton(new ConcurrentDictionary<string, OtpEntry>());
            builder.Services.AddTransient<IEmailService, EmailService>();

            // ?? Repository + UnitOfWork + Excel Import Service
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

            // ? (Tùy ch?n) Thêm AddProjectServices n?u b?n ?ã gom DI khác ? ?ó
            builder.Services.AddProjectServices();

            // ??? Build app
            var app = builder.Build();

            // 11?? Middleware pipeline
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // ??? Global Exception Handling Middleware - must be first
            app.UseMiddleware<ExceptionHandlingMiddleware>();

            app.UseSession();
            app.UseCors("AllowAll");
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
