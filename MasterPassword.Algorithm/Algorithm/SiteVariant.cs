using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MasterPasswordUWP.Algorithm
{
    [AttributeUsage(AttributeTargets.Field)]
    public class SiteVariantAttribute : Attribute
    {
        public string Description { get; set; }
        public string ContextDescription { get; set; }
        public string[] Options { get; set; }
        public string Scope { get; set; }
    }

    public enum SiteVariant
    {
        [SiteVariant(Description = "The password to log in with.", ContextDescription = "Doesn't currently use a context.", Options = new [] {"p", "password"}, Scope = "com.lyndir.masterpassword")]
        Password,

        [SiteVariant(Description = "The username to log in as.", ContextDescription = "Doesn't currently use a context.", Options = new[] { "l", "login" }, Scope = "com.lyndir.masterpassword.login")]
        Login,

        [SiteVariant(Description = "The answer to a security question.", ContextDescription = "Empty for a universal site answer or\nthe most significant word(s) of the question.", Options = new[] { "a", "answer" }, Scope = "com.lyndir.masterpassword.answer")]
        Answer
    }

    public static class SiteVariantHelper
    {
        public static string GetScope(SiteVariant e)
        {
            return e.GetType().GetField(e.ToString()).GetCustomAttribute<SiteVariantAttribute>().Scope;
        }
    }
}
