using System;
using System.Diagnostics;
using System.Linq;

namespace WindSong.Helpers;

public abstract class GameHelper
{


    public static IntPtr GetGameHwnd()
    {
        var ps = Process.GetProcessesByName("YuanShen");
        if (ps.Any())
        {
            return ps[0].MainWindowHandle;
        }
        ps = Process.GetProcessesByName("GenshinImpact");
        if (ps.Any())
        {
            return ps[0].MainWindowHandle;
        }
        ps = Process.GetProcessesByName("Genshin Impact Cloud Game");
        if (ps.Any())
        {
            return ps[0].MainWindowHandle;
        }
        return IntPtr.Zero;
    }



}
