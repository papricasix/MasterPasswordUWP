using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Storage.Pickers;
using Template10.Common;
using Template10.Controls;
using Template10.Services.NavigationService;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Autofac;
using MasterPassword.Client.Services.ImportExport;
using MasterPasswordUWP.Services;
using MasterPasswordUWP.Services.SettingsServices;
using MasterPasswordUWP.ViewModels;

namespace MasterPasswordUWP.Views
{
    [AttributeUsage(AttributeTargets.Field)]
    internal class HamburgerButtonsAttribute : Attribute
    {
        public string[] VisibleButtons { get; set; }

        public HamburgerButtonsAttribute(params string[] visibleButtons)
        {
            VisibleButtons = visibleButtons;
        }
    }

    public enum HamburgerMenuState
    {
        [HamburgerButtons("LoginButton")]
        Default,
        [HamburgerButtons("SitesButton", "LogoutButton", "ImportButton", "ExportButton")]
        LoggedIn,
    }

    public sealed partial class Shell : Page
    {
        public static Shell Instance { get; set; }
        public static HamburgerMenu HamburgerMenu => Instance.MyHamburgerMenu;

        public INavigationService NavigationService => MyHamburgerMenu.NavigationService;

        public Visibility FeedbackAvailability => Microsoft.Services.Store.Engagement.StoreServicesFeedbackLauncher.IsSupported() ? Visibility.Visible : Visibility.Collapsed;

        public Shell()
        {
            Instance = this;
            InitializeComponent();
        }

        public Shell(INavigationService navigationService) : this()
        {
            SetNavigationService(navigationService);
        }

        public void SetNavigationService(INavigationService navigationService)
        {
            MyHamburgerMenu.NavigationService = navigationService;
        }

        public void SetMenuState(HamburgerMenuState state)
        {
            var visibleButtons = state.GetType().GetField(Enum.GetName(typeof(HamburgerMenuState), state)).GetCustomAttribute<HamburgerButtonsAttribute>()?.VisibleButtons;
            foreach (var e in MyHamburgerMenu.PrimaryButtons)
            {
                e.Visibility = Visibility.Collapsed;
            }
            foreach (var e in visibleButtons)
            {
                var o = FindName(e) as HamburgerButtonInfo;
                if (o != null)
                {
                    o.Visibility = Visibility.Visible;
                }
            }
        }

        private async void ImportButton_OnTapped(object sender, RoutedEventArgs e)
        {
            ImportButton.IsChecked = false;

            if (await App.Container.Resolve<ISiteImportExportService>().ImportSitesFromUserInputSource(true, App.Container.Resolve<SettingsService>().PreferredImportExportFileType))
            {
                NavigationService.Navigate(typeof(Views.SitesPage), new SitesPageViewModelParameter {ParameterType = SitesPageViewModelParameterType.RefreshView});
            }
        }

        private async void ExportButton_OnTappedButton_OnTapped(object sender, RoutedEventArgs e)
        {
            ExportButton.IsChecked = false;

            if (await App.Container.Resolve<ISiteImportExportService>().ExportSitesToUserInputTarget(App.Container.Resolve<SettingsService>().PreferredImportExportFileType))
            {
                
            }
        }

        private async void FeedbackButton_OnTapped(object sender, RoutedEventArgs e)
        {
            var launcher = Microsoft.Services.Store.Engagement.StoreServicesFeedbackLauncher.GetDefault();
            await launcher.LaunchAsync();
        }

        private void LogoutButton_OnTapped(object sender, RoutedEventArgs e)
        {
            SetMenuState(HamburgerMenuState.Default);
            LoginButton.IsChecked = false;

            NavigationService.Navigate(typeof(Views.LoginPage));
            NavigationService.ClearCache(true);
            NavigationService.ClearHistory();
            App.Container.Resolve<SettingsService>().MasterKey = null;
        }
    }
}

