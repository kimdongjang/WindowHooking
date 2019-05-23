using System;
using System.Collections.Generic;
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
using System.Xml;

namespace Client
{
    /// <summary>
    /// MemoAdd.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MemoDialog : Window
    {
        public MemoDialog()
        {
            InitializeComponent();

            // (로그 저장하는 사람 입장)
            if (KeyboardHooking.Instance.pausing_starting)
                MemoOk.IsEnabled = false;

            // (로그 실행하는 사람 입장)
            else
            {
                MemoSave.IsEnabled = false;
                MemoCancel.IsEnabled = false;
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            // ======================================================================================
            // 사용자가 입력한 메모를 컬렉션 리스트에 저장
            Data.Instance.Rec_list.Add("M＆" + xaml_usermemo.Text);                           // data 클래스의 메모 리스트에 사용자가 지정한 메모를 추가
            // ======================================================================================
            // 메모창의 좌표를 컬렉션 리스트에 저장  
            Point location = new Point(this.Left, this.Top);
            Data.Instance.Rec_list.Add(location.X.ToString() + "↔" + location.Y.ToString());
            // ======================================================================================  
            //SaveFunction.Instance.XmlCreateRecordList(SaveFunction.Instance.realtime_folderpath + SaveFunction.Instance.realtime_savename); 
            xaml_usermemo.Text = "";
            MessageBox.Show("저장이 완료되었습니다!");
            PauseEvent.Instance.opened_memo = false;
            this.Close();

            PauseEvent.Instance.Show();
        }

        private void Cancle_Click(object sender, RoutedEventArgs e)
        {
            if(MessageBox.Show("저장하지 않고 종료하시겠습니까?", "경고", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                PauseEvent.Instance.opened_memo = false;
                this.Close();
            }
            PauseEvent.Instance.Show();
        }
         
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
         
        private void MemoOk_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            KeyboardHooking.Instance.recording_starting = true;
        }
    }
}
