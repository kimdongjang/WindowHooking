using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Client
{
    public delegate int HookProc(int nCode, int wParam, IntPtr lParam); // 델리게이트 선언해서 wparam과 lparam의 값을 받아오겠다.

    public class MouseHooker
    {

        static public bool MouseDown_Block = false;
        static public bool MouseUp_Block = false;

        #region 마우스이벤트

        private static event MouseEventHandler s_MouseMove;
        public static event MouseEventHandler MouseMove
        {
            add
            {
                EnsureSubscribedToGlobalMouseEvents();
                s_MouseMove += value;
            }
            remove
            {
                s_MouseMove -= value;
                TryUnsubscribeFromGlobalMouseEvents();
            }

        }

        private static event EventHandler<MouseEventExtArgs> s_MouseMoveExt;

        private static event EventHandler<MouseEventExtArgs> MouseMoveExt
        {
            add
            {
                EnsureSubscribedToGlobalMouseEvents();
                s_MouseMoveExt += value;
            }

            remove
            {

                s_MouseMoveExt -= value;
                TryUnsubscribeFromGlobalMouseEvents();
            }
        }

        private static event MouseEventHandler s_MouseClick;

        /// <summary>
        /// Occurs when a click was performed by the mouse. 
        /// </summary>
        public static event MouseEventHandler MouseClick
        {
            add
            {
                EnsureSubscribedToGlobalMouseEvents();
                s_MouseClick += value;
            }
            remove
            {
                s_MouseClick -= value;
                TryUnsubscribeFromGlobalMouseEvents();
            }
        }

        private static event EventHandler<MouseEventExtArgs> s_MouseClickExt;

        /// <summary>
        /// Occurs when a click was performed by the mouse. 
        /// </summary>
        /// <remarks>
        /// This event provides extended arguments of type <see cref="MouseEventArgs"/> enabling you to 
        /// supress further processing of mouse click in other applications.
        /// </remarks>
        private static event EventHandler<MouseEventExtArgs> MouseClickExt
        {
            add
            {
                EnsureSubscribedToGlobalMouseEvents();
                s_MouseClickExt += value;
            }
            remove
            {
                s_MouseClickExt -= value;
                TryUnsubscribeFromGlobalMouseEvents();
            }
        }

        private static event MouseEventHandler s_MouseDown;

        /// <summary>
        /// Occurs when the mouse a mouse button is pressed. 
        /// </summary>
        public static event MouseEventHandler MouseDown
        {
            add
            {
                EnsureSubscribedToGlobalMouseEvents();
                s_MouseDown += value;
            }
            remove
            {
                s_MouseDown -= value;
                TryUnsubscribeFromGlobalMouseEvents();
            }
        }

        private static event MouseEventHandler s_MouseUp;

        /// <summary>
        /// Occurs when a mouse button is released. 
        /// </summary>
        public static event MouseEventHandler MouseUp
        {
            add
            {
                EnsureSubscribedToGlobalMouseEvents();
                s_MouseUp += value;
            }
            remove
            {
                s_MouseUp -= value;
                TryUnsubscribeFromGlobalMouseEvents();
            }
        }

        private static event MouseEventHandler s_MouseWheel;

        /// <summary>
        /// Occurs when the mouse wheel moves. 
        /// </summary>
        public static event MouseEventHandler MouseWheel
        {
            add
            {
                EnsureSubscribedToGlobalMouseEvents();
                s_MouseWheel += value;
            }
            remove
            {
                s_MouseWheel -= value;
                TryUnsubscribeFromGlobalMouseEvents();
            }
        }


        private static event MouseEventHandler s_MouseDoubleClick;

        //The double click event will not be provided directly from hook.
        //To fire the double click event wee need to monitor mouse up event and when it occures 
        //Two times during the time interval which is defined in Windows as a doble click time
        //we fire this event.

        /// <summary>
        /// Occurs when a double clicked was performed by the mouse. 
        /// </summary>
        public static event MouseEventHandler MouseDoubleClick
        {
            add
            {
                EnsureSubscribedToGlobalMouseEvents();
                if (s_MouseDoubleClick == null)
                {
                    //We create a timer to monitor interval between two clicks
                    s_DoubleClickTimer = new Timer
                    {
                        //This interval will be set to the value we retrive from windows. This is a windows setting from contro planel.
                        Interval = ImportFunctions.GetDoubleClickTime(),
                        //We do not start timer yet. It will be start when the click occures.
                        Enabled = false
                    };
                    //We define the callback function for the timer
                    s_DoubleClickTimer.Tick += DoubleClickTimeElapsed;
                    //We start to monitor mouse up event.
                    MouseUp += OnMouseUp;
                }
                s_MouseDoubleClick += value;
            }
            remove
            {
                if (s_MouseDoubleClick != null)
                {
                    s_MouseDoubleClick -= value;
                    if (s_MouseDoubleClick == null)
                    {
                        //Stop monitoring mouse up
                        MouseUp -= OnMouseUp;
                        //Dispose the timer
                        s_DoubleClickTimer.Tick -= DoubleClickTimeElapsed;
                        s_DoubleClickTimer = null;
                    }
                }
                TryUnsubscribeFromGlobalMouseEvents();
            }
        }

        //This field remembers mouse button pressed because in addition to the short interval it must be also the same button.
        private static MouseButtons s_PrevClickedButton;
        //The timer to monitor time interval between two clicks.
        private static Timer s_DoubleClickTimer;

        private static void DoubleClickTimeElapsed(object sender, EventArgs e)
        {
            //Timer is alapsed and no second click ocuured
            s_DoubleClickTimer.Enabled = false;
            s_PrevClickedButton = MouseButtons.None;
        }

        /// <summary>
        /// This method is designed to monitor mouse clicks in order to fire a double click event if interval between 
        /// clicks was short enaugh.
        /// </summary>
        /// <param name="sender">Is always null</param>
        /// <param name="e">Some information about click heppened.</param>
        private static void OnMouseUp(object sender, MouseEventArgs e)
        {
            //This should not heppen
            if (e.Clicks < 1) { return; }
            //If the secon click heppened on the same button
            if (e.Button.Equals(s_PrevClickedButton))
            {
                if (s_MouseDoubleClick != null)
                {
                    //Fire double click
                    s_MouseDoubleClick.Invoke(null, e);
                }
                //Stop timer
                s_DoubleClickTimer.Enabled = false;
                s_PrevClickedButton = MouseButtons.None;
            }
            else
            {
                //If it was the firts click start the timer
                s_DoubleClickTimer.Enabled = true;
                s_PrevClickedButton = e.Button;
            }
        }
        #endregion

        private static void EnsureSubscribedToGlobalMouseEvents()
        {
            // install Mouse hook only if it is not installed and must be installed
            // 필요한 경우에만 마우스 훅을 설치
            if (s_MouseHookHandle == 0)
            {
                //See comment of this field. To avoid GC to clean it up.
                s_MouseDelegate = MouseHookProc; // 마우스훅 proc에서 받아온 값을 대리자에 전달하고
                //install hook

                //s_MouseHookHandle = SetWindowsHookEx(
                //    WH_MOUSE_LL,
                //    s_MouseDelegate,
                //    Marshal.GetHINSTANCE(
                //        Assembly.GetExecutingAssembly().GetModules()[0]),
                //    0);

                var mar = ImportFunctions.LoadLibrary("user32.dll");    //FrameWork 4.0 and up, Using this is alternative 
                s_MouseHookHandle = ImportFunctions.SetWindowsHookEx(
                    Constants.WH_MOUSE_LL,
                    s_MouseDelegate,
                    mar,
                    0);

                //If SetWindowsHookEx fails.
                if (s_MouseHookHandle == 0)
                {
                    //Returns the error code returned by the last unmanaged function called using platform invoke that has the DllImportAttribute.SetLastError flag set. 
                    int errorCode = Marshal.GetLastWin32Error();
                    //do cleanup

                    //Initializes and throws a new instance of the Win32Exception class with the specified error. 
                    throw new Win32Exception(errorCode);
                }
            }
        }

        private static void TryUnsubscribeFromGlobalMouseEvents()
        {
            //if no subsribers are registered unsubsribe from hook
            if (s_MouseClick == null &&
                s_MouseDown == null &&
                s_MouseMove == null &&
                s_MouseUp == null &&
                s_MouseClickExt == null &&
                s_MouseMoveExt == null &&
                s_MouseWheel == null)
            {
                ForceUnsunscribeFromGlobalMouseEvents();
            }
        }
        private static void ForceUnsunscribeFromGlobalMouseEvents()
        {
            if (s_MouseHookHandle != 0)
            {
                //uninstall hook
                int result = ImportFunctions.UnhookWindowsHookEx(s_MouseHookHandle);
                //reset invalid handle
                s_MouseHookHandle = 0;
                //Free up for GC
                s_MouseDelegate = null;
                //if failed and exception must be thrown
                if (result == 0)
                {
                    //Returns the error code returned by the last unmanaged function called using platform invoke that has the DllImportAttribute.SetLastError flag set. 
                    int errorCode = Marshal.GetLastWin32Error();
                    //Initializes and throws a new instance of the Win32Exception class with the specified error. 
                    throw new Win32Exception(errorCode);
                }
            }
        }

        #region Mouse hook processing

        /// <summary>
        /// This field is not objectively needed but we need to keep a reference on a delegate which will be 
        /// passed to unmanaged code. To avoid GC to clean it up.
        /// When passing delegates to unmanaged code, they must be kept alive by the managed application 
        /// until it is guaranteed that they will never be called.
        /// </summary>
        private static HookProc s_MouseDelegate; // 대리자 객체

        /// <summary>
        /// Stores the handle to the mouse hook procedure.
        /// </summary>
        private static int s_MouseHookHandle;

        private static int m_OldX;
        private static int m_OldY;

        /// <summary>
        /// A callback function which will be called every Time a mouse activity detected.
        /// </summary>
        /// <param name="nCode">
        /// [in] Specifies whether the hook procedure must process the message. 
        /// If nCode is HC_ACTION, the hook procedure must process the message. 
        /// If nCode is less than zero, the hook procedure must pass the message to the 
        /// CallNextHookEx function without further processing and must return the 
        /// value returned by CallNextHookEx.
        /// </param>
        /// <param name="wParam">
        /// [in] Specifies whether the message was sent by the current thread. 
        /// If the message was sent by the current thread, it is nonzero; otherwise, it is zero. 
        /// </param>
        /// <param name="lParam">
        /// [in] Pointer to a CWPSTRUCT structure that contains details about the message. 
        /// </param>
        /// <returns>
        /// If nCode is less than zero, the hook procedure must return the value returned by CallNextHookEx. 
        /// If nCode is greater than or equal to zero, it is highly recommended that you call CallNextHookEx 
        /// and return the value it returns; otherwise, other applications that have installed WH_CALLWNDPROC 
        /// hooks will not receive hook notifications and may behave incorrectly as a result. If the hook 
        /// procedure does not call CallNextHookEx, the return value should be zero. 
        /// </returns>
        private static int MouseHookProc(int nCode, int wParam, IntPtr lParam) // wparam lparam ncode받아서 대리자 객체에 넘겨줌
        {
            if (nCode >= 0)
            {
                //Marshall the data from callback.
                Structs.MouseLLHookStruct mouseHookStruct = (Structs.MouseLLHookStruct)Marshal.PtrToStructure(lParam, typeof(Structs.MouseLLHookStruct)); // 관련된 마우스 이벤트를 후킹해서 가져옴

                //detect button clicked 
                MouseButtons button = MouseButtons.None;
                short mouseDelta = 0;
                int clickCount = 0;
                bool mouseDown = false;
                bool mouseUp = false;

                switch (wParam)
                {
                    case Constants.WM_LBUTTONDOWN:
                        mouseDown = true;
                        button = MouseButtons.Left;
                        clickCount = 1;
                        break;
                    case Constants.WM_LBUTTONUP:
                        mouseUp = true;
                        button = MouseButtons.Left;
                        clickCount = 1;
                        break;
                    case Constants.WM_LBUTTONDBLCLK:
                        button = MouseButtons.Left;
                        clickCount = 2;
                        break;
                    case Constants.WM_RBUTTONDOWN:
                        mouseDown = true;
                        button = MouseButtons.Right;
                        clickCount = 1;
                        break;
                    case Constants.WM_RBUTTONUP:
                        mouseUp = true;
                        button = MouseButtons.Right;
                        clickCount = 1;
                        break;
                    case Constants.WM_RBUTTONDBLCLK:
                        button = MouseButtons.Right;
                        clickCount = 2;
                        break;
                    case Constants.WM_MOUSEWHEEL:
                        //If the message is WM_MOUSEWHEEL, the high-order word of MouseData member is the wheel delta. 
                        //One wheel click is defined as WHEEL_DELTA, which is 120. 
                        //(value >> 16) & 0xffff; retrieves the high-order word from the given 32-bit value
                        mouseDelta = (short)((mouseHookStruct.MouseData >> 16) & 0xffff);

                        //TODO: X BUTTONS (I havent them so was unable to test)
                        //If the message is WM_XBUTTONDOWN, WM_XBUTTONUP, WM_XBUTTONDBLCLK, WM_NCXBUTTONDOWN, WM_NCXBUTTONUP, 
                        //or WM_NCXBUTTONDBLCLK, the high-order word specifies which X button was pressed or released, 
                        //and the low-order word is reserved. This value can be one or more of the following values. 
                        //Otherwise, MouseData is not used. 
                        break;
                }

                //generate event 
                MouseEventExtArgs e = new MouseEventExtArgs(
                                                   button,
                                                   clickCount,
                                                   mouseHookStruct.Point.X,
                                                   mouseHookStruct.Point.Y,
                                                   mouseDelta);

                //Mouse up
                if (s_MouseUp != null && mouseUp)
                {
                    s_MouseUp.Invoke(null, e); // 아무것도 들어갈 필요가 없어서 null 호출만 시키면 되기 때문에
                    if (MouseUp_Block)
                        return s_MouseHookHandle;
                }

                //Mouse down
                if (s_MouseDown != null && mouseDown) // 마우스가 클릭된 상황에 MouseEventHandler의 대리자 객체로 선언한 s_MouseDown(마우스클릭성공)의 이벤트를 발생시키기 위해서 s_MouseDown.Invoke(null,e)를 선언
                {
                    s_MouseDown.Invoke(null, e);
                    if (MouseDown_Block)
                        return s_MouseHookHandle;
                }

                //If someone listens to click and a click is heppened
                if (s_MouseClick != null && clickCount > 0)
                {
                    s_MouseClick.Invoke(null, e);
                }

                //If someone listens to click and a click is heppened
                if (s_MouseClickExt != null && clickCount > 0)
                {
                    s_MouseClickExt.Invoke(null, e);
                }

                //If someone listens to double click and a click is heppened
                if (s_MouseDoubleClick != null && clickCount == 2)
                {
                    s_MouseDoubleClick.Invoke(null, e);
                }

                //Wheel was moved
                if (s_MouseWheel != null && mouseDelta != 0)
                {
                    s_MouseWheel.Invoke(null, e);
                }

                //만약 누군가가 마우스를 이동시켜 좌표가 변경되게 되면 move이벤트를 발생시킨다.
                if ((s_MouseMove != null || s_MouseMoveExt != null) && (m_OldX != mouseHookStruct.Point.X || m_OldY != mouseHookStruct.Point.Y))
                {
                    m_OldX = mouseHookStruct.Point.X;
                    m_OldY = mouseHookStruct.Point.Y;
                    if (s_MouseMove != null)
                    {
                        s_MouseMove.Invoke(null, e);
                    }

                    if (s_MouseMoveExt != null)
                    {
                        s_MouseMoveExt.Invoke(null, e);
                    }
                }

                if (e.Handled)
                {
                    return -1;
                }
            }

            //call next hook
            return ImportFunctions.CallNextHookEx(s_MouseHookHandle, nCode, wParam, lParam);
        }
        #endregion
        

       


    }
}
