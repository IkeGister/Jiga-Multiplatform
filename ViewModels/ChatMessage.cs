namespace JigaMultiplatform.ViewModels;

public class ChatMessage : BaseViewModel
{
    private string _text = string.Empty;
    private DateTime _timestamp = DateTime.Now;
    private bool _isFromUser;
    private bool _isFromAgent;
    private string _senderName = string.Empty;
    private bool _isAudioMessage;
    private string _audioData = string.Empty;
    private string _audioDuration = string.Empty;
    private bool _isTyping;

    public string Text
    {
        get => _text;
        set => SetProperty(ref _text, value);
    }

    public DateTime Timestamp
    {
        get => _timestamp;
        set
        {
            if (SetProperty(ref _timestamp, value))
            {
                OnPropertyChanged(nameof(FormattedTimestamp));
            }
        }
    }

    public bool IsFromUser
    {
        get => _isFromUser;
        set => SetProperty(ref _isFromUser, value);
    }

    public bool IsFromAgent
    {
        get => _isFromAgent;
        set => SetProperty(ref _isFromAgent, value);
    }

    public string SenderName
    {
        get => _senderName;
        set => SetProperty(ref _senderName, value);
    }

    public bool IsAudioMessage
    {
        get => _isAudioMessage;
        set => SetProperty(ref _isAudioMessage, value);
    }

    public string AudioData
    {
        get => _audioData;
        set => SetProperty(ref _audioData, value);
    }

    public string AudioDuration
    {
        get => _audioDuration;
        set => SetProperty(ref _audioDuration, value);
    }

    public bool IsTyping
    {
        get => _isTyping;
        set => SetProperty(ref _isTyping, value);
    }

    public string FormattedTimestamp => Timestamp.ToString("HH:mm:ss");

    public bool IsSystemMessage => !IsFromUser && !IsFromAgent;

    public static ChatMessage CreateUserMessage(string text)
    {
        return new ChatMessage
        {
            Text = text,
            IsFromUser = true,
            IsFromAgent = false,
            SenderName = "You",
            Timestamp = DateTime.Now
        };
    }

    public static ChatMessage CreateAgentMessage(string text, string agentName)
    {
        return new ChatMessage
        {
            Text = text,
            IsFromUser = false,
            IsFromAgent = true,
            SenderName = agentName,
            Timestamp = DateTime.Now
        };
    }

    public static ChatMessage CreateSystemMessage(string text)
    {
        return new ChatMessage
        {
            Text = text,
            IsFromUser = false,
            IsFromAgent = false,
            SenderName = "System",
            Timestamp = DateTime.Now
        };
    }

    public static ChatMessage CreateTypingIndicator(string agentName)
    {
        return new ChatMessage
        {
            Text = "",
            IsFromUser = false,
            IsFromAgent = true,
            SenderName = agentName,
            IsTyping = true,
            Timestamp = DateTime.Now
        };
    }

    public static ChatMessage CreateAudioMessage(string text, string agentName, string audioData, string duration)
    {
        return new ChatMessage
        {
            Text = text,
            IsFromUser = false,
            IsFromAgent = true,
            SenderName = agentName,
            IsAudioMessage = true,
            AudioData = audioData,
            AudioDuration = duration,
            Timestamp = DateTime.Now
        };
    }
} 