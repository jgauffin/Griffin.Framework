using System;
using System.Collections.Generic;
using System.Reflection;
using DotNetCqs;
using Griffin.Core.External.SimpleJson;

namespace Griffin.Cqs.Http
{
    /// <summary>
    ///     Used to map objects that is received from other languages (i.e. using different techniques to identify the .NET
    ///     type).
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         You typically include the .NET type (as a string) or the CQS object name (as long as you've mapped the .NET
    ///         types using <c>Map()</c> or <c>ScanAssembly()</c>).
    ///     </para>
    /// </remarks>
    public class CqsObjectMapper
    {
        private readonly Dictionary<string, Type> _cqsTypes =
            new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        ///     Will use the internal JSON serializer if not specified.
        /// </summary>
        public ICqsDeserializer Deserializer { get; set; }

        /// <summary>
        ///     Deserialize incoming object
        /// </summary>
        /// <param name="dotNetTypeOrCqsName">Name of the dot net type or CQS.</param>
        /// <param name="json">Received JSON.</param>
        /// <returns>CQS object</returns>
        public object Deserialize(string dotNetTypeOrCqsName, string json)
        {
            var type = Type.GetType(dotNetTypeOrCqsName);
            if (type == null)
            {
                if (!_cqsTypes.TryGetValue(dotNetTypeOrCqsName, out type))
                {
                    return null;
                }
            }

            if (string.IsNullOrEmpty(json))
                json = "{}";

            return Deserializer != null
                ? Deserializer.Deserialize(type, json)
                : SimpleJson.DeserializeObject(json, type);
        }

        /// <summary>
        ///     Determines whether the type implements the command handler interface
        /// </summary>
        /// <param name="cqsType">The type.</param>
        /// <returns><c>true</c> if the objects is a command handler; otherwise <c>false</c></returns>
        public static bool IsCqsType(Type cqsType)
        {
            if (cqsType.IsAbstract || cqsType.IsInterface)
                return false;

            if (cqsType.IsSubclassOf(typeof (Command))
                || cqsType.IsSubclassOf(typeof (ApplicationEvent)))
                return true;

            if (cqsType.BaseType == typeof (object) || cqsType.BaseType == null)
                return false;

            if (!cqsType.BaseType.IsGenericType)
                return false;

            var typeDef = cqsType.BaseType.GetGenericTypeDefinition();
            if (typeDef == typeof (Query<>)
                || typeDef == typeof (Request<>))
                return true;


            return false;
        }

        /// <summary>
        ///     Map a type directly.
        /// </summary>
        /// <param name="type">Must implement one of the handler interfaces.</param>
        /// <exception cref="System.ArgumentNullException">type</exception>
        /// <exception cref="System.ArgumentException">
        ///     ' + type.FullName + ' do not implement one of the handler interfaces.
        ///     or
        ///     ' + type.FullName + ' is abstract or an interface.
        /// </exception>
        /// <exception cref="System.InvalidOperationException">
        ///     Duplicate mappings for a name (two different handlers may not have
        ///     the same class name).
        /// </exception>
        /// <remarks>
        ///     Required if the HTTP client do not supply the full .NET type name (just the class name of the command/query).
        /// </remarks>
        public void Map(Type type)
        {
            if (type == null) throw new ArgumentNullException("type");

            if (!IsCqsType(type))
                throw new ArgumentException("'" + type.FullName + "' is not a CQS object.");

            if (_cqsTypes.ContainsKey(type.Name))
                throw new InvalidOperationException(
                    string.Format("Duplicate mappings for name '{0}'. '{1}' and '{2}'.", type.Name, type.FullName,
                        _cqsTypes[type.Name].FullName));

            _cqsTypes.Add(type.Name, type);
        }

        /// <summary>
        ///     Scan assembly for handlers.
        /// </summary>
        /// <remarks>
        ///     Required if the HTTP client do not supply the full .NET type name (just the class name of the command/query).
        /// </remarks>
        /// <param name="assembly">The assembly to scan for handlers.</param>
        /// <exception cref="System.InvalidOperationException">
        ///     Duplicate mappings for a name (two different handlers may not have
        ///     the same class name).
        /// </exception>
        public void ScanAssembly(Assembly assembly)
        {
            foreach (var type in assembly.GetTypes())
            {
                if (!IsCqsType(type))
                    continue;

                if (_cqsTypes.ContainsKey(type.Name))
                    throw new InvalidOperationException(
                        string.Format("Duplicate mappings for name '{0}'. '{1}' and '{2}'.", type.Name, type.FullName,
                            _cqsTypes[type.Name].FullName));

                _cqsTypes.Add(type.Name, type);
            }
        }
    }
}