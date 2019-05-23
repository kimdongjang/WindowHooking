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

namespace Client
{
    /// <summary>
    /// ChannelInputPasswordDialog.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ChannelInputPassword : Window
    {
        private string password = "";

        #region 싱글톤
        static ChannelInputPassword instance = null;
        static readonly object padlock = new Object();
        public static ChannelInputPassword Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new ChannelInputPassword();
                    }
                    return instance;
                }
            }
        }
        public ChannelInputPassword()
        {
            instance = this;
            InitializeComponent();
        }
        #endregion

        public void InitPassword(string inputpassword)
        {
            this.password = inputpassword;
        }

        private void TextBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (pw_textBox.Text == "비밀번호")
                pw_textBox.Text = "";
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if(pw_textBox.Text == password)
            {
                MainChannel.Instance.ChannelJoin(Data.Instance.ChannelList[MainRecording.Instance.xamlChannelList.SelectedIndex].ChannelName);
                this.Close();
            }
            else
            {
                MessageBox.Show("입력한 패스워드가 올바르지 않습니다.");
                return;
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            MainRecording.Instance.Show();
        }
    }
}
