using System.Text.Json.Serialization;

namespace JigaMultiplatform.Models;

// API Request Models
public class SessionRequest
{
    [JsonPropertyName("user_id")]
    public string UserId { get; set; } = string.Empty;

    [JsonPropertyName("agent_id")]
    public string AgentId { get; set; } = string.Empty;

    [JsonPropertyName("is_admin")]
    public bool IsAdmin { get; set; }

    [JsonPropertyName("permissions")]
    public string Permissions { get; set; } = "configure";

    [JsonPropertyName("preferences")]
    public Dictionary<string, object> Preferences { get; set; } = new();

    [JsonPropertyName("high_speed_vision")]
    public bool HighSpeedVision { get; set; }

    [JsonPropertyName("high_speed_mode")]
    public string HighSpeedMode { get; set; } = "balanced";

    [JsonPropertyName("high_speed_config")]
    public HighSpeedConfiguration? HighSpeedConfig { get; set; }

    [JsonPropertyName("voice_chat_enabled")]
    public bool VoiceChatEnabled { get; set; }

    [JsonPropertyName("auto_connect_audio")]
    public bool AutoConnectAudio { get; set; }

    [JsonPropertyName("voice_chat_config")]
    public VoiceChatConfiguration? VoiceChatConfig { get; set; }

    [JsonPropertyName("video_input_source")]
    public string VideoInputSource { get; set; } = "direct_screen";

    [JsonPropertyName("video_source_config")]
    public VideoSourceConfiguration? VideoSourceConfig { get; set; }
}

public class HighSpeedConfiguration
{
    [JsonPropertyName("target_fps")]
    public double TargetFps { get; set; } = 3.0;

    [JsonPropertyName("analysis_interval")]
    public double AnalysisInterval { get; set; } = 0.5;

    [JsonPropertyName("max_concurrent_analyses")]
    public int MaxConcurrentAnalyses { get; set; } = 10;

    [JsonPropertyName("compression_level")]
    public string CompressionLevel { get; set; } = "high";

    [JsonPropertyName("enable_caching")]
    public bool EnableCaching { get; set; } = true;
}

public class SkillToggleRequest
{
    [JsonPropertyName("skill_name")]
    public string SkillName { get; set; } = string.Empty;

    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; }

    [JsonPropertyName("configuration")]
    public Dictionary<string, object> Configuration { get; set; } = new();
}

public class AudioProcessRequest
{
    [JsonPropertyName("audio_data")]
    public string AudioData { get; set; } = string.Empty; // Base64 encoded

    [JsonPropertyName("format")]
    public string Format { get; set; } = "wav";

    [JsonPropertyName("sample_rate")]
    public int SampleRate { get; set; } = 22050;

    [JsonPropertyName("channels")]
    public int Channels { get; set; } = 1;

    [JsonPropertyName("with_response")]
    public bool WithResponse { get; set; } = true;
}

// API Response Models
public class SessionResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("data")]
    public SessionData? Data { get; set; }

    [JsonPropertyName("error")]
    public string? Error { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    // Helper property for SessionId
    [JsonIgnore]
    public string SessionId => Data?.SessionId ?? string.Empty;

    // Helper property for WebSocketEndpoints
    [JsonIgnore]
    public List<string> WebSocketEndpoints => Data?.Endpoints != null && Data.Endpoints.WebSocket != null
        ? new List<string> { Data.Endpoints.WebSocket }
        : new List<string>();
}

public class SessionData
{
    [JsonPropertyName("session_id")]
    public string SessionId { get; set; } = string.Empty;

    [JsonPropertyName("user_id")]
    public string UserId { get; set; } = string.Empty;

    [JsonPropertyName("agent")]
    public Agent? Agent { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = "created";

    [JsonPropertyName("endpoints")]
    public EndpointsData? Endpoints { get; set; }

    [JsonPropertyName("configuration")]
    public SessionConfiguration? Configuration { get; set; }

    [JsonPropertyName("capabilities")]
    public List<string> Capabilities { get; set; } = new();

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [JsonPropertyName("agent_id")]
    public string AgentId { get; set; } = string.Empty;
}

public class EndpointsData
{
    [JsonPropertyName("websocket")]
    public string? WebSocket { get; set; }

    [JsonPropertyName("high_speed_stream")]
    public string? HighSpeedStream { get; set; }

    [JsonPropertyName("vision_stream")]
    public string? VisionStream { get; set; }

    [JsonPropertyName("audio_stream")]
    public string? AudioStream { get; set; }

    [JsonPropertyName("direct_video_stream")]
    public string? DirectVideoStream { get; set; }
}

public class ApiResponse<T>
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("data")]
    public T? Data { get; set; }

    [JsonPropertyName("error")]
    public string? Error { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    // Add StatusCode property for HTTP status
    public int StatusCode { get; set; }

    [JsonPropertyName("error_details")]
    public string? ErrorDetails { get; set; }
}

public class CapabilitiesResponse
{
    [JsonPropertyName("high_speed_vision")]
    public bool HighSpeedVision { get; set; }

    [JsonPropertyName("voice_chat")]
    public bool VoiceChat { get; set; }

