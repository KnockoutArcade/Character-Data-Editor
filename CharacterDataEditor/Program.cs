using CharacterDataEditor.Constants;
using CharacterDataEditor.Helpers;
using CharacterDataEditor.Options;
using CharacterDataEditor.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Enrichers;
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace CharacterDataEditor
{
    class Program
    {
        static int Main(string[] args)
        {
            return CommandHelper.GenerateRootCommandAndExecuteHandler(args, Run);
        }

        static int Run(ArgValues argValues, string[] args)
        {
            var startup = new Startup();

            if (string.IsNullOrEmpty(argValues.LogPath))
            {
                Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Override(LogSourceConstants.Microsoft, Serilog.Events.LogEventLevel.Information)
                    .MinimumLevel.Override(LogSourceConstants.System, Serilog.Events.LogEventLevel.Warning)
                    .MinimumLevel.Information()
                    .Enrich.With(new MachineNameEnricher())
                    .WriteTo.Console()
                    .CreateLogger();
            }
            else
            {
                Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Override(LogSourceConstants.Microsoft, Serilog.Events.LogEventLevel.Information)
                    .MinimumLevel.Override(LogSourceConstants.System, Serilog.Events.LogEventLevel.Warning)
                    .MinimumLevel.Information()
                    .Enrich.With(new MachineNameEnricher())
                    .WriteTo.Console()
                    .WriteTo.File(path: $"{argValues.LogPath}KOArcadeLog-{DateTime.Now.Year}{DateTime.Now.Month.ToString().PadLeft(2, '0')}{DateTime.Now.Day.ToString().PadLeft(2, '0')}.log")
                    .CreateLogger();
            }

            if (!argValues.EnableConsole)
            {
                #if _WINDOWS
                [DllImport("kernel32.dll")]
                static extern IntPtr GetConsoleWindow();

                [DllImport("user32.dll")]
                static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

                const int SW_HIDE = 0;
                const int SW_SHOW = 5;

                var handle = GetConsoleWindow();
                ShowWindow(handle, SW_HIDE);
                #endif
            }

            Log.Logger.Information(MessageConstants.LoggerAttachedMessage);

            var host = Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    startup.ConfigureServices(services);
                })
                .UseSerilog()
                .Build();

            var svc = ActivatorUtilities.GetServiceOrCreateInstance<IRenderUI>(host.Services);

            return svc.StartUI();
        }
    }
}