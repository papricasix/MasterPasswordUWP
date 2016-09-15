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
using Template10.Validation;

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

    public class Site : /*BindableBase,*/ValidatableModelBase, ISite
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

        [JsonRequired]
        public KeyVersion AlgorithmVersion { get { return Read<KeyVersion>(); } set { Write(value); } }

        public string Category { get { return Read<string>(); } set { Write(value); } }

        public string GeneratedUserName { get { return Read<string>(); } set { Write(value); } }

        public long LastUsed { get { return Read<long>(); } set { Write(value); } }

        [JsonRequired]
        public SiteType PasswordType { get { return Read<SiteType>(); } set { Write(value); } }

        public SiteVariant PasswordVariant { get { return Read<SiteVariant>(); } set { Write(value); } }

        [JsonRequired]
        public int SiteCounter { get { return Read<int>(); } set { Write(value); } }

        [JsonRequired]
        public string SiteName { get { return Read<string>(); } set { Write(value); } }

        public string UserName { get { return Read<string>(); } set { Write(value); } }

        [JsonIgnore]
        public string GeneratedPassword { get; set; }

        public string Identifier { get; set; }

        public void MergeWith(ISite other)
        {
            foreach (var e in GetType().GetProperties())
            {
                if (e.CanWrite && e.CanRead)
                {
                    e.SetValue(this, e.GetValue(other));
                }
            }
        }
    }
}
