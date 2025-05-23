using System.Windows;
using System.Windows.Controls;

namespace ChatClient
{
    // 消息模板选择器，根据 IsMine 属性决定使用左侧或右侧模板
    public class MessageTemplateSelector : DataTemplateSelector
    {
        public DataTemplate LeftTemplate { get; set; }
        public DataTemplate RightTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is ChatMessage msg)
            {
                return msg.IsMine ? RightTemplate : LeftTemplate;
            }
            return base.SelectTemplate(item, container);
        }
    }
}
