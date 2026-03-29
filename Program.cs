using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using SystemResourceMonitorAPI.BackgroundJobs;
using SystemResourceMonitorAPI.Collectors;
using SystemResourceMonitorAPI.Data;
using SystemResourceMonitorAPI.Hubs;
using SystemResourceMonitorAPI.Services;
using SystemResourceMonitorAPI.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// ===== DATABASE CONFIGURATION =====
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsqlOptionsAction: sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorCodesToAdd: null);
        }));

// ===== COLLECTORS REGISTRATION (тільки на Windows/Development) =====
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddScoped<CpuCollector>();
    builder.Services.AddScoped<RamCollector>();
    builder.Services.AddScoped<ProcessCollector>();
    builder.Services.AddScoped<DiskCollector>();
    builder.Services.AddScoped<NetworkCollector>();
}

// ===== SERVICES REGISTRATION =====
builder.Services.AddScoped<IAuthService, AuthService>();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddScoped<ICpuMonitorService, CpuMonitorService>();
    builder.Services.AddScoped<IRamMonitorService, RamMonitorService>();
    builder.Services.AddScoped<IProcessMonitorService, ProcessMonitorService>();
    builder.Services.AddScoped<IDiskMonitorService, DiskMonitorService>();
    builder.Services.AddScoped<INetworkMonitorService, NetworkMonitorService>();
    builder.Services.AddScoped<IAlertService, AlertsService>();
    builder.Services.AddSingleton<IMetricsHistoryService, MetricsHistoryService>();
}

// ===== BACKGROUND SERVICE =====
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddHostedService<MetricsBackgroundService>();
}

// ===== SIGNALR =====
builder.Services.AddSignalR();

// ===== JWT AUTHENTICATION =====
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"]
    ?? throw new InvalidOperationException("JWT SecretKey is not configured in appsettings.json");

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
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"] ?? "SystemResourceMonitorAPI",
        ValidAudience = jwtSettings["Audience"] ?? "SystemResourceMonitorClient",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// ===== CORS CONFIGURATION =====
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
            "http://localhost:3000",
            "http://localhost:5173",
            "http://localhost:5174",
            "https://system-monitor-web-beta.vercel.app"
        )
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();
    });
});

// ===== CONTROLLERS + SWAGGER =====
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "System Resource Monitor API",
        Version = "v1.0",
        Description = "API для моніторингу системних ресурсів",
        Contact = new OpenApiContact
        {
            Name = "System Monitor",
            Email = "support@systemmonitor.com"
        }
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization. Enter 'Bearer' [space] and your token.",
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
});

// ===== LOGGING =====
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

// ===== DATABASE INITIALIZATION =====
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        context.Database.Migrate();
        DbInitializer.Initialize(context);
        app.Logger.LogInformation("Database initialized successfully");
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "An error occurred while initializing the database");
    }
}

// ===== MIDDLEWARE =====
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "System Resource Monitor API v1");
    options.RoutePrefix = "swagger";
});

app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

if (builder.Environment.IsDevelopment())
{
    app.MapHub<MetricsHub>("/hubs/metrics");
}

app.Logger.LogInformation("System Resource Monitor API v2.0 Started");

app.Run();