using System.Net.WebSockets;
using JigaMultiplatform.Models;

namespace JigaMultiplatform.Services;

/// <summary>
/// Service for managing multiple WebSocket connections for JIGA streams.
/// Equivalent to JavaScript WebSocketManager class.
/// </summary>
public interface IWebSocketService
{
    /// <summary>
    /// Connect to a WebSocket endpoint
    /// </summary>
    /// <param name="url">WebSocket URL</param>
    /// <param name="type">Connection type</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success status</returns>
    Task<bool> ConnectAsync(string url, WebSocketType type, CancellationToken cancellationToken = default);

    /// <summary>
    /// Connect multiple WebSocket endpoints from configuration
    /// </summary>
    /// <param name="endpoints">WebSocket endpoints configuration</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Connection results</returns>
    Task<Dictionary<WebSocketType, bool>> ConnectMultipleAsync(WebSocketEndpoints endpoints, CancellationToken cancellationToken = default);

    /// <summary>
    /// Send frame data to vision or high-speed WebSocket
    /// </summary>
    /// <param name="frameData">Base64 encoded frame data</param>
    /// <param name="type">WebSocket type to send to</param>
    /// <param name="timestamp">Optional timestamp</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success status</returns>
    Task<bool> SendFrameAsync(string frameData, WebSocketType type, double? timestamp = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Send audio data to audio WebSocket
    /// </summary>
    /// <param name="audioData">Base64 encoded audio data</param>
    /// <param name="frameData">Optional frame data for visual context</param>
    /// <param name="timestamp">Optional timestamp</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success status</returns>
    Task<bool> SendAudioAsync(string audioData, string? frameData = null, double? timestamp = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Send custom message to specific WebSocket
    /// </summary>
    /// <param name="message">Message object to send</param>
    /// <param name="type">WebSocket type</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success status</returns>
    Task<bool> SendMessageAsync(object message, WebSocketType type, CancellationToken cancellationToken = default);

    /// <summary>
    /// Send ping to specific WebSocket for health check
    /// </summary>
    /// <param name="type">WebSocket type</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success status</returns>
    Task<bool> SendPingAsync(WebSocketType type, CancellationToken cancellationToken = default);

    /// <summary>
    /// Disconnect specific WebSocket connection
    /// </summary>
    /// <param name="type">Connection type to disconnect</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task DisconnectAsync(WebSocketType type, CancellationToken cancellationToken = default);

    /// <summary>
    /// Disconnect all WebSocket connections
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    Task DisconnectAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if a specific connection is active
    /// </summary>
    /// <param name="type">Connection type</param>
    /// <returns>Connection status</returns>
    bool IsConnected(WebSocketType type);

    /// <summary>
    /// Get connection status for all types
    /// </summary>
    /// <returns>Connection status map</returns>
    Dictionary<WebSocketType, bool> GetConnectionStatus();

    /// <summary>
    /// Get connection statistics
    /// </summary>
    /// <param name="type">Connection type</param>
    /// <returns>Connection statistics</returns>
    WebSocketConnectionStats? GetConnectionStats(WebSocketType type);

    // Events
    /// <summary>
    /// Fired when a WebSocket message is received
    /// </summary>
    event EventHandler<WebSocketMessageEventArgs> MessageReceived;

    /// <summary>
    /// Fired when connection status changes
    /// </summary>
    event EventHandler<WebSocketStatusEventArgs> StatusChanged;

    /// <summary>
    /// Fired when a vision analysis result is received
    /// </summary>
    event EventHandler<VisionAnalysisEventArgs> VisionAnalysisReceived;

    /// <summary>
    /// Fired when an audio response is received
    /// </summary>
    event EventHandler<AudioResponseEventArgs> AudioResponseReceived;

    /// <summary>
    /// Fired when an error occurs
    /// </summary>
    event EventHandler<WebSocketErrorEventArgs> ErrorOccurred;
}

/// <summary>
/// WebSocket message event arguments
/// </summary>
public class WebSocketMessageEventArgs : EventArgs
{
    public WebSocketType ConnectionType { get; set; }
    public string MessageType { get; set; } = string.Empty;
    public object? Data { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// WebSocket status change event arguments
/// </summary>
public class WebSocketStatusEventArgs : EventArgs
{
    public WebSocketType ConnectionType { get; set; }
    public WebSocketState State { get; set; }
    public string? StatusMessage { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Vision analysis result event arguments
/// </summary>
public class VisionAnalysisEventArgs : EventArgs
{
    public WebSocketType ConnectionType { get; set; }
    public VisionAnalysisResult? Result { get; set; }
    public double ProcessingLatencyMs { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Audio response event arguments
/// </summary>
public class AudioResponseEventArgs : EventArgs
{
    public AudioResponse? Response { get; set; }
    public double ResponseTimeMs { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// WebSocket error event arguments
/// </summary>
public class WebSocketErrorEventArgs : EventArgs
{
    public WebSocketType ConnectionType { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public Exception? Exception { get; set; }
    public bool IsReconnectable { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// WebSocket connection statistics
/// </summary>
public class WebSocketConnectionStats
{
    public WebSocketType Type { get; set; }
    public string Url { get; set; } = string.Empty;
    public WebSocketState State { get; set; }
    public DateTime ConnectedAt { get; set; }
    public DateTime LastMessageAt { get; set; }
    public int MessagesSent { get; set; }
    public int MessagesReceived { get; set; }
    public int ReconnectAttempts { get; set; }
    public double AverageLatencyMs { get; set; }
    public bool IsHealthy { get; set; } = true;
}

/// <summary>
/// Vision analysis result from WebSocket
/// </summary>
public class VisionAnalysisResult
{
    public string Type { get; set; } = string.Empty;
    public Dictionary<string, object>? Data { get; set; }
    public List<Detection>? Detections { get; set; }
    public double ProcessingLatencyMs { get; set; }
    public double Confidence { get; set; }
    public string ProcessingMode { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Detection result from vision analysis
/// </summary>
public class Detection
{
    public string Type { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public Dictionary<string, object>? BoundingBox { get; set; }
    public Dictionary<string, object>? Properties { get; set; }
} 