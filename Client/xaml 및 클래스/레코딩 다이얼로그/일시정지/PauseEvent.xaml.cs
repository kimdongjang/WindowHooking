using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Client
{
    /// <summary>
    /// Edit.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class PauseEvent : Window
    {
        #region 싱글톤
        static PauseEvent instance = null;
        static readonly object padlock = new Object();
        public static PauseEvent Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new PauseEvent();
                    }
                    return instance;
                }
            }
        }
        public PauseEvent()
        {
            InitializeComponent();
            instance = this;
        }
        #endregion

        public bool opened_memo = false; // 메모창이 열려있는 상태. 두번 열지 못하게끔.
        public bool opened_screenshot = false; // 스크린샷창이 열려있는 상태. 두 번 열지 못하게 
        public bool autosave_isreadytocreatefile = false;
        public Screenshot screen_dlg;
        private LogSaveSetting logsave_dlg;

        private void Memo_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            if (opened_memo == false)
            {
                KeyboardHooking.Instance.ShowMemoDlg();
                opened_memo = true;
            }
        }

        private void Memo_Drop(object sender, DragEventArgs e)
        {

        }

        private void Memo_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("녹화를 다시 진행합니다.", "경고", MessageBoxButton.OK, MessageBoxImage.Warning) == MessageBoxResult.OK)
            {
                opened_memo = false;

                KeyboardHooking.Instance.pausing_starting = false;
                MainRecording.Instance.PauseAfterRecording();
                KeyboardHooking.Instance.recording_starting = true;

                this.Hide();

                if (screen_dlg != null) screen_dlg.Close();
                if (KeyboardHooking.Instance.memo_dlg != null) KeyboardHooking.Instance.memo_dlg.Close();
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            //if (MessageBox.Show("녹화를 다시 진행합니다.", "경고", MessageBoxButton.OK, MessageBoxImage.Warning) == MessageBoxResult.OK)
            //{
                opened_memo = false;

                KeyboardHooking.Instance.pausing_starting = false;
                MainRecording.Instance.PauseAfterRecording();
                KeyboardHooking.Instance.recording_starting = true;

                this.Hide();

                if (screen_dlg != null) screen_dlg.Close();
                if (KeyboardHooking.Instance.memo_dlg != null) KeyboardHooking.Instance.memo_dlg.Close();
            //}
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("녹화한 데이터를 SLog 폴더에 저장하시겠습니까?", "", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                logsave_dlg = new LogSaveSetting();
                logsave_dlg.Show();

                //SaveFunction.Instance.LogFileSaveFunction();
                //MainRecording.Instance.StopEvent();
                this.Hide();
                //MainRecording.Instance.WindowState = WindowState.Normal;

                if (screen_dlg != null) screen_dlg.Close();
                if (KeyboardHooking.Instance.memo_dlg != null) KeyboardHooking.Instance.memo_dlg.Close();
                if (MainRecording.Instance.ShowList_Dlg != null) MainRecording.Instance.ShowList_Dlg.Close();

                Data.Instance.screenIndex = 0;
            }
            else
            {
                try
                {
                    File.Delete(SaveFunction.Instance.g_filepathname);

                    // 관련된 이미지파일도 삭제한다.
                    string sDirPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\SLog\");
                    DirectoryInfo di = new DirectoryInfo(sDirPath);

                    // 파일디렉토리를 돌아서, FileName과, .jpg를 포함한다면 모두 삭제.
                    foreach (var item in di.GetFiles())
                    {
                        if (item.Name.Contains(".jpg"))
                        {
                            string[] filenameExceptjpg = item.Name.Split('_'); // SLog의 모든 jpg파일을 가져와서 확장자를 제거
                            if (SaveFunction.Instance.g_filenameexceptxml == filenameExceptjpg[0])
                            {
                                File.Delete(sDirPath + item.Name);
                            }
                        }
                    }
                    if (MessageBox.Show("녹화된 데이터를 초기화하시겠습니까?", "", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        MainRecording.Instance.StopEvent();
                        this.Hide();
                        MainRecording.Instance.WindowState = WindowState.Normal;

                        if (screen_dlg != null) screen_dlg.Close();
                        if (KeyboardHooking.Instance.memo_dlg != null) KeyboardHooking.Instance.memo_dlg.Close();
                        if (MainRecording.Instance.ShowList_Dlg != null) MainRecording.Instance.ShowList_Dlg.Close();
                    }
                    else
                    {
                        //SaveFunction.Instance.GetSaveFileIndex();
                        //this.Hide();
                        //MainRecording.Instance.WindowState = WindowState.Normal;
                    }
                    Data.Instance.screenIndex = 0;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            KeyboardHooking.Instance.f10key = false;
        }

        private void Screenshot_Click(object sender, RoutedEventArgs e)
        {
            if (opened_screenshot == false)
            {
                screen_dlg = new Screenshot();
                screen_dlg.Show();
                opened_screenshot = true;
            }
            this.Hide();
        }
    }
}