﻿using Microsoft.AspNetCore.Http;
using RollbarSharp.Serialization;
using System;

namespace RollbarSharp.Builders
{
    public class PersonModelBuilder
    {
        /// <summary>
        /// Find just the username from server vars: AUTH_USER, LOGON_USER, REMOTE_USER
        /// Sets both the ID and Username to this username since ID is required.
        /// Email address won't be set.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static PersonModel CreateFromHttpRequest(HttpRequest request)
        {
            var id = request.HttpContext.User.Identity;


            return new PersonModel {Id = id.Name, Username = id.Name};
        }


        // FIXME
#if false
        /// <summary>
        /// Find just the username from environment.
        /// Sets both the ID and Username to this username since ID is required.
        /// Email address won't be set.
        /// </summary>
        /// <returns></returns>
        public static PersonModel CreateFromEnvironment()
        {
            //Make the user-id as unique but reproducible as possible (a SID would be even better, but that might be a security risk)
            var id = string.Format(@"{0}\{1}", Environment.MachineName, Environment.UserName);
            
            if (id.Length > 40) {
                id = id.Substring(0, 40);
            }

            return new PersonModel {
                Id = id,
                Username = Environment.UserName };
        }
#endif
    }
}
