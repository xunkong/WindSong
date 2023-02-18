// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using Vanara.PInvoke;
using Windows.Graphics;
using Windows.UI;
using WindSong.Helpers;
using WindSong.Pages;
using WinRT.Interop;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WindSong;

/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MainWindow : Window
{

    public IntPtr HWND { get; private set; }

    private AppWindow _appWindow;

    private SystemBackdrop _backdrop;

    public static new MainWindow Current { get; private set; }

    public int Height => _appWindow.Size.Height;

    public int Width => _appWindow.Size.Width;

    public double UIScale => (double)User32.GetDpiForWindow(HWND) / 96;

    public XamlRoot XamlRoot => Content.XamlRoot;

    public DisplayArea DisplayArea => DisplayArea.GetFromWindowId(Win32Interop.GetWindowIdFromWindow(HWND), DisplayAreaFallback.Primary);

    public ElementTheme ActualTheme => ((FrameworkElement)Content).ActualTheme;

    public HotkeyManager HotKeyManager { get; private set; }


    public MainWindow()
    {
        Current = this;
        this.InitializeComponent();
        InitializeBackdrop();
        InitializeWindowState();
        HotKeyManager = new(this);
        Navigate(typeof(MainPage));
    }




    private void InitializeBackdrop()
    {
        _backdrop = new SystemBackdrop(this);
        if (_backdrop.TrySetMica(alwaysActive: true))
        {
            RootGrid.Background = null;
        }
    }



    private void InitializeWindowState()
    {
        HWND = WindowNative.GetWindowHandle(this);
        var windowId = Win32Interop.GetWindowIdFromWindow(HWND);
        _appWindow = AppWindow.GetFromWindowId(windowId);
        this.Title = "Wind Song";
        _appWindow.Title = this.Title;
        var scale = this.UIScale;
        if (AppWindowTitleBar.IsCustomizationSupported())
        {
            var top = (int)(48 * scale);
            var titleBar = _appWindow.TitleBar;
            titleBar.ExtendsContentIntoTitleBar = true;
            titleBar.ButtonBackgroundColor = Colors.Transparent;
            titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
            ChangeTitleBarButtonColor();
            titleBar.SetDragRectangles(new RectInt32[] { new RectInt32(0, 0, 10000, top) });
            RootGrid.ActualThemeChanged += (_, _) => ChangeTitleBarButtonColor();
        }
        if (AppSetting.IsMainWindowMaximum)
        {
            // Fix title bar cannot be dragged on Windows
            // https://github.com/microsoft/WindowsAppSDK/issues/2976
            _appWindow.ResizeClient(_appWindow.ClientSize);
            User32.ShowWindow(HWND, ShowWindowCommand.SW_MAXIMIZE);
            return;
        }
        var rect = AppSetting.MainWindowRect;
        var display = DisplayArea;
        var workAreaWidth = display.WorkArea.Width;
        var workAreaHeight = display.WorkArea.Height;
        if (rect.X > 0 && rect.Y > 0 && rect.X + rect.Width < workAreaWidth && rect.Y + rect.Height < workAreaHeight)
        {
            _appWindow.MoveAndResize(rect);
        }
        else
        {
            _appWindow.Resize(new SizeInt32((int)(600 * scale), (int)(720 * scale)));
        }
    }


    private void ChangeTitleBarButtonColor()
    {
        if (AppWindowTitleBar.IsCustomizationSupported())
        {
            var titleBar = _appWindow.TitleBar;
            switch (RootGrid.ActualTheme)
            {
                case ElementTheme.Default:
                    break;
                case ElementTheme.Light:
                    titleBar.ButtonForegroundColor = Colors.Black;
                    titleBar.ButtonHoverForegroundColor = Colors.Black;
                    titleBar.ButtonHoverBackgroundColor = Color.FromArgb(0x20, 0x00, 0x00, 0x00);
                    break;
                case ElementTheme.Dark:
                    titleBar.ButtonForegroundColor = Colors.White;
                    titleBar.ButtonHoverForegroundColor = Colors.White;
                    titleBar.ButtonHoverBackgroundColor = Color.FromArgb(0x20, 0xFF, 0xFF, 0xFF);
                    break;
                default:
                    break;
            }
        }
    }



    public void SetDragRectangles(params RectInt32[] rects)
    {
        if (AppWindowTitleBar.IsCustomizationSupported())
        {
            _appWindow.TitleBar.SetDragRectangles(rects);
        }
    }




    public void Navigate(Type sourcePageType, object? param = null, NavigationTransitionInfo? infoOverride = null)
    {
        if (param is null)
        {
            RootFrame.Navigate(sourcePageType);
        }
        else if (infoOverride is null)
        {
            RootFrame.Navigate(sourcePageType, param);
        }
        else
        {
            RootFrame.Navigate(sourcePageType, param, infoOverride);
        }
    }




    private void Window_Closed(object sender, WindowEventArgs args)
    {
        HotKeyManager.Dispose();
        SaveWindowState();
    }



    private void SaveWindowState()
    {
        var wpl = new User32.WINDOWPLACEMENT();
        if (User32.GetWindowPlacement(HWND, ref wpl))
        {
            AppSetting.IsMainWindowMaximum = wpl.showCmd == ShowWindowCommand.SW_MAXIMIZE;
            var p = _appWindow.Position;
            var s = _appWindow.Size;
            var rect = new RectInt32(p.X, p.Y, s.Width, s.Height);
            AppSetting.MainWindowRect = rect;
        }
    }


}
