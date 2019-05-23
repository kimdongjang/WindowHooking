using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    public static class Constants
    {
        #region Windows constants

        public const int MOUSEEVENTF_LEFTDOWN = 0x02;
        public const int MOUSEEVENTF_LEFTUP = 0x04;
        public const int MOUSEEVENTF_RIGHTDOWN = 0x08;
        public const int MOUSEEVENTF_RIGHTUP = 0x10;

        public const int WH_MOUSE_LL = 14;

        public const int WH_MOUSE = 7;

        public const int WH_KEYBOARD = 2;

        public const int WM_MOUSEMOVE = 0x200;

        public const int WM_LBUTTONDOWN = 0x201;

        public const int WM_RBUTTONDOWN = 0x204;

        public const int WM_MBUTTONDOWN = 0x207;

        public const int WM_LBUTTONUP = 0x202;

        public const int WM_RBUTTONUP = 0x205;

        public const int WM_MBUTTONUP = 0x208;

        public const int WM_LBUTTONDBLCLK = 0x203;

        public const int WM_RBUTTONDBLCLK = 0x206;

        public const int WM_MBUTTONDBLCLK = 0x209;

        public const int WM_MOUSEWHEEL = 0x020A;

        public const int WM_MOUSEWHEELACT = 0x0800;

        public const byte VK_SHIFT = 0x10;
        public const byte VK_CAPITAL = 0x14;
        public const byte VK_NUMLOCK = 0x90;

        #endregion

        #region 키보드 상수
        public const int WH_KEYBOARD_LL = 13;
        public const int WM_KEYDOWN = 0x100;
        public const int WM_KEYUP = 0x101;
        public const int WM_SYSKEYDOWN = 0x104;
        public const int WM_SYSKEYUP = 0x105;

        #endregion

        // 스크립 캡쳐 기능 상수
        // const color
        public const int SRCCOPY = 0xCC0020;
        public const int WM_IME_CONTROL = 643;
    }
}
