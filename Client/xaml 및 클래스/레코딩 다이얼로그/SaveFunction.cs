using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Forms;
using System.Xml;


namespace Client
{
    class SaveFunction
    {
        #region 싱글톤
        static SaveFunction instance = null;
        static readonly object padlock = new Object();
        public static SaveFunction Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new SaveFunction();
                    }
                    return instance;
                }
            }
        }
        public SaveFunction()
        {
            instance = this;
        }
        #endregion
        
        private Data dt_class = Data.Instance;
        private KeyboardHooking kh_class = KeyboardHooking.Instance;

        // 자동 저장 관련 전역 변수
        public string g_filepathname = "";    // 폴더 경로 + 파일 이름 + 확장자
        public string g_filepath = "";        // 폴더 경로
        public string g_filenameexceptxml;    // 확장자를 제외한 파일이름
        public string g_filename = "";           // 파일 이름 + 확장자

        public string autosave_filename_for_delete = "";          // log(n).xml 자동저장 파일이름 임시저장 (LogSaveSetting에서 해당파일 삭제로 쓰임)
        
        // RecSet창의 초기설정값 TextBox에 입력하는 값을 받기 위한 전역변수 
        public string InitSet = "";                    

        // 기타 변수
        public bool isSavelistClick = false;
        public string playinit_filename = "";
        public string logplay_start_filename = "";       // 로그 실행 할 때 선택한 로그의 파일 이름의 상태를 읽어오기 위한 변수  

        // 모듈 관련 전역변수
        public string module_savename = "";     // 모듈 로그 파일 이름
        public string modRec_folderpath = "";   // 모듈 로그 파일 경로

        // 이미지나 pauseEvent Xml 저장 시
        int image_count = 0;
        int pause_count = -1; // pauseEvent가 한 번에 녹화데이터에 두 번 일어나기 때문에 -1부터 시작

        #region 로그 리스트뷰 최신화
        public void InitRecentFileLoad() // 최근 불러온 파일 목록을 새로고침
        {
            try
            {
                dt_class.LoadLog_list.Clear();

                XmlTextReader reader = new XmlTextReader("Recent.xml");

                while (reader.Read())
                {
                    if (reader.LocalName.Contains("Recent"))
                    {
                        string path = reader.ReadElementContentAsString();
                        dt_class.LoadLog_list.Add(path.ToString());
                        //MainRecording.Instance.RecentXamlList.Items.Add(str.ToString().Trim());
                        //MainRecording.Instance.xaml_LogList.Items.Add(new LogFileListView(System.IO.Path.GetFileName(path), File.GetCreationTime(path).ToString(), path));
                    }
                }
            }
            catch
            {
                XmlDocument doc = new XmlDocument();
                XmlElement rec_xmllist = doc.CreateElement("Root"); // root 요소
                doc.AppendChild(rec_xmllist);
                doc.Save("Recent.xml");
                return;
            }
        }
        public void InitSaveFileLoad() // SLog폴더의 파일 목록 로드
        {
            MainWindow.Instance.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate()
            {
                // =====================================================================================
                // 데이터 컬렉션 클리어 영역
                if (MainRecording.Instance.xaml_LogList == null)
                    return;
                InitRecentFileLoad(); // 최근 불러온 파일 목록을 새로고침
                Data.Instance.Log_list.Clear();
                MainRecording.Instance.xaml_LogList.Items.Clear();
                Data.Instance.init_loglistview.Clear();
                // =====================================================================================


                string sDirPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\SLog\");
                DirectoryInfo di = new DirectoryInfo(sDirPath);
                // ==================================================================================
                // SLog폴더안에 있는 파일들을 가져옴
                foreach (var item in di.GetFiles())
                {
                    if (item.Name.Contains(".xml"))
                    {
                        Data.Instance.Log_list.Add(sDirPath + item.Name);
                    }
                }
                // ==================================================================================
                // 최근 불러온 목록과 SLog 폴더에 있는 목록을 모두 가져와서 리스트 뷰에 출력
                for (int i = 0; i < Data.Instance.Log_list.Count; i++)
                {
                    string path = Data.Instance.Log_list[i];
                    MainRecording.Instance.xaml_LogList.Items.Add(new LogFileListView(System.IO.Path.GetFileName(path), File.GetCreationTime(path).ToString(), path));
                }
            });
        }
        #endregion

        #region 로그 저장 폴더생성
        public void SLogCreateFolder() // 사용자의 "내 문서"를 읽어와 "SLog"라는 폴더가 있는지 없는지 검색하고, 없다면 새로 SLog 폴더 생성
        {
            // ===========================================================
            // 사용자의 "내 문서"를 읽어와 "SLog"라는 폴더가 있는지 없는지 검색하고, 없다면 새로 생성한다.
            string sDirPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\SLog");
            DirectoryInfo di = new DirectoryInfo(sDirPath);
            if (di.Exists == false)
            {
                di.Create();
            }
        }
        #endregion
        #region 인덱스를 이용한 녹화파일 자동저장
        public void GetSaveFileIndex() // 녹화를 시작할때 자동적으로 폴더를 검색해 폴더를 생성하고, 파일을 검색해 지정 인덱스로 xml문서를 생성
        {                                      // 즉, 녹화를 시작하면 바로 로그 파일을 생성한다는 의미
            // Log"0"번째 파일을 가리킴
            int file_count = 0;
            // 사용자의 내문서 경로를 가져온다.
            g_filepath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\SLog\";

            DirectoryInfo di = new DirectoryInfo(g_filepath);
            if (di.Exists == false)
            {
                di.Create();
            }
            else
            {
                int file_temp = 0;
                int file_compare = 0;
                // .xml이 포함된 로그 파일을 가져와 카운트를 증가시킴
                foreach (var item in di.GetFiles())
                {
                    if (item.Name.Contains(".xml"))
                    {
                        try
                        {
                            file_compare = int.Parse(Regex.Replace(item.Name, @"\D", "")); // 파일의 숫자만 compare변수에 가져옴

                            // 처음 읽은 로그 파일과 나중에 읽은 로그 파일의 인덱스가 1이상 차이나는지 확인
                            // 순차적으로 1씩 잘 저장이 되었으면 true
                            // 만약 로그1과 로그6번 이런 식으로 빈자리가 생긴다면!
                            if (CompareLogFileIndex(file_compare, file_temp) == true)
                            {
                                file_temp = file_compare;
                                file_count++;
                            }
                            else
                            {
                                file_count++;
                                break;
                            }
                        }
                        catch
                        {
                        }
                    }
                }
                g_filename = "log" + file_count + ".xml";
                g_filenameexceptxml = "log" + file_count;
                g_filepathname = g_filepath + g_filename; // 파일 경로 + 파일 이름
                autosave_filename_for_delete = g_filepath + g_filename;
                // ==========================================================
                // 파일 이름과 인덱스를 생성한 후, xml문서를 생성함
                XmlDocument doc = new XmlDocument();
                XmlElement rec_xmllist = doc.CreateElement("Root"); // root 요소
                doc.AppendChild(rec_xmllist);
                doc.Save(g_filepathname);
                RecordSetting.Instance.recordSave_onoff = true;
            }
        }
        private bool CompareLogFileIndex(int a, int b) // 처음 읽은 로그 파일과 나중에 읽은 로그 파일의 인덱스가 1이상 차이나는지 확인
        {
            if ((a - b) == 1)
            {
                return true;
            }
            else if (a == 0 && b == 0)
            {
                return true;
            }
            else
                return false;
        }
        #endregion
        #region 로그 파일 삭제
        private void DeleteRecentLog(int SelectedIndex)
        {
            try
            {
                XmlDocument docu = new XmlDocument();
                docu.Load("Recent.xml");

                XmlNode Node = docu.DocumentElement;
                XmlNode DeleteNode = Node.SelectSingleNode("Recent" + SelectedIndex.ToString());
                Node.RemoveChild(DeleteNode);

                docu.Save("Recent.xml");
            }
            catch
            {

            }
        }
        #endregion

        #region 레코드 리스트의 녹화 데이터를 xml로 저장
        public void XmlCreateRecordList(string filepath) // 레코드 리스트에 있는 데이터를 Xml에 저장
        {
            try
            { 
                XmlDocument doc = new XmlDocument();

                XmlElement rec_xmllist = doc.CreateElement("Root"); // root 요소

                XmlElement winSize = doc.CreateElement("WindowSize"); // 윈도우 해상도를 저장할 요소 생성
                winSize.InnerText = (SystemParameters.MaximizedPrimaryScreenWidth - 16) + "#" + (SystemParameters.MaximizedPrimaryScreenHeight + 24); // 윈도우 해상도 저장
                rec_xmllist.AppendChild(winSize); // 메인 요소에 winsSizw요소를 추가
                // ======================================================================================
                // 초기메모 패킷을 로그에 저장
                XmlElement FirstMemo = doc.CreateElement("InitMemo");
                rec_xmllist.AppendChild(FirstMemo);

                foreach (var item in Data.Instance.initmemo)
                {
                    InitMemo memo = (InitMemo)item;

                    XmlElement xRow = doc.CreateElement("Row");

                    XmlElement col1 = doc.CreateElement("col1");
                    col1.InnerText = memo.Col1;
                    XmlElement col2 = doc.CreateElement("col2");
                    col2.InnerText = memo.Col2;

                    xRow.AppendChild(col1);
                    xRow.AppendChild(col2);

                    FirstMemo.AppendChild(xRow);
                } 
                XmlElement DBSetting = doc.CreateElement("InitDBSetting");
                rec_xmllist.AppendChild(DBSetting);

                XmlElement ClientID = doc.CreateElement("ClientID");
                ClientID.InnerText = Data.Instance.myId;
                DBSetting.AppendChild(ClientID);

                XmlElement Today = doc.CreateElement("Today");
                Today.InnerText = Data.Instance.Rec_Starttime;
                DBSetting.AppendChild(Today);

                XmlElement Comment = doc.CreateElement("Comment");
                Comment.InnerText = Data.Instance.Rec_Comment;
                DBSetting.AppendChild(Comment);

                XmlElement FileName = doc.CreateElement("FileName");
                FileName.InnerText = Data.Instance.ftp_myfilename; 
                DBSetting.AppendChild(FileName);

                XmlElement FirstImage = doc.CreateElement("InitImg");
                rec_xmllist.AppendChild(FirstImage);

                // ======================================================================================
                // 일시정지 패킷을 로그에 저장
                XmlElement Pause = doc.CreateElement("Pause");     // Pause를 추가하기 위한 Pause 요소를 생성
                XmlElement Log;
                // ======================================================================================
                // 로그 저장시 마우스최적화 옵션이 켜져있을 경우
                //if (RecordSetting.Instance.is_mouseoptimize == true)
                //    this.MouseOptimize();

                // 현재 녹화된 데이터까지 xml 문서에 저장
                for (int i = 0; i < dt_class.Rec_list.Count; i++)
                {
                    if (dt_class.Rec_list[i].Contains("Pause"))
                    {
                        if (dt_class.Rec_list[i - 1] == "LControlKey")
                        {
                            dt_class.Rec_list.RemoveAt(i - 1);
                        }
                        Pause = doc.CreateElement("Pause" + pause_count.ToString());
                        rec_xmllist.AppendChild(Pause);
                        pause_count++;
                    }
                    #region 메모 다이얼로그 창이 열렸을 때의 이벤트
                    else if (dt_class.Rec_list[i].Contains("M＆")) // 일시정지 패킷 아래에 MemoData를 로그에 저장
                    {
                        XmlElement MemoData = doc.CreateElement("MemoData");            // Memo를 추가하기 위한 Memo 요소를 생성
                        MemoData.InnerText = Data.Instance.Rec_list[i];
                        Pause.AppendChild(MemoData);
                    }
                    else if (dt_class.Rec_list[i].Contains("↔"))  // 일시정지 패킷 아래에 MemoLocation를 로그에 저장
                    {
                        XmlElement MemoLocation = doc.CreateElement("MemoLocation");   // Memo를 추가하기 위한 MemoLocation 요소를 생성
                        MemoLocation.InnerText = Data.Instance.Rec_list[i];
                        Pause.AppendChild(MemoLocation);
                    }
                    #endregion
                    else if (dt_class.Rec_list[i].Contains("Image"))  // 일시정지 패킷 아래에 Image를 로그에 저장
                    {
                        XmlElement Image = doc.CreateElement("Image" + image_count.ToString());
                        Image.InnerText = Data.Instance.Rec_list[i].Replace("Image＆", "");
                        Pause.AppendChild(Image);
                        image_count++;
                    }
                    #region 파일 불러오기 같은 다이얼로그 창이 열렸을때의 이벤트
                    else if (dt_class.Rec_list[i].Contains("D＆"))
                    {
                        XmlElement DialogData = doc.CreateElement("DialogData");                    
                        DialogData.InnerText = Data.Instance.Rec_list[i];
                        Pause.AppendChild(DialogData);
                    }
                    else if (dt_class.Rec_list[i].Contains("♤"))
                    {
                        XmlElement DialogLocation = doc.CreateElement("DialogLocation");           
                        DialogLocation.InnerText = Data.Instance.Rec_list[i];
                        Pause.AppendChild(DialogLocation);
                    }
                    #endregion

                    else if (dt_class.Rec_list[i].Contains("InitImg")) // 초기이미지 패킷 아래에 InitImage를 로그에 저장
                    {
                        XmlElement InitImg = doc.CreateElement("InitImg");
                        InitImg.InnerText = Data.Instance.Rec_list[i].Replace("InitImg&", "");
                        FirstImage.AppendChild(InitImg);
                    }  
                    else
                    {
                        Log = doc.CreateElement("L" + (i).ToString()); // 녹화한 숫자의 끝까지 XML에 요소를 추가하여 저장한다.
                        Log.InnerText = Data.Instance.Rec_list[i]; // log라는 Xml요소에 녹화 데이터를 추가
                        rec_xmllist.AppendChild(Log); // Xml 요소를 LogList요소에 추가
                    }
                }
                doc.AppendChild(rec_xmllist); // Document에 root요소를 추가

                if (filepath == "")
                    return;
                doc.Save(filepath); // 인덱스에서 지정한 파일 경로 + 파일 이름으로 저장
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
        }
        #endregion
        #region 일시정지 시 xml에 <Pause>, <PauseEnd> 삽입
        public void PauseXmlAdd() 
        {
            dt_class.Rec_list.Add("Pause");
            XmlCreateRecordList(g_filepathname);
        }
       
        public void PauseEndXmlAdd()
        {
            dt_class.Rec_list.Add("PauseEnd");
            XmlCreateRecordList(g_filepathname);
        }
        #endregion
        #region 환경설정 추가기능 - 마우스 최적화
        private void MouseOptimize() // 마우스 움직임을 최적화하여 xml에 저장
        {
            int i = 0;
            bool logdelete = true;

            // 1. Rec_list의 Count까지 반복문을 돈다.
            for (i = 0; i < Data.Instance.Rec_list.Count; i++)
            {
                // 2. 마우스의 시작점을 찾는다.
                if (Data.Instance.Rec_list[i].Contains('§'))
                {
                    // 3. Rec_list[i + 1] == "LeftDown"이라면 삭제 중단
                    if (Data.Instance.Rec_list[i + 1] == "LeftDown")
                    {
                        logdelete = false;
                    }
                    if (i >= 3) // 인덱스 배열범위 예외처리
                    {
                        // 4. Rec_list[i - 1] == "LeftUp"이라면 삭제 진행
                        if (Data.Instance.Rec_list[i - 2] == "LeftUp")
                        {
                            logdelete = true;
                        }
                    }
                    if (logdelete == true)
                    {
                        Data.Instance.Rec_list.RemoveAt(i);
                        i--;
                    }
                }
            }
        }
        #endregion

        #region 최근 불러온 목록에 xml 파일 추가 및 초기화
        public void LogFileLoadFunction() // 불러오기
        {
            //System.Windows.Forms.FileDialog openFileDlg = new System.Windows.Forms.OpenFileDialog(); // 파일 찾아보기 다이얼로그
            //openFileDlg.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

            //if (openFileDlg.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            //    return;
            //string openedFile = openFileDlg.FileName;

            //dt_class.xmlLog_list.Add(openedFile);

            //// ========================================================================
            //// 파일을 불러올 때, 스크린 샷의 호환을 위해 OpenImage클래스를 생성

            //MainRecording.Instance.OpenImage_Dlg = new ImageChecking();
            //MainRecording.Instance.OpenImage_Dlg.Show();
            //MainRecording.Instance.OpenImage_Dlg.SearchLogFileImage(openedFile); // 로그 파일 안의 이미지를 검색함

            //SetLoadListSave(openedFile);
            //GetLoadListName();
        }
        public void SetLoadListSave(string file) // 불러온 로그 파일을 최근 목록에 추가
        {
            //try
            //{
            //    XmlDocument docu = new XmlDocument();

            //    int i = 0;

            //    XmlElement rec_xmllist = docu.CreateElement("Root"); // root 요소

            //    XmlElement RecentFile;
            //    for (i = 0; i < dt_class.xmlLog_list.Count - 1; i++)
            //    {
            //        string str = dt_class.xmlLog_list[i].ToString();
            //        RecentFile = docu.CreateElement("Recent" + i.ToString());
            //        RecentFile.InnerText = str;
            //        rec_xmllist.AppendChild(RecentFile);
            //    }

            //    XmlElement LastRecentFile = docu.CreateElement("Recent" + i.ToString());
            //    LastRecentFile.InnerText = file;
            //    rec_xmllist.AppendChild(LastRecentFile); // 메인 요소에 winsSizw요소를 추가

            //    docu.AppendChild(rec_xmllist);
            //    // ======================================================================================
            //    docu.Save("Recent.xml"); // 인덱스에서 지정한 파일 경로 + 파일 이름으로 저장
            //}
            //catch 
            //{
            //}
        }
        public void GetLoadListName() // 최근 불러온 목록 초기화
        {
            // 갱신을 요구하는 함수 (GetLoadListName())에서 LoadLog 컬렉션을 초기화한후 현재 xml_LogList를 순회하여 
            // SLog가 포함되지 않은 로그경로들을 모두 모아서 LoadLog 컬렉션에 추가함.
            Data.Instance.LoadLog_list.Clear();
            for(int i =0;i<MainRecording.Instance.xaml_LogList.Items.Count;i++)
            {
                if(!Data.Instance.init_loglistview[i].FilePath.Contains("SLog"))
                {
                    Data.Instance.LoadLog_list.Add(Data.Instance.init_loglistview[i].FilePath);
                }
            }
        }
        public void RecentLog_UpdateToXml() // 불러오기(찾아보기)한 로그파일을 프로그램 내부 데이터 xml에 저장함
        {
            try
            {
                XmlDocument docu = new XmlDocument();

                int i = 0;

                XmlElement rec_xmllist = docu.CreateElement("Root"); // root 요소

                XmlElement RecentFile;
                for (i = 0; i < Data.Instance.LoadLog_list.Count; i++)
                {
                    RecentFile = docu.CreateElement("Recent" + i.ToString());
                    RecentFile.InnerText = Data.Instance.LoadLog_list[i];
                    rec_xmllist.AppendChild(RecentFile);
                }
                docu.AppendChild(rec_xmllist);
                // ======================================================================================
                docu.Save("Recent.xml"); // 인덱스에서 지정한 파일 경로 + 파일 이름으로 저장
            }
            catch
            {
            }
        }
        #endregion
        #region 저장 목록 리스트뷰 선택 이벤트
        public void SaveListClickEvent() // 저장 목록 선택 이벤트
        {
            isSavelistClick = true;
            if (kh_class.recording_starting == true)
                return;
            if (kh_class.pausing_starting == true)
                return;
            if (kh_class.logplaying_pausing == true)
                return;

            try
            {
                dt_class.Play_list.Clear();
                dt_class.Rec_list.Clear();
                MainRecording.Instance.LogXamlList.Items.Clear();

                //if (MainRecording.Instance.SaveXamlList.SelectedItem == null)
                //    return;
                if (MainRecording.Instance.xaml_LogList.SelectedValue == null)
                {
                    return;
                }

                // =============================================================================

                // 저장 로그 파일 경로 =================================
                String sPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\SLog\";
                string filepathname = Data.Instance.init_loglistview[MainRecording.Instance.xaml_LogList.SelectedIndex].FilePath;
                string filename = Data.Instance.init_loglistview[MainRecording.Instance.xaml_LogList.SelectedIndex].FileName;
                logplay_start_filename = sPath + filename;
                XmlTextReader reader = new XmlTextReader(filepathname); // 선택한 로그 파일을 읽는다.
                string read_string;

                while (reader.Read())
                {
                    #region 노드 검색 제어문(일시정지, 초기설정 등)
                    switch (reader.LocalName)
                    {
                        case "WindowSize":
                            read_string = reader.ReadElementContentAsString(); 
                            string[] winSize = read_string.Split('#'); // 윈도우 해상도를 가져옴
                            MainRecording.Instance.PlayLog_Width = double.Parse(winSize[0]);
                            MainRecording.Instance.PlayLog_Height = double.Parse(winSize[1]);
                            break;
                        case "Header":
                            read_string = reader.ReadElementContentAsString();
                            System.Windows.Forms.MessageBox.Show(read_string);
                            break;
                        case "MemoData":
                            read_string = reader.ReadElementContentAsString();
                            MainRecording.Instance.LogXamlList.Items.Add("메모 : " + read_string);
                            dt_class.Play_list.Add("M&" + read_string);
                            break;
                        case "MemoLocation":
                            read_string = reader.ReadElementContentAsString();
                            string[] str = read_string.Split('↔');
                            MainRecording.Instance.LogXamlList.Items.Add("메모 위치 : " + str[0] + "," + str[1]);
                            dt_class.Play_list.Add(read_string);
                            break;
                        case "Image":
                            read_string = reader.ReadElementContentAsString();
                            MainRecording.Instance.LogXamlList.Items.Add("이미지 경로 : " + read_string);
                            dt_class.Play_list.Add("Image&" + read_string);
                            break;
                        case "DialogData":
                            read_string = reader.ReadElementContentAsString();
                            MainRecording.Instance.LogXamlList.Items.Add("다이얼로그 : " + read_string);
                            dt_class.Play_list.Add("D&" + read_string);
                            break;
                        case "DialogLocation":
                            read_string = reader.ReadElementContentAsString();
                            string[] str1 = read_string.Split('♤');
                            MainRecording.Instance.LogXamlList.Items.Add("다이얼로그 위치 : " + str1[0] + "," + str1[1]);
                            dt_class.Play_list.Add(read_string);
                            break;
                    }
                    #endregion
                    // 로그 데이터가 들어있을 경우 =================================================
                    #region 로그 데이터 제어문
                    if (reader.LocalName.Contains("L"))
                    {
                        read_string = reader.ReadElementContentAsString();
                        if (read_string.Contains('§'))
                        {
                            string[] pos = read_string.Split('§');
                            MainRecording.Instance.LogXamlList.Items.Add("X좌표 : " + pos[0] + " Y좌표 : " + pos[1]);
                            dt_class.Play_list.Add(pos[0] + "§" + pos[1]);
                        }
                        else if (read_string.Contains("LeftDown"))
                        {
                            MainRecording.Instance.LogXamlList.Items.Add(read_string);
                            dt_class.Play_list.Add("LeftDown");
                        }
                        else if (read_string.Contains("LeftUp"))
                        {
                            MainRecording.Instance.LogXamlList.Items.Add(read_string);
                            dt_class.Play_list.Add("LeftUp");
                        }
                        else if (read_string.Contains("RightDown"))
                        {
                            MainRecording.Instance.LogXamlList.Items.Add(read_string);
                            dt_class.Play_list.Add("RightDown");
                        }
                        else if (read_string.Contains("RightUp"))
                        {
                            MainRecording.Instance.LogXamlList.Items.Add(read_string);
                            dt_class.Play_list.Add("RightUp");
                        }
                        else if (read_string.Contains("Double"))
                        {
                            MainRecording.Instance.LogXamlList.Items.Add(read_string);
                            dt_class.Play_list.Add("Double");
                        }
                        else if (read_string.Contains('※'))
                        {
                            MainRecording.Instance.LogXamlList.Items.Add(read_string);
                            dt_class.Play_list.Add("※" + read_string);
                        }
                        else if (read_string.Contains("■"))
                        {
                            string[] kana_data = read_string.Split('■');
                            MainRecording.Instance.LogXamlList.Items.Add("한영 키 상태 값 : " + kana_data[1]);

                            dt_class.Play_list.Add("■" + kana_data[1]);
                        }
                        else if (read_string.Contains("□"))
                        {
                            string[] caps_data = read_string.Split('□');
                            MainRecording.Instance.LogXamlList.Items.Add("캡스락 상태 값 : " + caps_data[1]);

                            dt_class.Play_list.Add("□" + caps_data[1]);
                        }
                        else if (read_string.Contains("▲"))
                        {
                            // 문자를 읽을 경우 ================================================
                            string[] key_data = read_string.Split('▲');
                            string[] kana_caps_data = key_data[1].Split('△');

                            MainRecording.Instance.LogXamlList.Items.Add("Keyboard : " + key_data[0]);
                            dt_class.Play_list.Add(read_string);
                        }
                        else
                        {
                            // 문자를 읽을 경우 ================================================
                            MainRecording.Instance.LogXamlList.Items.Add("Keyboard : " + read_string);
                            dt_class.Play_list.Add(read_string);
                        }
                    }
                    #endregion

                    #region 캡쳐본
                    if (reader.LocalName.Contains("Image"))
                    {
                        read_string = reader.ReadElementContentAsString();
                        MainRecording.Instance.LogXamlList.Items.Add("이미지 경로 : " + read_string);
                        dt_class.Play_list.Add("Image&" + read_string);
                    }
                    #endregion

                    #region 일시정지 제어문
                    else if(reader.LocalName.Contains("Pause"))
                    {
                        MainRecording.Instance.LogXamlList.Items.Add("PauseEvent");
                        dt_class.Play_list.Add("PauseEvent");
                    }
                    #endregion

                }

                if (MainRecording.Instance.OpenImage_Dlg == null)
                {
                    MainRecording.Instance.OpenImage_Dlg = new ImageChecking();
                }
                MainRecording.Instance.OpenImage_Dlg.SearchLogFileImage(filepathname); // 로그 파일 안의 이미지를 검색함=

            }
            catch
            {
                //MainRecording.Instance.xaml_LogList.Items.RemoveAt(MainRecording.Instance.xaml_LogList.SelectedIndex);
            }
        }

        public void RecentListClickEvent() // 최근 목록 선택 이벤트
        {
            //try
            //{
            //    if (kh_class.recording_starting == true)
            //        return;
            //    if (kh_class.pausing_starting == true)
            //        return;
            //    if (kh_class.logplaying_pausing == true)
            //        return;

            //    dt_class.Play_list.Clear();
            //    dt_class.Rec_list.Clear();
            //    MainRecording.Instance.LogXamlList.Items.Clear();

            //    if (MainRecording.Instance.RecentXamlList.SelectedItem == null)
            //        return;
            //    // 불러올 로그 파일 경로

            //    MainRecording.Instance.SaveXamlList.SelectedIndex = -1;

            //    string lPath = MainRecording.Instance.RecentXamlList.SelectedItem.ToString(); // 선택한 파일의 이름과 경로
            //    logplay_start_filename = lPath;
            //    // =============================================================================
            //    XmlTextReader reader = new XmlTextReader(lPath); // 선택한 로그 파일을 읽는다.
            //    string read_string;

            //    while (reader.Read())
            //    {
            //        #region 노드 검색 제어문(일시정지, 초기설정 등)
            //        switch (reader.LocalName)
            //        {
            //            case "WindowSize":
            //                read_string = reader.ReadElementContentAsString();

            //                string[] winSize = read_string.Split('#'); // 윈도우 해상도를 가져옴
            //                MainRecording.Instance.PlayLog_Width = double.Parse(winSize[0]);
            //                MainRecording.Instance.PlayLog_Height = double.Parse(winSize[1]);
            //                break;
            //            case "Pause":
            //                MainRecording.Instance.LogXamlList.Items.Add("PauseEvent");
            //                dt_class.Play_list.Add("PauseEvent");
            //                break;
            //            case "Header":
            //                read_string = reader.ReadElementContentAsString();
            //                System.Windows.Forms.MessageBox.Show(read_string);
            //                break;
            //            case "MemoData":
            //                read_string = reader.ReadElementContentAsString();
            //                MainRecording.Instance.LogXamlList.Items.Add("메모 : " + read_string);
            //                dt_class.Play_list.Add("M&" + read_string);
            //                break;
            //            case "MemoLocation":
            //                read_string = reader.ReadElementContentAsString();
            //                string[] str = read_string.Split('↔');
            //                MainRecording.Instance.LogXamlList.Items.Add("메모 위치 : " + str[0] + "," + str[1]);
            //                dt_class.Play_list.Add(read_string);
            //                break;
            //            case "Image":
            //                read_string = reader.ReadElementContentAsString();
            //                MainRecording.Instance.LogXamlList.Items.Add("이미지 경로 : " + read_string);
            //                dt_class.Play_list.Add("Image&" + read_string);
            //                break;
            //            case "DialogData":
            //                read_string = reader.ReadElementContentAsString();
            //                MainRecording.Instance.LogXamlList.Items.Add("다이얼로그 : " + read_string);
            //                dt_class.Play_list.Add("D&" + read_string);
            //                break;
            //            case "DialogLocation":
            //                read_string = reader.ReadElementContentAsString();
            //                string[] str1 = read_string.Split('♤');
            //                MainRecording.Instance.LogXamlList.Items.Add("다이얼로그 위치 : " + str1[0] + "," + str1[1]);
            //                dt_class.Play_list.Add(read_string);
            //                break;
            //        }
            //        #endregion
            //        // 로그 데이터가 들어있을 경우 =================================================
            //        #region 로그 데이터 제어문
            //        if (reader.LocalName.Contains("L"))
            //        {
            //            read_string = reader.ReadElementContentAsString();
            //            if (read_string.Contains('§'))
            //            {
            //                string[] pos = read_string.Split('§');
            //                MainRecording.Instance.LogXamlList.Items.Add("X좌표 : " + pos[0] + " Y좌표 : " + pos[1]);
            //                dt_class.Play_list.Add(pos[0] + "§" + pos[1]);
            //            }
            //            else if (read_string.Contains("LeftDown"))
            //            {
            //                MainRecording.Instance.LogXamlList.Items.Add(read_string);
            //                dt_class.Play_list.Add("LeftDown");
            //            }
            //            else if (read_string.Contains("LeftUp"))
            //            {
            //                MainRecording.Instance.LogXamlList.Items.Add(read_string);
            //                dt_class.Play_list.Add("LeftUp");
            //            }
            //            else if (read_string.Contains("RightDown"))
            //            {
            //                MainRecording.Instance.LogXamlList.Items.Add(read_string);
            //                dt_class.Play_list.Add("RightDown");
            //            }
            //            else if (read_string.Contains("RightUp"))
            //            {
            //                MainRecording.Instance.LogXamlList.Items.Add(read_string);
            //                dt_class.Play_list.Add("RightUp");
            //            }
            //            else if (read_string.Contains("Double"))
            //            {
            //                MainRecording.Instance.LogXamlList.Items.Add(read_string);
            //                dt_class.Play_list.Add("Double");
            //            }
            //            else if (read_string.Contains('※'))
            //            {
            //                MainRecording.Instance.LogXamlList.Items.Add(read_string);
            //                dt_class.Play_list.Add("※" + read_string);
            //            }
            //            else if (read_string.Contains("■"))
            //            {
            //                string[] kana_data = read_string.Split('■');
            //                MainRecording.Instance.LogXamlList.Items.Add("윈도우 핸들 : " + kana_data[0] + " / " + kana_data[1]);

            //                dt_class.Play_list.Add(kana_data[0] + "■" + kana_data[1]);
            //            }

            //            else
            //            {
            //                // 문자를 읽을 경우 ================================================
            //                MainRecording.Instance.LogXamlList.Items.Add(read_string);
            //                dt_class.Play_list.Add(read_string);
            //            }
            //        }
            //        #endregion
            //    }
            //    if (MainRecording.Instance.OpenImage_Dlg == null)
            //    {
            //        MainRecording.Instance.OpenImage_Dlg = new ImageChecking();
            //    }
            //    MainRecording.Instance.OpenImage_Dlg.SearchLogFileImage(lPath); // 로그 파일 안의 이미지를 검색함
            //}
            //catch (Exception ex)
            //{
            //    int SelectedIndex = MainRecording.Instance.RecentXamlList.SelectedIndex;

            //    dt_class.xmlLog_list.RemoveAt(SelectedIndex);
            //    MainRecording.Instance.RecentXamlList.Items.RemoveAt(SelectedIndex);
            //    DeleteRecentLog(SelectedIndex);

            //    System.Windows.Forms.MessageBox.Show(ex.Message + "해당 기록을 삭제합니다.");
            //}
        }
        #endregion

        #region 모듈
        public void SaveModuleRecording() // 샘플 모듈화 > 샘플을 임의의 폴더에 사용자가 지정한 이름으로 저장  
        {
            // 사용자의 내문서 경로를 가져온다.
            modRec_folderpath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\sLogModule\";
            DirectoryInfo di = new DirectoryInfo(modRec_folderpath);
            if (di.Exists == false)
            {
                di.Create();
            }
            module_savename = modRec_folderpath + SampleModule.Instance.mouduleName.Text + ".xml";
            // 파일 이름과 인덱스를 생성한 후, xml문서를 생성함
            XmlDocument doc = new XmlDocument();
            XmlElement modRec_xmllist = doc.CreateElement("Root"); // root 요소
            doc.AppendChild(modRec_xmllist);
            doc.Save(module_savename);
        }
        public void XmlCreateMouduleRecList() // SampleModule의 녹화기록을 xml에 저장 
        {
            try
            {
                XmlDocument doc = new XmlDocument();

                XmlElement rec_xmllist = doc.CreateElement("Root"); // root 요소

                XmlElement winSize = doc.CreateElement("WindowSize"); // 윈도우 해상도를 저장할 요소 생성
                winSize.InnerText = (SystemParameters.MaximizedPrimaryScreenWidth - 16) + "#" + (SystemParameters.MaximizedPrimaryScreenHeight + 24); // 윈도우 해상도 저장
                rec_xmllist.AppendChild(winSize); // 메인 요소에 winsSizw요소를 추가
                // ======================================================================================
                // 초기메모 패킷을 로그에 저장
                XmlElement FirstMemo = doc.CreateElement("InitMemo");
                rec_xmllist.AppendChild(FirstMemo);

                foreach (var item in Data.Instance.initmemo)
                {
                    InitMemo memo = (InitMemo)item;

                    XmlElement xRow = doc.CreateElement("Row");

                    XmlElement col1 = doc.CreateElement("col1");
                    col1.InnerText = memo.Col1;
                    XmlElement col2 = doc.CreateElement("col2");
                    col2.InnerText = memo.Col2;

                    xRow.AppendChild(col1);
                    xRow.AppendChild(col2);

                    FirstMemo.AppendChild(xRow);
                }

                XmlElement Log;
                for (int i = 0; i < dt_class.ModRec_list.Count; i++)
                {
                    Log = doc.CreateElement("L" + (i).ToString()); // 녹화한 숫자의 끝까지 XML에 요소를 추가하여 저장한다.
                    Log.InnerText = Data.Instance.ModRec_list[i]; // log라는 Xml요소에 녹화 데이터를 추가
                    rec_xmllist.AppendChild(Log); // Xml 요소를 LogList요소에 추가
                }
                doc.AppendChild(rec_xmllist);   // Document에 root요소를 추가
                doc.Save(module_savename);      // 인덱스에서 지정한 파일 경로 + 파일 이름으로 저장

                dt_class.ModRec_list.Clear();   // xml 저장 후 리스트 비워주기.
            }

            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }

        }
        #endregion

        #region 로그파일 삭제기능(보류)
        public void SaveListDeleteClickEvent()
        {
            //try
            //{
            //    if (isSavelistClick == false)
            //        System.Windows.MessageBox.Show("삭제할 로그파일을 선택해주세요");
            //    else
            //    {
            //        if (MainRecording.Instance.SaveXamlList.SelectedItem == null)
            //            return;
            //        // 저장 로그 파일 경로
            //        String sPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\SLog\";
            //        String filename = "";
            //        String name = MainRecording.Instance.SaveXamlList.SelectedItem.ToString(); // 선택한 파일의 수정된 날짜 이름

            //        DirectoryInfo di = new DirectoryInfo(sPath);
            //        foreach (var item in di.GetFiles())
            //        {
            //            if (item.CreationTime.ToString() == name) // 선택한 파일의 수정된 날짜와 해당 폴더 안의 파일의 날짜와 같은지 비교
            //            {
            //                filename = item.Name; // 파일 이름(log(n).xml)
            //                foreach (var item1 in di.GetFiles(filename)) // 해당 경로의 선택한 파일이름을 구해서
            //                {
            //                    item1.Delete();                          // 삭제
            //                    MainRecording.Instance.Refresh();
            //                }
            //            }
            //        }
            //    }
            //}
            //catch
            //{
            //}
        }
        #endregion
        #region 다른 이름으로 저장(보류)
        //public void NewCreateLogFile() // 새로운 로그 파일 생성(다른 이름으로 저장)
        //{
        //    SaveFileDialog savefile = new SaveFileDialog();

        //    //SaveFileDialog 창 설정
        //    savefile.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory); //기본 저장 경로
        //    savefile.Title = "새 매크로 생성"; //saveFileDialog 창 이름 설정
        //    savefile.Filter = "매크로 전용 문서(*.xml)|*.xml"; //파일 형식 부분
        //    savefile.DefaultExt = "xml"; // 기본 확장명
        //    savefile.AddExtension = true; //확장명 추가 여부

        //    if (savefile.ShowDialog() == System.Windows.Forms.DialogResult.OK) // 파일 저장 버튼을 누르고
        //    {
        //        if (savefile.FileName != "") // 파일 이름과 경로가 입력이 되었을때
        //        {
        //            XmlDocument doc = new XmlDocument();
        //            XmlElement rec_xmllist = doc.CreateElement("Root"); // root 요소
        //            doc.AppendChild(rec_xmllist);
        //            doc.Save(savefile.FileName);
        //            // Root라는 이름의 요소로 새 파일을 생성

        //            dt_class.Recent_list.Add(savefile.FileName); // 파일 생성 목록에 추가

        //            SetLoadListSave(savefile.FileName);
        //            GetLoadListName();
        //        }
        //    }
        //}
        #endregion
    }
}