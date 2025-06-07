using Microsoft.AspNetCore.Http;
using Serilog;
using Serilog.Settings.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomationEngine.CustomMiddlewares.Configuration
{
    public static class SerilogConfig
    {
        public static void AddApplicationLogging(this ConfigureHostBuilder host, IConfiguration configuration)
        {
            host.UseSerilog((context, configuration) => {
                configuration
                    .ReadFrom.Configuration(context.Configuration)
                    .Enrich.FromLogContext()
                    .WriteTo.Console();
            });
            Log.Information("Application Starting...");
        }
    }
}
