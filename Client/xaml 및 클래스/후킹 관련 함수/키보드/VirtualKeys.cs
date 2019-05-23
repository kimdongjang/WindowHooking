using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    /// <summary>
    /// Enumeration for virtual keys.
    /// </summary>
    public enum VirtualKeys : ushort
    {
        /// <summary></summary>
        LeftButton = 0x01,
        /// <summary></summary>
        RightButton = 0x02,
        /// <summary></summary>
        Cancel = 0x03,
        /// <summary></summary>
        MiddleButton = 0x04,
        /// <summary></summary>
        ExtraButton1 = 0x05,
        /// <summary></summary>
        ExtraButton2 = 0x06,
        /// <summary></summary>
        Back = 0x08,
        /// <summary></summary>
        Tab = 0x09,
        /// <summary></summary>
        Clear = 0x0C,
        /// <summary></summary>
        Return = 0x0D,
        /// <summary></summary>
        Shift = 0x10,
        /// <summary></summary>
        Control = 0x11,
        /// <summary></summary>
        Menu = 0x12,
        /// <summary></summary>
        Pause = 0x13,
        /// <summary></summary>
        CapsLock = 0x14,
        /// <summary></summary>
        Kana = 0x15,
        /// <summary></summary>
        Hangeul = 0x15,
        /// <summary></summary>
        Hangul = 0x15,
        /// <summary></summary>
        Junja = 0x17,
        /// <summary></summary>
        Final = 0x18,
        /// <summary></summary>
        Hanja = 0x19,
        /// <summary></summary>
        Kanji = 0x19,
        /// <summary></summary>
        Escape = 0x1B,
        /// <summary></summary>
        Convert = 0x1C,
        /// <summary></summary>
        NonConvert = 0x1D,
        /// <summary></summary>
        Accept = 0x1E,
        /// <summary></summary>
        ModeChange = 0x1F,
        /// <summary></summary>
        Space = 0x20,
        /// <summary></summary>
        Prior = 0x21,
        /// <summary></summary>
        Next = 0x22,
        /// <summary></summary>
        End = 0x23,
        /// <summary></summary>
        Home = 0x24,
        /// <summary></summary>
        Left = 0x25,
        /// <summary></summary>
        Up = 0x26,
        /// <summary></summary>
        Right = 0x27,
        /// <summary></summary>
        Down = 0x28,
        /// <summary></summary>
        Select = 0x29,
        /// <summary></summary>
        Print = 0x2A,
        /// <summary></summary>
        Execute = 0x2B,
        /// <summary></summary>
        Snapshot = 0x2C,
        /// <summary></summary>
        Insert = 0x2D,
        /// <summary></summary>
        Delete = 0x2E,
        /// <summary></summary>
        Help = 0x2F,
        /// <summary></summary>
        N0 = 0x30,
        /// <summary></summary>
        N1 = 0x31,
        /// <summary></summary>
        N2 = 0x32,
        /// <summary></summary>
        N3 = 0x33,
        /// <summary></summary>
        N4 = 0x34,
        /// <summary></summary>
        N5 = 0x35,
        /// <summary></summary>
        N6 = 0x36,
        /// <summary></summary>
        N7 = 0x37,
        /// <summary></summary>
        N8 = 0x38,
        /// <summary></summary>
        N9 = 0x39,
        /// <summary></summary>
        A = 0x41,
        /// <summary></summary>
        B = 0x42,
        /// <summary></summary>
        C = 0x43,
        /// <summary></summary>
        D = 0x44,
        /// <summary></summary>
        E = 0x45,
        /// <summary></summary>
        F = 0x46,
        /// <summary></summary>
        G = 0x47,
        /// <summary></summary>
        H = 0x48,
        /// <summary></summary>
        I = 0x49,
        /// <summary></summary>
        J = 0x4A,
        /// <summary></summary>
        K = 0x4B,
        /// <summary></summary>
        L = 0x4C,
        /// <summary></summary>
        M = 0x4D,
        /// <summary></summary>
        N = 0x4E,
        /// <summary></summary>
        O = 0x4F,
        /// <summary></summary>
        P = 0x50,
        /// <summary></summary>
        Q = 0x51,
        /// <summary></summary>
        R = 0x52,
        /// <summary></summary>
        S = 0x53,
        /// <summary></summary>
        T = 0x54,
        /// <summary></summary>
        U = 0x55,
        /// <summary></summary>
        V = 0x56,
        /// <summary></summary>
        W = 0x57,
        /// <summary></summary>
        X = 0x58,
        /// <summary></summary>
        Y = 0x59,
        /// <summary></summary>
        Z = 0x5A,
        /// <summary></summary>
        LeftWindows = 0x5B,
        /// <summary></summary>
        RightWindows = 0x5C,
        /// <summary></summary>
        Application = 0x5D,
        /// <summary></summary>
        Sleep = 0x5F,
        /// <summary></summary>
        Numpad0 = 0x60,
        /// <summary></summary>
        Numpad1 = 0x61,
        /// <summary></summary>
        Numpad2 = 0x62,
        /// <summary></summary>
        Numpad3 = 0x63,
        /// <summary></summary>
        Numpad4 = 0x64,
        /// <summary></summary>
        Numpad5 = 0x65,
        /// <summary></summary>
        Numpad6 = 0x66,
        /// <summary></summary>
        Numpad7 = 0x67,
        /// <summary></summary>
        Numpad8 = 0x68,
        /// <summary></summary>
        Numpad9 = 0x69,
        /// <summary></summary>
        Multiply = 0x6A,
        /// <summary></summary>
        Add = 0x6B,
        /// <summary></summary>
        Separator = 0x6C,
        /// <summary></summary>
        Subtract = 0x6D,
        /// <summary></summary>
        Decimal = 0x6E,
        /// <summary></summary>
        Divide = 0x6F,
        /// <summary></summary>
        F1 = 0x70,
        /// <summary></summary>
        F2 = 0x71,
        /// <summary></summary>
        F3 = 0x72,
        /// <summary></summary>
        F4 = 0x73,
        /// <summary></summary>
        F5 = 0x74,
        /// <summary></summary>
        F6 = 0x75,
        /// <summary></summary>
        F7 = 0x76,
        /// <summary></summary>
        F8 = 0x77,
        /// <summary></summary>
        F9 = 0x78,
        /// <summary></summary>
        F10 = 0x79,
        /// <summary></summary>
        F11 = 0x7A,
        /// <summary></summary>
        F12 = 0x7B,
        /// <summary></summary>
        F13 = 0x7C,
        /// <summary></summary>
        F14 = 0x7D,
        /// <summary></summary>
        F15 = 0x7E,
        /// <summary></summary>
        F16 = 0x7F,
        /// <summary></summary>
        F17 = 0x80,
        /// <summary></summary>
        F18 = 0x81,
        /// <summary></summary>
        F19 = 0x82,
        /// <summary></summary>
        F20 = 0x83,
        /// <summary></summary>
        F21 = 0x84,
        /// <summary></summary>
        F22 = 0x85,
        /// <summary></summary>
        F23 = 0x86,
        /// <summary></summary>
        F24 = 0x87,
        /// <summary></summary>
        NumLock = 0x90,
        /// <summary></summary>
        ScrollLock = 0x91,
        /// <summary></summary>
        NEC_Equal = 0x92,
        /// <summary></summary>
        Fujitsu_Jisho = 0x92,
        /// <summary></summary>
        Fujitsu_Masshou = 0x93,
        /// <summary></summary>
        Fujitsu_Touroku = 0x94,
        /// <summary></summary>
        Fujitsu_Loya = 0x95,
        /// <summary></summary>
        Fujitsu_Roya = 0x96,
        /// <summary></summary>
        LeftShift = 0xA0,
        /// <summary></summary>
        RightShift = 0xA1,
        /// <summary></summary>
        LeftControl = 0xA2,
        /// <summary></summary>
        RightControl = 0xA3,
        /// <summary></summary>
        LeftMenu = 0xA4,
        /// <summary></summary>
        RightMenu = 0xA5,
        /// <summary></summary>
        BrowserBack = 0xA6,
        /// <summary></summary>
        BrowserForward = 0xA7,
        /// <summary></summary>
        BrowserRefresh = 0xA8,
        /// <summary></summary>
        BrowserStop = 0xA9,
        /// <summary></summary>
        BrowserSearch = 0xAA,
        /// <summary></summary>
        BrowserFavorites = 0xAB,
        /// <summary></summary>
        BrowserHome = 0xAC,
        /// <summary></summary>
        VolumeMute = 0xAD,
        /// <summary></summary>
        VolumeDown = 0xAE,
        /// <summary></summary>
        VolumeUp = 0xAF,
        /// <summary></summary>
        MediaNextTrack = 0xB0,
        /// <summary></summary>
        MediaPrevTrack = 0xB1,
        /// <summary></summary>
        MediaStop = 0xB2,
        /// <summary></summary>
        MediaPlayPause = 0xB3,
        /// <summary></summary>
        LaunchMail = 0xB4,
        /// <summary></summary>
        LaunchMediaSelect = 0xB5,
        /// <summary></summary>
        LaunchApplication1 = 0xB6,
        /// <summary></summary>
        LaunchApplication2 = 0xB7,
        /// <summary></summary>
        OEM1 = 0xBA,
        /// <summary></summary>
        OEMPlus = 0xBB,
        /// <summary></summary>
        OEMComma = 0xBC,
        /// <summary></summary>
        OEMMinus = 0xBD,
        /// <summary></summary>
        OEMPeriod = 0xBE,
        /// <summary></summary>
        OEM2 = 0xBF,
        /// <summary></summary>
        OEM3 = 0xC0,
        /// <summary></summary>
        OEM4 = 0xDB,
        /// <summary></summary>
        OEM5 = 0xDC,
        /// <summary></summary>
        OEM6 = 0xDD,
        /// <summary></summary>
        OEM7 = 0xDE,
        /// <summary></summary>
        OEM8 = 0xDF,
        /// <summary></summary>
        OEMAX = 0xE1,
        /// <summary></summary>
        OEM102 = 0xE2,
        /// <summary></summary>
        ICOHelp = 0xE3,
        /// <summary></summary>
        ICO00 = 0xE4,
        /// <summary></summary>
        ProcessKey = 0xE5,
        /// <summary></summary>
        ICOClear = 0xE6,
        /// <summary></summary>
        Packet = 0xE7,
        /// <summary></summary>
        OEMReset = 0xE9,
        /// <summary></summary>
        OEMJump = 0xEA,
        /// <summary></summary>
        OEMPA1 = 0xEB,
        /// <summary></summary>
        OEMPA2 = 0xEC,
        /// <summary></summary>
        OEMPA3 = 0xED,
        /// <summary></summary>
        OEMWSCtrl = 0xEE,
        /// <summary></summary>
        OEMCUSel = 0xEF,
        /// <summary></summary>
        OEMATTN = 0xF0,
        /// <summary></summary>
        OEMFinish = 0xF1,
        /// <summary></summary>
        OEMCopy = 0xF2,
        /// <summary></summary>
        OEMAuto = 0xF3,
        /// <summary></summary>
        OEMENLW = 0xF4,
        /// <summary></summary>
        OEMBackTab = 0xF5,
        /// <summary></summary>
        ATTN = 0xF6,
        /// <summary></summary>
        CRSel = 0xF7,
        /// <summary></summary>
        EXSel = 0xF8,
        /// <summary></summary>
        EREOF = 0xF9,
        /// <summary></summary>
        Play = 0xFA,
        /// <summary></summary>
        Zoom = 0xFB,
        /// <summary></summary>
        Noname = 0xFC,
        /// <summary></summary>
        PA1 = 0xFD,
        /// <summary></summary>
        OEMClear = 0xFE
    }

}
//    public static class VirtualKeys
//    {
//        /// <summary></summary>
//        public static uint LeftButton = 0x01;
//        /// <summary></summary>
//        public static uint RightButton = 0x02;
//        public static uint Cancel = 0x03;
//        public static uint MiddleButton = 0x04;
//        public static uint ExtraButton1 = 0x05;
//        public static uint ExtraButton2 = 0x06;
//        public static uint Back = 0x08;
//        public static uint Tab = 0x09;
//        public static uint Clear = 0x0C;
//        public static uint Return = 0x0D;
//        public static uint Shift = 0x10;
//        public static uint Control = 0x11;
//        public static uint Menu = 0x12;
//        public static uint Pause = 0x13;
//        public static uint CapsLock = 0x14;
//        public static uint Kana = 0x15;
//        public static uint Hangeul = 0x15;
//        public static uint Hangul = 0x15;
//        public static uint Junja = 0x17;
//        public static uint Final = 0x18;
//        public static uint Hanja = 0x19;
//        public static uint Kanji = 0x19;
//        public static uint Escape = 0x1B;
//        public static uint Convert = 0x1C;
//        public static uint NonConvert = 0x1D;
//        public static uint Accept = 0x1E;
//        public static uint ModeChange = 0x1F;
//        public static uint Space = 0x20;
//        public static uint Prior = 0x21;
//        public static uint Next = 0x22;
//        public static uint End = 0x23;
//        public static uint Home = 0x24;
//        public static uint Left = 0x25;
//        public static uint Up = 0x26;
//        public static uint Right = 0x27;
//        public static uint Down = 0x28;
//        public static uint Select = 0x29;
//        public static uint Print = 0x2A;
//        public static uint Execute = 0x2B;

