using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Client
{
    /// <summary>
    /// Setting.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class RecordSetting : Window
    {
        #region 전역변수
        // 단축키 변경 다이얼로그
        ChangeDefaultKeyDialog chk;

        // Data폴더 정보 (레코딩 컨트롤키에 대한 정보를 저장)
        private string folderpath = Environment.CurrentDirectory + "/Setting/";
        private string filename = "data.txt";
        private string text = "";
        DirectoryInfo di;

        // 자동 저장 변수
        public System.Timers.Timer RealTimeSaveTimer;  // 실시간 저장을 위한 타이머 객체
        public bool recordSave_onoff = false;          // 자동 저장 시키기 위한 전역 변수, 녹화가 시작되면 자동저장을 위해 true로 바꾸고 녹화가 끝나면 false로 바꾼다.
        public bool is_autosavechecked;                // 컨트롤의 자동저장 체크박스와 연결된 변수
        public bool is_mouseoptimize;                  // 마우스 최적화 설정변수
        #endregion

        #region 싱글톤
        static RecordSetting single_control;
        public static RecordSetting Instance { get { return single_control; } }
        static RecordSetting()
        {
            single_control = new RecordSetting();
        }
        private RecordSetting()
        {
            InitializeComponent();
        }
        #endregion

        public void SettingInit() // 환경설정 초기화
        {
            // 컨트롤 초기화
            PathBox.Text = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\SLog\";
            Interval_Slider.Value = 10;
            Speed_Slider.Value = 10;

            // Data.txt의 기본키값들을 읽어온다.
            SettiingDefaultKey();

            // 설정창의 단축키 초기화
            RecordStartKey_TextBox.Text = KeyboardHooking.Instance.RecordStartKey.ToString();
            RecordStopKey_TextBox.Text = KeyboardHooking.Instance.LogPlayStopKey.ToString();
            RecordPlayKey_TextBox.Text = KeyboardHooking.Instance.LogPlayKey.ToString();
            RecordPlayStopKey_TextBox.Text = KeyboardHooking.Instance.RecordStopKey.ToString();
            RecordPauseKey_TextBox.Text = KeyboardHooking.Instance.PauseKey.ToString();
            ModuleRecordKey_TextBox.Text = KeyboardHooking.Instance.ModulLogKey.ToString();
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e) // 자동실행
        {
            RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
            //레지스트리 등록 할때
            if (registryKey == null)
            {
                registryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            }

            registryKey.SetValue("AutoRestart", System.Windows.Forms.Application.ExecutablePath.ToString());

            //.................................................................
            //레지스트리 삭제 할때
            if (registryKey.GetValue("MyApp") == null)
            {
                registryKey.DeleteValue("MyApp", false);
            }
        }

        private void Slider_PreviewMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e) // 슬라이더
        {
            IInputElement target = System.Windows.Input.Mouse.DirectlyOver;
            target = target as System.Windows.Controls.Control;
            if (target == null) { return; }
            if (!target.IsMouseCaptured) { return; }

            Slider slider = sender as Slider;
            if (slider == Interval_Slider)
            {
                int value = (int)Interval_Slider.Value;
            }
        }

        #region 단축키 설정
        private void SettiingDefaultKey() // data.txt에서 값을 읽어들이고 전역변수 및 컨트롤에 적용시킴
        {
            di = new DirectoryInfo(folderpath);

            if (di.Exists == false) // 프로그램을 처음 사용한다면 사용자는 레코드 기본키를 가진 파일을 생성하게 됨
            {
                CreateSettingDataFile(di);
            }

            else // 데이터가 존재한다면 파일에 있는 데이터를 읽어와서 컨트롤키를 키보드후킹의 전역변수와 레코드세팅의 텍스트박스에 설정해줌
            {
                text = System.IO.File.ReadAllText(folderpath + filename);
                UnifyData(text);
            }
        }

        private void Confirm_Click(object sender, RoutedEventArgs e) // 사용자가 환경설정을 변경후 적용시킬 경우 data.txt와 KeyboardHooking의 전역변수에 적용
        {
            //======================================================
            //수정된 키값을 data.txt에 적용
            di = new DirectoryInfo(folderpath);

            if (di.Exists == false) // 프로그램을 처음 사용한다면 사용자는 레코드 기본키를 갖게됨
            {
                CreateSettingDataFile(di);
            }

            else // 데이터가 남아있다면 수정된 키값으로 변경
            {
                text = "[녹화시작키]=" + RecordStartKey_TextBox.Text + "#";
                text += "[녹화정지키]=" + RecordStopKey_TextBox.Text + "#";
                text += "[녹화재생키]=" + RecordPlayKey_TextBox.Text + "#";
                text += "[재생정지키]=" + RecordPlayStopKey_TextBox.Text + "#";
                text += "[일시정지키]=" + RecordPauseKey_TextBox.Text + "#";
                text += "[모듈녹화키]=" + ModuleRecordKey_TextBox.Text + "#";
                text += "[자동저장]=" + AutoSave_CheckBox.IsChecked + "#";
                text += "[마우스최적화]=" + MouseOptimize_CheckBox.IsChecked;
            }

            // 파일 수정
            System.IO.File.WriteAllText(folderpath + filename, text, Encoding.Default);

            //======================================================
            // 수정된 키값을 KeyboardHooking.Instance의 키값에 적용
            for (int i = 0; i < Data.Instance.vKeyList.Count; i++)
            {
                if (RecordStartKey_TextBox.Text == Data.Instance.vKeyList[i].ToString())
                {
                    KeyboardHooking.Instance.RecordStartKey = Data.Instance.vKeyList[i];
                }



                else if (RecordStopKey_TextBox.Text == Data.Instance.vKeyList[i].ToString())
                {
                    KeyboardHooking.Instance.LogPlayStopKey = Data.Instance.vKeyList[i];
                }



                else if (RecordPlayKey_TextBox.Text == Data.Instance.vKeyList[i].ToString())
                {
                    KeyboardHooking.Instance.LogPlayKey = Data.Instance.vKeyList[i];
                }



                else if (RecordPlayStopKey_TextBox.Text == Data.Instance.vKeyList[i].ToString())
                {
                    KeyboardHooking.Instance.RecordStopKey = Data.Instance.vKeyList[i];
                }



                else if (RecordPauseKey_TextBox.Text == Data.Instance.vKeyList[i].ToString())
                {
                    KeyboardHooking.Instance.PauseKey = Data.Instance.vKeyList[i];
                }



                else if (ModuleRecordKey_TextBox.Text == Data.Instance.vKeyList[i].ToString())
                {
                    KeyboardHooking.Instance.ModulLogKey = Data.Instance.vKeyList[i];
                }
            }

            // 자동저장 체크여부 적용
            if (AutoSave_CheckBox.IsChecked == true)
                is_autosavechecked = true;
            else
                is_autosavechecked = false;

            // 자동저장 경로 적용
            SaveFunction.Instance.g_filepath = PathBox.Text;

            this.Hide();
        }

        private void CreateSettingDataFile(DirectoryInfo fileinfo) // 폴더 및 파일 생성 1
        {
            // 파일을 새로 만들경우 파일에 아래의 데이터를 저장하게 됨
            di.Create();
            text = "[녹화시작키]=Q" + "#";
            text += "[녹화정지키]=W" + "#";
            text += "[녹화재생키]=E" + "#";
            text += "[재생정지키]=R" + "#";
            text += "[일시정지키]=T" + "#";
            text += "[모듈녹화키]=F8" + "#";
            text += "[자동저장]=False" + "#";
            text += "[마우스최적화]=False";

            System.IO.File.WriteAllText(folderpath + filename, text, Encoding.Default);
            // 새로 만든 data.txt의 저장된 데이터를 전역변수와 컨트롤에 적용시킴
            UnifyData(text);
        }

        private void UnifyData(string text) // 폴더 및 파일 생성 2
        {
            // data.txt에서 정보를 가져와서 KeyboardHooking의 전역변수, RecordSetting의 컨트롤과 데이터를 단일화

            /* data.txt 세부정보
             * data[0] -> [녹화시작키]=F11
             * data[1] -> [녹화정지키]=F3
             * data[2] -> [녹화재생키]=F12
             * data[3] -> [재생정지키]=F10
             * data[4] -> [일시정지키]=Escape
             * data[5] -> [모듈녹화키]=F8
             * data[6] -> [자동저장]=False
             * data[7] -> [마우스최적화]=False
             */

            string[] data = text.Split('#');
            string[] pdata1 = data[0].Split('=');
            string[] pdata2 = data[1].Split('=');
            string[] pdata3 = data[2].Split('=');
            string[] pdata4 = data[3].Split('=');
            string[] pdata5 = data[4].Split('=');
            string[] pdata6 = data[5].Split('=');
            string[] pdata7 = data[6].Split('=');
            string[] pdata8 = data[7].Split('=');

            // pdata[0]~pdata[6]까지는 각 textBox의 Key값들을 나타낸다.
            for (int i = 0; i < Data.Instance.vKeyList.Count; i++)
            {
                if (pdata1[1] == Data.Instance.vKeyList[i].ToString())
                {
                    KeyboardHooking.Instance.RecordStartKey = Data.Instance.vKeyList[i];
                    RecordStartKey_TextBox.Text = Data.Instance.vKeyList[i].ToString();
                }



                else if (pdata2[1] == Data.Instance.vKeyList[i].ToString())
                {
                    KeyboardHooking.Instance.LogPlayStopKey = Data.Instance.vKeyList[i];
                    RecordPlayStopKey_TextBox.Text = Data.Instance.vKeyList[i].ToString();
                }



                else if (pdata3[1] == Data.Instance.vKeyList[i].ToString())
                {
                    KeyboardHooking.Instance.LogPlayKey = Data.Instance.vKeyList[i];
                    RecordStartKey_TextBox.Text = Data.Instance.vKeyList[i].ToString();
                }



                else if (pdata4[1] == Data.Instance.vKeyList[i].ToString())
                {
                    KeyboardHooking.Instance.RecordStopKey = Data.Instance.vKeyList[i];
                    RecordStartKey_TextBox.Text = Data.Instance.vKeyList[i].ToString();
                }



                else if (pdata5[1] == Data.Instance.vKeyList[i].ToString())
                {
                    KeyboardHooking.Instance.PauseKey = Data.Instance.vKeyList[i];
                    RecordStartKey_TextBox.Text = Data.Instance.vKeyList[i].ToString();
                }



                else if (pdata6[1] == Data.Instance.vKeyList[i].ToString())
                {
                    KeyboardHooking.Instance.ModulLogKey = Data.Instance.vKeyList[i];
                    RecordStartKey_TextBox.Text = Data.Instance.vKeyList[i].ToString();
                }
            }

            // pdata[7]는 자동저장 여부가 저장된다.
            if (pdata7[1] == "True")
                AutoSave_CheckBox.IsChecked = is_autosavechecked = true;
            else if (pdata7[1] == "False")
                AutoSave_CheckBox.IsChecked = is_autosavechecked = false;

            // pdata[8]는 마우스 최적화 여부가 저장된다.
            if (pdata8[1] == "True")
                MouseOptimize_CheckBox.IsChecked = is_mouseoptimize = true;
            else if (pdata8[1] == "False")
                MouseOptimize_CheckBox.IsChecked = is_mouseoptimize = false;
        }
        #endregion

        #region 단축키 설정 다이얼로그
        private void RecordStartKey_Button_Click(object sender, RoutedEventArgs e)
        {
            chk = new ChangeDefaultKeyDialog("녹화시작키 : ", RecordStartKey_TextBox.Text);
            chk.Show();
        }

        private void RecordStopKey_Button_Click(object sender, RoutedEventArgs e)
        {
            chk = new ChangeDefaultKeyDialog("녹화정지키 : ", RecordStopKey_TextBox.Text);
            chk.Show();
        }

        private void RecordPlayKey_Button_Click(object sender, RoutedEventArgs e)
        {
            chk = new ChangeDefaultKeyDialog("녹화재생키 : ", RecordPlayKey_TextBox.Text);
            chk.Show();
        }

        private void RecordPlayStopKey_Button_Click(object sender, RoutedEventArgs e)
        {
            chk = new ChangeDefaultKeyDialog("재생정지키 : ", RecordPlayStopKey_TextBox.Text);
            chk.Show();
        }

        private void RecordPauseKey_Button_Click(object sender, RoutedEventArgs e)
        {
            chk = new ChangeDefaultKeyDialog("일시정지키 : ", RecordPauseKey_TextBox.Text);
            chk.Show();
        }

        private void ModuleRecordKey_Button_Click(object sender, RoutedEventArgs e)
        {
            chk = new ChangeDefaultKeyDialog("모듈녹화키 : ", ModuleRecordKey_TextBox.Text);
            chk.Show();
        }
        #endregion

        #region 자동저장
        public void RealTimeSaveTimerStart() // 자동 저장 타이머
        {
            RealTimeSaveTimer = new System.Timers.Timer();
            RealTimeSaveTimer.Enabled = true;
            RealTimeSaveTimer.Interval = 1 * 1000 * RecordSetting.Instance.Interval_Slider.Value;
            RealTimeSaveTimer.Elapsed += new ElapsedEventHandler(RealTimeSave_Elapsed);
            RealTimeSaveTimer.Start();
        }

        private void RealTimeSave_Elapsed(object sender, EventArgs e)
        {
            if (is_autosavechecked) // 자동저장 체크박스가 체크되어있을 경우 로그리스트를 파일에 저장함
            {
                SaveFunction.Instance.XmlCreateRecordList(SaveFunction.Instance.g_filepathname);
                SaveFunction.Instance.InitSaveFileLoad(); // 저장된 로그 목록 출력(갱신)
            }
        }

        private void SetFolder_Click(object sender, RoutedEventArgs e) // 자동저장 경로설정
        {
            System.Windows.Forms.FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.RootFolder = Environment.SpecialFolder.Desktop;

            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                SaveFunction.Instance.g_filepath = fbd.SelectedPath; // 자동 저장 폴더를 선택한 폴더로 선택한다.
                PathBox.Text = fbd.SelectedPath;
            }
        }
        #endregion
    }
}