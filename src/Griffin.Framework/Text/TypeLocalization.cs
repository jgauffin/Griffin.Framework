using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;

namespace Griffin.Framework.Text
{
    public class TypeLocalization
    {
        private readonly ITextDataSource _dataSource;
        private readonly bool _trainingMode;
        private readonly ConcurrentDictionary<string, string> _typeCache = new ConcurrentDictionary<string, string>();

        public TypeLocalization(ITextDataSource dataSource, bool trainingMode)
        {
            _dataSource = dataSource;
            _trainingMode = trainingMode;
        }

        public static string CreateKey(Type type, string identifier)
        {
            if (type == null) throw new ArgumentNullException("type");
            if (identifier == null) throw new ArgumentNullException("identifier");
            return string.Format("{0}/{1}", type.FullName, identifier);
        }

        public static string CreateKey(Type type, string identifier, string metadataName)
        {
            if (type == null) throw new ArgumentNullException("type");
            if (identifier == null) throw new ArgumentNullException("identifier");
            if (metadataName == null) throw new ArgumentNullException("metadataName");
            return string.Format("{0}/{1}_{2}", type.FullName, identifier, metadataName);
        }

        /// <summary>
        /// Get text from metadata (typically from the <c>[Localize]</c> attribute)
        /// </summary>
        /// <param name="type">Type to scan</param>
        /// <param name="identifier">Property/Method name (or <c>"Class"</c> for the type itself)</param>
        /// <param name="metadataName">Additional meta data</param>
        /// <returns>Text if found; otherwise <c>null</c>.</returns>
        public virtual string GetMetadataText(Type type, string identifier, string metadataName)
        {
            if (identifier != "Class")
            {
                var members = type.GetMember(identifier);
                foreach (var member in members)
                {
                    var attributes = member.GetCustomAttributes<LocalizeAttribute>();
                    var attribute = attributes.FirstOrDefault(x => x.MetadataName == metadataName);
                    if (attribute != null)
                    {
                        return attribute.Text;
                    }
                }
            }
            else
            {
                var attributes = type.GetCustomAttributes<LocalizeAttribute>();
                var attribute = attributes.FirstOrDefault(x => x.MetadataName == metadataName);
                if (attribute != null)
                {
                    return attribute.Text;
                }
            }

            return null;
        }


        public string Translate(Type type, string name)
        {
            string str;
            var key = CreateKey(type, name);
            if (_typeCache.TryGetValue(key, out str))
                return str;

            var text = _dataSource.Get(key);

            if (text == null)
            {
                text = GetMetadataText(type, name, null);
                var repos = _dataSource as ITextRepository;
                if (_trainingMode && repos != null)
                {
                    repos.Create(key, text);
                }
            }

            if (text != null)
                _typeCache.AddOrUpdate(key, text, (k, v) => text);

            return text;
        }

        public string Translate(Type type, string name, string metadataName)
        {
            string str;
            var key = CreateKey(type, name, metadataName);
            var shortKey = CreateKey(type, name);
            if (_typeCache.TryGetValue(key, out str))
                return str;

            var text = _dataSource.Get(shortKey, metadataName);
            if (text == null)
            {
                text = GetMetadataText(type, name, metadataName);
                var repos = _dataSource as ITextRepository;
                if (_trainingMode && repos != null)
                {
                    repos.Create(shortKey, metadataName, text);
                }
            }

            if (text != null)
                _typeCache.AddOrUpdate(key, text, (k, v) => text);

            return text;
        }
    }
}