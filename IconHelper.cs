using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace SettingsManager;

public static class IconHelper {
    const int GwlExStyle = -20;
    const int WsExDlgModalFrame = 0x0001;
    const int SwpNoSize = 0x0001;
    const int SwpNoMove = 0x0002;
    const int SwpNoZOrder = 0x0004;
    const int SwpFrameChanged = 0x0020;

    [DllImport("user32.dll")]
    static extern int GetWindowLong(IntPtr hWnd, int index);

    [DllImport("user32.dll")]
    static extern int SetWindowLong(IntPtr hWnd, int index, int newStyle);

    [DllImport("user32.dll")]
    static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int width, int height, uint flags);

    public static void RemoveIcon(Window window) {
        // Get this window's handle
        IntPtr hWnd = new WindowInteropHelper(window).Handle;

        // Change the extended window style to not show a window icon
        int extendedStyle = GetWindowLong(hWnd, GwlExStyle);
        SetWindowLong(hWnd, GwlExStyle, extendedStyle | WsExDlgModalFrame);

        // Update the window's non-client area to reflect the changes
        SetWindowPos(hWnd, IntPtr.Zero, 0, 0, 0, 0, SwpNoMove | SwpNoSize | SwpNoZOrder | SwpFrameChanged);
    }
}
