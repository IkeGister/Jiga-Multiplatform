namespace JigaMultiplatform.Controls;

public partial class CollapsibleSection : ContentView
{
    public static readonly BindableProperty TitleProperty = 
        BindableProperty.Create(nameof(Title), typeof(string), typeof(CollapsibleSection), "Section");

    public static readonly BindableProperty ContentProperty = 
        BindableProperty.Create(nameof(Content), typeof(View), typeof(CollapsibleSection));

    public static readonly BindableProperty IsExpandedProperty = 
        BindableProperty.Create(nameof(IsExpanded), typeof(bool), typeof(CollapsibleSection), true,
            propertyChanged: OnExpandedChanged);

    public static readonly BindableProperty IconProperty = 
        BindableProperty.Create(nameof(Icon), typeof(string), typeof(CollapsibleSection), "");

    public CollapsibleSection()
    {
        InitializeComponent();
        BindingContext = this;
    }

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public View Content
    {
        get => (View)GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    public bool IsExpanded
    {
        get => (bool)GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
    }

    public string Icon
    {
        get => (string)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    private async void OnHeaderTapped(object sender, EventArgs e)
    {
        // Toggle expanded state
        IsExpanded = !IsExpanded;
    }

    private static async void OnExpandedChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is CollapsibleSection section)
        {
            await section.AnimateExpandCollapse((bool)newValue);
        }
    }

    private async Task AnimateExpandCollapse(bool isExpanding)
    {
        // Animate chevron rotation using the label directly
        await ChevronIcon.RotateTo(isExpanding ? 0 : -90, 200, Easing.CubicInOut);

        if (isExpanding)
        {
            // Show content first, then animate
            ContentContainer.IsVisible = true;
            ContentContainer.Opacity = 0;
            ContentContainer.Scale = 0.95;

            var fadeInAnimation = ContentContainer.FadeTo(1, 200, Easing.CubicOut);
            var scaleAnimation = ContentContainer.ScaleTo(1, 200, Easing.CubicOut);

            await Task.WhenAll(fadeInAnimation, scaleAnimation);
        }
        else
        {
            // Animate out, then hide
            var fadeOutAnimation = ContentContainer.FadeTo(0, 200, Easing.CubicIn);
            var scaleAnimation = ContentContainer.ScaleTo(0.95, 200, Easing.CubicIn);

            await Task.WhenAll(fadeOutAnimation, scaleAnimation);
            ContentContainer.IsVisible = false;
        }
    }
} 