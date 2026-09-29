var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// 根路径默认进入 Login.html（自动跳转 SPA 入口 /app/），并托管 wwwroot 静态文件
app.UseFileServer(new FileServerOptions
{
    DefaultFilesOptions = { DefaultFileNames = { "Login.html" } }
});
// app.UseHttpsRedirection();

// Vue SPA 历史路由回退：/app/* 非文件路径统一返回应用入口
app.MapFallbackToFile("/app/{*path:nonfile}", "app/index.html");

app.Run();
