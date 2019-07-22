using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;
using TextWrangler.Configuration;
using TextWrangler.Models;

namespace TextWrangler.Extensions
{
    public static class LogManager
    {
        private static ILoggerFactory _loggerFactory;

        private static ILoggerFactory LogFactory => _loggerFactory ?? (_loggerFactory = TextWranglerConfig.LogFactory ?? GetDefaultFactory());

        public static ILogger GetLogger<T>() => TextWranglerConfig.LogFactory.CreateLogger<T>();
        public static ILogger GetLogger(string loggerName) => LogFactory.CreateLogger(loggerName);

        private static ILoggerFactory GetDefaultFactory()
        {
            var configureNamedOptions = new ConfigureNamedOptions<ConsoleLoggerOptions>("Console", null);

            var optionsFactory = new OptionsFactory<ConsoleLoggerOptions>(new[]
                                                                          {
                                                                              configureNamedOptions
                                                                          },
                                                                          Enumerable.Empty<IPostConfigureOptions<ConsoleLoggerOptions>>());

            var optionsMonitor = new OptionsMonitor<ConsoleLoggerOptions>(optionsFactory,
                                                                          Enumerable.Empty<IOptionsChangeTokenSource<ConsoleLoggerOptions>>(),
                                                                          new OptionsCache<ConsoleLoggerOptions>());

            return new LoggerFactory(new[]
                                     {
                                         new ConsoleLoggerProvider(optionsMonitor)
                                     },
                                     new LoggerFilterOptions
                                     {
                                         MinLevel = TextWranglerConfig.DefaultLogLevel
                                     });
        }

        public static void LogWranglerError(this ILogger log, TextWranglerException twx)
        {
            var msg = $"\n[{twx.GetType().Name}] error occurred - [{twx.Message}]\n";

#if DEBUG
            log.LogError(twx, msg);

            return;
#endif

            log.LogError(msg);
        }
    }
}
