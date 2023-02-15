using System.Collections.ObjectModel;

namespace WindSong.Midi;

public class MidiFolder : ObservableCollection<MidiFileInfo>
{

    public string FolderName { get; set; }

}
