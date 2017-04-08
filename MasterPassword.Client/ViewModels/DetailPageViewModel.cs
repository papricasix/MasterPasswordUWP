using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Template10.Mvvm;
using Template10.Services.NavigationService;
using Windows.UI.Xaml.Navigation;
using Autofac;
using MasterPassword.Common.Models;
using MasterPasswordUWP.Algorithm;
using MasterPasswordUWP.Models;
using MasterPasswordUWP.Services;
using MasterPasswordUWP.Services.SettingsServices;

namespace MasterPasswordUWP.ViewModels
{
    public interface IDetailPageViewModel
    {
        Site Site { get; set; }
    }

    public class PasswordTypeMapping
    {
        public SiteType Type { get; }
        public string Name { get; }

        public PasswordTypeMapping(SiteType type)
        {
            Name = SiteTypeHelper.GetShortName(Type = type);
        }

        public PasswordTypeMapping(string name)
        {
            Type = SiteTypeHelper.ForName(Name = name);
        }
    }

    public class ComboboxItemPasswordTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is SiteType)
            {
                return ((SiteType)value).ToHumanReadableString();
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is string)
            {
                return ((string)value).FromHumanReadableString(typeof(SiteType), SiteType.GeneratedMaximum);
            }
            return value;
        }
    }

    public class SliderDoubleIntConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return System.Convert.ToDouble( value );
        }
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return System.Convert.ToInt32( value );
        }
    }

    public class VisibleWhenFalseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if ((bool)value)
            {
                return Visibility.Collapsed;
            }
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class BorderBrushBooleanConverter : DependencyObject, IValueConverter
    {
        public static readonly DependencyProperty TrueBrushProperty = DependencyProperty.Register(nameof(TrueBrush),typeof(Brush),typeof(BorderBrushBooleanConverter), PropertyMetadata.Create(new SolidColorBrush(Colors.White)));
        public static readonly DependencyProperty FalseBrushProperty = DependencyProperty.Register(nameof(FalseBrush),typeof(Brush),typeof(BorderBrushBooleanConverter), PropertyMetadata.Create(new SolidColorBrush(Colors.Red)));

        public Brush TrueBrush { get { return (Brush) GetValue(TrueBrushProperty); } set { SetValue(TrueBrushProperty, value); } }
        public Brush FalseBrush { get { return (Brush) GetValue(FalseBrushProperty); } set { SetValue(FalseBrushProperty, value); } }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if ((bool)value)
            {
                return TrueBrush;
            }
            return FalseBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class DetailPageViewModel : ViewModelBase, IDetailPageViewModel
    {
        private SettingsService SettingsService { get; } = App.Container.Resolve<SettingsService>();

        public Site Site { get; set; }

        public IEnumerable<string> PasswordTypes { get; }

        public DetailPageViewModel()
        {
            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
            }

            PasswordTypes = Enum.GetValues(typeof(SiteType)).Cast<SiteType>().Select(e => e.ToHumanReadableString());

            PropertyChanged += (sender, args) => UpdateGeneratedPassword(Site);
        }

        private void UpdateGeneratedPassword(Site site)
        {
            site.SetGeneratedPassword( SettingsService.MasterKey?.Encode( Site.SiteName, Site.PasswordType, (uint) Site.SiteCounter, SiteVariant.Password ), !SettingsService.PasswordsVisible );
        }

        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> suspensionState)
        {
            Site = parameter as Site;

            Site.Validator = model =>
            {
                var e = model as Site;
                if (string.IsNullOrWhiteSpace(e.SiteName))
                {
                    e.Properties[nameof(e.SiteName)].Errors.Add("Site name is required");
                }
            };

            RegisterChangeEventHandlerFor(Site);
            UpdateGeneratedPassword(Site);

            await Task.CompletedTask;
        }

        private void RegisterChangeEventHandlerFor(INotifyPropertyChanged e)
        {
            e.PropertyChanged += (sender, args) => RaisePropertyChanged( nameof(Site) );
        }

        public override async Task OnNavigatedFromAsync(IDictionary<string, object> suspensionState, bool suspending)
        {
            if (suspending)
            {
                //suspensionState[nameof(Value)] = Value;
            }
            await Task.CompletedTask;
        }

        public override async Task OnNavigatingFromAsync(NavigatingEventArgs args)
        {
            args.Cancel = false;
            await Task.CompletedTask;
        }

        public void ApplyViewToModel()
        {
            NavigationService.Navigate(typeof(Views.SitesPage), new SitesPageViewModelParameter { ParameterType = SitesPageViewModelParameterType.MergeSite, Parameter = Site} );
        }
    }
}

