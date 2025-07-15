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
    public string CapabilitiesText => string.Join(", ", GetCapabilities());

    // UI Helper Properties for AgentCardControl
    [JsonIgnore]
    public bool IsOnline { get; set; } = false; // Will be updated based on session status
    
    [JsonIgnore]
    public string StatusText => IsOnline ? "Online" : "Offline";
    
    [JsonIgnore]
    public bool HasSpecialization => Specialization?.Games?.Any() == true;
    
    [JsonIgnore]
    public string PrimaryGame => Specialization?.Games?.FirstOrDefault() ?? "";
    
    [JsonIgnore]
    public bool HasSpecializationFocus => !string.IsNullOrEmpty(Specialization?.Focus);
    
    [JsonIgnore]
    public string SpecializationFocus => Specialization?.Focus ?? "";
    
    [JsonIgnore]
    public bool HasAvatarUrl => !string.IsNullOrEmpty(AvatarUrl);

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
} 