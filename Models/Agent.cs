using System.Text.Json.Serialization;

namespace JigaMultiplatform.Models;

public class Agent
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("game")]
    public string Game { get; set; } = string.Empty;

    [JsonPropertyName("voiceId")]
    public string VoiceId { get; set; } = string.Empty;

    [JsonPropertyName("nationality")]
    public string Nationality { get; set; } = string.Empty;

    [JsonPropertyName("language")]
    public string Language { get; set; } = string.Empty;

    [JsonPropertyName("rank")]
    public string Rank { get; set; } = "Agent";

    [JsonPropertyName("type")]
    public string Type { get; set; } = "AI Assistant";

    [JsonPropertyName("configuration")]
    public AgentConfiguration Configuration { get; set; } = new();

    [JsonPropertyName("toolkit")]
    public List<string> Toolkit { get; set; } = new();

    [JsonPropertyName("avatarUrl")]
    public string AvatarUrl { get; set; } = string.Empty;

    [JsonPropertyName("isActive")]
    public bool IsActive { get; set; }

    [JsonPropertyName("bio")]
    public string Bio { get; set; } = string.Empty;

    // UI Helper Properties
    [JsonIgnore]
    public string DisplayName => !string.IsNullOrEmpty(Name) ? Name : Id;

    [JsonIgnore]
    public string GameDisplayName => !string.IsNullOrEmpty(Game) ? Game : "General Gaming";

    [JsonIgnore]
    public bool HasHighSpeedVision => Configuration?.HighSpeedVision ?? false;

    [JsonIgnore]
    public bool HasVoiceCapabilities => Configuration?.VoiceEnabled ?? false;

    [JsonIgnore]
    public bool HasVoiceChat => Configuration?.VoiceEnabled ?? false;

    [JsonIgnore]
    public string CapabilitiesText => string.Join(", ", GetCapabilities());

    // UI Helper Properties for AgentCardControl
    [JsonIgnore]
    public bool IsOnline { get; set; } = false; // Will be updated based on session status
    
    [JsonIgnore]
    public string StatusText => IsOnline ? "Online" : "Offline";
    
    // Remove or comment out Specialization references for now
    // public Specialization? Specialization { get; set; }
    
    // [JsonIgnore]
    // public bool HasSpecialization => Specialization?.Games?.Any() == true;
    
    // [JsonIgnore]
    // public string PrimaryGame => Specialization?.Games?.FirstOrDefault() ?? "";
    
    // [JsonIgnore]
    // public bool HasSpecializationFocus => !string.IsNullOrEmpty(Specialization?.Focus);
    
    // [JsonIgnore]
    // public string SpecializationFocus => Specialization?.Focus ?? "";
    
    [JsonIgnore]
    public bool HasAvatarUrl => !string.IsNullOrEmpty(AvatarUrl);

    [JsonIgnore]
    public bool HasSpecialization => !string.IsNullOrEmpty(Game);

    [JsonIgnore]
    public string PrimaryGame => Game;

    [JsonIgnore]
    public bool HasSpecializationFocus => !string.IsNullOrEmpty(Bio);

    [JsonIgnore]
    public string SpecializationFocus => Bio;

    [JsonIgnore]
    public object? AvatarBrush => null; // Placeholder for XAML binding compatibility

    private IEnumerable<string> GetCapabilities()
    {
        var capabilities = new List<string>();
        
        if (HasHighSpeedVision)
            capabilities.Add("High-Speed Vision");
        
        if (HasVoiceCapabilities)
            capabilities.Add("Voice Chat");
        
        if (Toolkit?.Contains("team_messaging") == true)
            capabilities.Add("Team Messaging");
        
        if (Toolkit?.Contains("specialist_detection") == true)
            capabilities.Add("Specialist Detection");
        
        if (Toolkit?.Contains("learning") == true)
            capabilities.Add("Learning Mode");

        return capabilities;
    }
}

public class AgentConfiguration
{
    [JsonPropertyName("high_speed_vision")]
    public bool HighSpeedVision { get; set; }

    [JsonPropertyName("high_speed_mode")]
    public string HighSpeedMode { get; set; } = "balanced"; // ultra_fast, balanced, quality_focused

    [JsonPropertyName("voice_enabled")]
    public bool VoiceEnabled { get; set; }

    [JsonPropertyName("voice_chat_enabled")]
    public bool VoiceChatEnabled { get; set; }

    [JsonPropertyName("auto_voice_activation")]
    public bool AutoVoiceActivation { get; set; } = true;

    [JsonPropertyName("performance_mode")]
    public string PerformanceMode { get; set; } = "balanced";

    [JsonPropertyName("memory_optimization")]
    public bool MemoryOptimization { get; set; } = true;

    [JsonPropertyName("debug_mode")]
    public bool DebugMode { get; set; } = false;

    [JsonPropertyName("auto_connect_audio")]
    public bool AutoConnectAudio { get; set; }

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

    [JsonPropertyName("extended_properties")]
    public Dictionary<string, object> ExtendedProperties { get; set; } = new();

    [JsonPropertyName("vision_quality")]
    public int VisionQuality { get; set; } = 50;

    [JsonPropertyName("voice_sensitivity")]
    public double VoiceSensitivity { get; set; }

    [JsonPropertyName("frame_rate_limit")]
    public int FrameRateLimit { get; set; }

    [JsonPropertyName("video_input_source")]
    public string VideoInputSource { get; set; } = "direct_screen";

    [JsonPropertyName("auto_reconnect")]
    public bool AutoReconnect { get; set; } = true;
} 