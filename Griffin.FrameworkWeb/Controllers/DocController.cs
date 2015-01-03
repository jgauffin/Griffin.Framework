using System;
using System.Configuration;
using System.IO;
using System.Web;
using System.Web.Mvc;
using GriffinFrameworkWeb.Infrastructure;

namespace GriffinFrameworkWeb.Controllers
{
    public class DocController : Controller
    {
        [Route("doc/{*path}"), HttpGet]
        public ActionResult Index(string path)
        {
            var folderPath = string.IsNullOrEmpty(ConfigurationManager.AppSettings["DocPath"])
                ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DocPages")
                : ConfigurationManager.AppSettings["DocPath"];

            var parser = new Parser(folderPath, Url.Content("~/doc/"));

            //must include doc for all pages to render links correctly
            var html = parser.ParseUrl("doc/" + path);

            return View("Index", (object)html);
        }

        public ActionResult Image(string src)
        {
            var folderPath = string.IsNullOrEmpty(ConfigurationManager.AppSettings["DocPath"])
                ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DocPages")
                : ConfigurationManager.AppSettings["DocPath"];

            var mime = MimeMapping.GetMimeMapping(Path.GetFileName(src));

            return File(Path.Combine(folderPath, src.Replace("/", "\\")), mime);
        }
    }
}