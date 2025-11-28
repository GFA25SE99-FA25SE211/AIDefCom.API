using AIDefCom.API.Mapper;
using AIDefCom.API.Middlewares;
using AIDefCom.API.Hubs;
using AIDefCom.API.Swagger;
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
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Text;

namespace AIDefCom.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ---------- Logging providers (cho app) ----------
            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();

            // ---------- Startup logger (dùng trước khi Build) ----------
            using var startupLoggerFactory = LoggerFactory.Create(lb => lb.AddConsole());
            var startupLogger = startupLoggerFactory.CreateLogger("Startup");

            // 1) Controllers + Swagger
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(option =>
            {
                // Add custom schema filter for better examples
                option.SchemaFilter<SchemaExampleFilter>();

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

            // 2) Database
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // 3) Identity
            builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
            {
                // Password settings
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireDigit = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;

                // User settings - Allow spaces and special characters in username
                options.User.AllowedUserNameCharacters = 
                    "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+ ";
                options.User.RequireUniqueEmail = false; // Allow duplicate emails if needed
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            // 4) JWT Authentication (KHÔNG throw nếu thiếu – để app khởi động được)
            var secretKey = builder.Configuration["AppSettings:Token"];
            if (string.IsNullOrWhiteSpace(secretKey))
            {
                startupLogger.LogWarning("Missing AppSettings:Token – using TEMP secret (debug only).");
                secretKey = Guid.NewGuid().ToString(); // chỉ để khởi động; điền thật trên Azure
            }
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
                
                // Support JWT authentication for SignalR
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;
                        
                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                        {
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    }
                };
            })
            .AddGoogle(options =>
            {
                options.ClientId = builder.Configuration["Google:ClientId"];
                options.ClientSecret = builder.Configuration["Google:ClientSecret"];
                options.Scope.Add("https://www.googleapis.com/auth/calendar.events");
            });

            // 5) Authorization
            builder.Services.AddAuthorization();

            // 6) AutoMapper
            builder.Services.AddAutoMapper(typeof(AutoMapperProfile));

            // 7) Cache & Session
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            // 8) CORS - Updated for SignalR
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
                
                options.AddPolicy("SignalRPolicy", policy =>
                {
                    policy.WithOrigins("http://localhost:3000", "https://localhost:3000") // Add your frontend URLs
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials();
                });
            });

            // 9) SignalR
            builder.Services.AddSignalR();

            // 10) Email (optional — KHÔNG throw nếu thiếu)
            var emailSection = builder.Configuration.GetSection("EmailConfiguration");
            var emailConfig = emailSection.Get<EmailConfiguration>();
            if (emailConfig is null)
            {
                startupLogger.LogWarning("EmailConfiguration missing – email features disabled.");
            }
            else
            {
                builder.Services.Configure<EmailConfiguration>(emailSection);
                builder.Services.AddSingleton(emailConfig);
                builder.Services.AddSingleton(new ConcurrentDictionary<string, OtpEntry>());
                builder.Services.AddTransient<IEmailService, EmailService>();
            }

            // Repo + UoW + Services
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddProjectServices(builder.Configuration);

            // ---------- Build ----------
            var app = builder.Build();

            // ---------- Pipeline thứ tự chuẩn ----------
            
            // ⭐ Global Exception Handling Middleware - PHẢI ĐẶT ĐẦU TIÊN
            app.UseMiddleware<ExceptionHandlingMiddleware>();
            
            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseCors("AllowAll");
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseSession();

            // ---------- Endpoints debug ----------
            app.MapGet("/ping", () => "pong");
            app.MapGet("/__routes", (IEnumerable<EndpointDataSource> s) =>
            {
                var routes = s.SelectMany(x => x.Endpoints)
                              .OfType<RouteEndpoint>()
                              .Select(e => new
                              {
                                  Pattern = e.RoutePattern.RawText,
                                  Methods = string.Join(",", e.Metadata
                                      .OfType<HttpMethodMetadata>()
                                      .SelectMany(m => m.HttpMethods))
                              });
                return Results.Ok(routes);
            });

            // Controllers
            app.MapControllers();

            // SignalR Hubs
            app.MapHub<ScoreHub>("/hubs/score").RequireCors("SignalRPolicy");

            app.Run();
        }
    }
}
