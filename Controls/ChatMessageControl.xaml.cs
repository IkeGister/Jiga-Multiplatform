using System.Windows.Input;
using JigaMultiplatform.ViewModels;

namespace JigaMultiplatform.Controls;

public partial class ChatMessageControl : ContentView
{
    public static readonly BindableProperty PlayAudioCommandProperty = 
        BindableProperty.Create(nameof(PlayAudioCommand), typeof(ICommand), typeof(ChatMessageControl));

    public ChatMessageControl()
    {
        InitializeComponent();
    }

    public ICommand PlayAudioCommand
    {
        get => (ICommand)GetValue(PlayAudioCommandProperty);
        set => SetValue(PlayAudioCommandProperty, value);
    }

    // Dynamic styling properties based on message type
    public Brush MessageBackground
    {
        get
        {
            if (BindingContext is ChatMessage message)
            {
                return message.IsFromAgent ? (Brush)Resources["AgentGradient"] :
                       message.IsFromUser ? (Brush)Resources["UserGradient"] :
                       new SolidColorBrush((Color)Resources["SystemMessageBackground"]);
            }
            return new SolidColorBrush((Color)Resources["AgentMessageBackground"]);
        }
    }

    public Color MessageBorder
    {
        get
        {
            if (BindingContext is ChatMessage message)
            {
                return message.IsFromAgent ? (Color)Resources["AgentMessageBorder"] :
                       message.IsFromUser ? (Color)Resources["UserMessageBorder"] :
                       (Color)Resources["AgentMessageBorder"];
            }
            return (Color)Resources["AgentMessageBorder"];
        }
    }

    public CornerRadius MessageCornerRadius
    {
        get
        {
            if (BindingContext is ChatMessage message)
            {
                // Agent messages have rounded corners on right side
                // User messages have rounded corners on left side
                return message.IsFromAgent ? new CornerRadius(12, 12, 12, 4) :
                       message.IsFromUser ? new CornerRadius(12, 12, 4, 12) :
                       new CornerRadius(8); // System messages are symmetric
            }
            return new CornerRadius(12, 12, 12, 4);
        }
    }

    public LayoutOptions MessageAlignment
    {
        get
        {
            if (BindingContext is ChatMessage message)
            {
                return message.IsFromUser ? LayoutOptions.End : LayoutOptions.Start;
            }
            return LayoutOptions.Start;
        }
    }

    public Thickness MessageMargin
    {
        get
        {
            if (BindingContext is ChatMessage message)
            {
                return message.IsFromUser ? new Thickness(40, 0, 0, 0) :
                       message.IsFromAgent ? new Thickness(0, 0, 40, 0) :
                       new Thickness(20, 0, 20, 0); // System messages centered
            }
            return new Thickness(0, 0, 40, 0);
        }
    }

    public LayoutOptions TimestampAlignment
    {
        get
        {
            if (BindingContext is ChatMessage message)
            {
                return message.IsFromUser ? LayoutOptions.End : LayoutOptions.Start;
            }
            return LayoutOptions.Start;
        }
    }

    public Thickness TimestampMargin
    {
        get
        {
            if (BindingContext is ChatMessage message)
            {
                return message.IsFromUser ? new Thickness(0, 2, 6, 0) :
                       new Thickness(6, 2, 0, 0);
            }
            return new Thickness(6, 2, 0, 0);
        }
    }

    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();
        
        // Update all computed properties when binding context changes
        OnPropertyChanged(nameof(MessageBackground));
        OnPropertyChanged(nameof(MessageBorder));
        OnPropertyChanged(nameof(MessageCornerRadius));
        OnPropertyChanged(nameof(MessageAlignment));
        OnPropertyChanged(nameof(MessageMargin));
        OnPropertyChanged(nameof(TimestampAlignment));
        OnPropertyChanged(nameof(TimestampMargin));
    }
} 