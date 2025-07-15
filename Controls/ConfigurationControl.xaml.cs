using System.Collections.ObjectModel;
using System.Windows.Input;
using JigaMultiplatform.Models;

namespace JigaMultiplatform.Controls;

public partial class ConfigurationControl : ContentView
{
    public static readonly BindableProperty SaveConfigCommandProperty = 
        BindableProperty.Create(nameof(SaveConfigCommand), typeof(ICommand), typeof(ConfigurationControl));

    public static readonly BindableProperty ResetConfigCommandProperty = 
        BindableProperty.Create(nameof(ResetConfigCommand), typeof(ICommand), typeof(ConfigurationControl));

    public ConfigurationControl()
    {
        InitializeComponent();
        BindingContext = this;
    }

    public ICommand SaveConfigCommand
    {
        get => (ICommand)GetValue(SaveConfigCommandProperty);
        set => SetValue(SaveConfigCommandProperty, value);
    }

    public ICommand ResetConfigCommand
    {
        get => (ICommand)GetValue(ResetConfigCommandProperty);
        set => SetValue(ResetConfigCommandProperty, value);
    }

    // Helper collections for dropdowns
    public ObservableCollection<string> VideoSources { get; } = new()
    {
        "Direct Screen",
        "Twitch",
        "YouTube"
    };

    public ObservableCollection<string> PerformanceModes { get; } = new()
    {
        "Balanced",
        "Ultra Fast",
        "Quality Focused",
        "Power Saving"
    };

    // Helper properties for dynamic labels
    public string VisionQualityLabel
    {
        get
        {
            if (BindingContext is AgentConfiguration config)
            {
                return config.VisionQuality switch
                {
                    <= 30 => "Fast",
                    <= 70 => "Balanced",
                    _ => "Quality"
                };
            }
            return "Balanced";
        }
    }

    public string VoiceSensitivityLabel
    {
        get
        {
            if (BindingContext is AgentConfiguration config)
            {
                return config.VoiceSensitivity switch
                {
                    <= 30 => "Low",
                    <= 70 => "Medium",
                    _ => "High"
                };
            }
            return "Medium";
        }
    }

    public string FrameRateText
    {
        get
        {
            if (BindingContext is AgentConfiguration config)
            {
                return $"{config.FrameRateLimit:F0} fps";
            }
            return "2 fps";
        }
    }

    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();
        
        // Update computed properties when binding context changes
        OnPropertyChanged(nameof(VisionQualityLabel));
        OnPropertyChanged(nameof(VoiceSensitivityLabel));
        OnPropertyChanged(nameof(FrameRateText));
    }
} 