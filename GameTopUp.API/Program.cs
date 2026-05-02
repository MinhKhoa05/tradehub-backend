using Microsoft.AspNetCore.Mvc;
using GameTopUp.API;
using GameTopUp.API.Extensions;
using GameTopUp.API.Filters;
using GameTopUp.API.Middlewares;
using GameTopUp.BLL.ApplicationServices;
using GameTopUp.BLL.Common;
using GameTopUp.BLL.Services;
using GameTopUp.DAL;
using GameTopUp.DAL.Queries;
using GameTopUp.DAL.Repositories;
using GameTopUp.DAL.Interfaces;
using GameTopUp.BLL.Config;

var builder = WebApplication.CreateBuilder(args);

// ================= CORS CONFIGURATION =================
var originFromConfig = builder.Configuration["AllowedOrigins"];

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        policy =>
        {
            policy.WithOrigins(originFromConfig ?? "http://localhost:3000")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
});

// Add services to the container
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidationFilter>();
});

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "GameTopup API", Version = "v1" });

    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Nhập JWT Access Token"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Bind JwtSettings
builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("Jwt"));

// JWT Authentication
builder.Services.AddJwtAuthentication(builder.Configuration);

// ================= DATABASE =================
builder.Services.AddScoped<DatabaseContext>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var connectionString = config.GetConnectionString("Default");
    return new DatabaseContext(new MySqlConnector.MySqlConnection(connectionString!));
});

// ================= REPOSITORIES =================
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<CartItemRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderHistoryRepository, OrderHistoryRepository>();
builder.Services.AddScoped<IWalletTransactionRepository, WalletTransactionRepository>();
builder.Services.AddScoped<IWalletRepository, WalletRepository>();
builder.Services.AddScoped<IGameRepository, GameRepository>();
builder.Services.AddScoped<IGamePackageRepository, GamePackageRepository>();
builder.Services.AddScoped<IGameAccountRepository, GameAccountRepository>();

// ================= QUERIES =================
builder.Services.AddScoped<CartItemQuery>();

// ================= SERVICES =================
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<GameService>();
builder.Services.AddScoped<GamePackageService>();
builder.Services.AddScoped<CartService>();
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<WalletService>();

// ================= APPLICATION SERVICES =================
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<OrderUseCase>();

// ================= COMMON SERVICES =================
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<TokenService>();

builder.Services.AddScoped<PasswordService>();

// ================= MAPSTER =================
MapsterConfig.RegisterMappings();

var app = builder.Build();

// ================= MIDDLEWARE =================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowReactApp");
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
public partial class Program { }
