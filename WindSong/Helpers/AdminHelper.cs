using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Security.Principal;

namespace WindSong.Helpers;

internal abstract class AdminHelper
{


    public static bool IsAdmin()
    {
        using WindowsIdentity identity = WindowsIdentity.GetCurrent();
        WindowsPrincipal principal = new WindowsPrincipal(identity);
        return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }



    public static void RestartAsAdmin()
    {
        try
        {
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
        }
        catch (Win32Exception ex)
        {
            if (ex.NativeErrorCode == 1223)
            {
                // ERROR_CANCELLED
                // The operation was canceled by the user.
            }
            else
            {
                throw;
            }
        }
    }



}
