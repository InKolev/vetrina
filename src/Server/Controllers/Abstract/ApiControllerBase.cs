using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Vetrina.Server.Constants;
using Vetrina.Server.Domain;

namespace Vetrina.Server.Controllers.Abstract
{
    [Controller]
    public abstract class ApiControllerBase : ControllerBase
    {
        private User currentUser;

        public User CurrentUser => currentUser ??= (User)HttpContext.Items[AuthenticationControllerConstants.CurrentUser];

        internal string GetCallerIpAddress(bool tryUseXForwardHeader = true)
        {
            var ipAddress = string.Empty;

            // TODO: Add support for the new "Forwarded" header (2014) https://en.wikipedia.org/wiki/X-Forwarded-For
            // X-Forwarded-For (csv list):  Using the First entry in the list seems to work
            // for 99% of cases however it has been suggested that a better (although tedious)
            // approach might be to read each IP from right to left and use the first public IP.
            // http://stackoverflow.com/a/43554000/538763
            if (tryUseXForwardHeader)
            {
                ipAddress = SplitCsv(GetHeaderValueAs<string>("X-Forwarded-For")).FirstOrDefault();
            }

            // RemoteIpAddress is always null in DNX RC1 Update1 (bug).
            if (string.IsNullOrWhiteSpace(ipAddress) && HttpContext?.Connection.RemoteIpAddress != null)
            {
                ipAddress = HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
            }

            if (string.IsNullOrWhiteSpace(ipAddress))
            {
                ipAddress = GetHeaderValueAs<string>("REMOTE_ADDR");
            }

            if (string.IsNullOrWhiteSpace(ipAddress))
            {
                throw new Exception("Unable to determine caller's IP address.");
            }

            return ipAddress;
        }

        internal T GetHeaderValueAs<T>(string headerName)
        {
            var values = new StringValues();

            if (HttpContext?.Request.Headers.TryGetValue(headerName, out values) ?? false)
            {
                var rawValues = values.ToString(); // Writes out as Csv when there are multiple.

                if (!string.IsNullOrWhiteSpace(rawValues))
                {
                    return (T)Convert.ChangeType(values.ToString(), typeof(T));
                }
            }

            return default;
        }

        internal static List<string> SplitCsv(string csv)
        {
            if (string.IsNullOrWhiteSpace(csv))
            {
                return new List<string>();
            }

            return csv
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .ToList();
        }

        internal StatusCodeResult Forbidden()
        {
            return StatusCode((int)HttpStatusCode.Forbidden);
        }

        internal ObjectResult Forbidden(object response)
        {
            return StatusCode((int)HttpStatusCode.Forbidden, response);
        }

        internal StatusCodeResult InternalServerError()
        {
            return StatusCode((int)HttpStatusCode.InternalServerError);
        }

        internal ObjectResult InternalServerError(object response)
        {
            return StatusCode((int)HttpStatusCode.InternalServerError, response);
        }
    }
}