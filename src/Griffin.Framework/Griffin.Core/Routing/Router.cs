using Griffin.Net.Protocols.Http;
using Griffin.Routing.Attributes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Security.Authentication;
using System.Threading;

namespace Griffin.Routing
{
    /// <summary>
    /// Routing class for RoutedWebServer
    /// </summary>
    internal sealed class Router
    {
        const int StaticSegmentPriority = 3;
        const int TypedSegmentPriority = 9;
        const int StringSegmentPriority = 10;
        const string SegmentSeperator = "/";
        private List<intRoute> _routes = null;
        private Action<string> _logger = null;
        private IControllerFactory _factory = null;
        private bool _hideUnauthorized = false;

        /// <summary>
        /// Creates a new instance of the Router
        /// </summary>
        /// <param name="factory">class to create the controller instances</param>
        /// <param name="logger">error logging action</param>
        /// <param name="hideUnauthorized">
        ///     true to ignore routes where the client doesn't have the permissions, 
        ///     will return 404 instead of 401
        /// </param>
        public Router(IControllerFactory factory, Action<string> logger = null, bool hideUnauthorized = false)
        {
            if (factory == null)
                throw new ArgumentNullException("factory");

            _hideUnauthorized = hideUnauthorized;
            _factory = factory;
            _routes = new List<intRoute>();
            if (logger != null)
                _logger = logger;
            else
                _logger = (a) => { System.Diagnostics.Debug.Fail(a + Environment.NewLine); };
        }

