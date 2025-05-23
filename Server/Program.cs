using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using ChatServer.Data;
using ChatServer.Hubs;

var builder = WebApplication.CreateBuilder(args);
// 添加 EF Core 和 SQLite
builder.Services.AddDbContext<ChatContext>(options =>
    options.UseSqlite("Data Source=chat.db"));
// 添加 SignalR 服务
builder.Services.AddSignalR();

var app = builder.Build();

// 确保数据库表创建（如果不存在则创建）
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ChatContext>();
    db.Database.EnsureCreated();
}

// 映射 SignalR Hub 终点
app.MapHub<ChatHub>("/chathub");

// 监听 http://localhost:5000
app.Run("http://localhost:5000");
