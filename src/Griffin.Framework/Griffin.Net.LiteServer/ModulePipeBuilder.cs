using System;
using System.Collections.Generic;
using Griffin.Net.LiteServer.Modules;

namespace Griffin.Net.LiteServer
{
    /// <summary>
    ///     Build a handler pipeline used to identify the user and then take care of the incoming message
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Modules will be run in the following order:
    ///         <list type="number">
    ///             <item>Authentication</item>
    ///             <item>Authorization</item>
    ///             <item>PreRequestHandler</item>
    ///             <item>RequestHandler</item>
    ///             <item>PostRequestHandler</item>
    ///         </list>
    ///     </para>
    /// </remarks>
    public class ModulePipeBuilder
    {
        private readonly List<IServerModule> _authenticateModules = new List<IServerModule>();
        private readonly List<IServerModule> _authorizeModules = new List<IServerModule>();
        private readonly List<IServerModule> _postRequestModules = new List<IServerModule>();
        private readonly List<IServerModule> _preRequestModules = new List<IServerModule>();
        private readonly List<IServerModule> _requestModules = new List<IServerModule>();

        /// <summary>
        ///     Add a module which is used to authenticate (identitfy) the user.
        /// </summary>
        /// <param name="module">Module to add</param>
        /// <remarks>
        ///     <para>
        ///         These are the first modules which will be invoked when a new message arrives.
        ///     </para>
        /// </remarks>
        public void AddAuthentication(IServerModule module)
        {
            if (module == null) throw new ArgumentNullException("module");
            _authenticateModules.Add(module);
        }

        /// <summary>
        ///     Add a module which is used to check if the user have access to execute the incoming message
        /// </summary>
        /// <param name="module">Module</param>
        /// <remarks>
        ///     <para>
        ///         You can for instance user role based authentication on each message to verify that
        ///     </para>
        /// </remarks>
        public void AddAuthorization(IServerModule module)
        {
            if (module == null) throw new ArgumentNullException("module");
            _authorizeModules.Add(module);
        }

        /// <summary>
        ///     Builder the pipeline that the server will execute.
        /// </summary>
        /// <returns>Modules</returns>
        public IServerModule[] Build()
        {
            var allModules = new List<IServerModule>();
            allModules.AddRange(_authenticateModules);
            allModules.AddRange(_authorizeModules);
            allModules.AddRange(_preRequestModules);
            allModules.AddRange(_requestModules);
            allModules.AddRange(_postRequestModules);
            return allModules.ToArray();
        }

        /// <summary>
        ///     Add a module which is designed to handle certain kind of requests.
        /// </summary>
        /// <param name="module">Module</param>
        /// <remarks>
        ///     <para>
        ///         Used to process messages to generate responses.
        ///     </para>
        /// </remarks>
        public void AddHandler(IServerModule module)
        {
            if (module == null) throw new ArgumentNullException("module");
            _requestModules.Add(module);
        }

        /// <summary>
        ///     Add a module which is designed to handle certain kind of requests.
        /// </summary>
        /// <param name="module">Module</param>
        /// <remarks>
        ///     <para>
        ///         Used to process messages to generate responses.
        ///     </para>
        /// </remarks>
        public void AddPostHandler(IServerModule module)
        {
            if (module == null) throw new ArgumentNullException("module");
            _requestModules.Add(module);
        }

        /// <summary>
        ///     Add a module which do some pre processing before the actual request is being handled (user have been authenticated
        ///     and authorized when this module is being invoked)
        /// </summary>
        /// <param name="module">Module</param>
        public void AddPreHandler(IServerModule module)
        {
            if (module == null) throw new ArgumentNullException("module");
            _preRequestModules.Add(module);
        }
    }
}