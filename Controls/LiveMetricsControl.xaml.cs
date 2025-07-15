using JigaMultiplatform.ViewModels;

namespace JigaMultiplatform.Controls;

public partial class LiveMetricsControl : ContentView
{
    public LiveMetricsControl()
    {
        InitializeComponent();
    }

    // Session status color helper
    public Color SessionStatusColor
    {
        get
        {
            if (BindingContext is SessionMetrics metrics)
            {
                return metrics.IsOnline ? (Color)Resources["StatusOnline"] :
                       metrics.HasWarnings ? (Color)Resources["StatusWarning"] :
                       (Color)Resources["StatusOffline"];
            }
            return (Color)Resources["StatusOffline"];
        }
    }

    // Connection status color helpers
    public Color VisionConnectionColor
    {
        get
        {
            if (BindingContext is SessionMetrics metrics)
            {
                return GetConnectionColor(metrics.VisionConnectionStatus);
            }
            return (Color)Resources["StatusOffline"];
        }
    }

    public Color AudioConnectionColor
    {
        get
        {
            if (BindingContext is SessionMetrics metrics)
            {
                return GetConnectionColor(metrics.AudioConnectionStatus);
            }
            return (Color)Resources["StatusOffline"];
        }
    }

    public Color ApiConnectionColor
    {
        get
        {
            if (BindingContext is SessionMetrics metrics)
            {
                return GetConnectionColor(metrics.ApiConnectionStatus);
            }
            return (Color)Resources["StatusOffline"];
        }
    }

    private Color GetConnectionColor(string connectionStatus)
    {
        return connectionStatus?.ToLowerInvariant() switch
        {
            "connected" or "online" => (Color)Resources["StatusOnline"],
            "connecting" or "warning" => (Color)Resources["StatusWarning"],
            _ => (Color)Resources["StatusOffline"]
        };
    }

    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();
        
        // Update computed properties when binding context changes
        OnPropertyChanged(nameof(SessionStatusColor));
        OnPropertyChanged(nameof(VisionConnectionColor));
        OnPropertyChanged(nameof(AudioConnectionColor));
        OnPropertyChanged(nameof(ApiConnectionColor));
    }
} 