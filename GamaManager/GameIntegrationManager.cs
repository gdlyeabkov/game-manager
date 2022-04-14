using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using System.Windows.Interop;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Win32.SafeHandles;
using System.Windows;

namespace GamaManager
{
    internal class GameIntegrationManager : HwndHost
    {

        private const int GWL_STYLE = -16;
        private const int WS_CAPTION = 0x00C00000;
        private const int WS_THICKFRAME = 0x00040000;
        const int WS_CHILD = 0x40000000;
        public GameWindow window;
        public string gamePath;
        public Process _process;
        public IntPtr parentHwnd;

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32")]
        private static extern IntPtr SetParent(IntPtr hWnd, IntPtr hWndParent);

        [DllImport("user32.dll", EntryPoint = "DestroyWindow", CharSet = CharSet.Unicode)]
        internal static extern bool DestroyWindow(IntPtr hwnd);

        public GameIntegrationManager(GameWindow window, string gamePath)
        {
            this.window = window;
            this.gamePath = gamePath;
        }

        protected override HandleRef BuildWindowCore(HandleRef hwndParent)
        {
            // ProcessStartInfo psi = new ProcessStartInfo("notepad.exe");
            ProcessStartInfo psi = new ProcessStartInfo(gamePath);
            psi.WindowStyle = ProcessWindowStyle.Maximized;
            _process = Process.Start(psi);
            _process.WaitForInputIdle();
            // The main window handle may be unavailable for a while, just wait for it
            while (_process.MainWindowHandle == IntPtr.Zero)
            {
                Thread.Yield();
            }

            
            IntPtr notepadHandle = _process.MainWindowHandle;
            
            int style = GetWindowLong(notepadHandle, GWL_STYLE);
            style = style & ~((int)WS_CAPTION) & ~((int)WS_THICKFRAME); // Removes Caption bar and the sizing border
            style |= ((int)WS_CHILD); // Must be a child window to be hosted

            SetWindowLong(notepadHandle, GWL_STYLE, style);

            SetParent(notepadHandle, hwndParent.Handle);
            
            this.InvalidateVisual();

            HandleRef hwnd = new HandleRef(window, notepadHandle);
            parentHwnd = hwndParent.Handle;
            return hwnd;
        }

        protected override void DestroyWindowCore(HandleRef hwnd)
        {
            window.CloseGame();
            DestroyWindow(parentHwnd);
            window.Close();
            _process.CloseMainWindow();

            _process.WaitForExit(5000);

            if (_process.HasExited == false)
                _process.Kill();


            _process.Close();
            _process.Dispose();
            _process = null;
            DestroyWindow(hwnd.Handle);
        }

        protected override void Dispose(bool disposing)
        {
            
        }

    }
}
