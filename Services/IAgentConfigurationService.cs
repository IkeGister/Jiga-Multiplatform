using JigaMultiplatform.Models;

namespace JigaMultiplatform.Services;

/// <summary>
/// Service to convert agent configurations to API session requests and WebSocket endpoints.
/// Equivalent to JavaScript AgentConfigurationInterpreter class.
/// </summary>
public interface IAgentConfigurationService
{
    /// <summary>
    /// Convert agent config to API session creation request
    /// </summary>
    /// <param name="agent">Agent configuration</param>
    /// <param name="userId">User identifier</param>
    /// <param name="videoSource">Video input source (twitch, youtube, direct_screen)</param>
    /// <param name="customizations">Optional customizations to override defaults</param>
    /// <returns>Session creation request payload</returns>
    SessionRequest ToSessionCreationRequest(
        Agent agent, 
        string userId, 
        VideoInputSource videoSource = VideoInputSource.DirectScreen,
        SessionCustomizations? customizations = null);

    /// <summary>
    /// Extract high-speed vision configuration from agent
    /// </summary>
    /// <param name="agent">Agent configuration</param>
    /// <returns>High-speed configuration object</returns>
    HighSpeedConfiguration ExtractHighSpeedConfig(Agent agent);

    /// <summary>
    /// Extract voice chat configuration from agent
    /// </summary>
    /// <param name="agent">Agent configuration</param>
    /// <param name="customizations">User customizations</param>
    /// <returns>Voice chat configuration</returns>
    VoiceChatConfiguration ExtractVoiceConfig(Agent agent, SessionCustomizations? customizations = null);

    /// <summary>
    /// Extract video source configuration
    /// </summary>
    /// <param name="videoSource">Video input source type</param>
    /// <param name="customizations">User customizations</param>
    /// <returns>Video source configuration</returns>
    VideoSourceConfiguration ExtractVideoSourceConfig(VideoInputSource videoSource, SessionCustomizations? customizations = null);

    /// <summary>
    /// Get WebSocket endpoints for a session based on agent capabilities
    /// </summary>
    /// <param name="sessionId">Session identifier</param>
    /// <param name="agent">Agent configuration</param>
    /// <param name="baseUrl">Base WebSocket URL</param>
    /// <returns>WebSocket endpoint configuration</returns>
    WebSocketEndpoints GetWebSocketEndpoints(string sessionId, Agent agent, string baseUrl = "ws://localhost:8000");

    /// <summary>
    /// Get control buttons/actions based on agent toolkit
    /// </summary>
    /// <param name="agent">Agent configuration</param>
    /// <returns>List of available control actions</returns>
    List<AgentControlAction> GetControlActions(Agent agent);

    /// <summary>
    /// Get UI metrics configuration based on agent capabilities
    /// </summary>
    /// <param name="agent">Agent configuration</param>
    /// <returns>List of metrics to display</returns>
    List<MetricConfiguration> GetMetricsConfiguration(Agent agent);
}

/// <summary>
/// Session customizations for overriding agent defaults
/// </summary>
public class SessionCustomizations
{
    public bool? IsAdmin { get; set; }
    public string? Permissions { get; set; }
    public Dictionary<string, object>? Preferences { get; set; }
    public string? VoiceId { get; set; }
    public string? TwitchChannel { get; set; }
    public string? YouTubeUrl { get; set; }
    public string? TwitchQuality { get; set; }
    public string? YouTubeQuality { get; set; }
}

/// <summary>
/// High-speed vision configuration
/// </summary>
public class HighSpeedConfiguration
{
    public double TargetFps { get; set; } = 3.0;
    public double AnalysisInterval { get; set; } = 0.5;
    public int MaxConcurrentAnalyses { get; set; } = 10;
    public string CompressionLevel { get; set; } = "high";
    public bool EnableCaching { get; set; } = true;
}

/// <summary>
/// Video source configuration
/// </summary>
public class VideoSourceConfiguration
{
    public VideoInputSource Source { get; set; }
    public Dictionary<string, object> Properties { get; set; } = new();
}

/// <summary>
/// WebSocket endpoints configuration
/// </summary>
public class WebSocketEndpoints
{
    public string BaseUrl { get; set; } = string.Empty;
    public Dictionary<string, WebSocketConnection> Connections { get; set; } = new();
}

/// <summary>
/// Individual WebSocket connection configuration
/// </summary>
public class WebSocketConnection
{
    public string Url { get; set; } = string.Empty;
    public WebSocketType Type { get; set; }
    public int Priority { get; set; }
    public bool Enabled { get; set; } = true;
}

/// <summary>
/// WebSocket connection types
/// </summary>
public enum WebSocketType
{
    Vision,
    HighSpeed,
    Audio,
    TeamMessaging
}

/// <summary>
/// Agent control actions based on toolkit
/// </summary>
public class AgentControlAction
{
    public string Id { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public string Type { get; set; } = "secondary"; // primary, secondary, danger
    public string Icon { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;
    public string Tooltip { get; set; } = string.Empty;
}

/// <summary>
/// Metric configuration for UI display
/// </summary>
public class MetricConfiguration
{
    public string Id { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Format { get; set; } = "number"; // number, time, percentage
    public object DefaultValue { get; set; } = 0;
    public bool IsRealTime { get; set; } = true;
} 