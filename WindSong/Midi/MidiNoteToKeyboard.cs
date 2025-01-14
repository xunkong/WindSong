﻿using System.Collections.Generic;
using System.ComponentModel;
using Vanara.PInvoke;

namespace WindSong.Midi;

public abstract class MidiNoteToKeyboard
{

    private static InstrumentType instrumentType = InstrumentType.WindsongLyre;

    public static InstrumentType InstrumentType { get => instrumentType; set { instrumentType = value; Logger.Info($"Change instrument type: {value}"); } }

    private static KeyboardType keyboardType = KeyboardType.QWERTY;
    public static KeyboardType Keyboard { get => keyboardType; set { keyboardType = value; Logger.Info($"Change keyboard type: {value}"); } }


    public static User32.VK GetVirtualKey(int note)
    {
        return (InstrumentType, Keyboard) switch
        {
            (InstrumentType.WindsongLyre or InstrumentType.FloralZither, KeyboardType.QWERTY) => Windsong_QWERTY.GetValueOrDefault(note),
            (InstrumentType.VintageLyre, KeyboardType.QWERTY) => Vintage_QWERTY.GetValueOrDefault(note),
            _ => Windsong_QWERTY.GetValueOrDefault(note),
        };
    }



    public static bool IsHit(int note, InstrumentType type)
    {
        return type switch
        {
            InstrumentType.WindsongLyre => Windsong_QWERTY.ContainsKey(note),
            InstrumentType.VintageLyre => Vintage_QWERTY.ContainsKey(note),
            _ => Windsong_QWERTY.ContainsKey(note),
        };
    }




    private static Dictionary<int, User32.VK> Windsong_QWERTY = new Dictionary<int, User32.VK>
    {
        [48] = User32.VK.VK_Z, // C3
        [50] = User32.VK.VK_X, // D3
        [52] = User32.VK.VK_C, // E3
        [53] = User32.VK.VK_V, // F3
        [55] = User32.VK.VK_B, // G3
        [57] = User32.VK.VK_N, // A3
        [59] = User32.VK.VK_M, // B3

        [60] = User32.VK.VK_A, // C4
        [62] = User32.VK.VK_S, // D4
        [64] = User32.VK.VK_D, // E4
        [65] = User32.VK.VK_F, // F4
        [67] = User32.VK.VK_G, // G4
        [69] = User32.VK.VK_H, // A4
        [71] = User32.VK.VK_J, // B4

        [72] = User32.VK.VK_Q, // C5
        [74] = User32.VK.VK_W, // D5
        [76] = User32.VK.VK_E, // E5
        [77] = User32.VK.VK_R, // F5
        [79] = User32.VK.VK_T, // G5
        [81] = User32.VK.VK_Y, // A5
        [83] = User32.VK.VK_U, // B5
    };




    private static Dictionary<int, User32.VK> Vintage_QWERTY = new Dictionary<int, User32.VK>
    {
        [48] = User32.VK.VK_Z, // C3
        [50] = User32.VK.VK_X, // D3
        [51] = User32.VK.VK_C, // Eb3
        [53] = User32.VK.VK_V, // F3
        [55] = User32.VK.VK_B, // G3
        [57] = User32.VK.VK_N, // A3
        [58] = User32.VK.VK_M, // Bb3

        [60] = User32.VK.VK_A, // C4
        [62] = User32.VK.VK_S, // D4
        [63] = User32.VK.VK_D, // Eb4
        [65] = User32.VK.VK_F, // F4
        [67] = User32.VK.VK_G, // G4
        [69] = User32.VK.VK_H, // A4
        [70] = User32.VK.VK_J, // Bb4

        [72] = User32.VK.VK_Q, // C5
        [73] = User32.VK.VK_W, // Db5
        [75] = User32.VK.VK_E, // Eb5
        [77] = User32.VK.VK_R, // F5
        [79] = User32.VK.VK_T, // G5
        [80] = User32.VK.VK_Y, // Ab5
        [82] = User32.VK.VK_U, // Bb5
    };



}


public enum InstrumentType
{

    [Description("Windsong Lyre")]
    WindsongLyre,

    [Description("Floral Zither")]
    FloralZither,

    [Description("Vintage Lyre")]
    VintageLyre,

}



public enum KeyboardType
{
    QWERTY,
}