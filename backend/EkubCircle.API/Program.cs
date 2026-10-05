using System.Text;
using EkubCircle.API.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// 1. Configure SQLite Database with EF Core
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
                       ?? "Data Source=ekubcircle.db";
builder.Services.AddDbContext<EkubDbContext>(options =>
    options.UseSqlite(connectionString));

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

builder.Services.AddScoped<ITokenService, JwtTokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// 4. Configure Swagger with JWT Bearer Definition
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "EkubCircle Web API",
        Version = "v1",
        Description = "RESTful API and Server-Side Rule Engine for Rotating Ekub Savings Ledger"
    });
});

var app = builder.Build();

// 5. Apply Migrations and Seed Data Deterministically
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<EkubDbContext>();
    await DbInitializer.SeedAsync(db);
}

// 6. Configure HTTP pipeline
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
