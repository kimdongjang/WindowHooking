using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
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
    /// Join.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SignUpDialog : Window
    {
        MyClient client = MyClient.Instance;
        public bool is_idcheck = false;
        public string check_iddata = "";

        public SignUpDialog()
        {
            InitializeComponent();
            client.InitClient();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (is_idcheck == false)
            {
                MessageBox.Show("아이디 중복 체크를 먼저 해주시길 바랍니다.");
                return;
            }
            if (check_iddata != idTxt.Text)
            {
                MessageBox.Show("중복 체크한 아이디와 일치하지 않습니다!");
                return;
            }

            if (idTxt == null || pwTxt == null || nameTxt == null || phoneTxt == null)
            {
                MessageBox.Show("정보 입력이 부족합니다. 빈 칸이 있는지 확인해주세요!");
            }

            string packet = "MEMBERADD" + "$";
            packet += idTxt.Text + "#";
            packet += pwTxt.Password + "#";
            packet += nameTxt.Text + "#";
            packet += phoneTxt.Text;
            
            client.SendDataOne(packet);

            MainWindow.Instance.Show();
        }

        private void IDCheck_Click(object sender, RoutedEventArgs e)
        {
            if (idTxt == null)
            {
                MessageBox.Show("정보가 없습니다! 정보를 입력해주세요.");
            }
            string packet = "IDCHECK" + "$";
            packet += idTxt.Text;
            check_iddata = idTxt.Text;// 중복검사가 된 아이디를 등록

            client.SendDataOne(packet);
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance.Show();
            this.Close();
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }
    }
}
