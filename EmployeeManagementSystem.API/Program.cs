using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using EmployeeManagementSystem.Infrastructure.Context;
using EmployeeManagementSystem.Infrastructure.UnitOfWork;
using EmployeeManagementSystem.Infrastructure.Services;
using EmployeeManagementSystem.Application.Interfaces;
using EmployeeManagementSystem.Application.Services;


var builder = WebApplication.CreateBuilder(args);

// Add DbContext
builder.Services.AddDbContext<EmployeeManagementSystemDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add services to the container.
builder.Services.AddControllers();

// Repository pattern
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Business services
builder.Services.AddScoped<IAuthService, AuthService>();

// Database seeder service
builder.Services.AddHostedService<DatabaseSeederService>();

// JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? "default-secret-key-that-should-be-changed-in-production";
var key = Encoding.ASCII.GetBytes(jwtKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// Health checks
builder.Services.AddHealthChecks()
    .AddSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")!, name: "database");

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:4200", "http://localhost:5173", "http://localhost:8080")
               .AllowAnyMethod()
               .AllowAnyHeader()
               .AllowCredentials();
    });
});

// Swagger/OpenAPI configuration
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Employee Management System API",
        Version = "v1",
        Description = "A Employee Management System with role-based access control",
        Contact = new OpenApiContact
        {
            Name = "Team Aegis"
        }
    });

    // Add JWT authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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

var app = builder.Build();

// Configure the HTTP request pipeline
// Configure Swagger for all environments (can be restricted later)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Employee Management API v1");
        c.RoutePrefix = "swagger"; // Access at /swagger
        c.DocumentTitle = "Employee Management API";
    });
}


app.UseHttpsRedirection();
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Health check endpoints
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/database", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("database")
});

app.Run();