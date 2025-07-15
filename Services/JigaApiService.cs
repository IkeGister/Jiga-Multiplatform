using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using JigaMultiplatform.Models;

namespace JigaMultiplatform.Services;

public class JigaApiService : IJigaApiService, IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;
    private bool _disposed;

    public string BaseUrl { get; set; } = "http://localhost:8000";
    public string? AuthToken { get; set; }

    // Events
    public event EventHandler<ApiErrorEventArgs>? ApiError;
    public event EventHandler<ConnectionStatusEventArgs>? ConnectionStatusChanged;

    public JigaApiService(HttpClient? httpClient = null)
    {
        _httpClient = httpClient ?? new HttpClient();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            WriteIndented = false,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        // Configure HTTP client
        _httpClient.Timeout = TimeSpan.FromSeconds(30);
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "JIGA-MAUI/1.0");
    }

    #region Session Management

    public async Task<SessionResponse> CreateSessionAsync(SessionRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await PostAsync<SessionResponse>("/api/v1/jiga/sessions", request, cancellationToken);
            OnConnectionStatusChanged(true, "Session created successfully");
            return response.Data ?? throw new InvalidOperationException("Session creation returned null data");
        }
        catch (Exception ex)
        {
            OnApiError("Failed to create session", ex);
            throw;
        }
    }

    public async Task<ApiResponse<SessionData>> GetSessionStatusAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await GetAsync<SessionData>($"/api/v1/jiga/sessions/{sessionId}/status", cancellationToken);
        }
        catch (Exception ex)
        {
            OnApiError($"Failed to get session status for {sessionId}", ex);
            throw;
        }
    }

    public async Task<ApiResponse<SessionMetrics>> GetSessionMetricsAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await GetAsync<SessionMetrics>($"/api/v1/jiga/sessions/{sessionId}/metrics", cancellationToken);
        }
        catch (Exception ex)
        {
            OnApiError($"Failed to get session metrics for {sessionId}", ex);
            throw;
        }
    }

    public async Task<ApiResponse<bool>> ConfigureSessionAsync(string sessionId, SessionConfiguration config, CancellationToken cancellationToken = default)
    {
        try
        {
            return await PutAsync<bool>($"/api/v1/jiga/sessions/{sessionId}/configure", config, cancellationToken);
        }
        catch (Exception ex)
        {
            OnApiError($"Failed to configure session {sessionId}", ex);
            throw;
        }
    }

    public async Task<ApiResponse<bool>> DeleteSessionAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await DeleteAsync<bool>($"/api/v1/jiga/sessions/{sessionId}", cancellationToken);
        }
        catch (Exception ex)
        {
            OnApiError($"Failed to delete session {sessionId}", ex);
            throw;
        }
    }

    #endregion

    #region Agent Management

    public async Task<ApiResponse<List<Agent>>> GetAvailableAgentsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await GetAsync<List<Agent>>("/api/v1/agents", cancellationToken);
        }
        catch (Exception ex)
        {
            OnApiError("Failed to get available agents", ex);
            throw;
        }
    }

    public async Task<ApiResponse<Agent>> GetAgentAsync(string agentId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await GetAsync<Agent>($"/api/v1/agents/{agentId}", cancellationToken);
        }
        catch (Exception ex)
        {
            OnApiError($"Failed to get agent {agentId}", ex);
            throw;
        }
    }

    public async Task<ApiResponse<CapabilitiesResponse>> GetAgentCapabilitiesAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await GetAsync<CapabilitiesResponse>($"/api/v1/jiga/sessions/{sessionId}/capabilities", cancellationToken);
        }
        catch (Exception ex)
        {
            OnApiError($"Failed to get capabilities for session {sessionId}", ex);
            throw;
        }
    }

    #endregion

    #region Skill Management

    public async Task<ApiResponse<bool>> EnableSkillAsync(string sessionId, string skillName, Dictionary<string, object>? configuration = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new { skill_name = skillName, configuration = configuration ?? new Dictionary<string, object>() };
            return await PostAsync<bool>($"/api/v1/jiga/sessions/{sessionId}/skills/enable", request, cancellationToken);
        }
        catch (Exception ex)
        {
            OnApiError($"Failed to enable skill {skillName} for session {sessionId}", ex);
            throw;
        }
    }

    public async Task<ApiResponse<bool>> DisableSkillAsync(string sessionId, string skillName, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new { skill_name = skillName };
            return await PostAsync<bool>($"/api/v1/jiga/sessions/{sessionId}/skills/disable", request, cancellationToken);
        }
        catch (Exception ex)
        {
            OnApiError($"Failed to disable skill {skillName} for session {sessionId}", ex);
            throw;
        }
    }

    public async Task<ApiResponse<List<string>>> GetAvailableSkillsAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await GetAsync<List<string>>($"/api/v1/jiga/sessions/{sessionId}/skills/available", cancellationToken);
        }
        catch (Exception ex)
        {
            OnApiError($"Failed to get available skills for session {sessionId}", ex);
            throw;
        }
    }

    public async Task<ApiResponse<List<string>>> GetEnabledSkillsAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await GetAsync<List<string>>($"/api/v1/jiga/sessions/{sessionId}/skills/enabled", cancellationToken);
        }
        catch (Exception ex)
        {
            OnApiError($"Failed to get enabled skills for session {sessionId}", ex);
            throw;
        }
    }

    #endregion

    #region High-Speed Vision

    public async Task<ApiResponse<CapabilitiesResponse>> GetHighSpeedCapabilitiesAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await GetAsync<CapabilitiesResponse>($"/api/v1/jiga/sessions/{sessionId}/high-speed/capabilities", cancellationToken);
        }
        catch (Exception ex)
        {
            OnApiError($"Failed to get high-speed capabilities for session {sessionId}", ex);
            throw;
        }
    }

    public async Task<ApiResponse<SessionMetrics>> GetHighSpeedMetricsAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await GetAsync<SessionMetrics>($"/api/v1/jiga/sessions/{sessionId}/high-speed/metrics", cancellationToken);
        }
        catch (Exception ex)
        {
            OnApiError($"Failed to get high-speed metrics for session {sessionId}", ex);
            throw;
        }
    }

    public async Task<ApiResponse<string>> GetHighSpeedStatusAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await GetAsync<string>($"/api/v1/jiga/sessions/{sessionId}/high-speed/status", cancellationToken);
        }
        catch (Exception ex)
        {
            OnApiError($"Failed to get high-speed status for session {sessionId}", ex);
            throw;
        }
    }

    #endregion

    #region Audio Processing

    public async Task<ApiResponse<AudioResponse>> ProcessAudioAsync(string sessionId, AudioProcessRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            return await PostAsync<AudioResponse>($"/api/v1/jiga/sessions/{sessionId}/audio/process", request, cancellationToken);
        }
        catch (Exception ex)
        {
            OnApiError($"Failed to process audio for session {sessionId}", ex);
            throw;
        }
    }

    public async Task<ApiResponse<AudioResponse>> ProcessAudioWithResponseAsync(string sessionId, AudioProcessRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            return await PostAsync<AudioResponse>($"/api/v1/jiga/sessions/{sessionId}/audio/process-with-response", request, cancellationToken);
        }
        catch (Exception ex)
        {
            OnApiError($"Failed to process audio with response for session {sessionId}", ex);
            throw;
        }
    }

    #endregion

    #region Model Provider Configuration

    public async Task<ApiResponse<bool>> SetModelProviderAsync(string sessionId, string provider, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new { provider };
            return await PostAsync<bool>($"/api/v1/jiga/sessions/{sessionId}/model-provider", request, cancellationToken);
        }
        catch (Exception ex)
        {
            OnApiError($"Failed to set model provider for session {sessionId}", ex);
            throw;
        }
    }

    public async Task<ApiResponse<string>> GetModelProviderAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await GetAsync<string>($"/api/v1/jiga/sessions/{sessionId}/model-provider", cancellationToken);
        }
        catch (Exception ex)
        {
            OnApiError($"Failed to get model provider for session {sessionId}", ex);
            throw;
        }
    }

    public async Task<ApiResponse<List<string>>> GetAvailableModelProvidersAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await GetAsync<List<string>>("/api/v1/model-providers", cancellationToken);
        }
        catch (Exception ex)
        {
            OnApiError("Failed to get available model providers", ex);
            throw;
        }
    }

    #endregion

    #region Voice Configuration

    public async Task<ApiResponse<bool>> SetVoiceConfigurationAsync(string sessionId, VoiceChatConfiguration config, CancellationToken cancellationToken = default)
    {
        try
        {
            return await PostAsync<bool>($"/api/v1/jiga/sessions/{sessionId}/voice-config", config, cancellationToken);
        }
        catch (Exception ex)
        {
            OnApiError($"Failed to set voice configuration for session {sessionId}", ex);
            throw;
        }
    }

    public async Task<ApiResponse<VoiceChatConfiguration>> GetVoiceConfigurationAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await GetAsync<VoiceChatConfiguration>($"/api/v1/jiga/sessions/{sessionId}/voice-config", cancellationToken);
        }
        catch (Exception ex)
        {
            OnApiError($"Failed to get voice configuration for session {sessionId}", ex);
            throw;
        }
    }

    public async Task<ApiResponse<List<string>>> GetAvailableVoicesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await GetAsync<List<string>>("/api/v1/voices", cancellationToken);
        }
        catch (Exception ex)
        {
            OnApiError("Failed to get available voices", ex);
            throw;
        }
    }

    #endregion

    #region Health and Diagnostics

    public async Task<ApiResponse<string>> GetHealthAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await GetAsync<string>("/api/v1/health", cancellationToken);
        }
        catch (Exception ex)
        {
            OnApiError("Failed to get health status", ex);
            OnConnectionStatusChanged(false, "Health check failed");
            throw;
        }
    }

    public async Task<ApiResponse<Dictionary<string, object>>> GetDiagnosticsAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await GetAsync<Dictionary<string, object>>($"/api/v1/jiga/sessions/{sessionId}/diagnostics", cancellationToken);
        }
        catch (Exception ex)
        {
            OnApiError($"Failed to get diagnostics for session {sessionId}", ex);
            throw;
        }
    }

    #endregion

    #region HTTP Helper Methods

    private async Task<ApiResponse<T>> GetAsync<T>(string endpoint, CancellationToken cancellationToken = default)
    {
        var request = CreateHttpRequestMessage(HttpMethod.Get, endpoint);
        return await SendRequestAsync<T>(request, cancellationToken);
    }

    private async Task<ApiResponse<T>> PostAsync<T>(string endpoint, object data, CancellationToken cancellationToken = default)
    {
        var request = CreateHttpRequestMessage(HttpMethod.Post, endpoint);
        request.Content = JsonContent.Create(data, options: _jsonOptions);
        return await SendRequestAsync<T>(request, cancellationToken);
    }

    private async Task<ApiResponse<T>> PutAsync<T>(string endpoint, object data, CancellationToken cancellationToken = default)
    {
        var request = CreateHttpRequestMessage(HttpMethod.Put, endpoint);
        request.Content = JsonContent.Create(data, options: _jsonOptions);
        return await SendRequestAsync<T>(request, cancellationToken);
    }

    private async Task<ApiResponse<T>> DeleteAsync<T>(string endpoint, CancellationToken cancellationToken = default)
    {
        var request = CreateHttpRequestMessage(HttpMethod.Delete, endpoint);
        return await SendRequestAsync<T>(request, cancellationToken);
    }

    private HttpRequestMessage CreateHttpRequestMessage(HttpMethod method, string endpoint)
    {
        var request = new HttpRequestMessage(method, $"{BaseUrl.TrimEnd('/')}{endpoint}");
        
        if (!string.IsNullOrEmpty(AuthToken))
        {
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", AuthToken);
        }

        return request;
    }

    private async Task<ApiResponse<T>> SendRequestAsync<T>(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        try
        {
            using var response = await _httpClient.SendAsync(request, cancellationToken);
            var content = await response.Content.ReadAsStringAsync(cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var data = JsonSerializer.Deserialize<T>(content, _jsonOptions);
                return new ApiResponse<T>
                {
                    Success = true,
                    Data = data,
                    StatusCode = (int)response.StatusCode,
                    Message = "Success"
                };
            }
            else
            {
                return new ApiResponse<T>
                {
                    Success = false,
                    Data = default,
                    StatusCode = (int)response.StatusCode,
                    Message = $"HTTP {response.StatusCode}: {content}",
                    ErrorDetails = content
                };
            }
        }
        catch (HttpRequestException ex)
        {
            OnConnectionStatusChanged(false, "Connection failed");
            return new ApiResponse<T>
            {
                Success = false,
                Data = default,
                StatusCode = 0,
                Message = "Connection failed",
                ErrorDetails = ex.Message
            };
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Data = default,
                StatusCode = 0,
                Message = "Request timeout",
                ErrorDetails = ex.Message
            };
        }
        catch (JsonException ex)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Data = default,
                StatusCode = 0,
                Message = "Invalid JSON response",
                ErrorDetails = ex.Message
            };
        }
    }

    #endregion

    #region Event Handlers

    private void OnApiError(string message, Exception? exception = null, string? requestUrl = null, int? statusCode = null)
    {
        ApiError?.Invoke(this, new ApiErrorEventArgs
        {
            ErrorMessage = message,
            Exception = exception,
            RequestUrl = requestUrl,
            StatusCode = statusCode
        });
    }

    private void OnConnectionStatusChanged(bool isConnected, string? statusMessage = null)
    {
        ConnectionStatusChanged?.Invoke(this, new ConnectionStatusEventArgs
        {
            IsConnected = isConnected,
            StatusMessage = statusMessage
        });
    }

    #endregion

    #region IDisposable

    public void Dispose()
    {
        if (!_disposed)
        {
            _httpClient?.Dispose();
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }

    #endregion
} 