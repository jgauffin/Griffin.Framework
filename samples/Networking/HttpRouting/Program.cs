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
    /**
     * Small parts documentation
     * 
     * The Route attribute is used to define the HttpUrl Route
     * for the function of the controller.
     * 
     * The RouteMask attribute is only some prefix for your
     * Route's in route attributes simple {RouteMask}{Route}.
     * 
     * Route parameters can be everywhere in your route string
     * but only as uri segments so /{param:type}/ is valid but
     * /Home{param:type}/ is not valid. 
     * 
     * BEWARE: this is a working and stable version of the 
     *         router but its fair away from perfect.
     */
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
