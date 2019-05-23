using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Client
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        RecvClass ctrl = RecvClass.Instance;
        MyClient client = MyClient.Instance;
        public SignUpDialog Member_Dlg;

        #region 싱글톤
        static MainWindow instance = null;
        static readonly object padlock = new Object();
        public static MainWindow Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new MainWindow();
                    }
                    return instance;
                }
            }
        }
        public MainWindow()
        {
            instance = this;
            InitializeComponent();
            client.InitClient();
        }
        private void Window_Closed(object sender, EventArgs e)
        {
            instance = null;
            Environment.Exit(0);
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }
        #endregion

        private void MemberAdd_MouseDown(object sender, MouseButtonEventArgs e) // 회원가입 버튼 클릭
        {
            Member_Dlg = new SignUpDialog();
            Member_Dlg.Show();
            this.Hide();
        }
        public void joinCursor()
        {
            Cursor = Cursors.Hand;
        }
        public void ArrowCursor()
        {
            Cursor = Cursors.Arrow;
        }
        private void Label_MouseEnter(object sender, MouseEventArgs e)
        {
            this.joinCursor();
        }

        private void Label_MouseLeave(object sender, MouseEventArgs e)
        {
            this.ArrowCursor();
        }

        private void Button_Click(object sender, RoutedEventArgs e) // 로그인 버튼 클릭
        {
            if (idTxt == null || pwTxt == null)
            {
                MessageBox.Show("아이디 또는 비밀번호가 잘 못 입력되었습니다!");
                return;
            }

            // 자신의 IP주소를 얻는다.
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            string ip = string.Empty;
            for (int i = 0; i < host.AddressList.Length; i++)
            {
                if (host.AddressList[i].AddressFamily == AddressFamily.InterNetwork)
                {
                    ip = host.AddressList[i].ToString();
                }
            }

            string id = idTxt.Text;
            string pw = pwTxt.Password;
            Data.Instance.myId = id;
            Data.Instance.myIp = ip;

            string packet = "LOGIN" + "$";
            packet += id + "#";
            packet += pw + "#";
            packet += ip;

            client.SendDataOne(packet);
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
            System.Diagnostics.Process.GetCurrentProcess().Kill();
            this.Close();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
    }
}