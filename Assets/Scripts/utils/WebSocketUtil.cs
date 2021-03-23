using UnityEngine;
using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using System.Text;

public class WebSocketClient : IDisposable
{
    private ClientWebSocket webSocket;
    private Uri serverUri;
    private CancellationTokenSource cancellationTokenSource;
    private Action<string> onMessageReceived;
    private Action onConnected;
    private Action onDisconnected;
    private Action<Exception> onError;

    public bool IsConnected { get; private set; }

    public WebSocketClient(string url, Action<string> onMessageReceived = null, Action onConnected = null, Action onDisconnected = null, Action<Exception> onError = null)
    {
        serverUri = new Uri(url);
        this.onMessageReceived = onMessageReceived;
        this.onConnected = onConnected;
        this.onDisconnected = onDisconnected;
        this.onError = onError;
        webSocket = new ClientWebSocket();
        cancellationTokenSource = new CancellationTokenSource();
        IsConnected = false;
    }

    public async Task ConnectAsync()
    {
        try
        {
            Debug.Log($"正在连接WebSocket服务器: {serverUri}");
            await webSocket.ConnectAsync(serverUri, cancellationTokenSource.Token);
            IsConnected = true;
            Debug.Log("WebSocket连接成功");
            onConnected?.Invoke();
            
            // 开始接收消息
            _ = ReceiveMessagesAsync();
        }
        catch (Exception e)
        {
            Debug.LogError($"WebSocket连接失败: {e.Message}");
            onError?.Invoke(e);
        }
    }

    public async Task SendAsync(string message)
    {
        if (!IsConnected || webSocket.State != WebSocketState.Open)
        {
            Debug.LogError("WebSocket未连接，无法发送消息");
            return;
        }

        try
        {
            byte[] buffer = Encoding.UTF8.GetBytes(message);
            await webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, cancellationTokenSource.Token);
            Debug.Log($"WebSocket发送消息: {message}");
        }
        catch (Exception e)
        {
            Debug.LogError($"WebSocket发送消息失败: {e.Message}");
            onError?.Invoke(e);
        }
    }

    private async Task ReceiveMessagesAsync()
    {
        try
        {
            byte[] buffer = new byte[4096];
            while (webSocket.State == WebSocketState.Open && !cancellationTokenSource.Token.IsCancellationRequested)
            {
                WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationTokenSource.Token);
                
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, cancellationTokenSource.Token);
                    break;
                }
                else if (result.MessageType == WebSocketMessageType.Text)
                {
                    string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    Debug.Log($"WebSocket接收消息: {message}");
                    onMessageReceived?.Invoke(message);
                }
            }
        }
        catch (OperationCanceledException)
        {
            // 正常取消，不需要处理
        }
        catch (Exception e)
        {
            Debug.LogError($"WebSocket接收消息失败: {e.Message}");
            onError?.Invoke(e);
        }
        finally
        {
            IsConnected = false;
            Debug.Log("WebSocket连接已关闭");
            onDisconnected?.Invoke();
        }
    }

    public async Task DisconnectAsync()
    {
        try
        {
            cancellationTokenSource.Cancel();
            
            if (webSocket.State == WebSocketState.Open || webSocket.State == WebSocketState.Connecting)
            {
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"WebSocket断开连接失败: {e.Message}");
        }
        finally
        {
            IsConnected = false;
        }
    }

    public void Dispose()
    {
        DisconnectAsync().Wait();
        webSocket?.Dispose();
        cancellationTokenSource?.Dispose();
    }
}

public static class WebSocketUtil
{
    public static WebSocketClient CreateClient(string url, Action<string> onMessageReceived = null, Action onConnected = null, Action onDisconnected = null, Action<Exception> onError = null)
    {
        return new WebSocketClient(url, onMessageReceived, onConnected, onDisconnected, onError);
    }
}

public class WebSocketExample : MonoBehaviour
{
    public string testWebSocketUrl = "ws://localhost";
    private WebSocketClient webSocketClient;

    private async void Start()
    {
        // 示例：创建WebSocket客户端
        webSocketClient = WebSocketUtil.CreateClient(
            testWebSocketUrl,
            OnMessageReceived,
            OnConnected,
            OnDisconnected,
            OnError
        );

        // 连接到服务器
        await webSocketClient.ConnectAsync();

        // 发送测试消息
        await Task.Delay(1000);
        await webSocketClient.SendAsync("Hello WebSocket!");
    }

    private void OnMessageReceived(string message)
    {
        Debug.Log($"收到WebSocket消息: {message}");
    }

    private void OnConnected()
    {
        Debug.Log("WebSocket连接成功");
    }

    private void OnDisconnected()
    {
        Debug.Log("WebSocket连接断开");
    }

    private void OnError(Exception e)
    {
        Debug.LogError($"WebSocket错误: {e.Message}");
    }

    private void OnDestroy()
    {
        webSocketClient?.Dispose();
    }
}
