using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Vanara.PInvoke;
using WindSong.Helpers;

namespace WindSong.Midi;

public class MidiPlayer
{


    private IntPtr hwnd;

    private Stopwatch sw;

    private long swLastMicroseconds;

    private CancellationTokenSource tokenSource;

    private bool isAdmin = AdminHelper.IsAdmin();


    public MidiFileInfo? MidiFileInfo { get; private set; }

    public bool IsPlaying { get; private set; }

    public long CurrentMicroseconds { get; private set; }

    public long TotalMicroseconds { get; private set; }


    public event EventHandler<MidiFileInfo?> MidiFileChanged;


    public event EventHandler<MidiPlayState> PlayStateChanged;



    public void LoadMidiFile(string path)
    {
        var info = MidiReader.ReadFile(path);
        ChangeMidiFileInfo(info);
    }


    public void ChangeMidiFileInfo(MidiFileInfo midiFileInfo)
    {
        Pause();
        MidiFileInfo = midiFileInfo;
        CurrentMicroseconds = 0;
        TotalMicroseconds = MidiFileInfo.TotalMicroseconds;
        MidiFileChanged?.Invoke(this, MidiFileInfo);
    }


    public void Play()
    {
        if (isAdmin)
        {
            hwnd = GameHelper.GetGameHwnd();
        }
        else
        {
            hwnd = 0;
        }
        if (MidiFileInfo?.Notes?.Any() ?? false)
        {
            tokenSource = new CancellationTokenSource();
            PlayStateChanged?.Invoke(this, MidiPlayState.Start);
            new Thread(async () => await InternelPlayMidi(tokenSource.Token)).Start();
        }
    }




    public void Pause()
    {
        tokenSource?.Cancel();
        timeEndPeriod(1);
        IsPlaying = false;
        PlayStateChanged?.Invoke(this, MidiPlayState.Pause);
    }



    public void ChangeCurrentMilliseconds(int ms)
    {
        var isPlaying = IsPlaying;
        tokenSource?.Cancel();
        CurrentMicroseconds = Math.Clamp(ms * 1000, 0, TotalMicroseconds);
        if (isPlaying)
        {
            tokenSource = new CancellationTokenSource();
            new Thread(async () => await InternelPlayMidi(tokenSource.Token)).Start();
        }
    }



    private async Task InternelPlayMidi(CancellationToken token)
    {
        IsPlaying = true;
        timeBeginPeriod(1);
        var notes = MidiFileInfo!.Notes;
        int i = 0;
        if (CurrentMicroseconds > 0)
        {
            i = notes.FindIndex(x => x.AbsoluteMicrosecond > CurrentMicroseconds);
            if (i == -1)
            {
                i = notes.Count;
            }
        }
        RestartStopwatcher();
        for (; i < notes.Count; i++)
        {
            var note = notes[i];
            while (CurrentMicroseconds < note.AbsoluteMicrosecond)
            {
                var t = (int)((note.AbsoluteMicrosecond - CurrentMicroseconds) / 1000);
                if (t > 2)
                {
                    await Task.Delay(Math.Min(t, 16) - 1);
                }
                else
                {
                    Thread.SpinWait(1000);
                }
                CurrentMicroseconds += GetDeltaTime();
                if (token.IsCancellationRequested)
                {
                    return;
                }
            }
            PostMessage(note.NoteNumber);
        }
        if (CurrentMicroseconds < TotalMicroseconds)
        {
            await Task.Delay((int)((TotalMicroseconds - CurrentMicroseconds) / 1000));
            CurrentMicroseconds = TotalMicroseconds;
        }
        timeEndPeriod(1);
        IsPlaying = false;
        PlayStateChanged?.Invoke(this, MidiPlayState.Stop);
    }



    private void PostMessage(int note)
    {
        if (isAdmin && hwnd > 0)
        {
            var key = (int)MidiNoteToKeyboard.GetVirtualKey(note);
            if (key > 0)
            {
                User32.PostMessage(hwnd, User32.WindowMessage.WM_ACTIVATE, 1, 0);
                User32.PostMessage(hwnd, User32.WindowMessage.WM_KEYDOWN, key, 0x1e0001);
                User32.PostMessage(hwnd, User32.WindowMessage.WM_CHAR, key, 0x1e0001);
                User32.PostMessage(hwnd, User32.WindowMessage.WM_KEYUP, key, unchecked((nint)0xc01e0001));
            }
        }
    }



    private void RestartStopwatcher()
    {
        sw?.Stop();
        swLastMicroseconds = 0;
        sw = Stopwatch.StartNew();
    }


    private long GetDeltaTime()
    {
        var microseconds = sw.ElapsedTicks / 10;
        var delta = microseconds - swLastMicroseconds;
        swLastMicroseconds = microseconds;
        return delta;
    }




    [DllImport("Winmm.dll")]
    private static extern int timeBeginPeriod(int period);


    [DllImport("Winmm.dll")]
    private static extern int timeEndPeriod(int period);


}


public enum MidiPlayState
{
    None,

    Start,

    Pause,

    Stop,

    Change,
}