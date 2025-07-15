namespace JigaMultiplatform.ViewModels;

public class LogEntry : BaseViewModel
{
    private DateTime _timestamp = DateTime.Now;
    private string _level = "INFO";
    private string _message = string.Empty;
    private string _details = string.Empty;

    public DateTime Timestamp
    {
        get => _timestamp;
        set
        {
            if (SetProperty(ref _timestamp, value))
            {
                OnPropertyChanged(nameof(FormattedTime));
                OnPropertyChanged(nameof(FormattedTimestamp));
            }
        }
    }

    public string Level
    {
        get => _level;
        set => SetProperty(ref _level, value);
    }

    public string Message
    {
        get => _message;
        set => SetProperty(ref _message, value);
    }

    public string Details
    {
        get => _details;
        set
        {
            if (SetProperty(ref _details, value))
            {
                OnPropertyChanged(nameof(HasDetails));
            }
        }
    }

    public string FormattedTime => Timestamp.ToString("HH:mm:ss");
    public string FormattedTimestamp => Timestamp.ToString("HH:mm:ss.fff");
    public bool HasDetails => !string.IsNullOrEmpty(Details);

    public static LogEntry CreateInfo(string message, string? details = null)
    {
        return new LogEntry
        {
            Level = "INFO",
            Message = message,
            Details = details ?? string.Empty,
            Timestamp = DateTime.Now
        };
    }

    public static LogEntry CreateWarning(string message, string? details = null)
    {
        return new LogEntry
        {
            Level = "WARNING",
            Message = message,
            Details = details ?? string.Empty,
            Timestamp = DateTime.Now
        };
    }

    public static LogEntry CreateError(string message, string? details = null)
    {
        return new LogEntry
        {
            Level = "ERROR",
            Message = message,
            Details = details ?? string.Empty,
            Timestamp = DateTime.Now
        };
    }

    public static LogEntry CreateDebug(string message, string? details = null)
    {
        return new LogEntry
        {
            Level = "DEBUG",
            Message = message,
            Details = details ?? string.Empty,
            Timestamp = DateTime.Now
        };
    }

    public static LogEntry CreateSuccess(string message, string? details = null)
    {
        return new LogEntry
        {
            Level = "SUCCESS",
            Message = message,
            Details = details ?? string.Empty,
            Timestamp = DateTime.Now
        };
    }

    public string GetFullMessage()
    {
        if (HasDetails)
        {
            return $"{Message}\n{Details}";
        }
        return Message;
    }
} 