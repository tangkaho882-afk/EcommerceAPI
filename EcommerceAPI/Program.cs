using Asp.Versioning;
using EcommerceAPI.Configurations;
using EcommerceAPI.Constants;
using EcommerceAPI.Data;
using EcommerceAPI.DTOs.Error;
using EcommerceAPI.Filters;
using EcommerceAPI.HealthChecks;
using EcommerceAPI.Middlewares;
using EcommerceAPI.Services;
using EcommerceAPI.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Scalar.AspNetCore;
using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services
    .AddControllers()
    .AddJsonOptions(option =>
        option.AllowInputFormatterExceptionMessages = false);
builder.Services
    .AddApiVersioning(options =>
    {
        // 設定預設 API version 為 1.0
        options.DefaultApiVersion = new ApiVersion(1,0);
        // 當 client 冇指定版本時，是否使用預設版本
        options.AssumeDefaultVersionWhenUnspecified = true;
        // 在 response header 報告支援的 API versions
        options.ReportApiVersions = true;
        // 指定使用 URL Segment 讀取版本
        options.ApiVersionReader = new UrlSegmentApiVersionReader();
    })
    .AddMvc()
    .AddApiExplorer(options =>
    {
        // OpenAPI group 名稱格式，例如 v1、v2
        options.GroupNameFormat = "'v'V";
        // 將 route 裏面的 {version:apiVersion}
        // 替換成真正版本號
        options.SubstituteApiVersionInUrl = true;
    });
//interface register
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<CheckoutAuditFilter>();
builder.Services.AddScoped<ServerTimeHeaderFilter>();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.ConfigureOptions<ConfigureSwaggerOptions>();
builder.Services.AddSwaggerGen(options =>
{
    // 你現有嘅 v1 / v2 OpenAPI 設定
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header
    });

    options.AddSecurityRequirement(document =>
        new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecuritySchemeReference(
                    "Bearer",
                    document
                ),
                []
            }
        });
});

//health check
builder.Services.AddHealthChecks().AddCheck<DatabaseHealthCheck>("database", tags: new[] {"ready"});

//Local Memory Cache
builder.Services.AddMemoryCache();

//Limiter
builder.Services.AddRateLimiter(option => 
    {
        option.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

        option.AddPolicy(RateLimitPolicies.Checkout, HttpContext =>
        {
            var userId = HttpContext.User
            .FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? "anonymous";

            return RateLimitPartition.GetTokenBucketLimiter(
                partitionKey: userId,
                factory: _ => new TokenBucketRateLimiterOptions
                {
                    TokenLimit = 3,
                    TokensPerPeriod = 1,
                    ReplenishmentPeriod = TimeSpan.FromSeconds(20),
                    QueueLimit = 0,
                    AutoReplenishment = true
                });
        });

        option.AddPolicy(RateLimitPolicies.Login, HttpContext =>
        {
            var loginIp = HttpContext.Connection.RemoteIpAddress?.ToString()??"unknown";

            return RateLimitPartition.GetTokenBucketLimiter(
                partitionKey: loginIp,
                factory: _ => new TokenBucketRateLimiterOptions
                {
                    TokenLimit = 10,
                    TokensPerPeriod = 10,
                    ReplenishmentPeriod = TimeSpan.FromMinutes(1),
                    AutoReplenishment = true,
                    QueueLimit = 0
                });
        });
    });


builder.Services.Configure<ApiBehaviorOptions>(options => {
    options.InvalidModelStateResponseFactory = context => { 
        // 1. 將有錯誤嘅 ModelState 轉成 Dictionary<string, string[]>
        var errors = context.ModelState
        .Where(entry => entry.Value?.Errors.Count > 0)
        .ToDictionary(error => error.Key, error => error.Value!.Errors
        .Select(x => x.Exception != null ? "The supplied value is invalid." : x.ErrorMessage)
        .ToArray());

        // 2. 建立 ErrorResponseDto
        var errorResponse = new ErrorResponseDto
        {
            StatusCode = StatusCodes.Status400BadRequest,
            Message = "Validation failed.",
            TraceId = context.HttpContext.TraceIdentifier,
            Errors = errors
        };

        // 3. 回傳 BadRequestObjectResult
        return new BadRequestObjectResult(errorResponse);
    };
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));

builder.Services
    .AddOptions<JwtSettings>()
    .Bind(builder.Configuration.GetSection(JwtSettings.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services
    .AddOptions<JwtBearerOptions>(
        JwtBearerDefaults.AuthenticationScheme)
    .Configure<IOptions<JwtSettings>>(
        (options, jwtOptions) =>
        {
            var jwtSettings = jwtOptions.Value;

            // 在這裏設定 options.TokenValidationParameters
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key))
            };
        });

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.MapScalarApiReference(options =>
    {
        // 強制指定 Scalar 去讀取微軟預設產出的 json 地址
        options.OpenApiRoutePattern = "/swagger/{documentName}/swagger.json";
        options
            .AddDocument("v1", "Ecommerce API V1")
            .AddDocument("v2", "Ecommerce API V2")
            .EnablePersistentAuthentication(); //store the jwt token in browser local storage
    });
}
//Health Checks
app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => false,

    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";

        var response = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.ToDictionary(
                entry => entry.Key,
                entry => entry.Value.Status.ToString())
        };
        await context.Response.WriteAsJsonAsync(response);
    }
});

app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = async (context,report)=>
    {
        context.Response.ContentType = "application/json";

        var response = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.ToDictionary(
                entry => entry.Key,
                entry => entry.Value.Status.ToString())
        };

        await context.Response.WriteAsJsonAsync(response);
    }
});

app.UseHttpsRedirection();

app.UseMiddleware<ExceptionMiddleware>();

app.UseAuthentication();

app.UseMiddleware<LoggingScopeMiddleware>();

app.UseAuthorization();

app.UseRateLimiter();

app.MapControllers();

app.Run();
