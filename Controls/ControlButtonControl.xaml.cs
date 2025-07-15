using System.Windows.Input;
using JigaMultiplatform.Services;

namespace JigaMultiplatform.Controls;

public partial class ControlButtonControl : ContentView
{
    public static readonly BindableProperty ExecuteCommandProperty = 
        BindableProperty.Create(nameof(ExecuteCommand), typeof(ICommand), typeof(ControlButtonControl));

    public ControlButtonControl()
    {
        InitializeComponent();
    }

    public ICommand ExecuteCommand
    {
        get => (ICommand)GetValue(ExecuteCommandProperty);
        set => SetValue(ExecuteCommandProperty, value);
    }

    // Computed properties based on the bound AgentControlAction
    public bool HasIcon
    {
        get
        {
            if (BindingContext is AgentControlAction action)
            {
                return !string.IsNullOrEmpty(action.Icon);
            }
            return false;
        }
    }

    public Style ButtonStyle
    {
        get
        {
            if (BindingContext is AgentControlAction action)
            {
                return action.Type?.ToLowerInvariant() switch
                {
                    "primary" => (Style)Resources["PrimaryButtonStyle"],
                    "danger" => (Style)Resources["DangerButtonStyle"],
                    _ => (Style)Resources["SecondaryButtonStyle"]
                };
            }
            return (Style)Resources["SecondaryButtonStyle"];
        }
    }

    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();
        
        // Update computed properties when binding context changes
        OnPropertyChanged(nameof(HasIcon));
        OnPropertyChanged(nameof(ButtonStyle));
    }
} 