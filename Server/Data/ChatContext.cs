using Microsoft.EntityFrameworkCore;
using ChatServer.Models;

namespace ChatServer.Data
{
    // EF Core 上下文，负责 SQLite 数据库操作
    public class ChatContext : DbContext
    {
        public ChatContext(DbContextOptions<ChatContext> options) : base(options) { }

        // 聊天消息表
        public DbSet<ChatMessage> ChatMessages { get; set; }
    }
}
