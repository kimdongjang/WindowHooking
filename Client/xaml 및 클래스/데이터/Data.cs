using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Client
{
    public class Data
    {
        #region 싱글톤
        static Data instance = null;
        static readonly object padlock = new Object();
        public static Data Instance
        {
            get
            {
                lock(padlock)
                {
                    if(instance == null)
                    {
                        instance = new Data();
                    }
                    return instance;
                }
            }
        }
        public Data()
        {
            instance = this;
            //InitializeComponent();
        }
        #endregion

        // =========================================================================
        // 녹화 목록 리스트 박스 :: LogXamlList
        // SLog 폴더에 저장된 리스트 박스 :: SaveXamlList
        public List<string> Rec_list = new List<string>();       // 녹화 리스트 --> Record버튼을 눌렀을때 추가되는 리스트
        public List<string> Play_list = new List<string>();      // 재생 리스트 --> Play버튼을 눌렀을때 실행되는 리스트
        public List<string> ModRec_list = new List<string>();    // 모듈 녹화 리스트 --> SampleModule에서 녹화했을 때 추가되는 리스트
        public List<string> Log_list = new List<string>();       // 로그 목록 리스트
        public List<string> LoadLog_list = new List<string>();   // 불러오기(파일찾기)한 로그 목록 리스트
        public List<string> Clipboard_list = new List<string>(); // 클립 보드 리스트
        // =========================================================================
        // 데이터바인딩
        public List<InitMemo> initmemo = new List<InitMemo>();
        public List<InitImage> initimage = new List<InitImage>();
        public List<InitPlaySetting> initplaysetting = new List<InitPlaySetting>();
        public List<LogFileListView> init_loglistview = new List<LogFileListView>();
        public List<InitDBLogList> initdbloglist = new List<InitDBLogList>();
        // =========================================================================
        public string Rec_Starttime; // 녹화한 현재 시간
        public string Rec_Endtime; // 녹화한 현재 시간
        public string Rec_Comment; // 녹화 종료 시 로그 설명
        // =========================================================================
        public int screenIndex = 0;              // 이미지 저장 인덱스 변수
        public string screenpath = "";           // 이미지 저장 경로 변수 
        public bool RecSetting_Ondialog = false; // 레코딩 녹화/실행 시 부가기능(RecSet, Edit 창 등)한 번만 실행되게 하는 변수
        public bool Is_ImagePathChecked = false; // 이미지 적용 여부 확인

        // 녹화할 때 한영키나 캡스락키를 구분하기 위한 키 제한
        public string[] exceptionkey = { "Tab", "Capital", "LeftShift", "LShiftKey", "LWin", "System", "Apps", "HanjaMode", 
                                     "Left", "Right", "Up", "Down", "NumLock", "RightShift", "RShiftKey","Home", 
                                     "PageUp", "End", "Next", "Subtract", "Add", "Divide", "Multiply","LControlKey", "RControlKey", 
                                     "Back", "Delete", "LeftCtrl", "Clear", "Space", "Return", "RShiftKey", "Oetilde",
                                        "OemQuestion","Oemplus", "OemMinus", "Oem5", "Oem6", "Oem7", "Oem1",
                                        "OemOpenBrackets", "Oemtilde", "Oemcomma", "OemPeriod", "LControlKey",
                                       "D1", "D2", "D3", "D4", "D5", "D6", "D7", "D8", "D9", "D0",
                                       "F1", "F2", "F3", "F4", "F5", "F6", "F7", "F8", "F9", "F10", "F11", "F12",
                                       "NumPad0", "NumPad1", "NumPad2", "NumPad3", "NumPad4", "NumPad5",
                                       "NumPad6","NumPad7", "NumPad8", "NumPad9","PrintScreen"};
        // 녹화 실행 및 녹화를 위한 단축키 제한
        public string[] limitationkey = { "Tab", "Capital", "LeftShift", "LWin", "Apps", "HanjaMode", 
                                     "Left", "Right", "Up", "Down", "NumLock", "RightShift", "Home", 
                                     "PageUp", "End", "Next", "Subtract", "Add", "Divide", "Multiply", 
                                     "Back", "Delete", "LeftCtrl", "Clear", "Space", "Return", "RShiftKey", "Oetilde"};
        public List<VirtualKeys> vKeyList = new List<VirtualKeys>();
        // =========================================================================
        public List<ChannelDialog> MyChannelList = new List<ChannelDialog>();
        public List<ChannelList> ChannelList = new List<ChannelList>();
        public List<MyInformation> MyCient = new List<MyInformation>();
        // =========================================================================
        // 회원정보
        public string myIp = "";
        public string myId = "";
        public List<string> myChannelName = new List<string>();
        public List<string> myChannelIndex = new List<string>();
        public int myChannelheader = 0;
        public string mybeforeChannel = "";
        // =========================================================================
        // 채널 다중 접속을 위한 초기화 변수
        public bool Init_Check = false;
        public int Init_Header = 0;
        public string[] Init_IndexList; // 접속된 채널들
        // =========================================================================
        public string exam_cname;
        // =========================================================================
        // 서버에게 파일을 전송하기 전, 서버의 승낙 여부를 대기하며 이곳에 파일 데이터를 저장해놓음
        // 채널 파일 관련
        public string uploadfile_cidx = ""; // 업로드할 채널
        public string filePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\SLog\"); // 파일 경로
        // =========================================================================
        // 개인 Log파일 관련
        public string MyfilePath = ""; // 내 업로드할 파일 경로
        public string DeleteFileName = "";
        public string myFileInfo = "";
        // =========================================================================
        public string ftp_myfilename = "";
        // =========================================================================
        public bool ftpOn = false; // 채널에서 사용하는 FTP파일전송 창 관련
        public int count = 2; // 채널 입장 시 사용하는 Tab Selectindex count

        public string GetMyChannelIndex(string cname) // 현재 자신이 접속해 있는 채널의 인덱스를 반환
        {
            for (int i = 0; i < myChannelName.Count; i++)
            {
                if (myChannelName[i] == cname)
                {
                    return myChannelIndex[i];
                }
            }
            return "";
        }

        // =========================================================================
        public string SearchChannelName(string cindex) // 모든 채널에서 인덱스와 일치하는 이름을 반환함
        {
            for (int i = 0; i < ChannelList.Count; i++)
            {
                if (ChannelList[i].ChannelIndex == cindex)
                {
                    return ChannelList[i].ChannelName;
                }
            }
            return "";
        }

        public string SearchChannelIndex(string cname) // 모든 채널에서 인덱스와 일치하는 이름을 반환함
        {
            for (int i = 0; i < ChannelList.Count; i++)
            {
                if (ChannelList[i].ChannelName == cname)
                {
                    return ChannelList[i].ChannelIndex;
                }
            }
            return "";
        }
        public string SearchChannelAdmin(string cname) // 모든 채널에서 이름과 일치하는 관리자를  반환함
        {
            for (int i = 0; i < ChannelList.Count; i++)
            {
                if (ChannelList[i].ChannelName == cname)
                {
                    return ChannelList[i].ChannelAdmin;
                }
            }
            return "";
        }
        public string SearchChannelAdmin_idx(string cindex) // 모든 채널에서 인덱스와 일치하는 관리자를 반환함
        {
            for (int i = 0; i < ChannelList.Count; i++)
            {
                if (ChannelList[i].ChannelIndex == cindex)
                {
                    return ChannelList[i].ChannelAdmin;
                }
            }
            return "";
        }
    }
}
