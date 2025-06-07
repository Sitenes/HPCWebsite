using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;
using FrameWork.ExeptionHandler.ExeptionModel;
using FrameWork;
using HPCWebsite.Controllers;
using Microsoft.Extensions.Logging;
using System.Collections;
using Tools.TextTools;
using Tools.LoggingTools;
using ViewModels;

namespace AutomationEngine.CustomMiddlewares.Extensions
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly Logging _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, Logging logger)
        {
            _next = next;
            _logger = logger;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                context.Request.EnableBuffering();
                // فراخوانی middleware بعدی
                await _next(context);
            }
            catch (CustomException ex)
            {
                var resultModel = await ExceptionHandling.HandleCustomExceptionAsync(context, ex);
                _logger.LogErrorMiddleware(
                    ex,
                    resultModel.message + " Custom Exception : ",
                    ex.LogData,
                    await ExceptionHandling.GetRequestBodyAsync(context),
                    ExceptionHandling.GetFormData(context),
                    JsonConvert.SerializeObject(context.Request.Headers)
                    );
            }
            catch (Exception ex)
            {
                var resultModel = await ExceptionHandling.HandleGeneralExceptionAsync(context, ex);
                _logger.LogErrorMiddleware(
                    ex,
                    resultModel.message + " General Exception : ",
                    ex.Data,
                    await ExceptionHandling.GetRequestBodyAsync(context),
                    ExceptionHandling.GetFormData(context),
                    JsonConvert.SerializeObject(context.Request.Headers)
                    );
            }
        }
    }
    public static class ExceptionHandling
    {
        public static async Task<ResultViewModel> HandleCustomExceptionAsync(HttpContext context, CustomException ex)
        {
            var output = new ResultViewModel()
            {
                message = ex.GetMessage(),
                status = false,
                statusCode = ex.GetStatusCode()
            };

            var environment = context.RequestServices.GetService<IWebHostEnvironment>();
            if (environment != null && environment.IsDevelopment())
            {
                output.data = ex;
            }

            await WriteJsonResponseAsync(context, ex.GetStatusCode(), output);
            return output;
        }

        public static async Task<ResultViewModel> HandleGeneralExceptionAsync(HttpContext context, Exception ex)
        {
            var output = new ResultViewModel()
            {
                message = "خطایی در عملیات رخ داده است (درصورت اطمینان از صحت داده های خود و تکرار مجدد با پشتیبانی تماس حاصل نمایید)",
                status = false,
                statusCode = 503,
                data = null
            };

            var environment = context.RequestServices.GetService<IWebHostEnvironment>();
            if (environment != null && environment.IsDevelopment())
            {
                output.data = ex;
            }

            // تعیین کد وضعیت بر اساس نوع استثنا
            //int statusCode = ex switch
            //{
            //    ArgumentNullException => 400, // Bad Request
            //    UnauthorizedAccessException => 401, // Unauthorized
            //    KeyNotFoundException => 404, // Not Found
            //    InvalidOperationException => 405, // Method Not Allowed
            //    _ => 503 // Service Unavailable
            //};
            int statusCode = 503;
            await WriteJsonResponseAsync(context, statusCode, output);
            return output;
        }

        private static async Task WriteJsonResponseAsync(HttpContext context, int statusCode, ResultViewModel output)
        {

            string jsonString = JsonConvert.SerializeObject(output);
            byte[] byteArray = Encoding.UTF8.GetBytes(jsonString);

            context.Response.ContentType = "application/json"; // تنظیم نوع محتوا
            context.Response.StatusCode = statusCode; // تنظیم کد وضعیت

            await context.Response.Body.WriteAsync(byteArray, 0, byteArray.Length, CancellationToken.None);
        }
        public static async Task<string> GetRequestBodyAsync(HttpContext context)
        {
            var request = context.Request;

            string requestBody = string.Empty;
            if (request.Body.CanSeek)
            {
                request.Body.Position = 0;
                using var reader = new StreamReader(request.Body);
                requestBody = await reader.ReadToEndAsync();
                request.Body.Position = 0;
            }
            return requestBody;
        }

        public static string GetFormData(HttpContext context)
        {
            if (context.Request.HasFormContentType)
            {
                var form = context.Request.Form;
                var formData = new Dictionary<string, string>();

                foreach (var key in form.Keys)
                {
                    formData[key] = form[key];
                }

                return JsonConvert.SerializeObject(formData);
            }

            return string.Empty; // اگر فرم دیتا وجود نداشت
        }

    }
}
