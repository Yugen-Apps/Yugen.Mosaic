using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using System;
using System.IO;
using Windows.Storage;
using Yugen.Mosaic.Uwp.Interfaces;
using Yugen.Mosaic.Uwp.Services;
using Yugen.Mosaic.Uwp.ViewModels;
using Yugen.Toolkit.Standard.Services;

namespace Yugen.Mosaic.Uwp
{
    public class AppContainer
    {
        public static IServiceProvider Services { get; set; }

        public static void ConfigureServices()
        {
            string logFilePath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "Logs\\Yugen.Mosaic.Log.");

            Log.Logger = new LoggerConfiguration()
                   .MinimumLevel.Debug()
                   .WriteTo.Debug()
                   .WriteTo.File(logFilePath, restrictedToMinimumLevel: LogEventLevel.Information)
                   .CreateLogger();

            Log.Debug("Serilog started Debug!");
            Log.Information("Serilog started Information!");
            Log.Warning("Serilog started Warning!");

            Services = new ServiceCollection()
                .AddSingleton<IProgressService, ProgressService>()
                .AddSingleton<IMosaicService, MosaicService>()
                .AddSingleton<ISearchAndReplaceAsciiArtService, SearchAndReplaceAsciiArtService>()
                .AddTransient<SearchAndReplaceAdjustHueService>()
                .AddTransient<SearchAndReplaceClassicService>()
                .AddTransient<SearchAndReplacePlainColorService>()
                .AddTransient<SearchAndReplaceRandomService>()
                .AddTransient<SearchAndReplaceAdjustHueService>()
                .AddTransient<MainViewModel>()
                .AddTransient<SettingsViewModel>()
                .AddLogging(loggingBuilder =>
                {
                    loggingBuilder.AddSerilog(dispose: true);
                })
                .BuildServiceProvider();
        }
    }
}