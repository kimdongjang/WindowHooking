using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Client
{
    public class KeyboardHooking
    {
        #region 싱글톤
        static KeyboardHooking instance = null;
        static readonly object padlock = new Object();
        public static KeyboardHooking Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new KeyboardHooking();
                    }
                    return instance;
                }
            }
        }
        public KeyboardHooking()
        {
            instance = this;
        }
        #endregion

        #region Keyboard 후킹 관련 전역 변수
        public globalKeyboardHook gkh = new globalKeyboardHook();

        //================================================================
        //private bool IsCap;
        public bool IsCtr;
        public bool IsShift;
        public bool IsAlt;
        public bool IsWin;

        //================================================================
        public VirtualKeys LogPlayKey { get; set; } // 녹화 시작
        public VirtualKeys LogPlayStopKey { get; set; } // 녹화 정지
        public VirtualKeys RecordStartKey { get; set; } // 녹화 재생
        public VirtualKeys RecordStopKey { get; set; } // 재생 정지
        public VirtualKeys PauseKey { get; set; } // 일시 정지
        public VirtualKeys ModulLogKey { get; set; }  // 모듈 녹화 시작 or 종료 

        //================================================================
        //private static bool IsKey = false;
        //public bool rec_printkey = false;             // 로그가 실행될 때 키보드 값의 로그를 출력하기 위한 boolean 값

        public bool recording_starting = false;         // 녹화 중이라는 것을 알려주기 위한 boolean 값
        public bool logplaying_starting = false;        // 로그 파일 재생 중이라는 것을 알려주기 위한 boolean 값
        public bool pausing_starting = false;
        public bool logplaying_pausing = false;
        public bool module_StartOrStop = false;         // 모듈화 로그 상태
        //================================================================
        public bool f10key = false;
        public bool is_startsetting = false;

        //================================================================
        //private bool CapsLock = (((ushort)ImportFunctions.GetKeyState(0x14)) & 0xffff) != 0;
        //private bool NumLock = (((ushort)ImportFunctions.GetKeyState(0x90)) & 0xffff) != 0;
        //private bool ScrollLock = (((ushort)ImportFunctions.GetKeyState(0x91)) & 0xffff) != 0;
        private const uint WM_IME_CONTROL = 0x0283;
        #endregion

        // ===============================================================

        private CpuControl cpu_class = CpuControl.Instance;
        private Data dt_class = Data.Instance;
        public MemoDialog memo_dlg;
        public PlayInitSetting playinit_dlg;
        public RecordInitSetting rs_dlg;
        private LogSaveSetting logsave_dlg;

        public void RegistryKey()
        {
            foreach (Keys enumItem in Enum.GetValues(typeof(Keys))) // Keys enum타입 반복문, 레지스트리에 Key를 등록
            {
                gkh.HookedKeys.Add(enumItem);
            }
            foreach (VirtualKeys item in Enum.GetValues(typeof(VirtualKeys)))
            {
                Data.Instance.vKeyList.Add(item);
            }
        } // 키보드 모든 키 등록

        #region 키보드 후킹 함수
        public void KeyDown(object sender, KeyEventArgs e) // 새로운 키다운 함수임!!!
        {
            Keys Key;
            string str = "";
            char ch;

            if (recording_starting && is_startsetting)
            {
                for (int i = 0; i < gkh.HookedKeys.Count; i++)
                {
                    // 리스트의 인덱스가 입력키랑 일치할경우
                    if (e.KeyCode == gkh.HookedKeys[i])
                    {
                        str = gkh.HookedKeys[i].ToString();

                        #region (Shift) 영어 대문자 예외
                        if (IsShift == true && e.KeyValue >= 65 && e.KeyValue <= 90)
                        {
                            ch = char.Parse(e.KeyData.ToString());
                            str = ch.ToString();
                        }
                        #endregion
                        #region (Shift) 영어 소문자 예외
                        if (IsShift == true /*&& IsCap == true*/ && e.KeyValue >= 65 && e.KeyValue <= 90)
                        {
                            ch = (char)(char.Parse(str) + 32);
                            str = ch.ToString();
                        }
                        #endregion
                        #region 영어 소문자 예외
                        if (IsShift == false /*&& IsCap == false*/ && e.KeyValue >= 65 && e.KeyValue <= 90) // 영어 소문자일경우
                        {
                            ch = (char)(char.Parse(str) + 32);
                            str = ch.ToString();
                        }
                        #endregion

                        Key = gkh.HookedKeys[i];
                        // =========================================================
                        // 일반 영문 또는 한글 키 입력이 되었을 때 캡스락과 한영키가 눌렸는지 확인하기 위한 알고리즘
                        if (KeyboardHooking.Instance.module_StartOrStop == false)
                        {
                            string rec_string = str;

                            for (int j = 0; j < Data.Instance.exceptionkey.Length; j++)
                            {
                                if (str == "CapsLock")
                                {
                                    rec_string = str + "□" + MainRecording.Instance.GetWindowHandle_CapsLockValue();
                                    break;
                                }
                                else if (str == "KanaMode")
                                {
                                    rec_string = str + "■" + MainRecording.Instance.GetWindowHandle_KanaValue();
                                    break;
                                }
                                else if (str != Data.Instance.exceptionkey[j])
                                {
                                    rec_string = str + "▲" + MainRecording.Instance.GetWindowHandle_KanaValue() + "△" + MainRecording.Instance.GetWindowHandle_CapsLockValue();
                                }
                                else
                                {
                                    rec_string = str;
                                    break;
                                }
                            }

                            // key▲K△On
                            dt_class.Rec_list.Add(rec_string);
                            MainRecording.Instance.LogXamlList.Items.Add("Keyboard Count " + dt_class.Rec_list.Count().ToString() + " : " + rec_string);
                            MainRecording.Instance.LogXamlList.ScrollIntoView(MainRecording.Instance.LogXamlList.Items[MainRecording.Instance.LogXamlList.Items.Count - 1]);

                            // 레코딩 ShowList 이벤트 기록
                            MainRecording.Instance.ShowList_Dlg.S_List.Items.Add("Keyboard Count " + dt_class.Rec_list.Count().ToString() + " : " + rec_string);
                            MainRecording.Instance.ShowList_Dlg.S_List.ScrollIntoView(MainRecording.Instance.ShowList_Dlg.S_List.Items[MainRecording.Instance.ShowList_Dlg.S_List.Items.Count - 1]);
                        }
                        return;
                    }
                }
            }
        }
        public void KeyUp(object sender, KeyEventArgs e)
        {
            string str = "";

            #region ShiftKey, LControlKey, LMenu, LWin와 같은 특수키가 눌렸을 때의 처리
            switch (e.KeyCode)
            {
                case Keys.LShiftKey:
                    str = "LShiftKey Up";
                    break;
                case Keys.RShiftKey:
                    str = "RShiftKey Up";
                    break;
                case Keys.LControlKey:
                    str = "LControlKey Up";
                    break;
                case Keys.LMenu:
                    str = "LMenu Up";
                    break;
                case Keys.LWin:
                    str = "LWin Up";
                    break;
                case Keys.NumLock: ImportFunctions.keybd_event((byte)Keys.NumLock, 0, 0x02, 0); break;
                case Keys.Capital: ImportFunctions.keybd_event((byte)Keys.Capital, 0, 0x02, 0); break;
            }
            #endregion

            if (str != "")
            {
                dt_class.Rec_list.Add(str);
                // 레코딩 List 이벤트 기록
                MainRecording.Instance.LogXamlList.Items.Add("Keyboard Count " + dt_class.Rec_list.Count().ToString() + " : " + str);
                MainRecording.Instance.LogXamlList.ScrollIntoView(MainRecording.Instance.LogXamlList.Items[MainRecording.Instance.LogXamlList.Items.Count - 1]);

                // 레코딩 ShowList 이벤트 기록
                MainRecording.Instance.ShowList_Dlg.S_List.Items.Add("Keyboard Count " + dt_class.Rec_list.Count().ToString() + " : " + str);
                MainRecording.Instance.ShowList_Dlg.S_List.ScrollIntoView(MainRecording.Instance.ShowList_Dlg.S_List.Items[MainRecording.Instance.ShowList_Dlg.S_List.Items.Count - 1]);
            }
            return;
        }

        #region 녹화 종료 후 특수키가 눌려있을 경우, Up 시킴
        public void LControlKeyUp() // LControl이 눌렸을 경우 UpEvent를 호출
        {
            Keys cKey = Keys.LControlKey;
            ImportFunctions.keybd_event((byte)cKey, 0, 0x02, 0);
        }
        public void MenuKeyUp() // LControl이 눌렸을 경우 UpEvent를 호출
        {
            Keys cKey = Keys.Menu;
            ImportFunctions.keybd_event((byte)cKey, 0, 0x02, 0);
        }
        public void RShiftKeyUp() // LControl이 눌렸을 경우 UpEvent를 호출
        {
            Keys cKey = Keys.RShiftKey;
            ImportFunctions.keybd_event((byte)cKey, 0, 0x02, 0);
        }
        public void LShiftKeyUp() // LControl이 눌렸을 경우 UpEvent를 호출
        {
            Keys cKey = Keys.LShiftKey;
            ImportFunctions.keybd_event((byte)cKey, 0, 0x02, 0);
        }
        #endregion

        #region 임시 키업, 키다운 함수
        //public void gkh_KeyUp(object sender, KeyEventArgs e) // 키보드 업 이벤트가 발생했을 때의 처리
        //{
        //    string str = "";

        //    #region ShiftKey, LControlKey, LMenu, LWin와 같은 특수키가 눌렸을 때의 처리
        //    switch (e.KeyCode)
        //    {
        //        case Keys.LShiftKey:
        //            ImportFunctions.keybd_event((byte)Keys.LShiftKey, 0, 0x02, 0);
        //            if (IsShift == false) return;
        //            str = "LShiftKey Up";
        //            IsShift = false;
        //            break;
        //        case Keys.RShiftKey:
        //            ImportFunctions.keybd_event((byte)Keys.RShiftKey, 0, 0x02, 0);
        //            if (IsShift == false) return;
        //            str = "RShiftKey Up";
        //            IsShift = false;
        //            break;
        //        case Keys.LControlKey:
        //            ImportFunctions.keybd_event((byte)Keys.LControlKey, 0, 0x02, 0);
        //            if (IsCtr == false) return;
        //            if (recording_starting == false) return;
        //            str = "LControlKey Up";
        //            IsCtr = false;
        //            break;
        //        case Keys.LMenu:
        //            ImportFunctions.keybd_event((byte)Keys.LMenu, 0, 0x02, 0);
        //            if (IsAlt == false) return;
        //            str = "LMenu Up";
        //            IsAlt = false;
        //            break;
        //        case Keys.LWin:
        //            ImportFunctions.keybd_event((byte)Keys.LWin, 0, 0x02, 0);
        //            if (IsWin == false) return;
        //            str = "LWin Up";
        //            IsWin = false;
        //            break;
        //        case Keys.NumLock: ImportFunctions.keybd_event((byte)Keys.NumLock, 0, 0x02, 0); break;
        //        case Keys.Capital: ImportFunctions.keybd_event((byte)Keys.Capital, 0, 0x02, 0); break;
        //    }
        //    #endregion

        //    if (str != "")
        //    {
        //        dt_class.Rec_list.Add(str);
        //        // 레코딩 List 이벤트 기록
        //        MainRecording.Instance.LogXamlList.Items.Add("Keyboard Count " + dt_class.Rec_list.Count().ToString() + " : " + str);
        //        MainRecording.Instance.LogXamlList.ScrollIntoView(MainRecording.Instance.LogXamlList.Items[MainRecording.Instance.LogXamlList.Items.Count - 1]);

        //        // 레코딩 ShowList 이벤트 기록
        //        MainRecording.Instance.ShowList_Dlg.S_List.Items.Add("Keyboard Count " + dt_class.Rec_list.Count().ToString() + " : " + str);
        //        MainRecording.Instance.ShowList_Dlg.S_List.ScrollIntoView(MainRecording.Instance.ShowList_Dlg.S_List.Items[MainRecording.Instance.ShowList_Dlg.S_List.Items.Count - 1]);
        //    }
        //    e.Handled = true;
        //}

        //public void gkh_KeyDown(object sender, KeyEventArgs e)
        //{
        //    Keys Key;
        //    char ch = ' ';
        //    string str = "";

        //    #region ShiftKey, LControlKey, LMenu, LWin 키 상태값
        //    switch (e.KeyCode.ToString())
        //    {
        //        case "LControlKey": IsCtr = true; break;
        //        case "LMenu": IsAlt = true; break;
        //    }
        //    #endregion

        //    #region 단축키 기능
        //    if ( IsCtr == true && logplaying_starting == false) // 로그 재생
        //    {
        //        RecordPlayButtonEvent();
        //        IsCtr = false;
        //    }
        //    //if (e.KeyCode.ToString() == LogPlayStopKey && IsCtr == true) // 로그 재생 정지
        //    //{
        //    //    RecordPlayStopButtonEvent();
        //    //    IsCtr = false;
        //    //}
        //    //if (e.KeyCode.ToString() == RecordStartKey && IsCtr == true && logplaying_starting == false) // 로그 녹화 시작
        //    //{
        //    //    RecordStartButtonEvent();
        //    //    IsCtr = false;
        //    //}
        //    //if (e.KeyCode.ToString() == RecordStopKey && IsCtr == true && logplaying_starting == false) // 로그 녹화 정지
        //    //{
        //    //    RecordStopButtonEvent();
        //    //    IsCtr = false;
        //    //}
        //    //if (e.KeyCode.ToString() == ModulLogKey && IsCtr == true && logplaying_starting == false) // 로그 모듈화 시작 or 정지
        //    //{
        //    //    LogModulePlayOrStopButtonEvent();
        //    //    IsCtr = false;
        //    //}
        //    //if (e.KeyCode.ToString() == PauseKey && pausing_starting == true) // 일시정지 상태 해제 
        //    //    PauseStateEscape();
        //    #endregion


        //    #region Keydown 이벤트 처리
        //    else if (recording_starting && is_startsetting)
        //    {
        //        for (int i = 0; i < gkh.HookedKeys.Count; i++)
        //        {
        //            if (e.KeyCode == gkh.HookedKeys[i]) // 리스트의 인덱스가 입력키랑 일치할경우
        //            {
        //                str = gkh.HookedKeys[i].ToString();

        //                //#region ShiftKey, LControlKey, LMenu, LWin
        //                //switch (e.KeyCode.ToString())
        //                //{
        //                //    case "LShiftKey": IsShift = true; break;
        //                //    case "RShiftKey": IsShift = true; break;
        //                //    case "LControlKey": IsCtr = true; break;
        //                //    case "LMenu": IsAlt = true; break;
        //                //    case "LWin": IsWin = true; break;
        //                //}
        //                //#endregion
        //                #region (Shift) 영어 대문자 예외
        //                if (IsShift == true && e.KeyValue >= 65 && e.KeyValue <= 90)
        //                {
        //                    ch = char.Parse(e.KeyData.ToString());
        //                    str = ch.ToString();
        //                }
        //                #endregion
        //                #region (Shift) 영어 소문자 예외
        //                if (IsShift == true /*&& IsCap == true*/ && e.KeyValue >= 65 && e.KeyValue <= 90)
        //                {
        //                    ch = (char)(char.Parse(str) + 32);
        //                    str = ch.ToString();
        //                }
        //                #endregion
        //                #region 영어 소문자 예외
        //                if (IsShift == false /*&& IsCap == false*/ && e.KeyValue >= 65 && e.KeyValue <= 90) // 영어 소문자일경우
        //                {
        //                    ch = (char)(char.Parse(str) + 32);
        //                    str = ch.ToString();
        //                }
        //                #endregion

        //                Key = gkh.HookedKeys[i];

        //                // =========================================================
        //                // 일반 영문 또는 한글 키 입력이 되었을 때 캡스락과 한영키가 눌렸는지 확인하기 위한 알고리즘
        //                if (KeyboardHooking.Instance.module_StartOrStop == false)
        //                {
        //                    string rec_string = str;

        //                    for (int j = 0; j < Data.Instance.exceptionkey.Length; j++ )
        //                    {
        //                        if(str == "CapsLock")
        //                        {
        //                            rec_string = str + "□" + MainRecording.Instance.GetWindowHandle_CapsLockValue();
        //                            break;
        //                        }
        //                        else if(str == "KanaMode")
        //                        {
        //                            rec_string = str + "■" + MainRecording.Instance.GetWindowHandle_KanaValue();
        //                            break;
        //                        }
        //                        else if(str != Data.Instance.exceptionkey[j])
        //                        {
        //                            rec_string = str + "▲" + MainRecording.Instance.GetWindowHandle_KanaValue() + "△" + MainRecording.Instance.GetWindowHandle_CapsLockValue();
        //                        }
        //                        else
        //                        {
        //                            rec_string = str;
        //                            break;
        //                        }
        //                    }

        //                    // key▲K△On
        //                    dt_class.Rec_list.Add(rec_string);
        //                    MainRecording.Instance.LogXamlList.Items.Add("Keyboard Count " + dt_class.Rec_list.Count().ToString() + " : " + rec_string);
        //                    MainRecording.Instance.LogXamlList.ScrollIntoView(MainRecording.Instance.LogXamlList.Items[MainRecording.Instance.LogXamlList.Items.Count - 1]);

        //                    // 레코딩 ShowList 이벤트 기록
        //                    MainRecording.Instance.ShowList_Dlg.S_List.Items.Add("Keyboard Count " + dt_class.Rec_list.Count().ToString() + " : " + rec_string);
        //                    MainRecording.Instance.ShowList_Dlg.S_List.ScrollIntoView(MainRecording.Instance.ShowList_Dlg.S_List.Items[MainRecording.Instance.ShowList_Dlg.S_List.Items.Count - 1]);
        //                }

        //                else if (KeyboardHooking.Instance.module_StartOrStop)
        //                {
        //                    // 레코딩 SampleModule 이벤트 기록
        //                    MainRecording.Instance.SampleModule_Dlg.M_List.Items.Add(str);
        //                    MainRecording.Instance.SampleModule_Dlg.M_List.ScrollIntoView(MainRecording.Instance.SampleModule_Dlg.M_List.Items[MainRecording.Instance.SampleModule_Dlg.M_List.Items.Count - 1]);
                            
        //                    dt_class.Rec_list.Add(str);
        //                    dt_class.ModRec_list.Add(str); // 모듈녹화 리스트에 키 저장
        //                } 

        //                return;
        //            }
        //        }
        //        e.Handled = true;
        //    }
        //    #endregion
        //}
        
        #endregion

        #endregion

        #region Button, Keyboard 입력에 의한 Event 처리 함수

        public void RecordStopButtonEvent() // Ctrl + F10 녹화 완전 정지 
        {
            if (recording_starting) // 녹화 도중
            {
                recording_starting = false; // 일단 녹화정지 후 저장여부 메세지박스를 띄움
                MainRecording.Instance.PausedEvent(); // 녹화 일시 정지 버튼
                pausing_starting = true; // 일시정지가 시작 중

                logsave_dlg = new LogSaveSetting();
                logsave_dlg.Show();

                f10key = false;
            }
        }

        public void RecordStartButtonEvent() // Ctrl + F11 녹화 시작 / 일시 정지
        {
            if (logplaying_starting == true)
                return;

            if (pausing_starting == false)
            {
                recording_starting = !recording_starting; // toggle기능. 녹화 버튼이 눌리면 boolean 서로 바꿔준다.
                if (recording_starting && Data.Instance.RecSetting_Ondialog == false)                         // 녹화가 시작될 때
                {
                    MainRecording.Instance.LogXamlList.Items.Clear();
                    dt_class.Rec_list.Clear();

                    MainRecording.Instance.WindowStateChange("minimized");
                    rs_dlg = new RecordInitSetting();
                    rs_dlg.Show();
                    Data.Instance.RecSetting_Ondialog = true;
                }
                else // 일시정지
                {
                    if (Data.Instance.RecSetting_Ondialog)
                        return;
                    recording_starting = false;
                    MainRecording.Instance.PausedEvent(); // 녹화 일시 정지 버튼
                    SaveFunction.Instance.PauseXmlAdd(); // 일시 정지 로그 생성
                    pausing_starting = true; // 일시정지가 시작 중
                    PauseEvent.Instance.Show();
                }
            }
            else if (pausing_starting == true) // 일시 정지 상태라면
            {
                if (MessageBox.Show("일시정지를 해제 하시겠습니까?", "경고", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    PauseEvent.Instance.Hide();
                    pausing_starting = false;
                    SaveFunction.Instance.PauseEndXmlAdd(); // 녹화 일시 정지가 끝났다는 로그 생성
                    MainRecording.Instance.PauseAfterRecording();
                    recording_starting = true;

                    if (memo_dlg == null)
                        return;
                    memo_dlg.Hide();
                }
            }
        }

        public void RecordPlayButtonEvent() // Ctrl + F12 선택된 로그 실행  
        { 
            // 이미지 호환 여부 Checking
            if ((string)MainRecording.Instance.xaml_IsImageChecking.Content == "이미지 호환 여부 : X")
            {
                MessageBox.Show("해당 로그의 이미지가 호환되있지 않습니다. 이미지를 받아 호환해주세요");
                return;
            }
            
            if (recording_starting)
                return;
            if (MainRecording.Instance.xaml_LogList.SelectedValue == null)
                return;

            if (logplaying_pausing == false)
            {
                logplaying_starting = true; // toggle기능. 재생 버튼이 눌리면 boolean 서로 바꿔준다.
                if (logplaying_starting) // 로그 파일이 실행될 때
                {
                    if (playinit_dlg != null)
                    {
                        playinit_dlg.Close();
                    }
                    MainRecording.Instance.WindowStateChange("minimized");
                    playinit_dlg = new PlayInitSetting();
                    playinit_dlg.Show(); // 초기 설정을 확인하기 위한 다이얼로그 띄움
                }
            }
            // 로그 실행 중, 일시정지하고 다시 실행시켰을 때의 이벤트
            else if (logplaying_pausing == true)
            {
                MainRecording.Instance.RecordingPlayButtonControl.Content = "▶";
                logplaying_pausing = false;
                logplaying_starting = true; // 로그가 실행 중이라는 boolean변수
                MainRecording.Instance.PlayThread = new Thread(MainRecording.Instance.Play_Button);
                MainRecording.Instance.PlayThread.IsBackground = true;
                MainRecording.Instance.PlayThread.Start();
                if (MainRecording.Instance.memo_dlg == null)
                    return;
                MainRecording.Instance.memo_dlg.Close();
            }
        }

        public void RecordPlayStopButtonEvent() // Ctrl + F3 실행 중인 로그 정지
        {
            MainRecording.Instance.RecordingPlayButtonControl.Content = "▶";
            MainRecording.Instance.playthreadcontrol = true;
            logplaying_starting = false;
        }

        private void PauseStateEscape() // 일시정지 상태에서 ESC, 상태해제
        {
            if (MessageBox.Show("일시정지를 해제 하시겠습니까?", "경고", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                PauseEvent.Instance.Hide();
                pausing_starting = false;
                SaveFunction.Instance.PauseEndXmlAdd(); // 녹화 일시 정지가 끝났다는 로그 생성
                MainRecording.Instance.PauseAfterRecording();
                recording_starting = true;

                if (memo_dlg == null)
                    return;
                memo_dlg.Close();
            }
        }

        public void LogModulePlayOrStopButtonEvent() // Ctrl + F8 모듈 레코딩
        {
            if (recording_starting) // 녹화 도중
            {
                module_StartOrStop = !module_StartOrStop;   // toggle기능. M 버튼이 눌리면 boolean 서로 바꿔준다. 
                if (module_StartOrStop) // 모듈 녹화 
                {
                    MainRecording.Instance.ModuleRecord_Button();
                    SampleModule.Instance.module_Rec.Content = "Module Recording : On";
                }
                else
                {
                    SampleModule.Instance.module_Rec.Content = "Module Recording : Off";
                    MouseHooking.Instance.MouseStop(); // 마우스 이벤트 녹화 중지 
                    MessageBox.Show("샘플 모듈 녹화가 완료되었습니다. 저장할 파일이름을 적어주세요");
                    SampleModule.Instance.mouduleName.IsEnabled = true;
                }
            }
        }
        #endregion

        public void ShowMemoDlg() // 키보드 후킹 클래스를 통해서 메모 클래스를 보여줌
        {
            memo_dlg = new MemoDialog();
            memo_dlg.Show();
        }
    }
}
