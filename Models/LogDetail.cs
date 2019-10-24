using Logging.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace Logging.Models
{
    public abstract class LogDetail : BaseEntity<long>
    {
        public LogDetail()
        {
            Timestamp = DateTime.Now;
        }

        public DateTime Timestamp { get; }

        public string Message { get; set; }

        // Where
        public string Product { get; set; }

        public string Hostname { get; set; }

        public string Location { get; set; }

        // Who
        public int? UserId { get; set; }

        public string UserName { get; set; }

        public string SessionId { get; set; }

        // Everything Else
        public string CorrelationId { get; set; }

        public string AdditionalInfo { get; set; }

        public Dictionary<string, object> AdditionalInfoDictionary { get; set; }

        public void PopulateModel(string message, HttpContext httpContext, IConfiguration configuration, Dictionary<string, object> additionalInfo, bool skipAdditionalInfo)
        {
            Message = message;
            Product = configuration["Serilog:Model:Product"];
            Hostname = Environment.MachineName;
            CorrelationId = httpContext.TraceIdentifier;
            AdditionalInfoDictionary = additionalInfo ?? new Dictionary<string, object>();

            AddUserData(httpContext, skipAdditionalInfo);
            AddRequestData(httpContext, skipAdditionalInfo);
            AddSessionData(configuration, httpContext, skipAdditionalInfo);
            AddCookies(httpContext, skipAdditionalInfo);
        }

        private void AddCookies(HttpContext httpContext, bool skipAdditionalInfo)
        {
            if (!SkipAdditionalInfo(httpContext, skipAdditionalInfo))
            {
                var cookies = httpContext.Request.Cookies;

                if (cookies != null)
                {
                    var additionalInfo = new Dictionary<string, object>();
                    foreach (var key in cookies.Keys)
                    {
                        additionalInfo.Add(key, cookies[key]);
                    }
                    if (additionalInfo.Count > 0)
                    {
                        AdditionalInfoDictionary.Add("Cookies", additionalInfo);
                    }
                }
            }
        }

        private void AddSessionData(IConfiguration configuration, HttpContext httpContext, bool skipAdditionalInfo)
        {
            var logSession = Convert.ToBoolean(configuration["Serilog:Model:LogSession"]);

            if (logSession)
            {
                AddSessionData(httpContext, skipAdditionalInfo);
            }
            else
            {
                SessionId = httpContext.Request.Headers.ContainsKey("Authorization") ? httpContext.Request.Headers["Authorization"][0] : httpContext.TraceIdentifier;
            }

            SessionId = SessionId.ToHashMd5();
        }

        private void AddSessionData(HttpContext httpContext, bool skipAdditionalInfo)
        {
            var session = httpContext.Session;

            if (session != null)
            {
                SessionId = session.Id;

                if (!SkipAdditionalInfo(httpContext, skipAdditionalInfo))
                {
                    var additionalInfo = new Dictionary<string, object>();
                    foreach (var key in session.Keys)
                    {
                        additionalInfo.Add(key, session.Get<object>(key));
                    }
                    if (additionalInfo.Count > 0)
                    {
                        AdditionalInfoDictionary.Add("SessionData", additionalInfo);
                    }
                }
            }
        }

        private void AddRequestData(HttpContext httpContext, bool skipAdditionalInfo)
        {
            var request = httpContext.Request;

            if (request != null)
            {
                Location = request.Path;

                if (!SkipAdditionalInfo(httpContext, skipAdditionalInfo))
                {
                    AdditionalInfoDictionary.Add("UserAgent", request.Headers["User-Agent"]);
                    AdditionalInfoDictionary.Add("Languages", request.Headers["Accept-Language"]);

                    var queries = Microsoft.AspNetCore.WebUtilities
                        .QueryHelpers.ParseQuery(request.QueryString.ToString());

                    var additionalInfo = new Dictionary<string, object>();

                    foreach (var query in queries)
                    {
                        additionalInfo.Add(query.Key, query.Value);
                    }
                    if (additionalInfo.Count > 0)
                    {
                        AdditionalInfoDictionary.Add("QueryStrings", additionalInfo);
                    }
                }
            }
        }

        private void AddUserData(HttpContext httpContext, bool skipAdditionalInfo)
        {
            var user = httpContext.User;

            if (user != null)
            {
                var additionalInfo = new Dictionary<string, object>();
                foreach (var claim in user.Claims)
                {
                    if (claim.Type == ClaimTypes.NameIdentifier)
                    {
                        int.TryParse(claim.Value, out int userId);
                        UserId = userId;
                    }
                    else if (claim.Type == ClaimTypes.Name)
                    {
                        UserName = claim.Value;
                    }
                    else
                    {
                        if (!SkipAdditionalInfo(httpContext, skipAdditionalInfo))
                        {
                            additionalInfo.Add(claim.Type, claim.Value);
                        }
                    }
                }

                if (additionalInfo.Count > 0)
                {
                    AdditionalInfoDictionary.Add("UserClaims", additionalInfo);
                }
            }
        }

        private bool SkipAdditionalInfo(HttpContext httpContext, bool skipAdditionalInfo) => skipAdditionalInfo || httpContext.Items.ContainsKey(HttpContextItems.SessionUpdated);
    }
}
