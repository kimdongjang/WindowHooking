using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using DWORD = System.Int32;

namespace Client
{
    public static class ImportFunctions
    {
        #region Windows function imports
        [DllImport("user32.dll")]
        public static extern short GetKeyState(int keyCode);

        [DllImport("user32.dll", CharSet = CharSet.Auto,
            CallingConvention = CallingConvention.StdCall)]
        public static extern int CallNextHookEx(
            int idHook,
            int nCode,
            int wParam,
            IntPtr lParam);


        [DllImport("user32.dll", CharSet = CharSet.Auto,
            CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        public static  extern int SetWindowsHookEx(
            int idHook,
            HookProc lpfn,
            IntPtr hMod,
            int dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto,
            CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        public static extern int UnhookWindowsHookEx(int idHook);

        [DllImport("user32")]
        public static extern int GetDoubleClickTime();

      
        [DllImport("user32")]
        public static extern int ToAscii(
            int uVirtKey,
            int uScanCode,
            byte[] lpbKeyState,
            byte[] lpwTransKey,
            int fuState);

        [DllImport("user32")]
        public static extern int GetKeyboardState(byte[] pbKeyState);

        [DllImport("user32.dll")]
        public static extern int SetCursorPos(int x, int y);

        //[DllImport("user32.dll")]
        //public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);

        [DllImport("user32.dll")]
        public static extern void mouse_event(DWORD dwFlags, DWORD dx, DWORD dy, DWORD cButtons, uint dwExtraInfo);


        //[DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        //public static extern short GetKeyState(int vKey);

        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPStr)]string lpFileName);
        #endregion

        #region DLL imports

        /// <summary>
        /// Sets the windows hook, do the desired event, one of hInstance or threadId must be non-null
        /// </summary>
        /// <param name="idHook">The id of the event you want to hook</param>
        /// <param name="callback">The callback.</param>
        /// <param name="hInstance">The handle you want to attach the event to, can be null</param>
        /// <param name="threadId">The thread you want to attach the event to, can be null</param>
        /// <returns>a handle to the desired hook</returns>
        [DllImport("user32.dll")]
        public static extern IntPtr SetWindowsHookEx(int idHook, globalKeyboardHook.keyboardHookProc callback, IntPtr hInstance, uint threadId);

        /// <summary>
        /// Unhooks the windows hook.
        /// </summary>
        /// <param name="hInstance">The hook handle that was returned from SetWindowsHookEx</param>
        /// <returns>True if successful, false otherwise</returns>
        [DllImport("user32.dll")]
        public static extern bool UnhookWindowsHookEx(IntPtr hInstance);

        /// <summary>
        /// Calls the next hook.
        /// </summary>
        /// <param name="idHook">The hook id</param>
        /// <param name="nCode">The hook code</param>
        /// <param name="wParam">The wparam.</param>
        /// <param name="lParam">The lparam.</param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        public static extern int CallNextHookEx(IntPtr idHook, int nCode, int wParam, ref globalKeyboardHook.keyboardHookStruct lParam);

        /// <summary>
        /// Loads the library.
        /// </summary>
        /// <param name="lpFileName">Name of the library</param>
        /// <returns>A handle to the library</returns>
        //[DllImport("kernel32.dll")]
        //public static extern IntPtr LoadLibrary(string lpFileName);

        /*
       BYTE bVk,       // 가상 키코드
       BYTE bScan,     // 하드웨어 스캔 코드
       DWORD dwFlags,  // 동작 지정 Flag
       PTR dwExtraInfo // 추가 정보
       */
        [DllImport("user32.dll")]
        public static extern uint keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);
        #endregion

        #region NumLock, CapsLock, ScrollLock state DLL

        //[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true, CallingConvention = CallingConvention.Winapi)]
        //public static extern short GetKeyState(int keyCode);

        //[Flags]
        //public enum KeyStates : byte
        //{
        //    None = (byte)0,
        //    Down = (byte)1,
        //    Toggled = (byte)2,
        //}
        #endregion

        #region KanaLock state DLL

        //[DllImport("imm32.dll")]
        //public static extern IntPtr ImmGetDefaultIMEWnd(IntPtr hWnd);

        //// Declare external functions.
        //[DllImport("user32.dll")]
        //public static extern IntPtr GetForegroundWindow();

