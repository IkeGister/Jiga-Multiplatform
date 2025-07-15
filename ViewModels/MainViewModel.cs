using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text.Json;
using System.Windows.Input;
using JigaMultiplatform.Models;
using JigaMultiplatform.Services;

namespace JigaMultiplatform.ViewModels;

/// <summary>
/// Main ViewModel that orchestrates agent selection, session management, and WebSocket connections.
/// Central hub for the JIGA Control Center UI.
/// </summary>
public class MainViewModel : BaseViewModel
{
    private readonly IJigaApiService _apiService;
    private readonly IWebSocketService _webSocketService;
    private readonly IAgentConfigurationService _configService;
    
    private Agent? _selectedAgent;
    private JigaSession? _currentSession;
    private SessionMetrics? _liveMetrics;
    private string _connectionStatus = "Disconnected";
    private bool _isConnecting;
    private bool _isConnected;
    private bool _voiceChatEnabled;
    private bool _highSpeedVisionEnabled;
    private string _selectedVideoSource = "Direct";
    private double _sidebarWidth = 250;
    private double _rightPanelWidth = 300;
    private bool _isSidebarExpanded = true;
    private bool _isRightPanelExpanded = true;

    public MainViewModel(
        IJigaApiService apiService,
        IWebSocketService webSocketService,
        IAgentConfigurationService configService)
    {
        _apiService = apiService;
        _webSocketService = webSocketService;
        _configService = configService;

        // Initialize collections
        AvailableAgents = new ObservableCollection<Agent>();
        ControlActions = new ObservableCollection<AgentControlAction>();
        ChatMessages = new ObservableCollection<ChatMessage>();
        LogEntries = new ObservableCollection<LogEntry>();

        // Initialize commands
        ConnectAgentCommand = new AsyncRelayCommand(ConnectAgentAsync, CanConnectAgent);
        DisconnectCommand = new AsyncRelayCommand(DisconnectAsync, CanDisconnect);
        ToggleVoiceChatCommand = new AsyncRelayCommand(ToggleVoiceChatAsync);
        SelectVideoSourceCommand = new RelayCommand<string>(SelectVideoSource);
        ExecuteControlActionCommand = new AsyncRelayCommand<AgentControlAction>(ExecuteControlActionAsync);
        RefreshAgentsCommand = new AsyncRelayCommand(RefreshAgentsAsync);
        ToggleSidebarCommand = new RelayCommand(ToggleSidebar);
        ToggleRightPanelCommand = new RelayCommand(ToggleRightPanel);

        // Subscribe to service events
        SubscribeToServiceEvents();

        // Load initial data
        _ = Task.Run(InitializeAsync);
    }

    #region Properties

    public ObservableCollection<Agent> AvailableAgents { get; }
    public ObservableCollection<AgentControlAction> ControlActions { get; }
    public ObservableCollection<ChatMessage> ChatMessages { get; }
    public ObservableCollection<LogEntry> LogEntries { get; }
    public ObservableCollection<LogEntry> ActivityLog => LogEntries;

