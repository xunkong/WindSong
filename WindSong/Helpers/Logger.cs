using Serilog;
using Serilog.Context;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace WindSong.Helpers;

internal abstract class Logger
{

    public static void Initialize()
    {
        try
        {
            var path = Path.Combine(AppContext.BaseDirectory, $@"log\windsong_{DateTimeOffset.Now:yyyyMMdd_HHmmss}.txt");
            Log.Logger = new LoggerConfiguration().WriteTo.File(path: path, outputTemplate: "[{Timestamp:HH:mm:ss.fff}] [{Level:u4}] {CallerMemberName} at {CallerFilePath} line {CallerLineNumber}{NewLine}{Message}{NewLine}{Exception}{NewLine}")
                                                  .Enrich.FromLogContext()
                                                  .CreateLogger();

            var sb = new StringBuilder();
            sb.Append("Wind Song");
            var exe = Process.GetCurrentProcess().MainModule?.FileName;
            if (File.Exists(exe))
            {
                sb.AppendLine($" v{FileVersionInfo.GetVersionInfo(exe).FileVersion}");
            }
            else
            {
                sb.AppendLine();
            }
            sb.AppendLine($"Admin: {AdminHelper.IsAdmin}");
            sb.AppendLine($"Time: {DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss.fff zzz}");
            sb.AppendLine($"OSVersion: {Environment.OSVersion}");
            sb.AppendLine($"CommandLine: {Environment.CommandLine}");
            Info(sb.ToString());
        }
        catch { }
    }


    public static void CloseAndFlush()
    {
        Log.CloseAndFlush();
    }



    public static void Info(string message, [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
    {
        try
        {
            using var _1 = LogContext.PushProperty("CallerMemberName", callerMemberName);
            using var _2 = LogContext.PushProperty("CallerFilePath", callerFilePath);
            using var _3 = LogContext.PushProperty("CallerLineNumber", callerLineNumber);
            Log.Information(message);
        }
        catch { }
    }




    public static void Error(Exception ex, string? message = null, [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
    {
        try
        {
            using var _1 = LogContext.PushProperty("CallerMemberName", callerMemberName);
            using var _2 = LogContext.PushProperty("CallerFilePath", callerFilePath);
            using var _3 = LogContext.PushProperty("CallerLineNumber", callerLineNumber);
            Log.Error(ex, message ?? "--->");
        }
        catch { }
    }

}
