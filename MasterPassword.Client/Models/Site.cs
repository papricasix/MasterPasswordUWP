using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using MasterPasswordUWP.Algorithm;
using Newtonsoft.Json;
using Template10.Mvvm;

namespace MasterPasswordUWP.Models
{
    public interface ISite : INotifyPropertyChanged
    {
        string Identifier { get; set; }

        KeyVersion AlgorithmVersion { get; set; }

        string SiteName { get; set; }

        string UserName { get; set; }

        int SiteCounter { get; set; }

        SiteType PasswordType { get; set; }

        long LastUsed { get; set; }

        void MergeWith(ISite other);
    }

    public class Site : BindableBase, ISite
    {
        public Site()
        {
            Identifier = Guid.NewGuid().ToString("D");
        }

        public override int GetHashCode()
        {
            return Identifier?.GetHashCode() ?? base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return (obj as Site)?.Identifier == Identifier;
        }

        private KeyVersion _algorithmVersion;
        [JsonRequired]
        public KeyVersion AlgorithmVersion { get { return _algorithmVersion; } set { Set(ref _algorithmVersion, value); } }

        private string _category;
        public string Category { get { return _category; } set { Set(ref _category, value); } }

        private string _generatedUserName;
        public string GeneratedUserName { get { return _generatedUserName; } set { Set(ref _generatedUserName, value); } }

        private long _lastUsed;
        public long LastUsed { get { return _lastUsed; } set { Set(ref _lastUsed, value); } }

        private SiteType _passwordType;
        [JsonRequired]
        public SiteType PasswordType { get { return _passwordType; } set { Set(ref _passwordType, value); } }

        private SiteVariant _passwordVariant;
        public SiteVariant PasswordVariant { get { return _passwordVariant; } set { Set(ref _passwordVariant, value); } }

        private int _siteCounter;
        [JsonRequired]
        public int SiteCounter { get { return _siteCounter; } set { Set(ref _siteCounter, value); } }

        private string _siteName;
        [JsonRequired]
        public string SiteName { get { return _siteName; } set { Set(ref _siteName, value); } }

        private string _userName;
        public string UserName { get { return _userName; } set { Set(ref _userName, value); } }

        [JsonIgnore]
        public string GeneratedPassword { get; set; }

        public string Identifier { get; set; }

        public void MergeWith(ISite other)
        {
            foreach (var e in GetType().GetProperties())
            {
                e.SetValue(this, e.GetValue(other));
                //RaisePropertyChanged(e.Name);
            }
        }
    }
}
