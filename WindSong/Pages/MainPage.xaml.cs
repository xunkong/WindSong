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


    public bool IsAdmin { get; } = AdminHelper.IsAdmin();

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




    public void Navigate(Type sourcePageType, object? param = null, NavigationTransitionInfo? infoOverride = null)
    {
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
        try
        {
            AdminHelper.RestartAsAdmin();
            Application.Current.Exit();
        }
        catch (Exception ex)
        {

        }
    }




    #region Playlist



    [ObservableProperty]
    private ObservableCollection<MidiFolder> playlist = new();


    private void LoadMidiPlaylist()
    {
        var midiFolder = Path.Combine(AppContext.BaseDirectory, "midi");
        if (!Directory.Exists(midiFolder))
        {
            return;
        }
        var files = Directory.GetFiles(midiFolder, "*.mid", SearchOption.TopDirectoryOnly);
        var collection = new MidiFolder() { FolderName = "#" };
        foreach (var file in files)
        {
            try
            {
                collection.Add(MidiReader.ReadFile(file));
            }
            catch { }
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
                catch { }
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



    public bool ShowMidiTip => !(Playlist?.Any() ?? false);



    private void Button_Play_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement ele)
        {
            if (ele.DataContext is MidiFileInfo info)
            {
                ListView_Playlist.SelectedItem = info;
                MainPage.Current.MidiPlayer.ChangeMidiFileInfo(info);
                MainPage.Current.MidiPlayer.Play();
            }
        }
    }


    private void Grid_MidiFileInfo_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
    {
        if (sender is FrameworkElement ele)
        {
            if (ele.DataContext is MidiFileInfo info)
            {
                MainPage.Current.MidiPlayer.ChangeMidiFileInfo(info);
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

        }
    }


    [RelayCommand]
    private void LocateCurrentPlayingMidi()
    {
        var playing = MainPage.Current.MidiPlayer.MidiFileInfo;
        if (playing != null)
        {
            ListView_Playlist.SelectedItem = playing;
            ListView_Playlist.ScrollIntoView(playing, ScrollIntoViewAlignment.Leading);
        }
    }




    #endregion




    #region Search


    private void SearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
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


    private void SearchBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
    {
        if (args.SelectedItem is MidiFileInfo info)
        {
            _midiPlayer.ChangeMidiFileInfo(info);
            ListView_Playlist.SelectedItem = info;
            ListView_Playlist.ScrollIntoView(info, ScrollIntoViewAlignment.Leading);
        }
    }




    #endregion




    #region Player Control

    public MidiPlayer MidiPlayer => _midiPlayer;

    private readonly MidiPlayer _midiPlayer = new();

    private readonly DispatcherTimer _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(30) };

    private readonly FontIcon PlayIcon = new FontIcon { FontFamily = new FontFamily("Segoe Fluent Icons"), Glyph = "\uE102" };

    private readonly FontIcon PauseIcon = new FontIcon { FontFamily = new FontFamily("Segoe Fluent Icons"), Glyph = "\uE103" };


    private void InitializePlayerControl()
    {
        _timer.Tick += (_, _) => UpdatePlayerControl();
        _midiPlayer.MidiFileChanged += (_, e) => DispatcherQueue.TryEnqueue(() =>
        {
            if (e != null)
            {
                MidiFileName = e.FileName;
                MidiCurrentMilliseconds = 0;
                MidiTotalMilliseconds = MicroToMilli(e.TotalMicroseconds);
                Slider_PlayerControl.Value = 0;
                AppSetting.SelectMidi = Path.GetRelativePath(AppContext.BaseDirectory, e.FilePath);
            }
        });
        _midiPlayer.PlayStateChanged += (_, e) => DispatcherQueue.TryEnqueue(() =>
        {
            if (e == MidiPlayState.Start)
            {
                _timer.Start();
                Button_PlayOrPause.Content = PauseIcon;
            }
            else
            {
                _timer.Stop();
                Button_PlayOrPause.Content = PlayIcon;
                if (e == MidiPlayState.Stop)
                {
                    MidiCurrentMilliseconds = MidiTotalMilliseconds;
                    Slider_PlayerControl.Value = MidiTotalMilliseconds;
                }
            }
        });
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



    [ObservableProperty]
    private string midiFileName;


    [ObservableProperty]
    private int midiCurrentMilliseconds;


    [ObservableProperty]
    private int midiTotalMilliseconds;


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
    private void Previous()
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
        PlayOrPause();
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
    private void Next()
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
        PlayOrPause();
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



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int MicroToMilli(long value)
    {
        return (int)(value / 1000);
    }














    #endregion


}
