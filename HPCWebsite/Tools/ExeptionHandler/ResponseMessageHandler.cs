using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tools.TextTools
{
    public static class ResponseMessageHandler
    {
        public static string? GetMessage(string MessageParentKey, string MessageKey)
        {
            return GetMessageJson(MessageParentKey, MessageKey).Message;
        }
        public static int? GetStatusCode(string MessageParentKey, string MessageKey)
        {

            return GetMessageJson(MessageParentKey, MessageKey).StatusCode;
        }
        private static (int? StatusCode, string? Message) GetMessageJson(string MessageParentKey, string MessageKey)
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Message.json");
            if (File.Exists(filePath))
            {
                var jsonData = System.IO.File.ReadAllText(filePath);

                var jsonObject = JObject.Parse(jsonData);
                if (jsonObject?["fa"]?[MessageParentKey]?.Children<JProperty>() != null)
                    foreach (var errorType in jsonObject["fa"][MessageParentKey]?.Children<JProperty>())
                    {
                        var statusCode = errorType.Name; // خواندن statusCode مانند Entity، Property، و غیره

                        if (errorType.Value[MessageKey] != null)
                        {
                            return (int.Parse(statusCode), (string)errorType.Value[MessageKey]);
                        }
                    }
            }
            return (503, "");
        }
    }
}
