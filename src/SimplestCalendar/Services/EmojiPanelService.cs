using System.Runtime.InteropServices;

namespace SimplestCalendar.Services;

/// <summary>
/// Opens the Windows emoji panel (Win+.) focused on the current control.
/// </summary>
public static class EmojiPanelService
{
    private const byte VkLWin = 0x5B;
    private const byte VkOemPeriod = 0xBE; // .
    private const uint KeyeventfKeyup = 0x0002;

    [DllImport("user32.dll")]
    private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

    public static void Show()
    {
        keybd_event(VkLWin, 0, 0, UIntPtr.Zero);
        keybd_event(VkOemPeriod, 0, 0, UIntPtr.Zero);
        keybd_event(VkOemPeriod, 0, KeyeventfKeyup, UIntPtr.Zero);
        keybd_event(VkLWin, 0, KeyeventfKeyup, UIntPtr.Zero);
    }
}
