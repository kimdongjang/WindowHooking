using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Drawing;
using System.Windows;
using System.Text.RegularExpressions;
using System.Timers;
using System.Threading;
using System.Windows.Controls;
using System.Xml;
using System.Windows.Threading;

namespace Client
{
    /// <summary>
    /// Recording.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainRecording : Window
    {
        // ========================================================================

        #region Xmal 클래스 전역 변수
        public ShowPlaylog ShowList_Dlg;
        public RecordSetting Set_Dlg;
        public DBLogList dblist_Dlg;
        public MemoDialog memo_dlg;
        public RecentListEdit rListEdit_dlg;
        public OnFileDialogSetting on_dialog_setting;
        public ImageChecking image_checking_dlg;
        #endregion

        #region 프로그램 전역 변수
        private CpuControl cpu_class = CpuControl.Instance;
        private SaveFunction sf_class = SaveFunction.Instance;
        private Data dt_class = Data.Instance;
        private KeyboardHooking kh_class = KeyboardHooking.Instance;
        private MouseHooking ms_class = MouseHooking.Instance;
        public SampleModule SampleModule_Dlg;

        private NotifyIcon trayIcon;
        public Thread PlayThread;
        public bool pausethreadcontrol = false;
        public bool playthreadcontrol = false;
        public int play_index = 0;
        public HelpDialog Help_Dlg;
        public ImageChecking OpenImage_Dlg;
        public TabItem tabitem;
        public bool InitDBlist = false;
        #endregion

        #region 실행 윈도우 해상도 전역변수
        public double PlayLog_Width;
        public double PlayLog_Height;
        // 윈도우 해상도 초기값
        public double Window_Height = SystemParameters.MaximizedPrimaryScreenHeight + 24;
        public double Window_Width = SystemParameters.MaximizedPrimaryScreenWidth - 16;
        #endregion

        // ========================================================================

        #region 싱글톤
        static MainRecording instance = null;
        static readonly object padlock = new Object();
        public static MainRecording Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new MainRecording();
                    }
                    return instance;
                }
            }
        }
        public MainRecording()
        {
            instance = this;
            InitializeComponent();
            Init();
        }
        #endregion

        #region 초기화
        private void Init() // 초기화 함수
        {
            // ===========================================================
            // 자동저장 타이머
            RecordSetting.Instance.RealTimeSaveTimerStart();
            // 키보드 초기화
            kh_class.RegistryKey();
            ImportFunctions.keybd_event((byte)Keys.LControlKey, 0, 0x02, 0); // 컴파일 진행(Ctrl + F4)시에 Ctrl이 초기에 눌리는것을 방지
            //System.Windows.Forms.Application.Idle += kh_class.Application_Idle; // 녹화시 키보드 입력
            kh_class.gkh.KeyDown += new KeyEventHandler(kh_class.KeyDown); //후킹된 키 이벤트
            kh_class.gkh.KeyUp += new KeyEventHandler(kh_class.KeyUp);

            RecordSetting.Instance.SettingInit();
            // ===========================================================
            // CPU 점유율을 확인하기 위한 스레드 시작4
            cpu_class.StartCpu();

            // ===========================================================
            // 사용자의 "내 문서"를 읽어와 "SLog"라는 폴더가 있는지 없는지 검색하고, 없다면 새로 생성한다.
            sf_class.SLogCreateFolder();
            //Recent.xml파일을 자동 불러오기
            sf_class.InitRecentFileLoad();
            // 최근 불러온 목록과 SLog 폴더에 있는 목록을 모두 가져와서 리스트 뷰에 출력(초기화)
            sf_class.InitSaveFileLoad();
            // ===========================================================
            // 트레이 아이콘 초기화
            TrayIconInit();
        }

        private void TrayIconInit()
        {
            // 트레이아이콘에 컨텍스트 메뉴 아이템 추가, 이벤트 추가
            System.Windows.Forms.ContextMenu menu = new System.Windows.Forms.ContextMenu();

            // ===========================================================
            System.Windows.Forms.MenuItem item1 = new System.Windows.Forms.MenuItem();
            menu.MenuItems.Add(item1);
            item1.Text = "녹화시작";
            item1.Index = 0;
            item1.Click +=
                delegate(object senders, EventArgs args)
                {
                    kh_class.RecordStartButtonEvent();
                };
            // ===========================================================
            System.Windows.Forms.MenuItem item2 = new System.Windows.Forms.MenuItem();
            menu.MenuItems.Add(item2);
            item2.Text = "녹화재생";
            item2.Index = 1;
            item2.Click +=
                delegate(object senders, EventArgs args)
                {
                    kh_class.RecordPlayButtonEvent();
                };
            // ===========================================================
            System.Windows.Forms.MenuItem item3 = new System.Windows.Forms.MenuItem();
            menu.MenuItems.Add(item3);
            item3.Index = 2;
            item3.Text = "종료";
            item3.Click +=
                delegate(object senders, EventArgs args)
                {
                    ExitEvent();
                };
            // ===========================================================
            // 트레이 아이콘 설정
            trayIcon = new NotifyIcon();
            trayIcon.Icon = Client.Properties.Resources.BitIcon;
            trayIcon.Visible = true;
            trayIcon.DoubleClick +=
                delegate(object senders, EventArgs args)
                {
                    this.Show();
                    this.WindowState = WindowState.Normal;
                };
            trayIcon.ContextMenu = menu;
            trayIcon.Text = "WSU BIT24 PROJECT";
        }
        #endregion

        // ========================================================================

        #region 로그 실행 중 퍼즈 이벤트가 일어났을 때의 처리 함수
        public void PlayPause_ReadData() // 일시 정지 상태에서 메모나 스크린샷을 읽어 사용자에게 보여줌
        {
            string sdata = "";
            if (play_index == 0)
                return;

            for (int pindex = play_index; pindex < dt_class.Play_list.Count; pindex++)
            {
                if (dt_class.Play_list[pindex].Contains("PauseEvent"))
                {
                    play_index++;
                    break;
                }
                else if (dt_class.Play_list[pindex].Contains("M＆"))
                {
                    // ==================================================================================
                    // Memo 클래스를 생성하여 메모를 저장하고 초기 위치 설정을 해줌
                    sdata = dt_class.Play_list[pindex].Replace("M＆", ""); // M&이라는 문자 제거
                    memo_dlg = new MemoDialog();
                    memo_dlg.WindowStartupLocation = (WindowStartupLocation)FormStartPosition.Manual;
                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate() { memo_dlg.Show(); });
                    memo_dlg.xaml_usermemo.Text = sdata;

                    // ==================================================================================
                    // 로그에 메모 내용을 띄워줌
                    LogXamlList.Items.Add("메모 : " + sdata);
                    LogXamlList.ScrollIntoView(LogXamlList.Items[LogXamlList.Items.Count - 1]);

                    ShowList_Dlg.S_List.Items.Add("메모 : " + sdata);
                    ShowList_Dlg.S_List.ScrollIntoView(ShowList_Dlg.S_List.Items[ShowList_Dlg.S_List.Items.Count - 1]);

                    play_index++;
                }
                else if (dt_class.Play_list[pindex].Contains("↔"))
                {
                    string[] pos = dt_class.Play_list[pindex].Split('↔');
                    memo_dlg.Left = int.Parse(pos[0]);
                    memo_dlg.Top = int.Parse(pos[1]);
                    play_index++;
                }
                else if (dt_class.Play_list[pindex].Contains("Image&")) // 캡쳐 본이 담긴 로그 실행 시
                {
                    string[] pos = dt_class.Play_list[pindex].Split('&');
                    string ImagePath = pos[1];  // 이미지 경로

                    // 캡처본을 띄울 창
                    LogCapture lc = new LogCapture();
                    lc.Show();

                    //string temp = AppDomain.CurrentDomain.BaseDirectory; // 기본경로(=본 프로젝트 디버그파일) 
                    BitmapImage bi = new BitmapImage();      // BitmapImage로 우선 만들기
                    bi.BeginInit();
                    bi.CacheOption = BitmapCacheOption.OnLoad;
                    bi.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                    bi.UriSource = new Uri(ImagePath);
                    bi.EndInit();
                    lc.LogImg.Source = bi;
                }
                else if (dt_class.Play_list[pindex].Contains("D＆"))
                {
                    // ==================================================================================
                    // 다이얼로그 창을 생성, 초기 위치 설정
                    sdata = dt_class.Play_list[pindex].Replace("D＆", ""); // M&이라는 문자 제거
                    on_dialog_setting = new OnFileDialogSetting();
                    on_dialog_setting.WindowStartupLocation = (WindowStartupLocation)FormStartPosition.Manual;
                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate() { on_dialog_setting.Show(); });
                    on_dialog_setting.xaml_editdata.Text = sdata;

                    // ==================================================================================
                    // 로그에 메모 내용을 띄워줌
                    LogXamlList.Items.Add("파일 설정 : " + sdata);
                    LogXamlList.ScrollIntoView(LogXamlList.Items[LogXamlList.Items.Count - 1]);

                    ShowList_Dlg.S_List.Items.Add("파일 설정 : " + sdata);
                    ShowList_Dlg.S_List.ScrollIntoView(ShowList_Dlg.S_List.Items[ShowList_Dlg.S_List.Items.Count - 1]);

                    play_index++;
                }
                else if (dt_class.Play_list[pindex].Contains("♤"))
                {
                    string[] pos = dt_class.Play_list[pindex].Split('♤');
                    on_dialog_setting.Left = int.Parse(pos[0]);
                    on_dialog_setting.Top = int.Parse(pos[1]);
                    play_index++;
                }
            }

        }

        #endregion

        #region 녹화한 데이터를 실행할 때의 처리를 위한 함수
        // 레코딩 된 데이터 실행
        public void Play_Button()
        {
            #region 지역 변수 초기화
            Keys Key = Keys.None;
            char ch = ' ';
            int i = 0;
            bool on_mousemove = false; // 마우스가 움직일때 실행중인 로그의 정보를 UI로 보여주기 위한 boolean 값
            bool on_keymouse_checked = false;
            string[] temp = new string[2];
            #endregion

            while (true)
            {
                #region 퍼즈 이벤트가 읽힐 경우
                if (pausethreadcontrol == true)
                {
                    pausethreadcontrol = false;
                    play_index++;
                    Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate() { PlayPause_ReadData(); });

                    break;
                }
                #endregion
                if (play_index != 0) // 재생 중에서 일시정지를 했다가 다시 실행을 시킬 경우, 일시정지 한 시점의 index부터 재생되도록 설정
                {
                    i = ++play_index;
                }
                if (playthreadcontrol == true) // 실행 중인 로그를 정지시키는 기능
                {
                    playthreadcontrol = false;
                    play_index = 0;
                    break;
                }
                #region cpu 점유율이 높을 경우 멈춤
                if (cpu_class.iCPU >= 80) // CPU 점유율이 80이 넘을 경우
                {
                    //cpu_class.ExitCpu = true; // CPU 측정을 잠시 멈춤
                    //Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate() { Thread.Sleep(3000); });
                }
                #endregion
                else
                {
                    cpu_class.ExitCpu = false;
                    ch = ' ';
                    Key = Keys.None;
                    //==============================================================
                    if (i > dt_class.Play_list.Count - 2) // i(0)이 listbox.count까지 도달하면 초기화하고 중단
                    {
                        Refresh();
                        Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate() { RecordingPlayButtonControl.Content = "▶"; });
                        Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate() { ShowList_Dlg.Close(); });
                        dt_class.Play_list.Clear();
                        kh_class.logplaying_starting = false; // 실행 중인 녹화 로그의 종료를 알림
                        kh_class.logplaying_pausing = false;
                        kh_class.playinit_dlg = null;
                        play_index = 0;
                        MainRecording.Instance.WindowStateChange("normal");
                        break;
                    }
                    //==============================================================
                    
                    try
                    {
                        on_keymouse_checked = false;
                        #region 마우스 이동 이벤트
                        if (dt_class.Play_list[i].Contains("§")) // 로그에서 §을 읽어들어와 마우스 이동 이벤트인지 판별
                        {
                            string[] pos = dt_class.Play_list[i].Split('§');
                            if (Window_Height == PlayLog_Height && Window_Width == PlayLog_Width)   // 현재 해상도와 실행 로그 해상도가 같을 경우 그대로  
                                temp = pos;
                            else if (Window_Width != PlayLog_Width)    // 현재 해상도가 실행 해상도보다 클 경우 
                            {
                                temp[0] = (int.Parse(pos[0]) - (PlayLog_Width - Window_Width)).ToString();  // 둘의 차를 x좌표 값에서 차감
                                temp[1] = (int.Parse(pos[1])).ToString();
                            }

                            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate() {
                                on_keymouse_checked = true;

                                ImportFunctions.SetCursorPos(int.Parse(temp[0]), int.Parse(temp[1]));
                                ShowList_Dlg.S_List.Items.Add("X좌표 : " + temp[0] + " Y좌표 : " + temp[1]);
                                ShowList_Dlg.S_List.ScrollIntoView(ShowList_Dlg.S_List.Items[ShowList_Dlg.S_List.Items.Count - 1]);
                                LogXamlList.Items.Add("X좌표 : " + temp[0] + " Y좌표 : " + temp[1]);

                                LogXamlList.ScrollIntoView(LogXamlList.Items[LogXamlList.Items.Count - 1]);
                                Thread.Sleep((int)RecordSetting.Instance.Speed_Slider.Value);

                                on_mousemove = true;
                            });
                        }
                        #endregion
                        #region 마우스 휠 이벤트
                        else if (dt_class.Play_list[i].Contains("※"))
                        {
                            string[] delta = dt_class.Play_list[i].Split('※');
                            if (int.Parse(delta[2]) == -120)
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate() {
                                    on_keymouse_checked = true;
                                    ImportFunctions.mouse_event(Constants.WM_MOUSEWHEELACT, 0, 0, -120, 0);
                                });
                            else
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate() {
                                    on_keymouse_checked = true;
                                    ImportFunctions.mouse_event(Constants.WM_MOUSEWHEELACT, 0, 0, 120, 0);
                                });
                            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate() {
                                Thread.Sleep((int)RecordSetting.Instance.Speed_Slider.Value);
                            });
                        }
                        #endregion
                        else // #이 읽히지 않았으면 키보드와 마우스 이벤트로 간주하고,
                        {
                            // ▲는 영문키+한영키+캡스락키의 구분
                            if(dt_class.Play_list[i].ToString().Contains("▲"))
                            {
                                string[] key_data = dt_class.Play_list[i].ToString().Split('▲');
                                string[] kana_caps_data = key_data[1].Split('△');
                                #region 현재 윈도우의 한영키과 캡스락키 여부 확인
                                // 한영키 ===============================================================================
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate() {
                                    if (GetWindowHandle_KanaValue() != kana_caps_data[0]) // 현재 포커싱 된 핸들과 로그에서 읽어드린 한영 키의 상태값이 서로 다르다면
                                    {
                                        ImportFunctions.keybd_event((byte)Keys.KanaMode, 0, 0, 0);
                                    }
                                });
                                // 캡스락키 ==============================================================================
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate() {
                                    if (GetWindowHandle_CapsLockValue() != kana_caps_data[1]) // 현재 포커싱 된 핸들과 로그에서 읽어드린 한영 키의 상태값이 서로 다르다면
                                    {
                                        ImportFunctions.keybd_event((byte)Keys.Capital, 0, 0, 0);
                                    }
                                });
                                #endregion
                                ch = char.Parse(key_data[0]);

                                #region 키보드 입력 문자
                                switch (ch)
                                {
                                    case 'a': ch = (char)(ch - 32); break;
                                    case 'b': ch = (char)(ch - 32); break;
                                    case 'c': ch = (char)(ch - 32); break;
                                    case 'd': ch = (char)(ch - 32); break;
                                    case 'e': ch = (char)(ch - 32); break;
                                    case 'f': ch = (char)(ch - 32); break;
                                    case 'g': ch = (char)(ch - 32); break;
                                    case 'h': ch = (char)(ch - 32); break;
                                    case 'i': ch = (char)(ch - 32); break;
                                    case 'j': ch = (char)(ch - 32); break;
                                    case 'k': ch = (char)(ch - 32); break;
                                    case 'l': ch = (char)(ch - 32); break;
                                    case 'm': ch = (char)(ch - 32); break;
                                    case 'n': ch = (char)(ch - 32); break;
                                    case 'o': ch = (char)(ch - 32); break;
                                    case 'p': ch = (char)(ch - 32); break;
                                    case 'q': ch = (char)(ch - 32); break;
                                    case 'r': ch = (char)(ch - 32); break;
                                    case 's': ch = (char)(ch - 32); break;
                                    case 't': ch = (char)(ch - 32); break;
                                    case 'u': ch = (char)(ch - 32); break;
                                    case 'v': ch = (char)(ch - 32); break;
                                    case 'w': ch = (char)(ch - 32); break;
                                    case 'x': ch = (char)(ch - 32); break;
                                    case 'y': ch = (char)(ch - 32); break;
                                    case 'z': ch = (char)(ch - 32); break;
                                    default: break;
                                }
                                #endregion
                            }
                            else // 키보드로 입력한 문자가 아닐 경우
                            {
                                ch = char.Parse(dt_class.Play_list[i].ToString()); // ch에 listbox의 인덱스값을 집어넣고 1byte의 글자를 읽어온다.
                            }
                        }
                    }
                    #region catch
                    catch // 1byte 이상의 글자가 들어오면 catch문에서 Key를 정의해줌
                    {
                        #region 한영/캡스락
                        // 한영키 ===============================================================================
                        if (dt_class.Play_list[i].ToString().Contains("■"))
                        {
                            string[] kana_data = dt_class.Play_list[i].ToString().Split('■');
                            if (GetWindowHandle_KanaValue() != kana_data[1]) // 현재 포커싱 된 핸들과 로그에서 읽어드린 한영 키의 상태값이 서로 다르다면
                            {
                                Key = Keys.KanaMode;
                            }
                        }
                        // 캡스락키 ==============================================================================
                        if (dt_class.Play_list[i].ToString().Contains("□"))
                        {
                            string[] kana_data = dt_class.Play_list[i].ToString().Split('□');
                            if (GetWindowHandle_CapsLockValue() != kana_data[1]) // 현재 포커싱 된 핸들과 로그에서 읽어드린 한영 키의 상태값이 서로 다르다면
                            {
                                Key = Keys.Capital;
                            }
                        }
                        #endregion

                        switch (dt_class.Play_list[i].ToString())
                        {

                            #region 특수키 입력값 확인
                            case "LShiftKey": Key = Keys.LShiftKey; break;
                            case "LShiftKey Up": Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate() { ImportFunctions.keybd_event((byte)Keys.LShiftKey, 0, 0x02, 0); });
                                break;
                            case "RShiftKey": Key = Keys.RShiftKey; break;
                            case "RShiftKey Up": Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate() { ImportFunctions.keybd_event((byte)Keys.RShiftKey, 0, 0x02, 0); });
                                break;
                            case "LControlKey": Key = Keys.LControlKey; break;
                            case "LControlKey Up":
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate() { ImportFunctions.keybd_event((byte)Keys.LControlKey, 0, 0x02, 0); });
                                break;
                            case "LMenu": Key = Keys.LMenu; break;
                            case "LMenu Up":
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate() { ImportFunctions.keybd_event((byte)Keys.LMenu, 0, 0x02, 0); });
                                break;
                            case "LWin": Key = Keys.LWin; break;
                            case "LWin Up":
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate() { ImportFunctions.keybd_event((byte)Keys.LWin, 0, 0x02, 0); });
                                break;
                            case "NumLock": Key = Keys.NumLock; break;
                            case "Escape": Key = Keys.Escape; break;
                            case "Tab": Key = Keys.Tab; break;
                            //case "Capital": Key = Keys.Capital; break;
                            case "Space": Key = Keys.Space; break;
                            //case "KanaMode": Key = Keys.KanaMode; break;
                            case "Apps": Key = Keys.Apps; break;
                            case "HanjaMode": Key = Keys.HanjaMode; break;
                            case "Left": Key = Keys.Left; break;
                            case "Up": Key = Keys.Up; break;
                            case "Right": Key = Keys.Right; break;
                            case "Down": Key = Keys.Down; break;
                            case "Oemtilde": Key = Keys.Oemtilde; break;
                            case "Oemcomma": Key = Keys.Oemcomma; break;
                            case "OemPeriod": Key = Keys.OemPeriod; break;
                            case "OemQuestion": Key = Keys.OemQuestion; break;
                            case "Oem1": Key = Keys.Oem1; break;
                            case "Oem5": Key = Keys.Oem5; break;
                            case "Oem6": Key = Keys.Oem6; break;
                            case "Oem7": Key = Keys.Oem7; break;
                            case "OemOpenBrackets": Key = Keys.OemOpenBrackets; break;
                            case "OemMinus": Key = Keys.OemMinus; break;
                            case "Oemplus": Key = Keys.Oemplus; break;
                            case "Back": Key = Keys.Back; break;
                            case "Return": Key = Keys.Return; break;
                            case "D0": Key = Keys.D0; break;
                            case "D1": Key = Keys.D1; break;
                            case "D2": Key = Keys.D2; break;
                            case "D3": Key = Keys.D3; break;
                            case "D4": Key = Keys.D4; break;
                            case "D5": Key = Keys.D5; break;
                            case "D6": Key = Keys.D6; break;
                            case "D7": Key = Keys.D7; break;
                            case "D8": Key = Keys.D8; break;
                            case "D9": Key = Keys.D9; break;
                            case "F1": Key = Keys.F1; break;
                            case "F2": Key = Keys.F2; break;
                            case "F3": Key = Keys.F3; break;
                            case "F4": Key = Keys.F4; break;
                            case "F5": Key = Keys.F5; break;
                            case "F6": Key = Keys.F6; break;
                            case "F7": Key = Keys.F7; break;
                            case "F8": Key = Keys.F8; break;
                            case "F9": Key = Keys.F9; break;
                            case "F10": Key = Keys.F10; break;
                            case "F11": Key = Keys.F11; break;
                            case "F12": Key = Keys.F12; break;
                            case "NumPad0": Key = Keys.NumPad0; break;
                            case "NumPad1": Key = Keys.NumPad1; break;
                            case "NumPad2": Key = Keys.NumPad2; break;
                            case "NumPad3": Key = Keys.NumPad3; break;
                            case "NumPad4": Key = Keys.NumPad4; break;
                            case "NumPad5": Key = Keys.NumPad5; break;
                            case "NumPad6": Key = Keys.NumPad6; break;
                            case "NumPad7": Key = Keys.NumPad7; break;
                            case "NumPad8": Key = Keys.NumPad8; break;
                            case "NumPad9": Key = Keys.NumPad9; break;
                            case "PageUp": Key = Keys.PageUp; break;
                            case "Next": Key = Keys.Next; break;
                            case "PrintScreen": Key = Keys.PrintScreen; break;
                            case "Delete": Key = Keys.Delete; break;
                            case "Decimal": Key = Keys.Decimal; break;
                            case "Subtract": Key = Keys.Subtract; break;
                            case "Add": Key = Keys.Add; break;
                            case "Divide": Key = Keys.Divide; break;
                            case "Multiply": Key = Keys.Multiply; break;
                            #endregion
                            // ============================================================================
                            #region 마우스 이벤트
                            case "LeftDown":
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate() { ImportFunctions.mouse_event(Constants.MOUSEEVENTF_LEFTDOWN, 50, 50, 0, 0); });
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate() { Thread.Sleep((int)RecordSetting.Instance.Speed_Slider.Value); });
                                break;
                            case "LeftUp":
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate() { ImportFunctions.mouse_event(Constants.MOUSEEVENTF_LEFTUP, 50, 50, 0, 0); });
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate() { Thread.Sleep((int)RecordSetting.Instance.Speed_Slider.Value); });
                                break;
                            case "RightDown":
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate() { ImportFunctions.mouse_event(Constants.MOUSEEVENTF_RIGHTDOWN, 0, 0, 0, 0); });
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate() { Thread.Sleep((int)RecordSetting.Instance.Speed_Slider.Value); });
                                break;
                            case "RightUp":
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate() { ImportFunctions.mouse_event(Constants.MOUSEEVENTF_RIGHTUP, 0, 0, 0, 0); });
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate() { Thread.Sleep((int)RecordSetting.Instance.Speed_Slider.Value); });
                                break;
                            case "Double":
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate() { ImportFunctions.mouse_event(Constants.MOUSEEVENTF_LEFTDOWN | Constants.MOUSEEVENTF_LEFTUP, 0, 0, 0, 0); });
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate() { Thread.Sleep(150); });
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate() { ImportFunctions.mouse_event(Constants.MOUSEEVENTF_LEFTDOWN | Constants.MOUSEEVENTF_LEFTUP, 0, 0, 0, 0); });
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate() { Thread.Sleep((int)RecordSetting.Instance.Speed_Slider.Value); });
                                break;
                            #endregion
                            // ============================================================================
                            // 일시정지가 읽힐 경우 play_index에 현재까지 진행한 인덱스를 저장하고 로그 플레이를 멈춤
                            case "PauseEvent":
                                play_index = i;
                                kh_class.logplaying_starting = false;
                                kh_class.logplaying_pausing = true;
                                pausethreadcontrol = true;
                                break;

                            default: break;
                        }
                    }
                    #endregion
                    
                    finally
                    {
                        if (ch != ' ') // 1byte크기가 들어오면 Key에 집어넣음
                            Key = (Keys)ch;

                        #region 키보드 이벤트 실행
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate() {

                            ImportFunctions.keybd_event((byte)Key, 0, 0, 0);
                            if (on_keymouse_checked == false)
                            {
                                Thread.Sleep(50);
                            }
                        });
                        
                        switch (Key)
                        {
                            case Keys.NumLock: ImportFunctions.keybd_event((byte)Keys.NumLock, 0, 0x02, 0); break;
                            case Keys.Capital: ImportFunctions.keybd_event((byte)Keys.Capital, 0, 0x02, 0); break;
                        }

                        #endregion

                        // 실행중인 로그 데이터 ==================================================================
                        if (on_mousemove == false)
                        {
                            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate()
                            {
                                ShowList_Dlg.S_List.Items.Add(dt_class.Play_list[i]);
                                ShowList_Dlg.S_List.ScrollIntoView(ShowList_Dlg.S_List.Items[ShowList_Dlg.S_List.Items.Count - 1]);
                                LogXamlList.Items.Add(dt_class.Play_list[i]);
                                LogXamlList.ScrollIntoView(LogXamlList.Items[LogXamlList.Items.Count - 1]);
                            });
                        }
                        on_mousemove = false;
                        i++;
                        ch = ' ';
                    }

                }
            }
            kh_class.recording_starting = false;
        }
        #endregion

        public void Record_Button() // 레코딩 시작
        {
            SaveFunction.Instance.GetSaveFileIndex();
            RecordSetting.Instance.recordSave_onoff = true;
            dt_class.Rec_list.Clear();
            if (kh_class.recording_starting)
            {
                // UI로 보여질 녹화되는 이벤트 목록 출력 함수
                UpShowListDlg();

                // 레코딩 이벤트 시작
                state.Content = "Recording : True";
                ms_class.MouseStart();
            }
        }

        public void ModuleRecord_Button()  // 모듈 레코딩
        {
            if (kh_class.recording_starting)
            {
                //if (SampleModule_Dlg == null)
                //{
                // UI로 보여질 모듈 녹화 이벤트 목록 출력 함수
                MoudleShowListDlg();
                ms_class.MouseStart();
                //}
            }
        }

        public void PauseAfterRecording() // 레코딩 이벤트 시작
        {
            state.Content = "Recording : True";
            ms_class.MouseStart();
        }

        #region 녹화 정지 관련
        public void PausedEvent() // 녹화 일시 정지 기능
        {
            state.Content = "Recording : False";
            ms_class.MouseStop();
        }

        public void StopEvent() // 녹화 완전 정지 기능
        {
            LogXamlList.Items.Clear();
            //dt_class.Rec_list.Clear();

            ms_class.MouseStop();
            RecordSetting.Instance.recordSave_onoff = false;
            kh_class.recording_starting = false;
            kh_class.pausing_starting = false;
        }
        #endregion

        // ========================================================================

        #region Tab1 UI Control
        private void RecordStartButton(object sender, RoutedEventArgs e) // 레코딩 시작
        {
            kh_class.RecordStartButtonEvent();
        }
        private void RecordStopButton(object sender, RoutedEventArgs e) // 레코딩 정지
        {
            kh_class.RecordStopButtonEvent();
        }
        private void RecordPlayOrStopButton(object sender, RoutedEventArgs e) // 진행중인 레코딩 플레이
        {
            kh_class.RecordPlayButtonEvent();
        }
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            ExitEvent();
        }
        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            //RecentXamlList.Items.Clear();
        }
        private void Setting_Click(object sender, RoutedEventArgs e)
        {
            RecordSetting.Instance.Show();
        }
        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            Refresh();
        }
        private void Recording_Settings_Click(object sender, RoutedEventArgs e)
        {
            // RecSet 클래스의 컨트롤 사용하기
            RecordInitSetting rs = new RecordInitSetting();
            rs.Show();
        }
        private void LogModulePlayOrStopButton(object sender, RoutedEventArgs e)
        {
            kh_class.LogModulePlayOrStopButtonEvent();
        }
        private void Delete_Click(object sender, RoutedEventArgs e) // 삭제하기
        {
            sf_class.SaveListDeleteClickEvent();
        }
        private void Exit2_Click(object sender, RoutedEventArgs e)
        {
            xamlChannelList.Items.Clear();
            Control.Instance.ChannelAllList();
            ExitEvent();
        }
        private void ExitButton2_Click(object sender, RoutedEventArgs e)
        {
            ExitEvent();
        }
        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowStateChange("minimized");
        }
        private void Create_Click(object sender, RoutedEventArgs e) // 새 녹화 파일 생성
        {
            sf_class.SLogCreateFolder();
        }
        private void Load_Click(object sender, RoutedEventArgs e)  // 불러오기 
        {
            sf_class.LogFileLoadFunction();
        }

        private void LogEdit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Help_Dlg == null)
                {
                    Help_Dlg = new HelpDialog();
                    Help_Dlg.Show();
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
        }

        private void OpenImage_Click(object sender, RoutedEventArgs e)
        {
            if (OpenImage_Dlg == null)
            {
                OpenImage_Dlg = new ImageChecking();
                OpenImage_Dlg.Show();
            }
        }
        private void RecentListEdit_Click(object sender, RoutedEventArgs e)
        {
            rListEdit_dlg = new RecentListEdit();
            rListEdit_dlg.Show();
        }

        private void DataBaseButton_Click(object sender, RoutedEventArgs e)
        {
            if (InitDBlist == false)
            {
                InitDBlist = true;
                dblist_Dlg = new DBLogList();
                dblist_Dlg.Show();
            }
        }
        #endregion

        #region Tab1 Event
        public void WindowStateChange(string option)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate()
            {
                if (option == "minimized")
                    this.WindowState = WindowState.Minimized;
                else if (option == "normal")
                    this.WindowState = WindowState.Normal;
            });
        }
     
        public void UpShowListDlg() // UI로 보여질 녹화되는 이벤트 목록 출력 함수
        {
            ShowList_Dlg = new ShowPlaylog();
            ShowList_Dlg.WindowStartupLocation = (WindowStartupLocation)FormStartPosition.Manual;
            ShowList_Dlg.Left = SystemParameters.WorkArea.Width - ShowList_Dlg.Width;
            ShowList_Dlg.Top = SystemParameters.WorkArea.Height - ShowList_Dlg.Height;
            ShowList_Dlg.Show();
        }
        public void ShowWindowDialog_Pause() // 윈도우의 다이얼로그 창이 발생할 겨우 퍼즈 이벤트
        {
            const int nChars = 256;
            int handle = 0;
            StringBuilder Buff = new StringBuilder(nChars);
            handle = ImportFunctions.GetForegroundWindow();

            if (ImportFunctions.GetWindowText(handle, Buff, nChars) > 0)
            {
                string state = "";
                Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate() { state = Buff.ToString(); });

                if (state == "열기"
                    || state == "새 매크로 생성"
                    || state == "폴더 찾아보기"
                    || state == "참조할 파일 선택..."
                    || state == "다른 이름으로 저장"
                    || state.Contains("참조 관리자"))
                {
                    kh_class.recording_starting = false;
                    PausedEvent(); // 녹화 일시 정지 버튼
                    SaveFunction.Instance.PauseXmlAdd(); // 일시 정지 로그 생성
                    kh_class.pausing_starting = true; // 일시정지가 시작 중
                    Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate()
                    {
                        if (on_dialog_setting != null)
                        {
                            on_dialog_setting.Close();
                        }
                        on_dialog_setting = new OnFileDialogSetting();
                        on_dialog_setting.Show();
                    });
                }
            }

        }
        public string GetWindowHandle_CapsLockValue()
        {
            if ((ImportFunctions.GetKeyState((int)Keys.CapsLock) & 0xffff) != 0)
            {
                return "On";
            }
            else
                return "Off";
        }
        public string GetWindowHandle_KanaValue() // 윈도우의 핸들을 가져와 한영 모드의 상태를 확인하고 "영문"default로 맞춰줌
        {
            try
            {
                IntPtr hwnd = (IntPtr)ImportFunctions.GetForegroundWindow();

                IntPtr hime = ImportFunctions.ImmGetDefaultIMEWnd(hwnd);
                IntPtr status = ImportFunctions.SendMessage(hime, Constants.WM_IME_CONTROL, new IntPtr(0x5), new IntPtr(0));

                if (status.ToInt32() != 0)
                {
                    return "K";
                    //Keys Key = Keys.KanaMode;
                    //ImportFunctions.keybd_event((byte)Key, 0, 0, 0);
                }
                else
                {
                    return "E";
                }
            }
            catch
            {
                return "";
            }
        }
        private void Window_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        public void Refresh()
        {
            //Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate() { RecentXamlList.Items.Clear(); });
            Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate() { LogXamlList.Items.Clear(); });
            //Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate() { SaveXamlList.Items.Clear(); });
            Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate() { sf_class.InitSaveFileLoad(); });
        }
        private void ExitEvent()
        {
            try
            {
                Data.Instance.MyChannelList.Clear();
                for (int i = 0; i < MainRecording.Instance.xaml_TabControl.Items.Count; i++)
                {
                    MainRecording.Instance.xaml_TabControl.Items.RemoveAt(2);
                }
            }
            catch
            {


            }
            finally
            {
                SendingLogoutPacket();
                this.Hide();
            }
        }
        public void SendingLogoutPacket()
        {
            string packet = "LOGOUT" + "$";
            packet += Data.Instance.myId;
            MyClient.Instance.SendDataOne(packet);
        }
        private void SaveList_SelectionChanged(object sender, SelectionChangedEventArgs e) // 저장 목록 선택
        {
            sf_class.SaveListClickEvent();
        }
        private void RecentList_SelectionChanged(object sender, SelectionChangedEventArgs e) // 최근 불러온 목록 선택
        {
            sf_class.RecentListClickEvent();
        }
        public void MoudleShowListDlg() // UI로 보여질 녹화되는 이벤트 목록 출력 함수
        {
            SampleModule_Dlg = new SampleModule();
            SampleModule_Dlg.WindowStartupLocation = (WindowStartupLocation)FormStartPosition.CenterScreen;
            SampleModule_Dlg.Left = SystemParameters.WorkArea.Width - ShowList_Dlg.Width;
            SampleModule_Dlg.Top = SystemParameters.WorkArea.Height - ShowList_Dlg.Height;
            SampleModule_Dlg.Show();
        }
        #endregion


        #region Tab2 UI Control
        private void CreateChannel(object sender, RoutedEventArgs e) // 채널생성
        {
            CreateChannel ch = new CreateChannel();
            ch.Show();
        }
        private void SearchTextBox_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e) // 텍스트박스 선택
        {
            if (MainRecording.Instance.SearchTextBox.Text == "채널명")
                MainRecording.Instance.SearchTextBox.Text = "";
        }
        private void LoadChannelListButton(object sender, RoutedEventArgs e) // 전체채널
        {
            Control.Instance.ChannelAllList();
        }
        private void Join_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ChannelInputPassword passworddialog = null;
                string ischannelpassword = "";

                ischannelpassword = Data.Instance.ChannelList[MainRecording.Instance.xamlChannelList.SelectedIndex].ChannelIsPw;

                // 비밀번호 체크여부
                if (ischannelpassword == "O") // 채널 잠금상태
                {
                    // 채널이 잠금상태라면 사용자에게 다이얼로그를 띄워 비밀번호 입력을 요구
                    passworddialog = new ChannelInputPassword();
                    passworddialog.InitPassword(Data.Instance.ChannelList[MainRecording.Instance.xamlChannelList.SelectedIndex].ChannelPw);
                    passworddialog.Show();
                }

                else if (ischannelpassword == "X") // 채널 잠금해제상태
                {
                    MainChannel.Instance.ChannelJoin(Data.Instance.ChannelList[MainRecording.Instance.xamlChannelList.SelectedIndex].ChannelName);
                }
            }
            catch
            {

            }
        }
        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string[] cha_data = MainChannel.Instance.ChannelSearchList.Split('@');
            string[] data;

            for (int i = 0; i < cha_data.Length - 1; i++) // 전체 채널 리스트가 들어 있음
            {
                data = cha_data[i].Split('#');
                if (SearchTextBox.Text == data[1])
                {
                    if (data[6] == "1")
                    {
                        data[6] = "O";
                    }
                    else if (data[6] == "0")
                    {
                        data[6] = "X";
                    }
                    xamlChannelList.Items.Clear();
                    xamlChannelList.Items.Add(new ChannelList(data[1], data[3], data[4] + "/" + data[5], data[6]));
                }
            }
        }
        private void Sending_Click(object sender, RoutedEventArgs e)
        {
            MainChannel.Instance.Tab2Chat(Chatting.Text);
            Chatting.Clear();
        }
        #endregion

        #region Tab2 Event
        private void TabItem_Loaded(object sender, RoutedEventArgs e)
        {
            Control.Instance.ChannelAllList();
        }
        private void Chatting_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                MainChannel.Instance.Tab2Chat(Chatting.Text);
                Chatting.Clear();
            }
            else
            {
                return;
            }
        }
        #endregion

        #region Tab2 MenuItem
        private void Admin_ChannelInformation_Click(object sender, RoutedEventArgs e)
        {
            ChannelInformation.Instance.Show();
            ChannelInformation.Instance.ChannelInformationTextInput(Data.Instance.ChannelList[MainRecording.Instance.xamlChannelList.SelectedIndex].ChannelTitle);
        }
        #endregion

        private void LogDeleteAll_Click(object sender, RoutedEventArgs e)
        {
            if (MainRecording.Instance.xaml_LogList.Items.Count == 0)
            {
                return;
            }

            else
            {
                if (System.Windows.MessageBox.Show("모든 로그 리스트와 데이터를 삭제하시겠습니까?", "", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    // 파일 디렉토리 비우기
                    string sDirPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\SLog\");
                    DirectoryInfo di = new DirectoryInfo(sDirPath);

                    foreach (var item in di.GetFiles())
                    {
                        File.Delete(sDirPath + item.Name);
                    }
                    // 초기화
                    MainRecording.Instance.xaml_LogList.Items.Clear();
                    SaveFunction.Instance.InitSaveFileLoad();
                }
            }
        }

        private void Down_Update_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 아무것도 선택되어있지 않으면 리턴
                if (xaml_LogList.SelectedIndex == -1)
                {
                    return;
                }
                else
                {
                    if (xaml_LogList.SelectedIndex + 1 != -1)
                    {
                        // 5번 "abc" 6번 "def
                        int sindex = xaml_LogList.SelectedIndex;

                        var temp = xaml_LogList.Items[sindex + 1]; // temp 변수에 자리를 변경할 데이터 저장 "def"
                        //string path = Data.Instance.xmlLog_list[sindex]; //5번 선택

                        xaml_LogList.Items.Insert(sindex, temp);
                        // 선택한 인덱스 자리(5번)에 아이템을 추가
                        // 5번자리 "def", 6번자리 "abc"생성 7번자리 "def"
                        xaml_LogList.Items.RemoveAt(sindex + 2);
                        // 7번 자리에 def를 삭제
                        // 5번 abc 6번 def
                        // ===============================================================================

                        //var temp2 = Data.Instance.xmlLog_list[sindex];
                        //Data.Instance.xmlLog_list[sindex] = Data.Instance.xmlLog_list[sindex + 1];
                        //Data.Instance.xmlLog_list[sindex + 1] = temp2;

                    }
                    SaveFunction.Instance.GetLoadListName();
                    SaveFunction.Instance.RecentLog_UpdateToXml();
                }

            }
            catch
            {

            }
        }
        private void Up_Update_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 아무것도 선택되어있지 않으면 리턴
                if (xaml_LogList.SelectedIndex == -1)
                {
                    return;
                }
                else
                {
                    if (xaml_LogList.SelectedIndex - 1 != -1)
                    {
                        // 4번 "abc" 5번 "def"
                        int sindex = xaml_LogList.SelectedIndex; // 5번 선택

                        var temp = xaml_LogList.Items[sindex]; // temp 변수에 자리를 변경할 데이터 저장 "abc"
                        //string path = Data.Instance.xmlLog_list[sindex];

                        xaml_LogList.Items.Insert(sindex - 1, temp);
                        // 선택한 인덱스 자리 -1(4번)에 아이템을 추가
                        // 4번자리 "def", 5번자리 "abc"생성 6번자리 "def"
                        xaml_LogList.Items.RemoveAt(sindex + 1);
                        // 6번 자리에 def를 삭제
                        // 4번 def 5번 abc
                        // ===============================================================================

                        //var temp2 = Data.Instance.xmlLog_list[sindex]; // 5번 선택 
                        //Data.Instance.xmlLog_list[sindex] = Data.Instance.xmlLog_list[sindex - 1]; // 5번 자리에 4번을 넣고
                        //Data.Instance.xmlLog_list[sindex - 1] = temp2; // 4번 자리에 5번을 넣음

                    }
                    SaveFunction.Instance.GetLoadListName();
                    SaveFunction.Instance.RecentLog_UpdateToXml();
                }
            }
            catch
            {
            }
        }

        private void LogFileAdd_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog openFileDlg = new System.Windows.Forms.OpenFileDialog(); // 파일 찾아보기 다이얼로그
            openFileDlg.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            openFileDlg.Filter = "XML(*.xml)|*.xml|모든파일(*.*)|*.*";
            openFileDlg.Title = "추가할 로그를 선택하십시오.";

            if (openFileDlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                for (int i = 0; i < xaml_LogList.Items.Count; i++)
                {
                    if (Data.Instance.init_loglistview[i].FilePath == openFileDlg.FileName)
                    {
                        System.Windows.MessageBox.Show("같은 파일 이름이 로그 목록에 존재합니다.");
                        return;
                    }
                }

                string filepathname = Data.Instance.filePath + openFileDlg.SafeFileName; // 파일 경로 + 이름
                string filepath = Path.GetDirectoryName(filepathname);                   // 파일 경로
                string filename = Path.GetFileNameWithoutExtension(filepathname);        // 파일 이름

                File.Move(openFileDlg.FileName, filepathname); // 원본파일 이동
                string imagefilepath = Path.GetDirectoryName(openFileDlg.FileName);
                File.Move(imagefilepath + "\\" + filename + "_image[0].jpg", Data.Instance.filePath + "\\" + filename + "_image[0].jpg"); // 초기 이미지 파일 이동

                Data.Instance.LoadLog_list.Add(openFileDlg.FileName);
                SaveFunction.Instance.RecentLog_UpdateToXml();
                SaveFunction.Instance.InitSaveFileLoad();

                XmlDocument doc = new XmlDocument();
                doc.Load(filepathname);
                XmlNode Node = doc.DocumentElement;
                XmlNode InitImgNode = Node.SelectSingleNode("InitImg");
                InitImgNode.InnerText = Data.Instance.filePath + "\\" + filename + "_image[0].jpg";

                doc.Save(filepathname);
                Data.Instance.screenIndex = 0;
            }
        }
        
        private void EditImageLogLocation(OpenFileDialog filedlg)
        {
            string filepathname = filedlg.FileName; // 파일 경로 + 이름
            string filename = Path.GetFileNameWithoutExtension(filepathname); // 파일 이름
            string filepath = Path.GetDirectoryName(filepathname); // 파일 경로

            XmlDocument doc = new XmlDocument();
            doc.Load(filepathname);

            XmlNode Node = doc.DocumentElement;
            XmlNode InitImgNode = Node.SelectSingleNode("InitImg");
            InitImgNode.InnerText = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\SLog\") + "\\" + filename + "_image[0].jpg";

            doc.Save(filepathname);
        }

        private void SelectDelete_Click(object sender, RoutedEventArgs e)
        {
            int sindex = xaml_LogList.SelectedIndex;
            if (xaml_LogList.SelectedIndex != -1)
            {
                if (System.Windows.MessageBox.Show("이 항목을 삭제하시겠습니까?", "", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    // 선택한 인덱스가 "SLog"를 contains한다면, 자멜과 파일디렉토리에서 삭제를 함.
                    if (Data.Instance.init_loglistview[sindex].FilePath.Contains("SLog"))
                    {
                        try
                        {
                            File.Delete(Data.Instance.init_loglistview[sindex].FilePath);
                        }
                        catch
                        { 
                        }
                        // 관련된 이미지파일도 삭제한다.
                        string sDirPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\SLog\");
                        DirectoryInfo di = new DirectoryInfo(sDirPath);

                        // 파일디렉토리를 돌아서, FileName과, .jpg를 포함한다면 모두 삭제.
                        foreach (var item in di.GetFiles()) 
                        {
                            if (item.Name.Contains(".jpg"))
                            {
                                string[] filenameExceptjpg = item.Name.Split('_');
                                //if (Data.Instance.init_loglistview[sindex].FileName.Contains(filenameExceptjpg[0]))
                                if (Data.Instance.init_loglistview[sindex].FileName == filenameExceptjpg[0] + ".xml")
                                {
                                    File.Delete(sDirPath + item.Name);
                                    xaml_LogList.Items.RemoveAt(sindex);
                                }
                            }
                        }
                    }
                    // 그게 아니라면 Recent.xml에서 요소를 삭제하고(LoadLogList 컬렉션에서도 삭제) 리스트갱신을 다시 요구함
                    else
                    {
                        // 셀렉티드 인덱스를 LoadLog 컬렉션에서 삭제하고, 자멜에서 삭제함.
                        for (int i = 0; i < Data.Instance.LoadLog_list.Count; i++)
                        {
                            if (Data.Instance.LoadLog_list[i] == Data.Instance.init_loglistview[MainRecording.Instance.xaml_LogList.SelectedIndex].FilePath)
                            {
                                
                                // 실제 파일 디렉토리에서 삭제
                                try
                                {
                                    File.Delete(Data.Instance.init_loglistview[sindex].FilePath);
                                }
                                catch
                                {
                                }
                                // 데이터 삭제
                                Data.Instance.init_loglistview.RemoveAt(sindex);
                                // 리스트뷰에서 삭제
                                xaml_LogList.Items.RemoveAt(sindex);
                                break;
                            }
                        }
                        // 다시 갱신을 요구하는 함수 (GetLoadListName())에서 LoadLog 컬렉션을 초기화한후 현재 xml_LogList를 순회하여 
                        // SLog가 포함되지 않은 로그경로들을 모두 모아서 LoadLog 컬렉션에 추가함.
                        SaveFunction.Instance.GetLoadListName();
                        // 마지막으로 RecentLog_UpdateToXml()함수를 호출하여 메인화면의 xml_loglistview를 초기화해주면 끝.
                        SaveFunction.Instance.RecentLog_UpdateToXml();
                    }
                    //초기화
                    SaveFunction.Instance.InitSaveFileLoad();
                }
            }
        }

        private void xaml_LogList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            sf_class.SaveListClickEvent();
        }

        private void UploadImg_Click(object sender, RoutedEventArgs e)
        {
            if (xaml_LogList.SelectedItem == null)
            {
                System.Windows.MessageBox.Show("이미지 업로할 로그를 선택해주세요!");
                return;
            }
            string CompLog = Data.Instance.init_loglistview[xaml_LogList.SelectedIndex].FilePath;
            // 이미지 호환
            MainRecording.Instance.image_checking_dlg = new ImageChecking();
            MainRecording.Instance.image_checking_dlg.Show();
            MainRecording.Instance.image_checking_dlg.InitLogFileLoad(CompLog);
        }
    }

    public partial class MainChannel : Window
    {
        // ========================================================================

        MyClient client = MyClient.Instance;
        public string ChannelSearchList;

        // ========================================================================

        #region 싱글톤
        static MainChannel instance = null;
        static readonly object padlock = new Object();
        public static MainChannel Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new MainChannel();
                    }
                    return instance;
                }
            }
        }
        public MainChannel()
        {
            instance = this;
        }
        #endregion

        public void LoadChannel(string[] pack) // 전체채널 버튼 이벤트
        {
            Data.Instance.ChannelList.Clear();

            string[] channel_data = pack[1].Split('@');
            for (int i = 0; i < channel_data.Length - 1; i++)
            {
                string[] data = channel_data[i].Split('#');
                if (data[0] != "1000")
                {
                    MainRecording.Instance.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate()
                    {
                        if (data[6] == "1")
                        {
                            data[6] = "O";
                        }
                        else if (data[6] == "0")
                        {
                            data[6] = "X";
                        }

                        MainRecording.Instance.xamlChannelList.Items.Add(new ChannelList(data[1], data[3], data[4] + "/" + data[5], data[6]));
                        Data.Instance.ChannelList.Add(new ChannelList(data[0], data[1], data[2], data[3], data[4], data[5], data[6], data[7]));
                        ChannelSearchList = pack[1];
                    });
                }
            }
        }

        public void ChannelJoin(string cname) // 채널 접속 
        {
            try
            {
                Data.Instance.exam_cname = cname; // 전역 변수로 현재 접속한 채널 이름을 저장
                string packet = "CHANNEL_JOIN" + "$";
                packet += Data.Instance.exam_cname + "#";
                packet += Data.Instance.myId;

                MyClient.Instance.SendDataOne(packet);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
        }
        public void Tab2Chat(string msg) // 채널 접속 채팅
        {
            string packet = "ACHATTING_SEND" + "$"; // 전체채팅 패킷 메시지 
            packet += Data.Instance.myId + "#";     // 로그인한 아이디 
            packet += msg;                          // 채팅 내용

            MyClient.Instance.SendDataOne(packet);
        }

        public void Chatting(string[] pack) // 메인 채널 채팅
        {
            string[] chat_data = pack[1].Split('#');
            if (chat_data[1].Trim() == "1000")     // 디폴트 채팅 인덱스(1000)일 때
            {
                MainRecording.Instance.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate()
                {
                    string chatLine = "[" + chat_data[0] + "] :" + chat_data[2] + "\n";
                    MainRecording.Instance.ChatTxt.Text += chatLine;
                });
            }
        }

        // ========================================================================
    }

    public class LogFileListView
    {
        public string FileName { get; set; }
        public string Date { get; set; }
        public string FilePath { get; set; }
        public LogFileListView(string name, string date, string path)
        {
            FileName = name;
            Date = date;
            FilePath = path;
            Data.Instance.init_loglistview.Add(this);
        }
    }
}