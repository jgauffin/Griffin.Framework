using System;

namespace Griffin.Framework.Text
{
    /// <summary>
    /// Defines which localizable strings a type has.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class LocalizeAttribute : Attribute
    {
        public string LanguageCode { get; set; }
        public string MetadataName { get; set; }
        public string Text { get; set; }

        public LocalizeAttribute(string languageCode, string metadataName, string text)
        {
            LanguageCode = languageCode;
            MetadataName = metadataName;
            Text = text;
        }

        public LocalizeAttribute(string languageCode, string text)
        {
            LanguageCode = languageCode;
            Text = text;
        }
    }
}
