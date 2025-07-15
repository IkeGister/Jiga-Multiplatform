using System.Windows.Input;

namespace JigaMultiplatform.Controls;

public partial class VideoSourceButton : ContentView
{
    public static readonly BindableProperty SourceTypeProperty = 
        BindableProperty.Create(nameof(SourceType), typeof(string), typeof(VideoSourceButton), "Direct");

    public static readonly BindableProperty IsSelectedProperty = 
        BindableProperty.Create(nameof(IsSelected), typeof(bool), typeof(VideoSourceButton), false, 
            propertyChanged: OnSelectionChanged);

    public static readonly BindableProperty CommandProperty = 
        BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(VideoSourceButton));

    public static readonly BindableProperty CommandParameterProperty = 
        BindableProperty.Create(nameof(CommandParameter), typeof(object), typeof(VideoSourceButton));

    public VideoSourceButton()
    {
        InitializeComponent();
        BindingContext = this;
    }

    public string SourceType
    {
        get => (string)GetValue(SourceTypeProperty);
        set => SetValue(SourceTypeProperty, value);
    }

    public bool IsSelected
    {
        get => (bool)GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    public ICommand Command
    {
        get => (ICommand)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public object CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    // Helper properties for icon visibility
    public bool IsTwitch => SourceType?.Equals("Twitch", StringComparison.OrdinalIgnoreCase) == true;
    public bool IsYouTube => SourceType?.Equals("YouTube", StringComparison.OrdinalIgnoreCase) == true;
    public bool IsDirect => SourceType?.Equals("Direct", StringComparison.OrdinalIgnoreCase) == true;

    // Source color based on type
    public Color SourceColor
    {
        get
        {
            return SourceType?.ToLowerInvariant() switch
            {
                "twitch" => Color.FromArgb("#9147FF"),
                "youtube" => Color.FromArgb("#FF0000"),
                "direct" => Color.FromArgb("#00aaff"),
                _ => Color.FromArgb("#333333")
            };
        }
    }

    private static void OnSelectionChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is VideoSourceButton button)
        {
            // Update visual state based on selection
            string stateName = button.IsSelected ? "Selected" : "Unselected";
            VisualStateManager.GoToState(button, stateName);
        }
    }

    protected override void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
    {
        base.OnPropertyChanged(propertyName);

        if (propertyName == nameof(SourceType))
        {
            OnPropertyChanged(nameof(IsTwitch));
            OnPropertyChanged(nameof(IsYouTube));
            OnPropertyChanged(nameof(IsDirect));
            OnPropertyChanged(nameof(SourceColor));
        }
    }
} 