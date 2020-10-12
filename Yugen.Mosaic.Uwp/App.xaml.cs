using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Yugen.Mosaic.Uwp.Interfaces;
using Yugen.Mosaic.Uwp.Services;
using Yugen.Mosaic.Uwp.ViewModels;
using Yugen.Mosaic.Uwp.Views;
using Yugen.Toolkit.Standard.Extensions;
using Yugen.Toolkit.Standard.Services;
using Yugen.Toolkit.Uwp.Helpers;
using Yugen.Toolkit.Uwp.Services;

namespace Yugen.Mosaic.Uwp
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public sealed partial class App : Application
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            Services = ConfigureServices();

            InitializeComponent();
            Suspending += OnSuspending;

            AppCenter.Start("7df4b441-69ae-49c5-b27d-5a532f33b554",
                   typeof(Analytics), typeof(Crashes));
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override async void OnLaunched(LaunchActivatedEventArgs e)
        {
            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (!(Window.Current.Content is Frame rootFrame))
            {
                await InitializeServices();

                TitleBarHelper.ExtendToTitleBar();

                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    // When the navigation stack isn't restored navigate to the first page,
                    // configuring the new page by passing required information as a navigation
                    // parameter
                    rootFrame.Navigate(typeof(MainPage), e.Arguments);
                }
                // Ensure the current window is active
                Window.Current.Activate();

                await Services.GetService<IWhatsNewDisplayService>().ShowIfAppropriateAsync();
            }
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        private void OnNavigationFailed(object sender, NavigationFailedEventArgs e) => 
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            SuspendingDeferral deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }

        public new static App Current => (App)Application.Current;

        public IServiceProvider Services { get; }

        private IServiceProvider ConfigureServices()
        {
            string logFilePath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "Logs\\Yugen.Mosaic.Log.");

            Log.Logger = new LoggerConfiguration()
                   .MinimumLevel.Debug()
                   .WriteTo.Debug()
                   .WriteTo.File(logFilePath, restrictedToMinimumLevel: LogEventLevel.Information)
                   .CreateLogger();

            return new ServiceCollection()
                .AddSingleton<IMosaicService, MosaicService>()
                .AddSingleton<IProgressService, ProgressService>()
                .AddSingleton<ISearchAndReplaceAsciiArtService, SearchAndReplaceAsciiArtService>()
                .AddSingleton<IThemeSelectorService, ThemeSelectorService>()
                .AddSingleton<IWhatsNewDisplayService, WhatsNewDisplayService>()
                .AddTransient<SearchAndReplaceAdjustHueService>()
                .AddTransient<SearchAndReplaceClassicService>()
                .AddTransient<SearchAndReplacePlainColorService>()
                .AddTransient<SearchAndReplaceRandomService>()
                .AddTransient<SearchAndReplaceAdjustHueService>()
                .AddTransient<MainViewModel>()
                .AddTransient<SettingsViewModel>()
                .AddTransient<WhatsNewViewModel>()
                .AddLogging(loggingBuilder =>
                {
                    loggingBuilder.AddSerilog(dispose: true);
                })
                .BuildServiceProvider();
        }

        private async Task InitializeServices()
        {
            await Services.GetService<IThemeSelectorService>().InitializeAsync();
        }
    }
}