using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    public static class Structs
    {
        #region 구조체
        [StructLayout(LayoutKind.Sequential)]
        public struct Point
        {
            public int X;
            public int Y;
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct MouseLLHookStruct
        {
            public Point Point;
            public int MouseData;
            public int Flags;
            public int Time;
            public int ExtraInfo;
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct KeyboardHookStruct
        {
            /// <summary>
            /// Specifies a virtual-key code. The code must be a value in the range 1 to 254. 
            /// </summary>
            public int VirtualKeyCode;
            /// <summary>
            /// Specifies a hardware scan code for the key. 
            /// </summary>
            public int ScanCode;
            /// <summary>
            /// Specifies the extended-key flag, event-injected flag, context code, and transition-state flag.
            /// </summary>
            public int Flags;
            /// <summary>
            /// Specifies the Time stamp for this message.
            /// </summary>
            public int Time;
            /// <summary>
            /// Specifies extra information associated with the message. 
            /// </summary>
            public int ExtraInfo;
        }
        #endregion

        #region Screen Capture 관련 구조체

        [StructLayout(LayoutKind.Sequential)]
        public struct Win32Point
        {
            public int X;
            public int Y;

            public Win32Point(int x, int y)
            {
                this.X = x;
                this.Y = y;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Win32Rect
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;


            public int Width
            {
                get { return Right - Left; }
            }

            public int Height
            {
                get { return Bottom - Top; }
            }

        }

        #endregion
    }
}
