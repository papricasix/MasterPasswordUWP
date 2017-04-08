using System;
using System.Linq;
using System.Runtime.InteropServices;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Autofac;
using MasterPassword.Client.Services.ImportExport;
using MasterPasswordUWP.Models;
using MasterPasswordUWP.Services;

namespace MasterPasswordUWP.Views
{
    public sealed partial class SitesPage : Page
    {
        Template10.Services.SerializationService.ISerializationService _SerializationService;

        private DateTime _lastSearch;
        private readonly DispatcherTimer _searchTimer;

        public SitesPage()
        {
            InitializeComponent();
            _SerializationService = Template10.Services.SerializationService.SerializationService.Json;

            _searchTimer = new DispatcherTimer {Interval = TimeSpan.FromMilliseconds(100)};
            _searchTimer.Tick += SearchTimerOnTick;

            NavigationCacheMode = NavigationCacheMode.Enabled;

            Loaded += (sender, args) => SearchBox.Focus(FocusState.Keyboard);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            //var index = int.Parse(_SerializationService.Deserialize(e.Parameter?.ToString()).ToString());
            //MyPivot.SelectedIndex = index;
            try
            {
                // Disable screen capture when the user navigates to this page.
                ApplicationView.GetForCurrentView().IsScreenCaptureEnabled = false;
            }
            catch (COMException x )
            {
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            try
            {
                // Re-enable screen capture when the user navigates away from this page.
                ApplicationView.GetForCurrentView().IsScreenCaptureEnabled = true;
            }
            catch ( COMException x )
            {
            }
        }

        public static Rect GetElementRect(FrameworkElement element)
        {
            var buttonTransform = element.TransformToVisual(null);
            var point = buttonTransform.TransformPoint(new Point());
            return new Rect(point, new Size(element.ActualWidth, element.ActualHeight));
        }

        private async void SiteElementEdit_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            /*ViewModel.SelectedSite = (sender as Control)?.DataContext as ISite;
            ViewModel.EditItem();*/
            var menu = new PopupMenu();
            menu.Commands.Add(new UICommand(ResourceLoader.GetForCurrentView().GetString("ContextMenu_SitesPage_EditSite"), cmd =>
            {
                ViewModel.SelectedSite = (sender as Control)?.DataContext as ISite;
                ViewModel.EditItem();
            }));
            menu.Commands.Add(new UICommandSeparator());
            menu.Commands.Add(new UICommand(ResourceLoader.GetForCurrentView().GetString("ContextMenu_SitesPage_DeleteSite"), cmd =>
            {
                ViewModel.SelectedSite = (sender as Control)?.DataContext as ISite;
                ViewModel.DeleteItem();
            }));
            await menu.ShowForSelectionAsync(GetElementRect(sender as FrameworkElement));
        }

        private void SiteElementEdit_OnRightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            //SiteElementEdit_OnTapped(sender, new TappedRoutedEventArgs());
        }

        private void SitesItem_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            // do not copy password if only the edit button was clicked
            var editButtonHit = VisualTreeHelper.FindElementsInHostCoordinates( e.GetPosition(Window.Current.Content), null )?.Any( elem => (elem as Button)?.Name == "EditButton");
            if ( editButtonHit ?? false )
            {
                return;
            }

            ViewModel.SelectedSite = (sender as FrameworkElement)?.DataContext as ISite;
            App.Container.Resolve<IPasswordClipboardService>().CopyPasswordToClipboard(ViewModel.SelectedSite);

            // update lastUsed time
            ViewModel.SelectedSite.LastUsed = DateTime.Now.ToFileTimeUtc();
            App.Container.Resolve<ISitePersistor>().Persist(ViewModel.Sites);
        }

        private void SearchBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            _lastSearch = DateTime.Now;
            _searchTimer.Start();
        }

        private void SearchTimerOnTick(object sender, object o)
        {
            if (DateTime.Now - _lastSearch > TimeSpan.FromMilliseconds(300))
            {
                ViewModel.SearchString = SearchBox.Text;
                _searchTimer.Stop();
            }
        }

        private void AddNewSite_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            ViewModel.SelectedSite = ViewModel.CreateEmptySite();
            ViewModel.EditItem();
        }

        private async void ImportSites_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            if (await App.Container.Resolve<ISiteImportExportService>().ImportSitesFromUserInputSource(true))
            {
                ViewModel.RefreshSites();
            }
        }
    }
}
