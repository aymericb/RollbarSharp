using System;
using Microsoft.AspNetCore.Http;
using RollbarSharp.Serialization;

namespace RollbarSharp.Builders
{
    public static class ServerModelBuilder
    {
        /// <summary>
        /// Creates a <see cref="ServerModel"/> using data from the given
        /// <see cref="HttpRequest"/>. Finds: HTTP Host, Server Name, Application Physical Path
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static ServerModel CreateFromHttpRequest(HttpRequest request)
        {
            var host = request.Host.ToString();
            
            // FIXME var root = request.ServerVariables.Get("APPL_PHYSICAL_PATH");
            // See https://blog.mariusschulz.com/2016/05/22/getting-the-web-root-path-and-the-content-root-path-in-asp-net-core

            var machine = Environment.MachineName;

            return new ServerModel {
                Host = host,
                Machine = machine,
            };
        }
    }
}
