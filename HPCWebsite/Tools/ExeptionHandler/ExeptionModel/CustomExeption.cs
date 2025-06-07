using FrameWork.Model.DTO;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using Tools.TextTools;

namespace FrameWork.ExeptionHandler.ExeptionModel
{
    public class CustomException : Exception
    {
        public CustomException(string messageParentKey, string messageKey, object? logData = null, int? statusCode = null)
        {
            LogData = logData;
            MessageParentKey = messageParentKey;
            MessageKey = messageKey;
            StatusCode = statusCode;
        }
        public object? LogData { get; set; }
        public string MessageParentKey { get; set; }
        public string MessageKey { get; set; }
        public bool IsSuccess { get { return GetStatusCode().ToString().StartsWith("2"); } }
        private readonly int? StatusCode;
        public string GetMessage()
        {
            return ResponseMessageHandler.GetMessage(MessageParentKey, MessageKey) ?? "خطایی در عملیات رخ داده است (درصورت اطمینان از صحت داده های خود و تکرار مجدد با پشتیبانی تماس حاصل نمایید)";
        }
        public int GetStatusCode()
        {
            if (StatusCode != null)
                return StatusCode.Value;
            return ResponseMessageHandler.GetStatusCode(MessageParentKey, MessageKey) ?? 503;
        }
    }
}
