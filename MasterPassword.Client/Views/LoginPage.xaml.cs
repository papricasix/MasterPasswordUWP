using System;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Autofac;
using MasterPasswordUWP.Algorithm;
using MasterPasswordUWP.Models;
using MasterPasswordUWP.Services.SettingsServices;
using Microsoft.Xaml.Interactivity;
using Template10.Behaviors;
using Template10.Common;
using Template10.Controls;
using Template10.Services.NavigationService;

namespace MasterPasswordUWP.Views
{
    public sealed partial class LoginPage : Page
    {
        Template10.Services.SerializationService.ISerializationService _SerializationService;

        public LoginPage()
        {
            InitializeComponent();
            _SerializationService = Template10.Services.SerializationService.SerializationService.Json;

            Loaded += (sender, args) =>
            {
                if (string.IsNullOrEmpty(UserNameBox.Text))
                {
                    UserNameBox.Focus(FocusState.Keyboard);
                }
                else
                {
                    PasswordBox.Focus(FocusState.Keyboard);
                }
            };

            KeyUp += (sender, args) =>
            {
                if (args.Key == VirtualKey.Enter)
                {
                    LoginButton_OnClick(sender, args);
                }
            };
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            //var index = int.Parse(_SerializationService.Deserialize(e.Parameter?.ToString()).ToString());
            //MyPivot.SelectedIndex = index;
        }

        private async void LoginButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(PasswordBox.Password))
            {
                Views.Busy.SetBusy(true, "Generating master key...");
                // generate master password stuff
                var userName = UserNameBox.Text;
                var password = PasswordBox.Password.ToCharArray();
                await Task.Run(() => App.Container.Resolve<SettingsService>().MasterKey = MasterKey.Create(userName, password));
                Views.Busy.SetBusy(false);

                Shell.Instance.SetMenuState(HamburgerMenuState.LoggedIn);
                Shell.Instance.NavigationService.Navigate(typeof(Views.SitesPage));
                return;
            }
            Shell.Instance.SetMenuState(HamburgerMenuState.Default);
            PasswordBox.Focus(FocusState.Keyboard);
        }

        private void PasswordBox_OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            UpdateIdentIcon();
        }

        private void UserNameBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateIdentIcon();
        }

        private void UpdateIdentIcon()
        {
            var ii = new IdentIcon(UserNameBox.Text, PasswordBox.Password.ToCharArray());
            IdentIconText.Text = ii.Text;
            IdentIconText.Foreground = new SolidColorBrush(ii.Color);
        }
    }
}
