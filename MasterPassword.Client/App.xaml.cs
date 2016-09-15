using Windows.UI.Xaml;
using System.Threading.Tasks;
using MasterPasswordUWP.Services.SettingsServices;
using Windows.ApplicationModel.Activation;
using Template10.Controls;
using Template10.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Background;
using Windows.Foundation.Collections;
using Windows.Foundation.Metadata;
using Windows.Storage.Pickers;
using Windows.System;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Autofac;
using MasterPasswordUWP.Services;
using MasterPasswordUWP.Services.DataSources;
using MasterPasswordUWP.ViewModels;
using Template10.Services.LoggingService;

namespace MasterPasswordUWP
{
    /// Documentation on APIs used in this page:
    /// https://github.com/Windows-XAML/Template10/wiki

    [Bindable]
    sealed partial class App : Template10.Common.BootStrapper
    {
        private static Lazy<IContainer> LazyContainer { get; } = new Lazy<IContainer>( BuildContainer );

        public static IContainer Container => LazyContainer.Value;

        private static IContainer BuildContainer()
        {
            var bldr = new ContainerBuilder();
            // register all models
            bldr.RegisterInstance(new SettingsService()).ExternallyOwned().SingleInstance();
            bldr.RegisterType<SiteProvider>().As<ISiteProvider>().SingleInstance();
            bldr.RegisterType<SitePersistor>().As<ISitePersistor>().As<ISiteImporterExporter>();
            bldr.RegisterType<SiteDataSourceJson>().As<ISiteDataSource>();
            bldr.RegisterType<PasswordClipboardService>().As<IPasswordClipboardService>();
            //bldr.RegisterType<SettingsService>().SingleInstance();
            // register all ViewModels
            //bldr.RegisterType<SitesPageViewModel>().As<ISitesPageViewModel>().PropertiesAutowired();
            return bldr.Build();
        }

        public App()
        {
            InitializeComponent();
            SplashFactory = e => new Views.Splash(e);

            if (ApiInformation.IsApiContractPresent("Windows.Phone.PhoneContract", 1, 0))
            {
                //var statusBar = StatusBar
                throw new NullReferenceException("statusbar found!");
            }

            // Xbox one stuff
            //ApplicationView.GetForCurrentView().SetDesiredBoundsMode(ApplicationViewBoundsMode.UseCoreWindow);
            //Windows.UI.Xaml.Application.Current.re
            //this.RequiresPointerMode = Windows.UI.Xaml.ApplicationRequiresPointerMode.WhenRequested;

            LoggingService.Enabled = true;
            LoggingService.WriteLine = (text, severity, target, caller) => Debug.WriteLine( $"{caller}: {target}, {text}" );

            #region App settings

            var _settings = App.Container.Resolve<SettingsService>();
            RequestedTheme = _settings.AppTheme;
            CacheMaxDuration = _settings.CacheMaxDuration;

            #endregion
        }

        public override async Task OnInitializeAsync(IActivatedEventArgs args)
        {
            if (args.Kind == ActivationKind.ShareTarget)
            {
                //Windows.System.Launcher.LaunchUriAsync( new Uri( $"md-masterpassword://test" ) );
                return;
            }
            if (!(Window.Current.Content is ModalDialog))
            {
                // create a new frame 
                var nav = NavigationServiceFactory(BackButton.Attach, ExistingContent.Include);

                // create modal root
                Window.Current.Content = new ModalDialog
                {
                    DisableBackButtonWhenModal = true,
                    Content = new Views.Shell(nav),
                    ModalContent = new Views.Busy(),
                };
                // Xbox one stuff
                //TODO: check if we are on xbox
                //ApplicationView.GetForCurrentView().SetDesiredBoundsMode(ApplicationViewBoundsMode.UseCoreWindow);
            }

            await Task.CompletedTask;
        }

        static IDictionary<string, string> GetParams(string uri)
        {
            var matches = Regex.Matches( uri, @"[\?&](([^&=]+)=([^&=#]*))", RegexOptions.Compiled );
            return matches.Cast<Match>().ToDictionary(
                m => Uri.UnescapeDataString( m.Groups[2].Value ),
                m => Uri.UnescapeDataString( m.Groups[3].Value )
            );
        }

        public override async Task OnStartAsync(StartKind startKind, IActivatedEventArgs args)
        {
            // long-running startup tasks go here
            object loginParams = null;
            if (args.Kind == ActivationKind.Protocol && args is ProtocolActivatedEventArgs)
            {
                var protoArgs = (ProtocolActivatedEventArgs) args;
                var parameters = GetParams( protoArgs.Uri.ToString() );

                string userName, password;
                parameters.TryGetValue( "user", out userName );
                parameters.TryGetValue( "pass", out password );
                loginParams = new LoginPageParameters {UserName = userName, MasterPassword = password};
            }

            NavigationService.Navigate(typeof(Views.LoginPage), loginParams);
            await Task.CompletedTask;
        }
    }
}

