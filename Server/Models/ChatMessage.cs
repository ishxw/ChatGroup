using System;

namespace ChatServer.Models
{
    // 聊天消息实体类
    public class ChatMessage
    {
        public int Id { get; set; }           // 消息 ID，自增主键
        public string UserId { get; set; }    //用户ID
        public string NickName { get; set; }  // 发送者昵称
        public string AvatarUrl { get; set; } // 发送者头像 URL
        public string Content { get; set; }   // 消息内容
        public DateTime Timestamp { get; set; } // 发送时间
    }
}
