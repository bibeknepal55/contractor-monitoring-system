using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Asp.Versioning;
using ContractorMonitoring.API.Filters;
using ContractorMonitoring.API.Middleware;
using ContractorMonitoring.Application;
using ContractorMonitoring.Infrastructure;
using ContractorMonitoring.Domain.Constants;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidationFilter>();
});

// Configure API Versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
}).AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

// Add layers
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"]!;

// Startup guard: fail if using default key in production
if (!builder.Environment.IsDevelopment() && secretKey.Contains("YourSuperSecret"))
{
    throw new InvalidOperationException("JWT SecretKey must be changed from the default value before deploying to production.");
}

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.Zero,
        RoleClaimType = "Role" // Use consistent claim type for roles
    };

    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
            {
                context.Response.Headers.Append("Token-Expired", "true");
            }
            return Task.CompletedTask;
        }
    };
});

// FIXED: Authorization - Structured role check, no string-match bypass, no Test backdoor
builder.Services.AddAuthorization(options =>
{
    var allPermissions = Permissions.GetAllPermissions();

    foreach (var permission in allPermissions)
    {
        options.AddPolicy(permission, policy =>
            policy.RequireAssertion(context =>
            {
                // Check role claim specifically (not any claim value)
                var roleClaims = context.User.FindAll(System.Security.Claims.ClaimTypes.Role)
                    .Select(c => c.Value)
                    .ToList();

                // Also check "Role" claim type for backward compatibility
                var altRoleClaims = context.User.FindAll("Role")
                    .Select(c => c.Value)
                    .ToList();

                var allRoles = roleClaims.Union(altRoleClaims).Distinct().ToList();

                // SuperAdmin bypasses all permission checks
                if (allRoles.Contains("SuperAdmin"))
                    return true;

                // Check for specific permission claim
                var permissionClaims = context.User.FindAll("Permission")
                    .Select(c => c.Value)
                    .ToList();

                return permissionClaims.Contains(permission);
            }));
    }
});

// FIXED: Rate Limiting for auth endpoints
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = 429;

    // Strict rate limit for auth endpoints
    options.AddFixedWindowLimiter("AuthPolicy", config =>
    {
        config.PermitLimit = 10;      // 10 requests
        config.Window = TimeSpan.FromMinutes(1);  // per minute
        config.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        config.QueueLimit = 2;
    });

    // General rate limit for API
    options.AddFixedWindowLimiter("ApiPolicy", config =>
    {
        config.PermitLimit = 100;
        config.Window = TimeSpan.FromMinutes(1);
        config.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        config.QueueLimit = 10;
    });
});

// Configure Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Contractor Monitoring System API",
        Version = "v1",
        Description = "Government Contractor Monitoring System API",
        Contact = new OpenApiContact
        {
            Name = "Support Team",
            Email = "support@contractor.monitoring"
        }
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            Array.Empty<string>()
        }
    });
});

// FIXED: CORS - Explicit allow-list per environment
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecific", policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            policy.WithOrigins("http://localhost:4200", "http://localhost:5185")
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        }
        else
        {
            // Production origins from configuration
            var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                          ?? new[] { "https://your-production-domain.com" };
            policy.WithOrigins(origins)
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        }
    });
});

// Add health checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Contractor Monitoring API V1");
        options.RoutePrefix = "swagger";
    });
}

app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseMiddleware<TenantMiddleware>();
app.UseMiddleware<ActivityLoggingMiddleware>();

app.UseHttpsRedirection();

app.UseCors("AllowSpecific");

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

// Health check endpoint
app.MapHealthChecks("/health");

app.MapControllers();

// Seed database on startup
using (var scope = app.Services.CreateScope())
{
    try
    {
        var context = scope.ServiceProvider.GetRequiredService<ContractorMonitoring.Infrastructure.Data.ApplicationDbContext>();
        await ContractorMonitoring.Infrastructure.Data.SeedDataService.SeedAsync(context);
        Log.Information("Database seeded successfully");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "An error occurred while seeding the database");
    }
}

try
{
    Log.Information("Starting Contractor Monitoring System API");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}