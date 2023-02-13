using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using Vanara.PInvoke;
using WinRT.Interop;

namespace WindSong.Helpers;

public class HotkeyManager : IDisposable
{

    private readonly Window _window;

    private readonly IntPtr hwnd;

    private readonly User32.WindowProc preProc;

    private readonly User32.WindowProc newProc;

    private readonly Dictionary<int, HotkeyId> hotkeys;


    public HotkeyManager(Window window)
    {
        _window = window;
        hotkeys = new();
        hwnd = WindowNative.GetWindowHandle(window);
        var pPreProc = User32.GetWindowLongPtr(hwnd, User32.WindowLongFlags.GWLP_WNDPROC);
        preProc = Marshal.GetDelegateForFunctionPointer<User32.WindowProc>(pPreProc);
        newProc = new(WindowProc);
        var pNewProc = Marshal.GetFunctionPointerForDelegate(newProc);
        SetWindowLongPtr(hwnd, User32.WindowLongFlags.GWLP_WNDPROC, pNewProc);
    }




    private IntPtr WindowProc(HWND hwnd, uint uMsg, IntPtr wParam, IntPtr lParam)
    {
        if (uMsg == (uint)User32.WindowMessage.WM_HOTKEY)
        {
            var mod = (User32.HotKeyModifiers)(lParam & 0xFFFF);
            var key = (User32.VK)(lParam >> 16);
            Debug.WriteLine($"[Recieved Hotkey] Mod: {mod}, Key: {key}");
            if (hotkeys.FirstOrDefault(x => x.Value.Modifiers == mod && x.Value.VK == key).Value is HotkeyId hotkeyId)
            {
                try
                {
                    hotkeyId.Action?.Invoke();
                }
                catch { }
            }
            return IntPtr.Zero;
        }
        else
        {
            return User32.CallWindowProc(preProc, hwnd, uMsg, wParam, lParam);
        }
    }


    [DllImport("user32.dll")]
    private static extern nint SetWindowLongPtr(nint hwnd, User32.WindowLongFlags nIndex, nint dwNewLong);



    public bool RegisterHotkey(int id, User32.HotKeyModifiers hotKeyModifiers, User32.VK vK, Action? action = null)
    {
        if (hotkeys.ContainsKey(id))
        {
            UnregisterHotkey(id);
        }
        hotkeys.Add(id, new HotkeyId { Id = id, Modifiers = hotKeyModifiers, VK = vK, Action = action });
        return User32.RegisterHotKey(hwnd, id, hotKeyModifiers, (uint)vK);
    }


    public bool UnregisterHotkey(int id)
    {
        if (hotkeys.ContainsKey(id))
        {
            hotkeys.Remove(id);
        }
        return User32.UnregisterHotKey(hwnd, id);
    }



    ~HotkeyManager()
    {
        Dispose(disposing: false);
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    private bool disposedValue;

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {

            }

            foreach (var id in hotkeys.Keys)
            {
                User32.UnregisterHotKey(hwnd, id);
            }
            hotkeys.Clear();

            disposedValue = true;
        }
    }



    private struct HotkeyId
    {
        public int Id { get; set; }

        public User32.HotKeyModifiers Modifiers { get; set; }

        public User32.VK VK { get; set; }

        public Action? Action { get; set; }
    }



}
