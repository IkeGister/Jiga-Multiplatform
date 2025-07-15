using JigaMultiplatform.Models;

namespace JigaMultiplatform.Services;

public interface IJigaApiService
{
    /// <summary>
    /// Base URL for the JIGA API
    /// </summary>
    string BaseUrl { get; set; }

    /// <summary>
    /// Authentication token for API requests
    /// </summary>
    string? AuthToken { get; set; }

    // Session Management
    Task<SessionResponse> CreateSessionAsync(SessionRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<SessionData>> GetSessionStatusAsync(string sessionId, CancellationToken cancellationToken = default);
    Task<ApiResponse<SessionMetrics>> GetSessionMetricsAsync(string sessionId, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> ConfigureSessionAsync(string sessionId, SessionConfiguration config, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> DeleteSessionAsync(string sessionId, CancellationToken cancellationToken = default);

    // Agent Management
    Task<ApiResponse<List<Agent>>> GetAvailableAgentsAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<Agent>> GetAgentAsync(string agentId, CancellationToken cancellationToken = default);
    Task<ApiResponse<CapabilitiesResponse>> GetAgentCapabilitiesAsync(string sessionId, CancellationToken cancellationToken = default);

    // Skill Management
    Task<ApiResponse<bool>> EnableSkillAsync(string sessionId, string skillName, Dictionary<string, object>? configuration = null, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> DisableSkillAsync(string sessionId, string skillName, CancellationToken cancellationToken = default);
    Task<ApiResponse<List<string>>> GetAvailableSkillsAsync(string sessionId, CancellationToken cancellationToken = default);
    Task<ApiResponse<List<string>>> GetEnabledSkillsAsync(string sessionId, CancellationToken cancellationToken = default);

    // High-Speed Vision
    Task<ApiResponse<CapabilitiesResponse>> GetHighSpeedCapabilitiesAsync(string sessionId, CancellationToken cancellationToken = default);
    Task<ApiResponse<SessionMetrics>> GetHighSpeedMetricsAsync(string sessionId, CancellationToken cancellationToken = default);
    Task<ApiResponse<string>> GetHighSpeedStatusAsync(string sessionId, CancellationToken cancellationToken = default);

    // Audio Processing
    Task<ApiResponse<AudioResponse>> ProcessAudioAsync(string sessionId, AudioProcessRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<AudioResponse>> ProcessAudioWithResponseAsync(string sessionId, AudioProcessRequest request, CancellationToken cancellationToken = default);

    // Model Provider Configuration
    Task<ApiResponse<bool>> SetModelProviderAsync(string sessionId, string provider, CancellationToken cancellationToken = default);
    Task<ApiResponse<string>> GetModelProviderAsync(string sessionId, CancellationToken cancellationToken = default);
    Task<ApiResponse<List<string>>> GetAvailableModelProvidersAsync(CancellationToken cancellationToken = default);

    // Voice Configuration
    Task<ApiResponse<bool>> SetVoiceConfigurationAsync(string sessionId, VoiceChatConfiguration config, CancellationToken cancellationToken = default);
    Task<ApiResponse<VoiceChatConfiguration>> GetVoiceConfigurationAsync(string sessionId, CancellationToken cancellationToken = default);
    Task<ApiResponse<List<string>>> GetAvailableVoicesAsync(CancellationToken cancellationToken = default);

    // Health and Diagnostics
    Task<ApiResponse<string>> GetHealthAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<Dictionary<string, object>>> GetDiagnosticsAsync(string sessionId, CancellationToken cancellationToken = default);

    // Events and Notifications
    event EventHandler<ApiErrorEventArgs>? ApiError;
    event EventHandler<ConnectionStatusEventArgs>? ConnectionStatusChanged;
}

public class ApiErrorEventArgs : EventArgs
{
    public string? ErrorMessage { get; set; }
    public Exception? Exception { get; set; }
    public string? RequestUrl { get; set; }
    public int? StatusCode { get; set; }
}

public class ConnectionStatusEventArgs : EventArgs
{
    public bool IsConnected { get; set; }
    public string? StatusMessage { get; set; }
} 