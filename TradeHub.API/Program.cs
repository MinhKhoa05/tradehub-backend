using Microsoft.AspNetCore.Mvc;
using TradeHub.API;
using TradeHub.API.Extensions;
using TradeHub.API.Filters;
using TradeHub.API.Middlewares;
using TradeHub.BLL.ApplicationServices;
using TradeHub.BLL.Common;
using TradeHub.BLL.Configurations;
using TradeHub.BLL.Services;
using TradeHub.DAL;
using TradeHub.DAL.Queries;
using TradeHub.DAL.Repositories;

var builder = WebApplication.CreateBuilder(args);

// ================= CORS CONFIGURATION =================
// Lấy giá trị từ biến môi trường "AllowedOrigins" truyền từ Docker
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
    options.SwaggerDoc("v1", new() { Title = "TradeHub API", Version = "v1" });

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
    // Lấy chuỗi kết nối từ key "Default" (khớp với ConnectionStrings__Default trong Docker)
    var connectionString = config.GetConnectionString("Default");
    return new DatabaseContext(connectionString!);
});

// ================= REPOSITORIES =================
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<ProductRepository>();
builder.Services.AddScoped<CartItemRepository>();
builder.Services.AddScoped<OrderRepository>();
builder.Services.AddScoped<OrderItemRepository>();
builder.Services.AddScoped<OrderHistoryRepository>();
builder.Services.AddScoped<WalletRepository>();
builder.Services.AddScoped<WalletTransactionRepository>();

// ================= QUERIES =================
builder.Services.AddScoped<CartItemQuery>();

// ================= SERVICES =================
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<CartService>();
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<WalletService>();

// ================= APPLICATION SERVICES =================
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<OrderUsecase>();

// ================= COMMON SERVICES =================
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IIdentityService, IdentityService>();
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<PasswordService>();

var app = builder.Build();

// ================= MIDDLEWARE =================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Kích hoạt CORS trước khi Auth và MapControllers
app.UseCors("AllowReactApp");

// Tắt HttpsRedirection để tránh lỗi SSL khi chạy Docker local với React
// app.UseHttpsRedirection();

app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();