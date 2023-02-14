using NAudio.Midi;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WindSong.Midi;

public class MidiReader
{


    public static MidiFileInfo ReadFile(string filePath, bool strictChecking = true)
    {
        var midi = new MidiFile(filePath, strictChecking);
        var info = new MidiFileInfo();
        info.FilePath = filePath;
        info.FileName = Path.GetFileNameWithoutExtension(filePath);
        info.TrackCount = midi.Tracks;
        if (midi.FileFormat == 1)
        {
            var tpqn = midi.DeltaTicksPerQuarterNote;
            var tempos = midi.Events[0].Where(x => x is TempoEvent).Select(x => x as TempoEvent).ToList();
            var notes = new List<MidiNote>();
            int track = 0;
            long maxMicroseconds = 0;
            foreach (var events in midi.Events.Skip(1))
            {
                track++;
                var tps = new Queue<TempoEvent>(tempos!);
                long currentMicroseconds = 0;
                int microsecondsPerTick = tps.Dequeue().MicrosecondsPerQuarterNote / tpqn;
                long nextTempoTicks = 0;

                if (tps.TryPeek(out var tempo))
                {
                    nextTempoTicks = tempo.AbsoluteTime;
                }
                else
                {
                    nextTempoTicks = long.MaxValue;
                }

                for (int i = 0; i < events.Count; i++)
                {
                    var noteEvent = events[i];
                    if (noteEvent.AbsoluteTime > nextTempoTicks)
                    {
                        if (tps.TryDequeue(out var tempo1))
                        {
                            microsecondsPerTick = tempo1.MicrosecondsPerQuarterNote / tpqn;
                            if (tps.TryPeek(out var tempo2))
                            {
                                nextTempoTicks = tempo2.AbsoluteTime;
                            }
                            else
                            {
                                nextTempoTicks = long.MaxValue;
                            }
                        }
                        else
                        {
                            nextTempoTicks = long.MaxValue;
                        }
                    }
                    currentMicroseconds += noteEvent.DeltaTime * microsecondsPerTick;
                    if (noteEvent is NoteOnEvent noteOn)
                    {
                        notes.Add(new MidiNote { AbsoluteMicrosecond = currentMicroseconds, Channel = noteOn.Channel, Track = track, NoteNumber = noteOn.NoteNumber });
                    }
                }
                maxMicroseconds = Math.Max(maxMicroseconds, currentMicroseconds);
            }
            info.Notes = notes.OrderBy(x => x.AbsoluteMicrosecond).ThenBy(x => x.Track).ToList();
            info.NoteCount = info.Notes.Count;
            info.TotalMicroseconds = maxMicroseconds;
        }
        return info;
    }


}