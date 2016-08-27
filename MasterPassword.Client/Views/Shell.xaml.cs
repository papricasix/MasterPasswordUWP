using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Windows.Storage.Pickers;
using Template10.Common;
using Template10.Controls;
using Template10.Services.NavigationService;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Autofac;
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

        private static async Task<ContentDialogResult> DisplayImportWarnDialog()
        {
            var dialog = new ContentDialog()
            {
                Title = "Importing Sites",
                Content = "Importing another file will overwrite your current sites. Do you want to continue?",
                PrimaryButtonText = "No",
                PrimaryButtonCommandParameter = true,
                SecondaryButtonText = "Yes",
                SecondaryButtonCommandParameter = false,
            };

            return await dialog.ShowAsync();
        }

        private async void ImportButton_OnTapped(object sender, RoutedEventArgs e)
        {
            ImportButton.IsChecked = false;

            if ((App.Container.Resolve<ISiteProvider>()?.Sites.Any() ?? false) && await DisplayImportWarnDialog() == ContentDialogResult.Primary)
            {
                return;
            }

            var picker = new FileOpenPicker { FileTypeFilter = { ".json" }, ViewMode = PickerViewMode.List, SuggestedStartLocation = PickerLocationId.DocumentsLibrary };
            var file = await picker.PickSingleFileAsync();
            //await Task.Run(() => App.Container.Resolve<ISiteImporterExporter>().Import(file));
            await App.Container.Resolve<ISiteImporterExporter>().Import(file);

            NavigationService.Navigate(typeof(Views.SitesPage), new SitesPageViewModelParameter { ParameterType = SitesPageViewModelParameterType.RefreshView });
        }

        private async void ExportButton_OnTappedButton_OnTapped(object sender, RoutedEventArgs e)
        {
            ExportButton.IsChecked = false;

            var picker = new FileSavePicker { SuggestedFileName = "mpw-export.json", SuggestedStartLocation = PickerLocationId.DocumentsLibrary };
            picker.FileTypeChoices.Add("JSON", new List<string> { ".json" });
            var file = await picker.PickSaveFileAsync();
            await Task.Run(() => App.Container.Resolve<ISiteImporterExporter>().Export(file));
        }
    }
}

