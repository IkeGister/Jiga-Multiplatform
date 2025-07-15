using Microsoft.Extensions.Logging;
using JigaMultiplatform.Services;
using JigaMultiplatform.ViewModels;
using JigaMultiplatform.Views;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;

namespace JigaMultiplatform;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		// Register HTTP Client
		builder.Services.AddHttpClient();

		// Register Core Services as Singletons (shared across the app)
		builder.Services.AddSingleton<IJigaApiService>(provider =>
		{
			var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
			var httpClient = httpClientFactory.CreateClient();
			return new JigaApiService(httpClient);
		});
		
		builder.Services.AddSingleton<IWebSocketService, WebSocketService>();
		builder.Services.AddSingleton<IAgentConfigurationService, AgentConfigurationService>();

		// Register ViewModels as Transient (new instance each time)
		builder.Services.AddTransient<MainViewModel>();

		// Register Views as Transient
		builder.Services.AddTransient<MainView>();

		// Add MAUI debugging services in debug builds
#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
