using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using Autofac;
using Griffin.Core.Autofac;
using Griffin.Cqs;
using Griffin.Cqs.Authorization;
using Griffin.Cqs.Http;
using Griffin.Cqs.InversionOfControl;
using Griffin.Net.Channels;
using Griffin.Net.Protocols.Http;
using Griffin.Net.Protocols.Http.Authentication;
using HttpCqs.Contracts;

namespace HttpCqs.Server
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            // tell library to authorize the usage of command/queries
            GlobalConfiguration.AuthorizationFilter = new RoleAuthorizer();

            // build autofac
            var adapter = BuildContainer();

            var builder = new IocBusBuilder(adapter);
            var listener = new CqsHttpListener(builder.BuildMessageProcessor());
            listener.ScanAssembly(typeof(GetUsers).Assembly);
            listener.Authenticator = CreateAuthenticator();
            listener.Logger = Console.WriteLine;
            listener.RequestFilter += OnServeFiles;
            listener.Start(new IPEndPoint(IPAddress.Any, 8899));

            Console.WriteLine("Browse to http://localhost:8899 and login with user/user or admin/admin.");
            Console.WriteLine("Press ENTER to quit");
            Console.ReadLine();
        }

        private static HttpResponse OnServeFiles(ITcpChannel channel, HttpRequest request)
        {
            //do not do anything, lib will handle it.
            if (request.Uri.AbsolutePath.StartsWith("/cqs"))
                return null;

            var response = request.CreateResponse();

            var uriWithoutTrailingSlash = request.Uri.AbsolutePath.TrimEnd('/');
            var path = Debugger.IsAttached
                ? Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\public\\", uriWithoutTrailingSlash))
                : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "public\\", uriWithoutTrailingSlash);
            if (path.EndsWith("\\"))
                path = Path.Combine(path, "index.html");

            if (!File.Exists(path))
            {
                response.StatusCode = 404;
                return response;
            }

            var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            var extension = Path.GetExtension(path).TrimStart('.');
            response.ContentType = ApacheMimeTypes.Apache.MimeTypes[extension];
            response.Body = stream;
            return response;
        }

        private static IAuthenticator CreateAuthenticator()
        {
            var accountService = new SimpleAccountService();
            accountService.Add("admin", "admin", new[] { "Admin", "User" });
            accountService.Add("user", "user", new[] { "User" });
            var basicAuth = new BasicAuthentication(accountService, "localhost");
            return basicAuth;
        }

        private static AutofacAdapter BuildContainer()
        {
            var containerBuilder = new ContainerBuilder();
            containerBuilder.RegisterServices(Assembly.GetExecutingAssembly());
            var container = containerBuilder.Build();
            var adapter = new AutofacAdapter(container);
            return adapter;
        }


    }
}