//        public static uint Snapshot = 0x2C;

//        public static uint Insert = 0x2D;

//        public static uint Delete = 0x2E;

//        public static uint Help = 0x2F;

//        public static uint N0 = 0x30;

//        public static uint N1 = 0x31;

//        public static uint N2 = 0x32;

//        public static uint N3 = 0x33;

//        public static uint N4 = 0x34;

//        public static uint N5 = 0x35;

//        public static uint N6 = 0x36;

//        public static uint N7 = 0x37;

//        public static uint N8 = 0x38;

//        public static uint N9 = 0x39;

//        public static uint A = 0x41;

//        public static uint B = 0x42;

//        public static uint C = 0x43;

//        public static uint D = 0x44;

//        public static uint E = 0x45;

//        public static uint F = 0x46;

//        public static uint G = 0x47;

//        public static uint H = 0x48;

//        public static uint I = 0x49;

//        public static uint J = 0x4A;

//        public static uint K = 0x4B;

//        public static uint L = 0x4C;

//        public static uint M = 0x4D;

//        public static uint N = 0x4E;

//        public static uint O = 0x4F;

//        public static uint P = 0x50;

//        public static uint Q = 0x51;

//        public static uint R = 0x52;

//        public static uint S = 0x53;

//        public static uint T = 0x54;

