using System;
using System.Text.Json.Serialization;

namespace ChatClient
{
    // 与服务端 ChatMessage 相对应的客户端消息类
    public class ChatMessage
    {
        public int Id { get; set; }           // 消息 ID
        public string UserId { get; set; }    //用户ID
        public string NickName { get; set; }  // 用户昵称
        public string AvatarUrl { get; set; } // 头像 URL
        public string Content { get; set; }   // 消息内容
        public DateTime Timestamp { get; set; } // 时间戳

        [JsonIgnore]
        public bool IsTimeLabel { get; set; }
        public string TimeLabelText { get; set; }
        public bool IsMine { get; set; } // 标记是否为自己发送的消息（界面显示使用）
    }
}