        //[DllImport("user32.dll")]
        //public static extern int GetWindowText(IntPtr hWnd,
        //                                        StringBuilder text,
        //                                        int count);
        //[DllImport("user32.dll", CharSet = CharSet.Auto)]
        //public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, uint wParam, ref COPYDATASTRUCT lParam);

        //public struct COPYDATASTRUCT
        //{
        //    public IntPtr dwData;
        //    public int cbData;
        //    [MarshalAs(UnmanagedType.LPStr)]
        //    public string lpData;
        //}

        [DllImport("imm32.dll")]
        public static extern IntPtr ImmGetDefaultIMEWnd(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr IParam);  
        #endregion

        #region Get Handle DLL
        [DllImport(@"user32.dll", CharSet = CharSet.Auto)]
        public static extern int GetForegroundWindow();
        [DllImport("user32.dll")]
        public static extern int GetWindowText(int hWnd, StringBuilder text, int count);
        #endregion

        #region Screen Capture DLL

        #region gdi32

        // http://msdn.microsoft.com/en-us/library/dd183370(VS.85).aspx
        [DllImport("gdi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool BitBlt(IntPtr hDestDC, int x, int y, int nWidth, int nHeight, IntPtr hSrcDC, int xSrc, int ySrc, Int32 dwRop);

        // http://msdn.microsoft.com/en-us/library/dd183488(VS.85).aspx
        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);

        // http://msdn.microsoft.com/en-us/library/dd183489(VS.85).aspx
        [DllImport("gdi32.dll", SetLastError = true)]
        public static extern IntPtr CreateCompatibleDC(IntPtr hdc);

        // http://msdn.microsoft.com/en-us/library/dd162957(VS.85).aspx
        [DllImport("gdi32.dll", ExactSpelling = true, PreserveSig = true, SetLastError = true)]
        public static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

        // http://msdn.microsoft.com/en-us/library/dd183539(VS.85).aspx
        [DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        #endregion

        #region user32
        [DllImport("user32.dll")]
        public static extern IntPtr GetDesktopWindow();

        // http://msdn.microsoft.com/en-us/library/dd144871(VS.85).aspx
        [DllImport("user32.dll")]
        public static extern IntPtr GetDC(IntPtr hwnd);

        // http://msdn.microsoft.com/en-us/library/dd162920(VS.85).aspx
        [DllImport("user32.dll")]
        public static extern int ReleaseDC(IntPtr hwnd, IntPtr dc);

        // Important note for Vista / Win7 on this function. In those version, rectangle returned is not 100% correct
        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hWnd, out Client.Structs.Win32Rect rect);

        [DllImport("user32.dll")]
        public static extern bool GetClientRect(IntPtr hWnd, out Client.Structs.Win32Rect rect);

        [DllImport("user32.dll")]
        public static extern bool ClientToScreen(IntPtr hWnd, ref Client.Structs.Win32Point lpPoint);

        [DllImportAttribute("user32.dll")]
        public static extern IntPtr SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern IntPtr WindowFromPoint(Client.Structs.Win32Point pt);
        #endregion

        #region DwnApi
        // http://msdn.microsoft.com/en-us/library/aa969530(VS.85).aspx
        // http://msdn.microsoft.com/en-us/library/aa969515(VS.85).aspx
        // http://msdn.microsoft.com/en-us/library/aa970874.aspx (for signature)
        [DllImport("DwmApi.dll")]
        public static extern int DwmGetWindowAttribute(
            IntPtr hwnd,
            uint dwAttributeToGet, //DWMWA_* values
            IntPtr pvAttributeValue,
            uint cbAttribute);

        public const int DWMNCRP_USEWINDOWSTYLE = 0;           // Enable/disable non-client rendering based on window style
        public const int DWMNCRP_DISABLED = 1;                 // Disabled non-client rendering; window style is ignored
        public const int DWMNCRP_ENABLED = 2;                  // Enabled non-client rendering; window style is ignored

        public const int DWMWA_NCRENDERING_ENABLED = 1;        // Enable/disable non-client rendering Use DWMNCRP_* values
        public const int DWMWA_NCRENDERING_POLICY = 2;         // Non-client rendering policy
        public const int DWMWA_TRANSITIONS_FORCEDISABLED = 3;  // Potentially enable/forcibly disable transitions 0 or 1
        #endregion

        #endregion
    }
}