//        public static uint U = 0x55;

//        public static uint V = 0x56;

//        public static uint W = 0x57;

//        public static uint X = 0x58;

//        public static uint Y = 0x59;

//        public static uint Z = 0x5A;

//        public static uint LeftWindows = 0x5B;

//        public static uint RightWindows = 0x5C;

//        public static uint Application = 0x5D;

//        public static uint Sleep = 0x5F;

//        public static uint Numpad0 = 0x60;

//        public static uint Numpad1 = 0x61;

//        public static uint Numpad2 = 0x62;

//        public static uint Numpad3 = 0x63;

//        public static uint Numpad4 = 0x64;

//        public static uint Numpad5 = 0x65;

//        public static uint Numpad6 = 0x66;

//        public static uint Numpad7 = 0x67;

//        public static uint Numpad8 = 0x68;

//        public static uint Numpad9 = 0x69;

//        public static uint Multiply = 0x6A;

//        public static uint Add = 0x6B;

//        public static uint Separator = 0x6C;

//        public static uint Subtract = 0x6D;

//        public static uint Decimal = 0x6E;

//        public static uint Divide = 0x6F;

//        public static uint F1 = 0x70;

//        public static uint F2 = 0x71;

//        public static uint F3 = 0x72;

//        public static uint F4 = 0x73;

