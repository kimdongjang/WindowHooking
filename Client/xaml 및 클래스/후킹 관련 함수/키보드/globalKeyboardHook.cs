using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace Client
{
    public class globalKeyboardHook
    {
        public delegate int keyboardHookProc(int code, int wParam, ref keyboardHookStruct lParam);
        public bool IsAlt = false;

        #region 훅 구조체
        public struct keyboardHookStruct
        {
            public int vkCode;
            public int scanCode;
            public int flags;
            public int time;
            public int dwExtraInfo;
        }
        #endregion

        #region Instance Variables
        /// <summary>
        /// The collections of keys to watch for
        /// </summary>
        public List<Keys> HookedKeys = new List<Keys>();

        /// <summary>
        /// Handle to the hook, need this to unhook and call the next hook
        /// </summary>
        IntPtr hhook = IntPtr.Zero;
        #endregion

        #region 키보드 이벤트
        /// <summary>
        /// Occurs when one of the hooked keys is pressed
        /// </summary>
        public event KeyEventHandler KeyDown;
        /// <summary>
        /// Occurs when one of the hooked keys is released
        /// </summary>
        public event KeyEventHandler KeyUp;
        #endregion

        #region Constructors and Destructors
        /// <summary>
        /// Initializes a new instance of the <see cref="globalKeyboardHook"/> class and installs the keyboard hook.
        /// </summary>
        public globalKeyboardHook()
        {
            // 생성자 역할 
            // keyboardHookProc 대리자에 hookProc와 hook 함수 
            khp = new keyboardHookProc(hookProc);
            hook();
        }

        keyboardHookProc khp;

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="globalKeyboardHook"/> is reclaimed by garbage collection and uninstalls the keyboard hook.
        /// </summary>
        ~globalKeyboardHook()
        {
            unhook();
        }
        #endregion

        #region 후킹, 언후킹, 훅프로시저
        /// <summary>
        /// Installs the global hook
        /// </summary>
        public void hook()
        {
            IntPtr hInstance = ImportFunctions.LoadLibrary("User32");
            hhook = ImportFunctions.SetWindowsHookEx(Constants.WH_KEYBOARD_LL, khp, hInstance, 0);
        }

        /// <summary>
        /// Uninstalls the global hook
        /// </summary>
        public void unhook()
        {
            ImportFunctions.UnhookWindowsHookEx(hhook);
        }
        /// 키보드 입력이 일어났을 때 훅 프로시저 호출
        public int hookProc(int code, int wParam, ref keyboardHookStruct lParam)
        {
            try
            {
                if (code >= 0)
                {
                    Keys key = (Keys)lParam.vkCode;
                    if (key == Keys.None)
                        return 0;
                    if (HookedKeys.Contains(key))
                    {
                        KeyEventArgs kea = new KeyEventArgs(key);

                        if ((wParam == Constants.WM_KEYDOWN) && (KeyDown != null))
                        {
                            KeyboardHooking.Instance.KeyDown(this, kea);
                        }
                        else if ((wParam == Constants.WM_KEYUP || wParam == Constants.WM_SYSKEYUP) && (KeyUp != null))
                        {
                            if (KeyboardHooking.Instance.recording_starting && KeyboardHooking.Instance.is_startsetting)
                            {
                                KeyboardHooking.Instance.KeyUp(this, kea);
                            }
                            //KeyUp(this, kea);
                        }
                        else if (wParam == Constants.WM_SYSKEYDOWN)
                        {
                            #region 키보드로 컨트롤 하기 위한 제어문
                            if ((lParam.flags == 0x20) && (lParam.vkCode == (int)KeyboardHooking.Instance.RecordStartKey)) // 0x20은 alt
                            {
                                IsAlt = true;
                                KeyboardHooking.Instance.LControlKeyUp();
                                KeyboardHooking.Instance.RecordStartButtonEvent();
                                return 1;
                            }
                            else if ((lParam.flags == 0x20) && (lParam.vkCode == (int)KeyboardHooking.Instance.LogPlayKey)) // 0x20은 alt
                            {
                                IsAlt = true;
                                KeyboardHooking.Instance.LControlKeyUp();
                                KeyboardHooking.Instance.RecordPlayButtonEvent();
                                return 1;
                            }
                            else if ((lParam.flags == 0x20) && (lParam.vkCode == (int)KeyboardHooking.Instance.LogPlayStopKey)) // 0x20은 alt
                            {
                                IsAlt = true;
                                KeyboardHooking.Instance.LControlKeyUp();
                                KeyboardHooking.Instance.RecordPlayStopButtonEvent();
                                return 1;
                            }
                            else if ((lParam.flags == 0x20) && (lParam.vkCode == (int)KeyboardHooking.Instance.RecordStopKey)) // 0x20은 alt
                            {
                                IsAlt = true;
                                KeyboardHooking.Instance.LControlKeyUp();
                                KeyboardHooking.Instance.RecordStopButtonEvent();
                                return 1;
                            }
                            #endregion

                            else if (KeyboardHooking.Instance.recording_starting && KeyboardHooking.Instance.is_startsetting)
                            {
                                KeyboardHooking.Instance.KeyDown(this, kea);
                            }
                        }
                        else if (wParam == Constants.WM_SYSKEYUP)
                        {

                        }
                        if (kea.Handled)
                            return 1;
                    }
                }
                return ImportFunctions.CallNextHookEx(hhook, code, wParam, ref lParam);
            }
            catch
            {
                return 0;
            }
        }
        #endregion
    }
}