using System.Text.Json.Serialization;

namespace JigaMultiplatform.Models;

public class JigaSession
{
    [JsonPropertyName("session_id")]
    public string SessionId { get; set; } = string.Empty;

    [JsonPropertyName("agent_id")]
    public string AgentId { get; set; } = string.Empty;

    [JsonPropertyName("user_id")]
    public string UserId { get; set; } = string.Empty;

    [JsonPropertyName("agent")]
    public Agent? Agent { get; set; }

    [JsonPropertyName("status")]
    public SessionStatus Status { get; set; } = SessionStatus.Disconnected;

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [JsonPropertyName("configuration")]
    public SessionConfiguration Configuration { get; set; } = new();

    [JsonPropertyName("websocket_endpoints")]
    public List<string> WebSocketEndpoints { get; set; } = new();

    [JsonPropertyName("metrics")]
    public SessionMetrics? Metrics { get; set; }

    // UI Helper Properties
    [JsonIgnore]
    public bool IsConnected => Status == SessionStatus.Connected || Status == SessionStatus.Active;

    [JsonIgnore]
    public bool CanConnect => Status == SessionStatus.Disconnected || Status == SessionStatus.Failed;

    [JsonIgnore]
    public string StatusDisplayText => Status switch
    {
        SessionStatus.Connecting => "Connecting...",
        SessionStatus.Connected => "Connected",
        SessionStatus.Active => "Active Session",
        SessionStatus.Disconnected => "Disconnected",
        SessionStatus.Failed => "Connection Failed",
        _ => "Unknown"
    };

    [JsonIgnore]
    public TimeSpan SessionDuration => DateTime.UtcNow - CreatedAt;

    [JsonIgnore]
    public string DurationText => SessionDuration.ToString(@"hh\:mm\:ss");
}

public class SessionConfiguration
{
    [JsonPropertyName("high_speed_vision")]
    public bool HighSpeedVision { get; set; }

    [JsonPropertyName("high_speed_mode")]
    public string HighSpeedMode { get; set; } = "balanced"; // ultra_fast, balanced, quality_focused

    [JsonPropertyName("voice_chat_enabled")]
    public bool VoiceChatEnabled { get; set; }

    [JsonPropertyName("auto_connect_audio")]
    public bool AutoConnectAudio { get; set; }

    [JsonPropertyName("video_input_source")]
    public string VideoInputSource { get; set; } = "direct_screen"; // twitch, youtube, direct_screen

    [JsonPropertyName("video_source_config")]
    public VideoSourceConfiguration VideoSourceConfig { get; set; } = new();

    [JsonPropertyName("voice_chat_config")]
    public VoiceChatConfiguration VoiceChatConfig { get; set; } = new();

    [JsonPropertyName("permissions")]
    public string Permissions { get; set; } = "configure"; // read_only, configure, admin, full_control

    [JsonPropertyName("is_admin")]
    public bool IsAdmin { get; set; }

    [JsonPropertyName("extended_properties")]
    public Dictionary<string, object> ExtendedProperties { get; set; } = new();
}

public class VideoSourceConfiguration
{
    [JsonPropertyName("source")]
    public string Source { get; set; } = string.Empty;

    [JsonPropertyName("properties")]
    public Dictionary<string, object> Properties { get; set; } = new();

    [JsonPropertyName("twitch_channel")]
    public string? TwitchChannel { get; set; }

    [JsonPropertyName("youtube_url")]
    public string? YouTubeUrl { get; set; }

    [JsonPropertyName("screen_capture_config")]
    public ScreenCaptureConfiguration ScreenCaptureConfig { get; set; } = new();
}

public class ScreenCaptureConfiguration
{
    [JsonPropertyName("capture_region")]
    public string CaptureRegion { get; set; } = "full_screen"; // full_screen, custom_region

    [JsonPropertyName("capture_fps")]
    public int CaptureFps { get; set; } = 30;

    [JsonPropertyName("compression_quality")]
    public int CompressionQuality { get; set; } = 85;
}

public class SessionMetrics
{
    [JsonPropertyName("frames_processed")]
    public int FramesProcessed { get; set; }

    [JsonPropertyName("average_analysis_time")]
    public double AverageAnalysisTime { get; set; }

    [JsonPropertyName("detections_count")]
    public int DetectionsCount { get; set; }

    [JsonPropertyName("connected_time")]
    public TimeSpan ConnectedTime { get; set; }

    [JsonPropertyName("last_activity")]
    public DateTime LastActivity { get; set; } = DateTime.UtcNow;

    [JsonPropertyName("websocket_messages_sent")]
    public int WebSocketMessagesSent { get; set; }

    [JsonPropertyName("websocket_messages_received")]
    public int WebSocketMessagesReceived { get; set; }

    [JsonPropertyName("audio_messages_sent")]
    public int AudioMessagesSent { get; set; }

    [JsonPropertyName("audio_messages_received")]
    public int AudioMessagesReceived { get; set; }

    // --- Added for ViewModel/API compatibility ---
    [JsonPropertyName("status")]
    public string? Status { get; set; } = "Initializing";

    [JsonPropertyName("messages_sent")]
    public int MessagesSent { get; set; }

    [JsonPropertyName("voice_interactions")]
    public int VoiceInteractions { get; set; }

    [JsonPropertyName("frame_rate")]
    public double? FrameRate { get; set; }

    [JsonPropertyName("message_rate")]
    public double? MessageRate { get; set; }

    [JsonPropertyName("processing_speed")]
    public double? ProcessingSpeed { get; set; }

    [JsonPropertyName("session_start_time")]
    public DateTime? SessionStartTime { get; set; }
    // --- End added ---

    // UI Helper Properties
    [JsonIgnore]
    public string AnalysisTimeText => AverageAnalysisTime > 0 ? $"{AverageAnalysisTime:F1}ms" : "N/A";

    [JsonIgnore]
    public string ConnectedTimeText => ConnectedTime.ToString(@"hh\:mm\:ss");

    [JsonIgnore]
    public string LastActivityText => LastActivity.ToString("HH:mm:ss");

    [JsonIgnore]
    public double FramesPerSecond => ConnectedTime.TotalSeconds > 0 ? FramesProcessed / ConnectedTime.TotalSeconds : 0;

    [JsonIgnore]
    public string FpsText => $"{FramesPerSecond:F1} FPS";

    [JsonIgnore]
    public bool IsConnected { get; set; } // Set this based on your connection logic
}

public enum SessionStatus
{
    Disconnected,
    Connecting,
    Connected,
    Active,
    Failed
}

public enum HighSpeedMode
{
    UltraFast,
    Balanced,
    QualityFocused
} 