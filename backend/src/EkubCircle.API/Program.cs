using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using EkubCircle.Application;
using EkubCircle.Infrastructure;
using EkubCircle.Infrastructure.Persistence.Context;
using EkubCircle.Infrastructure.SeedData;
using EkubCircle.API.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// 1. Layered Architecture Services Registration
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

// 2. Add CORS policy to support Angular frontend dev server
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// 3. Configure JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? "EkubCircle_Super_Secret_Key_For_Hackathon_2026_Minimum_32_Bytes!";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "EkubCircleAPI";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "EkubCircleClient";

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
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// 4. Configure Swagger with JWT Bearer Definition
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "EkubCircle Web API",
        Version = "v1",
        Description = "RESTful API and Server-Side Rule Engine for Rotating Ekub Savings Ledger (Challenge 3)"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
});

var app = builder.Build();

// 5. Apply Migrations and Seed Data Deterministically
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<EkubDbContext>();
    await DbInitializer.SeedAsync(db);
}

// 6. Exception Handling Middleware
app.UseMiddleware<ExceptionMiddleware>();

// 7. Configure HTTP pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "EkubCircle API v1");
        c.RoutePrefix = string.Empty; // Serve Swagger at root "/"
    });
}

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }
