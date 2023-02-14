using System.Collections.Generic;

namespace WindSong.Midi;

public class MidiFileInfo
{

    public string FileName { get; set; }

    public string FilePath { get; set; }

    public int TrackCount { get; set; }

    public int NoteCount { get; set; }

    public List<MidiNote> Notes { get; set; }

    public long TotalMicroseconds { get; set; }

}
