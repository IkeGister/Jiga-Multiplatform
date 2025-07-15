using JigaMultiplatform.Views;

namespace JigaMultiplatform;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		// Get MainView from DI container
		var mainView = Handler?.MauiContext?.Services.GetService<MainView>();
		
		if (mainView == null)
		{
			// Fallback if DI fails
			mainView = new MainView();
		}

		return new Window(mainView)
		{
			Title = "JIGA Multiplatform"
		};
	}
}