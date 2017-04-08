using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Windows.Globalization;
using Windows.System;
using Windows.System.UserProfile;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

namespace MasterPasswordUWP.Views
{
    public sealed partial class SettingsPage : Page
    {
        Template10.Services.SerializationService.ISerializationService _SerializationService;

        public SettingsPage()
        {
            InitializeComponent();
            _SerializationService = Template10.Services.SerializationService.SerializationService.Json;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var index = int.Parse(_SerializationService.Deserialize(e.Parameter?.ToString()).ToString());
            MyPivot.SelectedIndex = index;
        }

        private async void VisitHomePage_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri(@"https://mdudek.com"));
        }

        private void ComboboxOverrideLanguage_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var lang = ViewModel.SettingsPartViewModel.ApplicationLanguage;
            if (!string.IsNullOrWhiteSpace(lang))
            {
                ApplicationLanguages.PrimaryLanguageOverride = lang;
            }
        }
    }
}
