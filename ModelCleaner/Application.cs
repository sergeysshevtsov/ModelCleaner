using ModelCleaner.Commands;
using Nice3point.Revit.Toolkit.External;
using Serilog;
using Serilog.Events;

namespace ModelCleaner;
[UsedImplicitly]
public class Application : ExternalApplication
{
    public override void OnStartup()
    {
        CreateLogger();
        CreateRibbon();
    }

    public override void OnShutdown()
    {
        Log.CloseAndFlush();
    }

    private void CreateRibbon()
    {
        var panel = Application.CreatePanel("Commands", "SHSS Tools");

        panel.AddPushButton<CmdModelCleaner>("Execute")
            .SetImage("/ModelCleaner;component/Resources/Icons/RevitCleaner16.png")
            .SetLargeImage("/ModelCleaner;component/Resources/Icons/RevitCleaner32.png");
    }

    private static void CreateLogger()
    {
        const string outputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}";

        Log.Logger = new LoggerConfiguration()
            .WriteTo.Debug(LogEventLevel.Debug, outputTemplate)
            .MinimumLevel.Debug()
            .CreateLogger();

        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
        {
            var exception = (Exception)args.ExceptionObject;
            Log.Fatal(exception, "Domain unhandled exception");
        };
    }
}