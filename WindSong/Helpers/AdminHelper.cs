using Microsoft.UI.Xaml;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Security.Principal;

namespace WindSong.Helpers;

internal abstract class AdminHelper
{


    public static bool IsAdmin { get; private set; }


    static AdminHelper()
    {
        IsAdmin = InternelIsAdmin();
    }


    private static bool InternelIsAdmin()
    {
        using WindowsIdentity identity = WindowsIdentity.GetCurrent();
        WindowsPrincipal principal = new WindowsPrincipal(identity);
        return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }


    /// <summary>
    /// No exception
    /// </summary>
    public static void RestartAsAdmin()
    {
        try
        {
            Logger.Info("Restart as admin.");
            var file = Process.GetCurrentProcess().MainModule?.FileName;
            if (string.IsNullOrWhiteSpace(file))
            {
                file = Path.Join(AppContext.BaseDirectory, "WindSong.exe");
            }
            Process.Start(new ProcessStartInfo
            {
                FileName = file,
                UseShellExecute = true,
                Verb = "runas",
            });
            Logger.Info("Exit the application.");
            Application.Current.Exit();
        }
        catch (Win32Exception ex) when (ex.NativeErrorCode == 1223)
        {
            // ERROR_CANCELLED
            // The operation was canceled by the user.
            Logger.Info("Restart as admin operation cancelled.");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error in restart as admin.");
        }
    }



}
