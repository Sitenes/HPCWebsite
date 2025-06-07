using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using Tools.TextTools;

namespace Tools.LoggingTools
{
    public class Logging
    {
        private readonly ILogger<Logging> _logger;

        public Logging(ILogger<Logging> logger)
        {
            _logger = logger;
        }
        private void WriteLog(LogLevel logLevel, Exception exception, string message)
        {
            switch (logLevel)
            {
                case LogLevel.Critical:
                    _logger.LogCritical(exception, message);
                    break;
                case LogLevel.Error:
                    _logger.LogError(exception, message);
                    break;
                case LogLevel.Warning:
                    _logger.LogWarning(exception, message);
                    break;
                case LogLevel.Information:
                    _logger.LogInformation(exception, message);
                    break;
                case LogLevel.Debug:
                    _logger.LogDebug(exception, message);
                    break;
                case LogLevel.Trace:
                    _logger.LogTrace(exception, message);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, null);
            }
        }
        private string CreateFormattedMessage(string title, params (string Key, object Value)[] properties)
        {
            var formattedProperties = string.Join(",", properties.Select(p => @$"""{p.Key}"":""{p.Value}"""));
            return $"{title} {{{formattedProperties}}}";
        }
        public void LogErrorMiddleware(
    Exception exception,
    string message,
    object? logData,
    string requestBody,
    string requestFormData,
    string headers)
        {
            string formattedMessage = CreateFormattedMessage(
                message,
                ("LogData", ConvertObjectToString(logData)),
                ("RequestBody", requestBody),
                ("RequestFormData", requestFormData),
                ("Headers", headers)
            );

            WriteLog(LogLevel.Critical, exception, formattedMessage);
        }

        public void LogUserLoginSuccess(
            int userId,
            string username,
            string ip,
            string userAgent)
        {
            string formattedMessage = CreateFormattedMessage(
                "User login success",
                ("userId", userId),
                ("username", username),
                ("ip", ip),
                ("userAgent", userAgent)
            );

            WriteLog(LogLevel.Warning, null, formattedMessage);
        }

        public void LogUserChangePassword(
            int userId,
            string username,
            string ip,
            string userAgent)
        {
            string formattedMessage = CreateFormattedMessage(
                "User Change Password",
                ("userId", userId),
                ("username", username),
                ("ip", ip),
                ("userAgent", userAgent)
            );

            WriteLog(LogLevel.Warning, null, formattedMessage);
        }

        public void LogAddServer(
          string ServerId,
          string username,
          string ip,
          string userAgent)
        {
            string formattedMessage = CreateFormattedMessage(
                "User Change Password",
                ("userId", ServerId),
                ("username", username),
                ("ip", ip),
                ("userAgent", userAgent)
            );

            WriteLog(LogLevel.Warning, null, formattedMessage);
        }

        public void LogUserLogout(
            int userId,
            string username,
            string ip,
            string userAgent)
        {
            string formattedMessage = CreateFormattedMessage("User Logout",
                ("userId", userId),
                ("username", username),
                ("ip", ip),
                ("userAgent", userAgent));

            WriteLog(LogLevel.Warning, null, formattedMessage);
        }
        public void TestLog(
           string Key,
           string Value)
        {
            string formattedMessage = CreateFormattedMessage("Custom Log",
                (Key, Value));

            WriteLog(LogLevel.Information, null, formattedMessage);
        }
        public static string ConvertObjectToString(object? input)
        {
            if (input == null)
            {
                return "null";
            }

            if (input is string str)
            {
                return str;
            }

            if (input is IEnumerable enumerable)
            {
                var result = new StringBuilder();
                result.Append("[");
                foreach (var item in enumerable)
                {
                    result.Append(ConvertObjectToString(item) + ", ");
                }

                if (result.Length > 1)
                    result.Remove(result.Length - 2, 2); // حذف ویرگول اضافی

                result.Append("]");
                return result.ToString();
            }

            if (input.GetType().IsPrimitive || input is decimal)
            {
                return input.ToString();
            }

            try
            {
                return JsonConvert.SerializeObject(input, Formatting.Indented);
            }
            catch
            {
                var result = new StringBuilder();
                var properties = input.GetType().GetProperties();

                result.Append("{ ");
                foreach (var property in properties)
                {
                    var name = property.Name;
                    var value = property.GetValue(input, null);
                    result.Append($"{name}: {ConvertObjectToString(value)}, ");
                }

                if (result.Length > 2)
                    result.Remove(result.Length - 2, 2);

                result.Append(" }");
                return result.ToString();
            }
        }
    }
}
