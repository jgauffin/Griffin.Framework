using System;

namespace Griffin.Framework.Text
{
    /// <summary>
    /// Defines which localizable strings a type has.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class|AttributeTargets.Method, AllowMultiple = true)]
    public class LocalizeAttribute : Attribute
    {
        public string LanguageCode { get; set; }
        public string MetadataName { get; set; }
        public string Text { get; set; }

        public LocalizeAttribute(string languageCode, string metadataName, string text)
        {
            if (languageCode == null) throw new ArgumentNullException("languageCode");
            if (text == null) throw new ArgumentNullException("text");
            if (metadataName == "")
                metadataName = null;
            LanguageCode = languageCode;
            MetadataName = metadataName;
            Text = text;
        }

        public LocalizeAttribute(string languageCode, string text)
        {
            if (text == null) throw new ArgumentNullException("text");
            LanguageCode = languageCode;
            Text = text;
        }
    }
}
