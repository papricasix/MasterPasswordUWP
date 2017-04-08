using Template10.Mvvm;
using System.Collections.Generic;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Template10.Services.NavigationService;
using Windows.UI.Xaml.Navigation;
using Autofac;
using MasterPassword.Common.Models;
using MasterPasswordUWP.Algorithm;
using MasterPasswordUWP.CollectionTools;
using MasterPasswordUWP.Models;
using MasterPasswordUWP.Services;
using MasterPasswordUWP.Services.DataSources;
using MasterPasswordUWP.Services.SettingsServices;
using Template10.Common;
using Template10.Controls;
using Template10.Utils;

namespace MasterPasswordUWP.ViewModels
{
    public interface ISitesPageViewModel
    {
        ObservableItemCollection<ISite> Sites { get; set; }
        ISite SelectedSite { get; set; }
    }

    public class ComboboxItemSitesOrderConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is SitesOrder)
            {
                return ((SitesOrder)value).ToHumanReadableString();
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is string)
            {
                return ((string)value).FromHumanReadableString(typeof(SitesOrder), SitesOrder.Name);
            }
            return value;
        }
    }

    public enum SitesPageViewModelParameterType
    {
        NewSite,

        MergeSite,

        RefreshView
    }

    [DataContract]
    public class SitesPageViewModelParameter
    {
        [DataMember]
        public SitesPageViewModelParameterType ParameterType { get; set; }

        [DataMember]
        public object Parameter { get; set; }
    }

    public class SitesPageViewModel : ViewModelBase, ISitesPageViewModel
    {
        private SettingsService Settings => App.Container.Resolve<SettingsService>();

        private ObservableItemCollection<ISite> _sites;
        public ObservableItemCollection<ISite> Sites
        {
            get { return _sites; }
            set { Set(ref _sites, value); base.RaisePropertyChanged(); }
        }

        public FilterableCollectionView<ISite> FilteredSites { get; set; }

        public IEnumerable<string> SiteOrders { get; }

        private SitesOrder _sitesOrder;
        public SitesOrder SitesOrder
        {
            get { return _sitesOrder; }
            set { Set(ref _sitesOrder, value); Settings.LastSortBy = value; }
        }

        public ISite SelectedSite { get; set; }

        public Visibility SitesScrollViewerVisibility => Sites?.Any() ?? false ? Visibility.Visible : Visibility.Collapsed;
        public Visibility AddNewSiteLinkVisibility => Sites?.Any() ?? false ? Visibility.Collapsed : Visibility.Visible;

        public SitesPageViewModel()
        {
            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
            }

            SiteOrders = Enum.GetValues(typeof(SitesOrder)).Cast<SitesOrder>().Select(order => order.ToHumanReadableString());
            FilteredSites = new FilterableCollectionView<ISite>();
        }

        private async Task ApplyModelToViewModel()
        {
            var provider = App.Container.Resolve<ISiteProvider>();
            var preparedSites = provider?.Sites.Select(site =>
            {
                (site as Site).SetGeneratedPassword( GeneratePassword(site), !Settings.PasswordsVisible );
                return site;
            });
            Sites = new ObservableItemCollection<ISite>(preparedSites);

            FilteredSites.Source = Sites;
            FilteredSites.OrderFunction = site => SitesOrderToPropertyName( SitesOrder, site );
            FilteredSites.IncludedFunction = site => site.SiteName?.ToLower().Contains( SearchString?.ToLower() ?? string.Empty ) ?? true;

            SelectedSite = Sites.FirstOrDefault();

            await Task.CompletedTask;
        }

        private void ApplySortOrderToFilteredSites()
        {
            FilteredSites.AscendingOrder = SitesOrder != SitesOrder.LastUse;
        }

        private async void OnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if ( args.PropertyName == nameof(SitesOrder) || args.PropertyName == nameof(SearchString) )
            {
                if (args.PropertyName == nameof(SitesOrder))
                {
                    ApplySortOrderToFilteredSites();
                }
                FilteredSites.RefreshView();
            }
        }

        private static object SitesOrderToPropertyName(SitesOrder sitesOrder, ISite site)
        {
            switch (sitesOrder)
            {
                case SitesOrder.Name:
                    return site.SiteName;
                case SitesOrder.LastUse:
                    return site.LastUsed;
                case SitesOrder.Login:
                    return site.UserName;
            }
            return null;
        }

        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> suspensionState)
        {
            if (suspensionState.Any())
            {
                //Value = suspensionState[nameof(Value)]?.ToString();
            }

            SitesOrder = Settings.LastSortBy;
            ApplySortOrderToFilteredSites();
            if (Sites == null || Settings.PopPasswordsVisibleChangedFlag())
            {
                await ApplyModelToViewModel();
            }

            if (parameter is SitesPageViewModelParameter)
            {
                var param = parameter as SitesPageViewModelParameter;
                if ((param.ParameterType == SitesPageViewModelParameterType.NewSite || param.ParameterType == SitesPageViewModelParameterType.MergeSite) && param.Parameter is ISite)
                {
                    // the identifier of an edited or new site
                    var detailSite = param.Parameter as ISite;

                    // regenerate password
                    (detailSite as Site).SetGeneratedPassword( GeneratePassword( detailSite ), !Settings.PasswordsVisible );

                    var replaceTarget = Sites.FirstOrDefault(site => site.Identifier == detailSite.Identifier);
                    if (replaceTarget != null)
                    {
                        // merge with existing (cause was edit mode)
                        replaceTarget.MergeWith(detailSite);
                    }
                    else
                    {
                        // add as new
                        Sites?.Add(detailSite);
                    }
                    App.Container.Resolve<ISitePersistor>().Persist(Sites);
                    FilteredSites.RefreshView();

                    SelectedSite = replaceTarget;
                }

                if (param.ParameterType == SitesPageViewModelParameterType.RefreshView)
                {
                    await ApplyModelToViewModel();
                }
            }

            // register some listeners
            PropertyChanged += OnPropertyChanged;
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

        /*public void GotoDetailsPage() => NavigationService.Navigate(typeof(Views.DetailPage), Value);

        public void GotoSettings() => NavigationService.Navigate(typeof(Views.SettingsPage), 0);

        public void GotoPrivacy() => NavigationService.Navigate(typeof(Views.SettingsPage), 1);

        public void GotoAbout() => NavigationService.Navigate(typeof(Views.SettingsPage), 2);*/

        public void EditItem() => NavigationService.Navigate(typeof(Views.DetailPage), SelectedSite);

        public async void RefreshSites()
        {
            await ApplyModelToViewModel();
            FilteredSites.RefreshView();
            NavigationService.GoBack();
            NavigationService.GoForward();
        }

        public void DeleteItem()
        {
            foreach (var e in Sites.Where(site => site.Identifier == SelectedSite?.Identifier).ToList())
            {
                Sites.Remove(e);
            }
            App.Container.Resolve<ISitePersistor>().Persist(Sites);
            FilteredSites.RefreshView();
        }

        private string _searchString;
        public string SearchString
        {
            get { return _searchString; }
            set
            {
                Set(ref _searchString, value);
            }
        }

        public Site CreateEmptySite()
        {
            var r = new Site { UserName = string.Empty, SiteName = string.Empty, SiteCounter = 1, PasswordType = SiteType.GeneratedMaximum, AlgorithmVersion = KeyVersion.Current, LastUsed = DateTime.Now.ToFileTimeUtc() };
            return r;
        }

        private string GeneratePassword(ISite site)
        {
            return App.Container.Resolve<SettingsService>()?.MasterKey?.Encode( site.SiteName, site.PasswordType, (uint) site.SiteCounter, SiteVariant.Password );
        }
    }
}