        /// <summary>
        /// will map all route/routeprefix attributes on controller classes
        /// in the current appdomain
        /// </summary>
        public void MapAttributes()
        {
            foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type t in a.GetTypes())
                {
                    if (t.IsClass && !t.IsAbstract)
                    {
                        if (t.IsSubclassOf(typeof(Controller)))
                        {
                            string prefix = "";
                            bool auth = false;
                            string[] authorize = null;

                            var preAttr = t.GetCustomAttribute<RoutePrefixAttribute>();
                            if (preAttr != null)
                            {
                                prefix = preAttr.Prefix;
                                if (prefix.EndsWith(SegmentSeperator))
                                    prefix = prefix.Remove(prefix.Length - SegmentSeperator.Length);
                            } // if (preAttr != null)

                            var authAttr = t.GetCustomAttribute<AuthenticateAttribute>();
                            if (authAttr != null)
                                auth = true;

                            var authorizeAttr = t.GetCustomAttribute<AuthorizeAttribute>();
                            if (authorizeAttr != null)
                            {
                                auth = true;
                                authorize = authorizeAttr.Groups;
                            } // if (authorizeAttr != null)

                            var methods = t.GetMethods();
                            foreach (var method in methods)
                            {
                                var rtAttr = method.GetCustomAttribute<RouteAttribute>();
                                if (rtAttr != null)
                                {
                                    bool methAuth = auth;
                                    var methAutorize = authorize;
                                    if (auth)
                                    {
                                        var mNAuthAttr = method.GetCustomAttribute<AllowAnonymousAttribute>();
                                        if (mNAuthAttr != null)
                                            methAuth = false;
                                    } // if (auth)
                                    else
                                    {
                                        var mAuthAttr = method.GetCustomAttribute<AuthenticateAttribute>();
                                        if (mAuthAttr != null)
                                            methAuth = true;
                                    } // if (auth) else
                                    var mAuthorizeAttr = method.GetCustomAttribute<AuthorizeAttribute>();
                                    if (mAuthorizeAttr != null)
                                    {
                                        if (mAuthorizeAttr.AppendClassGroups && methAutorize != null)
                                        {
                                            var combinedGroups = new string[methAutorize.Length + mAuthorizeAttr.Groups.Length];
                                            for (int i = 0; i < methAutorize.Length; i++)
                                            {
                                                combinedGroups[i] = methAutorize[i];
                                            }
                                            for (int i = 0; i < mAuthorizeAttr.Groups.Length; i++)
                                            {
                                                combinedGroups[i + methAutorize.Length] = mAuthorizeAttr.Groups[i];
                                            }
                                            methAutorize = combinedGroups;
                                        } // if (mAuthorizeAttr.AppendClassGroups && methAutorize != null)
                                    } // if (mAuthorizeAttr != null)

                                    string route = prefix;
                                    if (rtAttr.IgnorePrefix)
                                        route = "";
                                    if (!rtAttr.RouteMask.StartsWith(SegmentSeperator))
                                        route += SegmentSeperator;
                                    route += rtAttr.RouteMask;

                                    _routes.Add(new intRoute(route, t, method, methAuth, methAutorize, SegmentSeperator));
                                } // if (rtAttr != null)
                            } // foreach (var method in methods)
                        } // if (t.IsSubclassOf(typeof(Controller)))
                    } // if (t.IsClass && !t.IsAbstract)
                } // foreach (Type t in a.GetTypes())
            } // foreach (Assembly a in appDomain.GetAssemblies())
        }

        /// <summary>
        /// Add route
        /// </summary>
        /// <param name="routeMask"></param>
        /// <param name="controller"></param>
        /// <param name="methodInfo"></param>
        /// <param name="authenticate"></param>
        /// <param name="requiredGroups"></param>
        public void AddRoute(string routeMask, Type controller, MethodInfo methodInfo, bool authenticate, string[] requiredGroups)
        {
            _routes.Add(new intRoute(routeMask, controller, methodInfo, authenticate, requiredGroups, SegmentSeperator));
        }

        /// <summary>
        /// Route the request to the right controller
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public object Route(HttpRequest request)
        {
            string url = request.Uri.AbsolutePath;
            bool authenticated = Thread.CurrentPrincipal.Identity.IsAuthenticated;

            var routes = _routes.FindAll(p => p.Matches(url));
            if(_hideUnauthorized)
            {
                routes = routes.FindAll(p => p.Authenticate ? p.Authenticate.Equals(authenticated) : true && p.AuthGroups.All(x => Thread.CurrentPrincipal.IsInRole(x)));
            }

            var route = routes.OrderBy(p => p.Priority).FirstOrDefault();
            if (route != null)
            {
                if(!_hideUnauthorized)
                {
                    if (route.Authenticate && !authenticated)
                        throw new AuthenticationException();
                    if (!route.AuthGroups.All(p => Thread.CurrentPrincipal.IsInRole(p)))
                        throw new UnauthorizedAccessException();
                }

                var rtValues = route.Route(url);

                var ctrl = _factory.CreateNew(route.Controller);
                ctrl.Request = request;

                var @params = new List<object>();

                foreach (var param in route.Method.GetParameters())
                {
                    if(rtValues.ContainsKey(param.Name))
                    {
                        @params.Add(rtValues[param.Name]);
                    }
                    else
                    {
                        if (param.HasDefaultValue)
                            @params.Add(param.DefaultValue);
                        else
                        {
                            _logger("Router->Route error mapping value Url: " + url);
                            return null;
                        }
                    }
                }

                return route.Method.Invoke(ctrl, @params.ToArray());
            }

            return null;
        }

        private class intRoute
        {
            private List<intRouteSegment> Segments;
            private int _requiredArguments = 0;
            private string _segmentSeperator = null;

            public int Priority { get; private set; }
            public Type Controller { get; private set; }
            public MethodInfo Method { get; private set; }
            public bool Authenticate { get; private set; }
            public string[] AuthGroups { get; private set; }

            public intRoute(string routeMask, Type controllerType, MethodInfo methodInfo, bool authenticate, string[] requiredGroups, string segmentSeperator)
            {
                Controller = controllerType;
                Method = methodInfo;
                Authenticate = authenticate;
                AuthGroups = requiredGroups;
                _segmentSeperator = segmentSeperator;

                if (AuthGroups == null)
                    AuthGroups = new string[0];

                ParseRouteMask(routeMask);

                var @params = methodInfo.GetParameters();
                foreach (var param in @params)
                {
                    if (!param.HasDefaultValue)
                        _requiredArguments++;
                }

                if (_requiredArguments > Segments.Where(p => p.Priority > StaticSegmentPriority).Count())
                    throw new Exception("Can not route this, there are not enaught placeholders.");

            }

            private void ParseRouteMask(string routeMask)
            {
                if (routeMask.StartsWith(_segmentSeperator))
                    routeMask = routeMask.Remove(0, _segmentSeperator.Length);

                string[] rawSegments = routeMask.Split(new string[] { _segmentSeperator }, StringSplitOptions.RemoveEmptyEntries);

                if (rawSegments.Length == 0)
                    return;

                Segments = new List<intRouteSegment>(rawSegments.Length);
                for (int i = 0; i < rawSegments.Length; i++)
                {
                    Segments.Add(new intRouteSegment(rawSegments[i]));
                }
                Priority = 0;
                for (int i = 0; i < Segments.Count; i++)
                {
                    Priority += Segments[i].Priority;
                }
            }

            public bool Matches(string route)
            {
                if (route.StartsWith(_segmentSeperator))
                    route = route.Remove(0, _segmentSeperator.Length);

                string[] rawSegments = route.Split(new string[] { _segmentSeperator }, StringSplitOptions.RemoveEmptyEntries);

                if (rawSegments.Length != Segments.Count)
                    return false;

                for (int i = 0; i < rawSegments.Length && i < Segments.Count; i++)
                {
                    if (!Segments[i].Matches(rawSegments[i]))
                        return false;
                }

                return true;
            }

            public Dictionary<string, object> Route(string route)
            {
                var ret = new Dictionary<string, object>();
                if (route.StartsWith(_segmentSeperator))
                    route = route.Remove(0, _segmentSeperator.Length);

                string[] rawSegments = route.Split(new string[] { _segmentSeperator }, StringSplitOptions.RemoveEmptyEntries);

                if (rawSegments.Length < Segments.Count)
                    return ret;

                for (int i = 0; i < rawSegments.Length; i++)
                {
                    var res = Segments[i].Route(rawSegments[i]);
                    ret.Add(res.Key, res.Value);
                }

                return ret;
            }
        }
        private class intRouteSegment
        {
            private string _rawSegment = null;
            private SegmentType _type = SegmentType.RAW;
            private string _name = null;

            public int Priority { get; private set; }
            
            public intRouteSegment(string segment)
            {
                _rawSegment = segment;
                ParseSegment(segment);
            }
            
            private void ParseSegment(string segment)
            {
                if(segment.StartsWith("{") && segment.EndsWith("}"))
                {
                    int idx = segment.IndexOf(':');
                    if(idx == -1)
                    {
                        _type = SegmentType.STRING;
                        _name = segment.Remove(segment.Length - 1, 1).Remove(0, 1);
                    }
                    else
                    {
                        _name = segment.Substring(1, idx - 1);
                        string type = segment.Substring(idx + 1);
                        type = type.Remove(type.Length - 1, 1);
                        _type = ParseType(type);
                    }

                    if (_type == SegmentType.STRING)
                        Priority = StringSegmentPriority;
                    else
                        Priority = TypedSegmentPriority;
                }
                else
                {
                    _type = SegmentType.RAW;
                    _name = _rawSegment = segment;
                    Priority = StaticSegmentPriority;
                }
            }

            private SegmentType ParseType(string type)
            {
                type = type.ToLower();
                switch (type)
                {
                    case "short":
                    case "int16":
                    case "int":
                    case "int32":
                        return SegmentType.INT;
                        break;
                    case "long":
                    case "int64":
                        return SegmentType.LONG;
                        break;
                    case "float":
                    case "double":
                        return SegmentType.DOUBLE;
                        break;
                    case "bool":
                    case "bit":
                    case "on":
                    case "onoff":
                        return SegmentType.BOOL;
                        break;
                }
                return SegmentType.STRING;
            }

            public bool Matches(string segment)
            {
                string[] boolValues = new string[] { "true", "false", "1", "0", "on", "off" };
                switch (_type)
                {
                    case SegmentType.RAW:
                        return segment.CompareTo(_rawSegment) == 0;
                        break;
                    case SegmentType.STRING:
                        return true;
                        break;
                    case SegmentType.INT:
                        int intVal;
                        return int.TryParse(segment, out intVal);
                        break;
                    case SegmentType.LONG:
                        long longVal;
                        return long.TryParse(segment, out longVal);
                        break;
                    case SegmentType.DOUBLE:
                        double dblVal;
                        return double.TryParse(segment, NumberStyles.Any, CultureInfo.InvariantCulture, out dblVal);
                        break;
                    case SegmentType.BOOL:
                        return boolValues.Contains(segment.ToLower());
                        break;
                }

                return false;
            }
            public KeyValuePair<string, object> Route(string segment)
            {
                switch (_type)
                {
                    case SegmentType.STRING:
                        return new KeyValuePair<string, object>(_name, segment);
                        break;
                    case SegmentType.INT:
                        return new KeyValuePair<string, object>(_name, int.Parse(segment));
                        break;
                    case SegmentType.LONG:
                        return new KeyValuePair<string, object>(_name, long.Parse(segment));
                        break;
                    case SegmentType.DOUBLE:
                        return new KeyValuePair<string, object>(_name, double.Parse(segment, NumberStyles.Any, CultureInfo.InvariantCulture));
                        break;
                    case SegmentType.BOOL:
                        return new KeyValuePair<string, object>(_name, ParseUrlBool(segment));
                        break;
                }

                return new KeyValuePair<string, object>(_rawSegment, null);
            }
            private bool ParseUrlBool(string segment)
            {
                switch (segment.ToLower())
                {
                    case "true":
                    case "1":
                    case "on":
                        return true;
                        break;
                    case "false":
                    case "0":
                    case "off":
                        return false;
                        break;
                    default:
                        return false;
                        break;
                }
            }

            private enum SegmentType
            {
                RAW,
                STRING,
                INT,
                LONG,
                DOUBLE,
                BOOL,
            }
        }
    }
}
