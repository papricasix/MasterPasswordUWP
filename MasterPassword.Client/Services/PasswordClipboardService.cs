using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.Resources;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;
using Windows.UI.Xaml;
using MasterPasswordUWP.Models;

namespace MasterPasswordUWP.Services
{
    internal interface IPasswordClipboardService
    {
        void CopyPasswordToClipboard(ISite site);
    }

    internal class PasswordClipboardService : IPasswordClipboardService
    {
        public void ShowToast(string line1, string line2)
        {
            var template = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText03);
            var textTags = template.GetElementsByTagName("text");
            textTags.First().AppendChild(template.CreateTextNode(/*Package.Current.DisplayName*/line1));
            textTags.Last().AppendChild(template.CreateTextNode(line2));
            var toast = new ToastNotification(template);
            ToastNotificationManager.CreateToastNotifier().Show(toast);
        }

        public void CopyPasswordToClipboard(ISite site)
        {
            var package = new DataPackage { RequestedOperation = DataPackageOperation.Copy };
            package.SetText((site as Site)?.GeneratedPassword ?? string.Empty);
            Clipboard.SetContent(package);

            ShowToast($"{site.SiteName}", ResourceLoader.GetForCurrentView().GetString("Toast_Message_PasswordHasBeenCopied"));
        }
    }
}
