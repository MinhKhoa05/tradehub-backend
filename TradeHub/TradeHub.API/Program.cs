using TradeHub.API.Extensions;
using TradeHub.API.Middlewares;
using TradeHub.BLL.ApplicationServices;
using TradeHub.BLL.Configurations;
using TradeHub.BLL.Services;
using TradeHub.DAL;
using TradeHub.DAL.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
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
    var connectionString = config.GetConnectionString("Default");
    return new DatabaseContext(connectionString!);
});


// ================= REPOSITORIES =================

builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<ProductRepository>();
builder.Services.AddScoped<CartItemRepository>();


// ================= SERVICES =================

builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<CartService>();


// ================= APPLICATION SERVICES =================

builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<CartViewUsecase>();


// ================= TOKEN SERVICE =================

builder.Services.AddScoped<TokenService>();


var app = builder.Build();


// ================= MIDDLEWARE =================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();