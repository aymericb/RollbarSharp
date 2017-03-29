using System;
using Microsoft.AspNetCore.Http;

namespace RollbarSharp
{
// TODO: Create middleware? https://docs.microsoft.com/en-us/aspnet/core/migration/http-modules
#if false
    public class RollbarHttpModule : IHttpModule
    {
        public void Init(HttpApplication context)
        {
            context.Error += SendError;
        }

        public void Dispose()
        {
        }

        private static void SendError(object sender, EventArgs e)
        {
            var application = (HttpApplication) sender;
            var ex = application.Server.GetLastError();

            if (ex is HttpUnhandledException)
                ex = ex.InnerException;

            new RollbarClient().SendException(ex);
        }

    }
#endif 
}