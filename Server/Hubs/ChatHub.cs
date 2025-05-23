using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using ChatServer.Data;
using ChatServer.Models;
using Microsoft.EntityFrameworkCore;

namespace ChatServer.Hubs
{
    // SignalR 聊天 Hub
    public class ChatHub : Hub
    {
        private readonly ChatContext _context;
        public ChatHub(ChatContext context)
        {
            _context = context;
        }

        // 接收客户端发送的消息，将消息保存到数据库并广播给所有客户端
        public async Task SendMessage(ChatMessage message)
        {
            message.Timestamp = DateTime.Now;
            _context.ChatMessages.Add(message);
            // 如果消息数量超过 5000，删除最旧的一条
            if (_context.ChatMessages.Count() > 5000)
            {
                var oldest = _context.ChatMessages.OrderBy(m => m.Timestamp).First();
                _context.ChatMessages.Remove(oldest);
            }
            await _context.SaveChangesAsync();
            await Clients.All.SendAsync("ReceiveMessage", message);
        }

        // 获取最近 count 条消息（按时间升序返回）
        public async Task<List<ChatMessage>> GetRecentMessages(int count)
        {
            var messages = await _context.ChatMessages
                .OrderByDescending(m => m.Timestamp)
                .Take(count)
                .OrderBy(m => m.Timestamp)
                .ToListAsync();
            return messages;
        }

        // 获取指定消息 ID 之前的更多消息（用于滚动加载历史记录）
        public async Task<List<ChatMessage>> GetPreviousMessages(int lastId, int count)
        {
            var messages = await _context.ChatMessages
                .Where(m => m.Id < lastId)
                .OrderByDescending(m => m.Timestamp)
                .Take(count)
                .OrderBy(m => m.Timestamp)
                .ToListAsync();
            return messages;
        }
    }
}
