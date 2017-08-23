using Griffin.Cqs.AspNetCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Builder
{
    public static class GriffinCqsApplicationBuilderExtensions
    {
        /// <summary>
        /// Adds GriffinCqs to the pipeline.
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseGriffinCqs(this IApplicationBuilder app)
        {
            app.UseMiddleware<CqsMiddleware>();
            return app;
        }
    }
}
