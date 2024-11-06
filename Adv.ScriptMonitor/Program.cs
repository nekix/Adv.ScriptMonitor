using Adv.ScriptMonitor.Final.Services.DomainScriptReportService;
using Adv.ScriptMonitor.Repositories;
using Adv.ScriptMonitor.Services.DomainScriptReportService;
using Adv.ScriptMonitor.Services.HtmlFetcher;
using Adv.ScriptMonitor.Services.ScriptAvailabilityService;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Quartz;
using System.Reflection.Metadata;

namespace Adv.ScriptMonitor
{
    internal class Program
    {
        static void Main(string[] args)
        {
            CreateHostBuilder(args)
                .Build()
                .Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) => ConfigureServices(services, args))
                .ConfigureLogging((context, logging) =>
                {
                    logging.ClearProviders();

                    // Для примера не настроено, но необходимо
                    // настроить как минимум для логирования исключений.
                    //logging.AddConsole();
                })
                .UseConsoleLifetime();

        private static void ConfigureServices(IServiceCollection services, string[] args)
        {
            services.ConfigureHttpClientDefaults(opt => 
                opt.ConfigureHttpClient(c => c.Timeout = TimeSpan.FromSeconds(15)));

            // Использование Quartz для управления переодической задачей
            // проверки доменов.
            ConfigureQuartz(services, args);

            // Управление потоком ввода для получения доменов.
            services.AddHostedService<DomainStdInBackgroundService>();

            services.AddSingleton<IDomainStatusRepository, DomainStatusRepository>();

            // Сервисы, используемые для проверки доменов.
            services.AddSingleton<IHtmlFetcher, DefaultHtmlFetcher>();
            services.AddSingleton<IScriptAvailabilityService, NaiveScriptAvailabilityService>();

            // Форматирование и вывод результатов проверки.
            services.AddSingleton<IDomainStatusReportService, StdOutDomainScriptReportService>();
        }

        private static void ConfigureQuartz(IServiceCollection services, string[] args)
        {
            TimeSpan checkScriptJobInterval = GetIntervalFromArgs(args);

            services.AddQuartz(q =>
            {
                q.SchedulerId = "domain-script-scheduler";

                // Настройка задачи проверки доменов.
                var jobKey = new JobKey("domain-script-monitor");
                q.AddJob<DomainsScriptCheckJob>(opt => opt.WithIdentity(jobKey));

                q.AddTrigger(opt => opt
                    .ForJob(jobKey)
                    .WithIdentity("domain-script-monitor-trigger-interval")
                    .StartNow()
                    .WithSimpleSchedule(x => x
                        .WithIntervalInSeconds(checkScriptJobInterval.Seconds)
                        .RepeatForever()));
            });

            services.AddQuartzHostedService(options =>
            {
                options.WaitForJobsToComplete = true;
            });
        }

        private static TimeSpan GetIntervalFromArgs(string[] args)
        {
            if (args.Length == 2 && args[0] == "-i" && int.TryParse(args[1], out int seconds))
            {
                return TimeSpan.FromSeconds(seconds);
            }

            throw new ArgumentException("The required interval parameter '-i' is missing or invalid.", nameof(args));
        }
    }
}
