using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
    /// OnFileDialogSetting.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class OnFileDialogSetting : Window
    {
        public OnFileDialogSetting()
        {
            InitializeComponent();
            // (로그 저장하는 사람 입장)
            if (KeyboardHooking.Instance.pausing_starting)
                xaml_confirm.IsEnabled = false;

            // (로그 실행하는 사람 입장)
            else
            {
                xaml_save.IsEnabled = false;
                xaml_cancel.IsEnabled = false;
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if(xaml_editdata.Text == "")
            {
                if (MessageBox.Show("아무 정보도 입력되지 않았습니다. 저장하지 않고 닫으시겠습니까?", "경고", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    KeyboardHooking.Instance.pausing_starting = false;
                    SaveFunction.Instance.PauseEndXmlAdd(); // 녹화 일시 정지가 끝났다는 로그 생성
                    MainRecording.Instance.PauseAfterRecording();
                    KeyboardHooking.Instance.recording_starting = true;
                    this.Close();
                }
            }
            else
            {
                // ======================================================================================
                // 사용자가 입력한 메모를 컬렉션 리스트에 저장
                Data.Instance.Rec_list.Add("D＆" + xaml_editdata.Text);                           // data 클래스의 메모 리스트에 사용자가 지정한 메모를 추가
                // ======================================================================================
                // 현재 창의 좌표를 컬렉션 리스트에 저장  
                Point location = new Point(this.Left, this.Top);
                Data.Instance.Rec_list.Add(location.X.ToString() + "♤" + location.Y.ToString());
                // ======================================================================================  
                xaml_editdata.Text = "";
                MessageBox.Show("저장이 완료되었습니다!");
                KeyboardHooking.Instance.pausing_starting = false;
                SaveFunction.Instance.PauseEndXmlAdd(); // 녹화 일시 정지가 끝났다는 로그 생성
                MainRecording.Instance.PauseAfterRecording();
                KeyboardHooking.Instance.recording_starting = true;
                this.Close();
            }
        }
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void Cancle_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("저장하지 않고 닫으시겠습니까?", "경고", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                KeyboardHooking.Instance.pausing_starting = false;
                SaveFunction.Instance.PauseEndXmlAdd(); // 녹화 일시 정지가 끝났다는 로그 생성
                MainRecording.Instance.PauseAfterRecording();
                KeyboardHooking.Instance.recording_starting = true;
                this.Close();
            }
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            KeyboardHooking.Instance.logplaying_pausing = false;
            KeyboardHooking.Instance.logplaying_starting = true; // 로그가 실행 중이라는 boolean변수
            MainRecording.Instance.PlayThread = new Thread(MainRecording.Instance.Play_Button);
            MainRecording.Instance.PlayThread.IsBackground = true;
            MainRecording.Instance.PlayThread.Start();
           
            this.Close();
        }
    }
}
