using Griffin.Net.Protocols.Http;
using Griffin.Routing;
using Griffin.Routing.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HttpRouting.Controllers
{
    [RoutePrefix("/home")]
    class HomeController : Controller
    {
        [Route("/")]
        public HttpResponse Index()
        {
            return OkText("HomeController->Index");
        }

        [Route("/{name:string}")]
        public HttpResponse Index(string name)
        {
            return OkText("HomeController->Index " + name);
        }

        [Route("/exit/{i:int}")]
        public HttpResponse Exit(int i)
        {
            return OkText("HomeController->Exit " + i);
        }
    }
}
