using JigaMultiplatform.ViewModels;

namespace JigaMultiplatform.Views;

public partial class MainView : ContentPage
{
    public MainView()
    {
        InitializeComponent();
    }

    public MainView(MainViewModel viewModel) : this()
    {
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        
        // If no ViewModel was injected, try to get it from DI
        if (BindingContext == null)
        {
            var viewModel = Handler?.MauiContext?.Services.GetService<MainViewModel>();
            if (viewModel != null)
            {
                BindingContext = viewModel;
            }
        }
    }
} 