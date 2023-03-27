using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using RoadsApp2.ViewModels;

namespace RoadsApp2;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		builder.Services.AddSingleton<MainPage>();
        builder.Services.AddSingleton<HomePage>();

        builder.Services.AddSingleton<AccidentRegistrationPage>();
        builder.Services.AddSingleton<AccidentRegistrationViewModel>();

		builder.Services.AddTransient<NewParticipantPage>();
        builder.Services.AddTransient<NewParticipantViewModel>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