//        public static uint F5 = 0x74;

//        public static uint F6 = 0x75;

//        public static uint F7 = 0x76;

//        public static uint F8 = 0x77;

//        public static uint F9 = 0x78;

//        public static uint F10 = 0x79;

//        public static uint F11 = 0x7A;

//        public static uint F12 = 0x7B;

//        public static uint F13 = 0x7C;

//        public static uint F14 = 0x7D;

//        public static uint F15 = 0x7E;

//        public static uint F16 = 0x7F;

//        public static uint F17 = 0x80;

//        public static uint F18 = 0x81;

//        public static uint F19 = 0x82;

//        public static uint F20 = 0x83;

//        public static uint F21 = 0x84;

//        public static uint F22 = 0x85;

//        public static uint F23 = 0x86;

//        public static uint F24 = 0x87;

//        public static uint NumLock = 0x90;

//        public static uint ScrollLock = 0x91;

//        public static uint NEC_Equal = 0x92;

//        public static uint Fujitsu_Jisho = 0x92;

//        public static uint Fujitsu_Masshou = 0x93;

//        public static uint Fujitsu_Touroku = 0x94;

//        public static uint Fujitsu_Loya = 0x95;

//        public static uint Fujitsu_Roya = 0x96;

//        public static uint LeftShift = 0xA0;

