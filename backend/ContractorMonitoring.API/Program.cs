using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Asp.Versioning;
using ContractorMonitoring.API.Filters;
using ContractorMonitoring.API.Middleware;
using ContractorMonitoring.Application;
using ContractorMonitoring.Infrastructure;
using ContractorMonitoring.Domain.Constants;

//  Repository Pattern Registration 
using ContractorMonitoring.Application.Interfaces;
using ContractorMonitoring.Infrastructure.Repositories;

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
    // Add global validation filter
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

//  Register Repository for Dependency Injection 
builder.Services.AddScoped<IApprovalRepository, ApprovalRepository>();

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"]!;

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.Zero
    };

    // JWT events for additional validation
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
            {
                context.Response.Headers.Append("Token-Expired", "true");
            }
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            // Additional token validation if needed
            return Task.CompletedTask;
        }
    };
});

// Register Authorization Policies from Permission Constants
builder.Services.AddAuthorization(options =>
{
    var allPermissions = Permissions.GetAllPermissions();

    foreach (var permission in allPermissions)
    {
        options.AddPolicy(permission, policy =>
        {
            policy.RequireAssertion(context =>
            {
                // Log for debugging
                var claims = context.User.Claims.Select(c => $"{c.Type}:{c.Value}").ToList();
                System.Diagnostics.Debug.WriteLine($"Claims: {string.Join(", ", claims)}");

                // Check SuperAdmin by value
                var isSuperAdmin = context.User.Claims.Any(c => c.Value == "SuperAdmin");
                var isTest = context.User.Claims.Any(c => c.Value == "Test");

                if (isSuperAdmin || isTest)
                {
                    System.Diagnostics.Debug.WriteLine("SuperAdmin/Test bypass");
                    return true;
                }

                // Check permission
                var hasPermission = context.User.Claims.Any(c =>
                    c.Type == "Permission" && c.Value == permission);

                System.Diagnostics.Debug.WriteLine($"Permission {permission}: {hasPermission}");
                return hasPermission;
            });
        });
    }
});

// Configure Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Contractor Monitoring System API",
        Version = "v1",
        Description = "SaaS-based Government Contractor Monitoring System API",
        Contact = new OpenApiContact
        {
            Name = "Support Team",
            Email = "support@contractor.monitoring"
        }
    });

    // Add JWT Authentication to Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
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

    // Include XML comments (optional)
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

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

// Global exception handling middleware (should be first)
app.UseMiddleware<GlobalExceptionMiddleware>();

// Tenant middleware
app.UseMiddleware<TenantMiddleware>();

// Activity logging middleware - enterprise audit trail
app.UseMiddleware<ActivityLoggingMiddleware>();


app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Seed database on startup
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ContractorMonitoring.Infrastructure.Data.ApplicationDbContext>();
    await ContractorMonitoring.Infrastructure.Data.SeedDataService.SeedAsync(context);
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