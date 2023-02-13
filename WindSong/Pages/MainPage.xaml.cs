// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics;
using WindSong.Helpers;

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


    public MainPage()
    {
        Current = this;
        this.InitializeComponent();
        _timer.Tick += (_, _) => UpdatePlayerControl();
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





    #region Player Control


    private readonly DispatcherTimer _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(30) };


    [ObservableProperty]
    private string midiFileName;


    [ObservableProperty]
    private int midiCurrentSeconds;


    [ObservableProperty]
    private int midiTotalSeconds;



    private void UpdatePlayerControl()
    {

    }



    [RelayCommand]
    private void Previous()
    {

    }



    [RelayCommand]
    private void PlayOrPause()
    {

    }



    [RelayCommand]
    private void Next()
    {

    }




    #endregion





}
