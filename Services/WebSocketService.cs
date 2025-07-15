using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using JigaMultiplatform.Models;

namespace JigaMultiplatform.Services;

/// <summary>
/// Service for managing multiple WebSocket connections for JIGA streams.
/// C# equivalent of JavaScript WebSocketManager class.
/// </summary>
public class WebSocketService : IWebSocketService, IDisposable
{
    private readonly Dictionary<WebSocketType, ClientWebSocket> _connections = new();
    private readonly Dictionary<WebSocketType, WebSocketConnectionStats> _stats = new();
    private readonly Dictionary<WebSocketType, CancellationTokenSource> _cancellationTokens = new();
    private readonly Dictionary<WebSocketType, int> _reconnectAttempts = new();
    private readonly JsonSerializerOptions _jsonOptions;
    
    private const int MaxReconnectAttempts = 3;
    private const int BaseReconnectDelayMs = 5000; // 5 seconds
    private const int ReceiveBufferSize = 4096;

    public WebSocketService()
    {
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            WriteIndented = false
        };
    }

    // Events
    public event EventHandler<WebSocketMessageEventArgs>? MessageReceived;
    public event EventHandler<WebSocketStatusEventArgs>? StatusChanged;
    public event EventHandler<VisionAnalysisEventArgs>? VisionAnalysisReceived;
    public event EventHandler<AudioResponseEventArgs>? AudioResponseReceived;
    public event EventHandler<WebSocketErrorEventArgs>? ErrorOccurred;

    public async Task<bool> ConnectAsync(string url, WebSocketType type, CancellationToken cancellationToken = default)
    {
        try
        {
            // Close existing connection if present
            if (_connections.ContainsKey(type))
            {
                await DisconnectAsync(type, cancellationToken);
            }

            var ws = new ClientWebSocket();
            var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            
            _connections[type] = ws;
            _cancellationTokens[type] = cts;
            _reconnectAttempts[type] = 0;
            
            // Initialize stats
            _stats[type] = new WebSocketConnectionStats
            {
                Type = type,
                Url = url,
                ConnectedAt = DateTime.UtcNow,
                State = WebSocketState.Connecting
            };

            OnStatusChanged(type, WebSocketState.Connecting, "Connecting...");

            // Connect to WebSocket
            await ws.ConnectAsync(new Uri(url), cts.Token);
            
            _stats[type].State = WebSocketState.Open;
            _stats[type].ConnectedAt = DateTime.UtcNow;
            _stats[type].IsHealthy = true;

            OnStatusChanged(type, WebSocketState.Open, "Connected successfully");

            // Start receiving messages in background
            _ = Task.Run(() => ReceiveLoop(type, cts.Token), cts.Token);
            
            // Start heartbeat for connection health
            _ = Task.Run(() => HeartbeatLoop(type, cts.Token), cts.Token);

            return true;
        }
        catch (Exception ex)
        {
            OnError(type, $"Failed to connect: {ex.Message}", ex, true);
            return false;
        }
    }

    public async Task<Dictionary<WebSocketType, bool>> ConnectMultipleAsync(WebSocketEndpoints endpoints, CancellationToken cancellationToken = default)
    {
        var results = new Dictionary<WebSocketType, bool>();
        var tasks = new List<Task>();

        foreach (var connection in endpoints.Connections.Values.OrderBy(c => c.Priority))
        {
            if (connection.Enabled)
            {
                tasks.Add(Task.Run(async () =>
                {
                    var success = await ConnectAsync(connection.Url, connection.Type, cancellationToken);
                    lock (results)
                    {
                        results[connection.Type] = success;
                    }
                }, cancellationToken));
            }
        }

        await Task.WhenAll(tasks);
        return results;
    }

    public async Task<bool> SendFrameAsync(string frameData, WebSocketType type, double? timestamp = null, CancellationToken cancellationToken = default)
    {
        var message = new
        {
            type = "analyze_frame",
            frame_data = frameData,
            timestamp = timestamp ?? GetUnixTimestamp(),
            frame_number = _stats.ContainsKey(type) ? _stats[type].MessagesSent + 1 : 1
        };

        return await SendMessageAsync(message, type, cancellationToken);
    }

    public async Task<bool> SendAudioAsync(string audioData, string? frameData = null, double? timestamp = null, CancellationToken cancellationToken = default)
    {
        var message = new
        {
            type = "process_audio",
            audio_base64 = audioData,
            frame_data = frameData,
            timestamp = timestamp ?? GetUnixTimestamp(),
            audio_format = "wav",
            sample_rate = 22050,
            channels = 1
        };

        return await SendMessageAsync(message, WebSocketType.Audio, cancellationToken);
    }

    public async Task<bool> SendMessageAsync(object message, WebSocketType type, CancellationToken cancellationToken = default)
    {
        if (!_connections.TryGetValue(type, out var ws) || ws.State != WebSocketState.Open)
        {
            OnError(type, $"Cannot send message: {type} connection not open", null, true);
            return false;
        }

        try
        {
            var json = JsonSerializer.Serialize(message, _jsonOptions);
            var bytes = Encoding.UTF8.GetBytes(json);
            var buffer = new ArraySegment<byte>(bytes);

            await ws.SendAsync(buffer, WebSocketMessageType.Text, true, cancellationToken);
            
            // Update stats
            if (_stats.ContainsKey(type))
            {
                _stats[type].MessagesSent++;
                _stats[type].LastMessageAt = DateTime.UtcNow;
            }

            return true;
        }
        catch (Exception ex)
        {
            OnError(type, $"Failed to send message: {ex.Message}", ex, true);
            return false;
        }
    }

    public async Task<bool> SendPingAsync(WebSocketType type, CancellationToken cancellationToken = default)
    {
        var pingMessage = new
        {
            type = "ping",
            timestamp = GetUnixTimestamp()
        };

        return await SendMessageAsync(pingMessage, type, cancellationToken);
    }

    public async Task DisconnectAsync(WebSocketType type, CancellationToken cancellationToken = default)
    {
        if (_connections.TryGetValue(type, out var ws))
        {
            try
            {
                if (ws.State == WebSocketState.Open)
                {
                    await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Manual disconnect", cancellationToken);
                }
            }
            catch (Exception ex)
            {
                OnError(type, $"Error during disconnect: {ex.Message}", ex, false);
            }
            finally
            {
                ws.Dispose();
                _connections.Remove(type);
            }
        }

        if (_cancellationTokens.TryGetValue(type, out var cts))
        {
            cts.Cancel();
            cts.Dispose();
            _cancellationTokens.Remove(type);
        }

        _stats.Remove(type);
        _reconnectAttempts.Remove(type);

        OnStatusChanged(type, WebSocketState.Closed, "Disconnected");
    }

    public async Task DisconnectAllAsync(CancellationToken cancellationToken = default)
    {
        var tasks = _connections.Keys.Select(type => DisconnectAsync(type, cancellationToken));
        await Task.WhenAll(tasks);
    }

    public bool IsConnected(WebSocketType type)
    {
        return _connections.TryGetValue(type, out var ws) && ws.State == WebSocketState.Open;
    }

    public Dictionary<WebSocketType, bool> GetConnectionStatus()
    {
        return _connections.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value.State == WebSocketState.Open
        );
    }

    public WebSocketConnectionStats? GetConnectionStats(WebSocketType type)
    {
        return _stats.TryGetValue(type, out var stats) ? stats : null;
    }

    // Private methods
    private async Task ReceiveLoop(WebSocketType type, CancellationToken cancellationToken)
    {
        if (!_connections.TryGetValue(type, out var ws))
            return;

        var buffer = new byte[ReceiveBufferSize];
        var messageBuffer = new StringBuilder();

        try
        {
            while (ws.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
            {
                var result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
                
                if (result.MessageType == WebSocketMessageType.Text)
                {
                    var text = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    messageBuffer.Append(text);

                    if (result.EndOfMessage)
                    {
                        var completeMessage = messageBuffer.ToString();
                        messageBuffer.Clear();

                        await ProcessMessage(type, completeMessage);
                        
                        // Update stats
                        if (_stats.ContainsKey(type))
                        {
                            _stats[type].MessagesReceived++;
                            _stats[type].LastMessageAt = DateTime.UtcNow;
                        }
                    }
                }
                else if (result.MessageType == WebSocketMessageType.Close)
                {
                    OnStatusChanged(type, WebSocketState.Closed, result.CloseStatusDescription);
                    break;
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when cancellation is requested
        }
        catch (Exception ex)
        {
            OnError(type, $"Error in receive loop: {ex.Message}", ex, true);
            await AttemptReconnect(type);
        }
    }

    private async Task ProcessMessage(WebSocketType type, string message)
    {
        try
        {
            using var document = JsonDocument.Parse(message);
            var root = document.RootElement;
            var messageType = root.GetProperty("type").GetString() ?? "";

            // Trigger generic message event
            OnMessageReceived(type, messageType, root);

            // Handle specific message types
            switch (messageType.ToLowerInvariant())
            {
                case "direct_video_analysis_result":
                case "high_speed_analysis_result":
                    await HandleVisionAnalysisResult(type, root);
                    break;
                    
                case "audio_response":
                case "voice_response":
                    await HandleAudioResponse(root);
                    break;
                    
                case "pong":
                    // Heartbeat response - update connection health
                    if (_stats.ContainsKey(type))
                    {
                        _stats[type].IsHealthy = true;
                        _stats[type].LastMessageAt = DateTime.UtcNow;
                    }
                    break;
                    
                case "error":
                    var errorMsg = root.GetProperty("message").GetString() ?? "Unknown error";
                    OnError(type, errorMsg, null, false);
                    break;
            }
        }
        catch (Exception ex)
        {
            OnError(type, $"Error processing message: {ex.Message}", ex, false);
        }
    }

    private async Task HandleVisionAnalysisResult(WebSocketType type, JsonElement root)
    {
        try
        {
            var result = new VisionAnalysisResult
            {
                Type = root.GetProperty("type").GetString() ?? "",
                ProcessingMode = root.TryGetProperty("processing_mode", out var mode) ? mode.GetString() ?? "" : "",
                Timestamp = DateTime.UtcNow
            };

            if (root.TryGetProperty("processing_latency_ms", out var latency))
            {
                result.ProcessingLatencyMs = latency.GetDouble();
            }

            if (root.TryGetProperty("data", out var data))
            {
                result.Data = JsonSerializer.Deserialize<Dictionary<string, object>>(data.GetRawText());
                
                if (data.TryGetProperty("detections", out var detections))
                {
                    result.Detections = JsonSerializer.Deserialize<List<Detection>>(detections.GetRawText());
                }
            }

            OnVisionAnalysisReceived(type, result);
        }
        catch (Exception ex)
        {
            OnError(type, $"Error handling vision analysis result: {ex.Message}", ex, false);
        }
    }

    private async Task HandleAudioResponse(JsonElement root)
    {
        try
        {
            var response = JsonSerializer.Deserialize<AudioResponse>(root.GetRawText(), _jsonOptions);
            if (response != null)
            {
                OnAudioResponseReceived(response);
            }
        }
        catch (Exception ex)
        {
            OnError(WebSocketType.Audio, $"Error handling audio response: {ex.Message}", ex, false);
        }
    }

    private async Task HeartbeatLoop(WebSocketType type, CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested && IsConnected(type))
            {
                await Task.Delay(30000, cancellationToken); // Send ping every 30 seconds
                
                if (IsConnected(type))
                {
                    await SendPingAsync(type, cancellationToken);
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when cancellation is requested
        }
        catch (Exception ex)
        {
            OnError(type, $"Error in heartbeat loop: {ex.Message}", ex, false);
        }
    }

    private async Task AttemptReconnect(WebSocketType type)
    {
        var attempts = _reconnectAttempts.GetValueOrDefault(type, 0);
        
        if (attempts >= MaxReconnectAttempts)
        {
            OnError(type, $"Max reconnection attempts ({MaxReconnectAttempts}) reached", null, false);
            return;
        }

        _reconnectAttempts[type] = attempts + 1;
        var delay = BaseReconnectDelayMs * (int)Math.Pow(1.5, attempts); // Exponential backoff

        OnStatusChanged(type, WebSocketState.Connecting, $"Reconnecting... (attempt {attempts + 1}/{MaxReconnectAttempts})");

        await Task.Delay(delay);

        if (_stats.TryGetValue(type, out var stats))
        {
            stats.ReconnectAttempts++;
            await ConnectAsync(stats.Url, type);
        }
    }

    private double GetUnixTimestamp()
    {
        return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000.0;
    }

    // Event handlers
    private void OnMessageReceived(WebSocketType type, string messageType, JsonElement data)
    {
        MessageReceived?.Invoke(this, new WebSocketMessageEventArgs
        {
            ConnectionType = type,
            MessageType = messageType,
            Data = data
        });
    }

    private void OnStatusChanged(WebSocketType type, WebSocketState state, string? message = null)
    {
        StatusChanged?.Invoke(this, new WebSocketStatusEventArgs
        {
            ConnectionType = type,
            State = state,
            StatusMessage = message
        });
    }

    private void OnVisionAnalysisReceived(WebSocketType type, VisionAnalysisResult result)
    {
        VisionAnalysisReceived?.Invoke(this, new VisionAnalysisEventArgs
        {
            ConnectionType = type,
            Result = result,
            ProcessingLatencyMs = result.ProcessingLatencyMs
        });
    }

    private void OnAudioResponseReceived(AudioResponse response)
    {
        AudioResponseReceived?.Invoke(this, new AudioResponseEventArgs
        {
            Response = response,
            ResponseTimeMs = 0 // Could be calculated from timestamps
        });
    }

    private void OnError(WebSocketType type, string message, Exception? exception = null, bool isReconnectable = false)
    {
        ErrorOccurred?.Invoke(this, new WebSocketErrorEventArgs
        {
            ConnectionType = type,
            ErrorMessage = message,
            Exception = exception,
            IsReconnectable = isReconnectable
        });
    }

    public void Dispose()
    {
        _ = Task.Run(async () => await DisconnectAllAsync());
        
        foreach (var cts in _cancellationTokens.Values)
        {
            cts?.Dispose();
        }
        _cancellationTokens.Clear();
    }
} 