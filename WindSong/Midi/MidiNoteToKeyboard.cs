using System.Collections.Generic;
using Vanara.PInvoke;

namespace WindSong.Midi;

public abstract class MidiNoteToKeyboard
{

    public static InstrumentType InstrumentType { get; set; } = InstrumentType.WindsongLyre;

    public static KeyboradType KeyboradType { get; set; } = KeyboradType.QWERTY;


    public static User32.VK GetVirtualKey(int note)
    {
        return (InstrumentType, KeyboradType) switch
        {
            (InstrumentType.WindsongLyre, KeyboradType.QWERTY) => Windsong_QWERTY.GetValueOrDefault(note),
            _ => Windsong_QWERTY.GetValueOrDefault(note),
        };
    }




    private static Dictionary<int, User32.VK> Windsong_QWERTY = new Dictionary<int, User32.VK>
    {
        [48] = User32.VK.VK_Z,
        [50] = User32.VK.VK_X,
        [52] = User32.VK.VK_C,
        [53] = User32.VK.VK_V,
        [55] = User32.VK.VK_B,
        [57] = User32.VK.VK_N,
        [59] = User32.VK.VK_M,

        [60] = User32.VK.VK_A,
        [62] = User32.VK.VK_S,
        [64] = User32.VK.VK_D,
        [65] = User32.VK.VK_F,
        [67] = User32.VK.VK_G,
        [69] = User32.VK.VK_H,
        [71] = User32.VK.VK_J,

        [72] = User32.VK.VK_Q,
        [74] = User32.VK.VK_W,
        [76] = User32.VK.VK_E,
        [77] = User32.VK.VK_R,
        [79] = User32.VK.VK_T,
        [81] = User32.VK.VK_Y,
        [83] = User32.VK.VK_U,
    };
}


public enum InstrumentType
{
    WindsongLyre,

}



public enum KeyboradType
{
    QWERTY,
}