using Windows.UI.Xaml;
using MasterPasswordUWP.ViewModels;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Autofac;
using MasterPasswordUWP.Services;
using Microsoft.Services.Store.Engagement;

namespace MasterPasswordUWP.Views
{
    public sealed partial class DetailPage : Page
    {
        public DetailPage()
        {
            InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Disabled;

            Loaded += (sender, args) => SiteNameTextBox.Focus(FocusState.Keyboard);
            //InputScopeNameValue.EmailNameOrAddress
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            App.Container.Resolve<ITelemetryService>().LogMessage(this);
        }

        private void SaveButton_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            if (ViewModel.Site.IsValid)
            {
                ViewModel.ApplyViewToModel();
            }
        }

        private void GeneratedPasswordHyperLink_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            App.Container.Resolve<IPasswordClipboardService>().CopyPasswordToClipboard(ViewModel.Site);
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            ViewModel.Site.SiteCounter++;
        }
    }
}

