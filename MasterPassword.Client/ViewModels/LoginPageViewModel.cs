using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Template10.Mvvm;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;
using Autofac;
using MasterPasswordUWP.Services.SettingsServices;

namespace MasterPasswordUWP.ViewModels
{
    public class LoginPageParameters
    {
        public string UserName { set; get; }

        public string MasterPassword { set; get; }
    }

    public class LoginPageViewModel : ViewModelBase
    {
        Services.SettingsServices.SettingsService _settings;

        public LoginPageViewModel()
        {
            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
                // designtime
            }
            else
            {
                _settings = App.Container.Resolve<SettingsService>();
            }
        }

        public string UserName
        {
            get { return _settings.PersistedUserName; }
            set { _settings.PersistedUserName = value; base.RaisePropertyChanged(); }
        }

        private string _masterPassword;
        public string MasterPassword
        {
            get { return _masterPassword; }
            set { _masterPassword = value; base.RaisePropertyChanged(); }
        }




        public override Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            var param = parameter as LoginPageParameters;
            if (param != null)
            {
                UserName = param.UserName;
                MasterPassword = param.MasterPassword;
            }

            return Task.CompletedTask;
            //return base.OnNavigatedToAsync(parameter, mode, state);
        }
    }
}

