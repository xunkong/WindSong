using Microsoft.Win32;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Windows.Graphics;
using WindSong.Midi;

namespace WindSong.Helpers;

internal abstract class AppSetting
{



    public static bool IsMainWindowMaximum
    {
        get => GetValue<bool>();
        set => SetValue(value);
    }



    public static RectInt32 MainWindowRect
    {
        get => new WindowRect(GetValue<ulong>()).ToRectInt32();
        set => SetValue(new WindowRect(value).Value);
    }



    public static InstrumentType SelectInstrument
    {
        get => GetValue<InstrumentType>();
        set => SetValue(value);
    }



    public static string? SelectMidi
    {
        get => GetValue<string>();
        set => SetValue(value);
    }



    public static bool Topmost
    {
        get => GetValue<bool>();
        set => SetValue(value);
    }




    [StructLayout(LayoutKind.Explicit)]
    private struct WindowRect
    {
        [FieldOffset(0)] public short X;
        [FieldOffset(2)] public short Y;
        [FieldOffset(4)] public short Width;
        [FieldOffset(6)] public short Height;
        [FieldOffset(0)] public ulong Value;

        public int Left => X;
        public int Top => Y;
        public int Right => X + Width;
        public int Bottom => Y + Height;

        public WindowRect(RectInt32 rect)
        {
            Value = 0;
            X = (short)rect.X;
            Y = (short)rect.Y;
            Width = (short)rect.Width;
            Height = (short)rect.Height;
        }

        public WindowRect(ulong value)
        {
            X = 0;
            Y = 0;
            Width = 0;
            Height = 0;
            Value = value;
        }

        public RectInt32 ToRectInt32()
        {
            return new RectInt32(X, Y, Width, Height);
        }
    }







    private const string KEY = @"HKEY_CURRENT_USER\Software\WindSong";

    private static Dictionary<string, string?> cache = new();


    public static T? GetValue<T>(T? defaultValue = default, [CallerMemberName] string? key = null)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return defaultValue;
        }
        try
        {
            if (!cache.TryGetValue(key, out var value))
            {
                value = Registry.GetValue(KEY, key, null) as string;
                cache[key] = value;
            }
            if (value is null)
            {
                return defaultValue;
            }
            var converter = TypeDescriptor.GetConverter(typeof(T));
            if (converter == null)
            {
                return defaultValue;
            }
            return (T?)converter.ConvertFromString(value);
        }
        catch
        {
            return defaultValue;
        }
    }


    public static void SetValue<T>(T value, [CallerMemberName] string? key = null)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return;
        }
        try
        {
            if (value?.ToString() is string str)
            {
                Registry.SetValue(KEY, key, str);
                cache[key] = str;
            }
        }
        catch { }
    }


}
