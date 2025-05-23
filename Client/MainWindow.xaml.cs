using ChatClient;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ChatClient
{
    public partial class MainWindow : Window
    {
        private HubConnection _connection;
        private string _userNick;
        private string _userId;
        private string _avatarUrl;
        private bool _loadingMore = false;

        // ObservableCollection 绑定到消息列表
        public ObservableCollection<ChatMessage> Messages { get; set; } = new ObservableCollection<ChatMessage>();

        public MainWindow()
        {
            InitializeComponent();
            // 初始时隐藏聊天面板
            MessagesScrollViewer.Visibility = Visibility.Collapsed;
            SendPanel.Visibility = Visibility.Collapsed;
            // 绑定消息集合到 ItemsControl
            MessagesItemsControl.ItemsSource = Messages;
        }

        // 点击“加入”按钮
        private async void JoinButton_Click(object sender, RoutedEventArgs e)
        {
            _userNick = NickNameTextBox.Text.Trim();
            _userId = UserIdTextBox.Text.Trim();
            _avatarUrl = AvatarUrlTextBox.Text.Trim();

            if (string.IsNullOrEmpty(_userNick) || string.IsNullOrEmpty(_avatarUrl))
            {
                MessageBox.Show("请输入昵称和头像 URL。", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 创建并启动 SignalR 连接
            _connection = new HubConnectionBuilder()
                .WithUrl("http://localhost:5000/chathub")
                .WithAutomaticReconnect()
                .Build();

            // 处理接收到的消息事件
            _connection.On<ChatMessage>("ReceiveMessage", (message) =>
            {
                bool isMine = message.UserId == _userId;
                message.IsMine = isMine;
                Application.Current.Dispatcher.Invoke(() =>
                {
                    AddMessageWithTimeLabel(message);
                    // 滚动到底部显示最新消息
                    MessagesScrollViewer.ScrollToEnd();
                });
            });

            try
            {
                await _connection.StartAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show("无法连接到服务器: " + ex.Message);
                return;
            }

            // 加入成功后，显示聊天界面
            JoinPanel.Visibility = Visibility.Collapsed;
            MessagesScrollViewer.Visibility = Visibility.Visible;
            SendPanel.Visibility = Visibility.Visible;

            // 加载最近 10 条消息
            var recent = await _connection.InvokeAsync<ChatMessage[]>("GetRecentMessages", 10);
            foreach (var msg in recent)
            {
                msg.IsMine = msg.UserId == _userId;
                Messages.Add(msg);
            }
            // 滚动到底部
            MessagesScrollViewer.ScrollToEnd();
        }
        private void AddMessageWithTimeLabel(ChatMessage message)
        {
            // 获取最后一条非时间标签消息
            var lastMsg = Messages.LastOrDefault(m => !m.IsTimeLabel);
            if (lastMsg != null && (message.Timestamp - lastMsg.Timestamp).TotalMinutes >= 2)
            {
                // 插入时间标签
                Messages.Add(new ChatMessage
                {
                    IsTimeLabel = true,
                    TimeLabelText = message.Timestamp.ToString("yyyy-MM-dd HH:mm")
                });
            }
            Messages.Add(message);
        }

        // 发送消息
        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            var content = MessageTextBox.Text.Trim();
            if (!string.IsNullOrEmpty(content))
            {
                var msg = new ChatMessage
                {
                    UserId = _userId,
                    NickName = _userNick,
                    AvatarUrl = _avatarUrl,
                    Content = content,
                    Timestamp = DateTime.Now
                };
                await _connection.InvokeAsync("SendMessage", msg);
                MessageTextBox.Clear();
            }
        }

        // 滚动事件：滚动到顶部时加载更多历史消息
        private async void MessagesScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.VerticalOffset == 0 && !_loadingMore && Messages.Count > 0)
            {
                var firstMsg = Messages.First();
                if (firstMsg.Id > 1)
                {
                    _loadingMore = true;

                    // 记录加载前第一个元素的位置
                    var firstContainer = MessagesItemsControl.ItemContainerGenerator.ContainerFromItem(firstMsg) as FrameworkElement;
                    double offsetBefore = firstContainer?.TranslatePoint(new Point(0, 0), MessagesScrollViewer).Y ?? 0;

                    // 加载旧消息
                    var older = await _connection.InvokeAsync<ChatMessage[]>("GetPreviousMessages", firstMsg.Id, 10);
                    foreach (var msg in older)
                    {
                        msg.IsMine = msg.UserId == _userNick;
                    }

                    for (int i = older.Length - 1; i >= 0; i--)
                    {
                        Messages.Insert(0, older[i]);
                    }

                    //延迟执行更新UI
                    await Dispatcher.InvokeAsync(() =>
                    {
                        var newFirstContainer = MessagesItemsControl.ItemContainerGenerator.ContainerFromItem(firstMsg) as FrameworkElement;
                        if (newFirstContainer != null)
                        {
                            double offsetAfter = newFirstContainer.TranslatePoint(new Point(0, 0), MessagesScrollViewer).Y;
                            double delta = offsetAfter - offsetBefore;
                            MessagesScrollViewer.ScrollToVerticalOffset(MessagesScrollViewer.VerticalOffset + delta);
                        }
                    }, System.Windows.Threading.DispatcherPriority.Loaded);

                    _loadingMore = false;
                }
            }
        }

    }
}
