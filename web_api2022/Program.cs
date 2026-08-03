using web_api2022.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json;
using web_api2022.Configurations;
using web_api2022.Hubs;
using web_api2022.Models;
using Microsoft.AspNetCore.HttpOverrides;
using web_api2022.Services;

var builder = WebApplication.CreateBuilder(args);

// 从配置文件加载连接字符串
builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 生产看板聚合服务
builder.Services.AddScoped<IProductionDashboardService, ProductionDashboardService>();

// 控制器 + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 从配置文件读取允许的 CORS 来源
var allowedOrigins = builder.Configuration.GetSection("AllowedCorsOrigins").Get<string[]>() ?? new string[] { };
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", policy =>
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});



// 加载自定义配置
builder.Services.Configure<ApiSettings>(builder.Configuration.GetSection("ApiSettings"));
// 加载更新接口配置
builder.Services.Configure<UpdateSettings>(builder.Configuration.GetSection("UpdateSettings"));
builder.Services.AddSignalR();

// JWT 配置
var jwtSection = builder.Configuration.GetSection("Jwt");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuers = jwtSection.GetSection("TrustedIssuers").Get<string[]>(),
            ValidateAudience = true,
            ValidAudiences = jwtSection.GetSection("TrustedIAudiences").Get<string[]>(),
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["SecretKey"] ?? "")),
            ClockSkew = TimeSpan.FromMinutes(1)
        };
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsync(JsonSerializer.Serialize(new
                {
                    error = "Authentication failed",
                    details = context.Exception.Message
                }));
            }
        };
    });

// 从 appsettings.json 读取 Kestrel 配置（监听地址）
builder.WebHost.ConfigureKestrel((context, options) =>
{
    options.Configure(context.Configuration.GetSection("Kestrel"));

});

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.MaxRequestBodySize = 200 * 1024 * 1024; // 设置为 200MB
});

var app = builder.Build();


// 必须放在其他中间件之前
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.UseRouting();
app.UseCors("AllowSpecificOrigins");
app.UseAuthentication();
//app.UseHttpsRedirection();
app.UseAuthorization();
app.MapHub<DataHub>("/dataHub");

if (app.Environment.IsDevelopment() || app.Configuration.GetValue<bool>("EnableSwagger"))
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.MapControllers();



// 放在 app.Run() 之前，迁移执行
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DataContext>();
    db.Database.Migrate();
}
app.Run();
