// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using WindSong.Messages;
using WindSong.Midi;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WindSong.Pages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
[INotifyPropertyChanged]
public sealed partial class PlaylistPage : Page
{

    public PlaylistPage()
    {
        this.InitializeComponent();
    }



    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        WeakReferenceMessenger.Default.Register<ChoseSearchResultMessage>(this, (_, m) =>
        {
            ListView_Playlist.SelectedItem = m.MidiFileInfo;
            ListView_Playlist.ScrollIntoView(m.MidiFileInfo, ScrollIntoViewAlignment.Leading);
        });
    }



    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        base.OnNavigatedFrom(e);
        WeakReferenceMessenger.Default.Unregister<ChoseSearchResultMessage>(this);
    }



    public ObservableCollection<MidiFolder> Playlist => MainPage.Current.Playlist;


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









}
