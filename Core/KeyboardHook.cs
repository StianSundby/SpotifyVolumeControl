using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace SpotifyVolumeControl
{
    //allow the callback to block the keypress from reaching Windows
    public delegate void keyPressedHandler(Keys key, ref bool handled);

    public partial class KeyboardHook : IDisposable
    {
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_SYSKEYDOWN = 0x0104;

        private readonly lowLevelKeyboardProc _process; //keep a reference so the garbage collector doesn't collect the delegate while the hook is alive. Why is this a thing
        private readonly IntPtr _hookId = IntPtr.Zero;

        public event keyPressedHandler? KeyPressed;

        public KeyboardHook()
        {
            _process = HookCallback;
            _hookId = SetHook(_process);
        }

        private static IntPtr SetHook(lowLevelKeyboardProc proc)
        {
            using var currentProcess = Process.GetCurrentProcess();
            using var currentModule = currentProcess.MainModule!;
            return SetWindowsHook(WH_KEYBOARD_LL, proc, GetModuleHandle(currentModule.ModuleName!), 0);
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && (wParam == WM_KEYDOWN || wParam == WM_SYSKEYDOWN))
            {
                var key = (Keys)Marshal.ReadInt32(lParam);
                bool handled = false;

                KeyPressed?.Invoke(key, ref handled);

                if (handled)
                    return (IntPtr)1; //returning 1 swallows the keypress
            }

            return CallNextHook(_hookId, nCode, wParam, lParam);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
                UnhookWindowsHook(_hookId);
        }

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate IntPtr lowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        [LibraryImport("user32.dll", EntryPoint = "SetWindowsHookExW")]
        private static partial IntPtr SetWindowsHook(int idHook, lowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [LibraryImport("user32.dll", EntryPoint = "UnhookWindowsHookEx")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool UnhookWindowsHook(IntPtr hhk);

        [LibraryImport("user32.dll", EntryPoint = "CallNextHookEx")]
        private static partial IntPtr CallNextHook(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [LibraryImport("kernel32.dll", EntryPoint = "GetModuleHandleW", StringMarshalling = StringMarshalling.Utf16)]
        private static partial IntPtr GetModuleHandle(string lpModuleName);
    }
}
