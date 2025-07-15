using JigaMultiplatform.Models;

namespace JigaMultiplatform.Controls;

public partial class AgentBioControl : ContentView
{
    public AgentBioControl()
    {
        InitializeComponent();
    }

    // Helper properties for avatar display
    public bool HasAvatar
    {
        get
        {
            if (BindingContext is Agent agent)
            {
                return !string.IsNullOrEmpty(agent.AvatarUrl);
            }
            return false;
        }
    }

    public bool HasNoAvatar => !HasAvatar;

    public string AvatarInitials
    {
        get
        {
            if (BindingContext is Agent agent && !string.IsNullOrEmpty(agent.Name))
            {
                var words = agent.Name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (words.Length >= 2)
                {
                    return $"{words[0][0]}{words[1][0]}".ToUpper();
                }
                else if (words.Length == 1)
                {
                    return words[0].Length >= 2 ? 
                           $"{words[0][0]}{words[0][1]}".ToUpper() : 
                           words[0][0].ToString().ToUpper();
                }
            }
            return "AG";
        }
    }

    // Helper properties for capabilities display
    public bool HasCapabilities
    {
        get
        {
            if (BindingContext is Agent agent)
            {
                return agent.Toolkit?.Any() == true;
            }
            return false;
        }
    }

    public bool HasVoiceId
    {
        get
        {
            if (BindingContext is Agent agent)
            {
                return !string.IsNullOrEmpty(agent.VoiceId);
            }
            return false;
        }
    }

    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();
        
        // Update computed properties when binding context changes
        OnPropertyChanged(nameof(HasAvatar));
        OnPropertyChanged(nameof(HasNoAvatar));
        OnPropertyChanged(nameof(AvatarInitials));
        OnPropertyChanged(nameof(HasCapabilities));
        OnPropertyChanged(nameof(HasVoiceId));
    }
} 