//        public static uint RightShift = 0xA1;

//        public static uint LeftControl = 0xA2;

//        public static uint RightControl = 0xA3;

//        public static uint LeftMenu = 0xA4;

//        public static uint RightMenu = 0xA5;

//        public static uint BrowserBack = 0xA6;

//        public static uint BrowserForward = 0xA7;

//        public static uint BrowserRefresh = 0xA8;

//        public static uint BrowserStop = 0xA9;

//        public static uint BrowserSearch = 0xAA;

//        public static uint BrowserFavorites = 0xAB;

//        public static uint BrowserHome = 0xAC;

//        public static uint VolumeMute = 0xAD;

//        public static uint VolumeDown = 0xAE;

//        public static uint VolumeUp = 0xAF;

//        public static uint MediaNextTrack = 0xB0;

//        public static uint MediaPrevTrack = 0xB1;

//        public static uint MediaStop = 0xB2;

//        public static uint MediaPlayPause = 0xB3;

//        public static uint LaunchMail = 0xB4;

//        public static uint LaunchMediaSelect = 0xB5;

//        public static uint LaunchApplication1 = 0xB6;

//        public static uint LaunchApplication2 = 0xB7;

//        public static uint OEM1 = 0xBA;

//        public static uint OEMPlus = 0xBB;

//        public static uint OEMComma = 0xBC;

//        public static uint OEMMinus = 0xBD;

//        public static uint OEMPeriod = 0xBE;

//        public static uint OEM2 = 0xBF;

//        public static uint OEM3 = 0xC0;

//        public static uint OEM4 = 0xDB;

//        public static uint OEM5 = 0xDC;

//        public static uint OEM6 = 0xDD;

//        public static uint OEM7 = 0xDE;

//        public static uint OEM8 = 0xDF;

//        public static uint OEMAX = 0xE1;

//        public static uint OEM102 = 0xE2;

//        public static uint ICOHelp = 0xE3;

//        public static uint ICO00 = 0xE4;

//        public static uint ProcessKey = 0xE5;

//        public static uint ICOClear = 0xE6;

//        public static uint Packet = 0xE7;

//        public static uint OEMReset = 0xE9;

//        public static uint OEMJump = 0xEA;

//        public static uint OEMPA1 = 0xEB;

//        public static uint OEMPA2 = 0xEC;

//        public static uint OEMPA3 = 0xED;

//        public static uint OEMWSCtrl = 0xEE;

//        public static uint OEMCUSel = 0xEF;

//        public static uint OEMATTN = 0xF0;

//        public static uint OEMFinish = 0xF1;

//        public static uint OEMCopy = 0xF2;

//        public static uint OEMAuto = 0xF3;

//        public static uint OEMENLW = 0xF4;

//        public static uint OEMBackTab = 0xF5;

//        public static uint ATTN = 0xF6;

//        public static uint CRSel = 0xF7;

//        public static uint EXSel = 0xF8;

//        public static uint EREOF = 0xF9;

//        public static uint Play = 0xFA;

//        public static uint Zoom = 0xFB;

//        public static uint Noname = 0xFC;

//        public static uint PA1 = 0xFD;

//        public static uint OEMClear = 0xFE;
//    }

//}