using Microsoft.Extensions.Logging;
using StudentAID.Services;
using StudentAID.ViewModel;

namespace StudentAID
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("Roboto-Medium.ttf", "RobotoMedium");
                })
                .UseMauiMaps();

            // Register OpenAIService for dependency injection
            builder.Services.AddSingleton<OpenAIService>();
            builder.Services.AddTransient<ChatBotViewModel>();

#if DEBUG
            // This configures logging to include debug level logs in DEBUG mode.
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
