// See https://aka.ms/new-console-template for more information
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using ConsoleApp.Classes;
using ConsoleApp.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Console = ConsoleApp.Classes.Console;
using Microsoft.Extensions.Configuration;
using System;
using System.Diagnostics.CodeAnalysis;

namespace ConsoleApp
{
  [SuppressMessage("ReSharper", "CoVariantArrayConversion")]
  internal class Program
  {
    private static async Task Main(string[] args)
    {
      #region Initialization
      // Configuration: Managed by Host.CreateDefaultBuilder
      #endregion

      #region Loading Configuration used before Host is built
      var environmentName = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
      var initConfiguration = new ConfigurationBuilder()
        .AddEnvironmentVariables()
        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
        .AddJsonFile($"appsettings.{environmentName}.json", true, true)
        .AddCommandLine(args)
        .Build();
      #endregion

      #region Adding Services
      // Ref: https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.hosting.host.createdefaultbuilder?view=dotnet-plat-ext-7.0
      var builder = Host.CreateDefaultBuilder(args);
      
      builder.ConfigureServices((_, services) =>
        {
          services
            .AddSingleton<ISbMessages, SbMessages>()
            .AddTransient<IConsole, Console>();

          services.AddLogging(loggingBuilder =>
          {
            loggingBuilder.AddSeq(initConfiguration.GetSection("Seq"));
            loggingBuilder.AddSimpleConsole(options =>
            {
              // Ref: https://learn.microsoft.com/en-us/dotnet/core/extensions/console-log-formatter
              options.IncludeScopes = true;
              options.SingleLine = true;
              options.TimestampFormat = "yyy-MM-dd HH:mm:ss.fff ";
            });
          });
        });
      #endregion

      #region Build Host
      var host = builder.Build();
      #endregion

      // Adding logger and logging 
      var logger = host.Services.GetRequiredService<ILogger<Program>>();
      logger.LogInformation("Main({args}) started", args);

      // Launch: either this block for a Console UI
      var console = host.Services.GetRequiredService<IConsole>();
      _ = await console.RunAsync();

      // Or this block for a Job/batch
      //await host.RunAsync();

      logger.LogInformation("Main({args}) finished", args);
    }
  }
}