    [JsonPropertyName("team_messaging")]
    public bool TeamMessaging { get; set; }

    [JsonPropertyName("specialist_detection")]
    public bool SpecialistDetection { get; set; }

    [JsonPropertyName("learning_mode")]
    public bool LearningMode { get; set; }

    [JsonPropertyName("supported_modes")]
    public List<string> SupportedModes { get; set; } = new();

    [JsonPropertyName("max_fps")]
    public double MaxFps { get; set; }

    [JsonPropertyName("supported_video_sources")]
    public List<string> SupportedVideoSources { get; set; } = new();

    [JsonPropertyName("enabled_skills")]
    public List<string> EnabledSkills { get; set; } = new();
}

public class AudioResponse
{
    [JsonPropertyName("audio_data")]
    public string? AudioData { get; set; } // Base64 encoded

    [JsonPropertyName("transcript")]
    public string? Transcript { get; set; }

    [JsonPropertyName("response_text")]
    public string? ResponseText { get; set; }

    [JsonPropertyName("duration")]
    public double Duration { get; set; }

    [JsonPropertyName("format")]
    public string Format { get; set; } = "wav";

    [JsonPropertyName("sample_rate")]
    public int SampleRate { get; set; } = 22050;

    [JsonPropertyName("text")]
    public string? Text { get; set; }
}

// WebSocket Message Models
public class WebSocketMessage
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("data")]
    public object? Data { get; set; }

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    [JsonPropertyName("session_id")]
    public string? SessionId { get; set; }
}

public class FrameMessage
{
    [JsonPropertyName("frame_data")]
    public string FrameData { get; set; } = string.Empty; // Base64 encoded

    [JsonPropertyName("format")]
    public string Format { get; set; } = "png";

    [JsonPropertyName("width")]
    public int Width { get; set; }

    [JsonPropertyName("height")]
    public int Height { get; set; }

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    [JsonPropertyName("frame_id")]
    public string FrameId { get; set; } = Guid.NewGuid().ToString();
}

public class VisionAnalysisResult
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("data")]
    public Dictionary<string, object>? Data { get; set; }

    [JsonPropertyName("detections")]
    public List<Detection> Detections { get; set; } = new();

    [JsonPropertyName("processing_latency_ms")]
    public double ProcessingLatencyMs { get; set; }

    [JsonPropertyName("processing_mode")]
    public string ProcessingMode { get; set; } = string.Empty;

    [JsonPropertyName("analysis_time")]
    public double AnalysisTime { get; set; }

    [JsonPropertyName("confidence")]
    public double Confidence { get; set; }

    [JsonPropertyName("insights")]
    public List<string> Insights { get; set; } = new();

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class Detection
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("confidence")]
    public double Confidence { get; set; }

    [JsonPropertyName("bounding_box")]
    public Dictionary<string, object>? BoundingBox { get; set; }

    [JsonPropertyName("properties")]
    public Dictionary<string, object>? Properties { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("category")]
    public string Category { get; set; } = string.Empty;
}

public class BoundingBox
{
    [JsonPropertyName("x")]
    public int X { get; set; }

    [JsonPropertyName("y")]
    public int Y { get; set; }

    [JsonPropertyName("width")]
    public int Width { get; set; }

    [JsonPropertyName("height")]
    public int Height { get; set; }
}

public class VoiceChatConfiguration
{
    // See JIGA-API-SPECS.md: VoiceChatConfig
    [JsonPropertyName("response_type")]
    public string ResponseType { get; set; } = "base64_audio";

    [JsonPropertyName("audio_format")]
    public string AudioFormat { get; set; } = "wav";

    [JsonPropertyName("sample_rate")]
    public int SampleRate { get; set; } = 22050;

    [JsonPropertyName("channels")]
    public int Channels { get; set; } = 1;

    [JsonPropertyName("auto_voice_activation")]
    public bool AutoVoiceActivation { get; set; } = true;

    [JsonPropertyName("voice_activation_threshold")]
    public double VoiceActivationThreshold { get; set; } = 0.3;

    [JsonPropertyName("continuous_listening")]
    public bool ContinuousListening { get; set; } = true;

    [JsonPropertyName("enable_team_messaging")]
    public bool EnableTeamMessaging { get; set; } = true;

    [JsonPropertyName("voice_quality")]
    public string VoiceQuality { get; set; } = "standard";

    [JsonPropertyName("enable_noise_suppression")]
    public bool EnableNoiseSuppression { get; set; } = true;

    [JsonPropertyName("enable_echo_cancellation")]
    public bool EnableEchoCancellation { get; set; } = true;

    [JsonPropertyName("max_recording_duration")]
    public int MaxRecordingDuration { get; set; } = 30;

    [JsonPropertyName("silence_timeout")]
    public double SilenceTimeout { get; set; } = 2.0;

    [JsonPropertyName("voice_id")]
    public string? VoiceId { get; set; }

    [JsonPropertyName("nationality")]
    public string? Nationality { get; set; }

    [JsonPropertyName("language")]
    public string? Language { get; set; }
}

// Session customizations for overriding agent defaults
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