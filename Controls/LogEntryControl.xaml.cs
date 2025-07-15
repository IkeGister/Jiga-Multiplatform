using System.Windows.Input;
using JigaMultiplatform.ViewModels;

namespace JigaMultiplatform.Controls;

public partial class LogEntryControl : ContentView
{
    public static readonly BindableProperty CopyLogCommandProperty = 
        BindableProperty.Create(nameof(CopyLogCommand), typeof(ICommand), typeof(LogEntryControl));

    public LogEntryControl()
    {
        InitializeComponent();
    }

    public ICommand CopyLogCommand
    {
        get => (ICommand)GetValue(CopyLogCommandProperty);
        set => SetValue(CopyLogCommandProperty, value);
    }

    // Computed property for log level color
    public Color LevelColor
    {
        get
        {
            if (BindingContext is LogEntry entry)
            {
                return entry.Level?.ToUpperInvariant() switch
                {
                    "DEBUG" => (Color)Resources["DebugColor"],
                    "INFO" => (Color)Resources["InfoColor"],
                    "WARNING" or "WARN" => (Color)Resources["WarningColor"],
                    "ERROR" => (Color)Resources["ErrorColor"],
                    _ => (Color)Resources["InfoColor"]
                };
            }
            return (Color)Resources["InfoColor"];
        }
    }

    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();
        
        // Update computed property when binding context changes
        OnPropertyChanged(nameof(LevelColor));
    }
} 