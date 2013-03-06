using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Griffin.Framework.Text
{

    public class StringLocalizer
    {
        private readonly ITextDataSource _dataSource;

        public StringLocalizer(ITextDataSource dataSource, bool autoInsert)
        {
            _dataSource = dataSource;
        }

/// <summary>
        ///     Translate a text
        /// </summary>
        /// <param name="identifier">The identifier.</param>
        /// <param name="text">
        ///     Text to translate. Should contain <c>{0}</c> etc.
        /// </param>
        /// <returns>Depends on the training mode</returns>
        /// <remarks>
        ///     Will use <c>"/"</c> as the path.
        /// </remarks>
        public string String(string identifier, string text)
        {
            return String("/", identifier, text);
        }

        /// <summary>
        ///     Translate a text
        /// </summary>
        /// <param name="path">Path defines where the string being localized is located. It's used to make the administration easier, see remarks.</param>
        /// <param name="identifier">The identifier.</param>
        /// <param name="text">
        ///     Text to translate. Should contain <c>{0}</c> etc.
        /// </param>
        /// <returns>Depends on the training mode</returns>
        /// <remarks>
        ///     Path can vary. For types it might be the Namespace + TypeName (<c>Some.NameSpace.TypeName</c>) while
        ///     for HTML views it might be the view uri (<c>/Views/ModelName/SomeViewName</c>).
        /// </remarks>
        public string String(string path, string identifier, string text)
        {
            if (path == null) throw new ArgumentNullException("path");
            if (identifier == null) throw new ArgumentNullException("identifier");

            var value = _dataSource.Get(identifier);
            if (!string.IsNullOrEmpty(value))
                return value;

            if (!TrainingMode)
                return text;

            var repository = DataSource as ITextRepository;
            if (repository != null)
            {
                repository.Create(identifier, "");
            }

            return "-(" + identifier + ")-";
        }

        /// <summary>
        ///     Transate a text
        /// </summary>
        /// <param name="path">Path defines where the string being localized is located. It's used to make the administration easier, see remarks.</param>
        /// <param name="identifier">The identifier.</param>
        /// <param name="metadataName"></param>
        /// <param name="text">
        ///     Text to translate. Should contain <c>{0}</c> etc.
        /// </param>
        /// <returns>Depends on the training mode</returns>
        /// <remarks>
        ///     Path can vary. For types it might be the Namespace + TypeName (<c>Some.NameSpace.TypeName</c>) while
        ///     for HTML views it might be the view uri (<c>/Views/ModelName/SomeViewName</c>).
        /// </remarks>
        public string String(string path, string identifier, string metadataName, string text)
        {
            if (DataSource == null)
                throw new InvalidOperationException("You have to assign a source to the 'DataSource' property first.");

            var value = DataSource.Get(identifier, metadataName);
            if (!string.IsNullOrEmpty(value))
                return value;

            if (!TrainingMode)
                return text;

            var repository = DataSource as ITextRepository;
            if (repository != null)
            {
                repository.Create(identifier, "");
            }

            return MissingField(identifier);
        }
    }

    /// <summary>
    ///     Text provider allows you to get easier localization.
    /// </summary>
    /// <remarks>
    ///     You must specify the <see cref="DataSource" /> oroperty if you want to use this class.
    ///     <para>
    ///         This class just acts as a facade. Methods/Types are really serialized using a string identifier (i.e. the method/type are used to construct a string identifier).
    ///     </para>
    ///     <para>
    ///         <c>Thread.CurrentThread.CurrentUICulture</c> is used to determine which language to load.
    ///     </para>
    /// </remarks>
    /// <seealso cref="ITextDataSource" />
    /// <seealso cref="ITextRepository" />
    public class Localize
    {
        private static readonly Localize _instance = new Localize();
    ();
        private ITextDataSource _dataSource = new StringTableProvider(StringResources.ResourceManager);
        private TypeLocalization _typeLocalization;

        private Localize()
        {
            _typeLocalization = new TypeLocalization(_dataSource);
        }

        public static Localize A
        {
            get { return _instance; }
        }


        /// <summary>
        ///     Gets or sets if we should report missing prompts
        /// </summary>
        /// <remarks>
        ///     If you want texts to get inserted automatically in the data source you have to make sure that
        ///     <see
        ///         cref="DataSource" />
        ///     property implements <see cref="ITextRepository" />.
        /// </remarks>
        public bool TrainingMode { get; set; }

        /// <summary>
        ///     Culture which is used as a base for every other translation.
        /// </summary>
        /// <remarks>
        ///     Used for instance when doing translations. The missing text is shown using the text
        ///     for this culture.
        /// </remarks>
        public CultureInfo SourceCulture { get; set; }

        /// <summary>
        ///     Gets or sets the data source.
        /// </summary>
        public ITextDataSource DataSource
        {
            get
            {
                if (_dataSource == null)
                    throw new InvalidOperationException(
                        "You must specify a source (Localize.DataSource = XXXX) before using the localize class.");
                return _dataSource;
            }
            set
            {
                if (value == null)
                    throw new ArgumentException("value");

                _dataSource = value;
                _typeLocalization = new TypeLocalization(_dataSource);
            }
        }

        /// <summary>
        ///     A text is missing for the current language
        /// </summary>
        public event EventHandler<TextMissingEventArgs> TextMissing = delegate { };


        public string Method(MethodBase methodBase, string metadataName)
        {
            var str = "";
            var key = methodBase.ReflectedType.FullName + "." + methodBase.Name + "/" + metadataName;

            var value = DataSource.Get(key);
            if (!string.IsNullOrEmpty(value))
                return value;

            //TODO: Caches for different languages.
            if (_typeCache.TryGetValue(key, out str))
                return str;

            var attributes = methodBase.GetCustomAttributes<LocalizeAttribute>();
            var attribute = attributes.FirstOrDefault(x => x.MetadataName == metadataName);
            if (attribute != null)
            {
                str = attribute.Text;
            }

            return str;
        }

        public string Method(MethodBase methodBase)
        {
            var str = "";
            var key = methodBase.ReflectedType.FullName + "." + methodBase.Name;

            var value = DataSource.Get(key);
            if (!string.IsNullOrEmpty(value))
                return value;

            //TODO: Caches for different languages.
            if (_typeCache.TryGetValue(key, out str))
                return str;

            var attributes = methodBase.GetCustomAttributes<LocalizeAttribute>();
            var attribute = attributes.FirstOrDefault(x => x.MetadataName == null);
            if (attribute != null)
            {
                str = attribute.Text;
            }

            return str;
        }

        private static string MissingField(string identifier)
        {
            return "-(" + identifier + ")-";
        }

        /// <summary>
        ///     Translate a text
        /// </summary>
        /// <param name="identifier">The identifier.</param>
        /// <param name="text">
        ///     Text to translate. Should contain <c>{0}</c> etc.
        /// </param>
        /// <returns>Depends on the training mode</returns>
        /// <remarks>
        ///     Will use <c>"/"</c> as the path.
        /// </remarks>
        public string String(string identifier, string text)
        {
            return String("/", identifier, text);
        }

        /// <summary>
        ///     Translate a text
        /// </summary>
        /// <param name="path">Path defines where the string being localized is located. It's used to make the administration easier, see remarks.</param>
        /// <param name="identifier">The identifier.</param>
        /// <param name="text">
        ///     Text to translate. Should contain <c>{0}</c> etc.
        /// </param>
        /// <returns>Depends on the training mode</returns>
        /// <remarks>
        ///     Path can vary. For types it might be the Namespace + TypeName (<c>Some.NameSpace.TypeName</c>) while
        ///     for HTML views it might be the view uri (<c>/Views/ModelName/SomeViewName</c>).
        /// </remarks>
        public string String(string path, string identifier, string text)
        {
            if (path == null) throw new ArgumentNullException("path");
            if (identifier == null) throw new ArgumentNullException("identifier");
            if (DataSource == null)
                throw new InvalidOperationException("You have to assign a source to the 'DataSource' property first.");

            var value = DataSource.Get(identifier);
            if (!string.IsNullOrEmpty(value))
                return value;

            if (!TrainingMode)
                return text;

            var repository = DataSource as ITextRepository;
            if (repository != null)
            {
                repository.Create(identifier, "");
            }

            return "-(" + identifier + ")-";
        }

        /// <summary>
        ///     Transate a text
        /// </summary>
        /// <param name="path">Path defines where the string being localized is located. It's used to make the administration easier, see remarks.</param>
        /// <param name="identifier">The identifier.</param>
        /// <param name="metadataName"></param>
        /// <param name="text">
        ///     Text to translate. Should contain <c>{0}</c> etc.
        /// </param>
        /// <returns>Depends on the training mode</returns>
        /// <remarks>
        ///     Path can vary. For types it might be the Namespace + TypeName (<c>Some.NameSpace.TypeName</c>) while
        ///     for HTML views it might be the view uri (<c>/Views/ModelName/SomeViewName</c>).
        /// </remarks>
        public string String(string path, string identifier, string metadataName, string text)
        {
            if (DataSource == null)
                throw new InvalidOperationException("You have to assign a source to the 'DataSource' property first.");

            var value = DataSource.Get(identifier, metadataName);
            if (!string.IsNullOrEmpty(value))
                return value;

            if (!TrainingMode)
                return text;

            var repository = DataSource as ITextRepository;
            if (repository != null)
            {
                repository.Create(identifier, "");
            }

            return MissingField(identifier);
        }


        /// <summary>
        ///     Translate a string from a type
        /// </summary>
        /// <param name="name">Should either be "Class" (if the text is for the class itself) or the property/method name.</param>
        /// <param name="metadataName">Additional field name, for instance if the property has meta data which should get localized.</param>
        /// <returns></returns>
        public string Type<T>(string name, string metadataName) where T : class
        {
            var text = _typeLocalization.Translate(typeof (T), name, metadataName);
            if (text != null)
                return text;

            return MissingField(TypeLocalization.CreateKey(typeof (T), name, metadataName));
        }

        /// <summary>
        ///     Translate a string from a type
        /// </summary>
        /// <param name="name">Should either be "Class" (if the text is for the class itself) or the property/method name.</param>
        /// <returns></returns>
        public string Type<T>(string name) where T : class
        {
            return Type(typeof (T), name);
        }


        /// <summary>
        ///     Translate a string from a type
        /// </summary>
        /// <returns></returns>
        public string Type<T>() where T : class
        {
            return Type(typeof (T), "Class");
        }

        /// <summary>
        ///     Translate a string from a type
        /// </summary>
        /// <param name="type">Type to localize</param>
        /// <param name="name">Should either be "Class" (if the text is for the class itself) or the property/method name.</param>
        /// <returns></returns>
        public string Type(Type type, string name)
        {
            var text = _typeLocalization.Translate(type, name);
            if (!TrainingMode)
                return text ?? name;

            return MissingField(name);
        }
    }
}