// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Graphics;
using WindSong.Helpers;
using WindSong.Midi;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WindSong.Pages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
[INotifyPropertyChanged]
public sealed partial class MainPage : Page
{


    public static MainPage Current { get; private set; }


    public bool IsAdmin { get; } = AdminHelper.IsAdmin;

    public string TitleText => IsAdmin ? "Wind Song - Admin" : "Wind Song";

    public bool AdminButtonEnable => !IsAdmin;


    public string AdminButtonText => IsAdmin ? "Administrator Mode" : "Restart as Administrator";


    public MainPage()
    {
        Current = this;
        this.InitializeComponent();
        InitializePlayerControl();
        LoadMidiPlaylist();
    }



    private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        var scale = MainWindow.Current.UIScale;
        var point = SearchBox.TransformToVisual(this).TransformPoint(new Point());
        var left = point.X;
        var right = left + SearchBox.ActualWidth;
        MainWindow.Current.SetDragRectangles(new RectInt32((int)(48 * scale), 0, (int)((left - 48) * scale), (int)(48 * scale)),
                                             new RectInt32((int)(right * scale), 0, (int)((this.ActualWidth - right) * scale), (int)(48 * scale)),
                                             new RectInt32((int)(left * scale), 0, (int)(SearchBox.ActualWidth * scale), (int)(8 * scale)));
    }




    public void Navigate(Type? sourcePageType, object? param = null, NavigationTransitionInfo? infoOverride = null)
    {
        if (sourcePageType is null)
        {
            MainPageFrame.Content = null;
            return;
        }
        if (param is null)
        {
            MainPageFrame.Navigate(sourcePageType);
        }
        else if (infoOverride is null)
        {
            MainPageFrame.Navigate(sourcePageType, param);
        }
        else
        {
            MainPageFrame.Navigate(sourcePageType, param, infoOverride);
        }
    }


    [RelayCommand]
    private void RestartAsAdmin()
    {
        if (IsAdmin)
        {
            return;
        }
        AdminHelper.RestartAsAdmin();
    }




    #region Playlist



    [ObservableProperty]
    private ObservableCollection<MidiFolder> playlist = new();


    private void LoadMidiPlaylist()
    {
        var midiFolder = Path.Combine(AppContext.BaseDirectory, "midi");
        if (Directory.Exists(midiFolder))
        {
            var files = Directory.GetFiles(midiFolder, "*.mid", SearchOption.TopDirectoryOnly);
            var collection = new MidiFolder() { FolderName = "#" };
            foreach (var file in files)
            {
                try
                {
                    collection.Add(MidiReader.ReadFile(file));
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, $"Cannot read midi file '{file}'.");
                }
            }
            if (collection.Any())
            {
                Playlist.Add(collection);
            }
            var dirs = Directory.GetDirectories(midiFolder);
            foreach (var dir in dirs)
            {
                var col = new MidiFolder() { FolderName = Path.GetFileName(dir)! };
                foreach (var file in Directory.GetFiles(dir, "*.mid", SearchOption.AllDirectories))
                {
                    try
                    {
                        col.Add(MidiReader.ReadFile(file));
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex, $"Cannot read midi file '{file}'.");
                    }
                }
                Playlist.Add(col);
            }
            var lastMidi = AppSetting.SelectMidi;
            if (File.Exists(lastMidi))
            {
                var path = Path.GetFullPath(lastMidi);
                var midi = Playlist.SelectMany(x => x).FirstOrDefault(x => x.FilePath == path);
                if (midi != null)
                {
                    _midiPlayer.ChangeMidiFileInfo(midi);
                }
            }
            else
            {
                var midi = Playlist.SelectMany(x => x).FirstOrDefault();
                if (midi != null)
                {
                    _midiPlayer.ChangeMidiFileInfo(midi);
                }
            }
        }
        if (!(Playlist?.Any() ?? false))
        {
            StackPanel_MidiTooltip.Visibility = Visibility.Visible;
            Logger.Info("Cannot find any midi files.");
        }
    }




    private void Button_Play_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement ele)
        {
            if (ele.DataContext is MidiFileInfo info)
            {
                try
                {
                    ListView_Playlist.SelectedItem = info;
                    MidiPlayer.ChangeMidiFileInfo(info);
                    MidiPlayer.Play();
                }
                catch { }
            }
        }
    }


    private void Grid_MidiFileInfo_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
    {
        if (sender is FrameworkElement ele)
        {
            if (ele.DataContext is MidiFileInfo info)
            {
                try
                {
                    MidiPlayer.ChangeMidiFileInfo(info);
                }
                catch { }
            }
        }
    }


    [RelayCommand]
    private void OpenMidiFolder()
    {
        try
        {
            var folder = Path.Join(AppContext.BaseDirectory, "midi");
            Directory.CreateDirectory(folder);
            Process.Start(new ProcessStartInfo
            {
                FileName = folder,
                UseShellExecute = true,
            });
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Open midi folder.");
        }
    }


    [RelayCommand]
    private void LocateCurrentPlayingMidi()
    {
        try
        {
            var playing = MidiPlayer.MidiFileInfo;
            if (playing != null)
            {
                ListView_Playlist.SelectedItem = playing;
                ListView_Playlist.ScrollIntoView(playing, ScrollIntoViewAlignment.Leading);
            }
        }
        catch { }
    }



    private void Grid_Playlist_DragOver(object sender, DragEventArgs e)
    {
        e.AcceptedOperation = DataPackageOperation.Copy;
    }

    private async void Grid_Playlist_Drop(object sender, DragEventArgs e)
    {
        try
        {
            if (e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                var items = await e.DataView.GetStorageItemsAsync();
                if (items.Count > 0)
                {
                    var files = items.Where(x => x.Name.ToLower().EndsWith(".mid") || x.Name.ToLower().EndsWith(".midi")).ToList();
                    Logger.Info($"Drag into {files.Count} midi files.");
                    if (files.Count > 0)
                    {
                        var midiFolder = Path.Combine(AppContext.BaseDirectory, "midi");
                        Directory.CreateDirectory(midiFolder);
                        var list = Playlist.FirstOrDefault();
                        if (list is null)
                        {
                            list = new MidiFolder { FolderName = "#" };
                            Playlist.Add(list);
                        }
                        foreach (var file in files)
                        {
                            try
                            {
                                var midi = MidiReader.ReadFile(file.Path);
                                File.Copy(file.Path, Path.Combine(midiFolder, file.Name), true);
                                list.Add(midi);
                            }
                            catch (Exception ex)
                            {
                                Logger.Error(ex, $"Cannot read midi file '{file}'.");
                            }
                        }
                        if (list.Any())
                        {
                            StackPanel_MidiTooltip.Visibility = Visibility.Collapsed;
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error in drag midi files into playlist.");
        }

    }



    #endregion




    #region Search


    private void SearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        try
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                var patternMidis = new List<MidiFileInfo>();
                var text = sender.Text.Trim();
                if (string.IsNullOrWhiteSpace(text))
                {
                    sender.ItemsSource = null;
                    sender.IsSuggestionListOpen = false;
                    return;
                }
                foreach (var midi in Playlist.SelectMany(x => x))
                {
                    if (midi.FileName.Contains(text, StringComparison.CurrentCultureIgnoreCase))
                    {
                        patternMidis.Add(midi);
                    }
                }
                if (patternMidis.Count == 0)
                {
                    sender.ItemsSource = null;
                    sender.IsSuggestionListOpen = false;
                }
                else
                {
                    sender.ItemsSource = patternMidis;
                }
            }
        }
        catch { }
    }


    private void SearchBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
    {
        try
        {
            if (args.SelectedItem is MidiFileInfo info)
            {
                _midiPlayer.ChangeMidiFileInfo(info);
                ListView_Playlist.SelectedItem = info;
                ListView_Playlist.ScrollIntoView(info, ScrollIntoViewAlignment.Leading);
            }
        }
        catch { }
    }




    #endregion




    #region Player Control

    public MidiPlayer MidiPlayer => _midiPlayer;

    private readonly MidiPlayer _midiPlayer = new();

    private readonly DispatcherTimer _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(30) };

    private readonly FontIcon PlayIcon = new FontIcon { FontFamily = new FontFamily("Segoe Fluent Icons"), Glyph = "\uE102" };
    private readonly FontIcon PauseIcon = new FontIcon { FontFamily = new FontFamily("Segoe Fluent Icons"), Glyph = "\uE103" };

    private readonly FontIcon RepeatOffIcon = new FontIcon { FontFamily = new FontFamily("Segoe Fluent Icons"), Glyph = "\uF5E7" };
    private readonly FontIcon RepeatOneIcon = new FontIcon { FontFamily = new FontFamily("Segoe Fluent Icons"), Glyph = "\uE1CC" };
    private readonly FontIcon RepeatAllIcon = new FontIcon { FontFamily = new FontFamily("Segoe Fluent Icons"), Glyph = "\uE1CD" };


    private void InitializePlayerControl()
    {
        _timer.Tick += (_, _) => UpdatePlayerControl();
        _midiPlayer.MidiFileChanged += (_, e) => DispatcherQueue.TryEnqueue(() =>
        {
            if (e != null)
            {
                Logger.Info($"Midi file changed: {e.FileName}");
                MidiFileName = e.FileName;
                MidiCurrentMilliseconds = 0;
                MidiTotalMilliseconds = MicroToMilli(e.TotalMicroseconds);
                Slider_PlayerControl.Value = 0;
                AppSetting.SelectMidi = Path.GetRelativePath(AppContext.BaseDirectory, e.FilePath);
            }
        });
        _midiPlayer.PlayStateChanged += (_, e) => DispatcherQueue.TryEnqueue(() =>
        {
            switch (e)
            {
                case MidiPlayState.None:
                    break;
                case MidiPlayState.Start:
                    _timer.Start();
                    Button_PlayOrPause.Content = PauseIcon;
                    break;
                case MidiPlayState.Pause:
                    _timer.Stop();
                    Button_PlayOrPause.Content = PlayIcon;
                    break;
                case MidiPlayState.Stop:
                    _timer.Stop();
                    Button_PlayOrPause.Content = PlayIcon;
                    MidiCurrentMilliseconds = MidiTotalMilliseconds;
                    Slider_PlayerControl.Value = MidiTotalMilliseconds;
                    switch (RepeatMode)
                    {
                        case RepeatMode.RepeatOff:
                            break;
                        case RepeatMode.RepeatOne:
                            MidiCurrentMilliseconds = 0;
                            Slider_PlayerControl.Value = 0;
                            _midiPlayer.ChangeCurrentMilliseconds(0);
                            PlayOrPause();
                            break;
                        case RepeatMode.RepeatAll:
                            PlayNext();
                            break;
                        default:
                            break;
                    }
                    break;
                case MidiPlayState.Change:
                    _timer.Stop();
                    Button_PlayOrPause.Content = PlayIcon;
                    break;
                default:
                    break;
            }
        });
        InitializeInstrument();
        InitializeRepeatMode();
    }



    private void InitializeInstrument()
    {
        MidiNoteToKeyboard.InstrumentType = AppSetting.SelectInstrument;
        switch (MidiNoteToKeyboard.InstrumentType)
        {
            case InstrumentType.WindsongLyre:
                RadioMenuFlyoutItem_Windsong.IsChecked = true;
                break;
            case InstrumentType.FloralZither:
                RadioMenuFlyoutItem_Floral.IsChecked = true;
                break;
            case InstrumentType.VintageLyre:
                RadioMenuFlyoutItem_Vintage.IsChecked = true;
                break;
            default:
                MidiNoteToKeyboard.InstrumentType = InstrumentType.WindsongLyre;
                RadioMenuFlyoutItem_Windsong.IsChecked = true;
                break;
        }
    }


    private void InitializeRepeatMode()
    {
        repeatMode = AppSetting.RepeatMode;
        switch (RepeatMode)
        {
            case RepeatMode.RepeatOff:
                Button_RepeatMode.Content = RepeatOffIcon;
                break;
            case RepeatMode.RepeatOne:
                Button_RepeatMode.Content = RepeatOneIcon;
                break;
            case RepeatMode.RepeatAll:
                Button_RepeatMode.Content = RepeatAllIcon;
                break;
            default:
                RepeatMode = RepeatMode.RepeatOff;
                Button_RepeatMode.Content = RepeatOffIcon;
                break;
        }
    }






    [ObservableProperty]
    private string midiFileName;


    [ObservableProperty]
    private int midiCurrentMilliseconds;


    [ObservableProperty]
    private int midiTotalMilliseconds;



    private RepeatMode repeatMode;

    public RepeatMode RepeatMode
    {
        get => repeatMode;
        set
        {
            repeatMode = value;
            AppSetting.RepeatMode = value;
        }
    }


    public bool PlayRandom
    {
        get => AppSetting.PlayRandom;
        set => AppSetting.PlayRandom = value;
    }


    public bool Topmost
    {
        get => MainWindow.Current.TopMost;
        set => MainWindow.Current.TopMost = value;
    }


    private bool isPressed = false;

    private void UpdatePlayerControl()
    {
        MidiCurrentMilliseconds = MicroToMilli(_midiPlayer.CurrentMicroseconds);
        MidiTotalMilliseconds = MicroToMilli(_midiPlayer.TotalMicroseconds);
        if (!isPressed)
        {
            Slider_PlayerControl.Value = MidiCurrentMilliseconds;
        }
    }


    private void Slider_PlayerControl_ManipulationStarting(object sender, ManipulationStartingRoutedEventArgs e)
    {
        isPressed = true;
    }

    private void Slider_PlayerControl_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
    {
        isPressed = true;
    }


    private void Slider_PlayerControl_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
    {
        _midiPlayer.ChangeCurrentMilliseconds((int)Slider_PlayerControl.Value);
        MidiCurrentMilliseconds = MicroToMilli(_midiPlayer.CurrentMicroseconds);
        isPressed = false;
    }


    private void Slider_PlayerControl_Tapped(object sender, TappedRoutedEventArgs e)
    {
        var p = e.GetPosition(Slider_PlayerControl);
        var value = MidiTotalMilliseconds * p.X / Slider_PlayerControl.ActualWidth;
        _midiPlayer.ChangeCurrentMilliseconds((int)value);
        MidiCurrentMilliseconds = MicroToMilli(_midiPlayer.CurrentMicroseconds);
        isPressed = false;
    }


    [RelayCommand]
    private void PlayPrevious()
    {
        try
        {
            if (PlayRandom)
            {
                var list = Playlist.SelectMany(x => x).OrderBy(x => Random.Shared.Next()).ToList();
                var midi = list.FirstOrDefault();
                if (midi != null)
                {
                    _midiPlayer.ChangeMidiFileInfo(midi);
                }
            }
            else
            {
                var list = Playlist.SelectMany(x => x).ToList();
                if (list.Any())
                {
                    MidiFileInfo? midi = null;
                    if (_midiPlayer.MidiFileInfo is null)
                    {
                        midi = list.Last();
                    }
                    else
                    {
                        var index = list.FindIndex(x => x == _midiPlayer.MidiFileInfo);
                        if (index <= 0)
                        {
                            midi = list.Last();
                        }
                        else
                        {
                            midi = list[index - 1];
                        }
                    }
                    _midiPlayer.ChangeMidiFileInfo(midi);
                }
            }
            PlayOrPause();
        }
        catch { }
    }



    [RelayCommand]
    private void PlayOrPause()
    {
        if (_midiPlayer.IsPlaying)
        {
            _midiPlayer.Pause();
        }
        else
        {
            _midiPlayer.Play();
        }
    }



    [RelayCommand]
    private void PlayNext()
    {
        try
        {
            if (PlayRandom)
            {
                var list = Playlist.SelectMany(x => x).OrderBy(x => Random.Shared.Next()).ToList();
                var midi = list.FirstOrDefault();
                if (midi != null)
                {
                    _midiPlayer.ChangeMidiFileInfo(midi);
                }
            }
            else
            {
                var list = Playlist.SelectMany(x => x).ToList();
                if (list.Any())
                {
                    MidiFileInfo? midi = null;
                    if (_midiPlayer.MidiFileInfo is null)
                    {
                        midi = list.First();
                    }
                    else
                    {
                        var index = list.FindIndex(x => x == _midiPlayer.MidiFileInfo);
                        if (index >= list.Count - 1)
                        {
                            midi = list.First();
                        }
                        else
                        {
                            midi = list[index + 1];
                        }
                    }
                    _midiPlayer.ChangeMidiFileInfo(midi);
                }
            }
            PlayOrPause();
        }
        catch { }
    }



    private void RadioMenuFlyoutItem_ChangeInstrument_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement ele)
        {
            if (ele.Tag is "Windsong")
            {
                MidiNoteToKeyboard.InstrumentType = InstrumentType.WindsongLyre;
            }
            if (ele.Tag is "Floral")
            {
                MidiNoteToKeyboard.InstrumentType = InstrumentType.FloralZither;
            }
            if (ele.Tag is "Vintage")
            {
                MidiNoteToKeyboard.InstrumentType = InstrumentType.VintageLyre;
            }
            AppSetting.SelectInstrument = MidiNoteToKeyboard.InstrumentType;
        }
    }


    private void RadioMenuFlyoutItem_RepeatMode_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement ele)
        {
            if (ele.Tag is "RepeatOff")
            {
                RepeatMode = RepeatMode.RepeatOff;
                Button_RepeatMode.Content = RepeatOffIcon;
            }
            if (ele.Tag is "RepeatOne")
            {
                RepeatMode = RepeatMode.RepeatOne;
                Button_RepeatMode.Content = RepeatOneIcon;
            }
            if (ele.Tag is "RepeatAll")
            {
                RepeatMode = RepeatMode.RepeatAll;
                Button_RepeatMode.Content = RepeatAllIcon;
            }
        }
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int MicroToMilli(long value)
    {
        return (int)(value / 1000);
    }







    #endregion


}
