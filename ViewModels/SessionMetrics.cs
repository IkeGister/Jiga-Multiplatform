namespace JigaMultiplatform.ViewModels;

public class SessionMetrics : BaseViewModel
{
    private bool _isOnline;
    private bool _hasWarnings;
    private string _sessionStatus = "Initializing";
    private string _sessionDuration = "00:00:00";
    private string _lastUpdateTime = "--:--:--";
    private int _framesProcessed;
    private string _frameRate = "0 fps";
    private int _messagesSent;
    private string _messageRate = "0/min";
    private int _voiceInteractions;
    private string _voiceStatus = "Disabled";
    private string _processingSpeed = "0";
    private string _speedUnit = "fps";
    private string _visionConnectionStatus = "Disconnected";
    private string _audioConnectionStatus = "Disconnected";
    private string _apiConnectionStatus = "Disconnected";
    private string _visionLatency = "-- ms";
    private string _audioLatency = "-- ms";
    private string _apiLatency = "-- ms";
    private string _status = "Initializing";
    private DateTime? _sessionStartTime;

    public bool IsOnline
    {
        get => _isOnline;
        set => SetProperty(ref _isOnline, value);
    }

    public bool HasWarnings
    {
        get => _hasWarnings;
        set => SetProperty(ref _hasWarnings, value);
    }

    public string SessionStatus
    {
        get => _sessionStatus;
        set => SetProperty(ref _sessionStatus, value);
    }

    public string SessionDuration
    {
        get => _sessionDuration;
        set => SetProperty(ref _sessionDuration, value);
    }

    public string LastUpdateTime
    {
        get => _lastUpdateTime;
        set => SetProperty(ref _lastUpdateTime, value);
    }

    public int FramesProcessed
    {
        get => _framesProcessed;
        set => SetProperty(ref _framesProcessed, value);
    }

    public string FrameRate
    {
        get => _frameRate;
        set => SetProperty(ref _frameRate, value);
    }

    public int MessagesSent
    {
        get => _messagesSent;
        set => SetProperty(ref _messagesSent, value);
    }

    public string MessageRate
    {
        get => _messageRate;
        set => SetProperty(ref _messageRate, value);
    }

    public int VoiceInteractions
    {
        get => _voiceInteractions;
        set => SetProperty(ref _voiceInteractions, value);
    }

    public string VoiceStatus
    {
        get => _voiceStatus;
        set => SetProperty(ref _voiceStatus, value);
    }

    public string ProcessingSpeed
    {
        get => _processingSpeed;
        set => SetProperty(ref _processingSpeed, value);
    }

    public string SpeedUnit
    {
        get => _speedUnit;
        set => SetProperty(ref _speedUnit, value);
    }

    public string VisionConnectionStatus
    {
        get => _visionConnectionStatus;
        set => SetProperty(ref _visionConnectionStatus, value);
    }

    public string AudioConnectionStatus
    {
        get => _audioConnectionStatus;
        set => SetProperty(ref _audioConnectionStatus, value);
    }

    public string ApiConnectionStatus
    {
        get => _apiConnectionStatus;
        set => SetProperty(ref _apiConnectionStatus, value);
    }

    public string VisionLatency
    {
        get => _visionLatency;
        set => SetProperty(ref _visionLatency, value);
    }

    public string AudioLatency
    {
        get => _audioLatency;
        set => SetProperty(ref _audioLatency, value);
    }

    public string ApiLatency
    {
        get => _apiLatency;
        set => SetProperty(ref _apiLatency, value);
    }

    public string Status
    {
        get => _status;
        set => SetProperty(ref _status, value);
    }
    public DateTime? SessionStartTime
    {
        get => _sessionStartTime;
        set => SetProperty(ref _sessionStartTime, value);
    }

    public void UpdateFromApiMetrics(Models.SessionMetrics apiMetrics)
    {
        if (apiMetrics == null) return;

        IsOnline = apiMetrics.IsConnected;
        SessionStatus = apiMetrics.Status ?? "Unknown";
        FramesProcessed = apiMetrics.FramesProcessed;
        MessagesSent = apiMetrics.MessagesSent;
        VoiceInteractions = apiMetrics.VoiceInteractions;
        
        // Calculate derived values
        var fps = apiMetrics.FrameRate ?? 0;
        FrameRate = $"{fps:F1} fps";
        
        var msgRate = apiMetrics.MessageRate ?? 0;
        MessageRate = $"{msgRate:F0}/min";
        
        ProcessingSpeed = apiMetrics.ProcessingSpeed?.ToString("F1") ?? "0";
        
        // Update timestamps
        LastUpdateTime = DateTime.Now.ToString("HH:mm:ss");
        
        if (apiMetrics.SessionStartTime.HasValue)
        {
            var duration = DateTime.UtcNow - apiMetrics.SessionStartTime.Value;
            SessionDuration = $"{(int)duration.TotalHours:D2}:{duration.Minutes:D2}:{duration.Seconds:D2}";
        }
    }
} 