    public Agent? SelectedAgent
    {
        get => _selectedAgent;
        set
        {
            if (SetProperty(ref _selectedAgent, value))
            {
                UpdateControlActions();
                ConnectAgentCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public JigaSession? CurrentSession
    {
        get => _currentSession;
        set => SetProperty(ref _currentSession, value);
    }

    public SessionMetrics? LiveMetrics
    {
        get => _liveMetrics;
        set => SetProperty(ref _liveMetrics, value);
    }

    public string ConnectionStatus
    {
        get => _connectionStatus;
        set => SetProperty(ref _connectionStatus, value);
    }

    public bool IsConnecting
    {
        get => _isConnecting;
        set => SetProperty(ref _isConnecting, value);
    }

    public bool IsConnected
    {
        get => _isConnected;
        set
        {
            if (SetProperty(ref _isConnected, value))
            {
                ConnectAgentCommand.NotifyCanExecuteChanged();
                DisconnectCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public bool VoiceChatEnabled
    {
        get => _voiceChatEnabled;
        set => SetProperty(ref _voiceChatEnabled, value);
    }

    public bool HighSpeedVisionEnabled
    {
        get => _highSpeedVisionEnabled;
        set => SetProperty(ref _highSpeedVisionEnabled, value);
    }

    public string SelectedVideoSource
    {
        get => _selectedVideoSource;
        set => SetProperty(ref _selectedVideoSource, value);
    }

    public double SidebarWidth
    {
        get => _sidebarWidth;
        set => SetProperty(ref _sidebarWidth, value);
    }

    public double RightPanelWidth
    {
        get => _rightPanelWidth;
        set => SetProperty(ref _rightPanelWidth, value);
    }

    public bool IsSidebarExpanded
    {
        get => _isSidebarExpanded;
        set => SetProperty(ref _isSidebarExpanded, value);
    }

    public bool IsRightPanelExpanded
    {
        get => _isRightPanelExpanded;
        set => SetProperty(ref _isRightPanelExpanded, value);
    }

    public bool HasSelectedAgent => SelectedAgent != null;
    public bool IsTwitchSelected => SelectedVideoSource?.Equals("Twitch", StringComparison.OrdinalIgnoreCase) == true;
    public bool IsYouTubeSelected => SelectedVideoSource?.Equals("YouTube", StringComparison.OrdinalIgnoreCase) == true;
    public bool IsDirectSelected => SelectedVideoSource?.Equals("Direct", StringComparison.OrdinalIgnoreCase) == true;
    public bool ShowWelcomeScreen => SelectedAgent == null;
    public bool IsRightPanelVisible => SelectedAgent != null;

    public AgentConfiguration? AgentConfiguration => SelectedAgent?.Configuration;

    public string ConnectionStatusColor
    {
        get
        {
            if (IsConnected)
                return "#00FF88"; // Green
            if (IsConnecting)
                return "#FFD700"; // Gold/Yellow
            if (ConnectionStatus?.ToLower().Contains("error") == true || ConnectionStatus?.ToLower().Contains("fail") == true)
                return "#FF4444"; // Red
            return "#888888"; // Gray/Neutral
        }
    }

    public string ConnectionStatusBorderColor => ConnectionStatusColor;

    public string ConnectionStatusText
    {
        get
        {
            if (IsConnected)
                return "Connected";
            if (IsConnecting)
                return "Connecting...";
            if (ConnectionStatus?.ToLower().Contains("error") == true || ConnectionStatus?.ToLower().Contains("fail") == true)
                return "Error";
            return "Disconnected";
        }
    }

    public string ConnectionStatusTextColor
    {
        get
        {
            if (IsConnected)
                return "#FFFFFF"; // White
            if (IsConnecting)
                return "#000000"; // Black
            if (ConnectionStatus?.ToLower().Contains("error") == true || ConnectionStatus?.ToLower().Contains("fail") == true)
                return "#FFFFFF"; // White
            return "#CCCCCC"; // Light gray
        }
    }

    public string AgentStatusJson
    {
        get
        {
            if (SelectedAgent == null)
                return "No agent selected.";
            return JsonSerializer.Serialize(SelectedAgent, new JsonSerializerOptions { WriteIndented = true });
        }
    }

    #endregion

    #region Commands

    public AsyncRelayCommand ConnectAgentCommand { get; }
    public AsyncRelayCommand DisconnectCommand { get; }
    public AsyncRelayCommand ToggleVoiceChatCommand { get; }
    public RelayCommand<string> SelectVideoSourceCommand { get; }
    public AsyncRelayCommand<AgentControlAction> ExecuteControlActionCommand { get; }
    public AsyncRelayCommand RefreshAgentsCommand { get; }
    public RelayCommand ToggleSidebarCommand { get; }
    public RelayCommand ToggleRightPanelCommand { get; }

    #endregion

    #region Command Implementation

    private bool CanConnectAgent() => SelectedAgent != null && !IsConnecting && !IsConnected;
    private bool CanDisconnect() => IsConnected || IsConnecting;

    private async Task ConnectAgentAsync()
    {
        if (SelectedAgent == null) return;

        try
        {
            IsConnecting = true;
            ConnectionStatus = "Creating session...";
            
            AddLogEntry("INFO", $"Connecting to agent: {SelectedAgent.Name}");

            // Create session request
            var videoSource = SelectedVideoSource switch
            {
                "Twitch" => "twitch",
                "YouTube" => "youtube",
                _ => "direct_screen"
            };

            var sessionRequest = _configService.ToSessionCreationRequest(
                SelectedAgent, 
                "user123", // TODO: Get actual user ID
                videoSource,
                new SessionCustomizations
                {
                    IsAdmin = true,
                    Permissions = "admin"
                });

            // Create session via API
            ConnectionStatus = "Creating JIGA session...";
            var sessionResponse = await _apiService.CreateSessionAsync(sessionRequest);

            if (!sessionResponse.Success)
            {
                throw new InvalidOperationException($"Session creation failed: {sessionResponse.Error}");
            }

            // Update session info
            CurrentSession = new JigaSession
            {
                SessionId = sessionResponse.SessionId,
                AgentId = SelectedAgent.Id,
                Agent = SelectedAgent,
                Status = SessionStatus.Active,
                CreatedAt = DateTime.UtcNow,
                Configuration = new SessionConfiguration
                {
                    HighSpeedVision = HighSpeedVisionEnabled,
                    VoiceChatEnabled = VoiceChatEnabled,
                    VideoInputSource = SelectedVideoSource
                },
                WebSocketEndpoints = sessionResponse.WebSocketEndpoints ?? new List<string>()
            };

            // Connect WebSockets
            ConnectionStatus = "Connecting WebSocket streams...";
            await ConnectWebSocketsAsync(sessionResponse);

            // Update UI state
            IsConnected = true;
            ConnectionStatus = "Connected";
            
            AddLogEntry("SUCCESS", $"Successfully connected to {SelectedAgent.Name}");
            AddChatMessage($"Connected to {SelectedAgent.Name}", true);

            // Start metrics polling
            _ = Task.Run(PollMetricsAsync);
        }
        catch (Exception ex)
        {
            ConnectionStatus = "Connection failed";
            AddLogEntry("ERROR", $"Connection failed: {ex.Message}");
            AddChatMessage($"Connection failed: {ex.Message}", false, "System");
        }
        finally
        {
            IsConnecting = false;
        }
    }

    private async Task DisconnectAsync()
    {
        try
        {
            ConnectionStatus = "Disconnecting...";
            AddLogEntry("INFO", "Disconnecting from agent...");

            // Disconnect WebSockets
            await _webSocketService.DisconnectAllAsync();

            // Delete session if exists
            if (CurrentSession != null)
            {
                await _apiService.DeleteSessionAsync(CurrentSession.SessionId);
            }

            // Reset state
            CurrentSession = null;
            LiveMetrics = null;
            IsConnected = false;
            ConnectionStatus = "Disconnected";
            
            AddLogEntry("INFO", "Disconnected successfully");
            AddChatMessage("Disconnected from agent", false, "System");
        }
        catch (Exception ex)
        {
            AddLogEntry("ERROR", $"Disconnect error: {ex.Message}");
        }
    }

    private async Task ToggleVoiceChatAsync()
    {
        VoiceChatEnabled = !VoiceChatEnabled;
        
        if (CurrentSession != null)
        {
            // Update voice configuration on server
            var voiceConfig = _configService.ExtractVoiceConfig(SelectedAgent!, 
                new SessionCustomizations { VoiceId = SelectedAgent?.VoiceId });
                
            await _apiService.SetVoiceConfigurationAsync(CurrentSession.SessionId, voiceConfig);
            
            AddLogEntry("INFO", $"Voice chat {(VoiceChatEnabled ? "enabled" : "disabled")}");
        }
    }

    private void SelectVideoSource(string? source)
    {
        if (!string.IsNullOrEmpty(source))
        {
            SelectedVideoSource = source;
            AddLogEntry("INFO", $"Video source changed to: {source}");
        }
    }

    private void ToggleSidebar()
    {
        IsSidebarExpanded = !IsSidebarExpanded;
    }

    private void ToggleRightPanel()
    {
        IsRightPanelExpanded = !IsRightPanelExpanded;
    }

    private async Task ExecuteControlActionAsync(AgentControlAction? action)
    {
        if (action == null || CurrentSession == null) return;

        try
        {
            AddLogEntry("INFO", $"Executing action: {action.Text}");
            
            // Handle different action types
            switch (action.Id.ToLowerInvariant())
            {
                case "toggle_vision":
                    HighSpeedVisionEnabled = !HighSpeedVisionEnabled;
                    AddChatMessage($"Vision analysis {(HighSpeedVisionEnabled ? "enabled" : "disabled")}", false, "System");
                    break;
                    
                case "toggle_voice":
                    await ToggleVoiceChatAsync();
                    break;
                    
                case "get_capabilities":
                    var capabilities = await _apiService.GetAgentCapabilitiesAsync(CurrentSession.SessionId);
                    if (capabilities.Success && capabilities.Data != null)
                    {
                        AddChatMessage($"Agent capabilities: {string.Join(", ", capabilities.Data.EnabledSkills)}", false, SelectedAgent?.Name);
                    }
                    break;
                    
                default:
                    AddChatMessage($"Executed: {action.Text}", false, SelectedAgent?.Name);
                    break;
            }
        }
        catch (Exception ex)
        {
            AddLogEntry("ERROR", $"Action execution failed: {ex.Message}");
        }
    }

    private async Task RefreshAgentsAsync()
    {
        try
        {
            AddLogEntry("INFO", "Refreshing available agents...");
            
            var response = await _apiService.GetAvailableAgentsAsync();
            if (response.Success && response.Data != null)
            {
                AvailableAgents.Clear();
                foreach (var agent in response.Data)
                {
                    AvailableAgents.Add(agent);
                }
                
                AddLogEntry("INFO", $"Loaded {response.Data.Count} agents");
            }
            else
            {
                AddLogEntry("ERROR", "Failed to load agents");
            }
        }
        catch (Exception ex)
        {
            AddLogEntry("ERROR", $"Failed to refresh agents: {ex.Message}");
        }
    }

    #endregion

    #region Helper Methods

    private async Task InitializeAsync()
    {
        try
        {
            // Check API health
            var health = await _apiService.GetHealthAsync();
            if (health.Success)
            {
                ConnectionStatus = "API Ready";
                AddLogEntry("INFO", "JIGA API is healthy");
            }

            // Load available agents
            await RefreshAgentsAsync();
            
            // Select first agent by default
            if (AvailableAgents.Any())
            {
                SelectedAgent = AvailableAgents.First();
            }
        }
        catch (Exception ex)
        {
            AddLogEntry("ERROR", $"Initialization failed: {ex.Message}");
            ConnectionStatus = "API Unavailable";
        }
    }

    private async Task ConnectWebSocketsAsync(SessionResponse sessionResponse)
    {
        if (SelectedAgent == null || CurrentSession == null) return;

        var endpoints = _configService.GetWebSocketEndpoints(
            CurrentSession.SessionId, 
            SelectedAgent, 
            "ws://localhost:8000");

        var connectionResults = await _webSocketService.ConnectMultipleAsync(endpoints);
        
        foreach (var result in connectionResults)
        {
            var status = result.Value ? "connected" : "failed";
            AddLogEntry("INFO", $"WebSocket {result.Key}: {status}");
        }
    }

    private async Task PollMetricsAsync()
    {
        while (IsConnected && CurrentSession != null)
        {
            try
            {
                var metricsResponse = await _apiService.GetSessionMetricsAsync(CurrentSession.SessionId);
                if (metricsResponse.Success && metricsResponse.Data != null)
                {
                    if (LiveMetrics == null)
                    {
                        LiveMetrics = new SessionMetrics();
                    }
                    LiveMetrics.UpdateFromApiMetrics(metricsResponse.Data);
                }
                
                await Task.Delay(2000); // Poll every 2 seconds
            }
            catch (Exception ex)
            {
                AddLogEntry("ERROR", $"Metrics polling error: {ex.Message}");
                await Task.Delay(5000); // Wait longer on error
            }
        }
    }

    private void UpdateControlActions()
    {
        ControlActions.Clear();
        
        if (SelectedAgent != null)
        {
            var actions = _configService.GetControlActions(SelectedAgent);
            foreach (var action in actions)
            {
                ControlActions.Add(action);
            }
        }
    }

    private void SubscribeToServiceEvents()
    {
        // API service events
        _apiService.ApiError += OnApiError;
        _apiService.ConnectionStatusChanged += OnApiConnectionStatusChanged;

        // WebSocket service events
        _webSocketService.MessageReceived += OnWebSocketMessageReceived;
        _webSocketService.StatusChanged += OnWebSocketStatusChanged;
        _webSocketService.VisionAnalysisReceived += OnVisionAnalysisReceived;
        _webSocketService.AudioResponseReceived += OnAudioResponseReceived;
        _webSocketService.ErrorOccurred += OnWebSocketError;
    }

    private void AddLogEntry(string level, string message)
    {
        Application.Current?.Dispatcher.Dispatch(() =>
        {
            LogEntries.Insert(0, new LogEntry
            {
                Level = level,
                Message = message,
                Timestamp = DateTime.Now
            });

            // Keep only last 100 entries
            if (LogEntries.Count > 100)
            {
                LogEntries.RemoveAt(LogEntries.Count - 1);
            }
        });
    }

    private void AddChatMessage(string text, bool isFromUser, string? senderName = null)
    {
        Application.Current?.Dispatcher.Dispatch(() =>
        {
            ChatMessages.Add(new ChatMessage
            {
                Text = text,
                IsFromUser = isFromUser,
                IsFromAgent = !isFromUser && senderName != "System",
                SenderName = senderName ?? (isFromUser ? "You" : "Agent"),
                Timestamp = DateTime.Now
            });
        });
    }

    #endregion

    #region Event Handlers

    private void OnApiError(object? sender, ApiErrorEventArgs e)
    {
        AddLogEntry("ERROR", $"API Error: {e.ErrorMessage}");
    }

    private void OnApiConnectionStatusChanged(object? sender, ConnectionStatusEventArgs e)
    {
        ConnectionStatus = e.IsConnected ? "API Connected" : "API Disconnected";
    }

    private void OnWebSocketMessageReceived(object? sender, WebSocketMessageEventArgs e)
    {
        AddLogEntry("DEBUG", $"WebSocket {e.ConnectionType}: {e.MessageType}");
    }

    private void OnWebSocketStatusChanged(object? sender, WebSocketStatusEventArgs e)
    {
        AddLogEntry("INFO", $"WebSocket {e.ConnectionType}: {e.State}");
    }

    private void OnVisionAnalysisReceived(object? sender, VisionAnalysisEventArgs e)
    {
        if (e.Result != null)
        {
            AddChatMessage($"Vision: {e.Result.Type} (confidence: {e.Result.Confidence:P0})", false, "Vision AI");
        }
    }

    private void OnAudioResponseReceived(object? sender, AudioResponseEventArgs e)
    {
        if (e.Response != null && !string.IsNullOrEmpty(e.Response.Text))
        {
            AddChatMessage(e.Response.Text, false, SelectedAgent?.Name);
        }
    }

    private void OnWebSocketError(object? sender, WebSocketErrorEventArgs e)
    {
        AddLogEntry("ERROR", $"WebSocket {e.ConnectionType}: {e.ErrorMessage}");
    }

    #endregion
} 