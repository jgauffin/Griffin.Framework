using Griffin.Logging.Loggers;
using Griffin.Routing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace HttpRouting
{
    class Program
    {
        static void Main(string[] args)
        {
            var server = new RoutedWebServer(new SystemDebugLogger(typeof(RoutedWebServer)), new ActivatorControllerFactory());

            server.MapAttributes();
            server.On404 = (request) =>
            {
                var resp = request.CreateResponse();
                resp.StatusCode = 404;
                resp.Body = new MemoryStream(Encoding.UTF8.GetBytes("Not found"));
                return resp;
            };

            server.Start(IPAddress.Any, 13488);
            Console.WriteLine("Server listening on http://*:13488/");
            Console.WriteLine("Press enter to exit");
            Console.ReadLine();
            server.Stop();
        }
    }
}
