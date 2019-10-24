using MessagePack;
using MessagePack.Resolvers;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Logging
{
    public static class Extensions
    {
        public static string JsonSerialize(this object obj)
        {
            var settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };

            return JsonConvert.SerializeObject(obj, settings);
        }

        public static string ToHashMd5(this string input)
        {
            using (MD5 md5Hash = MD5.Create())
            {
                byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

                StringBuilder sBuilder = new StringBuilder();

                for (int i = 0; i < data.Length; i++)
                {
                    sBuilder.Append(data[i].ToString("x2"));
                }

                return sBuilder.ToString();
            }
        }

        public static T Get<T>(this ISession session, string key)
        {
            session.TryGetValue(key, out byte[] value);
            return value == null
                ? default(T)
                : MessagePackSerializer.Deserialize<T>(value, ContractlessStandardResolver.Instance);
        }

        public static Type GetBaseExceptionType(this Exception exception)
        {
            return (exception.GetBaseException() ?? exception).GetType();
        }

        public static string ToBetterString(this Exception ex, string prepend = null)
        {
            var exceptionMessage = new StringBuilder();

            exceptionMessage.Append("\n" + prepend + "Exception:" + ex.GetType());
            exceptionMessage.Append("\n" + prepend + "Message:" + ex.Message);

            exceptionMessage.Append(GetOtherExceptionProperties(ex, "\n" + prepend));

            exceptionMessage.Append("\n" + prepend + "Source:" + ex.Source);
            exceptionMessage.Append("\n" + prepend + "StackTrace:" + ex.StackTrace);

            exceptionMessage.Append(GetExceptionData("\n" + prepend, ex));

            if (ex.InnerException != null)
                exceptionMessage.Append("\n" + prepend + "InnerException: "
                    + ex.InnerException.ToBetterString(prepend + "\t"));

            return exceptionMessage.ToString();
        }

        private static string GetExceptionData(string prependText, Exception exception)
        {
            var exData = new StringBuilder();
            foreach (var key in exception.Data.Keys.Cast<object>()
                .Where(key => exception.Data[key] != null))
            {
                exData.Append(prependText + String.Format("DATA-{0}:{1}", key,
                    JsonConvert.SerializeObject(exception.Data[key])));
            }

            return exData.ToString();
        }

        private static string GetOtherExceptionProperties(Exception exception, string s)
        {
            var allOtherProps = new StringBuilder();
            var exPropList = exception.GetType().GetProperties();

            var propertiesAlreadyHandled = new List<string>
            { "StackTrace", "Message", "InnerException", "Data", "HelpLink",
                "Source", "TargetSite" };

            foreach (var prop in exPropList
                .Where(prop => !propertiesAlreadyHandled.Contains(prop.Name)))
            {
                var propObject = exception.GetType().GetProperty(prop.Name)
                    .GetValue(exception, null);
                var propEnumerable = propObject as IEnumerable;

                if (propEnumerable == null || propObject is string)
                    allOtherProps.Append(s + String.Format("{0} : {1}",
                        prop.Name, propObject));
                else
                {
                    var enumerableSb = new StringBuilder();
                    foreach (var item in propEnumerable)
                    {
                        enumerableSb.Append(item + "|");
                    }
                    allOtherProps.Append(s + String.Format("{0} : {1}",
                        prop.Name, enumerableSb));
                }
            }

            return allOtherProps.ToString();
        }
    }
}
