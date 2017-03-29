using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace RollbarSharp
{
    internal static class InternalExtensions
    {
        /// <summary>
        /// Convert a <see cref="NameValueCollection"/> to a dictionary which is far more usable.
        /// </summary>
        /// <param name="col"></param>
        /// <returns></returns>
        public static IDictionary<string, string> ToDictionary(this NameValueCollection col)
        {
            if (col == null || col.Count == 0)
                return new Dictionary<string, string>();

            return col.AllKeys.Where(key => key != null).ToDictionary(key => key, key => col[key]);
        }

        // TODO: Migrate session state
#if false

        /// <summary>
        /// Convert the objects in a session dictionary to a readable string dictionary
        /// </summary>
        /// <param name="session"></param>
        /// <returns></returns>
        public static IDictionary<string, string> ToDictionary(this HttpSessionState session)
        {
            if (session == null || session.Count == 0)
                return new Dictionary<string, string>();

            return session.Keys.Cast<string>()
                          .Where(key => key != null)
                          .ToDictionary(key => key, key => session[key].Describe());
        }
#endif


        /// <summary>
        /// Convert an unknown object into a readable string
        /// Strings and value types simply call the ToString() method
        /// Arrays are returned as a comma-separated list surrounded by square brackets
        /// Other objects return the type name, hash code, and their ToString() method
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string Describe(this object obj)
        {
            if (obj == null)
                return "(NULL)";

            var type = obj.GetType();

            if (type == typeof(string))
                return obj.ToString();

//          FIXME
//            if (type.IsValueType)
//                return obj.ToString();

            if (type.IsArray)
            {
                var temp = (Array)obj;
                return temp.Cast<object>().Describe();
            }

            return "<" + type.Name + ":0x" + obj.GetHashCode().ToString("X") + ">: " + obj;
        }

        public static string Describe<T>(this IEnumerable<T> source)
        {
            if (source == null)
                return "(null)";

            return "[" + string.Join(", ", source.Select(x => x.Describe())) + "]";
        }
    }
}
